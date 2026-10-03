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
        // CONSTANTES
        // =========================================================

        private const int SECCAO_PROTECCAO_OBJECTOS = 7;

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

            // -----------------------------------------------------
            // POSTO ASSOCIADO
            // Usado pelas SeccoesInternas
            // -----------------------------------------------------

            public int? PostoId { get; set; }

            public string? Posto { get; set; }

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

            // -----------------------------------------------------
            // POSTO ASSOCIADO
            // -----------------------------------------------------

            public int? PostoId { get; set; }

            public string? Posto { get; set; }

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

            public int? PostoId { get; set; }

            public string? Posto { get; set; }
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
                .Include(u => u.Posto)
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

                    PostoId = u.PostoId,

                    Posto = u.Posto != null
                        ? u.Posto.Nome
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
                .Include(u => u.Posto)
                .Include(u => u.UnidadesFilhas)
                    .ThenInclude(f => f.Posto)
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

                PostoId = unidade.PostoId,

                Posto = unidade.Posto?.Nome,

                Descricao = unidade.Descricao,

                Ativo = unidade.Ativo,

                DataCadastro = unidade.DataCadastro,

                UnidadesFilhas = unidade.UnidadesFilhas
                    .Select(f => new UnidadeFilhaDto
                    {
                        Id = f.Id,

                        Nome = f.Nome,

                        Tipo = f.Tipo,

                        Ativo = f.Ativo,

                        PostoId = f.PostoId,

                        Posto = f.Posto != null
                            ? f.Posto.Nome
                            : null
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
                .Include(u => u.Posto)
                .Where(u => u.SeccaoId == seccaoId)
                .OrderBy(u => u.Tipo)
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

                    PostoId = u.PostoId,

                    Posto = u.Posto != null
                        ? u.Posto.Nome
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
        // Lista unidades filhas
        // =========================================================

        [HttpGet("pai/{unidadePaiId:int}")]
        public async Task<ActionResult<IEnumerable<UnidadeOperacionalListaDto>>>
            GetFilhas(int unidadePaiId)
        {
            var pai = await _context.UnidadesOperacionais
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == unidadePaiId);

            if (pai == null)
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
                .Include(u => u.Posto)
                .Where(u => u.UnidadePaiId == unidadePaiId)
                .OrderBy(u => u.Tipo)
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

                    PostoId = u.PostoId,

                    Posto = u.Posto != null
                        ? u.Posto.Nome
                        : null,

                    Descricao = u.Descricao,

                    Ativo = u.Ativo,

                    DataCadastro = u.DataCadastro
                })
                .ToListAsync();

            return Ok(unidades);
        }

        // =========================================================
        // VALIDAÇÃO DO POSTO DA SECÇÃO INTERNA
        // =========================================================

        private async Task<string?> ValidarPostoDaSeccaoInternaAsync(
            UnidadeOperacional dados)
        {
            // -----------------------------------------------------
            // Só exige PostoId para Secção Interna.
            // -----------------------------------------------------

            if (dados.Tipo !=
                UnidadeOperacional.TipoUnidadeOperacional.SeccaoInterna)
            {
                return null;
            }

            // -----------------------------------------------------
            // Secção Interna deve possuir Companhia como pai.
            // -----------------------------------------------------

            if (!dados.UnidadePaiId.HasValue)
            {
                return "A Secção Interna deve estar associada a uma Companhia.";
            }

            var companhia = await _context.UnidadesOperacionais
                .AsNoTracking()
                .FirstOrDefaultAsync(u =>
                    u.Id == dados.UnidadePaiId.Value);

            if (companhia == null)
            {
                return "A Companhia indicada para a Secção Interna não existe.";
            }

            if (!companhia.Ativo)
            {
                return "A Companhia indicada está desactivada.";
            }

            if (companhia.SeccaoId != SECCAO_PROTECCAO_OBJECTOS)
            {
                return "A Companhia indicada não pertence à Protecção de Objectos.";
            }

            if (companhia.Tipo !=
                UnidadeOperacional.TipoUnidadeOperacional.Companhia)
            {
                return "A unidade pai de uma Secção Interna deve ser uma Companhia.";
            }

            // -----------------------------------------------------
            // Secção Interna deve possuir Posto.
            // -----------------------------------------------------

            if (!dados.PostoId.HasValue)
            {
                return "A Secção Interna deve estar associada a um Posto.";
            }

            var posto = await _context.Postos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == dados.PostoId.Value);

            if (posto == null)
            {
                return "O Posto indicado não existe.";
            }

            if (!posto.Ativo)
            {
                return "O Posto indicado está desactivado.";
            }

            if (posto.SeccaoId != SECCAO_PROTECCAO_OBJECTOS)
            {
                return "O Posto indicado não pertence à Protecção de Objectos.";
            }

            // -----------------------------------------------------
            // O Posto deve pertencer à mesma Companhia.
            // -----------------------------------------------------

            if (!posto.UnidadeOperacionalId.HasValue)
            {
                return "O Posto indicado não está associado a uma Companhia.";
            }

            if (posto.UnidadeOperacionalId.Value != companhia.Id)
            {
                return "O Posto indicado pertence a uma Companhia diferente.";
            }

            return null;
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

            // -----------------------------------------------------
            // VALIDAR UNIDADE PAI
            // -----------------------------------------------------

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

                if (!pai.Ativo)
                {
                    return BadRequest(new
                    {
                        mensagem = "A unidade operacional pai está desactivada."
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

            // -----------------------------------------------------
            // VALIDAR POSTO DA SECÇÃO INTERNA
            // -----------------------------------------------------

            var erroPosto = await ValidarPostoDaSeccaoInternaAsync(dados);

            if (erroPosto != null)
            {
                return BadRequest(new
                {
                    mensagem = erroPosto
                });
            }

            // -----------------------------------------------------
            // PARA UNIDADES QUE NÃO SÃO SECÇÃO INTERNA,
            // NÃO GUARDAR PostoId.
            // -----------------------------------------------------

            if (dados.Tipo !=
                UnidadeOperacional.TipoUnidadeOperacional.SeccaoInterna)
            {
                dados.PostoId = null;
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
                    mensagem =
                        "Já existe uma unidade com este nome nesta estrutura."
                });
            }

            var novaUnidade = new UnidadeOperacional
            {
                Nome = nome,

                Tipo = dados.Tipo,

                SeccaoId = dados.SeccaoId,

                UnidadePaiId = dados.UnidadePaiId,

                PostoId = dados.PostoId,

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
                .Include(u => u.Posto)
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

                    PostoId = u.PostoId,

                    Posto = u.Posto != null
                        ? u.Posto.Nome
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

            // -----------------------------------------------------
            // VALIDAR PAI
            // -----------------------------------------------------

            if (dados.UnidadePaiId.HasValue)
            {
                if (dados.UnidadePaiId.Value == id)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "Uma unidade não pode ser pai de si própria."
                    });
                }

                var pai = await _context.UnidadesOperacionais
                    .FirstOrDefaultAsync(u =>
                        u.Id == dados.UnidadePaiId.Value);

                if (pai == null)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "A unidade operacional pai indicada não existe."
                    });
                }

                if (!pai.Ativo)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "A unidade operacional pai está desactivada."
                    });
                }

                if (pai.SeccaoId != dados.SeccaoId)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "A unidade pai deve pertencer à mesma secção."
                    });
                }

                // -------------------------------------------------
                // Secção Interna deve ter Companhia como pai.
                // -------------------------------------------------

                if (dados.Tipo ==
                    UnidadeOperacional.TipoUnidadeOperacional.SeccaoInterna &&
                    pai.Tipo !=
                    UnidadeOperacional.TipoUnidadeOperacional.Companhia)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "A Secção Interna deve ter uma Companhia como unidade pai."
                    });
                }
            }

            // -----------------------------------------------------
            // VALIDAR POSTO DA SECÇÃO INTERNA
            // -----------------------------------------------------

            var erroPosto = await ValidarPostoDaSeccaoInternaAsync(dados);

            if (erroPosto != null)
            {
                return BadRequest(new
                {
                    mensagem = erroPosto
                });
            }

            // -----------------------------------------------------
            // UNIDADES QUE NÃO SÃO SECÇÃO INTERNA NÃO POSSUEM POSTO
            // -----------------------------------------------------

            if (dados.Tipo !=
                UnidadeOperacional.TipoUnidadeOperacional.SeccaoInterna)
            {
                dados.PostoId = null;
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
                    mensagem =
                        "Já existe outra unidade com este nome nesta estrutura."
                });
            }

            unidade.Nome = nome;

            unidade.Tipo = dados.Tipo;

            unidade.SeccaoId = dados.SeccaoId;

            unidade.UnidadePaiId = dados.UnidadePaiId;

            unidade.PostoId = dados.PostoId;

            unidade.Descricao = dados.Descricao?.Trim();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem =
                    "Unidade operacional actualizada com sucesso.",

                unidade = new
                {
                    unidade.Id,
                    unidade.Nome,
                    unidade.Tipo,
                    unidade.SeccaoId,
                    unidade.UnidadePaiId,
                    unidade.PostoId,
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
                    mensagem =
                        "Unidade operacional não encontrada."
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
                    unidade.PostoId,
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
                    mensagem =
                        "Unidade operacional não encontrada."
                });
            }

            unidade.Ativo = false;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem =
                    "Unidade operacional desactivada com sucesso."
            });
        }

        [HttpPost("reinicializar-proteccao-objectos")]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<IActionResult> ReinicializarProteccaoObjectos()
        {
            const int SECCAO_PROTECCAO_OBJECTOS = 7;

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // ============================================================
                // 1. CONFIRMAR A SECÇÃO
                // ============================================================

                var seccao = await _context.Seccoes
                    .FirstOrDefaultAsync(s => s.Id == SECCAO_PROTECCAO_OBJECTOS);

                if (seccao == null)
                {
                    return NotFound(new
                    {
                        mensagem = "A Secção Protecção de Objectos (ID 7) não existe."
                    });
                }

                // ============================================================
                // 2. IDENTIFICAR TODAS AS UNIDADES DA PROTECÇÃO DE OBJECTOS
                // ============================================================

                var unidadesPO = await _context.UnidadesOperacionais
                    .Where(u => u.SeccaoId == SECCAO_PROTECCAO_OBJECTOS)
                    .ToListAsync();

                var unidadesIds = unidadesPO
                    .Select(u => u.Id)
                    .ToList();

                // ============================================================
                // 3. APAGAR ESCALAS DA PROTECÇÃO DE OBJECTOS
                // ============================================================

                var escalasPO = await _context.Escalas
                    .Where(e =>
                        e.SeccaoId == SECCAO_PROTECCAO_OBJECTOS ||
                        (
                            e.UnidadeOperacionalId.HasValue &&
                            unidadesIds.Contains(e.UnidadeOperacionalId.Value)
                        ))
                    .ToListAsync();

                if (escalasPO.Count > 0)
                {
                    _context.Escalas.RemoveRange(escalasPO);
                    await _context.SaveChangesAsync();
                }

                // ============================================================
                // 4. APAGAR LOTAÇÕES DA PROTECÇÃO DE OBJECTOS
                // ============================================================

                var lotacoesPO = await _context.LotacoesFuncionarios
                    .Where(l =>
                        l.SeccaoId == SECCAO_PROTECCAO_OBJECTOS ||
                        (
                            l.UnidadeOperacionalId.HasValue &&
                            unidadesIds.Contains(l.UnidadeOperacionalId.Value)
                        ))
                    .ToListAsync();

                if (lotacoesPO.Count > 0)
                {
                    _context.LotacoesFuncionarios.RemoveRange(lotacoesPO);
                    await _context.SaveChangesAsync();
                }

                // ============================================================
                // 5. APAGAR GRUPOS DE ESCALA EVENTUALMENTE ASSOCIADOS À PO
                // ============================================================
                //
                // GrupoEscala deve continuar a ser utilizado apenas pela
                // Segurança Pessoal / Escolta.
                //
                // Portanto, qualquer grupo associado às unidades da PO
                // é considerado dado de teste e será removido.
                // ============================================================

                if (unidadesIds.Count > 0)
                {
                    var gruposPO = await _context.GruposEscala
                        .Where(g =>
                            unidadesIds.Contains(g.UnidadeOperacionalId))
                        .ToListAsync();

                    if (gruposPO.Count > 0)
                    {
                        _context.GruposEscala.RemoveRange(gruposPO);
                        await _context.SaveChangesAsync();
                    }
                }

                // ============================================================
                // 6. APAGAR UNIDADES OPERACIONAIS INTERNAS
                // ============================================================
                //
                // SeccaoInterna possui:
                // UnidadePaiId
                // PostoId
                //
                // Por isso devem ser apagadas antes dos Pelotões/Companhias
                // e antes dos Postos.
                // ============================================================

                var unidadesInternas = unidadesPO
                    .Where(u =>
                        u.Tipo ==
                        UnidadeOperacional.TipoUnidadeOperacional.SeccaoInterna)
                    .ToList();

                if (unidadesInternas.Count > 0)
                {
                    _context.UnidadesOperacionais.RemoveRange(unidadesInternas);
                    await _context.SaveChangesAsync();
                }

                // ============================================================
                // 7. APAGAR POSTOS DA PROTECÇÃO DE OBJECTOS
                // ============================================================

                var postosPO = await _context.Postos
                    .Where(p => p.SeccaoId == SECCAO_PROTECCAO_OBJECTOS)
                    .ToListAsync();

                if (postosPO.Count > 0)
                {
                    _context.Postos.RemoveRange(postosPO);
                    await _context.SaveChangesAsync();
                }

                // ============================================================
                // 8. APAGAR PELOTÕES
                // ============================================================

                var pelotoesPO = unidadesPO
                    .Where(u =>
                        u.Tipo ==
                        UnidadeOperacional.TipoUnidadeOperacional.Pelotao)
                    .ToList();

                if (pelotoesPO.Count > 0)
                {
                    _context.UnidadesOperacionais.RemoveRange(pelotoesPO);
                    await _context.SaveChangesAsync();
                }

                // ============================================================
                // 9. APAGAR COMPANHIAS
                // ============================================================

                var companhiasPO = unidadesPO
                    .Where(u =>
                        u.Tipo ==
                        UnidadeOperacional.TipoUnidadeOperacional.Companhia)
                    .ToList();

                if (companhiasPO.Count > 0)
                {
                    _context.UnidadesOperacionais.RemoveRange(companhiasPO);
                    await _context.SaveChangesAsync();
                }

                // ============================================================
                // 10. RECRIAR AS COMPANHIAS
                // ============================================================

                var companhia1 = new UnidadeOperacional
                {
                    Nome = "1.ª Companhia",
                    Tipo = UnidadeOperacional.TipoUnidadeOperacional.Companhia,
                    SeccaoId = SECCAO_PROTECCAO_OBJECTOS,
                    UnidadePaiId = null,
                    PostoId = null,
                    Descricao = "1.ª Companhia de Protecção de Objectos.",
                    Ativo = true,
                    DataCadastro = DateTime.Now
                };

                var companhia2 = new UnidadeOperacional
                {
                    Nome = "2.ª Companhia",
                    Tipo = UnidadeOperacional.TipoUnidadeOperacional.Companhia,
                    SeccaoId = SECCAO_PROTECCAO_OBJECTOS,
                    UnidadePaiId = null,
                    PostoId = null,
                    Descricao = "2.ª Companhia de Protecção de Objectos.",
                    Ativo = true,
                    DataCadastro = DateTime.Now
                };

                var companhia3 = new UnidadeOperacional
                {
                    Nome = "3.ª Companhia",
                    Tipo = UnidadeOperacional.TipoUnidadeOperacional.Companhia,
                    SeccaoId = SECCAO_PROTECCAO_OBJECTOS,
                    UnidadePaiId = null,
                    PostoId = null,
                    Descricao = "3.ª Companhia de Protecção de Objectos.",
                    Ativo = true,
                    DataCadastro = DateTime.Now
                };

                _context.UnidadesOperacionais.AddRange(
                    companhia1,
                    companhia2,
                    companhia3);

                await _context.SaveChangesAsync();

                // ============================================================
                // 11. CRIAR OS 3 PELOTÕES DA 1.ª COMPANHIA
                // ============================================================

                var pelotoesCompanhia1 = new[]
                {
            new UnidadeOperacional
            {
                Nome = "1.º Pelotão",
                Tipo = UnidadeOperacional.TipoUnidadeOperacional.Pelotao,
                SeccaoId = SECCAO_PROTECCAO_OBJECTOS,
                UnidadePaiId = companhia1.Id,
                PostoId = null,
                Descricao = "1.º Pelotão da 1.ª Companhia.",
                Ativo = true,
                DataCadastro = DateTime.Now
            },
            new UnidadeOperacional
            {
                Nome = "2.º Pelotão",
                Tipo = UnidadeOperacional.TipoUnidadeOperacional.Pelotao,
                SeccaoId = SECCAO_PROTECCAO_OBJECTOS,
                UnidadePaiId = companhia1.Id,
                PostoId = null,
                Descricao = "2.º Pelotão da 1.ª Companhia.",
                Ativo = true,
                DataCadastro = DateTime.Now
            },
            new UnidadeOperacional
            {
                Nome = "3.º Pelotão",
                Tipo = UnidadeOperacional.TipoUnidadeOperacional.Pelotao,
                SeccaoId = SECCAO_PROTECCAO_OBJECTOS,
                UnidadePaiId = companhia1.Id,
                PostoId = null,
                Descricao = "3.º Pelotão da 1.ª Companhia.",
                Ativo = true,
                DataCadastro = DateTime.Now
            }
        };

                _context.UnidadesOperacionais.AddRange(pelotoesCompanhia1);

                // ============================================================
                // 12. CRIAR OS 3 PELOTÕES DA 2.ª COMPANHIA
                // ============================================================

                var pelotoesCompanhia2 = new[]
                {
            new UnidadeOperacional
            {
                Nome = "1.º Pelotão",
                Tipo = UnidadeOperacional.TipoUnidadeOperacional.Pelotao,
                SeccaoId = SECCAO_PROTECCAO_OBJECTOS,
                UnidadePaiId = companhia2.Id,
                PostoId = null,
                Descricao = "1.º Pelotão da 2.ª Companhia.",
                Ativo = true,
                DataCadastro = DateTime.Now
            },
            new UnidadeOperacional
            {
                Nome = "2.º Pelotão",
                Tipo = UnidadeOperacional.TipoUnidadeOperacional.Pelotao,
                SeccaoId = SECCAO_PROTECCAO_OBJECTOS,
                UnidadePaiId = companhia2.Id,
                PostoId = null,
                Descricao = "2.º Pelotão da 2.ª Companhia.",
                Ativo = true,
                DataCadastro = DateTime.Now
            },
            new UnidadeOperacional
            {
                Nome = "3.º Pelotão",
                Tipo = UnidadeOperacional.TipoUnidadeOperacional.Pelotao,
                SeccaoId = SECCAO_PROTECCAO_OBJECTOS,
                UnidadePaiId = companhia2.Id,
                PostoId = null,
                Descricao = "3.º Pelotão da 2.ª Companhia.",
                Ativo = true,
                DataCadastro = DateTime.Now
            }
        };

                _context.UnidadesOperacionais.AddRange(pelotoesCompanhia2);

                // ============================================================
                // 13. CRIAR OS 3 PELOTÕES DA 3.ª COMPANHIA
                // ============================================================

                var pelotoesCompanhia3 = new[]
                {
            new UnidadeOperacional
            {
                Nome = "1.º Pelotão",
                Tipo = UnidadeOperacional.TipoUnidadeOperacional.Pelotao,
                SeccaoId = SECCAO_PROTECCAO_OBJECTOS,
                UnidadePaiId = companhia3.Id,
                PostoId = null,
                Descricao = "1.º Pelotão da 3.ª Companhia.",
                Ativo = true,
                DataCadastro = DateTime.Now
            },
            new UnidadeOperacional
            {
                Nome = "2.º Pelotão",
                Tipo = UnidadeOperacional.TipoUnidadeOperacional.Pelotao,
                SeccaoId = SECCAO_PROTECCAO_OBJECTOS,
                UnidadePaiId = companhia3.Id,
                PostoId = null,
                Descricao = "2.º Pelotão da 3.ª Companhia.",
                Ativo = true,
                DataCadastro = DateTime.Now
            },
            new UnidadeOperacional
            {
                Nome = "3.º Pelotão",
                Tipo = UnidadeOperacional.TipoUnidadeOperacional.Pelotao,
                SeccaoId = SECCAO_PROTECCAO_OBJECTOS,
                UnidadePaiId = companhia3.Id,
                PostoId = null,
                Descricao = "3.º Pelotão da 3.ª Companhia.",
                Ativo = true,
                DataCadastro = DateTime.Now
            }
        };

                _context.UnidadesOperacionais.AddRange(pelotoesCompanhia3);

                await _context.SaveChangesAsync();

                // ============================================================
                // 14. CRIAR O POSTO DA 3.ª COMPANHIA
                // ============================================================

                var postoEscola = new Posto
                {
                    Codigo = "PO-001",
                    Nome = "Escola Central do Partido no Poder",
                    Localizacao = "Matola-C",
                    SeccaoId = SECCAO_PROTECCAO_OBJECTOS,
                    SectorId = null,
                    UnidadeOperacionalId = companhia3.Id,
                    Descricao = "Posto da 3.ª Companhia de Protecção de Objectos.",
                    Ativo = true,
                    DataCadastro = DateTime.Now
                };

                _context.Postos.Add(postoEscola);

                await _context.SaveChangesAsync();

                // ============================================================
                // 15. CRIAR AS 3 SECÇÕES INTERNAS DA 3.ª COMPANHIA
                // ============================================================

                var seccoesInternas = new[]
                {
            new UnidadeOperacional
            {
                Nome = "1.ª Secção",
                Tipo = UnidadeOperacional.TipoUnidadeOperacional.SeccaoInterna,
                SeccaoId = SECCAO_PROTECCAO_OBJECTOS,
                UnidadePaiId = companhia3.Id,
                PostoId = postoEscola.Id,
                Descricao =
                    "1.ª Secção da 3.ª Companhia, " +
                    "responsável pelo serviço no Posto Escola Central.",
                Ativo = true,
                DataCadastro = DateTime.Now
            },
            new UnidadeOperacional
            {
                Nome = "2.ª Secção",
                Tipo = UnidadeOperacional.TipoUnidadeOperacional.SeccaoInterna,
                SeccaoId = SECCAO_PROTECCAO_OBJECTOS,
                UnidadePaiId = companhia3.Id,
                PostoId = postoEscola.Id,
                Descricao =
                    "2.ª Secção da 3.ª Companhia, " +
                    "responsável pelo serviço no Posto Escola Central.",
                Ativo = true,
                DataCadastro = DateTime.Now
            },
            new UnidadeOperacional
            {
                Nome = "3.ª Secção",
                Tipo = UnidadeOperacional.TipoUnidadeOperacional.SeccaoInterna,
                SeccaoId = SECCAO_PROTECCAO_OBJECTOS,
                UnidadePaiId = companhia3.Id,
                PostoId = postoEscola.Id,
                Descricao =
                    "3.ª Secção da 3.ª Companhia, " +
                    "responsável pelo serviço no Posto Escola Central.",
                Ativo = true,
                DataCadastro = DateTime.Now
            }
        };

                _context.UnidadesOperacionais.AddRange(seccoesInternas);

                await _context.SaveChangesAsync();

                // ============================================================
                // 16. CONFIRMAR TRANSAÇÃO
                // ============================================================

                await transaction.CommitAsync();

                return Ok(new
                {
                    sucesso = true,
                    mensagem =
                        "A Protecção de Objectos foi reinicializada com sucesso.",
                    seccaoId = SECCAO_PROTECCAO_OBJECTOS,

                    companhias = new[]
                    {
                new
                {
                    id = companhia1.Id,
                    nome = companhia1.Nome
                },
                new
                {
                    id = companhia2.Id,
                    nome = companhia2.Nome
                },
                new
                {
                    id = companhia3.Id,
                    nome = companhia3.Nome
                }
            },

                    estrutura = new
                    {
                        pelotoesPorCompanhia = 3,
                        totalPelotoes = 9,
                        postoTerceiraCompanhia = new
                        {
                            id = postoEscola.Id,
                            codigo = postoEscola.Codigo,
                            nome = postoEscola.Nome
                        },
                        seccoesInternasTerceiraCompanhia = 3
                    }
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return StatusCode(500, new
                {
                    sucesso = false,
                    mensagem =
                        "Ocorreu um erro ao reinicializar a Protecção de Objectos.",
                    erro = ex.Message,
                    innerException = ex.InnerException?.Message
                });
            }
        }
    }


}
