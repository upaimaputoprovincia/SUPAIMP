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
        // DTO PARA CRIAR UMA LOTAÇÃO
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

            public DateTime? DataInicio { get; set; }

            public string? Motivo { get; set; }

            public string? Observacao { get; set; }
        }

        // ============================================================
        // DTO DE RESPOSTA
        // ============================================================
        public class LotacaoFuncionarioRespostaDto
        {
            public int Id { get; set; }

            public int FuncionarioId { get; set; }

            public string Funcionario { get; set; } = string.Empty;

            public string Nip { get; set; } = string.Empty;

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
        // DTO PARA TRANSFERÊNCIA
        // ============================================================
        public class TransferirFuncionarioDto
        {
            public int? SeccaoId { get; set; }

            public int? SectorId { get; set; }

            public int? UnidadeOperacionalId { get; set; }

            public int? EquipaId { get; set; }

            public int? PostoId { get; set; }

            public int FuncaoOperacionalId { get; set; }

            public int? TipoTurnoId { get; set; }

            public DateTime? DataInicio { get; set; }

            public string? Motivo { get; set; }

            public string? Observacao { get; set; }
        }

        // ============================================================
        // MÉTODO AUXILIAR — CONVERTER PARA DTO
        // ============================================================
        private static LotacaoFuncionarioRespostaDto ParaDto(
            LotacaoFuncionario l)
        {
            return new LotacaoFuncionarioRespostaDto
            {
                Id = l.Id,

                FuncionarioId = l.FuncionarioId,

                Funcionario = l.Funcionario?.NomeCompleto ?? string.Empty,

                Nip = l.Funcionario?.Nip ?? string.Empty,

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
        // GET — TODAS AS LOTAÇÕES
        // ============================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LotacaoFuncionarioRespostaDto>>> GetTodas()
        {
            var lotacoes = await _context.LotacoesFuncionarios
                .AsNoTracking()
                .Include(x => x.Funcionario)
                .Include(x => x.Seccao)
                .Include(x => x.Sector)
                .Include(x => x.UnidadeOperacional)
                .Include(x => x.Equipa)
                .Include(x => x.Posto)
                .Include(x => x.FuncaoOperacional)
                .Include(x => x.TipoTurno)
                .OrderByDescending(x => x.Ativo)
                .ThenByDescending(x => x.DataInicio)
                .ToListAsync();

            return Ok(lotacoes.Select(ParaDto));
        }

        // ============================================================
        // GET — LOTAÇÃO POR ID
        // ============================================================
        [HttpGet("{id:int}")]
        public async Task<ActionResult<LotacaoFuncionarioRespostaDto>> GetPorId(int id)
        {
            var lotacao = await _context.LotacoesFuncionarios
                .AsNoTracking()
                .Include(x => x.Funcionario)
                .Include(x => x.Seccao)
                .Include(x => x.Sector)
                .Include(x => x.UnidadeOperacional)
                .Include(x => x.Equipa)
                .Include(x => x.Posto)
                .Include(x => x.FuncaoOperacional)
                .Include(x => x.TipoTurno)
                .FirstOrDefaultAsync(x => x.Id == id);

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
        // GET — LOTAÇÃO ATUAL DE UM FUNCIONÁRIO
        // ============================================================
        [HttpGet("funcionario/{funcionarioId:int}")]
        public async Task<ActionResult<IEnumerable<LotacaoFuncionarioRespostaDto>>>
            GetPorFuncionario(int funcionarioId)
        {
            var funcionarioExiste = await _context.Funcionarios
                .AnyAsync(x => x.Id == funcionarioId);

            if (!funcionarioExiste)
            {
                return NotFound(new
                {
                    mensagem = "Funcionário não encontrado."
                });
            }

            var lotacoes = await _context.LotacoesFuncionarios
                .AsNoTracking()
                .Where(x => x.FuncionarioId == funcionarioId)
                .Include(x => x.Funcionario)
                .Include(x => x.Seccao)
                .Include(x => x.Sector)
                .Include(x => x.UnidadeOperacional)
                .Include(x => x.Equipa)
                .Include(x => x.Posto)
                .Include(x => x.FuncaoOperacional)
                .Include(x => x.TipoTurno)
                .OrderByDescending(x => x.Ativo)
                .ThenByDescending(x => x.DataInicio)
                .ToListAsync();

            return Ok(lotacoes.Select(ParaDto));
        }

        // ============================================================
        // GET — LOTAÇÃO ATUAL DE UM FUNCIONÁRIO
        // ============================================================
        [HttpGet("funcionario/{funcionarioId:int}/atual")]
        public async Task<ActionResult<LotacaoFuncionarioRespostaDto>>
            GetLotacaoAtual(int funcionarioId)
        {
            var lotacao = await _context.LotacoesFuncionarios
                .AsNoTracking()
                .Where(x =>
                    x.FuncionarioId == funcionarioId &&
                    x.Ativo)
                .Include(x => x.Funcionario)
                .Include(x => x.Seccao)
                .Include(x => x.Sector)
                .Include(x => x.UnidadeOperacional)
                .Include(x => x.Equipa)
                .Include(x => x.Posto)
                .Include(x => x.FuncaoOperacional)
                .Include(x => x.TipoTurno)
                .OrderByDescending(x => x.DataInicio)
                .FirstOrDefaultAsync();

            if (lotacao == null)
            {
                return NotFound(new
                {
                    mensagem = "O funcionário não possui uma lotação ativa."
                });
            }

            return Ok(ParaDto(lotacao));
        }

        // ============================================================
        // GET — FUNCIONÁRIOS DE UMA EQUIPA
        // ============================================================
        [HttpGet("equipa/{equipaId:int}")]
        public async Task<ActionResult<IEnumerable<LotacaoFuncionarioRespostaDto>>>
            GetPorEquipa(int equipaId)
        {
            var equipaExiste = await _context.Equipas
                .AnyAsync(x => x.Id == equipaId);

            if (!equipaExiste)
            {
                return NotFound(new
                {
                    mensagem = "Equipa não encontrada."
                });
            }

            var lotacoes = await _context.LotacoesFuncionarios
                .AsNoTracking()
                .Where(x =>
                    x.EquipaId == equipaId &&
                    x.Ativo)
                .Include(x => x.Funcionario)
                .Include(x => x.Seccao)
                .Include(x => x.Sector)
                .Include(x => x.UnidadeOperacional)
                .Include(x => x.Equipa)
                .Include(x => x.Posto)
                .Include(x => x.FuncaoOperacional)
                .Include(x => x.TipoTurno)
                .OrderBy(x => x.Funcionario!.NomeCompleto)
                .ToListAsync();

            return Ok(lotacoes.Select(ParaDto));
        }

        // ============================================================
        // GET — FUNCIONÁRIOS DE UMA UNIDADE/ESCOLTA
        // ============================================================
        [HttpGet("unidade/{unidadeOperacionalId:int}")]
        public async Task<ActionResult<IEnumerable<LotacaoFuncionarioRespostaDto>>>
            GetPorUnidade(int unidadeOperacionalId)
        {
            var unidadeExiste = await _context.UnidadesOperacionais
                .AnyAsync(x => x.Id == unidadeOperacionalId);

            if (!unidadeExiste)
            {
                return NotFound(new
                {
                    mensagem = "Unidade operacional não encontrada."
                });
            }

            var lotacoes = await _context.LotacoesFuncionarios
                .AsNoTracking()
                .Where(x =>
                    x.UnidadeOperacionalId == unidadeOperacionalId &&
                    x.Ativo)
                .Include(x => x.Funcionario)
                .Include(x => x.Seccao)
                .Include(x => x.Sector)
                .Include(x => x.UnidadeOperacional)
                .Include(x => x.Equipa)
                .Include(x => x.Posto)
                .Include(x => x.FuncaoOperacional)
                .Include(x => x.TipoTurno)
                .OrderBy(x => x.Equipa!.Nome)
                .ThenBy(x => x.Funcionario!.NomeCompleto)
                .ToListAsync();

            return Ok(lotacoes.Select(ParaDto));
        }

        // ============================================================
        // POST — CRIAR NOVA LOTAÇÃO
        // ============================================================
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<LotacaoFuncionarioRespostaDto>>
            Criar(LotacaoFuncionarioDto dto)
        {
            // --------------------------------------------------------
            // FUNCIONÁRIO
            // --------------------------------------------------------
            var funcionario = await _context.Funcionarios
                .FirstOrDefaultAsync(x => x.Id == dto.FuncionarioId);

            if (funcionario == null)
            {
                return BadRequest(new
                {
                    mensagem = "Funcionário não encontrado."
                });
            }

            // --------------------------------------------------------
            // FUNÇÃO OPERACIONAL
            // --------------------------------------------------------
            var funcao = await _context.FuncoesOperacionais
                .FirstOrDefaultAsync(x => x.Id == dto.FuncaoOperacionalId);

            if (funcao == null)
            {
                return BadRequest(new
                {
                    mensagem = "Função operacional não encontrada."
                });
            }

            // --------------------------------------------------------
            // SECCAO
            // --------------------------------------------------------
            if (dto.SeccaoId.HasValue)
            {
                var existe = await _context.Seccoes
                    .AnyAsync(x => x.Id == dto.SeccaoId.Value);

                if (!existe)
                {
                    return BadRequest(new
                    {
                        mensagem = "Secção não encontrada."
                    });
                }
            }

            // --------------------------------------------------------
            // SECTOR
            // --------------------------------------------------------
            if (dto.SectorId.HasValue)
            {
                var sector = await _context.Sectores
                    .FirstOrDefaultAsync(x => x.Id == dto.SectorId.Value);

                if (sector == null)
                {
                    return BadRequest(new
                    {
                        mensagem = "Sector não encontrado."
                    });
                }

                if (dto.SeccaoId.HasValue &&
                    sector.SeccaoId != dto.SeccaoId.Value)
                {
                    return BadRequest(new
                    {
                        mensagem = "O sector selecionado não pertence à secção indicada."
                    });
                }
            }

            // --------------------------------------------------------
            // UNIDADE OPERACIONAL
            // --------------------------------------------------------
            if (dto.UnidadeOperacionalId.HasValue)
            {
                var unidade = await _context.UnidadesOperacionais
                    .FirstOrDefaultAsync(x =>
                        x.Id == dto.UnidadeOperacionalId.Value);

                if (unidade == null)
                {
                    return BadRequest(new
                    {
                        mensagem = "Unidade operacional não encontrada."
                    });
                }

                if (dto.SeccaoId.HasValue &&
                    unidade.SeccaoId != dto.SeccaoId.Value)
                {
                    return BadRequest(new
                    {
                        mensagem = "A unidade operacional não pertence à secção indicada."
                    });
                }
            }

            // --------------------------------------------------------
            // EQUIPA
            // --------------------------------------------------------
            if (dto.EquipaId.HasValue)
            {
                var equipa = await _context.Equipas
                    .FirstOrDefaultAsync(x => x.Id == dto.EquipaId.Value);

                if (equipa == null)
                {
                    return BadRequest(new
                    {
                        mensagem = "Equipa não encontrada."
                    });
                }

                if (dto.UnidadeOperacionalId.HasValue &&
                    equipa.UnidadeOperacionalId != dto.UnidadeOperacionalId.Value)
                {
                    return BadRequest(new
                    {
                        mensagem = "A equipa selecionada não pertence à unidade operacional indicada."
                    });
                }

                if (dto.SectorId.HasValue &&
                    equipa.SectorId != dto.SectorId.Value)
                {
                    return BadRequest(new
                    {
                        mensagem = "A equipa selecionada não pertence ao sector indicado."
                    });
                }
            }

            // --------------------------------------------------------
            // POSTO
            // --------------------------------------------------------
            if (dto.PostoId.HasValue)
            {
                var posto = await _context.Postos
                    .FirstOrDefaultAsync(x => x.Id == dto.PostoId.Value);

                if (posto == null)
                {
                    return BadRequest(new
                    {
                        mensagem = "Posto não encontrado."
                    });
                }

                if (!posto.Ativo)
                {
                    return BadRequest(new
                    {
                        mensagem = "O posto selecionado está inativo."
                    });
                }

                if (dto.UnidadeOperacionalId.HasValue &&
                    posto.UnidadeOperacionalId != dto.UnidadeOperacionalId.Value)
                {
                    return BadRequest(new
                    {
                        mensagem = "O posto selecionado não pertence à unidade operacional indicada."
                    });
                }

                if (dto.SectorId.HasValue &&
                    posto.SectorId != dto.SectorId.Value)
                {
                    return BadRequest(new
                    {
                        mensagem = "O posto selecionado não pertence ao sector indicado."
                    });
                }

                if (dto.SeccaoId.HasValue &&
                    posto.SeccaoId != dto.SeccaoId.Value)
                {
                    return BadRequest(new
                    {
                        mensagem = "O posto selecionado não pertence à secção indicada."
                    });
                }
            }

            // --------------------------------------------------------
            // TIPO DE TURNO
            // --------------------------------------------------------
            if (dto.TipoTurnoId.HasValue)
            {
                var turno = await _context.TiposTurno
                    .FirstOrDefaultAsync(x => x.Id == dto.TipoTurnoId.Value);

                if (turno == null)
                {
                    return BadRequest(new
                    {
                        mensagem = "Tipo de turno não encontrado."
                    });
                }

                if (!turno.Ativo)
                {
                    return BadRequest(new
                    {
                        mensagem = "O tipo de turno selecionado está inativo."
                    });
                }
            }

            // --------------------------------------------------------
            // VERIFICAR LOTAÇÃO ATIVA
            // --------------------------------------------------------
            var lotacaoAtual = await _context.LotacoesFuncionarios
                .FirstOrDefaultAsync(x =>
                    x.FuncionarioId == dto.FuncionarioId &&
                    x.Ativo);

            if (lotacaoAtual != null)
            {
                return Conflict(new
                {
                    mensagem =
                        "Este funcionário já possui uma lotação ativa. " +
                        "Para mudar de equipa ou unidade, utilize a transferência."
                });
            }

            // --------------------------------------------------------
            // CRIAR
            // --------------------------------------------------------
            var novaLotacao = new LotacaoFuncionario
            {
                FuncionarioId = dto.FuncionarioId,

                SeccaoId = dto.SeccaoId,

                SectorId = dto.SectorId,

                UnidadeOperacionalId = dto.UnidadeOperacionalId,

                EquipaId = dto.EquipaId,

                PostoId = dto.PostoId,

                FuncaoOperacionalId = dto.FuncaoOperacionalId,

                TipoTurnoId = dto.TipoTurnoId,

                DataInicio = dto.DataInicio ?? DateTime.Now,

                DataFim = null,

                Ativo = true,

                Motivo = dto.Motivo,

                Observacao = dto.Observacao
            };

            _context.LotacoesFuncionarios.Add(novaLotacao);

            await _context.SaveChangesAsync();

            // --------------------------------------------------------
            // RECARREGAR
            // --------------------------------------------------------
            var resultado = await _context.LotacoesFuncionarios
                .AsNoTracking()
                .Include(x => x.Funcionario)
                .Include(x => x.Seccao)
                .Include(x => x.Sector)
                .Include(x => x.UnidadeOperacional)
                .Include(x => x.Equipa)
                .Include(x => x.Posto)
                .Include(x => x.FuncaoOperacional)
                .Include(x => x.TipoTurno)
                .FirstAsync(x => x.Id == novaLotacao.Id);

            return CreatedAtAction(
                nameof(GetPorId),
                new { id = novaLotacao.Id },
                ParaDto(resultado));
        }

        // ============================================================
        // PUT — EDITAR LOTAÇÃO
        // ============================================================
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<LotacaoFuncionarioRespostaDto>>
            Editar(
                int id,
                LotacaoFuncionarioDto dto)
        {
            var lotacao = await _context.LotacoesFuncionarios
                .FirstOrDefaultAsync(x => x.Id == id);

            if (lotacao == null)
            {
                return NotFound(new
                {
                    mensagem = "Lotação não encontrada."
                });
            }

            var funcionarioExiste = await _context.Funcionarios
                .AnyAsync(x => x.Id == dto.FuncionarioId);

            if (!funcionarioExiste)
            {
                return BadRequest(new
                {
                    mensagem = "Funcionário não encontrado."
                });
            }

            var funcaoExiste = await _context.FuncoesOperacionais
                .AnyAsync(x => x.Id == dto.FuncaoOperacionalId);

            if (!funcaoExiste)
            {
                return BadRequest(new
                {
                    mensagem = "Função operacional não encontrada."
                });
            }

            if (dto.UnidadeOperacionalId.HasValue)
            {
                var unidade = await _context.UnidadesOperacionais
                    .FirstOrDefaultAsync(x =>
                        x.Id == dto.UnidadeOperacionalId.Value);

                if (unidade == null)
                {
                    return BadRequest(new
                    {
                        mensagem = "Unidade operacional não encontrada."
                    });
                }
            }

            if (dto.EquipaId.HasValue)
            {
                var equipa = await _context.Equipas
                    .FirstOrDefaultAsync(x => x.Id == dto.EquipaId.Value);

                if (equipa == null)
                {
                    return BadRequest(new
                    {
                        mensagem = "Equipa não encontrada."
                    });
                }

                if (dto.UnidadeOperacionalId.HasValue &&
                    equipa.UnidadeOperacionalId != dto.UnidadeOperacionalId.Value)
                {
                    return BadRequest(new
                    {
                        mensagem = "A equipa não pertence à unidade operacional indicada."
                    });
                }
            }

            if (dto.TipoTurnoId.HasValue)
            {
                var turno = await _context.TiposTurno
                    .FirstOrDefaultAsync(x =>
                        x.Id == dto.TipoTurnoId.Value);

                if (turno == null)
                {
                    return BadRequest(new
                    {
                        mensagem = "Tipo de turno não encontrado."
                    });
                }

                if (!turno.Ativo)
                {
                    return BadRequest(new
                    {
                        mensagem = "O tipo de turno está inativo."
                    });
                }
            }

            lotacao.FuncionarioId = dto.FuncionarioId;
            lotacao.SeccaoId = dto.SeccaoId;
            lotacao.SectorId = dto.SectorId;
            lotacao.UnidadeOperacionalId = dto.UnidadeOperacionalId;
            lotacao.EquipaId = dto.EquipaId;
            lotacao.PostoId = dto.PostoId;
            lotacao.FuncaoOperacionalId = dto.FuncaoOperacionalId;
            lotacao.TipoTurnoId = dto.TipoTurnoId;
            lotacao.DataInicio = dto.DataInicio ?? lotacao.DataInicio;
            lotacao.Motivo = dto.Motivo;
            lotacao.Observacao = dto.Observacao;

            await _context.SaveChangesAsync();

            var resultado = await _context.LotacoesFuncionarios
                .AsNoTracking()
                .Include(x => x.Funcionario)
                .Include(x => x.Seccao)
                .Include(x => x.Sector)
                .Include(x => x.UnidadeOperacional)
                .Include(x => x.Equipa)
                .Include(x => x.Posto)
                .Include(x => x.FuncaoOperacional)
                .Include(x => x.TipoTurno)
                .FirstAsync(x => x.Id == id);

            return Ok(ParaDto(resultado));
        }

        // ============================================================
        // POST — TRANSFERIR FUNCIONÁRIO
        // ============================================================
        [HttpPost("funcionario/{funcionarioId:int}/transferir")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<LotacaoFuncionarioRespostaDto>>
            Transferir(
                int funcionarioId,
                TransferirFuncionarioDto dto)
        {
            var funcionario = await _context.Funcionarios
                .FirstOrDefaultAsync(x => x.Id == funcionarioId);

            if (funcionario == null)
            {
                return NotFound(new
                {
                    mensagem = "Funcionário não encontrado."
                });
            }

            var lotacaoAtual = await _context.LotacoesFuncionarios
                .FirstOrDefaultAsync(x =>
                    x.FuncionarioId == funcionarioId &&
                    x.Ativo);

            if (lotacaoAtual == null)
            {
                return BadRequest(new
                {
                    mensagem =
                        "O funcionário não possui uma lotação ativa. " +
                        "Crie primeiro uma lotação."
                });
            }

            // --------------------------------------------------------
            // VALIDAR FUNÇÃO
            // --------------------------------------------------------
            var funcaoExiste = await _context.FuncoesOperacionais
                .AnyAsync(x => x.Id == dto.FuncaoOperacionalId);

            if (!funcaoExiste)
            {
                return BadRequest(new
                {
                    mensagem = "Função operacional não encontrada."
                });
            }

            // --------------------------------------------------------
            // VALIDAR UNIDADE
            // --------------------------------------------------------
            if (dto.UnidadeOperacionalId.HasValue)
            {
                var unidadeExiste = await _context.UnidadesOperacionais
                    .AnyAsync(x =>
                        x.Id == dto.UnidadeOperacionalId.Value);

                if (!unidadeExiste)
                {
                    return BadRequest(new
                    {
                        mensagem = "Unidade operacional não encontrada."
                    });
                }
            }

            // --------------------------------------------------------
            // VALIDAR EQUIPA
            // --------------------------------------------------------
            if (dto.EquipaId.HasValue)
            {
                var equipa = await _context.Equipas
                    .FirstOrDefaultAsync(x =>
                        x.Id == dto.EquipaId.Value);

                if (equipa == null)
                {
                    return BadRequest(new
                    {
                        mensagem = "Equipa não encontrada."
                    });
                }

                if (dto.UnidadeOperacionalId.HasValue &&
                    equipa.UnidadeOperacionalId !=
                    dto.UnidadeOperacionalId.Value)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "A equipa não pertence à unidade operacional indicada."
                    });
                }
            }

            // --------------------------------------------------------
            // VALIDAR TURNO
            // --------------------------------------------------------
            if (dto.TipoTurnoId.HasValue)
            {
                var turno = await _context.TiposTurno
                    .FirstOrDefaultAsync(x =>
                        x.Id == dto.TipoTurnoId.Value);

                if (turno == null)
                {
                    return BadRequest(new
                    {
                        mensagem = "Tipo de turno não encontrado."
                    });
                }

                if (!turno.Ativo)
                {
                    return BadRequest(new
                    {
                        mensagem = "O tipo de turno está inativo."
                    });
                }
            }

            var dataTransferencia =
                dto.DataInicio ?? DateTime.Now;

            // --------------------------------------------------------
            // ENCERRAR LOTAÇÃO ANTERIOR
            // --------------------------------------------------------
            lotacaoAtual.Ativo = false;
            lotacaoAtual.DataFim = dataTransferencia;

            // --------------------------------------------------------
            // CRIAR NOVA LOTAÇÃO
            // --------------------------------------------------------
            var novaLotacao = new LotacaoFuncionario
            {
                FuncionarioId = funcionarioId,

                SeccaoId = dto.SeccaoId,

                SectorId = dto.SectorId,

                UnidadeOperacionalId =
                    dto.UnidadeOperacionalId,

                EquipaId = dto.EquipaId,

                PostoId = dto.PostoId,

                FuncaoOperacionalId =
                    dto.FuncaoOperacionalId,

                TipoTurnoId = dto.TipoTurnoId,

                DataInicio = dataTransferencia,

                DataFim = null,

                Ativo = true,

                Motivo = dto.Motivo,

                Observacao = dto.Observacao
            };

            _context.LotacoesFuncionarios.Add(novaLotacao);

            await _context.SaveChangesAsync();

            // --------------------------------------------------------
            // RECARREGAR
            // --------------------------------------------------------
            var resultado = await _context.LotacoesFuncionarios
                .AsNoTracking()
                .Include(x => x.Funcionario)
                .Include(x => x.Seccao)
                .Include(x => x.Sector)
                .Include(x => x.UnidadeOperacional)
                .Include(x => x.Equipa)
                .Include(x => x.Posto)
                .Include(x => x.FuncaoOperacional)
                .Include(x => x.TipoTurno)
                .FirstAsync(x => x.Id == novaLotacao.Id);

            return Ok(ParaDto(resultado));
        }

        // ============================================================
        // PATCH — ENCERRAR LOTAÇÃO
        // ============================================================
        [HttpPatch("{id:int}/encerrar")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Encerrar(int id)
        {
            var lotacao = await _context.LotacoesFuncionarios
                .FirstOrDefaultAsync(x => x.Id == id);

            if (lotacao == null)
            {
                return NotFound(new
                {
                    mensagem = "Lotação não encontrada."
                });
            }

            if (!lotacao.Ativo)
            {
                return BadRequest(new
                {
                    mensagem = "Esta lotação já está encerrada."
                });
            }

            lotacao.Ativo = false;
            lotacao.DataFim = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Lotação encerrada com sucesso.",
                id = lotacao.Id,
                dataFim = lotacao.DataFim
            });
        }

        // ============================================================
        // PATCH — RETIRAR FUNCIONÁRIO DA EQUIPA / LOTAÇÃO
        // ============================================================
        [HttpPatch("funcionario/{funcionarioId:int}/retirar")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Retirar(int funcionarioId)
        {
            // --------------------------------------------------------
            // VERIFICAR FUNCIONÁRIO
            // --------------------------------------------------------
            var funcionarioExiste = await _context.Funcionarios
                .AnyAsync(x => x.Id == funcionarioId);

            if (!funcionarioExiste)
            {
                return NotFound(new
                {
                    mensagem = "Funcionário não encontrado."
                });
            }

            // --------------------------------------------------------
            // LOCALIZAR LOTAÇÃO ATIVA
            // --------------------------------------------------------
            var lotacaoAtual = await _context.LotacoesFuncionarios
                .FirstOrDefaultAsync(x =>
                    x.FuncionarioId == funcionarioId &&
                    x.Ativo);

            if (lotacaoAtual == null)
            {
                return BadRequest(new
                {
                    mensagem =
                        "O funcionário não possui uma lotação ativa."
                });
            }

            // --------------------------------------------------------
            // ENCERRAR LOTAÇÃO
            // --------------------------------------------------------
            lotacaoAtual.Ativo = false;
            lotacaoAtual.DataFim = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem =
                    "Funcionário retirado da equipa com sucesso.",

                funcionarioId = funcionarioId,

                lotacaoId = lotacaoAtual.Id,

                dataFim = lotacaoAtual.DataFim
            });
        }

        // ============================================================
        // DELETE — REMOVER LOTAÇÃO
        // ============================================================
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var lotacao = await _context.LotacoesFuncionarios
                .FirstOrDefaultAsync(x => x.Id == id);

            if (lotacao == null)
            {
                return NotFound(new
                {
                    mensagem = "Lotação não encontrada."
                });
            }

            // Não apagar histórico de lotações encerradas.
            if (!lotacao.Ativo)
            {
                return BadRequest(new
                {
                    mensagem =
                        "Uma lotação histórica não pode ser eliminada. " +
                        "O histórico deve ser preservado."
                });
            }

            _context.LotacoesFuncionarios.Remove(lotacao);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Lotação eliminada com sucesso."
            });
        }
    }
}