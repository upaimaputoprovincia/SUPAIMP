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
    public class EquipasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EquipasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // DTOs
        // ============================================================

        public class EquipaListaDto
        {
            public int Id { get; set; }
            public string Nome { get; set; } = string.Empty;

            public int? SectorId { get; set; }
            public string? Sector { get; set; }

            public int? SeccaoId { get; set; }
            public string? Seccao { get; set; }

            public int? UnidadeOperacionalId { get; set; }
            public string? UnidadeOperacional { get; set; }

            public string? Descricao { get; set; }

            public bool Ativo { get; set; }

            public DateTime DataCadastro { get; set; }
        }

        public class EquipaDetalheDto
        {
            public int Id { get; set; }
            public string Nome { get; set; } = string.Empty;

            public int? SectorId { get; set; }
            public string? Sector { get; set; }

            public int? SeccaoId { get; set; }
            public string? Seccao { get; set; }

            public int? UnidadeOperacionalId { get; set; }
            public string? UnidadeOperacional { get; set; }

            public string? Descricao { get; set; }

            public bool Ativo { get; set; }

            public DateTime DataCadastro { get; set; }

            public List<LotacaoDto> Lotacoes { get; set; }
                = new List<LotacaoDto>();
        }

        public class LotacaoDto
        {
            public int Id { get; set; }

            public int FuncionarioId { get; set; }

            public string? Funcionario { get; set; }

            public int? SeccaoId { get; set; }
            public int? SectorId { get; set; }

            public int? UnidadeOperacionalId { get; set; }
            public int? EquipaId { get; set; }
            public int? PostoId { get; set; }

            public int FuncaoOperacionalId { get; set; }
            public string? FuncaoOperacional { get; set; }

            public int? TipoTurnoId { get; set; }

            public DateTime DataInicio { get; set; }
            public DateTime? DataFim { get; set; }

            public bool Ativo { get; set; }

            public string? Motivo { get; set; }
            public string? Observacao { get; set; }
        }

        // ============================================================
        // GET: api/Equipas
        // LEITURA: qualquer utilizador autenticado
        // ============================================================

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EquipaListaDto>>> GetEquipas()
        {
            var equipas = await _context.Equipas
                .AsNoTracking()
                .Include(e => e.Sector)
                    .ThenInclude(s => s!.Seccao)
                .Include(e => e.UnidadeOperacional)
                .OrderBy(e => e.UnidadeOperacional!.Nome)
                .ThenBy(e => e.Nome)
                .Select(e => new EquipaListaDto
                {
                    Id = e.Id,
                    Nome = e.Nome,

                    SectorId = e.SectorId,

                    Sector = e.Sector != null
                        ? e.Sector.Nome
                        : null,

                    SeccaoId = e.Sector != null
                        ? e.Sector.SeccaoId
                        : null,

                    Seccao = e.Sector != null &&
                             e.Sector.Seccao != null
                        ? e.Sector.Seccao.Nome
                        : null,

                    UnidadeOperacionalId =
                        e.UnidadeOperacionalId,

                    UnidadeOperacional =
                        e.UnidadeOperacional != null
                            ? e.UnidadeOperacional.Nome
                            : null,

                    Descricao = e.Descricao,

                    Ativo = e.Ativo,

                    DataCadastro = e.DataCadastro
                })
                .ToListAsync();

            return Ok(equipas);
        }

        // ============================================================
        // GET: api/Equipas/5
        // LEITURA: qualquer utilizador autenticado
        // ============================================================

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EquipaDetalheDto>> GetEquipa(int id)
        {
            var equipa = await _context.Equipas
                .AsNoTracking()
                .Include(e => e.Sector)
                    .ThenInclude(s => s!.Seccao)
                .Include(e => e.UnidadeOperacional)
                .Include(e => e.Lotacoes)
                    .ThenInclude(l => l.Funcionario)
                .Include(e => e.Lotacoes)
                    .ThenInclude(l => l.FuncaoOperacional)
                .Where(e => e.Id == id)
                .Select(e => new EquipaDetalheDto
                {
                    Id = e.Id,
                    Nome = e.Nome,

                    SectorId = e.SectorId,

                    Sector = e.Sector != null
                        ? e.Sector.Nome
                        : null,

                    SeccaoId = e.Sector != null
                        ? e.Sector.SeccaoId
                        : null,

                    Seccao = e.Sector != null &&
                             e.Sector.Seccao != null
                        ? e.Sector.Seccao.Nome
                        : null,

                    UnidadeOperacionalId =
                        e.UnidadeOperacionalId,

                    UnidadeOperacional =
                        e.UnidadeOperacional != null
                            ? e.UnidadeOperacional.Nome
                            : null,

                    Descricao = e.Descricao,

                    Ativo = e.Ativo,

                    DataCadastro = e.DataCadastro,

                    Lotacoes = e.Lotacoes
                        .Select(l => new LotacaoDto
                        {
                            Id = l.Id,

                            FuncionarioId =
                                l.FuncionarioId,

                            Funcionario =
                                l.Funcionario != null
                                    ? l.Funcionario.NomeCompleto
                                    : null,

                            SeccaoId = l.SeccaoId,

                            SectorId = l.SectorId,

                            UnidadeOperacionalId =
                                l.UnidadeOperacionalId,

                            EquipaId = l.EquipaId,

                            PostoId = l.PostoId,

                            FuncaoOperacionalId =
                                l.FuncaoOperacionalId,

                            FuncaoOperacional =
                                l.FuncaoOperacional != null
                                    ? l.FuncaoOperacional.Nome
                                    : null,

                            TipoTurnoId = l.TipoTurnoId,

                            DataInicio = l.DataInicio,

                            DataFim = l.DataFim,

                            Ativo = l.Ativo,

                            Motivo = l.Motivo,

                            Observacao = l.Observacao
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (equipa == null)
            {
                return NotFound(new
                {
                    mensagem = "Equipa não encontrada."
                });
            }

            return Ok(equipa);
        }

        // ============================================================
        // GET: api/Equipas/unidade/5
        // LEITURA: qualquer utilizador autenticado
        // ============================================================

        [HttpGet("unidade/{unidadeOperacionalId:int}")]
        public async Task<ActionResult<IEnumerable<EquipaListaDto>>> GetPorUnidade(
            int unidadeOperacionalId)
        {
            var unidadeExiste = await _context.UnidadesOperacionais
                .AnyAsync(u => u.Id == unidadeOperacionalId);

            if (!unidadeExiste)
            {
                return NotFound(new
                {
                    mensagem =
                        "Unidade operacional não encontrada."
                });
            }

            var equipas = await _context.Equipas
                .AsNoTracking()
                .Include(e => e.Sector)
                    .ThenInclude(s => s!.Seccao)
                .Include(e => e.UnidadeOperacional)
                .Where(e =>
                    e.UnidadeOperacionalId ==
                    unidadeOperacionalId)
                .OrderBy(e => e.Nome)
                .Select(e => new EquipaListaDto
                {
                    Id = e.Id,
                    Nome = e.Nome,

                    SectorId = e.SectorId,

                    Sector = e.Sector != null
                        ? e.Sector.Nome
                        : null,

                    SeccaoId = e.Sector != null
                        ? e.Sector.SeccaoId
                        : null,

                    Seccao = e.Sector != null &&
                             e.Sector.Seccao != null
                        ? e.Sector.Seccao.Nome
                        : null,

                    UnidadeOperacionalId =
                        e.UnidadeOperacionalId,

                    UnidadeOperacional =
                        e.UnidadeOperacional != null
                            ? e.UnidadeOperacional.Nome
                            : null,

                    Descricao = e.Descricao,

                    Ativo = e.Ativo,

                    DataCadastro = e.DataCadastro
                })
                .ToListAsync();

            return Ok(equipas);
        }

        // ============================================================
        // GET: api/Equipas/sector/5
        // LEITURA: qualquer utilizador autenticado
        // ============================================================

        [HttpGet("sector/{sectorId:int}")]
        public async Task<ActionResult<IEnumerable<EquipaListaDto>>> GetPorSector(
            int sectorId)
        {
            var sectorExiste = await _context.Sectores
                .AnyAsync(s => s.Id == sectorId);

            if (!sectorExiste)
            {
                return NotFound(new
                {
                    mensagem = "Sector não encontrado."
                });
            }

            var equipas = await _context.Equipas
                .AsNoTracking()
                .Include(e => e.Sector)
                    .ThenInclude(s => s!.Seccao)
                .Include(e => e.UnidadeOperacional)
                .Where(e => e.SectorId == sectorId)
                .OrderBy(e => e.Nome)
                .Select(e => new EquipaListaDto
                {
                    Id = e.Id,
                    Nome = e.Nome,

                    SectorId = e.SectorId,

                    Sector = e.Sector != null
                        ? e.Sector.Nome
                        : null,

                    SeccaoId = e.Sector != null
                        ? e.Sector.SeccaoId
                        : null,

                    Seccao = e.Sector != null &&
                             e.Sector.Seccao != null
                        ? e.Sector.Seccao.Nome
                        : null,

                    UnidadeOperacionalId =
                        e.UnidadeOperacionalId,

                    UnidadeOperacional =
                        e.UnidadeOperacional != null
                            ? e.UnidadeOperacional.Nome
                            : null,

                    Descricao = e.Descricao,

                    Ativo = e.Ativo,

                    DataCadastro = e.DataCadastro
                })
                .ToListAsync();

            return Ok(equipas);
        }

        // ============================================================
        // POST: api/Equipas
        // ADMINISTRADOR
        // ============================================================

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<EquipaListaDto>> CriarEquipa(
            Equipa dados)
        {
            if (dados == null)
            {
                return BadRequest(new
                {
                    mensagem = "Os dados da equipa são obrigatórios."
                });
            }

            if (string.IsNullOrWhiteSpace(dados.Nome))
            {
                return BadRequest(new
                {
                    mensagem = "O nome da equipa é obrigatório."
                });
            }

            if (!dados.UnidadeOperacionalId.HasValue &&
                !dados.SectorId.HasValue)
            {
                return BadRequest(new
                {
                    mensagem =
                        "A equipa deve estar associada a uma unidade operacional ou a um sector."
                });
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

                var seccaoSeguranca =
                    await _context.Seccoes
                        .FirstOrDefaultAsync(s =>
                            s.Nome.ToLower() ==
                            "segurança pessoal".ToLower());

                if (seccaoSeguranca != null &&
                    unidade.SeccaoId != seccaoSeguranca.Id)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "As equipas operacionais estão reservadas à Secção de Segurança Pessoal."
                    });
                }
            }

            if (dados.SectorId.HasValue)
            {
                var sector = await _context.Sectores
                    .Include(s => s.Seccao)
                    .FirstOrDefaultAsync(s =>
                        s.Id == dados.SectorId.Value);

                if (sector == null)
                {
                    return BadRequest(new
                    {
                        mensagem = "O sector indicado não existe."
                    });
                }

                if (sector.Seccao == null ||
                    !sector.Seccao.Nome.Equals(
                        "Segurança Pessoal",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "As equipas estão reservadas à Secção de Segurança Pessoal."
                    });
                }
            }

            var nome = dados.Nome.Trim();

            var existe = await _context.Equipas
                .AnyAsync(e =>
                    e.Nome.ToLower() == nome.ToLower() &&
                    e.UnidadeOperacionalId ==
                        dados.UnidadeOperacionalId &&
                    e.SectorId == dados.SectorId);

            if (existe)
            {
                return Conflict(new
                {
                    mensagem =
                        "Já existe uma equipa com este nome nesta estrutura."
                });
            }

            var novaEquipa = new Equipa
            {
                Nome = nome,

                SectorId = dados.SectorId,

                UnidadeOperacionalId =
                    dados.UnidadeOperacionalId,

                Descricao =
                    dados.Descricao?.Trim(),

                Ativo = true,

                DataCadastro = DateTime.Now
            };

            _context.Equipas.Add(novaEquipa);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEquipa),
                new { id = novaEquipa.Id },
                new
                {
                    id = novaEquipa.Id,
                    nome = novaEquipa.Nome,
                    sectorId = novaEquipa.SectorId,
                    unidadeOperacionalId =
                        novaEquipa.UnidadeOperacionalId,
                    descricao = novaEquipa.Descricao,
                    ativo = novaEquipa.Ativo,
                    dataCadastro =
                        novaEquipa.DataCadastro
                });
        }

        // ============================================================
        // PUT: api/Equipas/5
        // ADMINISTRADOR
        // ============================================================

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EditarEquipa(
            int id,
            Equipa dados)
        {
            if (dados == null)
            {
                return BadRequest(new
                {
                    mensagem = "Os dados da equipa são obrigatórios."
                });
            }

            var equipa = await _context.Equipas
                .FirstOrDefaultAsync(e => e.Id == id);

            if (equipa == null)
            {
                return NotFound(new
                {
                    mensagem = "Equipa não encontrada."
                });
            }

            if (string.IsNullOrWhiteSpace(dados.Nome))
            {
                return BadRequest(new
                {
                    mensagem = "O nome da equipa é obrigatório."
                });
            }

            if (!dados.UnidadeOperacionalId.HasValue &&
                !dados.SectorId.HasValue)
            {
                return BadRequest(new
                {
                    mensagem =
                        "A equipa deve estar associada a uma unidade operacional ou a um sector."
                });
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

                var seccaoSeguranca =
                    await _context.Seccoes
                        .FirstOrDefaultAsync(s =>
                            s.Nome.ToLower() ==
                            "segurança pessoal".ToLower());

                if (seccaoSeguranca != null &&
                    unidade.SeccaoId != seccaoSeguranca.Id)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "As equipas estão reservadas à Secção de Segurança Pessoal."
                    });
                }
            }

            if (dados.SectorId.HasValue)
            {
                var sector = await _context.Sectores
                    .Include(s => s.Seccao)
                    .FirstOrDefaultAsync(s =>
                        s.Id == dados.SectorId.Value);

                if (sector == null)
                {
                    return BadRequest(new
                    {
                        mensagem = "O sector indicado não existe."
                    });
                }

                if (sector.Seccao == null ||
                    !sector.Seccao.Nome.Equals(
                        "Segurança Pessoal",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "As equipas estão reservadas à Secção de Segurança Pessoal."
                    });
                }
            }

            var nome = dados.Nome.Trim();

            var duplicada = await _context.Equipas
                .AnyAsync(e =>
                    e.Id != id &&
                    e.Nome.ToLower() == nome.ToLower() &&
                    e.UnidadeOperacionalId ==
                        dados.UnidadeOperacionalId &&
                    e.SectorId == dados.SectorId);

            if (duplicada)
            {
                return Conflict(new
                {
                    mensagem =
                        "Já existe outra equipa com este nome nesta estrutura."
                });
            }

            equipa.Nome = nome;

            equipa.SectorId =
                dados.SectorId;

            equipa.UnidadeOperacionalId =
                dados.UnidadeOperacionalId;

            equipa.Descricao =
                dados.Descricao?.Trim();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem =
                    "Equipa actualizada com sucesso.",

                id = equipa.Id,

                nome = equipa.Nome,

                sectorId =
                    equipa.SectorId,

                unidadeOperacionalId =
                    equipa.UnidadeOperacionalId,

                descricao =
                    equipa.Descricao,

                ativo =
                    equipa.Ativo
            });
        }

        // ============================================================
        // PATCH: api/Equipas/5/estado
        // ADMINISTRADOR
        // ============================================================

        [HttpPatch("{id:int}/estado")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> AlterarEstado(
            int id,
            [FromBody] bool ativo)
        {
            var equipa = await _context.Equipas
                .FirstOrDefaultAsync(e => e.Id == id);

            if (equipa == null)
            {
                return NotFound(new
                {
                    mensagem = "Equipa não encontrada."
                });
            }

            equipa.Ativo = ativo;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = ativo
                    ? "Equipa activada com sucesso."
                    : "Equipa desactivada com sucesso.",

                id = equipa.Id,

                nome = equipa.Nome,

                ativo = equipa.Ativo
            });
        }

        // ============================================================
        // DELETE: api/Equipas/5
        // ADMINISTRADOR
        // ============================================================

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EliminarEquipa(
            int id)
        {
            var equipa = await _context.Equipas
                .FirstOrDefaultAsync(e => e.Id == id);

            if (equipa == null)
            {
                return NotFound(new
                {
                    mensagem = "Equipa não encontrada."
                });
            }

            // Eliminação lógica
            equipa.Ativo = false;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem =
                    "Equipa desactivada com sucesso.",

                id = equipa.Id,

                nome = equipa.Nome,

                ativo = equipa.Ativo
            });
        }
    }
}