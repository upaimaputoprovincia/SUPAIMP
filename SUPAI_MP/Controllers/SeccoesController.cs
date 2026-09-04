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
    public class SeccoesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SeccoesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // GET: api/Seccoes
        // Lista todas as secções
        // =========================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Seccao>>> GetSeccoes()
        {
            var seccoes = await _context.Seccoes
                .AsNoTracking()
                .OrderBy(s => s.Nome)
                .ToListAsync();

            return Ok(seccoes);
        }

        // =========================================================
        // GET: api/Seccoes/5
        // Consulta uma secção específica
        // =========================================================
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Seccao>> GetSeccao(int id)
        {
            var seccao = await _context.Seccoes
                .AsNoTracking()
                .Include(s => s.Sectores)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (seccao == null)
                return NotFound(new
                {
                    mensagem = "Secção não encontrada."
                });

            return Ok(seccao);
        }

        // =========================================================
        // POST: api/Seccoes
        // Cria uma nova secção
        // =========================================================
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<Seccao>> CriarSeccao(Seccao seccao)
        {
            if (string.IsNullOrWhiteSpace(seccao.Nome))
            {
                return BadRequest(new
                {
                    mensagem = "O nome da secção é obrigatório."
                });
            }

            var nome = seccao.Nome.Trim();

            var existe = await _context.Seccoes
                .AnyAsync(s => s.Nome.ToLower() == nome.ToLower());

            if (existe)
            {
                return Conflict(new
                {
                    mensagem = "Já existe uma secção com este nome."
                });
            }

            var novaSeccao = new Seccao
            {
                Nome = nome,
                Descricao = seccao.Descricao?.Trim(),
                Ativo = true,
                DataCadastro = DateTime.Now
            };

            _context.Seccoes.Add(novaSeccao);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetSeccao),
                new { id = novaSeccao.Id },
                novaSeccao);
        }

        // =========================================================
        // PUT: api/Seccoes/5
        // Edita uma secção
        // =========================================================
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EditarSeccao(
            int id,
            Seccao dados)
        {
            var seccao = await _context.Seccoes
                .FirstOrDefaultAsync(s => s.Id == id);

            if (seccao == null)
            {
                return NotFound(new
                {
                    mensagem = "Secção não encontrada."
                });
            }

            if (string.IsNullOrWhiteSpace(dados.Nome))
            {
                return BadRequest(new
                {
                    mensagem = "O nome da secção é obrigatório."
                });
            }

            var nome = dados.Nome.Trim();

            var nomeDuplicado = await _context.Seccoes
                .AnyAsync(s =>
                    s.Id != id &&
                    s.Nome.ToLower() == nome.ToLower());

            if (nomeDuplicado)
            {
                return Conflict(new
                {
                    mensagem = "Já existe outra secção com este nome."
                });
            }

            seccao.Nome = nome;
            seccao.Descricao = dados.Descricao?.Trim();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Secção actualizada com sucesso.",
                seccao
            });
        }

        // =========================================================
        // PATCH: api/Seccoes/5/estado
        // Activa ou desactiva uma secção
        // =========================================================
        [HttpPatch("{id:int}/estado")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> AlterarEstado(
            int id,
            [FromBody] bool ativo)
        {
            var seccao = await _context.Seccoes
                .FirstOrDefaultAsync(s => s.Id == id);

            if (seccao == null)
            {
                return NotFound(new
                {
                    mensagem = "Secção não encontrada."
                });
            }

            seccao.Ativo = ativo;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = ativo
                    ? "Secção activada com sucesso."
                    : "Secção desactivada com sucesso.",
                seccao
            });
        }

        // =========================================================
        // DELETE: api/Seccoes/5
        // Não elimina fisicamente.
        // Apenas desactiva.
        // =========================================================
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EliminarSeccao(int id)
        {
            var seccao = await _context.Seccoes
                .FirstOrDefaultAsync(s => s.Id == id);

            if (seccao == null)
            {
                return NotFound(new
                {
                    mensagem = "Secção não encontrada."
                });
            }

            seccao.Ativo = false;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Secção desactivada com sucesso."
            });
        }
    }
}