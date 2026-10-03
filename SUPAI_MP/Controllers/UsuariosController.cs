
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using supai_mp.Data;
using supai_mp.Models;
using supai_mp.Models.DTOs;
using supai_mp.Models.Institucional;

namespace supai_mp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsuariosController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // LISTAR UTILIZADORES
        // GET: api/Usuarios
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> ListarUsuarios()
        {
            var usuarios = await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.Funcionario)
                .Include(u => u.UnidadeInstitucional)
                    .ThenInclude(u => u.TipoUnidadeInstitucional)
                .Include(u => u.UnidadeInstitucional)
                    .ThenInclude(u => u.EntidadeInstitucional)
                .OrderBy(u => u.NomeUsuario)
                .Select(u => new
                {
                    u.Id,
                    u.NomeUsuario,
                    u.Perfil,
                    u.Ativo,
                    u.DataCadastro,

                    TipoUtilizador =
                        u.FuncionarioId.HasValue
                            ? "Interno"
                            : "Institucional",

                    Funcionario =
                        u.Funcionario == null
                            ? null
                            : new
                            {
                                u.Funcionario.Id,
                                u.Funcionario.NomeCompleto,
                                u.Funcionario.Nip
                            },

                    UnidadeInstitucional =
                        u.UnidadeInstitucional == null
                            ? null
                            : new
                            {
                                u.UnidadeInstitucional.Id,
                                u.UnidadeInstitucional.Nome,
                                u.UnidadeInstitucional.Sigla,

                                Tipo =
                                    u.UnidadeInstitucional
                                        .TipoUnidadeInstitucional
                                        .Nome,

                                Entidade =
                                    u.UnidadeInstitucional
                                        .EntidadeInstitucional
                                        .Nome
                            }
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        // ============================================================
        // OBTER UTILIZADOR
        // GET: api/Usuarios/5
        // ============================================================

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObterUsuario(
            int id)
        {
            var usuario = await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.Funcionario)
                .Include(u => u.UnidadeInstitucional)
                    .ThenInclude(u =>
                        u.TipoUnidadeInstitucional)
                .Include(u => u.UnidadeInstitucional)
                    .ThenInclude(u =>
                        u.EntidadeInstitucional)
                .FirstOrDefaultAsync(
                    u => u.Id == id);

            if (usuario == null)
            {
                return NotFound(new
                {
                    mensagem =
                        "Utilizador não encontrado."
                });
            }

            return Ok(new
            {
                usuario.Id,
                usuario.NomeUsuario,
                usuario.Perfil,
                usuario.Ativo,
                usuario.DataCadastro,

                TipoUtilizador =
                    usuario.FuncionarioId.HasValue
                        ? "Interno"
                        : "Institucional",

                Funcionario =
                    usuario.Funcionario == null
                        ? null
                        : new
                        {
                            usuario.Funcionario.Id,
                            usuario.Funcionario.NomeCompleto,
                            usuario.Funcionario.Nip
                        },

                UnidadeInstitucional =
                    usuario.UnidadeInstitucional == null
                        ? null
                        : new
                        {
                            usuario.UnidadeInstitucional.Id,
                            usuario.UnidadeInstitucional.Nome,
                            usuario.UnidadeInstitucional.Sigla,

                            Tipo =
                                usuario.UnidadeInstitucional
                                    .TipoUnidadeInstitucional
                                    .Nome,

                            Entidade =
                                usuario.UnidadeInstitucional
                                    .EntidadeInstitucional
                                    .Nome
                        }
            });
        }

        // ============================================================
        // CRIAR UTILIZADOR
        // POST: api/Usuarios
        // ============================================================

        [HttpPost]
        public async Task<IActionResult> CriarUsuario(
            [FromBody] CriarUsuarioDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (string.IsNullOrWhiteSpace(dto.NomeUsuario))
            {
                return BadRequest(new
                {
                    mensagem =
                        "O nome de utilizador é obrigatório."
                });
            }

            if (string.IsNullOrWhiteSpace(dto.Senha))
            {
                return BadRequest(new
                {
                    mensagem =
                        "A palavra-passe é obrigatória."
                });
            }

            if (string.IsNullOrWhiteSpace(dto.Perfil))
            {
                return BadRequest(new
                {
                    mensagem =
                        "O perfil é obrigatório."
                });
            }

            // ========================================================
            // VALIDAR PERFIL
            // ========================================================

            if (!PerfisUsuario.EhValido(dto.Perfil))
            {
                return BadRequest(new
                {
                    mensagem =
                        "Perfil de utilizador inválido.",

                    perfisPermitidos =
                        PerfisUsuario.Todos
                });
            }

            // ========================================================
            // NORMALIZAR NOME DE UTILIZADOR
            // ========================================================

            var nomeUsuario =
                dto.NomeUsuario.Trim();

            var usuarioExiste =
                await _context.Usuarios
                    .AnyAsync(u =>
                        u.NomeUsuario == nomeUsuario);

            if (usuarioExiste)
            {
                return Conflict(new
                {
                    mensagem =
                        "Já existe um utilizador com este nome."
                });
            }

            // ========================================================
            // É OBRIGATÓRIO TER PELO MENOS UMA ASSOCIAÇÃO
            // ========================================================

            if (!dto.FuncionarioId.HasValue &&
                !dto.UnidadeInstitucionalId.HasValue)
            {
                return BadRequest(new
                {
                    mensagem =
                        "O utilizador deve estar associado a um funcionário ou a uma unidade institucional."
                });
            }

            // ========================================================
            // FUNCIONÁRIO
            // ========================================================

            Funcionario? funcionario = null;

            if (dto.FuncionarioId.HasValue)
            {
                funcionario =
                    await _context.Funcionarios
                        .FirstOrDefaultAsync(
                            f => f.Id ==
                                dto.FuncionarioId.Value);

                if (funcionario == null)
                {
                    return NotFound(new
                    {
                        mensagem =
                            "Funcionário não encontrado."
                    });
                }

                var funcionarioJaAssociado =
                    await _context.Usuarios
                        .AnyAsync(u =>
                            u.FuncionarioId ==
                            dto.FuncionarioId.Value);

                if (funcionarioJaAssociado)
                {
                    return Conflict(new
                    {
                        mensagem =
                            "Este funcionário já possui um utilizador."
                    });
                }
            }

            // ========================================================
            // UNIDADE INSTITUCIONAL
            // ========================================================

            UnidadeInstitucional? unidade = null;

            if (dto.UnidadeInstitucionalId.HasValue)
            {
                unidade =
                    await _context
                        .UnidadesInstitucionais
                        .Include(u =>
                            u.TipoUnidadeInstitucional)
                        .FirstOrDefaultAsync(
                            u =>
                                u.Id ==
                                dto.UnidadeInstitucionalId.Value &&
                                u.Ativo);

                if (unidade == null)
                {
                    return NotFound(new
                    {
                        mensagem =
                            "Unidade institucional não encontrada ou está inativa."
                    });
                }
            }

            // ========================================================
            // CRIAR UTILIZADOR
            // ========================================================

            var usuario = new Usuario
            {
                NomeUsuario =
                    nomeUsuario,

                SenhaHash =
                    BCrypt.Net.BCrypt.HashPassword(
                        dto.Senha),

                Perfil =
                    dto.Perfil.Trim(),

                FuncionarioId =
                    dto.FuncionarioId,

                UnidadeInstitucionalId =
                    dto.UnidadeInstitucionalId,

                Ativo = true,

                DataCadastro =
                    DateTime.Now
            };

            _context.Usuarios.Add(usuario);

            await _context.SaveChangesAsync();

            // ========================================================
            // RESPOSTA
            // ========================================================

            return Created(
                $"api/Usuarios/{usuario.Id}",
                new
                {
                    mensagem =
                        "Utilizador criado com sucesso.",

                    usuario = new
                    {
                        usuario.Id,
                        usuario.NomeUsuario,
                        usuario.Perfil,
                        usuario.Ativo,

                        TipoUtilizador =
                            usuario.FuncionarioId.HasValue
                                ? "Interno"
                                : "Institucional",

                        Funcionario =
                            funcionario == null
                                ? null
                                : new
                                {
                                    funcionario.Id,
                                    funcionario.NomeCompleto,
                                    funcionario.Nip
                                },

                        UnidadeInstitucional =
                            unidade == null
                                ? null
                                : new
                                {
                                    unidade.Id,
                                    unidade.Nome,
                                    unidade.Sigla
                                }
                    }
                });
        }

        // ============================================================
        // ALTERAR ESTADO
        // PUT: api/Usuarios/5/estado
        // ============================================================

        [HttpPut("{id:int}/estado")]
        public async Task<IActionResult> AlterarEstado(
            int id,
            [FromBody] bool ativo)
        {
            var usuario =
                await _context.Usuarios
                    .FirstOrDefaultAsync(
                        u => u.Id == id);

            if (usuario == null)
            {
                return NotFound(new
                {
                    mensagem =
                        "Utilizador não encontrado."
                });
            }

            // ========================================================
            // NÃO PERMITIR DESACTIVAR O ÚLTIMO ADMIN
            // ========================================================

            if (!ativo &&
                string.Equals(
                    usuario.Perfil,
                    PerfisUsuario.Administrador,
                    StringComparison.OrdinalIgnoreCase))
            {
                var administradoresAtivos =
                    await _context.Usuarios
                        .CountAsync(u =>
                            u.Ativo &&
                            u.Perfil ==
                                PerfisUsuario.Administrador);

                if (administradoresAtivos <= 1)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "Não é possível desactivar o último Administrador activo do sistema."
                    });
                }
            }

            usuario.Ativo = ativo;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem =
                    ativo
                        ? "Utilizador activado com sucesso."
                        : "Utilizador desactivado com sucesso.",

                usuarioId =
                    usuario.Id,

                usuario.Ativo
            });
        }
    }
}

