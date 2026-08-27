using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using supai_mp.Data;
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

            

            //if (!senhaValida)
            //{
            //    return Unauthorized("Utilizador ou senha inválidos.");
            //}

            //var token = GerarToken(usuario);

            return Ok(new
            {
                token = token,
                usuario = usuario.NomeUsuario,
                perfil = usuario.Perfil
            });
        }
        [Authorize]
        [HttpPut("alterar-minha-senha")]
        public async Task<IActionResult> AlterarMinhaSenha(
        AlterarSenhaDto dto)
        {
            var usuarioIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(usuarioIdString))
                return Unauthorized();

            int usuarioId = int.Parse(usuarioIdString);

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario == null)
                return NotFound("Utilizador não encontrado.");

            // Verificar senha atual
            bool senhaCorreta = BCrypt.Net.BCrypt.Verify(
                dto.SenhaAtual,
                usuario.SenhaHash);

            if (!senhaCorreta)
                return BadRequest("A senha atual está incorreta.");

            // Confirmar nova senha
            if (dto.NovaSenha != dto.ConfirmarNovaSenha)
                return BadRequest("A confirmação da nova senha não corresponde.");

            // Impedir reutilização da mesma senha
            if (BCrypt.Net.BCrypt.Verify(dto.NovaSenha, usuario.SenhaHash))
                return BadRequest("A nova senha deve ser diferente da senha atual.");

            // Gerar novo hash
            usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.NovaSenha);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Senha alterada com sucesso."
            });
        }
        private string GerarToken(Models.Usuario usuario)
        {
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
        new Claim(ClaimTypes.Name, usuario.NomeUsuario),
        new Claim(ClaimTypes.Role, usuario.Perfil),

        // Adicione esta linha
        new Claim("Nip", usuario.Funcionario.Nip)
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(8),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        [HttpPost("criar-admin")]
        public async Task<IActionResult> CriarAdmin()
        {
            var existe = await _context.Usuarios
                .AnyAsync(u => u.NomeUsuario == "admin");

            if (existe)
            {
                return BadRequest("O administrador já existe.");
            }

            var usuario = new Models.Usuario
            {
                NomeUsuario = "admin",
                SenhaHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Perfil = "Administrador",
                Ativo = true,
                DataCadastro = DateTime.Now
            };

            _context.Usuarios.Add(usuario);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Administrador criado com sucesso."
            });
        }
        [Authorize(Roles = "Administrador")]
        [HttpPut("associar-funcionario")]
        public async Task<IActionResult> AssociarFuncionario(
        AssociarFuncionarioDto dto)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == dto.UsuarioId);

            if (usuario == null)
            {
                return NotFound("Utilizador não encontrado.");
            }

            var funcionario = await _context.Funcionarios
                .FirstOrDefaultAsync(f => f.Id == dto.FuncionarioId);

            if (funcionario == null)
            {
                return NotFound("Funcionário não encontrado.");
            }

            usuario.FuncionarioId = funcionario.Id;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Utilizador associado ao funcionário com sucesso.",
                usuario = usuario.NomeUsuario,
                funcionario = funcionario.NomeCompleto
            });
        }
    }
}