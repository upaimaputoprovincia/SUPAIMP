using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using supai_mp.Data;
using supai_mp.Models.Institucional;
using System.Security.Claims;

namespace supai_mp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador")]
    public class PermissoesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PermissoesController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // OBTER ID DO UTILIZADOR AUTENTICADO
        // ============================================================

        private bool TentarObterUsuarioId(out int usuarioId)
        {
            var claimId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            return int.TryParse(
                claimId,
                out usuarioId);
        }

        // ============================================================
        // LISTAR TIPOS DE PERMISSÃO
        // GET: api/Permissoes/tipos
        // ============================================================

        [HttpGet("tipos")]
        public async Task<IActionResult> ObterTiposPermissao()
        {
            var tipos =
                await _context.TiposPermissao
                    .AsNoTracking()
                    .Where(t => t.Ativo)
                    .OrderBy(t => t.Id)
                    .Select(t => new
                    {
                        t.Id,
                        t.Codigo,
                        t.Nome,
                        t.Descricao,
                        t.Ativo
                    })
                    .ToListAsync();

            return Ok(tipos);
        }

        // ============================================================
        // LISTAR UNIDADES INSTITUCIONAIS
        // GET: api/Permissoes/unidades
        // ============================================================

        [HttpGet("unidades")]
        public async Task<IActionResult> ObterUnidadesInstitucionais()
        {
            var unidades =
                await _context.UnidadesInstitucionais
                    .AsNoTracking()
                    .Include(u => u.TipoUnidadeInstitucional)
                    .Include(u => u.EntidadeInstitucional)
                    .Where(u => u.Ativo)
                    .OrderBy(u => u.Nome)
                    .Select(u => new
                    {
                        u.Id,
                        u.Nome,
                        u.Sigla,
                        u.Descricao,

                        TipoUnidade = new
                        {
                            u.TipoUnidadeInstitucionalId,
                            u.TipoUnidadeInstitucional.Nome
                        },

                        Entidade = new
                        {
                            u.EntidadeInstitucionalId,
                            u.EntidadeInstitucional.Nome,
                            u.EntidadeInstitucional.Sigla
                        },

                        u.UnidadePaiId,
                        u.Ativo
                    })
                    .ToListAsync();

            return Ok(unidades);
        }

        // ============================================================
        // LISTAR PERMISSÕES DE UM UTILIZADOR
        // GET: api/Permissoes/utilizador/5
        // ============================================================

        [HttpGet("utilizador/{usuarioId:int}")]
        public async Task<IActionResult> ObterPorUtilizador(
            int usuarioId)
        {
            var usuario =
                await _context.Usuarios
                    .AsNoTracking()
                    .Include(u => u.Funcionario)
                    .Include(u => u.UnidadeInstitucional)
                    .FirstOrDefaultAsync(
                        u => u.Id == usuarioId);

            if (usuario == null)
            {
                return NotFound(new
                {
                    mensagem =
                        "Utilizador não encontrado."
                });
            }

            var permissoes =
                await _context.PermissoesAcesso
                    .AsNoTracking()
                    .Include(p => p.TipoPermissao)
                    .Include(p => p.UnidadeInstitucional)
                    .Where(p => p.UsuarioId == usuarioId)
                    .OrderBy(p => p.TipoPermissao.Nome)
                    .ThenBy(p => p.UnidadeInstitucional.Nome)
                    .Select(p => new
                    {
                        p.Id,

                        UsuarioId = p.UsuarioId,

                        TipoPermissao = new
                        {
                            p.TipoPermissaoId,
                            p.TipoPermissao.Codigo,
                            p.TipoPermissao.Nome,
                            p.TipoPermissao.Descricao
                        },

                        UnidadeInstitucional = new
                        {
                            p.UnidadeInstitucionalId,
                            p.UnidadeInstitucional.Nome,
                            p.UnidadeInstitucional.Sigla
                        },

                        p.Ativo,
                        p.DataConcessao,
                        p.DataExpiracao,
                        p.Observacao
                    })
                    .ToListAsync();

            return Ok(new
            {
                utilizador = new
                {
                    usuario.Id,
                    usuario.NomeUsuario,
                    usuario.Perfil,
                    usuario.Ativo,

                    funcionarioId =
                        usuario.FuncionarioId,

                    funcionario =
                        usuario.Funcionario?.NomeCompleto,

                    unidadeInstitucionalId =
                        usuario.UnidadeInstitucionalId,

                    unidadeInstitucional =
                        usuario.UnidadeInstitucional?.Nome
                },

                permissoes
            });
        }

        // ============================================================
        // LISTAR TODAS AS PERMISSÕES
        // GET: api/Permissoes
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> ObterTodas()
        {
            var permissoes =
                await _context.PermissoesAcesso
                    .AsNoTracking()
                    .Include(p => p.Usuario)
                    .Include(p => p.TipoPermissao)
                    .Include(p => p.UnidadeInstitucional)
                    .OrderBy(p => p.Usuario.NomeUsuario)
                    .ThenBy(p => p.TipoPermissao.Nome)
                    .Select(p => new
                    {
                        p.Id,

                        Usuario = new
                        {
                            p.UsuarioId,
                            p.Usuario.NomeUsuario,
                            p.Usuario.Perfil
                        },

                        TipoPermissao = new
                        {
                            p.TipoPermissaoId,
                            p.TipoPermissao.Codigo,
                            p.TipoPermissao.Nome
                        },

                        UnidadeInstitucional = new
                        {
                            p.UnidadeInstitucionalId,
                            p.UnidadeInstitucional.Nome,
                            p.UnidadeInstitucional.Sigla
                        },

                        p.Ativo,
                        p.DataConcessao,
                        p.DataExpiracao,
                        p.Observacao
                    })
                    .ToListAsync();

            return Ok(permissoes);
        }

        // ============================================================
        // CONCEDER PERMISSÃO
        // POST: api/Permissoes
        // ============================================================

        [HttpPost]
        public async Task<IActionResult> Conceder(
            [FromBody] ConcederPermissaoDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var usuario =
                await _context.Usuarios
                    .FirstOrDefaultAsync(
                        u => u.Id == dto.UsuarioId);

            if (usuario == null)
            {
                return NotFound(new
                {
                    mensagem =
                        "Utilizador não encontrado."
                });
            }

            if (!usuario.Ativo)
            {
                return BadRequest(new
                {
                    mensagem =
                        "Não é possível conceder uma permissão a um utilizador inativo."
                });
            }

            var tipoPermissao =
                await _context.TiposPermissao
                    .FirstOrDefaultAsync(
                        t =>
                            t.Id == dto.TipoPermissaoId &&
                            t.Ativo);

            if (tipoPermissao == null)
            {
                return BadRequest(new
                {
                    mensagem =
                        "Tipo de permissão inválido ou inativo."
                });
            }

            var unidade =
                await _context.UnidadesInstitucionais
                    .FirstOrDefaultAsync(
                        u =>
                            u.Id == dto.UnidadeInstitucionalId &&
                            u.Ativo);

            if (unidade == null)
            {
                return BadRequest(new
                {
                    mensagem =
                        "Unidade institucional inválida ou inativa."
                });
            }

            // ========================================================
            // VALIDAR DATA DE EXPIRAÇÃO
            // ========================================================

            if (dto.DataExpiracao.HasValue &&
                dto.DataExpiracao.Value < DateTime.Now)
            {
                return BadRequest(new
                {
                    mensagem =
                        "A data de expiração não pode estar no passado."
                });
            }

            // ========================================================
            // VERIFICAR DUPLICAÇÃO
            // ========================================================

            var existente =
                await _context.PermissoesAcesso
                    .FirstOrDefaultAsync(
                        p =>
                            p.UsuarioId == dto.UsuarioId &&
                            p.TipoPermissaoId ==
                                dto.TipoPermissaoId &&
                            p.UnidadeInstitucionalId ==
                                dto.UnidadeInstitucionalId);

            if (existente != null)
            {
                if (existente.Ativo)
                {
                    return Conflict(new
                    {
                        mensagem =
                            "Esta permissão já está atribuída ao utilizador para a unidade indicada."
                    });
                }

                // Reativar permissão anteriormente revogada
                existente.Ativo = true;
                existente.DataConcessao = DateTime.Now;
                existente.DataExpiracao =
                    dto.DataExpiracao;
                existente.Observacao =
                    dto.Observacao;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    mensagem =
                        "A permissão foi reativada com sucesso.",
                    permissaoId = existente.Id
                });
            }

            // ========================================================
            // CRIAR NOVA PERMISSÃO
            // ========================================================

            var permissao = new PermissaoAcesso
            {
                UsuarioId =
                    dto.UsuarioId,

                TipoPermissaoId =
                    dto.TipoPermissaoId,

                UnidadeInstitucionalId =
                    dto.UnidadeInstitucionalId,

                Ativo = true,

                DataConcessao =
                    DateTime.Now,

                DataExpiracao =
                    dto.DataExpiracao,

                Observacao =
                    dto.Observacao
            };

            _context.PermissoesAcesso.Add(
                permissao);

            await _context.SaveChangesAsync();

            return Created(
                $"api/Permissoes/{permissao.Id}",
                new
                {
                    mensagem =
                        "Permissão concedida com sucesso.",

                    permissaoId =
                        permissao.Id
                });
        }

        // ============================================================
        // REVOGAR / DESACTIVAR PERMISSÃO
        // PUT: api/Permissoes/5/revogar
        // ============================================================

        [HttpPut("{id:int}/revogar")]
        public async Task<IActionResult> Revogar(
            int id)
        {
            var permissao =
                await _context.PermissoesAcesso
                    .Include(p => p.TipoPermissao)
                    .Include(p => p.UnidadeInstitucional)
                    .FirstOrDefaultAsync(
                        p => p.Id == id);

            if (permissao == null)
            {
                return NotFound(new
                {
                    mensagem =
                        "Permissão não encontrada."
                });
            }

            if (!permissao.Ativo)
            {
                return BadRequest(new
                {
                    mensagem =
                        "Esta permissão já está desactivada."
                });
            }

            permissao.Ativo = false;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem =
                    "Permissão revogada com sucesso.",

                permissaoId =
                    permissao.Id,

                permissao.TipoPermissao.Codigo,

                unidade =
                    permissao.UnidadeInstitucional.Nome
            });
        }

        // ============================================================
        // REACTIVAR PERMISSÃO
        // PUT: api/Permissoes/5/reactivar
        // ============================================================

        [HttpPut("{id:int}/reactivar")]
        public async Task<IActionResult> Reactivar(
            int id)
        {
            var permissao =
                await _context.PermissoesAcesso
                    .Include(p => p.Usuario)
                    .Include(p => p.TipoPermissao)
                    .Include(p => p.UnidadeInstitucional)
                    .FirstOrDefaultAsync(
                        p => p.Id == id);

            if (permissao == null)
            {
                return NotFound(new
                {
                    mensagem =
                        "Permissão não encontrada."
                });
            }

            if (!permissao.Usuario.Ativo)
            {
                return BadRequest(new
                {
                    mensagem =
                        "Não é possível reactivar a permissão de um utilizador inativo."
                });
            }

            if (!permissao.TipoPermissao.Ativo)
            {
                return BadRequest(new
                {
                    mensagem =
                        "O tipo de permissão está inativo."
                });
            }

            if (!permissao.UnidadeInstitucional.Ativo)
            {
                return BadRequest(new
                {
                    mensagem =
                        "A unidade institucional está inativa."
                });
            }

            if (permissao.DataExpiracao.HasValue &&
                permissao.DataExpiracao.Value < DateTime.Now)
            {
                return BadRequest(new
                {
                    mensagem =
                        "A permissão possui uma data de expiração ultrapassada. Defina uma nova data antes de a reactivar."
                });
            }

            permissao.Ativo = true;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem =
                    "Permissão reactivada com sucesso.",

                permissaoId =
                    permissao.Id
            });
        }
    }

    // =================================================================
    // DTO
    // =================================================================

    public class ConcederPermissaoDto
    {
        public int UsuarioId { get; set; }

        public int TipoPermissaoId { get; set; }

        public int UnidadeInstitucionalId { get; set; }

        public DateTime? DataExpiracao { get; set; }

        public string? Observacao { get; set; }
    }
}