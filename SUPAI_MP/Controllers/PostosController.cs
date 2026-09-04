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
    public class PostosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PostosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // DTOs
        // ============================================================

        public class PostoListaDto
        {
            public int Id { get; set; }
            public string Codigo { get; set; } = string.Empty;
            public string Nome { get; set; } = string.Empty;
            public string? Localizacao { get; set; }

            public int SeccaoId { get; set; }
            public string? Seccao { get; set; }

            public int? SectorId { get; set; }
            public string? Sector { get; set; }

            public int? UnidadeOperacionalId { get; set; }
            public string? UnidadeOperacional { get; set; }

            public string? Descricao { get; set; }
            public bool Ativo { get; set; }
            public DateTime DataCadastro { get; set; }
        }

        public class PostoDetalheDto
        {
            public int Id { get; set; }
            public string Codigo { get; set; } = string.Empty;
            public string Nome { get; set; } = string.Empty;
            public string? Localizacao { get; set; }

            public int SeccaoId { get; set; }
            public string? Seccao { get; set; }

            public int? SectorId { get; set; }
            public string? Sector { get; set; }

            public int? UnidadeOperacionalId { get; set; }
            public string? UnidadeOperacional { get; set; }

            public string? Descricao { get; set; }
            public bool Ativo { get; set; }
            public DateTime DataCadastro { get; set; }

            public List<PostoLotacaoDto> Lotacoes { get; set; }
                = new List<PostoLotacaoDto>();
        }

        // IMPORTANTE:
        // O nome foi alterado de LotacaoDto para PostoLotacaoDto
        // para evitar conflito com outro DTO existente no projecto.
        public class PostoLotacaoDto
        {
            public int Id { get; set; }

            public int FuncionarioId { get; set; }
            public string? Funcionario { get; set; }

            public int FuncaoOperacionalId { get; set; }
            public string? FuncaoOperacional { get; set; }

            public int? TipoTurnoId { get; set; }
            public string? TipoTurno { get; set; }

            public DateTime DataInicio { get; set; }
            public DateTime? DataFim { get; set; }

            public bool Ativo { get; set; }
        }

        // ============================================================
        // GET: api/Postos
        // ============================================================

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostoListaDto>>> GetPostos()
        {
            var postos = await _context.Postos
                .AsNoTracking()
                .Select(p => new PostoListaDto
                {
                    Id = p.Id,
                    Codigo = p.Codigo,
                    Nome = p.Nome,
                    Localizacao = p.Localizacao,

                    SeccaoId = p.SeccaoId,
                    Seccao = p.Seccao != null
                        ? p.Seccao.Nome
                        : null,

                    SectorId = p.SectorId,
                    Sector = p.Sector != null
                        ? p.Sector.Nome
                        : null,

                    UnidadeOperacionalId = p.UnidadeOperacionalId,
                    UnidadeOperacional = p.UnidadeOperacional != null
                        ? p.UnidadeOperacional.Nome
                        : null,

                    Descricao = p.Descricao,
                    Ativo = p.Ativo,
                    DataCadastro = p.DataCadastro
                })
                .OrderBy(p => p.Seccao)
                .ThenBy(p => p.Nome)
                .ToListAsync();

            return Ok(postos);
        }

        // ============================================================
        // GET: api/Postos/5
        // ============================================================

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PostoDetalheDto>> GetPosto(int id)
        {
            var posto = await _context.Postos
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new PostoDetalheDto
                {
                    Id = p.Id,
                    Codigo = p.Codigo,
                    Nome = p.Nome,
                    Localizacao = p.Localizacao,

                    SeccaoId = p.SeccaoId,
                    Seccao = p.Seccao != null
                        ? p.Seccao.Nome
                        : null,

                    SectorId = p.SectorId,
                    Sector = p.Sector != null
                        ? p.Sector.Nome
                        : null,

                    UnidadeOperacionalId = p.UnidadeOperacionalId,
                    UnidadeOperacional = p.UnidadeOperacional != null
                        ? p.UnidadeOperacional.Nome
                        : null,

                    Descricao = p.Descricao,
                    Ativo = p.Ativo,
                    DataCadastro = p.DataCadastro,

                    Lotacoes = p.Lotacoes
                        .Select(l => new PostoLotacaoDto
                        {
                            Id = l.Id,

                            FuncionarioId = l.FuncionarioId,

                            Funcionario = l.Funcionario != null
                                ? l.Funcionario.NomeCompleto
                                : null,

                            FuncaoOperacionalId = l.FuncaoOperacionalId,

                            FuncaoOperacional = l.FuncaoOperacional != null
                                ? l.FuncaoOperacional.Nome
                                : null,

                            TipoTurnoId = l.TipoTurnoId,

                            TipoTurno = l.TipoTurno != null
                                ? l.TipoTurno.Nome
                                : null,

                            DataInicio = l.DataInicio,
                            DataFim = l.DataFim,
                            Ativo = l.Ativo
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (posto == null)
            {
                return NotFound(new
                {
                    mensagem = "Posto não encontrado."
                });
            }

            return Ok(posto);
        }

        // ============================================================
        // GET: api/Postos/seccao/5
        // ============================================================

        [HttpGet("seccao/{seccaoId:int}")]
        public async Task<ActionResult<IEnumerable<PostoListaDto>>> GetPorSeccao(
            int seccaoId)
        {
            var existe = await _context.Seccoes
                .AnyAsync(s => s.Id == seccaoId);

            if (!existe)
            {
                return NotFound(new
                {
                    mensagem = "Secção não encontrada."
                });
            }

            var postos = await _context.Postos
                .AsNoTracking()
                .Where(p => p.SeccaoId == seccaoId)
                .Select(p => new PostoListaDto
                {
                    Id = p.Id,
                    Codigo = p.Codigo,
                    Nome = p.Nome,
                    Localizacao = p.Localizacao,

                    SeccaoId = p.SeccaoId,
                    Seccao = p.Seccao != null
                        ? p.Seccao.Nome
                        : null,

                    SectorId = p.SectorId,
                    Sector = p.Sector != null
                        ? p.Sector.Nome
                        : null,

                    UnidadeOperacionalId = p.UnidadeOperacionalId,
                    UnidadeOperacional = p.UnidadeOperacional != null
                        ? p.UnidadeOperacional.Nome
                        : null,

                    Descricao = p.Descricao,
                    Ativo = p.Ativo,
                    DataCadastro = p.DataCadastro
                })
                .OrderBy(p => p.Nome)
                .ToListAsync();

            return Ok(postos);
        }

        // ============================================================
        // GET: api/Postos/sector/5
        // ============================================================

        [HttpGet("sector/{sectorId:int}")]
        public async Task<ActionResult<IEnumerable<PostoListaDto>>> GetPorSector(
            int sectorId)
        {
            var sector = await _context.Sectores
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == sectorId);

            if (sector == null)
            {
                return NotFound(new
                {
                    mensagem = "Sector não encontrado."
                });
            }

            var postos = await _context.Postos
                .AsNoTracking()
                .Where(p => p.SectorId == sectorId)
                .Select(p => new PostoListaDto
                {
                    Id = p.Id,
                    Codigo = p.Codigo,
                    Nome = p.Nome,
                    Localizacao = p.Localizacao,

                    SeccaoId = p.SeccaoId,
                    Seccao = p.Seccao != null
                        ? p.Seccao.Nome
                        : null,

                    SectorId = p.SectorId,
                    Sector = p.Sector != null
                        ? p.Sector.Nome
                        : null,

                    UnidadeOperacionalId = p.UnidadeOperacionalId,
                    UnidadeOperacional = p.UnidadeOperacional != null
                        ? p.UnidadeOperacional.Nome
                        : null,

                    Descricao = p.Descricao,
                    Ativo = p.Ativo,
                    DataCadastro = p.DataCadastro
                })
                .OrderBy(p => p.Nome)
                .ToListAsync();

            return Ok(postos);
        }

        // ============================================================
        // GET: api/Postos/unidade/5
        // ============================================================

        [HttpGet("unidade/{unidadeOperacionalId:int}")]
        public async Task<ActionResult<IEnumerable<PostoListaDto>>> GetPorUnidade(
            int unidadeOperacionalId)
        {
            var existe = await _context.UnidadesOperacionais
                .AnyAsync(u => u.Id == unidadeOperacionalId);

            if (!existe)
            {
                return NotFound(new
                {
                    mensagem = "Unidade operacional não encontrada."
                });
            }

            var postos = await _context.Postos
                .AsNoTracking()
                .Where(p =>
                    p.UnidadeOperacionalId == unidadeOperacionalId)
                .Select(p => new PostoListaDto
                {
                    Id = p.Id,
                    Codigo = p.Codigo,
                    Nome = p.Nome,
                    Localizacao = p.Localizacao,

                    SeccaoId = p.SeccaoId,
                    Seccao = p.Seccao != null
                        ? p.Seccao.Nome
                        : null,

                    SectorId = p.SectorId,
                    Sector = p.Sector != null
                        ? p.Sector.Nome
                        : null,

                    UnidadeOperacionalId = p.UnidadeOperacionalId,
                    UnidadeOperacional = p.UnidadeOperacional != null
                        ? p.UnidadeOperacional.Nome
                        : null,

                    Descricao = p.Descricao,
                    Ativo = p.Ativo,
                    DataCadastro = p.DataCadastro
                })
                .OrderBy(p => p.Nome)
                .ToListAsync();

            return Ok(postos);
        }

        // ============================================================
        // POST: api/Postos
        // ============================================================

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<PostoListaDto>> CriarPosto(
            [FromBody] Posto dados)
        {
            if (dados == null)
            {
                return BadRequest(new
                {
                    mensagem = "Os dados do posto são obrigatórios."
                });
            }

            if (string.IsNullOrWhiteSpace(dados.Codigo))
            {
                return BadRequest(new
                {
                    mensagem = "O código do posto é obrigatório."
                });
            }

            if (string.IsNullOrWhiteSpace(dados.Nome))
            {
                return BadRequest(new
                {
                    mensagem = "O nome do posto é obrigatório."
                });
            }

            var seccao = await _context.Seccoes
                .FirstOrDefaultAsync(s => s.Id == dados.SeccaoId);

            if (seccao == null)
            {
                return BadRequest(new
                {
                    mensagem = "A secção indicada não existe."
                });
            }

            if (dados.SectorId.HasValue)
            {
                var sector = await _context.Sectores
                    .FirstOrDefaultAsync(s =>
                        s.Id == dados.SectorId.Value);

                if (sector == null)
                {
                    return BadRequest(new
                    {
                        mensagem = "O sector indicado não existe."
                    });
                }

                if (sector.SeccaoId != dados.SeccaoId)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "O sector deve pertencer à mesma secção do posto."
                    });
                }
            }

            if (dados.UnidadeOperacionalId.HasValue)
            {
                var unidade = await _context.UnidadesOperacionais
                    .FirstOrDefaultAsync(u =>
                        u.Id == dados.UnidadeOperacionalId.Value);

                if (unidade == null)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "A unidade operacional indicada não existe."
                    });
                }

                if (unidade.SeccaoId != dados.SeccaoId)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "A unidade operacional deve pertencer à mesma secção do posto."
                    });
                }
            }

            var codigo = dados.Codigo.Trim();

            var codigoExiste = await _context.Postos
                .AnyAsync(p =>
                    p.Codigo.ToLower() == codigo.ToLower());

            if (codigoExiste)
            {
                return Conflict(new
                {
                    mensagem =
                        "Já existe um posto com este código."
                });
            }

            var nome = dados.Nome.Trim();

            var nomeExiste = await _context.Postos
                .AnyAsync(p =>
                    p.SeccaoId == dados.SeccaoId &&
                    p.Nome.ToLower() == nome.ToLower());

            if (nomeExiste)
            {
                return Conflict(new
                {
                    mensagem =
                        "Já existe um posto com este nome nesta secção."
                });
            }

            var novoPosto = new Posto
            {
                Codigo = codigo,
                Nome = nome,
                Localizacao = dados.Localizacao?.Trim(),
                Descricao = dados.Descricao?.Trim(),
                SeccaoId = dados.SeccaoId,
                SectorId = dados.SectorId,
                UnidadeOperacionalId = dados.UnidadeOperacionalId,
                Ativo = true,
                DataCadastro = DateTime.Now
            };

            _context.Postos.Add(novoPosto);

            await _context.SaveChangesAsync();

            var resultado = await _context.Postos
                .AsNoTracking()
                .Where(p => p.Id == novoPosto.Id)
                .Select(p => new PostoListaDto
                {
                    Id = p.Id,
                    Codigo = p.Codigo,
                    Nome = p.Nome,
                    Localizacao = p.Localizacao,

                    SeccaoId = p.SeccaoId,
                    Seccao = p.Seccao != null
                        ? p.Seccao.Nome
                        : null,

                    SectorId = p.SectorId,
                    Sector = p.Sector != null
                        ? p.Sector.Nome
                        : null,

                    UnidadeOperacionalId = p.UnidadeOperacionalId,
                    UnidadeOperacional = p.UnidadeOperacional != null
                        ? p.UnidadeOperacional.Nome
                        : null,

                    Descricao = p.Descricao,
                    Ativo = p.Ativo,
                    DataCadastro = p.DataCadastro
                })
                .FirstAsync();

            return CreatedAtAction(
                nameof(GetPosto),
                new { id = novoPosto.Id },
                resultado);
        }

        // ============================================================
        // PUT: api/Postos/5
        // ============================================================

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EditarPosto(
            int id,
            [FromBody] Posto dados)
        {
            if (dados == null)
            {
                return BadRequest(new
                {
                    mensagem = "Os dados do posto são obrigatórios."
                });
            }

            var posto = await _context.Postos
                .FirstOrDefaultAsync(p => p.Id == id);

            if (posto == null)
            {
                return NotFound(new
                {
                    mensagem = "Posto não encontrado."
                });
            }

            if (string.IsNullOrWhiteSpace(dados.Codigo))
            {
                return BadRequest(new
                {
                    mensagem = "O código do posto é obrigatório."
                });
            }

            if (string.IsNullOrWhiteSpace(dados.Nome))
            {
                return BadRequest(new
                {
                    mensagem = "O nome do posto é obrigatório."
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

            if (dados.SectorId.HasValue)
            {
                var sector = await _context.Sectores
                    .FirstOrDefaultAsync(s =>
                        s.Id == dados.SectorId.Value);

                if (sector == null)
                {
                    return BadRequest(new
                    {
                        mensagem = "O sector indicado não existe."
                    });
                }

                if (sector.SeccaoId != dados.SeccaoId)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "O sector deve pertencer à mesma secção do posto."
                    });
                }
            }

            if (dados.UnidadeOperacionalId.HasValue)
            {
                var unidade = await _context.UnidadesOperacionais
                    .FirstOrDefaultAsync(u =>
                        u.Id == dados.UnidadeOperacionalId.Value);

                if (unidade == null)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "A unidade operacional indicada não existe."
                    });
                }

                if (unidade.SeccaoId != dados.SeccaoId)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "A unidade operacional deve pertencer à mesma secção do posto."
                    });
                }
            }

            var codigo = dados.Codigo.Trim();

            var codigoDuplicado = await _context.Postos
                .AnyAsync(p =>
                    p.Id != id &&
                    p.Codigo.ToLower() == codigo.ToLower());

            if (codigoDuplicado)
            {
                return Conflict(new
                {
                    mensagem =
                        "Já existe outro posto com este código."
                });
            }

            var nome = dados.Nome.Trim();

            var nomeDuplicado = await _context.Postos
                .AnyAsync(p =>
                    p.Id != id &&
                    p.SeccaoId == dados.SeccaoId &&
                    p.Nome.ToLower() == nome.ToLower());

            if (nomeDuplicado)
            {
                return Conflict(new
                {
                    mensagem =
                        "Já existe outro posto com este nome nesta secção."
                });
            }

            posto.Codigo = codigo;
            posto.Nome = nome;
            posto.Localizacao = dados.Localizacao?.Trim();
            posto.Descricao = dados.Descricao?.Trim();
            posto.SeccaoId = dados.SeccaoId;
            posto.SectorId = dados.SectorId;
            posto.UnidadeOperacionalId = dados.UnidadeOperacionalId;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Posto actualizado com sucesso.",
                id = posto.Id,
                codigo = posto.Codigo,
                nome = posto.Nome,
                ativo = posto.Ativo
            });
        }

        // ============================================================
        // PATCH: api/Postos/5/estado
        // ============================================================

        [HttpPatch("{id:int}/estado")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> AlterarEstado(
            int id,
            [FromBody] bool ativo)
        {
            var posto = await _context.Postos
                .FirstOrDefaultAsync(p => p.Id == id);

            if (posto == null)
            {
                return NotFound(new
                {
                    mensagem = "Posto não encontrado."
                });
            }

            posto.Ativo = ativo;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = ativo
                    ? "Posto activado com sucesso."
                    : "Posto desactivado com sucesso.",

                id = posto.Id,
                codigo = posto.Codigo,
                nome = posto.Nome,
                ativo = posto.Ativo
            });
        }

        // ============================================================
        // DELETE: api/Postos/5
        // ============================================================

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EliminarPosto(int id)
        {
            var posto = await _context.Postos
                .FirstOrDefaultAsync(p => p.Id == id);

            if (posto == null)
            {
                return NotFound(new
                {
                    mensagem = "Posto não encontrado."
                });
            }

            // Eliminação lógica
            posto.Ativo = false;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Posto desactivado com sucesso."
            });
        }
    }
}