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
        // DTO - RESPOSTA
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
        // CONVERSÃO PARA DTO
        // ============================================================

        private static EscalaRespostaDto ParaDto(
            Escala escala,
            LotacaoFuncionario? lotacao)
        {
            return new EscalaRespostaDto
            {
                Id = escala.Id,

                FuncionarioId = escala.FuncionarioId,

                Funcionario = escala.Funcionario?.NomeCompleto,

                Nip = escala.Funcionario?.Nip,

                Data = escala.Data,

                HoraInicio = escala.HoraInicio,

                HoraFim = escala.HoraFim,

                TipoTurnoId = escala.TipoTurnoId,

                TipoTurno = escala.TipoTurno?.Nome,

                PostoId = escala.PostoId,

                Posto = escala.Posto?.Nome,

                Observacao = escala.Observacao,

                SeccaoId = lotacao?.SeccaoId,

                Seccao = lotacao?.Seccao?.Nome,

                UnidadeOperacionalId = lotacao?.UnidadeOperacionalId,

                UnidadeOperacional = lotacao?.UnidadeOperacional?.Nome,

                EquipaId = lotacao?.EquipaId,

                Equipa = lotacao?.Equipa?.Nome,

                FuncaoOperacionalId = lotacao?.FuncaoOperacionalId,

                FuncaoOperacional = lotacao?.FuncaoOperacional?.Nome
            };
        }

        // ============================================================
        // CARREGAR LOTAÇÕES ATIVAS
        // ============================================================

        private async Task<Dictionary<int, LotacaoFuncionario>>
            CarregarLotacoesAsync(IEnumerable<int> funcionariosIds)
        {
            var ids = funcionariosIds
                .Distinct()
                .ToList();

            if (!ids.Any())
                return new Dictionary<int, LotacaoFuncionario>();

            var lotacoes = await _context.LotacoesFuncionarios
                .Include(l => l.Seccao)
                .Include(l => l.UnidadeOperacional)
                .Include(l => l.Equipa)
                .Include(l => l.FuncaoOperacional)
                .Include(l => l.TipoTurno)
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
        // CALCULAR DATA/HORA FINAL
        // ============================================================

        private static DateTime CalcularFim(
            DateTime data,
            TimeSpan horaInicio,
            TimeSpan horaFim)
        {
            var inicio = data.Date + horaInicio;

            var fim = data.Date + horaFim;

            // Se terminar antes ou exatamente na hora de início,
            // significa que terminou no dia seguinte.
            if (fim <= inicio)
            {
                fim = fim.AddDays(1);
            }

            return fim;
        }

        // ============================================================
        // VALIDAR INTERVALO
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
                dataExistente.Date + inicioExistente;

            var fimExistenteDt =
                CalcularFim(
                    dataExistente,
                    inicioExistente,
                    fimExistente);

            var novoInicioDt =
                novaData.Date + novoInicio;

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

        private async Task<(bool Valido, string? Erro,
            Funcionario? Funcionario,
            TipoTurno? TipoTurno,
            LotacaoFuncionario? Lotacao,
            Posto? Posto)>
            ValidarEscalaAsync(EscalaDto dados)
        {
            var funcionario = await _context.Funcionarios
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

            var turno = await _context.TiposTurno
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

            var lotacao = await _context.LotacoesFuncionarios
                .Include(l => l.Seccao)
                .Include(l => l.UnidadeOperacional)
                .Include(l => l.Equipa)
                .Include(l => l.FuncaoOperacional)
                .Include(l => l.TipoTurno)
                .Where(l =>
                    l.FuncionarioId == dados.FuncionarioId &&
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
            // VALIDAR TURNO DA LOTAÇÃO
            // --------------------------------------------------------

            if (lotacao.TipoTurnoId.HasValue &&
                lotacao.TipoTurnoId.Value != dados.TipoTurnoId)
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
            // VALIDAR POSTO
            // --------------------------------------------------------

            Posto? posto = null;

            if (dados.PostoId.HasValue)
            {
                posto = await _context.Postos
                    .FirstOrDefaultAsync(p =>
                        p.Id == dados.PostoId.Value &&
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
            // VALIDAR HORAS
            // --------------------------------------------------------

            var inicio = dados.HoraInicio;

            var fim = dados.HoraFim;

            if (inicio < TimeSpan.Zero ||
                inicio >= TimeSpan.FromDays(1))
            {
                return (
                    false,
                    "A hora de início é inválida.",
                    funcionario,
                    turno,
                    lotacao,
                    posto);
            }

            if (fim < TimeSpan.Zero ||
                fim >= TimeSpan.FromDays(1))
            {
                return (
                    false,
                    "A hora de fim é inválida.",
                    funcionario,
                    turno,
                    lotacao,
                    posto);
            }

            // Para turnos de 24 horas,
            // 08:00 -> 08:00 significa 24 horas.
            if (inicio == fim &&
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
            // VALIDAR DURAÇÃO
            // --------------------------------------------------------

            var inicioDt =
                dados.Data.Date + inicio;

            var fimDt =
                CalcularFim(
                    dados.Data,
                    inicio,
                    fim);

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
        public async Task<ActionResult<IEnumerable<EscalaRespostaDto>>> GetEscalas()
        {
            var escalas = await _context.Escalas
                .Include(e => e.Funcionario)
                .Include(e => e.TipoTurno)
                .Include(e => e.Posto)
                .OrderBy(e => e.Data)
                .ThenBy(e => e.HoraInicio)
                .ToListAsync();

            var lotacoes =
                await CarregarLotacoesAsync(
                    escalas.Select(e => e.FuncionarioId));

            var resultado = escalas
                .Select(e =>
                    ParaDto(
                        e,
                        lotacoes.TryGetValue(
                            e.FuncionarioId,
                            out var l)
                            ? l
                            : null))
                .ToList();

            return Ok(resultado);
        }

        // ============================================================
        // GET - POR ID
        // ============================================================

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EscalaRespostaDto>> GetEscala(int id)
        {
            var escala = await _context.Escalas
                .Include(e => e.Funcionario)
                .Include(e => e.TipoTurno)
                .Include(e => e.Posto)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (escala == null)
                return NotFound(new
                {
                    mensagem = "Escala não encontrada."
                });

            var lotacao =
                await _context.LotacoesFuncionarios
                    .Include(l => l.Seccao)
                    .Include(l => l.UnidadeOperacional)
                    .Include(l => l.Equipa)
                    .Include(l => l.FuncaoOperacional)
                    .FirstOrDefaultAsync(l =>
                        l.FuncionarioId ==
                        escala.FuncionarioId &&
                        l.Ativo);

            return Ok(
                ParaDto(
                    escala,
                    lotacao));
        }

        // ============================================================
        // GET - POR DATA
        // ============================================================

        [HttpGet("data/{data}")]
        public async Task<ActionResult<IEnumerable<EscalaRespostaDto>>>
    GetPorData(DateTime data)
        {
            var dia = data.Date;

            var escalas = await _context.Escalas
                .Include(e => e.Funcionario)
                .Include(e => e.TipoTurno)
                .Include(e => e.Posto)
                .Where(e => e.Data.Date == dia)
                .OrderBy(e => e.HoraInicio)
                .ThenBy(e => e.Funcionario!.NomeCompleto)
                .ToListAsync();

            var lotacoes =
                await CarregarLotacoesAsync(
                    escalas.Select(e => e.FuncionarioId));

            var resultado = escalas
                .Select(e =>
                {
                    lotacoes.TryGetValue(
                        e.FuncionarioId,
                        out var lotacao);

                    return ParaDto(
                        e,
                        lotacao);
                })
                .ToList();

            // ========================================================
            // DIAGNÓSTICO
            // ========================================================

            Console.WriteLine(
                "====================================================");

            Console.WriteLine(
                $"ESCALAS DA DATA: {dia:yyyy-MM-dd}");

            Console.WriteLine(
                $"TOTAL DE ESCALAS: {resultado.Count}");

            Console.WriteLine(
                $"ESCOLTA A: {resultado.Count(x =>
                    string.Equals(
                        x.UnidadeOperacional?.Trim(),
                        "Escolta A",
                        StringComparison.OrdinalIgnoreCase))}");

            Console.WriteLine(
                $"ESCOLTA C: {resultado.Count(x =>
                    string.Equals(
                        x.UnidadeOperacional?.Trim(),
                        "Escolta C",
                        StringComparison.OrdinalIgnoreCase))}");

            var unidades =
                resultado
                    .Select(x => x.UnidadeOperacional)
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!.Trim())
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

            Console.WriteLine(
                "UNIDADES OPERACIONAIS DEVOLVIDAS:");

            foreach (var unidade in unidades)
            {
                Console.WriteLine(
                    $" -> [{unidade}]");
            }

            Console.WriteLine(
                "====================================================");

            return Ok(resultado);
        }

        // ============================================================
        // GET - HOJE
        // ============================================================

        [HttpGet("hoje")]
        public async Task<ActionResult<IEnumerable<EscalaRespostaDto>>>
            GetHoje()
        {
            return await GetPorData(DateTime.Today);
        }

        // ============================================================
        // GET - ONTEM
        // ============================================================

        [HttpGet("ontem")]
        public async Task<ActionResult<IEnumerable<EscalaRespostaDto>>>
            GetOntem()
        {
            return await GetPorData(
                DateTime.Today.AddDays(-1));
        }

        // ============================================================
        // GET - AMANHÃ
        // ============================================================

        [HttpGet("amanha")]
        public async Task<ActionResult<IEnumerable<EscalaRespostaDto>>>
            GetAmanha()
        {
            return await GetPorData(
                DateTime.Today.AddDays(1));
        }

        // ============================================================
        // GET - POR FUNCIONÁRIO
        // ============================================================

        [HttpGet("funcionario/{funcionarioId:int}")]
        public async Task<ActionResult<IEnumerable<EscalaRespostaDto>>>
            GetPorFuncionario(int funcionarioId)
        {
            var escalas = await _context.Escalas
                .Include(e => e.Funcionario)
                .Include(e => e.TipoTurno)
                .Include(e => e.Posto)
                .Where(e =>
                    e.FuncionarioId == funcionarioId)
                .OrderByDescending(e => e.Data)
                .ThenBy(e => e.HoraInicio)
                .ToListAsync();

            var lotacao =
                await _context.LotacoesFuncionarios
                    .Include(l => l.Seccao)
                    .Include(l => l.UnidadeOperacional)
                    .Include(l => l.Equipa)
                    .Include(l => l.FuncaoOperacional)
                    .FirstOrDefaultAsync(l =>
                        l.FuncionarioId == funcionarioId &&
                        l.Ativo);

            return Ok(
                escalas.Select(e =>
                    ParaDto(e, lotacao)));
        }

        // ============================================================
        // GET - POR POSTO
        // ============================================================

        [HttpGet("posto/{postoId:int}")]
        public async Task<ActionResult<IEnumerable<EscalaRespostaDto>>>
            GetPorPosto(int postoId)
        {
            var escalas = await _context.Escalas
                .Include(e => e.Funcionario)
                .Include(e => e.TipoTurno)
                .Include(e => e.Posto)
                .Where(e => e.PostoId == postoId)
                .OrderByDescending(e => e.Data)
                .ThenBy(e => e.HoraInicio)
                .ToListAsync();

            var lotacoes =
                await CarregarLotacoesAsync(
                    escalas.Select(e => e.FuncionarioId));

            return Ok(
                escalas.Select(e =>
                    ParaDto(
                        e,
                        lotacoes.TryGetValue(
                            e.FuncionarioId,
                            out var l)
                            ? l
                            : null)));
        }

        // ============================================================
        // GET - POR TIPO DE TURNO
        // ============================================================

        [HttpGet("tipo-turno/{tipoTurnoId:int}")]
        public async Task<ActionResult<IEnumerable<EscalaRespostaDto>>>
            GetPorTipoTurno(int tipoTurnoId)
        {
            var escalas = await _context.Escalas
                .Include(e => e.Funcionario)
                .Include(e => e.TipoTurno)
                .Include(e => e.Posto)
                .Where(e =>
                    e.TipoTurnoId == tipoTurnoId)
                .OrderByDescending(e => e.Data)
                .ThenBy(e => e.HoraInicio)
                .ToListAsync();

            var lotacoes =
                await CarregarLotacoesAsync(
                    escalas.Select(e => e.FuncionarioId));

            return Ok(
                escalas.Select(e =>
                    ParaDto(
                        e,
                        lotacoes.TryGetValue(
                            e.FuncionarioId,
                            out var l)
                            ? l
                            : null)));
        }

        // ============================================================
        // POST - CRIAR ESCALA MANUAL
        // ============================================================

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<EscalaRespostaDto>>
            CriarEscala(EscalaDto dados)
        {
            var validacao =
                await ValidarEscalaAsync(dados);

            if (!validacao.Valido)
            {
                return BadRequest(new
                {
                    mensagem = validacao.Erro
                });
            }

            // --------------------------------------------------------
            // VERIFICAR CONFLITOS
            // --------------------------------------------------------

            var escalasFuncionario =
                await _context.Escalas
                    .Where(e =>
                        e.FuncionarioId ==
                        dados.FuncionarioId)
                    .ToListAsync();

            foreach (var existente in escalasFuncionario)
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
                        escalaId = existente.Id
                    });
                }
            }

            // --------------------------------------------------------
            // CRIAR
            // --------------------------------------------------------

            var escala = new Escala
            {
                FuncionarioId = dados.FuncionarioId,

                Data = dados.Data.Date,

                HoraInicio = dados.HoraInicio,

                HoraFim = dados.HoraFim,

                TipoTurnoId = dados.TipoTurnoId,

                PostoId = dados.PostoId,

                Observacao = dados.Observacao
            };

            _context.Escalas.Add(escala);

            await _context.SaveChangesAsync();

            // --------------------------------------------------------
            // RECARREGAR
            // --------------------------------------------------------

            escala = await _context.Escalas
                .Include(e => e.Funcionario)
                .Include(e => e.TipoTurno)
                .Include(e => e.Posto)
                .FirstAsync(e => e.Id == escala.Id);

            return CreatedAtAction(
                nameof(GetEscala),
                new { id = escala.Id },
                ParaDto(
                    escala,
                    validacao.Lotacao));
        }

        // ============================================================
        // PUT - EDITAR ESCALA
        // ============================================================

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<EscalaRespostaDto>>
            EditarEscala(
                int id,
                EscalaDto dados)
        {
            var escala = await _context.Escalas
                .FirstOrDefaultAsync(e => e.Id == id);

            if (escala == null)
            {
                return NotFound(new
                {
                    mensagem = "Escala não encontrada."
                });
            }

            var validacao =
                await ValidarEscalaAsync(dados);

            if (!validacao.Valido)
            {
                return BadRequest(new
                {
                    mensagem = validacao.Erro
                });
            }

            // --------------------------------------------------------
            // CONFLITOS
            // --------------------------------------------------------

            var outrasEscalas =
                await _context.Escalas
                    .Where(e =>
                        e.FuncionarioId ==
                        dados.FuncionarioId &&
                        e.Id != id)
                    .ToListAsync();

            foreach (var existente in outrasEscalas)
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
                        escalaId = existente.Id
                    });
                }
            }

            // --------------------------------------------------------
            // ATUALIZAR
            // --------------------------------------------------------

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

            escala = await _context.Escalas
                .Include(e => e.Funcionario)
                .Include(e => e.TipoTurno)
                .Include(e => e.Posto)
                .FirstAsync(e => e.Id == id);

            return Ok(
                ParaDto(
                    escala,
                    validacao.Lotacao));
        }

        // ============================================================
        // DELETE
        // ============================================================

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult>
            ApagarEscala(int id)
        {
            var escala =
                await _context.Escalas
                    .FirstOrDefaultAsync(e => e.Id == id);

            if (escala == null)
            {
                return NotFound(new
                {
                    mensagem = "Escala não encontrada."
                });
            }

            _context.Escalas.Remove(escala);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Escala eliminada com sucesso."
            });
        }

        // ============================================================
        // POST - GERAR ESCALA AUTOMÁTICA
        // ============================================================



        [HttpPost("gerar-automatica")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> GerarEscalaAutomatica(
            [FromBody] GerarEscalaDto dados)
        {
            // ============================================================
            // 1. VALIDAR DADOS RECEBIDOS
            // ============================================================

            if (dados == null)
                return BadRequest("Os dados da geração da escala são obrigatórios.");

            if (dados.UnidadeOperacionalId <= 0)
                return BadRequest("A UnidadeOperacionalId é obrigatória.");

            var dataEscala = dados.Data.Date;

            // ============================================================
            // 2. VALIDAR UNIDADE OPERACIONAL
            // ============================================================

            var unidade = await _context.UnidadesOperacionais
                .FirstOrDefaultAsync(u =>
                    u.Id == dados.UnidadeOperacionalId &&
                    u.Ativo);

            if (unidade == null)
            {
                return NotFound(
                    $"A unidade operacional com ID {dados.UnidadeOperacionalId} " +
                    "não foi encontrada ou está inativa.");
            }

            // ============================================================
            // 3. CARREGAR GRUPOS DE ESCALA
            // ============================================================

            var grupos = await _context.GruposEscala
                .Include(g => g.Equipa)
                .Include(g => g.TipoTurno)
                .Where(g =>
                    g.UnidadeOperacionalId == dados.UnidadeOperacionalId &&
                    g.Ativo)
                .OrderBy(g => g.OrdemRotacao)
                .ToListAsync();

            if (!grupos.Any())
            {
                return BadRequest(
                    $"Não existem grupos de escala ativos configurados para " +
                    $"a unidade '{unidade.Nome}'.");
            }

            // ============================================================
            // 4. VALIDAR ORDENS DA ROTAÇÃO
            // ============================================================

            var ordensEsperadas = Enumerable
                .Range(1, grupos.Count)
                .ToList();

            var ordensExistentes = grupos
                .Select(g => g.OrdemRotacao)
                .OrderBy(x => x)
                .ToList();

            if (!ordensEsperadas.SequenceEqual(ordensExistentes))
            {
                return BadRequest(
                    $"A configuração da rotação da unidade '{unidade.Nome}' " +
                    "é inválida. As OrdensRotacao devem ser sequenciais.");
            }

            // ============================================================
            // 5. DETERMINAR O GRUPO DE SERVIÇO
            // ============================================================

            var grupoReferencia = grupos.First();

            var diasDesdeReferencia =
                (dataEscala - grupoReferencia.DataReferencia.Date).Days;

            var quantidadeGrupos = grupos.Count;

            var indiceRotacao =
                ((diasDesdeReferencia % quantidadeGrupos) +
                 quantidadeGrupos) %
                 quantidadeGrupos;

            var ordemEmServico = indiceRotacao + 1;

            var grupoServico = grupos
                .FirstOrDefault(g =>
                    g.OrdemRotacao == ordemEmServico);

            if (grupoServico == null)
            {
                return BadRequest(
                    $"Não foi possível determinar o grupo de serviço " +
                    $"para a data {dataEscala:dd/MM/yyyy}.");
            }

            // ============================================================
            // 6. VALIDAR EQUIPA
            // ============================================================

            if (grupoServico.EquipaId <= 0)
            {
                return BadRequest(
                    $"O grupo '{grupoServico.Nome}' não possui uma equipa válida.");
            }

            if (grupoServico.Equipa == null)
            {
                return BadRequest(
                    $"A equipa associada ao grupo '{grupoServico.Nome}' " +
                    "não foi encontrada.");
            }

            if (grupoServico.Equipa.UnidadeOperacionalId !=
                dados.UnidadeOperacionalId)
            {
                return BadRequest(
                    $"A equipa '{grupoServico.Equipa.Nome}' não pertence " +
                    $"à unidade operacional '{unidade.Nome}'.");
            }

            // ============================================================
            // 7. VALIDAR TIPO DE TURNO
            // ============================================================

            if (grupoServico.TipoTurnoId <= 0)
            {
                return BadRequest(
                    $"O grupo '{grupoServico.Nome}' não possui um TipoTurno válido.");
            }

            var tipoTurnoId = grupoServico.TipoTurnoId;

            var tipoTurno = await _context.TiposTurno
                .FirstOrDefaultAsync(t =>
                    t.Id == tipoTurnoId &&
                    t.Ativo);

            if (tipoTurno == null)
            {
                return BadRequest(
                    $"O TipoTurno ID {tipoTurnoId} associado ao grupo " +
                    $"'{grupoServico.Nome}' não existe ou está inativo.");
            }

            // ============================================================
            // 8. VALIDAR TURNO ENVIADO PELO UTILIZADOR
            // ============================================================

            if (dados.TipoTurnoId > 0 &&
                dados.TipoTurnoId != tipoTurnoId)
            {
                return BadRequest(
                    $"O TipoTurno enviado ({dados.TipoTurnoId}) não corresponde " +
                    $"ao TipoTurno configurado no grupo '{grupoServico.Nome}' " +
                    $"({tipoTurnoId}).");
            }

            // ============================================================
            // 9. HORA DE INÍCIO
            // ============================================================

            var horaInicio = dados.HoraInicio;

            // ============================================================
            // 10. CALCULAR HORA DE FIM
            // ============================================================

            var duracaoTurno =
                TimeSpan.FromHours(tipoTurno.HorasTrabalho);

            var horaFim =
                horaInicio.Add(duracaoTurno);

            // Se passar de 24 horas, ajustar para o dia seguinte.
            if (horaFim.TotalHours >= 24)
            {
                horaFim -= TimeSpan.FromDays(1);
            }

            // ============================================================
            // 11. CARREGAR LOTAÇÕES DA EQUIPA EM SERVIÇO
            // ============================================================

            var lotacoes = await _context.LotacoesFuncionarios
                .Include(l => l.Funcionario)
                .Include(l => l.Equipa)
                .Include(l => l.TipoTurno)
                .Where(l =>
                    l.Ativo &&
                    l.UnidadeOperacionalId == dados.UnidadeOperacionalId &&
                    l.EquipaId == grupoServico.EquipaId)
                .OrderBy(l => l.Funcionario.NomeCompleto)
                .ToListAsync();

            if (!lotacoes.Any())
            {
                return Ok(new
                {
                    mensagem =
                        $"O grupo '{grupoServico.Nome}' está em serviço " +
                        $"na data {dataEscala:dd/MM/yyyy}, mas não existem " +
                        $"funcionários distribuídos na equipa " +
                        $"'{grupoServico.Equipa.Nome}'.",

                    unidadeId = unidade.Id,
                    unidadeNome = unidade.Nome,

                    grupoId = grupoServico.Id,
                    grupoNome = grupoServico.Nome,

                    ordemRotacao = grupoServico.OrdemRotacao,

                    equipaId = grupoServico.EquipaId,
                    equipaNome = grupoServico.Equipa.Nome,

                    data = dataEscala,

                    horaInicio = horaInicio,
                    horaFim = horaFim,

                    tipoTurnoId = tipoTurnoId,
                    tipoTurnoNome = tipoTurno.Nome,

                    funcionariosNaEquipa = 0,
                    escalasCriadas = 0,
                    escalasJaExistentes = 0,
                    funcionariosComConflito = 0
                });
            }

            // ============================================================
            // 12. VALIDAR TIPO DE TURNO DAS LOTAÇÕES
            // ============================================================

            var lotacoesComTurnoInvalido = lotacoes
                .Where(l =>
                    l.TipoTurnoId > 0 &&
                    l.TipoTurnoId != tipoTurnoId)
                .ToList();

            var lotacoesValidas = lotacoes
                .Where(l =>
                    l.TipoTurnoId <= 0 ||
                    l.TipoTurnoId == tipoTurnoId)
                .ToList();

            // ============================================================
            // 13. VALIDAR POSTO INFORMADO
            // ============================================================

            if (dados.PostoId > 0)
            {
                var posto = await _context.Postos
                    .FirstOrDefaultAsync(p =>
                        p.Id == dados.PostoId &&
                        p.Ativo);

                if (posto == null)
                {
                    return BadRequest(
                        $"O Posto ID {dados.PostoId} não existe ou está inativo.");
                }

                if (posto.UnidadeOperacionalId !=
                    dados.UnidadeOperacionalId)
                {
                    return BadRequest(
                        $"O posto '{posto.Nome}' não pertence à unidade " +
                        $"operacional '{unidade.Nome}'.");
                }
            }

            // ============================================================
            // 14. OBTER FUNCIONÁRIOS DA EQUIPA
            // ============================================================

            var funcionarioIds = lotacoesValidas
                .Select(l => l.FuncionarioId)
                .Distinct()
                .ToList();

            if (!funcionarioIds.Any())
            {
                return Ok(new
                {
                    mensagem =
                        $"Não existem funcionários com lotação válida na " +
                        $"equipa '{grupoServico.Equipa.Nome}'.",

                    unidadeId = unidade.Id,
                    unidadeNome = unidade.Nome,

                    grupoId = grupoServico.Id,
                    grupoNome = grupoServico.Nome,

                    ordemRotacao = grupoServico.OrdemRotacao,

                    equipaId = grupoServico.EquipaId,
                    equipaNome = grupoServico.Equipa.Nome,

                    data = dataEscala,

                    horaInicio = horaInicio,
                    horaFim = horaFim,

                    tipoTurnoId = tipoTurnoId,
                    tipoTurnoNome = tipoTurno.Nome,

                    funcionariosNaEquipa = lotacoes.Count,

                    funcionariosComTurnoInvalido =
                        lotacoesComTurnoInvalido.Count,

                    escalasCriadas = 0
                });
            }

            // ============================================================
            // 15. CARREGAR ESCALAS EXISTENTES
            // ============================================================

            var escalasExistentes = await _context.Escalas
                .Where(e =>
                    funcionarioIds.Contains(e.FuncionarioId))
                .ToListAsync();

            // ============================================================
            // 16. LISTAS DE CONTROLO
            // ============================================================

            var funcionariosEscalados = new List<object>();

            var funcionariosJaEscalados = new List<object>();

            var funcionariosComConflito = new List<object>();

            // ============================================================
            // 17. GERAR ESCALA PARA CADA FUNCIONÁRIO
            // ============================================================

            foreach (var lotacao in lotacoesValidas)
            {
                var funcionario = lotacao.Funcionario;

                if (funcionario == null)
                    continue;

                // --------------------------------------------------------
                // DETERMINAR POSTO
                // --------------------------------------------------------

                var postoId =
                    dados.PostoId > 0
                        ? dados.PostoId
                        : lotacao.PostoId;

                // --------------------------------------------------------
                // VERIFICAR SE JÁ EXISTE A MESMA ESCALA
                // --------------------------------------------------------

                var escalaIgual = escalasExistentes.Any(e =>
                    e.FuncionarioId == lotacao.FuncionarioId &&
                    e.Data.Date == dataEscala &&
                    e.HoraInicio == horaInicio &&
                    e.HoraFim == horaFim &&
                    e.TipoTurnoId == tipoTurnoId &&
                    e.PostoId == postoId);

                if (escalaIgual)
                {
                    funcionariosJaEscalados.Add(new
                    {
                        funcionarioId = funcionario.Id,
                        nome = funcionario.NomeCompleto,
                        nip = funcionario.Nip
                    });

                    continue;
                }

                // --------------------------------------------------------
                // VERIFICAR CONFLITO DE HORÁRIO
                // --------------------------------------------------------

                var conflito = escalasExistentes.Any(e =>
                    e.FuncionarioId == lotacao.FuncionarioId &&
                    ExisteConflito(
                        e.Data,
                        e.HoraInicio,
                        e.HoraFim,
                        dataEscala,
                        horaInicio,
                        horaFim));

                if (conflito)
                {
                    funcionariosComConflito.Add(new
                    {
                        funcionarioId = funcionario.Id,
                        nome = funcionario.NomeCompleto,
                        nip = funcionario.Nip
                    });

                    continue;
                }

                // --------------------------------------------------------
                // CRIAR ESCALA
                // --------------------------------------------------------

                var novaEscala = new Escala
                {
                    FuncionarioId = lotacao.FuncionarioId,

                    Data = dataEscala,

                    HoraInicio = horaInicio,

                    HoraFim = horaFim,

                    TipoTurnoId = tipoTurnoId,

                    PostoId = postoId,

                    Observacao =
                        dados.Observacao ??
                        $"Escala {unidade.Nome} - " +
                        $"Grupo {grupoServico.Nome} - " +
                        $"Equipa {grupoServico.Equipa.Nome}"
                };

                _context.Escalas.Add(novaEscala);

                // Adicionar à coleção local.
                // Isto evita uma duplicação durante a mesma execução.
                escalasExistentes.Add(novaEscala);

                funcionariosEscalados.Add(new
                {
                    funcionarioId = funcionario.Id,
                    nome = funcionario.NomeCompleto,
                    nip = funcionario.Nip
                });
            }

            // ============================================================
            // 18. GUARDAR ALTERAÇÕES
            // ============================================================

            if (funcionariosEscalados.Count > 0)
            {
                await _context.SaveChangesAsync();
            }

            // ============================================================
            // 19. RESPOSTA FINAL
            // ============================================================

            return Ok(new
            {
                mensagem =
        $"Geração da escala concluída para a unidade " +
        $"'{unidade.Nome}'.",

                unidadeId = unidade.Id,

                unidadeNome = unidade.Nome,

                grupoId = grupoServico.Id,

                grupoNome = grupoServico.Nome,

                ordemRotacao = grupoServico.OrdemRotacao,

                equipaId = grupoServico.EquipaId,

                equipaNome = grupoServico.Equipa.Nome,

                data = dataEscala,

                horaInicio = horaInicio,

                horaFim = horaFim,

                tipoTurnoId = tipoTurnoId,

                tipoTurnoNome = tipoTurno.Nome,

                funcionariosNaEquipa = lotacoes.Count,

                funcionariosComLotacaoValida = lotacoesValidas.Count,

                funcionariosComTurnoInvalido =
        lotacoesComTurnoInvalido.Count,

                escalasCriadas =
        funcionariosEscalados.Count,

                escalasJaExistentes =
        funcionariosJaEscalados.Count,

                quantidadeFuncionariosComConflito =
        funcionariosComConflito.Count,

                funcionariosEscalados = funcionariosEscalados,

                funcionariosJaEscalados = funcionariosJaEscalados,

                funcionariosComConflito = funcionariosComConflito
            });
        }
    }

}