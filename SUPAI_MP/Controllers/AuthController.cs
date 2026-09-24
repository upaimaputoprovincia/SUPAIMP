using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using supai_mp.Data;
using supai_mp.Models;
using supai_mp.Models.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace supai_mp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // =========================================================
        // LOGIN
        // =========================================================

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Funcionario)
                .FirstOrDefaultAsync(u =>
                    u.NomeUsuario == dto.NomeUsuario);

            if (usuario == null)
            {
                return Unauthorized("Utilizador ou senha inválidos.");
            }

            if (!usuario.Ativo)
            {
                return Unauthorized("Este utilizador está desativado.");
            }

            if (usuario.Funcionario == null)
            {
                return Unauthorized(
                    "Este utilizador não está associado a um funcionário.");
            }

            bool senhaValida = BCrypt.Net.BCrypt.Verify(
                dto.Senha,
                usuario.SenhaHash);

            if (!senhaValida)
            {
                return Unauthorized("Utilizador ou senha inválidos.");
            }

            var token = GerarToken(usuario);

            return Ok(new
            {
                token = token,
                usuario = usuario.NomeUsuario,
                perfil = usuario.Perfil,
                funcionarioId = usuario.FuncionarioId,
                funcionario = usuario.Funcionario.NomeCompleto
            });
        }

        // =========================================================
        // ALTERAR MINHA SENHA
        // =========================================================

        [Authorize]
        [HttpPut("alterar-minha-senha")]
        public async Task<IActionResult> AlterarMinhaSenha(
            AlterarSenhaDto dto)
        {
            var usuarioIdString =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(usuarioIdString) ||
                !int.TryParse(usuarioIdString, out int usuarioId))
            {
                return Unauthorized();
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario == null)
            {
                return NotFound("Utilizador não encontrado.");
            }

            bool senhaCorreta = BCrypt.Net.BCrypt.Verify(
                dto.SenhaAtual,
                usuario.SenhaHash);

            if (!senhaCorreta)
            {
                return BadRequest(
                    "A senha atual está incorreta.");
            }

            if (dto.NovaSenha != dto.ConfirmarNovaSenha)
            {
                return BadRequest(
                    "A confirmação da nova senha não corresponde.");
            }

            if (BCrypt.Net.BCrypt.Verify(
                dto.NovaSenha,
                usuario.SenhaHash))
            {
                return BadRequest(
                    "A nova senha deve ser diferente da senha atual.");
            }

            usuario.SenhaHash =
                BCrypt.Net.BCrypt.HashPassword(dto.NovaSenha);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Senha alterada com sucesso."
            });
        }

        // =========================================================
        // LISTAR UTILIZADORES
        // =========================================================

        [Authorize(Roles = PerfisUsuario.Administrador)]
        [HttpGet("utilizadores")]
        public async Task<IActionResult> ListarUtilizadores()
        {
            var utilizadores = await _context.Usuarios
                .Include(u => u.Funcionario)
                .OrderBy(u => u.NomeUsuario)
                .Select(u => new
                {
                    id = u.Id,
                    nomeUsuario = u.NomeUsuario,
                    perfil = u.Perfil,
                    ativo = u.Ativo,
                    dataCadastro = u.DataCadastro,

                    funcionarioId = u.FuncionarioId,

                    funcionario = u.Funcionario == null
                        ? null
                        : new
                        {
                            id = u.Funcionario.Id,
                            nomeCompleto = u.Funcionario.NomeCompleto,
                            nip = u.Funcionario.Nip
                        }
                })
                .ToListAsync();

            return Ok(utilizadores);
        }

        // =========================================================
        // OBTER UTILIZADOR POR ID
        // =========================================================

        [Authorize(Roles = PerfisUsuario.Administrador)]
        [HttpGet("utilizadores/{id:int}")]
        public async Task<IActionResult> ObterUtilizador(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Funcionario)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
            {
                return NotFound("Utilizador não encontrado.");
            }

            return Ok(new
            {
                id = usuario.Id,
                nomeUsuario = usuario.NomeUsuario,
                perfil = usuario.Perfil,
                ativo = usuario.Ativo,
                dataCadastro = usuario.DataCadastro,

                funcionarioId = usuario.FuncionarioId,

                funcionario = usuario.Funcionario == null
                    ? null
                    : new
                    {
                        id = usuario.Funcionario.Id,
                        nomeCompleto = usuario.Funcionario.NomeCompleto,
                        nip = usuario.Funcionario.Nip
                    }
            });
        }

        // =========================================================
        // CRIAR UTILIZADOR
        // =========================================================

        [Authorize(Roles = PerfisUsuario.Administrador)]
        [HttpPost("utilizadores")]
        public async Task<IActionResult> CriarUtilizador(
            CriarUsuarioDto dto)
        {
            // -----------------------------------------------------
            // Validar perfil
            // -----------------------------------------------------

            var perfil = PerfisUsuario.Todos
                .FirstOrDefault(p =>
                    string.Equals(
                        p,
                        dto.Perfil,
                        StringComparison.OrdinalIgnoreCase));

            if (perfil == null)
            {
                return BadRequest(new
                {
                    mensagem = "Perfil inválido.",
                    perfisPermitidos = PerfisUsuario.Todos
                });
            }

            // -----------------------------------------------------
            // Verificar nome de utilizador
            // -----------------------------------------------------

            var nomeUsuario = dto.NomeUsuario.Trim();

            if (string.IsNullOrWhiteSpace(nomeUsuario))
            {
                return BadRequest(
                    "O nome de utilizador é obrigatório.");
            }

            bool nomeExiste = await _context.Usuarios
                .AnyAsync(u => u.NomeUsuario == nomeUsuario);

            if (nomeExiste)
            {
                return Conflict(
                    "Já existe um utilizador com esse nome.");
            }

            // -----------------------------------------------------
            // Verificar funcionário
            // -----------------------------------------------------

            var funcionario = await _context.Funcionarios
                .FirstOrDefaultAsync(f =>
                    f.Id == dto.FuncionarioId);

            if (funcionario == null)
            {
                return NotFound(
                    "Funcionário não encontrado.");
            }

            // -----------------------------------------------------
            // Verificar se funcionário já possui acesso
            // -----------------------------------------------------

            bool funcionarioJaPossuiUsuario =
                await _context.Usuarios
                    .AnyAsync(u =>
                        u.FuncionarioId == funcionario.Id);

            if (funcionarioJaPossuiUsuario)
            {
                return Conflict(
                    "Este funcionário já possui um utilizador associado.");
            }

            // -----------------------------------------------------
            // Criar utilizador
            // -----------------------------------------------------

            var usuario = new Usuario
            {
                NomeUsuario = nomeUsuario,

                SenhaHash =
                    BCrypt.Net.BCrypt.HashPassword(dto.Senha),

                Perfil = perfil,

                FuncionarioId = funcionario.Id,

                Ativo = true,

                DataCadastro = DateTime.Now
            };

            _context.Usuarios.Add(usuario);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(ObterUtilizador),
                new { id = usuario.Id },
                new
                {
                    mensagem =
                        "Utilizador criado com sucesso.",

                    id = usuario.Id,

                    nomeUsuario = usuario.NomeUsuario,

                    perfil = usuario.Perfil,

                    ativo = usuario.Ativo,

                    funcionarioId =
                        funcionario.Id,

                    funcionario =
                        funcionario.NomeCompleto
                });
        }

        // =========================================================
        // EDITAR UTILIZADOR
        // =========================================================

        [Authorize(Roles = PerfisUsuario.Administrador)]
        [HttpPut("utilizadores/{id:int}")]
        public async Task<IActionResult> EditarUtilizador(
            int id,
            EditarUsuarioDto dto)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
            {
                return NotFound("Utilizador não encontrado.");
            }

            // -----------------------------------------------------
            // Validar perfil
            // -----------------------------------------------------

            var perfil = PerfisUsuario.Todos
                .FirstOrDefault(p =>
                    string.Equals(
                        p,
                        dto.Perfil,
                        StringComparison.OrdinalIgnoreCase));

            if (perfil == null)
            {
                return BadRequest(new
                {
                    mensagem = "Perfil inválido.",
                    perfisPermitidos = PerfisUsuario.Todos
                });
            }

            // -----------------------------------------------------
            // Verificar nome de utilizador duplicado
            // -----------------------------------------------------

            var nomeUsuario = dto.NomeUsuario.Trim();

            bool nomeExiste = await _context.Usuarios
                .AnyAsync(u =>
                    u.Id != id &&
                    u.NomeUsuario == nomeUsuario);

            if (nomeExiste)
            {
                return Conflict(
                    "Já existe outro utilizador com esse nome.");
            }

            // -----------------------------------------------------
            // Verificar funcionário
            // -----------------------------------------------------

            var funcionario = await _context.Funcionarios
                .FirstOrDefaultAsync(f =>
                    f.Id == dto.FuncionarioId);

            if (funcionario == null)
            {
                return NotFound(
                    "Funcionário não encontrado.");
            }

            // -----------------------------------------------------
            // Verificar se outro utilizador usa o funcionário
            // -----------------------------------------------------

            bool funcionarioJaUtilizado =
                await _context.Usuarios
                    .AnyAsync(u =>
                        u.Id != id &&
                        u.FuncionarioId == funcionario.Id);

            if (funcionarioJaUtilizado)
            {
                return Conflict(
                    "Este funcionário já está associado a outro utilizador.");
            }

            // -----------------------------------------------------
            // Impedir retirar o último administrador ativo
            // -----------------------------------------------------

            if (usuario.Perfil ==
                    PerfisUsuario.Administrador &&
                perfil !=
                    PerfisUsuario.Administrador)
            {
                int administradoresAtivos =
                    await _context.Usuarios
                        .CountAsync(u =>
                            u.Ativo &&
                            u.Perfil ==
                            PerfisUsuario.Administrador);

                if (administradoresAtivos <= 1)
                {
                    return BadRequest(
                        "Não é possível retirar o perfil do último administrador ativo.");
                }
            }

            // -----------------------------------------------------
            // Atualizar
            // -----------------------------------------------------

            usuario.NomeUsuario = nomeUsuario;
            usuario.Perfil = perfil;
            usuario.FuncionarioId = funcionario.Id;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem =
                    "Utilizador atualizado com sucesso.",

                id = usuario.Id,

                nomeUsuario = usuario.NomeUsuario,

                perfil = usuario.Perfil,

                funcionarioId =
                    usuario.FuncionarioId,

                funcionario =
                    funcionario.NomeCompleto,

                ativo = usuario.Ativo
            });
        }

        // =========================================================
        // ATIVAR / DESATIVAR UTILIZADOR
        // =========================================================

        [Authorize(Roles = PerfisUsuario.Administrador)]
        [HttpPut("utilizadores/{id:int}/estado")]
        public async Task<IActionResult> AlterarEstadoUtilizador(
            int id,
            AlterarEstadoUsuarioDto dto)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
            {
                return NotFound("Utilizador não encontrado.");
            }

            // -----------------------------------------------------
            // Impedir desativar o último administrador ativo
            // -----------------------------------------------------

            if (!dto.Ativo &&
                usuario.Ativo &&
                usuario.Perfil ==
                PerfisUsuario.Administrador)
            {
                int administradoresAtivos =
                    await _context.Usuarios
                        .CountAsync(u =>
                            u.Ativo &&
                            u.Perfil ==
                            PerfisUsuario.Administrador);

                if (administradoresAtivos <= 1)
                {
                    return BadRequest(
                        "Não é possível desativar o último administrador ativo.");
                }
            }

            usuario.Ativo = dto.Ativo;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = usuario.Ativo
                    ? "Utilizador ativado com sucesso."
                    : "Utilizador desativado com sucesso.",

                id = usuario.Id,

                nomeUsuario = usuario.NomeUsuario,

                ativo = usuario.Ativo
            });
        }

        // =========================================================
        // ALTERAR SENHA DE UM UTILIZADOR
        // ADMINISTRADOR
        // =========================================================

        [Authorize(Roles = PerfisUsuario.Administrador)]
        [HttpPut("utilizadores/{id:int}/senha")]
        public async Task<IActionResult> AlterarSenhaUtilizador(
            int id,
            AlterarSenhaUsuarioDto dto)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
            {
                return NotFound("Utilizador não encontrado.");
            }

            if (dto.NovaSenha != dto.ConfirmarNovaSenha)
            {
                return BadRequest(
                    "A confirmação da nova senha não corresponde.");
            }

            if (BCrypt.Net.BCrypt.Verify(
                dto.NovaSenha,
                usuario.SenhaHash))
            {
                return BadRequest(
                    "A nova senha deve ser diferente da senha atual.");
            }

            usuario.SenhaHash =
                BCrypt.Net.BCrypt.HashPassword(dto.NovaSenha);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem =
                    "Senha do utilizador alterada com sucesso."
            });
        }

        // =========================================================
        // ALTERAR ASSOCIAÇÃO DO FUNCIONÁRIO
        // =========================================================

        [Authorize(Roles = PerfisUsuario.Administrador)]
        [HttpPut("associar-funcionario")]
        public async Task<IActionResult> AssociarFuncionario(
            AssociarFuncionarioDto dto)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.Id == dto.UsuarioId);

            if (usuario == null)
            {
                return NotFound(
                    "Utilizador não encontrado.");
            }

            var funcionario = await _context.Funcionarios
                .FirstOrDefaultAsync(f =>
                    f.Id == dto.FuncionarioId);

            if (funcionario == null)
            {
                return NotFound(
                    "Funcionário não encontrado.");
            }

            // -----------------------------------------------------
            // Verificar se outro utilizador já possui este funcionário
            // -----------------------------------------------------

            bool funcionarioJaUtilizado =
                await _context.Usuarios
                    .AnyAsync(u =>
                        u.Id != usuario.Id &&
                        u.FuncionarioId == funcionario.Id);

            if (funcionarioJaUtilizado)
            {
                return Conflict(
                    "Este funcionário já está associado a outro utilizador.");
            }

            usuario.FuncionarioId = funcionario.Id;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem =
                    "Utilizador associado ao funcionário com sucesso.",

                usuario = usuario.NomeUsuario,

                funcionario = funcionario.NomeCompleto,

                funcionarioId = funcionario.Id
            });
        }

        // =========================================================
        // GERAR TOKEN JWT
        // =========================================================

        private string GerarToken(Usuario usuario)
        {
            if (usuario.Funcionario == null)
            {
                throw new InvalidOperationException(
                    "O utilizador precisa estar associado a um funcionário para gerar o token.");
            }

            var claims = new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuario.Id.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    usuario.NomeUsuario),

                new Claim(
                    ClaimTypes.Role,
                    usuario.Perfil),

                new Claim(
                    "Nip",
                    usuario.Funcionario.Nip)
            };

            var jwtKey = _configuration["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                throw new InvalidOperationException(
                    "Jwt:Key não está configurada.");
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey));

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }

        // =========================================================
        // CRIAR / RECUPERAR ADMINISTRADOR INICIAL
        // =========================================================
        //
        // ESTE ENDPOINT É TEMPORÁRIO.
        // Depois de confirmarmos que a nova gestão de utilizadores
        // funciona, vamos removê-lo/protegê-lo.
        //
        // =========================================================

        [AllowAnonymous]
        [HttpPost("criar-admin")]
        public async Task<IActionResult> CriarAdmin()
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.NomeUsuario == "admin");

            var funcionario = await _context.Funcionarios
                .FirstOrDefaultAsync(f =>
                    f.Nip == "11452654");

            // -----------------------------------------------------
            // Criar funcionário caso não exista
            // -----------------------------------------------------

            if (funcionario == null)
            {
                funcionario = new Funcionario
                {
                    NomeCompleto = "MILTON EDUARDO CUMBE",
                    Nip = "11452654",
                    Bi = "110500829930I",
                    Nuit = "1457965",

                    G = Genero.M,

                    estado_civil =
                        EstadoCivil.Solteiro,

                    nivelAcademico =
                        NivelAcademico.Técnico,

                    grauParentesco =
                        GrauParentesco.MÃE,

                    Contacto = "840474886",
                    C_Alternativo = "870843769",
                    C_Familiar = "849523864",

                    DataNascimento =
                        new DateTime(1995, 11, 12),

                    DataIngresso =
                        new DateTime(2021, 10, 27),

                    LocalTrabalho = "SUPAI_MP",
                    Bairro = "LUIS CABRAL",
                    Quarterao_N = "38",
                    Casa_N = "60",

                    Categoria = Categoria.GUA,
                    Funcao = "TECNICO",

                    Estado =
                        EstadoFuncionario.ACTIVO,

                    DataCadastro = DateTime.Now
                };

                _context.Funcionarios.Add(funcionario);

                await _context.SaveChangesAsync();
            }

            // -----------------------------------------------------
            // Criar admin caso não exista
            // -----------------------------------------------------

            if (usuario == null)
            {
                usuario = new Usuario
                {
                    NomeUsuario = "admin",

                    SenhaHash =
                        BCrypt.Net.BCrypt.HashPassword(
                            "Admin@123"),

                    Perfil =
                        PerfisUsuario.Administrador,

                    Ativo = true,

                    DataCadastro = DateTime.Now,

                    FuncionarioId =
                        funcionario.Id
                };

                _context.Usuarios.Add(usuario);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    mensagem =
                        "Administrador criado e associado ao funcionário com sucesso.",

                    usuario = usuario.NomeUsuario,

                    funcionario =
                        funcionario.NomeCompleto
                });
            }

            // -----------------------------------------------------
            // Admin já existe
            // -----------------------------------------------------

            if (usuario.FuncionarioId != null)
            {
                return Ok(new
                {
                    mensagem =
                        "O administrador já existe e está associado a um funcionário.",

                    usuario = usuario.NomeUsuario,

                    funcionarioId =
                        usuario.FuncionarioId
                });
            }

            // -----------------------------------------------------
            // Associar funcionário ao admin existente
            // -----------------------------------------------------

            usuario.FuncionarioId =
                funcionario.Id;

            usuario.Perfil =
                PerfisUsuario.Administrador;

            usuario.Ativo = true;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem =
                    "Administrador associado ao funcionário com sucesso.",

                usuario = usuario.NomeUsuario,

                funcionario =
                    funcionario.NomeCompleto,

                funcionarioId =
                    funcionario.Id
            });
        }
    }
}