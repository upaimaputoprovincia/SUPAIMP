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

        [HttpPost("gerar")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult>
            GerarEscalaAutomatica(GerarEscalaDto dados)
        {
            if (dados.UnidadeOperacionalId <= 0)
            {
                return BadRequest(new
                {
                    mensagem =
                        "É necessário indicar a unidade operacional."
                });
            }

            // --------------------------------------------------------
            // UNIDADE
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
            // GRUPOS ATIVOS DA UNIDADE
            // --------------------------------------------------------

            var grupos =
                await _context.GruposEscala
                    .Include(g => g.Equipa)
                    .Include(g => g.TipoTurno)
                    .Where(g =>
                        g.UnidadeOperacionalId ==
                        dados.UnidadeOperacionalId &&
                        g.Ativo)
                    .OrderBy(g => g.OrdemRotacao)
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
            // DETERMINAR GRUPO EM SERVIÇO
            //
            // A rotação utiliza a DataReferencia do primeiro grupo
            // e a OrdemRotacao.
            // --------------------------------------------------------

            var grupoReferencia =
                grupos.First();

            var dias =
                (dados.Data.Date -
                 grupoReferencia.DataReferencia.Date).Days;

            var quantidadeGrupos =
                grupos.Count;

            var indiceRotacao =
                ((dias % quantidadeGrupos) +
                 quantidadeGrupos) %
                quantidadeGrupos;

            var ordemEmServico =
                indiceRotacao + 1;

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
            // TIPO DE TURNO
            // --------------------------------------------------------

            var tipoTurnoId =
                dados.TipoTurnoId ??
                grupoServico.TipoTurnoId;

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
            // EQUIPA
            // --------------------------------------------------------

            if (!grupoServico.EquipaId.Equals(
                    grupoServico.Equipa?.Id))
            {
                // Proteção adicional; normalmente não será necessária.
            }

            var lotacoes =
                await _context.LotacoesFuncionarios
                    .Where(l =>
                        l.Ativo &&
                        l.UnidadeOperacionalId ==
                        dados.UnidadeOperacionalId &&
                        l.EquipaId ==
                        grupoServico.EquipaId)
                    .ToListAsync();

            if (!lotacoes.Any())
            {
                return Ok(new
                {
                    mensagem =
                        $"O grupo {grupoServico.Nome} ({grupoServico.Equipa?.Nome}) está de serviço, mas não existem funcionários com lotação ativa nessa equipa.",
                    unidade = unidade.Nome,
                    grupo = grupoServico.Nome,
                    equipa = grupoServico.Equipa?.Nome,
                    data = dados.Data.Date,
                    escalasCriadas = 0
                });
            }

            // --------------------------------------------------------
            // HORA DE FIM
            // --------------------------------------------------------

            var horaInicio =
                dados.HoraInicio;

            var horaFim =
                horaInicio.Add(
                    TimeSpan.FromHours(
                        turno.HorasTrabalho));

            // TimeSpan não pode ultrapassar 24h.
            horaFim =
                TimeSpan.FromTicks(
                    horaFim.Ticks %
                    TimeSpan.FromDays(1).Ticks);

            // --------------------------------------------------------
            // CRIAR ESCALAS
            // --------------------------------------------------------

            var funcionarioIds =
                lotacoes
                    .Select(l => l.FuncionarioId)
                    .Distinct()
                    .ToList();

            var existentes =
                await _context.Escalas
                    .Where(e =>
                        funcionarioIds.Contains(
                            e.FuncionarioId))
                    .ToListAsync();

            var funcionariosCriados = new List<int>();

            foreach (var lotacao in lotacoes)
            {
                var jaExiste =
                    existentes.Any(e =>
                        e.FuncionarioId ==
                        lotacao.FuncionarioId &&
                        ExisteConflito(
                            e.Data,
                            e.HoraInicio,
                            e.HoraFim,
                            dados.Data.Date,
                            horaInicio,
                            horaFim));

                if (jaExiste)
                    continue;

                var escala = new Escala
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
                        dados.Observacao ??
                        $"Escala automática - {grupoServico.Nome} - {grupoServico.Equipa?.Nome}"
                };

                _context.Escalas.Add(escala);

                funcionariosCriados.Add(
                    lotacao.FuncionarioId);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem =
                    "Escala automática gerada com sucesso.",

                unidade = unidade.Nome,

                unidadeOperacionalId =
                    unidade.Id,

                grupo = grupoServico.Nome,

                grupoEscalaId =
                    grupoServico.Id,

                equipa =
                    grupoServico.Equipa?.Nome,

                equipaId =
                    grupoServico.EquipaId,

                ordemRotacao =
                    grupoServico.OrdemRotacao,

                data =
                    dados.Data.Date,

                horaInicio,

                horaFim,

                tipoTurno =
                    turno.Nome,

                funcionariosNaEquipa =
                    lotacoes.Count,

                escalasCriadas =
                    funcionariosCriados.Count,

                funcionariosIgnorados =
                    lotacoes.Count -
                    funcionariosCriados.Count
            });
        }
    }
}