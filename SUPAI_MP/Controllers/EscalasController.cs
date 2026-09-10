using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using supai_mp.Data;
using supai_mp.Models;
using supai_mp.Models.Organizacao;

namespace supai_mp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EscalasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;


    public EscalasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // DTO - CRIAR / EDITAR ESCALA
        // ============================================================

        public class EscalaDto
        {
            public int FuncionarioId { get; set; }

            public DateTime Data { get; set; }

            public TimeSpan HoraInicio { get; set; }

            public TimeSpan HoraFim { get; set; }

            public int TipoTurnoId { get; set; }

            public int? PostoId { get; set; }

            public string? Observacao { get; set; }
        }

        // ============================================================
        // DTO - RESPOSTA DA ESCALA
        // ============================================================

        public class EscalaRespostaDto
        {
            public int Id { get; set; }

            public int FuncionarioId { get; set; }

            public string? Funcionario { get; set; }

            public string? Nip { get; set; }

            public DateTime Data { get; set; }

            public TimeSpan HoraInicio { get; set; }

            public TimeSpan HoraFim { get; set; }

            public int TipoTurnoId { get; set; }

            public string? TipoTurno { get; set; }

            public int? PostoId { get; set; }

            public string? Posto { get; set; }

            public string? Observacao { get; set; }

            public int? SeccaoId { get; set; }

            public string? Seccao { get; set; }

            public int? UnidadeOperacionalId { get; set; }

            public string? UnidadeOperacional { get; set; }

            public int? EquipaId { get; set; }

            public string? Equipa { get; set; }

            public int? FuncaoOperacionalId { get; set; }

            public string? FuncaoOperacional { get; set; }
        }

        // ============================================================
        // DTO - GERAÇÃO AUTOMÁTICA
        // ============================================================

        public class GerarEscalaDto
        {
            public int UnidadeOperacionalId { get; set; }

            public DateTime Data { get; set; }

            public TimeSpan HoraInicio { get; set; }

            public int? TipoTurnoId { get; set; }

            public int? PostoId { get; set; }

            public string? Observacao { get; set; }
        }

        // ============================================================
        // DTO - RESULTADO DE FUNCIONÁRIO NA GERAÇÃO AUTOMÁTICA
        // ============================================================

        public class FuncionarioEscalaResultadoDto
        {
            public int FuncionarioId { get; set; }

            public string? Nome { get; set; }

            public string? Nip { get; set; }

            public string? Funcao { get; set; }

            public string? Equipa { get; set; }

            public int? PostoId { get; set; }

            public string? Posto { get; set; }

            public string? TurnoLotacao { get; set; }
        }

        // ============================================================
        // DTO - RESULTADO DA GERAÇÃO AUTOMÁTICA
        // ============================================================

        public class GerarEscalaResultadoDto
        {
            public string Mensagem { get; set; } = string.Empty;

            public int UnidadeId { get; set; }

            public string? UnidadeNome { get; set; }

            public int GrupoId { get; set; }

            public string? GrupoNome { get; set; }

            public int OrdemRotacao { get; set; }

            public int EquipaId { get; set; }

            public string? EquipaNome { get; set; }

            public DateTime Data { get; set; }

            public TimeSpan HoraInicio { get; set; }

            public TimeSpan HoraFim { get; set; }

            public int TipoTurnoId { get; set; }

            public string? TipoTurnoNome { get; set; }

            public int FuncionariosNaEquipa { get; set; }

            public int FuncionariosComLotacaoValida { get; set; }

            public int QuantidadeFuncionariosComTurnoInvalido { get; set; }

            public int EscalasCriadas { get; set; }

            public int EscalasJaExistentes { get; set; }

            public int QuantidadeFuncionariosComConflito { get; set; }

            public List<FuncionarioEscalaResultadoDto>
                FuncionariosEscalados
            { get; set; }
                = new();

            public List<FuncionarioEscalaResultadoDto>
                FuncionariosJaEscalados
            { get; set; }
                = new();

            public List<FuncionarioEscalaResultadoDto>
                FuncionariosComConflito
            { get; set; }
                = new();

            public List<FuncionarioEscalaResultadoDto>
                FuncionariosComTurnoInvalido
            { get; set; }
                = new();
        }

        // ============================================================
        // CONVERTER ESCALA PARA DTO
        // ============================================================

        private static EscalaRespostaDto ParaDto(
            Escala escala,
            LotacaoFuncionario? lotacao)
        {
            return new EscalaRespostaDto
            {
                Id = escala.Id,

                FuncionarioId = escala.FuncionarioId,

                Funcionario =
                    escala.Funcionario?.NomeCompleto,

                Nip =
                    escala.Funcionario?.Nip,

                Data = escala.Data,

                HoraInicio =
                    escala.HoraInicio,

                HoraFim =
                    escala.HoraFim,

                TipoTurnoId =
                    escala.TipoTurnoId,

                TipoTurno =
                    escala.TipoTurno?.Nome,

                PostoId =
                    escala.PostoId,

                Posto =
                    escala.Posto?.Nome,

                Observacao =
                    escala.Observacao,

                SeccaoId =
                    lotacao?.SeccaoId,

                Seccao =
                    lotacao?.Seccao?.Nome,

                UnidadeOperacionalId =
                    lotacao?.UnidadeOperacionalId,

                UnidadeOperacional =
                    lotacao?.UnidadeOperacional?.Nome,

                EquipaId =
                    lotacao?.EquipaId,

                Equipa =
                    lotacao?.Equipa?.Nome,

                FuncaoOperacionalId =
                    lotacao?.FuncaoOperacionalId,

                FuncaoOperacional =
                    lotacao?.FuncaoOperacional?.Nome
            };
        }

        // ============================================================
        // CARREGAR LOTAÇÕES ATIVAS
        // ============================================================

        private async Task<Dictionary<int, LotacaoFuncionario>>
            CarregarLotacoesAsync(
                IEnumerable<int> funcionariosIds)
        {
            var ids = funcionariosIds
                .Distinct()
                .ToList();

            if (!ids.Any())
            {
                return new Dictionary<int, LotacaoFuncionario>();
            }

            var lotacoes =
                await _context.LotacoesFuncionarios
                    .Include(l => l.Seccao)
                    .Include(l => l.UnidadeOperacional)
                    .Include(l => l.Equipa)
                    .Include(l => l.FuncaoOperacional)
                    .Include(l => l.TipoTurno)
                    .Include(l => l.Funcionario)
                    .Where(l =>
                        l.Ativo &&
                        ids.Contains(l.FuncionarioId))
                    .OrderByDescending(l => l.DataInicio)
                    .ToListAsync();

            return lotacoes
                .GroupBy(l => l.FuncionarioId)
                .ToDictionary(
                    g => g.Key,
                    g => g.First());
        }

        // ============================================================
        // CALCULAR FIM DA ESCALA
        // ============================================================
        //
        // Exemplos:
        //
        // 07:00 -> 15:00 = 8 horas
        // 22:00 -> 07:00 = atravessa a meia-noite
        // 07:00 -> 07:00 = 24 horas
        //
        // ============================================================

        private static DateTime CalcularFim(
            DateTime data,
            TimeSpan horaInicio,
            TimeSpan horaFim)
        {
            var inicio =
                data.Date + horaInicio;

            var fim =
                data.Date + horaFim;

            if (fim <= inicio)
            {
                fim = fim.AddDays(1);
            }

            return fim;
        }

        // ============================================================
        // VALIDAR HORÁRIO
        // ============================================================

        private static bool HoraValida(TimeSpan hora)
        {
            return hora >= TimeSpan.Zero &&
                   hora < TimeSpan.FromDays(1);
        }

        // ============================================================
        // VERIFICAR CONFLITO ENTRE ESCALAS
        // ============================================================

        private static bool ExisteConflito(
            DateTime dataExistente,
            TimeSpan inicioExistente,
            TimeSpan fimExistente,
            DateTime novaData,
            TimeSpan novoInicio,
            TimeSpan novoFim)
        {
            var inicioExistenteDt =
                dataExistente.Date +
                inicioExistente;

            var fimExistenteDt =
                CalcularFim(
                    dataExistente,
                    inicioExistente,
                    fimExistente);

            var novoInicioDt =
                novaData.Date +
                novoInicio;

            var novoFimDt =
                CalcularFim(
                    novaData,
                    novoInicio,
                    novoFim);

            return
                inicioExistenteDt < novoFimDt &&
                fimExistenteDt > novoInicioDt;
        }

        // ============================================================
        // VALIDAR DADOS DA ESCALA
        // ============================================================

        private async Task<(
            bool Valido,
            string? Erro,
            Funcionario? Funcionario,
            TipoTurno? TipoTurno,
            LotacaoFuncionario? Lotacao,
            Posto? Posto)>
            ValidarEscalaAsync(
                EscalaDto dados)
        {
            // --------------------------------------------------------
            // FUNCIONÁRIO
            // --------------------------------------------------------

            var funcionario =
                await _context.Funcionarios
                    .FirstOrDefaultAsync(f =>
                        f.Id == dados.FuncionarioId);

            if (funcionario == null)
            {
                return (
                    false,
                    "Funcionário não encontrado.",
                    null,
                    null,
                    null,
                    null);
            }

            // --------------------------------------------------------
            // TIPO DE TURNO
            // --------------------------------------------------------

            var turno =
                await _context.TiposTurno
                    .FirstOrDefaultAsync(t =>
                        t.Id == dados.TipoTurnoId &&
                        t.Ativo);

            if (turno == null)
            {
                return (
                    false,
                    "Tipo de turno não encontrado ou está inativo.",
                    funcionario,
                    null,
                    null,
                    null);
            }

            // --------------------------------------------------------
            // LOTAÇÃO
            // --------------------------------------------------------

            var lotacao =
                await _context.LotacoesFuncionarios
                    .Include(l => l.Seccao)
                    .Include(l => l.UnidadeOperacional)
                    .Include(l => l.Equipa)
                    .Include(l => l.FuncaoOperacional)
                    .Include(l => l.TipoTurno)
                    .Where(l =>
                        l.FuncionarioId ==
                        dados.FuncionarioId &&
                        l.Ativo)
                    .OrderByDescending(l => l.DataInicio)
                    .FirstOrDefaultAsync();

            if (lotacao == null)
            {
                return (
                    false,
                    "O funcionário não possui uma lotação ativa. Faça primeiro o enquadramento do funcionário.",
                    funcionario,
                    turno,
                    null,
                    null);
            }

            // --------------------------------------------------------
            // FUNÇÃO OPERACIONAL
            // --------------------------------------------------------

            if (lotacao.FuncaoOperacionalId <= 0)
            {
                return (
                    false,
                    "O funcionário possui uma lotação ativa, mas não possui uma função operacional definida.",
                    funcionario,
                    turno,
                    lotacao,
                    null);
            }

            // --------------------------------------------------------
            // TURNO DA LOTAÇÃO
            // --------------------------------------------------------
            //
            // TipoTurnoId é INT no modelo.
            //
            // Portanto NÃO usar:
            // HasValue()
            // Value
            //
            // --------------------------------------------------------

            if (lotacao.TipoTurnoId > 0 &&
                lotacao.TipoTurnoId != dados.TipoTurnoId)
            {
                return (
                    false,
                    $"O funcionário está lotado no regime '{lotacao.TipoTurno?.Nome}', mas a escala utiliza o regime '{turno.Nome}'.",
                    funcionario,
                    turno,
                    lotacao,
                    null);
            }

            // --------------------------------------------------------
            // POSTO
            // --------------------------------------------------------

            Posto? posto = null;

            if (dados.PostoId.HasValue)
            {
                posto =
                    await _context.Postos
                        .FirstOrDefaultAsync(p =>
                            p.Id ==
                            dados.PostoId.Value &&
                            p.Ativo);

                if (posto == null)
                {
                    return (
                        false,
                        "O posto indicado não existe ou está inativo.",
                        funcionario,
                        turno,
                        lotacao,
                        null);
                }

                if (posto.UnidadeOperacionalId.HasValue &&
                    lotacao.UnidadeOperacionalId !=
                    posto.UnidadeOperacionalId)
                {
                    return (
                        false,
                        "O posto selecionado não pertence à mesma unidade operacional da lotação do funcionário.",
                        funcionario,
                        turno,
                        lotacao,
                        posto);
                }
            }

            // --------------------------------------------------------
            // HORÁRIO
            // --------------------------------------------------------

            if (!HoraValida(dados.HoraInicio))
            {
                return (
                    false,
                    "A hora de início é inválida.",
                    funcionario,
                    turno,
                    lotacao,
                    posto);
            }

            if (!HoraValida(dados.HoraFim))
            {
                return (
                    false,
                    "A hora de fim é inválida.",
                    funcionario,
                    turno,
                    lotacao,
                    posto);
            }

            // --------------------------------------------------------
            // INÍCIO = FIM
            // --------------------------------------------------------
            //
            // Permitido apenas quando o turno é de 24h ou mais.
            //
            // Exemplo:
            // 07:00 -> 07:00 = 24 horas
            //
            // --------------------------------------------------------

            if (dados.HoraInicio ==
                dados.HoraFim &&
                turno.HorasTrabalho < 24)
            {
                return (
                    false,
                    "A hora de início e a hora de fim não podem ser iguais para este tipo de turno.",
                    funcionario,
                    turno,
                    lotacao,
                    posto);
            }

            // --------------------------------------------------------
            // DURAÇÃO
            // --------------------------------------------------------

            var inicioDt =
                dados.Data.Date +
                dados.HoraInicio;

            var fimDt =
                CalcularFim(
                    dados.Data,
                    dados.HoraInicio,
                    dados.HoraFim);

            var duracao =
                fimDt - inicioDt;

            if (duracao.TotalHours <= 0)
            {
                return (
                    false,
                    "A duração da escala é inválida.",
                    funcionario,
                    turno,
                    lotacao,
                    posto);
            }

            if (duracao.TotalHours >
                turno.HorasTrabalho + 0.01)
            {
                return (
                    false,
                    $"A duração da escala ({duracao.TotalHours:0.#}h) ultrapassa as {turno.HorasTrabalho} horas previstas no turno.",
                    funcionario,
                    turno,
                    lotacao,
                    posto);
            }

            return (
                true,
                null,
                funcionario,
                turno,
                lotacao,
                posto);
        }

        // ============================================================
        // GET - TODAS AS ESCALAS
        // ============================================================

        [HttpGet]
        public async Task<ActionResult<
            IEnumerable<EscalaRespostaDto>>>
            GetEscalas()
        {
            var escalas =
                await _context.Escalas
                    .AsNoTracking()
                    .Include(e => e.Funcionario)
                    .Include(e => e.TipoTurno)
                    .Include(e => e.Posto)
                    .OrderByDescending(e => e.Data)
                    .ThenBy(e => e.HoraInicio)
                    .ToListAsync();

            var lotacoes =
                await CarregarLotacoesAsync(
                    escalas.Select(e =>
                        e.FuncionarioId));

            var resultado =
                escalas
                    .Select(e =>
                        ParaDto(
                            e,
                            lotacoes.TryGetValue(
                                e.FuncionarioId,
                                out var lotacao)
                                ? lotacao
                                : null))
                    .ToList();

            return Ok(resultado);
        }

        // ============================================================
        // GET - POR ID
        // ============================================================

        [HttpGet("{id:int}")]
        public async Task<ActionResult<
            EscalaRespostaDto>>
            GetEscala(int id)
        {
            var escala =
                await _context.Escalas
                    .AsNoTracking()
                    .Include(e => e.Funcionario)
                    .Include(e => e.TipoTurno)
                    .Include(e => e.Posto)
                    .FirstOrDefaultAsync(e =>
                        e.Id == id);

            if (escala == null)
            {
                return NotFound(new
                {
                    mensagem =
                        "Escala não encontrada."
                });
            }

            var lotacao =
                await _context.LotacoesFuncionarios
                    .AsNoTracking()
                    .Include(l => l.Seccao)
                    .Include(l => l.UnidadeOperacional)
                    .Include(l => l.Equipa)
                    .Include(l => l.FuncaoOperacional)
                    .Include(l => l.TipoTurno)
                    .Where(l =>
                        l.FuncionarioId ==
                        escala.FuncionarioId &&
                        l.Ativo)
                    .OrderByDescending(l =>
                        l.DataInicio)
                    .FirstOrDefaultAsync();

            return Ok(
                ParaDto(
                    escala,
                    lotacao));
        }

        // ============================================================
        // GET - POR DATA
        // ============================================================

        [HttpGet("data/{data}")]
        public async Task<ActionResult<
            IEnumerable<EscalaRespostaDto>>>
            GetPorData(DateTime data)
        {
            var inicio =
                data.Date;

            var fim =
                inicio.AddDays(1);

            var escalas =
                await _context.Escalas
                    .AsNoTracking()
                    .Include(e => e.Funcionario)
                    .Include(e => e.TipoTurno)
                    .Include(e => e.Posto)
                    .Where(e =>
                        e.Data >= inicio &&
                        e.Data < fim)
                    .OrderBy(e => e.HoraInicio)
                    .ThenBy(e =>
                        e.Funcionario!.NomeCompleto)
                    .ToListAsync();

            var lotacoes =
                await CarregarLotacoesAsync(
                    escalas.Select(e =>
                        e.FuncionarioId));

            var resultado =
                escalas
                    .Select(e =>
                        ParaDto(
                            e,
                            lotacoes.TryGetValue(
                                e.FuncionarioId,
                                out var lotacao)
                                ? lotacao
                                : null))
                    .ToList();

            return Ok(resultado);
        }

        // ============================================================
        // GET - HOJE
        // ============================================================

        [HttpGet("hoje")]
        public async Task<ActionResult<
            IEnumerable<EscalaRespostaDto>>>
            GetHoje()
        {
            return await GetPorData(
                DateTime.Today);
        }

        // ============================================================
        // GET - ONTEM
        // ============================================================

        [HttpGet("ontem")]
        public async Task<ActionResult<
            IEnumerable<EscalaRespostaDto>>>
            GetOntem()
        {
            return await GetPorData(
                DateTime.Today.AddDays(-1));
        }

        // ============================================================
        // GET - AMANHÃ
        // ============================================================

        [HttpGet("amanha")]
        public async Task<ActionResult<
            IEnumerable<EscalaRespostaDto>>>
            GetAmanha()
        {
            return await GetPorData(
                DateTime.Today.AddDays(1));
        }

        // ============================================================
        // GET - POR FUNCIONÁRIO
        // ============================================================

        [HttpGet("funcionario/{funcionarioId:int}")]
        public async Task<ActionResult<
            IEnumerable<EscalaRespostaDto>>>
            GetPorFuncionario(
                int funcionarioId)
        {
            var funcionarioExiste =
                await _context.Funcionarios
                    .AnyAsync(f =>
                        f.Id == funcionarioId);

            if (!funcionarioExiste)
            {
                return NotFound(new
                {
                    mensagem =
                        "Funcionário não encontrado."
                });
            }

            var escalas =
                await _context.Escalas
                    .AsNoTracking()
                    .Include(e => e.Funcionario)
                    .Include(e => e.TipoTurno)
                    .Include(e => e.Posto)
                    .Where(e =>
                        e.FuncionarioId ==
                        funcionarioId)
                    .OrderByDescending(e => e.Data)
                    .ThenByDescending(e =>
                        e.HoraInicio)
                    .ToListAsync();

            var lotacao =
                await _context.LotacoesFuncionarios
                    .AsNoTracking()
                    .Include(l => l.Seccao)
                    .Include(l => l.UnidadeOperacional)
                    .Include(l => l.Equipa)
                    .Include(l => l.FuncaoOperacional)
                    .Include(l => l.TipoTurno)
                    .Where(l =>
                        l.FuncionarioId ==
                        funcionarioId &&
                        l.Ativo)
                    .OrderByDescending(l =>
                        l.DataInicio)
                    .FirstOrDefaultAsync();

            var resultado =
                escalas
                    .Select(e =>
                        ParaDto(
                            e,
                            lotacao))
                    .ToList();

            return Ok(resultado);
        }

        // ============================================================
        // GET - POR POSTO
        // ============================================================

        [HttpGet("posto/{postoId:int}")]
        public async Task<ActionResult<
            IEnumerable<EscalaRespostaDto>>>
            GetPorPosto(int postoId)
        {
            var postoExiste =
                await _context.Postos
                    .AnyAsync(p =>
                        p.Id == postoId);

            if (!postoExiste)
            {
                return NotFound(new
                {
                    mensagem =
                        "Posto não encontrado."
                });
            }

            var escalas =
                await _context.Escalas
                    .AsNoTracking()
                    .Include(e => e.Funcionario)
                    .Include(e => e.TipoTurno)
                    .Include(e => e.Posto)
                    .Where(e =>
                        e.PostoId == postoId)
                    .OrderByDescending(e => e.Data)
                    .ThenBy(e => e.HoraInicio)
                    .ToListAsync();

            var lotacoes =
                await CarregarLotacoesAsync(
                    escalas.Select(e =>
                        e.FuncionarioId));

            var resultado =
                escalas
                    .Select(e =>
                        ParaDto(
                            e,
                            lotacoes.TryGetValue(
                                e.FuncionarioId,
                                out var lotacao)
                                ? lotacao
                                : null))
                    .ToList();

            return Ok(resultado);
        }

        // ============================================================
        // GET - POR TIPO DE TURNO
        // ============================================================

        [HttpGet("tipo-turno/{tipoTurnoId:int}")]
        public async Task<ActionResult<
            IEnumerable<EscalaRespostaDto>>>
            GetPorTipoTurno(
                int tipoTurnoId)
        {
            var turnoExiste =
                await _context.TiposTurno
                    .AnyAsync(t =>
                        t.Id == tipoTurnoId);

            if (!turnoExiste)
            {
                return NotFound(new
                {
                    mensagem =
                        "Tipo de turno não encontrado."
                });
            }

            var escalas =
                await _context.Escalas
                    .AsNoTracking()
                    .Include(e => e.Funcionario)
                    .Include(e => e.TipoTurno)
                    .Include(e => e.Posto)
                    .Where(e =>
                        e.TipoTurnoId ==
                        tipoTurnoId)
                    .OrderByDescending(e => e.Data)
                    .ThenBy(e => e.HoraInicio)
                    .ToListAsync();

            var lotacoes =
                await CarregarLotacoesAsync(
                    escalas.Select(e =>
                        e.FuncionarioId));

            var resultado =
                escalas
                    .Select(e =>
                        ParaDto(
                            e,
                            lotacoes.TryGetValue(
                                e.FuncionarioId,
                                out var lotacao)
                                ? lotacao
                                : null))
                    .ToList();

            return Ok(resultado);
        }

        // ============================================================
        // POST - CRIAR ESCALA MANUAL
        // ============================================================

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<
            EscalaRespostaDto>>
            CriarEscala(
                [FromBody] EscalaDto dados)
        {
            if (dados == null)
            {
                return BadRequest(new
                {
                    mensagem =
                        "Os dados da escala são obrigatórios."
                });
            }

            var validacao =
                await ValidarEscalaAsync(dados);

            if (!validacao.Valido)
            {
                return BadRequest(new
                {
                    mensagem =
                        validacao.Erro
                });
            }

            var escalasFuncionario =
                await _context.Escalas
                    .Where(e =>
                        e.FuncionarioId ==
                        dados.FuncionarioId)
                    .ToListAsync();

            foreach (var existente
                in escalasFuncionario)
            {
                if (ExisteConflito(
                    existente.Data,
                    existente.HoraInicio,
                    existente.HoraFim,
                    dados.Data,
                    dados.HoraInicio,
                    dados.HoraFim))
                {
                    return Conflict(new
                    {
                        mensagem =
                            "O funcionário já possui uma escala que entra em conflito com este horário.",

                        escalaId =
                            existente.Id
                    });
                }
            }

            var escala =
                new Escala
                {
                    FuncionarioId =
                        dados.FuncionarioId,

                    Data =
                        dados.Data.Date,

                    HoraInicio =
                        dados.HoraInicio,

                    HoraFim =
                        dados.HoraFim,

                    TipoTurnoId =
                        dados.TipoTurnoId,

                    PostoId =
                        dados.PostoId,

                    Observacao =
                        dados.Observacao
                };

            _context.Escalas.Add(escala);

            await _context.SaveChangesAsync();

            escala =
                await _context.Escalas
                    .AsNoTracking()
                    .Include(e => e.Funcionario)
                    .Include(e => e.TipoTurno)
                    .Include(e => e.Posto)
                    .FirstAsync(e =>
                        e.Id == escala.Id);

            return CreatedAtAction(
                nameof(GetEscala),
                new
                {
                    id = escala.Id
                },
                ParaDto(
                    escala,
                    validacao.Lotacao));
        }

        // ============================================================
        // PUT - EDITAR ESCALA
        // ============================================================

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<
            EscalaRespostaDto>>
            EditarEscala(
                int id,
                [FromBody] EscalaDto dados)
        {
            if (dados == null)
            {
                return BadRequest(new
                {
                    mensagem =
                        "Os dados da escala são obrigatórios."
                });
            }

            var escala =
                await _context.Escalas
                    .FirstOrDefaultAsync(e =>
                        e.Id == id);

            if (escala == null)
            {
                return NotFound(new
                {
                    mensagem =
                        "Escala não encontrada."
                });
            }

            var validacao =
                await ValidarEscalaAsync(dados);

            if (!validacao.Valido)
            {
                return BadRequest(new
                {
                    mensagem =
                        validacao.Erro
                });
            }

            var outrasEscalas =
                await _context.Escalas
                    .Where(e =>
                        e.FuncionarioId ==
                        dados.FuncionarioId &&
                        e.Id != id)
                    .ToListAsync();

            foreach (var existente
                in outrasEscalas)
            {
                if (ExisteConflito(
                    existente.Data,
                    existente.HoraInicio,
                    existente.HoraFim,
                    dados.Data,
                    dados.HoraInicio,
                    dados.HoraFim))
                {
                    return Conflict(new
                    {
                        mensagem =
                            "O funcionário já possui outra escala que entra em conflito com este horário.",

                        escalaId =
                            existente.Id
                    });
                }
            }

            escala.FuncionarioId =
                dados.FuncionarioId;

            escala.Data =
                dados.Data.Date;

            escala.HoraInicio =
                dados.HoraInicio;

            escala.HoraFim =
                dados.HoraFim;

            escala.TipoTurnoId =
                dados.TipoTurnoId;

            escala.PostoId =
                dados.PostoId;

            escala.Observacao =
                dados.Observacao;

            await _context.SaveChangesAsync();

            escala =
                await _context.Escalas
                    .AsNoTracking()
                    .Include(e => e.Funcionario)
                    .Include(e => e.TipoTurno)
                    .Include(e => e.Posto)
                    .FirstAsync(e =>
                        e.Id == id);

            return Ok(
                ParaDto(
                    escala,
                    validacao.Lotacao));
        }

        // ============================================================
        // DELETE - ELIMINAR ESCALA
        // ============================================================

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult>
            ApagarEscala(int id)
        {
            var escala =
                await _context.Escalas
                    .FirstOrDefaultAsync(e =>
                        e.Id == id);

            if (escala == null)
            {
                return NotFound(new
                {
                    mensagem =
                        "Escala não encontrada."
                });
            }

            _context.Escalas.Remove(escala);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem =
                    "Escala eliminada com sucesso."
            });
        }

        // ============================================================
        // POST - GERAR ESCALA AUTOMÁTICA
        // ============================================================
        //
        // ROTA:
        //
        // POST /api/Escalas/gerar-automatica
        //
        // A geração:
        //
        // 1. Identifica a unidade;
        // 2. Obtém os grupos ativos;
        // 3. Calcula o grupo em serviço;
        // 4. Obtém a equipa correspondente;
        // 5. Busca SOMENTE funcionários lotados nessa equipa;
        // 6. Não distribui aleatoriamente;
        // 7. Valida o turno;
        // 8. Verifica conflitos;
        // 9. Cria somente as escalas necessárias.
        //
        // ============================================================

        [HttpPost("gerar-automatica")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<
            GerarEscalaResultadoDto>>
            GerarEscalaAutomatica(
                [FromBody] GerarEscalaDto dados)
        {
            // --------------------------------------------------------
            // VALIDAR DADOS
            // --------------------------------------------------------

            if (dados == null)
            {
                return BadRequest(new
                {
                    mensagem =
                        "Os dados para geração da escala são obrigatórios."
                });
            }

            if (dados.UnidadeOperacionalId <= 0)
            {
                return BadRequest(new
                {
                    mensagem =
                        "É necessário indicar a unidade operacional."
                });
            }

            // --------------------------------------------------------
            // VALIDAR DATA
            // --------------------------------------------------------

            if (dados.Data == default)
            {
                return BadRequest(new
                {
                    mensagem =
                        "É necessário indicar a data da escala."
                });
            }

            // --------------------------------------------------------
            // VALIDAR HORA
            // --------------------------------------------------------

            if (!HoraValida(dados.HoraInicio))
            {
                return BadRequest(new
                {
                    mensagem =
                        "A hora de início indicada é inválida."
                });
            }

            // --------------------------------------------------------
            // UNIDADE OPERACIONAL
            // --------------------------------------------------------

            var unidade =
                await _context.UnidadesOperacionais
                    .FirstOrDefaultAsync(u =>
                        u.Id ==
                        dados.UnidadeOperacionalId &&
                        u.Ativo);

            if (unidade == null)
            {
                return NotFound(new
                {
                    mensagem =
                        "Unidade operacional não encontrada ou está inativa."
                });
            }

            // --------------------------------------------------------
            // GRUPOS ATIVOS
            // --------------------------------------------------------

            var grupos =
                await _context.GruposEscala
                    .Include(g => g.Equipa)
                    .Include(g => g.TipoTurno)
                    .Where(g =>
                        g.UnidadeOperacionalId ==
                        dados.UnidadeOperacionalId &&
                        g.Ativo)
                    .OrderBy(g =>
                        g.OrdemRotacao)
                    .ToListAsync();

            if (!grupos.Any())
            {
                return BadRequest(new
                {
                    mensagem =
                        "Não existem grupos de escala ativos configurados para esta unidade."
                });
            }

            // --------------------------------------------------------
            // VALIDAR ORDEM DA ROTAÇÃO
            // --------------------------------------------------------
            //
            // Exemplo correto:
            //
            // 1 = Charlie
            // 2 = Alfa
            // 3 = Beta
            //
            // ou:
            //
            // 1 = Alfa
            // 2 = Charlie
            // 3 = Beta
            //
            // Não pode haver:
            //
            // 1, 2, 4
            //
            // --------------------------------------------------------

            var ordensEsperadas =
                Enumerable
                    .Range(1, grupos.Count)
                    .ToHashSet();

            var ordensExistentes =
                grupos
                    .Select(g =>
                        g.OrdemRotacao)
                    .ToHashSet();

            if (!ordensEsperadas
                .SetEquals(ordensExistentes))
            {
                return BadRequest(new
                {
                    mensagem =
                        "A configuração dos grupos de rotação é inválida. As ordens devem ser sequenciais começando em 1."
                });
            }

            // --------------------------------------------------------
            // GRUPO DE REFERÊNCIA
            // --------------------------------------------------------

            var grupoReferencia =
                grupos.First();

            // --------------------------------------------------------
            // CALCULAR DIA DA ROTAÇÃO
            // --------------------------------------------------------

            var dias =
                (dados.Data.Date -
                 grupoReferencia.DataReferencia.Date)
                .Days;

            var quantidadeGrupos =
                grupos.Count;

            var indiceRotacao =
                ((dias % quantidadeGrupos) +
                 quantidadeGrupos) %
                quantidadeGrupos;

            var ordemEmServico =
                indiceRotacao + 1;

            // --------------------------------------------------------
            // LOCALIZAR GRUPO EM SERVIÇO
            // --------------------------------------------------------

            var grupoServico =
                grupos.FirstOrDefault(g =>
                    g.OrdemRotacao ==
                    ordemEmServico);

            if (grupoServico == null)
            {
                return BadRequest(new
                {
                    mensagem =
                        "Não foi possível determinar o grupo em serviço para a data indicada."
                });
            }

            // --------------------------------------------------------
            // VALIDAR EQUIPA
            // --------------------------------------------------------

            if (grupoServico.Equipa == null)
            {
                return BadRequest(new
                {
                    mensagem =
                        $"O grupo '{grupoServico.Nome}' não possui uma equipa associada."
                });
            }

            if (grupoServico.Equipa.UnidadeOperacionalId !=
                dados.UnidadeOperacionalId)
            {
                return BadRequest(new
                {
                    mensagem =
                        "A equipa do grupo de escala não pertence à unidade operacional selecionada."
                });
            }

            // --------------------------------------------------------
            // TIPO DE TURNO
            // --------------------------------------------------------

            var tipoTurnoId =
                dados.TipoTurnoId ??
                grupoServico.TipoTurnoId;

            // --------------------------------------------------------
            // NÃO PERMITIR TURNO DIFERENTE DO GRUPO
            // --------------------------------------------------------

            if (dados.TipoTurnoId.HasValue &&
                dados.TipoTurnoId.Value !=
                grupoServico.TipoTurnoId)
            {
                return BadRequest(new
                {
                    mensagem =
                        $"O grupo '{grupoServico.Nome}' está configurado para o turno '{grupoServico.TipoTurno?.Nome}', mas foi solicitado outro tipo de turno."
                });
            }

            // --------------------------------------------------------
            // CARREGAR TURNO
            // --------------------------------------------------------

            var turno =
                await _context.TiposTurno
                    .FirstOrDefaultAsync(t =>
                        t.Id == tipoTurnoId &&
                        t.Ativo);

            if (turno == null)
            {
                return BadRequest(new
                {
                    mensagem =
                        "O tipo de turno configurado para o grupo não existe ou está inativo."
                });
            }

            // --------------------------------------------------------
            // VALIDAR HORAS DO TURNO
            // --------------------------------------------------------

            if (turno.HorasTrabalho <= 0)
            {
                return BadRequest(new
                {
                    mensagem =
                        $"O turno '{turno.Nome}' possui uma duração inválida."
                });
            }

            // --------------------------------------------------------
            // CALCULAR HORA DE FIM
            // --------------------------------------------------------
            //
            // Exemplo:
            //
            // início 07:00
            // turno 24h
            //
            // resultado:
            //
            // 07:00 -> 07:00
            //
            // O CalcularFim() interpreta isso como 24 horas.
            //
            // --------------------------------------------------------

            var horaInicio =
                dados.HoraInicio;

            var horaFim =
                horaInicio.Add(
                    TimeSpan.FromHours(
                        turno.HorasTrabalho));

            // TimeSpan representa apenas a hora dentro do dia.
            // Fazemos módulo de 24h para obter a hora final.
            horaFim =
                TimeSpan.FromTicks(
                    horaFim.Ticks %
                    TimeSpan.FromDays(1).Ticks);

            // --------------------------------------------------------
            // LOTAÇÕES DA EQUIPA
            // --------------------------------------------------------
            //
            // A DISTRIBUIÇÃO MANUAL é a fonte de verdade.
            //
            // Não existe qualquer Random() aqui.
            //
            // --------------------------------------------------------

            var lotacoes =
                await _context.LotacoesFuncionarios
                    .Include(l => l.Funcionario)
                    .Include(l => l.FuncaoOperacional)
                    .Include(l => l.TipoTurno)
                    .Include(l => l.Equipa)
                    .Include(l => l.Posto)
                    .Where(l =>
                        l.Ativo &&
                        l.UnidadeOperacionalId ==
                        dados.UnidadeOperacionalId &&
                        l.EquipaId ==
                        grupoServico.EquipaId)
                    .OrderBy(l =>
                        l.Funcionario!.NomeCompleto)
                    .ToListAsync();

            if (!lotacoes.Any())
            {
                var respostaSemFuncionarios =
     new GerarEscalaResultadoDto
     {
         Mensagem =
             $"O grupo {grupoServico.Nome} ({grupoServico.Equipa.Nome}) está de serviço, mas não existem funcionários com lotação ativa nessa equipa.",

         UnidadeId =
             unidade.Id,

         UnidadeNome =
             unidade.Nome,

         GrupoId =
             grupoServico.Id,

         GrupoNome =
             grupoServico.Nome,

         OrdemRotacao =
             grupoServico.OrdemRotacao,

         EquipaId =
             grupoServico.EquipaId,

         EquipaNome =
             grupoServico.Equipa.Nome,

         Data =
             dados.Data.Date,

         HoraInicio =
             horaInicio,

         HoraFim =
             horaFim,

         TipoTurnoId =
             turno.Id,

         TipoTurnoNome =
             turno.Nome,

         FuncionariosNaEquipa =
             0,

         FuncionariosComLotacaoValida =
             0,

         QuantidadeFuncionariosComTurnoInvalido =
             0,

         EscalasCriadas =
             0,

         EscalasJaExistentes =
             0,

         QuantidadeFuncionariosComConflito =
             0,

         FuncionariosEscalados =
             new List<FuncionarioEscalaResultadoDto>(),

         FuncionariosJaEscalados =
             new List<FuncionarioEscalaResultadoDto>(),

         FuncionariosComConflito =
             new List<FuncionarioEscalaResultadoDto>(),

         FuncionariosComTurnoInvalido =
             new List<FuncionarioEscalaResultadoDto>()
     };

                return Ok(
                    respostaSemFuncionarios);
            }

            // --------------------------------------------------------
            // SEPARAR LOTAÇÕES COM TURNO INVÁLIDO
            // --------------------------------------------------------

            var lotacoesComTurnoInvalido =
                lotacoes
                    .Where(l =>
                        l.TipoTurnoId > 0 &&
                        l.TipoTurnoId !=
                        tipoTurnoId)
                    .ToList();

            var lotacoesValidas =
                lotacoes
                    .Where(l =>
                        l.TipoTurnoId <= 0 ||
                        l.TipoTurnoId ==
                        tipoTurnoId)
                    .ToList();

            // --------------------------------------------------------
            // RESULTADOS
            // --------------------------------------------------------

            var funcionariosEscalados =
                new List<FuncionarioEscalaResultadoDto>();

            var funcionariosJaEscalados =
                new List<FuncionarioEscalaResultadoDto>();

            var funcionariosComConflito =
                new List<FuncionarioEscalaResultadoDto>();

            var funcionariosComTurnoInvalido =
                lotacoesComTurnoInvalido
                    .Select(l =>
                        new FuncionarioEscalaResultadoDto
                        {
                            FuncionarioId =
                                l.FuncionarioId,

                            Nome =
                                l.Funcionario?.NomeCompleto,

                            Nip =
                                l.Funcionario?.Nip,

                            Funcao =
                                l.FuncaoOperacional?.Nome,

                            Equipa =
                                l.Equipa?.Nome,

                            PostoId =
                                l.PostoId,

                            Posto =
                                l.Posto?.Nome,

                            TurnoLotacao =
                                l.TipoTurno?.Nome
                        })
                    .ToList();

            // --------------------------------------------------------
            // FUNCIONÁRIOS
            // --------------------------------------------------------

            var funcionarioIds =
                lotacoesValidas
                    .Select(l =>
                        l.FuncionarioId)
                    .Distinct()
                    .ToList();

            // --------------------------------------------------------
            // ESCALAS EXISTENTES
            // --------------------------------------------------------

            var escalasExistentes =
                await _context.Escalas
                    .Include(e => e.Funcionario)
                    .Where(e =>
                        funcionarioIds.Contains(
                            e.FuncionarioId))
                    .ToListAsync();

            // --------------------------------------------------------
            // PROCESSAR FUNCIONÁRIOS
            // --------------------------------------------------------

            foreach (var lotacao
                in lotacoesValidas)
            {
                var funcionario =
                    lotacao.Funcionario;

                if (funcionario == null)
                {
                    continue;
                }

                var resultadoFuncionario =
                    new FuncionarioEscalaResultadoDto
                    {
                        FuncionarioId =
                            lotacao.FuncionarioId,

                        Nome =
                            funcionario.NomeCompleto,

                        Nip =
                            funcionario.Nip,

                        Funcao =
                            lotacao.FuncaoOperacional?.Nome,

                        Equipa =
                            lotacao.Equipa?.Nome,

                        PostoId =
                            lotacao.PostoId,

                        Posto =
                            lotacao.Posto?.Nome,

                        TurnoLotacao =
                            lotacao.TipoTurno?.Nome
                    };

                // ----------------------------------------------------
                // VERIFICAR CONFLITO
                // ----------------------------------------------------

                var conflito =
                    escalasExistentes.Any(e =>
                        e.FuncionarioId ==
                        lotacao.FuncionarioId &&
                        ExisteConflito(
                            e.Data,
                            e.HoraInicio,
                            e.HoraFim,
                            dados.Data.Date,
                            horaInicio,
                            horaFim));

                if (conflito)
                {
                    funcionariosComConflito.Add(
                        resultadoFuncionario);

                    continue;
                }

                // ----------------------------------------------------
                // CRIAR ESCALA
                // ----------------------------------------------------

                var escala =
                    new Escala
                    {
                        FuncionarioId =
                            lotacao.FuncionarioId,

                        Data =
                            dados.Data.Date,

                        HoraInicio =
                            horaInicio,

                        HoraFim =
                            horaFim,

                        TipoTurnoId =
                            turno.Id,

                        PostoId =
                            dados.PostoId ??
                            lotacao.PostoId,

                        Observacao =
                            string.IsNullOrWhiteSpace(
                                dados.Observacao)
                                ? $"Escala automática - {grupoServico.Nome} - {grupoServico.Equipa.Nome}"
                                : dados.Observacao
                    };

                _context.Escalas.Add(escala);

                // ----------------------------------------------------
                // IMPORTANTE
                // ----------------------------------------------------
                //
                // Adicionamos a nova escala à lista local.
                //
                // Isso evita que, caso a mesma geração encontre
                // duas lotações para o mesmo funcionário, ele seja
                // criado duas vezes.
                //
                // ----------------------------------------------------

                escalasExistentes.Add(escala);

                funcionariosEscalados.Add(
                    resultadoFuncionario);
            }

            // --------------------------------------------------------
            // SALVAR
            // --------------------------------------------------------

            await _context.SaveChangesAsync();

            // --------------------------------------------------------
            // RESULTADO FINAL
            // --------------------------------------------------------

            var resultado =
                new GerarEscalaResultadoDto
                {
                    Mensagem =
                        $"Geração da escala concluída para a unidade '{unidade.Nome}'.",

                    UnidadeId =
                        unidade.Id,

                    UnidadeNome =
                        unidade.Nome,

                    GrupoId =
                        grupoServico.Id,

                    GrupoNome =
                        grupoServico.Nome,

                    OrdemRotacao =
                        grupoServico.OrdemRotacao,

                    EquipaId =
                        grupoServico.EquipaId,

                    EquipaNome =
                        grupoServico.Equipa.Nome,

                    Data =
                        dados.Data.Date,

                    HoraInicio =
                        horaInicio,

                    HoraFim =
                        horaFim,

                    TipoTurnoId =
                        turno.Id,

                    TipoTurnoNome =
                        turno.Nome,

                    FuncionariosNaEquipa =
                        lotacoes.Count,

                    FuncionariosComLotacaoValida =
                        lotacoesValidas.Count,

                    QuantidadeFuncionariosComTurnoInvalido =
    funcionariosComTurnoInvalido.Count,

                    EscalasCriadas =
                        funcionariosEscalados.Count,

                    EscalasJaExistentes =
                        funcionariosJaEscalados.Count,

                    QuantidadeFuncionariosComConflito =
                        funcionariosComConflito.Count,

                    FuncionariosEscalados =
                        funcionariosEscalados,

                    FuncionariosJaEscalados =
                        funcionariosJaEscalados,

                    FuncionariosComConflito =
                        funcionariosComConflito,

                    FuncionariosComTurnoInvalido =
                        funcionariosComTurnoInvalido
                };

            return Ok(resultado);
        }
    }


}
