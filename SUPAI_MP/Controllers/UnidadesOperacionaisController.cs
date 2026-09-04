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
    public class UnidadesOperacionaisController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UnidadesOperacionaisController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // DTOs
        // =========================================================

        public class UnidadeOperacionalListaDto
        {
            public int Id { get; set; }
            public string Nome { get; set; } = string.Empty;
            public UnidadeOperacional.TipoUnidadeOperacional Tipo { get; set; }
            public int SeccaoId { get; set; }
            public string? Seccao { get; set; }
            public int? UnidadePaiId { get; set; }
            public string? UnidadePai { get; set; }
            public string? Descricao { get; set; }
            public bool Ativo { get; set; }
            public DateTime DataCadastro { get; set; }
        }

        public class UnidadeOperacionalDetalheDto
        {
            public int Id { get; set; }
            public string Nome { get; set; } = string.Empty;
            public UnidadeOperacional.TipoUnidadeOperacional Tipo { get; set; }

            public int SeccaoId { get; set; }
            public string? Seccao { get; set; }

            public int? UnidadePaiId { get; set; }
            public string? UnidadePai { get; set; }

            public string? Descricao { get; set; }
            public bool Ativo { get; set; }
            public DateTime DataCadastro { get; set; }

            public List<UnidadeFilhaDto> UnidadesFilhas { get; set; }
                = new();

            public List<PostoDto> Postos { get; set; }
                = new();

            public List<EquipaDto> Equipas { get; set; }
                = new();
        }

        public class UnidadeFilhaDto
        {
            public int Id { get; set; }
            public string Nome { get; set; } = string.Empty;
            public UnidadeOperacional.TipoUnidadeOperacional Tipo { get; set; }
            public bool Ativo { get; set; }
        }

        public class PostoDto
        {
            public int Id { get; set; }
            public string Codigo { get; set; } = string.Empty;
            public string Nome { get; set; } = string.Empty;
            public string? Localizacao { get; set; }
            public bool Ativo { get; set; }
        }

        public class EquipaDto
        {
            public int Id { get; set; }
            public string Nome { get; set; } = string.Empty;
            public bool Ativo { get; set; }
        }

        // =========================================================
        // GET: api/UnidadesOperacionais
        // Lista todas as unidades operacionais
        // =========================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UnidadeOperacionalListaDto>>>
            GetUnidadesOperacionais()
        {
            var unidades = await _context.UnidadesOperacionais
                .AsNoTracking()
                .Include(u => u.Seccao)
                .Include(u => u.UnidadePai)
                .OrderBy(u => u.Seccao!.Nome)
                .ThenBy(u => u.Nome)
                .Select(u => new UnidadeOperacionalListaDto
                {
                    Id = u.Id,
                    Nome = u.Nome,
                    Tipo = u.Tipo,
                    SeccaoId = u.SeccaoId,
                    Seccao = u.Seccao != null
                        ? u.Seccao.Nome
                        : null,
                    UnidadePaiId = u.UnidadePaiId,
                    UnidadePai = u.UnidadePai != null
                        ? u.UnidadePai.Nome
                        : null,
                    Descricao = u.Descricao,
                    Ativo = u.Ativo,
                    DataCadastro = u.DataCadastro
                })
                .ToListAsync();

            return Ok(unidades);
        }

        // =========================================================
        // GET: api/UnidadesOperacionais/5
        // Consulta uma unidade específica
        // =========================================================
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UnidadeOperacionalDetalheDto>>
            GetUnidadeOperacional(int id)
        {
            var unidade = await _context.UnidadesOperacionais
                .AsNoTracking()
                .Include(u => u.Seccao)
                .Include(u => u.UnidadePai)
                .Include(u => u.UnidadesFilhas)
                .Include(u => u.Postos)
                .Include(u => u.Equipas)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (unidade == null)
            {
                return NotFound(new
                {
                    mensagem = "Unidade operacional não encontrada."
                });
            }

            var resultado = new UnidadeOperacionalDetalheDto
            {
                Id = unidade.Id,
                Nome = unidade.Nome,
                Tipo = unidade.Tipo,

                SeccaoId = unidade.SeccaoId,
                Seccao = unidade.Seccao?.Nome,

                UnidadePaiId = unidade.UnidadePaiId,
                UnidadePai = unidade.UnidadePai?.Nome,

                Descricao = unidade.Descricao,
                Ativo = unidade.Ativo,
                DataCadastro = unidade.DataCadastro,

                UnidadesFilhas = unidade.UnidadesFilhas
                    .Select(f => new UnidadeFilhaDto
                    {
                        Id = f.Id,
                        Nome = f.Nome,
                        Tipo = f.Tipo,
                        Ativo = f.Ativo
                    })
                    .OrderBy(f => f.Nome)
                    .ToList(),

                Postos = unidade.Postos
                    .Select(p => new PostoDto
                    {
                        Id = p.Id,
                        Codigo = p.Codigo,
                        Nome = p.Nome,
                        Localizacao = p.Localizacao,
                        Ativo = p.Ativo
                    })
                    .OrderBy(p => p.Nome)
                    .ToList(),

                Equipas = unidade.Equipas
                    .Select(e => new EquipaDto
                    {
                        Id = e.Id,
                        Nome = e.Nome,
                        Ativo = e.Ativo
                    })
                    .OrderBy(e => e.Nome)
                    .ToList()
            };

            return Ok(resultado);
        }

        // =========================================================
        // GET: api/UnidadesOperacionais/seccao/5
        // Lista unidades de uma secção
        // =========================================================
        [HttpGet("seccao/{seccaoId:int}")]
        public async Task<ActionResult<IEnumerable<UnidadeOperacionalListaDto>>>
            GetPorSeccao(int seccaoId)
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

            var unidades = await _context.UnidadesOperacionais
                .AsNoTracking()
                .Include(u => u.Seccao)
                .Include(u => u.UnidadePai)
                .Where(u => u.SeccaoId == seccaoId)
                .OrderBy(u => u.Nome)
                .Select(u => new UnidadeOperacionalListaDto
                {
                    Id = u.Id,
                    Nome = u.Nome,
                    Tipo = u.Tipo,
                    SeccaoId = u.SeccaoId,
                    Seccao = u.Seccao != null
                        ? u.Seccao.Nome
                        : null,
                    UnidadePaiId = u.UnidadePaiId,
                    UnidadePai = u.UnidadePai != null
                        ? u.UnidadePai.Nome
                        : null,
                    Descricao = u.Descricao,
                    Ativo = u.Ativo,
                    DataCadastro = u.DataCadastro
                })
                .ToListAsync();

            return Ok(unidades);
        }

        // =========================================================
        // GET: api/UnidadesOperacionais/pai/5
        // Lista unidades filhas de uma unidade
        // =========================================================
        [HttpGet("pai/{unidadePaiId:int}")]
        public async Task<ActionResult<IEnumerable<UnidadeOperacionalListaDto>>>
            GetFilhas(int unidadePaiId)
        {
            var paiExiste = await _context.UnidadesOperacionais
                .AnyAsync(u => u.Id == unidadePaiId);

            if (!paiExiste)
            {
                return NotFound(new
                {
                    mensagem = "Unidade operacional pai não encontrada."
                });
            }

            var unidades = await _context.UnidadesOperacionais
                .AsNoTracking()
                .Include(u => u.Seccao)
                .Include(u => u.UnidadePai)
                .Where(u => u.UnidadePaiId == unidadePaiId)
                .OrderBy(u => u.Nome)
                .Select(u => new UnidadeOperacionalListaDto
                {
                    Id = u.Id,
                    Nome = u.Nome,
                    Tipo = u.Tipo,
                    SeccaoId = u.SeccaoId,
                    Seccao = u.Seccao != null
                        ? u.Seccao.Nome
                        : null,
                    UnidadePaiId = u.UnidadePaiId,
                    UnidadePai = u.UnidadePai != null
                        ? u.UnidadePai.Nome
                        : null,
                    Descricao = u.Descricao,
                    Ativo = u.Ativo,
                    DataCadastro = u.DataCadastro
                })
                .ToListAsync();

            return Ok(unidades);
        }

        // =========================================================
        // POST: api/UnidadesOperacionais
        // Cria uma unidade operacional
        // =========================================================
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<UnidadeOperacionalListaDto>>
            CriarUnidade(UnidadeOperacional dados)
        {
            if (string.IsNullOrWhiteSpace(dados.Nome))
            {
                return BadRequest(new
                {
                    mensagem = "O nome da unidade operacional é obrigatório."
                });
            }

            var seccaoExiste = await _context.Seccoes
                .AnyAsync(s => s.Id == dados.SeccaoId);

            if (!seccaoExiste)
            {
                return BadRequest(new
                {
                    mensagem = "A secção indicada não existe."
                });
            }

            if (dados.UnidadePaiId.HasValue)
            {
                var pai = await _context.UnidadesOperacionais
                    .FirstOrDefaultAsync(u =>
                        u.Id == dados.UnidadePaiId.Value);

                if (pai == null)
                {
                    return BadRequest(new
                    {
                        mensagem = "A unidade operacional pai indicada não existe."
                    });
                }

                if (pai.SeccaoId != dados.SeccaoId)
                {
                    return BadRequest(new
                    {
                        mensagem = "A unidade pai deve pertencer à mesma secção."
                    });
                }

                if (pai.Id == dados.Id && dados.Id != 0)
                {
                    return BadRequest(new
                    {
                        mensagem = "Uma unidade não pode ser pai de si própria."
                    });
                }
            }

            var nome = dados.Nome.Trim();

            var existe = await _context.UnidadesOperacionais
                .AnyAsync(u =>
                    u.SeccaoId == dados.SeccaoId &&
                    u.UnidadePaiId == dados.UnidadePaiId &&
                    u.Nome.ToLower() == nome.ToLower());

            if (existe)
            {
                return Conflict(new
                {
                    mensagem = "Já existe uma unidade com este nome nesta estrutura."
                });
            }

            var novaUnidade = new UnidadeOperacional
            {
                Nome = nome,
                Tipo = dados.Tipo,
                SeccaoId = dados.SeccaoId,
                UnidadePaiId = dados.UnidadePaiId,
                Descricao = dados.Descricao?.Trim(),
                Ativo = true,
                DataCadastro = DateTime.Now
            };

            _context.UnidadesOperacionais.Add(novaUnidade);

            await _context.SaveChangesAsync();

            var criada = await _context.UnidadesOperacionais
                .AsNoTracking()
                .Include(u => u.Seccao)
                .Include(u => u.UnidadePai)
                .Where(u => u.Id == novaUnidade.Id)
                .Select(u => new UnidadeOperacionalListaDto
                {
                    Id = u.Id,
                    Nome = u.Nome,
                    Tipo = u.Tipo,
                    SeccaoId = u.SeccaoId,
                    Seccao = u.Seccao != null
                        ? u.Seccao.Nome
                        : null,
                    UnidadePaiId = u.UnidadePaiId,
                    UnidadePai = u.UnidadePai != null
                        ? u.UnidadePai.Nome
                        : null,
                    Descricao = u.Descricao,
                    Ativo = u.Ativo,
                    DataCadastro = u.DataCadastro
                })
                .FirstAsync();

            return CreatedAtAction(
                nameof(GetUnidadeOperacional),
                new { id = novaUnidade.Id },
                criada);
        }

        // =========================================================
        // PUT: api/UnidadesOperacionais/5
        // Edita uma unidade operacional
        // =========================================================
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EditarUnidade(
            int id,
            UnidadeOperacional dados)
        {
            var unidade = await _context.UnidadesOperacionais
                .FirstOrDefaultAsync(u => u.Id == id);

            if (unidade == null)
            {
                return NotFound(new
                {
                    mensagem = "Unidade operacional não encontrada."
                });
            }

            if (string.IsNullOrWhiteSpace(dados.Nome))
            {
                return BadRequest(new
                {
                    mensagem = "O nome da unidade operacional é obrigatório."
                });
            }

            var seccaoExiste = await _context.Seccoes
                .AnyAsync(s => s.Id == dados.SeccaoId);

            if (!seccaoExiste)
            {
                return BadRequest(new
                {
                    mensagem = "A secção indicada não existe."
                });
            }

            if (dados.UnidadePaiId.HasValue)
            {
                if (dados.UnidadePaiId.Value == id)
                {
                    return BadRequest(new
                    {
                        mensagem = "Uma unidade não pode ser pai de si própria."
                    });
                }

                var pai = await _context.UnidadesOperacionais
                    .FirstOrDefaultAsync(u =>
                        u.Id == dados.UnidadePaiId.Value);

                if (pai == null)
                {
                    return BadRequest(new
                    {
                        mensagem = "A unidade operacional pai indicada não existe."
                    });
                }

                if (pai.SeccaoId != dados.SeccaoId)
                {
                    return BadRequest(new
                    {
                        mensagem = "A unidade pai deve pertencer à mesma secção."
                    });
                }
            }

            var nome = dados.Nome.Trim();

            var duplicada = await _context.UnidadesOperacionais
                .AnyAsync(u =>
                    u.Id != id &&
                    u.SeccaoId == dados.SeccaoId &&
                    u.UnidadePaiId == dados.UnidadePaiId &&
                    u.Nome.ToLower() == nome.ToLower());

            if (duplicada)
            {
                return Conflict(new
                {
                    mensagem = "Já existe outra unidade com este nome nesta estrutura."
                });
            }

            unidade.Nome = nome;
            unidade.Tipo = dados.Tipo;
            unidade.SeccaoId = dados.SeccaoId;
            unidade.UnidadePaiId = dados.UnidadePaiId;
            unidade.Descricao = dados.Descricao?.Trim();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Unidade operacional actualizada com sucesso.",
                unidade = new
                {
                    unidade.Id,
                    unidade.Nome,
                    unidade.Tipo,
                    unidade.SeccaoId,
                    unidade.UnidadePaiId,
                    unidade.Descricao,
                    unidade.Ativo,
                    unidade.DataCadastro
                }
            });
        }

        // =========================================================
        // PATCH: api/UnidadesOperacionais/5/estado
        // Activa/desactiva
        // =========================================================
        [HttpPatch("{id:int}/estado")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> AlterarEstado(
            int id,
            [FromBody] bool ativo)
        {
            var unidade = await _context.UnidadesOperacionais
                .FirstOrDefaultAsync(u => u.Id == id);

            if (unidade == null)
            {
                return NotFound(new
                {
                    mensagem = "Unidade operacional não encontrada."
                });
            }

            unidade.Ativo = ativo;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = ativo
                    ? "Unidade operacional activada com sucesso."
                    : "Unidade operacional desactivada com sucesso.",
                unidade = new
                {
                    unidade.Id,
                    unidade.Nome,
                    unidade.Tipo,
                    unidade.SeccaoId,
                    unidade.UnidadePaiId,
                    unidade.Ativo
                }
            });
        }

        // =========================================================
        // DELETE: api/UnidadesOperacionais/5
        // Desactivação lógica
        // =========================================================
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EliminarUnidade(int id)
        {
            var unidade = await _context.UnidadesOperacionais
                .FirstOrDefaultAsync(u => u.Id == id);

            if (unidade == null)
            {
                return NotFound(new
                {
                    mensagem = "Unidade operacional não encontrada."
                });
            }

            unidade.Ativo = false;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Unidade operacional desactivada com sucesso."
            });
        }
    }
}