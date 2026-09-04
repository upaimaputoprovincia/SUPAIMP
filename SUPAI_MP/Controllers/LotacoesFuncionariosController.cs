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
    public class LotacoesFuncionariosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LotacoesFuncionariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // DTO DE ENTRADA
        // ============================================================

        public class LotacaoFuncionarioDto
        {
            public int FuncionarioId { get; set; }

            public int? SeccaoId { get; set; }

            public int? SectorId { get; set; }

            public int? UnidadeOperacionalId { get; set; }

            public int? EquipaId { get; set; }

            public int? PostoId { get; set; }

            public int FuncaoOperacionalId { get; set; }

            public int? TipoTurnoId { get; set; }

            public DateTime DataInicio { get; set; }

            public DateTime? DataFim { get; set; }

            public bool Ativo { get; set; } = true;

            public string? Motivo { get; set; }

            public string? Observacao { get; set; }
        }

        // ============================================================
        // DTO DE RESPOSTA
        // IMPORTANTE:
        // Não contém as colecções de navegação.
        // ============================================================

        public class LotacaoFuncionarioRespostaDto
        {
            public int Id { get; set; }

            public int FuncionarioId { get; set; }
            public string? Funcionario { get; set; }

            public int? SeccaoId { get; set; }
            public string? Seccao { get; set; }

            public int? SectorId { get; set; }
            public string? Sector { get; set; }

            public int? UnidadeOperacionalId { get; set; }
            public string? UnidadeOperacional { get; set; }

            public int? EquipaId { get; set; }
            public string? Equipa { get; set; }

            public int? PostoId { get; set; }
            public string? Posto { get; set; }

            public int FuncaoOperacionalId { get; set; }
            public string? FuncaoOperacional { get; set; }

            public int? TipoTurnoId { get; set; }
            public string? TipoTurno { get; set; }

            public DateTime DataInicio { get; set; }

            public DateTime? DataFim { get; set; }

            public bool Ativo { get; set; }

            public string? Motivo { get; set; }

            public string? Observacao { get; set; }
        }

        // ============================================================
        // CONVERTER ENTIDADE -> DTO
        // ============================================================

        private static LotacaoFuncionarioRespostaDto ParaDto(
            LotacaoFuncionario l)
        {
            return new LotacaoFuncionarioRespostaDto
            {
                Id = l.Id,

                FuncionarioId = l.FuncionarioId,
                Funcionario = l.Funcionario?.NomeCompleto,

                SeccaoId = l.SeccaoId,
                Seccao = l.Seccao?.Nome,

                SectorId = l.SectorId,
                Sector = l.Sector?.Nome,

                UnidadeOperacionalId = l.UnidadeOperacionalId,
                UnidadeOperacional = l.UnidadeOperacional?.Nome,

                EquipaId = l.EquipaId,
                Equipa = l.Equipa?.Nome,

                PostoId = l.PostoId,
                Posto = l.Posto?.Nome,

                FuncaoOperacionalId = l.FuncaoOperacionalId,
                FuncaoOperacional = l.FuncaoOperacional?.Nome,

                TipoTurnoId = l.TipoTurnoId,
                TipoTurno = l.TipoTurno?.Nome,

                DataInicio = l.DataInicio,
                DataFim = l.DataFim,

                Ativo = l.Ativo,

                Motivo = l.Motivo,
                Observacao = l.Observacao
            };
        }

        // ============================================================
        // GET: api/LotacoesFuncionarios
        // Lista todas as lotações
        // ============================================================

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LotacaoFuncionarioRespostaDto>>>
            GetLotacoes()
        {
            var lotacoes = await _context.LotacoesFuncionarios
                .AsNoTracking()

                .Include(l => l.Funcionario)
                .Include(l => l.Seccao)
                .Include(l => l.Sector)
                .Include(l => l.UnidadeOperacional)
                .Include(l => l.Equipa)
                .Include(l => l.Posto)
                .Include(l => l.FuncaoOperacional)
                .Include(l => l.TipoTurno)

                .OrderByDescending(l => l.Ativo)
                .ThenByDescending(l => l.DataInicio)

                .ToListAsync();

            var resultado = lotacoes
                .Select(ParaDto)
                .ToList();

            return Ok(resultado);
        }

        // ============================================================
        // GET: api/LotacoesFuncionarios/5
        // ============================================================

        [HttpGet("{id:int}")]
        public async Task<ActionResult<LotacaoFuncionarioRespostaDto>>
            GetLotacao(int id)
        {
            var lotacao = await _context.LotacoesFuncionarios
                .AsNoTracking()

                .Include(l => l.Funcionario)
                .Include(l => l.Seccao)
                .Include(l => l.Sector)
                .Include(l => l.UnidadeOperacional)
                .Include(l => l.Equipa)
                .Include(l => l.Posto)
                .Include(l => l.FuncaoOperacional)
                .Include(l => l.TipoTurno)

                .FirstOrDefaultAsync(l => l.Id == id);

            if (lotacao == null)
            {
                return NotFound(new
                {
                    mensagem = "Lotação não encontrada."
                });
            }

            return Ok(ParaDto(lotacao));
        }

        // ============================================================
        // GET:
        // api/LotacoesFuncionarios/funcionario/5
        //
        // Histórico do funcionário
        // ============================================================

        [HttpGet("funcionario/{funcionarioId:int}")]
        public async Task<ActionResult<IEnumerable<LotacaoFuncionarioRespostaDto>>>
            GetLotacoesFuncionario(int funcionarioId)
        {
            var funcionarioExiste =
                await _context.Funcionarios
                    .AnyAsync(f => f.Id == funcionarioId);

            if (!funcionarioExiste)
            {
                return NotFound(new
                {
                    mensagem = "Funcionário não encontrado."
                });
            }

            var lotacoes = await _context.LotacoesFuncionarios
                .AsNoTracking()

                .Where(l => l.FuncionarioId == funcionarioId)

                .Include(l => l.Funcionario)
                .Include(l => l.Seccao)
                .Include(l => l.Sector)
                .Include(l => l.UnidadeOperacional)
                .Include(l => l.Equipa)
                .Include(l => l.Posto)
                .Include(l => l.FuncaoOperacional)
                .Include(l => l.TipoTurno)

                .OrderByDescending(l => l.DataInicio)

                .ToListAsync();

            var resultado = lotacoes
                .Select(ParaDto)
                .ToList();

            return Ok(resultado);
        }

        // ============================================================
        // GET:
        // api/LotacoesFuncionarios/funcionario/5/actual
        //
        // Lotação activa
        // ============================================================

        [HttpGet("funcionario/{funcionarioId:int}/actual")]
        public async Task<ActionResult<LotacaoFuncionarioRespostaDto>>
            GetLotacaoActual(int funcionarioId)
        {
            var lotacao = await _context.LotacoesFuncionarios

                .AsNoTracking()

                .Where(l =>
                    l.FuncionarioId == funcionarioId &&
                    l.Ativo)

                .Include(l => l.Funcionario)
                .Include(l => l.Seccao)
                .Include(l => l.Sector)
                .Include(l => l.UnidadeOperacional)
                .Include(l => l.Equipa)
                .Include(l => l.Posto)
                .Include(l => l.FuncaoOperacional)
                .Include(l => l.TipoTurno)

                .OrderByDescending(l => l.DataInicio)

                .FirstOrDefaultAsync();

            if (lotacao == null)
            {
                return NotFound(new
                {
                    mensagem =
                        "O funcionário não possui uma lotação activa."
                });
            }

            return Ok(ParaDto(lotacao));
        }

        // ============================================================
        // POST: api/LotacoesFuncionarios
        // Criar lotação
        // ============================================================

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<LotacaoFuncionarioRespostaDto>>
            CriarLotacao(
                [FromBody] LotacaoFuncionarioDto dados)
        {
            if (dados == null)
            {
                return BadRequest(new
                {
                    mensagem =
                        "Os dados da lotação são obrigatórios."
                });
            }

            // --------------------------------------------------------
            // Funcionário
            // --------------------------------------------------------

            var funcionario =
                await _context.Funcionarios
                    .FirstOrDefaultAsync(
                        f => f.Id == dados.FuncionarioId);

            if (funcionario == null)
            {
                return BadRequest(new
                {
                    mensagem =
                        "O funcionário indicado não existe."
                });
            }

            // --------------------------------------------------------
            // Função operacional
            // --------------------------------------------------------

            var funcao =
                await _context.FuncoesOperacionais
                    .FirstOrDefaultAsync(f =>
                        f.Id == dados.FuncaoOperacionalId &&
                        f.Ativo);

            if (funcao == null)
            {
                return BadRequest(new
                {
                    mensagem =
                        "A função operacional indicada não existe ou está inactiva."
                });
            }

            // --------------------------------------------------------
            // COMANDANTE
            // --------------------------------------------------------

            if (funcao.Nome.Equals(
                    "Comandante",
                    StringComparison.OrdinalIgnoreCase))
            {
                if (dados.SeccaoId.HasValue ||
                    dados.SectorId.HasValue ||
                    dados.UnidadeOperacionalId.HasValue ||
                    dados.EquipaId.HasValue ||
                    dados.PostoId.HasValue)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "O Comandante é uma autoridade de nível máximo e não deve possuir Secção, Sector, Unidade, Equipa ou Posto."
                    });
                }
            }

            // --------------------------------------------------------
            // SECÇÃO
            // --------------------------------------------------------

            Seccao? seccao = null;

            if (dados.SeccaoId.HasValue)
            {
                seccao =
                    await _context.Seccoes
                        .FirstOrDefaultAsync(s =>
                            s.Id == dados.SeccaoId.Value &&
                            s.Ativo);

                if (seccao == null)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "A secção indicada não existe ou está inactiva."
                    });
                }
            }

            // --------------------------------------------------------
            // SECTOR
            // --------------------------------------------------------

            if (dados.SectorId.HasValue)
            {
                var sector =
                    await _context.Sectores
                        .FirstOrDefaultAsync(s =>
                            s.Id == dados.SectorId.Value &&
                            s.Ativo);

                if (sector == null)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "O sector indicado não existe ou está inactivo."
                    });
                }

                if (!dados.SeccaoId.HasValue ||
                    sector.SeccaoId != dados.SeccaoId.Value)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "O sector indicado não pertence à secção seleccionada."
                    });
                }
            }

            // --------------------------------------------------------
            // UNIDADE OPERACIONAL
            // --------------------------------------------------------

            if (dados.UnidadeOperacionalId.HasValue)
            {
                var unidade =
                    await _context.UnidadesOperacionais
                        .FirstOrDefaultAsync(u =>
                            u.Id ==
                                dados.UnidadeOperacionalId.Value &&
                            u.Ativo);

                if (unidade == null)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "A unidade operacional indicada não existe ou está inactiva."
                    });
                }

                if (!dados.SeccaoId.HasValue ||
                    unidade.SeccaoId != dados.SeccaoId.Value)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "A unidade operacional não pertence à secção seleccionada."
                    });
                }
            }

            // --------------------------------------------------------
            // EQUIPA
            // --------------------------------------------------------

            if (dados.EquipaId.HasValue)
            {
                var equipa =
                    await _context.Equipas
                        .FirstOrDefaultAsync(e =>
                            e.Id == dados.EquipaId.Value &&
                            e.Ativo);

                if (equipa == null)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "A equipa indicada não existe ou está inactiva."
                    });
                }

                if (!dados.SeccaoId.HasValue)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "Uma equipa exige uma secção."
                    });
                }

                var nomeSeccao =
                    seccao?.Nome ?? string.Empty;

                if (!nomeSeccao.Equals(
                        "Segurança Pessoal",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "As equipas estão reservadas à Secção de Segurança Pessoal."
                    });
                }

                if (equipa.UnidadeOperacionalId.HasValue)
                {
                    if (!dados.UnidadeOperacionalId.HasValue ||
                        equipa.UnidadeOperacionalId.Value !=
                            dados.UnidadeOperacionalId.Value)
                    {
                        return BadRequest(new
                        {
                            mensagem =
                                "A equipa seleccionada não pertence à unidade operacional indicada."
                        });
                    }
                }

                if (equipa.SectorId.HasValue)
                {
                    if (!dados.SectorId.HasValue ||
                        equipa.SectorId.Value !=
                            dados.SectorId.Value)
                    {
                        return BadRequest(new
                        {
                            mensagem =
                                "A equipa seleccionada não pertence ao sector indicado."
                        });
                    }
                }
            }

            // --------------------------------------------------------
            // POSTO
            // --------------------------------------------------------

            if (dados.PostoId.HasValue)
            {
                var posto =
                    await _context.Postos
                        .FirstOrDefaultAsync(p =>
                            p.Id == dados.PostoId.Value &&
                            p.Ativo);

                if (posto == null)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "O posto indicado não existe ou está inactivo."
                    });
                }

                if (!dados.SeccaoId.HasValue ||
                    posto.SeccaoId != dados.SeccaoId.Value)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "O posto indicado não pertence à secção seleccionada."
                    });
                }

                if (posto.SectorId.HasValue)
                {
                    if (!dados.SectorId.HasValue ||
                        posto.SectorId.Value !=
                            dados.SectorId.Value)
                    {
                        return BadRequest(new
                        {
                            mensagem =
                                "O posto indicado não pertence ao sector seleccionado."
                        });
                    }
                }

                if (posto.UnidadeOperacionalId.HasValue)
                {
                    if (!dados.UnidadeOperacionalId.HasValue ||
                        posto.UnidadeOperacionalId.Value !=
                            dados.UnidadeOperacionalId.Value)
                    {
                        return BadRequest(new
                        {
                            mensagem =
                                "O posto indicado não pertence à unidade operacional seleccionada."
                        });
                    }
                }
            }

            // --------------------------------------------------------
            // TURNO
            // --------------------------------------------------------

            if (dados.TipoTurnoId.HasValue)
            {
                var turnoExiste =
                    await _context.TiposTurno
                        .AnyAsync(t =>
                            t.Id == dados.TipoTurnoId.Value &&
                            t.Ativo);

                if (!turnoExiste)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "O tipo de turno indicado não existe ou está inactivo."
                    });
                }
            }

            // --------------------------------------------------------
            // LOTAMENTO ACTUAL
            // --------------------------------------------------------

            var lotacaoActual =
                await _context.LotacoesFuncionarios
                    .FirstOrDefaultAsync(l =>
                        l.FuncionarioId ==
                            dados.FuncionarioId &&
                        l.Ativo);

            if (lotacaoActual != null)
            {
                return Conflict(new
                {
                    mensagem =
                        "O funcionário já possui uma lotação activa. Termine a lotação actual antes de criar outra."
                });
            }

            // --------------------------------------------------------
            // CRIAR
            // --------------------------------------------------------

            var novaLotacao =
                new LotacaoFuncionario
                {
                    FuncionarioId =
                        dados.FuncionarioId,

                    SeccaoId =
                        dados.SeccaoId,

                    SectorId =
                        dados.SectorId,

                    UnidadeOperacionalId =
                        dados.UnidadeOperacionalId,

                    EquipaId =
                        dados.EquipaId,

                    PostoId =
                        dados.PostoId,

                    FuncaoOperacionalId =
                        dados.FuncaoOperacionalId,

                    TipoTurnoId =
                        dados.TipoTurnoId,

                    DataInicio =
                        dados.DataInicio == default
                            ? DateTime.Now
                            : dados.DataInicio,

                    DataFim = null,

                    Ativo = true,

                    Motivo =
                        dados.Motivo?.Trim(),

                    Observacao =
                        dados.Observacao?.Trim()
                };

            _context.LotacoesFuncionarios
                .Add(novaLotacao);

            await _context.SaveChangesAsync();

            // --------------------------------------------------------
            // RECARREGAR COM RELAÇÕES
            // --------------------------------------------------------

            var criada =
                await _context.LotacoesFuncionarios
                    .AsNoTracking()

                    .Include(l => l.Funcionario)
                    .Include(l => l.Seccao)
                    .Include(l => l.Sector)
                    .Include(l => l.UnidadeOperacional)
                    .Include(l => l.Equipa)
                    .Include(l => l.Posto)
                    .Include(l => l.FuncaoOperacional)
                    .Include(l => l.TipoTurno)

                    .FirstAsync(
                        l => l.Id == novaLotacao.Id);

            return CreatedAtAction(
                nameof(GetLotacao),
                new { id = criada.Id },
                ParaDto(criada));
        }

        // ============================================================
        // PUT: api/LotacoesFuncionarios/5
        // ============================================================

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<LotacaoFuncionarioRespostaDto>>
            EditarLotacao(
                int id,
                [FromBody] LotacaoFuncionarioDto dados)
        {
            if (dados == null)
            {
                return BadRequest(new
                {
                    mensagem =
                        "Os dados da lotação são obrigatórios."
                });
            }

            var lotacao =
                await _context.LotacoesFuncionarios
                    .FirstOrDefaultAsync(l => l.Id == id);

            if (lotacao == null)
            {
                return NotFound(new
                {
                    mensagem =
                        "Lotação não encontrada."
                });
            }

            if (!lotacao.Ativo)
            {
                return BadRequest(new
                {
                    mensagem =
                        "Não é possível editar uma lotação que já foi encerrada."
                });
            }

            // --------------------------------------------------------
            // FUNCIONÁRIO
            // --------------------------------------------------------

            var funcionarioExiste =
                await _context.Funcionarios
                    .AnyAsync(f =>
                        f.Id == dados.FuncionarioId);

            if (!funcionarioExiste)
            {
                return BadRequest(new
                {
                    mensagem =
                        "O funcionário indicado não existe."
                });
            }

            // --------------------------------------------------------
            // FUNÇÃO
            // --------------------------------------------------------

            var funcao =
                await _context.FuncoesOperacionais
                    .FirstOrDefaultAsync(f =>
                        f.Id ==
                            dados.FuncaoOperacionalId &&
                        f.Ativo);

            if (funcao == null)
            {
                return BadRequest(new
                {
                    mensagem =
                        "A função operacional indicada não existe ou está inactiva."
                });
            }

            // --------------------------------------------------------
            // COMANDANTE
            // --------------------------------------------------------

            if (funcao.Nome.Equals(
                    "Comandante",
                    StringComparison.OrdinalIgnoreCase))
            {
                if (dados.SeccaoId.HasValue ||
                    dados.SectorId.HasValue ||
                    dados.UnidadeOperacionalId.HasValue ||
                    dados.EquipaId.HasValue ||
                    dados.PostoId.HasValue)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "O Comandante não deve possuir Secção, Sector, Unidade, Equipa ou Posto."
                    });
                }
            }

            // --------------------------------------------------------
            // SECÇÃO
            // --------------------------------------------------------

            Seccao? seccao = null;

            if (dados.SeccaoId.HasValue)
            {
                seccao =
                    await _context.Seccoes
                        .FirstOrDefaultAsync(s =>
                            s.Id == dados.SeccaoId.Value &&
                            s.Ativo);

                if (seccao == null)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "A secção indicada não existe ou está inactiva."
                    });
                }
            }

            // --------------------------------------------------------
            // SECTOR
            // --------------------------------------------------------

            if (dados.SectorId.HasValue)
            {
                var sector =
                    await _context.Sectores
                        .FirstOrDefaultAsync(s =>
                            s.Id == dados.SectorId.Value &&
                            s.Ativo);

                if (sector == null)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "O sector indicado não existe ou está inactivo."
                    });
                }

                if (!dados.SeccaoId.HasValue ||
                    sector.SeccaoId != dados.SeccaoId.Value)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "O sector não pertence à secção indicada."
                    });
                }
            }

            // --------------------------------------------------------
            // UNIDADE
            // --------------------------------------------------------

            if (dados.UnidadeOperacionalId.HasValue)
            {
                var unidade =
                    await _context.UnidadesOperacionais
                        .FirstOrDefaultAsync(u =>
                            u.Id ==
                                dados.UnidadeOperacionalId.Value &&
                            u.Ativo);

                if (unidade == null)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "A unidade operacional não existe ou está inactiva."
                    });
                }

                if (!dados.SeccaoId.HasValue ||
                    unidade.SeccaoId != dados.SeccaoId.Value)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "A unidade operacional não pertence à secção indicada."
                    });
                }
            }

            // --------------------------------------------------------
            // EQUIPA
            // --------------------------------------------------------

            if (dados.EquipaId.HasValue)
            {
                var equipa =
                    await _context.Equipas
                        .FirstOrDefaultAsync(e =>
                            e.Id == dados.EquipaId.Value &&
                            e.Ativo);

                if (equipa == null)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "A equipa não existe ou está inactiva."
                    });
                }

                if (!dados.SeccaoId.HasValue)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "Uma equipa exige uma secção."
                    });
                }

                var nomeSeccao =
                    seccao?.Nome ?? string.Empty;

                if (!nomeSeccao.Equals(
                        "Segurança Pessoal",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "As equipas estão reservadas à Segurança Pessoal."
                    });
                }

                if (equipa.UnidadeOperacionalId.HasValue &&
                    equipa.UnidadeOperacionalId.Value !=
                        dados.UnidadeOperacionalId)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "A equipa não pertence à unidade indicada."
                    });
                }

                if (equipa.SectorId.HasValue &&
                    equipa.SectorId.Value !=
                        dados.SectorId)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "A equipa não pertence ao sector indicado."
                    });
                }
            }

            // --------------------------------------------------------
            // POSTO
            // --------------------------------------------------------

            if (dados.PostoId.HasValue)
            {
                var posto =
                    await _context.Postos
                        .FirstOrDefaultAsync(p =>
                            p.Id == dados.PostoId.Value &&
                            p.Ativo);

                if (posto == null)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "O posto não existe ou está inactivo."
                    });
                }

                if (!dados.SeccaoId.HasValue ||
                    posto.SeccaoId != dados.SeccaoId.Value)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "O posto não pertence à secção indicada."
                    });
                }

                if (posto.SectorId.HasValue &&
                    posto.SectorId.Value !=
                        dados.SectorId)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "O posto não pertence ao sector indicado."
                    });
                }

                if (posto.UnidadeOperacionalId.HasValue &&
                    posto.UnidadeOperacionalId.Value !=
                        dados.UnidadeOperacionalId)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "O posto não pertence à unidade indicada."
                    });
                }
            }

            // --------------------------------------------------------
            // TURNO
            // --------------------------------------------------------

            if (dados.TipoTurnoId.HasValue)
            {
                var turnoExiste =
                    await _context.TiposTurno
                        .AnyAsync(t =>
                            t.Id == dados.TipoTurnoId.Value &&
                            t.Ativo);

                if (!turnoExiste)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "O tipo de turno não existe ou está inactivo."
                    });
                }
            }

            // --------------------------------------------------------
            // OUTRA LOTACAO ACTIVA
            // --------------------------------------------------------

            var outraLotacao =
                await _context.LotacoesFuncionarios
                    .AnyAsync(l =>
                        l.Id != id &&
                        l.FuncionarioId ==
                            dados.FuncionarioId &&
                        l.Ativo);

            if (outraLotacao)
            {
                return Conflict(new
                {
                    mensagem =
                        "O funcionário já possui outra lotação activa."
                });
            }

            // --------------------------------------------------------
            // ACTUALIZAR
            // --------------------------------------------------------

            lotacao.FuncionarioId =
                dados.FuncionarioId;

            lotacao.SeccaoId =
                dados.SeccaoId;

            lotacao.SectorId =
                dados.SectorId;

            lotacao.UnidadeOperacionalId =
                dados.UnidadeOperacionalId;

            lotacao.EquipaId =
                dados.EquipaId;

            lotacao.PostoId =
                dados.PostoId;

            lotacao.FuncaoOperacionalId =
                dados.FuncaoOperacionalId;

            lotacao.TipoTurnoId =
                dados.TipoTurnoId;

            if (dados.DataInicio != default)
            {
                lotacao.DataInicio =
                    dados.DataInicio;
            }

            lotacao.Motivo =
                dados.Motivo?.Trim();

            lotacao.Observacao =
                dados.Observacao?.Trim();

            await _context.SaveChangesAsync();

            // --------------------------------------------------------
            // RECARREGAR
            // --------------------------------------------------------

            var actualizada =
                await _context.LotacoesFuncionarios
                    .AsNoTracking()

                    .Include(l => l.Funcionario)
                    .Include(l => l.Seccao)
                    .Include(l => l.Sector)
                    .Include(l => l.UnidadeOperacional)
                    .Include(l => l.Equipa)
                    .Include(l => l.Posto)
                    .Include(l => l.FuncaoOperacional)
                    .Include(l => l.TipoTurno)

                    .FirstAsync(l => l.Id == id);

            return Ok(ParaDto(actualizada));
        }

        // ============================================================
        // PATCH:
        // api/LotacoesFuncionarios/5/encerrar
        // ============================================================

        [HttpPatch("{id:int}/encerrar")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult>
            EncerrarLotacao(
                int id,
                [FromBody] string? motivo)
        {
            var lotacao =
                await _context.LotacoesFuncionarios
                    .FirstOrDefaultAsync(l =>
                        l.Id == id);

            if (lotacao == null)
            {
                return NotFound(new
                {
                    mensagem =
                        "Lotação não encontrada."
                });
            }

            if (!lotacao.Ativo)
            {
                return BadRequest(new
                {
                    mensagem =
                        "A lotação já está encerrada."
                });
            }

            lotacao.Ativo = false;

            lotacao.DataFim =
                DateTime.Now;

            if (!string.IsNullOrWhiteSpace(motivo))
            {
                lotacao.Motivo =
                    motivo.Trim();
            }

            await _context.SaveChangesAsync();

            // --------------------------------------------------------
            // RECARREGAR SEM CICLO
            // --------------------------------------------------------

            var resultado =
                await _context.LotacoesFuncionarios
                    .AsNoTracking()

                    .Include(l => l.Funcionario)
                    .Include(l => l.Seccao)
                    .Include(l => l.Sector)
                    .Include(l => l.UnidadeOperacional)
                    .Include(l => l.Equipa)
                    .Include(l => l.Posto)
                    .Include(l => l.FuncaoOperacional)
                    .Include(l => l.TipoTurno)

                    .FirstAsync(l => l.Id == id);

            return Ok(new
            {
                mensagem =
                    "Lotação encerrada com sucesso.",

                lotacao =
                    ParaDto(resultado)
            });
        }

        // ============================================================
        // DELETE:
        // api/LotacoesFuncionarios/5
        //
        // Eliminação lógica
        // ============================================================

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult>
            EliminarLotacao(int id)
        {
            var lotacao =
                await _context.LotacoesFuncionarios
                    .FirstOrDefaultAsync(l =>
                        l.Id == id);

            if (lotacao == null)
            {
                return NotFound(new
                {
                    mensagem =
                        "Lotação não encontrada."
                });
            }

            if (!lotacao.Ativo)
            {
                return BadRequest(new
                {
                    mensagem =
                        "A lotação já está desactivada."
                });
            }

            lotacao.Ativo = false;

            lotacao.DataFim ??=
                DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem =
                    "Lotação desactivada com sucesso."
            });
        }
    }
}