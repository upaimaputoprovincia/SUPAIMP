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
    public class SectoresController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SectoresController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // GET: api/Sectores
        // Lista todos os sectores
        // =========================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Sector>>> GetSectores()
        {
            var sectores = await _context.Sectores
                .AsNoTracking()
                .Include(s => s.Seccao)
                .OrderBy(s => s.Seccao!.Nome)
                .ThenBy(s => s.Nome)
                .ToListAsync();

            return Ok(sectores);
        }

        // =========================================================
        // GET: api/Sectores/5
        // Consulta um sector específico
        // =========================================================
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Sector>> GetSector(int id)
        {
            var sector = await _context.Sectores
                .AsNoTracking()
                .Include(s => s.Seccao)
                .Include(s => s.Equipas)
                .Include(s => s.Postos)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sector == null)
            {
                return NotFound(new
                {
                    mensagem = "Sector não encontrado."
                });
            }

            return Ok(sector);
        }

        // =========================================================
        // GET: api/Sectores/seccao/5
        // Lista os sectores de uma determinada secção
        // =========================================================
        [HttpGet("seccao/{seccaoId:int}")]
        public async Task<ActionResult<IEnumerable<Sector>>> GetSectoresPorSeccao(
            int seccaoId)
        {
            var seccaoExiste = await _context.Seccoes
                .AnyAsync(s => s.Id == seccaoId);

            if (!seccaoExiste)
            {
                return NotFound(new
                {
                    mensagem = "Secção não encontrada."
                });
            }

            var sectores = await _context.Sectores
                .AsNoTracking()
                .Where(s => s.SeccaoId == seccaoId)
                .OrderBy(s => s.Nome)
                .ToListAsync();

            return Ok(sectores);
        }

        // =========================================================
        // POST: api/Sectores
        // Cria um novo sector
        // =========================================================
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<Sector>> CriarSector(Sector sector)
        {
            if (string.IsNullOrWhiteSpace(sector.Nome))
            {
                return BadRequest(new
                {
                    mensagem = "O nome do sector é obrigatório."
                });
            }

            // Verificar se a secção existe
            var seccao = await _context.Seccoes
                .FirstOrDefaultAsync(s => s.Id == sector.SeccaoId);

            if (seccao == null)
            {
                return BadRequest(new
                {
                    mensagem = "A secção indicada não existe."
                });
            }

            var nome = sector.Nome.Trim();

            // Não permitir dois sectores com o mesmo nome
            // dentro da mesma secção
            var existe = await _context.Sectores
                .AnyAsync(s =>
                    s.SeccaoId == sector.SeccaoId &&
                    s.Nome.ToLower() == nome.ToLower());

            if (existe)
            {
                return Conflict(new
                {
                    mensagem = "Já existe um sector com este nome nesta secção."
                });
            }

            var novoSector = new Sector
            {
                Nome = nome,
                SeccaoId = sector.SeccaoId,
                Descricao = sector.Descricao?.Trim(),
                Ativo = true,
                DataCadastro = DateTime.Now
            };

            _context.Sectores.Add(novoSector);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetSector),
                new { id = novoSector.Id },
                novoSector);
        }

        // =========================================================
        // PUT: api/Sectores/5
        // Edita um sector
        // =========================================================
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EditarSector(
            int id,
            Sector dados)
        {
            var sector = await _context.Sectores
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sector == null)
            {
                return NotFound(new
                {
                    mensagem = "Sector não encontrado."
                });
            }

            if (string.IsNullOrWhiteSpace(dados.Nome))
            {
                return BadRequest(new
                {
                    mensagem = "O nome do sector é obrigatório."
                });
            }

            // Verificar se a nova secção existe
            var seccaoExiste = await _context.Seccoes
                .AnyAsync(s => s.Id == dados.SeccaoId);

            if (!seccaoExiste)
            {
                return BadRequest(new
                {
                    mensagem = "A secção indicada não existe."
                });
            }

            var nome = dados.Nome.Trim();

            var nomeDuplicado = await _context.Sectores
                .AnyAsync(s =>
                    s.Id != id &&
                    s.SeccaoId == dados.SeccaoId &&
                    s.Nome.ToLower() == nome.ToLower());

            if (nomeDuplicado)
            {
                return Conflict(new
                {
                    mensagem = "Já existe outro sector com este nome nesta secção."
                });
            }

            sector.Nome = nome;
            sector.SeccaoId = dados.SeccaoId;
            sector.Descricao = dados.Descricao?.Trim();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Sector actualizado com sucesso.",
                sector
            });
        }

        // =========================================================
        // PATCH: api/Sectores/5/estado
        // Activa ou desactiva um sector
        // =========================================================
        [HttpPatch("{id:int}/estado")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> AlterarEstado(
            int id,
            [FromBody] bool ativo)
        {
            var sector = await _context.Sectores
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sector == null)
            {
                return NotFound(new
                {
                    mensagem = "Sector não encontrado."
                });
            }

            sector.Ativo = ativo;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = ativo
                    ? "Sector activado com sucesso."
                    : "Sector desactivado com sucesso.",
                sector
            });
        }

        // =========================================================
        // DELETE: api/Sectores/5
        // Desactiva o sector sem apagar fisicamente
        // =========================================================
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EliminarSector(int id)
        {
            var sector = await _context.Sectores
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sector == null)
            {
                return NotFound(new
                {
                    mensagem = "Sector não encontrado."
                });
            }

            sector.Ativo = false;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Sector desactivado com sucesso."
            });
        }
    }
}