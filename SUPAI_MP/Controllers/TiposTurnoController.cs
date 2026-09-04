using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using supai_mp.Data;
using supai_mp.Models.Organizacao;

namespace supai_mp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TiposTurnoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TiposTurnoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/TiposTurno
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TipoTurno>>> GetTurnos()
        {
            var turnos = await _context.TiposTurno
                .AsNoTracking()
                .OrderBy(t => t.Nome)
                .ToListAsync();

            return Ok(turnos);
        }

        // GET: api/TiposTurno/activas
        [HttpGet("activas")]
        public async Task<ActionResult<IEnumerable<TipoTurno>>> GetTurnosActivos()
        {
            var turnos = await _context.TiposTurno
                .AsNoTracking()
                .Where(t => t.Ativo)
                .OrderBy(t => t.Nome)
                .ToListAsync();

            return Ok(turnos);
        }

        // GET: api/TiposTurno/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TipoTurno>> GetTurno(int id)
        {
            var turno = await _context.TiposTurno
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (turno == null)
            {
                return NotFound(new
                {
                    mensagem = "Tipo de turno não encontrado."
                });
            }

            return Ok(turno);
        }

        // POST: api/TiposTurno
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<TipoTurno>> CriarTurno(
            [FromBody] TipoTurno dados)
        {
            if (string.IsNullOrWhiteSpace(dados.Nome))
            {
                return BadRequest(new
                {
                    mensagem = "O nome do tipo de turno é obrigatório."
                });
            }

            if (dados.HorasTrabalho <= 0)
            {
                return BadRequest(new
                {
                    mensagem = "As horas de trabalho devem ser superiores a zero."
                });
            }

            if (dados.HorasDescanso < 0)
            {
                return BadRequest(new
                {
                    mensagem = "As horas de descanso não podem ser negativas."
                });
            }

            var nome = dados.Nome.Trim();

            var existe = await _context.TiposTurno
                .AnyAsync(t =>
                    t.Nome.ToLower() == nome.ToLower());

            if (existe)
            {
                return Conflict(new
                {
                    mensagem =
                        "Já existe um tipo de turno com este nome."
                });
            }

            var novoTurno = new TipoTurno
            {
                Nome = nome,
                Descricao = dados.Descricao?.Trim(),
                HorasTrabalho = dados.HorasTrabalho,
                HorasDescanso = dados.HorasDescanso,
                Ativo = true
            };

            _context.TiposTurno.Add(novoTurno);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetTurno),
                new { id = novoTurno.Id },
                novoTurno);
        }

        // PUT: api/TiposTurno/5
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EditarTurno(
            int id,
            [FromBody] TipoTurno dados)
        {
            var turno = await _context.TiposTurno
                .FirstOrDefaultAsync(t => t.Id == id);

            if (turno == null)
            {
                return NotFound(new
                {
                    mensagem = "Tipo de turno não encontrado."
                });
            }

            if (string.IsNullOrWhiteSpace(dados.Nome))
            {
                return BadRequest(new
                {
                    mensagem = "O nome do tipo de turno é obrigatório."
                });
            }

            if (dados.HorasTrabalho <= 0)
            {
                return BadRequest(new
                {
                    mensagem = "As horas de trabalho devem ser superiores a zero."
                });
            }

            if (dados.HorasDescanso < 0)
            {
                return BadRequest(new
                {
                    mensagem = "As horas de descanso não podem ser negativas."
                });
            }

            var nome = dados.Nome.Trim();

            var duplicado = await _context.TiposTurno
                .AnyAsync(t =>
                    t.Id != id &&
                    t.Nome.ToLower() == nome.ToLower());

            if (duplicado)
            {
                return Conflict(new
                {
                    mensagem =
                        "Já existe outro tipo de turno com este nome."
                });
            }

            turno.Nome = nome;
            turno.Descricao = dados.Descricao?.Trim();
            turno.HorasTrabalho = dados.HorasTrabalho;
            turno.HorasDescanso = dados.HorasDescanso;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Tipo de turno actualizado com sucesso.",
                turno
            });
        }

        // PATCH: api/TiposTurno/5/estado
        [HttpPatch("{id:int}/estado")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> AlterarEstado(
            int id,
            [FromBody] bool ativo)
        {
            var turno = await _context.TiposTurno
                .FirstOrDefaultAsync(t => t.Id == id);

            if (turno == null)
            {
                return NotFound(new
                {
                    mensagem = "Tipo de turno não encontrado."
                });
            }

            turno.Ativo = ativo;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = ativo
                    ? "Tipo de turno activado com sucesso."
                    : "Tipo de turno desactivado com sucesso.",
                turno
            });
        }

        // DELETE: api/TiposTurno/5
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EliminarTurno(int id)
        {
            var turno = await _context.TiposTurno
                .FirstOrDefaultAsync(t => t.Id == id);

            if (turno == null)
            {
                return NotFound(new
                {
                    mensagem = "Tipo de turno não encontrado."
                });
            }

            // Eliminação lógica
            turno.Ativo = false;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Tipo de turno desactivado com sucesso."
            });
        }
    }
}