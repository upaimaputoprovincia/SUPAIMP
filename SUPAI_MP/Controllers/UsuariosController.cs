using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using supai_mp.Models.DTOs;
using supai_mp.Data;

namespace supai_mp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> ListarUsuarios()
        {
            var usuarios = await _context.Usuarios
                .Include(u => u.Funcionario)
                .Select(u => new
                {
                    u.Id,
                    u.NomeUsuario,
                    u.Perfil,

                    Funcionario = u.Funcionario == null
                        ? null
                        : new
                        {
                            u.Funcionario.Id,
                            u.Funcionario.NomeCompleto,
                            u.Funcionario.Nip
                        }
                })
                .ToListAsync();

            return Ok(usuarios);
        }
        [HttpPost]
        public async Task<IActionResult> CriarUsuario(
        CriarUsuarioDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NomeUsuario))
            {
                return BadRequest("O nome de utilizador é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(dto.Senha))
            {
                return BadRequest("A palavra-passe é obrigatória.");
            }

            var usuarioExiste = await _context.Usuarios
                .AnyAsync(u => u.NomeUsuario == dto.NomeUsuario);

            if (usuarioExiste)
            {
                return BadRequest(
                    "Já existe um utilizador com este nome.");
            }

            var funcionario = await _context.Funcionarios
                .FirstOrDefaultAsync(f => f.Id == dto.FuncionarioId);

            if (funcionario == null)
            {
                return NotFound("Funcionário não encontrado.");
            }

            var funcionarioJaAssociado = await _context.Usuarios
                .AnyAsync(u => u.FuncionarioId == dto.FuncionarioId);

            if (funcionarioJaAssociado)
            {
                return BadRequest(
                    "Este funcionário já possui um utilizador.");
            }

            var usuario = new Models.Usuario
            {
                NomeUsuario = dto.NomeUsuario,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha),
                Perfil = dto.Perfil,
                FuncionarioId = dto.FuncionarioId
            };

            _context.Usuarios.Add(usuario);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Utilizador criado com sucesso.",
                usuario = usuario.NomeUsuario,
                funcionario = funcionario.NomeCompleto
            });
        }
    }
}