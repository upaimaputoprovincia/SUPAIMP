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
    public class FuncoesOperacionaisController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FuncoesOperacionaisController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/FuncoesOperacionais
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FuncaoOperacional>>>
            GetFuncoes()
        {
            var funcoes = await _context.FuncoesOperacionais
                .AsNoTracking()
                .OrderBy(f => f.Nome)
                .ToListAsync();

            return Ok(funcoes);
        }

        // GET: api/FuncoesOperacionais/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<FuncaoOperacional>>
            GetFuncao(int id)
        {
            var funcao = await _context.FuncoesOperacionais
                .AsNoTracking()
                .Include(f => f.Lotacoes)
                    .ThenInclude(l => l.Funcionario)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (funcao == null)
            {
                return NotFound(new
                {
                    mensagem = "Função operacional não encontrada."
                });
            }

            return Ok(funcao);
        }

        // GET: api/FuncoesOperacionais/activas
        [HttpGet("activas")]
        public async Task<ActionResult<IEnumerable<FuncaoOperacional>>>
            GetFuncoesActivas()
        {
            var funcoes = await _context.FuncoesOperacionais
                .AsNoTracking()
                .Where(f => f.Ativo)
                .OrderBy(f => f.Nome)
                .ToListAsync();

            return Ok(funcoes);
        }

        // POST: api/FuncoesOperacionais
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<FuncaoOperacional>>
            CriarFuncao(FuncaoOperacional dados)
        {
            if (string.IsNullOrWhiteSpace(dados.Nome))
            {
                return BadRequest(new
                {
                    mensagem = "O nome da função é obrigatório."
                });
            }

            var nome = dados.Nome.Trim();

            var existe = await _context.FuncoesOperacionais
                .AnyAsync(f =>
                    f.Nome.ToLower() == nome.ToLower());

            if (existe)
            {
                return Conflict(new
                {
                    mensagem =
                        "Já existe uma função operacional com este nome."
                });
            }

            var novaFuncao = new FuncaoOperacional
            {
                Nome = nome,
                Descricao = dados.Descricao?.Trim(),
                Ativo = true
            };

            _context.FuncoesOperacionais.Add(novaFuncao);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetFuncao),
                new { id = novaFuncao.Id },
                novaFuncao);
        }

        // PUT: api/FuncoesOperacionais/5
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EditarFuncao(
            int id,
            FuncaoOperacional dados)
        {
            var funcao = await _context.FuncoesOperacionais
                .FirstOrDefaultAsync(f => f.Id == id);

            if (funcao == null)
            {
                return NotFound(new
                {
                    mensagem = "Função operacional não encontrada."
                });
            }

            if (string.IsNullOrWhiteSpace(dados.Nome))
            {
                return BadRequest(new
                {
                    mensagem = "O nome da função é obrigatório."
                });
            }

            var nome = dados.Nome.Trim();

            var duplicada = await _context.FuncoesOperacionais
                .AnyAsync(f =>
                    f.Id != id &&
                    f.Nome.ToLower() == nome.ToLower());

            if (duplicada)
            {
                return Conflict(new
                {
                    mensagem =
                        "Já existe outra função operacional com este nome."
                });
            }

            funcao.Nome = nome;
            funcao.Descricao = dados.Descricao?.Trim();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem =
                    "Função operacional actualizada com sucesso.",
                funcao
            });
        }

        // PATCH: api/FuncoesOperacionais/5/estado
        [HttpPatch("{id:int}/estado")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> AlterarEstado(
            int id,
            [FromBody] bool ativo)
        {
            var funcao = await _context.FuncoesOperacionais
                .FirstOrDefaultAsync(f => f.Id == id);

            if (funcao == null)
            {
                return NotFound(new
                {
                    mensagem = "Função operacional não encontrada."
                });
            }

            funcao.Ativo = ativo;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = ativo
                    ? "Função operacional activada com sucesso."
                    : "Função operacional desactivada com sucesso.",
                funcao
            });
        }

        // DELETE: api/FuncoesOperacionais/5
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EliminarFuncao(int id)
        {
            var funcao = await _context.FuncoesOperacionais
                .FirstOrDefaultAsync(f => f.Id == id);

            if (funcao == null)
            {
                return NotFound(new
                {
                    mensagem = "Função operacional não encontrada."
                });
            }

            // Eliminação lógica
            funcao.Ativo = false;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem =
                    "Função operacional desactivada com sucesso."
            });
        }
    }
}