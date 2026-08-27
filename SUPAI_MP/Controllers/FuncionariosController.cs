using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using supai_mp.Data;
using supai_mp.Models;
using supai_mp.Models.DTOs;
using SUPAI_MP.Data.DTOs;
using System.Security.Claims;

namespace supai_mp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FuncionariosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FuncionariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Funcionarios       
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> GetFuncionarios()
        {
            var funcionarios = await _context.Funcionarios
                .Where(f => f.Estado == EstadoFuncionario.ACTIVO)
                .ToListAsync();

            return Ok(funcionarios);
        }
        // GET: api/Funcionarios/meu-perfil
        [Authorize]
        [HttpGet("meu-perfil")]
        public async Task<IActionResult> MeuPerfil()
        {
            var nip = User.FindFirst("Nip")?.Value;

            if (string.IsNullOrEmpty(nip))
                return Unauthorized();

            var funcionario = await _context.Funcionarios
                .FirstOrDefaultAsync(f => f.Nip == nip);

            if (funcionario == null)
                return NotFound();

            return Ok(funcionario);
        }

        // PUT: api/Funcionarios/meu-perfil
        [Authorize]
        [HttpPut("meu-perfil")]
        public async Task<IActionResult> AtualizarMeuPerfil(
         AtualizarMeuPerfilDto dto)
        {
            var usuarioIdString =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(usuarioIdString))
            {
                return Unauthorized();
            }

            if (!int.TryParse(usuarioIdString, out int usuarioId))
            {
                return Unauthorized();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Funcionario)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario == null)
            {
                return NotFound("Utilizador não encontrado.");
            }

            if (usuario.Funcionario == null)
            {
                return NotFound(
                    "Este utilizador ainda não está associado a um funcionário.");
            }

            var funcionario = usuario.Funcionario;

            funcionario.Contacto = dto.Contacto;
            funcionario.C_Alternativo = dto.C_Alternativo;
            funcionario.C_Familiar = dto.C_Familiar;
            funcionario.Bairro = dto.Bairro;
            funcionario.Quarterao_N = dto.Quarterao_N;
            funcionario.Casa_N = dto.Casa_N;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Perfil atualizado com sucesso."
            });
        }
        // GET: api/Funcionarios/pesquisar?termo=Joao
        [Authorize(Roles = "Administrador")]
        [HttpGet("pesquisar")]
        public async Task<IActionResult> PesquisarFuncionarios(string termo)
        {
            if (string.IsNullOrWhiteSpace(termo))
            {
                return BadRequest("Digite um termo para pesquisar.");
            }

            termo = termo.Trim();

            var funcionarios = await _context.Funcionarios
                .Where(f =>
                    f.Estado == EstadoFuncionario.ACTIVO &&
                    (
                        f.NomeCompleto.Contains(termo) ||
                        f.Nip.Contains(termo) ||
                        f.Bi.Contains(termo) ||
                        f.Nuit.Contains(termo) ||
                        f.Contacto.Contains(termo) ||
                        f.LocalTrabalho.Contains(termo)
                    ))
                .ToListAsync();

            return Ok(funcionarios);
        }
        [Authorize]
        [HttpPost("minha-fotografia")]
        public async Task<IActionResult> AtualizarMinhaFotografia(
            IFormFile fotografia)
        {
            if (fotografia == null || fotografia.Length == 0)
            {
                return BadRequest("Selecione uma fotografia.");
            }

            var usuarioIdString =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(usuarioIdString))
            {
                return Unauthorized();
            }

            if (!int.TryParse(usuarioIdString, out int usuarioId))
            {
                return Unauthorized();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Funcionario)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario == null)
            {
                return NotFound("Utilizador não encontrado.");
            }

            if (usuario.Funcionario == null)
            {
                return NotFound(
                    "Este utilizador não está associado a um funcionário.");
            }

            // Extensões permitidas
            var extensoesPermitidas = new[]
            {
        ".jpg",
        ".jpeg",
        ".png"
    };

            var extensao = Path.GetExtension(
                fotografia.FileName).ToLowerInvariant();

            if (!extensoesPermitidas.Contains(extensao))
            {
                return BadRequest(
                    "Formato inválido. Use JPG, JPEG ou PNG.");
            }

            // Limite de 5 MB
            if (fotografia.Length > 5 * 1024 * 1024)
            {
                return BadRequest(
                    "A fotografia não pode ultrapassar 5 MB.");
            }

            var pastaFotos = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "fotos");

            if (!Directory.Exists(pastaFotos))
            {
                Directory.CreateDirectory(pastaFotos);
            }

            // ==========================================
            // ELIMINAR FOTOGRAFIA ANTIGA
            // ==========================================

            if (!string.IsNullOrEmpty(
                usuario.Funcionario.FotografiaUrl))
            {
                var nomeFotoAntiga =
                    Path.GetFileName(
                        usuario.Funcionario.FotografiaUrl);

                var caminhoFotoAntiga =
                    Path.Combine(
                        pastaFotos,
                        nomeFotoAntiga);

                if (System.IO.File.Exists(caminhoFotoAntiga))
                {
                    System.IO.File.Delete(caminhoFotoAntiga);
                }
            }

            // ==========================================
            // GUARDAR NOVA FOTOGRAFIA
            // ==========================================

            var nomeArquivo =
                $"{Guid.NewGuid()}{extensao}";

            var caminhoArquivo =
                Path.Combine(
                    pastaFotos,
                    nomeArquivo);

            using (var stream = new FileStream(
                caminhoArquivo,
                FileMode.Create))
            {
                await fotografia.CopyToAsync(stream);
            }

            // Atualizar banco de dados
            usuario.Funcionario.FotografiaUrl =
                $"/fotos/{nomeArquivo}";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem =
                    "Fotografia atualizada com sucesso.",

                fotografiaUrl =
                    usuario.Funcionario.FotografiaUrl
            });
        }
        [Authorize]
        [HttpPut("alterar-senha")]
        public async Task<IActionResult> AlterarSenha(
    AlterarSenhaDto dto)
        {
            var usuarioIdString =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(usuarioIdString))
            {
                return Unauthorized();
            }

            if (!int.TryParse(usuarioIdString, out int usuarioId))
            {
                return Unauthorized();
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario == null)
            {
                return NotFound("Utilizador não encontrado.");
            }

            if (!usuario.Ativo)
            {
                return Unauthorized("Este utilizador está inativo.");
            }

            var senhaValida =
                BCrypt.Net.BCrypt.Verify(
                    dto.SenhaAtual,
                    usuario.SenhaHash);

            if (!senhaValida)
            {
                return BadRequest(new
                {
                    mensagem = "A senha atual está incorreta."
                });
            }

            usuario.SenhaHash =
                BCrypt.Net.BCrypt.HashPassword(dto.NovaSenha);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Senha alterada com sucesso."
            });
        }
        [Authorize(Roles = "Administrador")]
        [HttpGet("pesquisa-avancada")]
        public async Task<IActionResult> PesquisaAvancada(
        [FromQuery] FuncionarioPesquisaDto filtro)
        {
            var query = _context.Funcionarios.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro.Nome))
            {
                query = query.Where(f =>
                    f.NomeCompleto.Contains(filtro.Nome));
            }

            if (!string.IsNullOrWhiteSpace(filtro.Nip))
            {
                query = query.Where(f =>
                    f.Nip.Contains(filtro.Nip));
            }

            if (!string.IsNullOrWhiteSpace(filtro.Bi))
            {
                query = query.Where(f =>
                    f.Bi.Contains(filtro.Bi));
            }

            if (!string.IsNullOrWhiteSpace(filtro.Nuit))
            {
                query = query.Where(f =>
                    f.Nuit.Contains(filtro.Nuit));
            }

            if (!string.IsNullOrWhiteSpace(filtro.Contacto))
            {
                query = query.Where(f =>
                    f.Contacto.Contains(filtro.Contacto));
            }

            if (filtro.Categoria.HasValue)
            {
                query = query.Where(f =>
                    f.Categoria == filtro.Categoria.Value);
            }

            if (!string.IsNullOrWhiteSpace(filtro.Funcao))
            {
                query = query.Where(f =>
                    f.Funcao.Contains(filtro.Funcao));
            }

            if (!string.IsNullOrWhiteSpace(filtro.LocalTrabalho))
            {
                query = query.Where(f =>
                    f.LocalTrabalho.Contains(filtro.LocalTrabalho));
            }

            if (filtro.EstadoFuncionario.HasValue)
            {
                query = query.Where(f =>
                    f.Estado == filtro.EstadoFuncionario.Value);
            }

            var funcionarios = await query.ToListAsync();

            return Ok(funcionarios);
        }

        // GET: api/Funcionarios/paginado
        [Authorize(Roles = "Administrador")]
        [HttpGet("paginado")]
        public async Task<IActionResult> GetFuncionariosPaginado(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 20)
        {
            if (pagina < 1)
                pagina = 1;

            if (tamanhoPagina < 1)
                tamanhoPagina = 20;

            if (tamanhoPagina > 100)
                tamanhoPagina = 100;

            var totalRegistros = await _context.Funcionarios.CountAsync();

            var funcionarios = await _context.Funcionarios
                .OrderBy(f => f.NomeCompleto)
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .Select(f => new
                {
                    f.Id,
                    f.NomeCompleto,
                    f.Nip,
                    f.Bi,
                    f.Nuit,
                    f.G,
                    f.estado_civil,
                    f.nivelAcademico,
                    f.grauParentesco,
                    f.Contacto,
                    f.C_Alternativo,
                    f.C_Familiar,
                    f.DataNascimento,
                    f.DataIngresso,
                    f.LocalTrabalho,
                    f.Bairro,
                    f.Quarterao_N,
                    f.Casa_N,
                    f.Categoria,
                    f.Funcao,
                    f.Estado,
                    f.FotografiaUrl,
                    TemUsuario = _context.Usuarios
                        .Any(u => u.FuncionarioId == f.Id)
                })
                .ToListAsync();

            var totalPaginas = (int)Math.Ceiling(
                totalRegistros / (double)tamanhoPagina);

            return Ok(new
            {
                pagina,
                tamanhoPagina,
                totalRegistros,
                totalPaginas,
                dados = funcionarios
            });
        }

        // POST: api/Funcionarios
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> CriarFuncionario(
            FuncionarioCreateDto dto)
        {
            // Verificar se o nome de usuário já existe
            var usuarioExiste = await _context.Usuarios
                .AnyAsync(u => u.NomeUsuario == dto.NomeUsuario);

            if (usuarioExiste)
            {
                return BadRequest(new
                {
                    mensagem = "O nome de usuário já está em uso."
                });
            }

            // Criar funcionário
            var funcionario = new Funcionario
            {
                NomeCompleto = dto.NomeCompleto,
                Nip = dto.Nip,
                Bi = dto.Bi,
                Nuit = dto.Nuit,
                G = dto.Genero,
                estado_civil = dto.EstadoCivil,
                nivelAcademico = dto.NivelAcademico,
                grauParentesco = dto.GrauParentesco,
                Categoria = dto.Categoria,
                Funcao = dto.Funcao,
                Contacto = dto.Contacto,
                C_Alternativo = dto.C_Alternativo,
                C_Familiar = dto.C_Familiar,
                DataNascimento = dto.DataNascimento,
                DataIngresso = dto.DataIngresso,
                LocalTrabalho = dto.LocalTrabalho,
                Bairro = dto.Bairro,
                Quarterao_N = dto.Quarterao_N,
                Casa_N = dto.Casa_N,
                Estado = dto.EstadoFuncionario,
                FotografiaUrl = dto.FotografiaUrl,
                DataCadastro = DateTime.Now
            };

            _context.Funcionarios.Add(funcionario);

            // Salvar primeiro para obter o ID do funcionário
            await _context.SaveChangesAsync();

            // Criar usuário associado ao funcionário
            var usuario = new Usuario
            {
                NomeUsuario = dto.NomeUsuario,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha),
                Perfil = "Funcionario",
                FuncionarioId = funcionario.Id,
                Ativo = true,
                DataCadastro = DateTime.Now
            };

            _context.Usuarios.Add(usuario);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetFuncionario),
                new { id = funcionario.Id },
                new
                {
                    mensagem = "Funcionário e usuário criados com sucesso.",
                    funcionarioId = funcionario.Id,
                    nomeUsuario = usuario.NomeUsuario
                });
        }

        // GET: api/Funcionarios/5
        [Authorize(Roles = "Administrador")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFuncionario(int id)
        {
            var funcionario = await _context.Funcionarios
                .FindAsync(id);

            if (funcionario == null)
            {
                return NotFound();
            }

            return Ok(funcionario);
        }
        // PUT: api/Funcionarios/5
        [Authorize(Roles = "Administrador")]
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarFuncionario(
            int id,
            FuncionarioUpdateDto dto)
        {
            var funcionario = await _context.Funcionarios
                .FindAsync(id);

            if (funcionario == null)
            {
                return NotFound("Funcionário não encontrado.");
            }

            funcionario.NomeCompleto = dto.NomeCompleto;
            funcionario.Nip = dto.Nip;
            funcionario.Bi = dto.Bi;
            funcionario.Nuit = dto.Nuit;

            funcionario.G = dto.Genero;
            funcionario.estado_civil = dto.EstadoCivil;
            funcionario.nivelAcademico = dto.NivelAcademico;
            funcionario.grauParentesco = dto.GrauParentesco;

            funcionario.Contacto = dto.Contacto;
            funcionario.C_Alternativo = dto.C_Alternativo;
            funcionario.C_Familiar = dto.C_Familiar;

            funcionario.DataNascimento = dto.DataNascimento;
            funcionario.DataIngresso = dto.DataIngresso;

            funcionario.LocalTrabalho = dto.LocalTrabalho;
            funcionario.Bairro = dto.Bairro;
            funcionario.Quarterao_N = dto.Quarterao_N;
            funcionario.Casa_N = dto.Casa_N;

            funcionario.Categoria = dto.Categoria;
            funcionario.Funcao = dto.Funcao;
            funcionario.Estado = dto.EstadoFuncionario;

            // NÃO alterar FotografiaUrl.
            // A fotografia possui um processo próprio.

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Funcionário atualizado com sucesso.",
                funcionario
            });
        }
        // POST: api/Funcionarios/5/criar-acesso
        [Authorize(Roles = "Administrador")]
        [HttpPost("{id}/criar-acesso")]
        public async Task<IActionResult> CriarAcesso(
            int id,
            CriarAcessoFuncionarioDto dto)
        {
            var funcionario = await _context.Funcionarios
                .FindAsync(id);

            if (funcionario == null)
            {
                return NotFound(new
                {
                    mensagem = "Funcionário não encontrado."
                });
            }

            // Verificar se o funcionário já possui usuário
            var funcionarioTemUsuario = await _context.Usuarios
                .AnyAsync(u => u.FuncionarioId == id);

            if (funcionarioTemUsuario)
            {
                return BadRequest(new
                {
                    mensagem = "Este funcionário já possui um usuário."
                });
            }

            // Verificar se o nome de usuário já existe
            var nomeUsuarioExiste = await _context.Usuarios
                .AnyAsync(u => u.NomeUsuario == dto.NomeUsuario);

            if (nomeUsuarioExiste)
            {
                return BadRequest(new
                {
                    mensagem = "O nome de usuário já está em uso."
                });
            }

            // Criar usuário
            var usuario = new Usuario
            {
                NomeUsuario = dto.NomeUsuario,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha),
                Perfil = "Funcionario",
                FuncionarioId = funcionario.Id,
                Ativo = true,
                DataCadastro = DateTime.Now
            };

            _context.Usuarios.Add(usuario);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Acesso criado com sucesso.",
                usuario = usuario.NomeUsuario,
                funcionarioId = funcionario.Id
            });
        }

        // GET: api/Funcionarios/acessos
        [Authorize(Roles = "Administrador")]
        [HttpGet("acessos")]
        public async Task<IActionResult> ListarAcessos()
        {
            var acessos = await _context.Usuarios
                .Include(u => u.Funcionario)
                .OrderBy(u => u.Funcionario!.NomeCompleto)
                .Select(u => new
                {
                    u.Id,
                    u.NomeUsuario,
                    u.Perfil,
                    u.Ativo,
                    u.DataCadastro,

                    FuncionarioId = u.FuncionarioId,

                    NomeCompleto = u.Funcionario != null
                        ? u.Funcionario.NomeCompleto
                        : null,

                    Nip = u.Funcionario != null
                        ? u.Funcionario.Nip
                        : null,

                    FotografiaUrl = u.Funcionario != null
                        ? u.Funcionario.FotografiaUrl
                        : null
                })
                .ToListAsync();

            return Ok(acessos);
        }



        // DELETE: api/Funcionarios/5
        [Authorize(Roles = "Administrador")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> InativarFuncionario(int id)
        {
            var funcionario = await _context.Funcionarios.FindAsync(id);

            if (funcionario == null)
                return NotFound("Funcionário não encontrado.");

            funcionario.Estado = EstadoFuncionario.INACTIVO;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Funcionário inativado com sucesso."
            });
        }

    }
}