using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using supai_mp.Data;
using supai_mp.Models;
using supai_mp.Models.Organizacao;

namespace SUPAI_MP.Controllers
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
        // DTOs
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

            // Snapshot da organização no momento da escala
            public int? SeccaoId { get; set; }
            public string? Seccao { get; set; }

            public int? UnidadeOperacionalId { get; set; }
            public string? UnidadeOperacional { get; set; }

            public int? EquipaId { get; set; }
            public string? Equipa { get; set; }

            public int FuncaoOperacionalId { get; set; }
            public string? FuncaoOperacional { get; set; }
        }

        public class GerarEscalaDto
        {
            public int UnidadeOperacionalId { get; set; }

            public DateTime Data { get; set; }

            public TimeSpan HoraInicio { get; set; }

            public int? TipoTurnoId { get; set; }

            public int? PostoId { get; set; }

            public string? Observacao { get; set; }
        }

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

            public List<FuncionarioEscalaResultadoDto> FuncionariosEscalados { get; set; }
                = new();

            public List<FuncionarioEscalaResultadoDto> FuncionariosJaEscalados { get; set; }
                = new();

            public List<FuncionarioEscalaResultadoDto> FuncionariosComConflito { get; set; }
                = new();

            public List<FuncionarioEscalaResultadoDto> FuncionariosComTurnoInvalido { get; set; }
                = new();
        }

        // ============================================================
        // HELPERS
        // ============================================================

        private static EscalaRespostaDto ParaDto(Escala escala)
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

                // ====================================================
                // IMPORTANTE:
                // Estes dados vêm do SNAPSHOT gravado na Escala.
                // Não vêm da lotação atual do funcionário.
                // ====================================================

                SeccaoId = escala.SeccaoId,
                Seccao = escala.Seccao?.Nome,

                UnidadeOperacionalId = escala.UnidadeOperacionalId,
                UnidadeOperacional = escala.UnidadeOperacional?.Nome,

                EquipaId = escala.EquipaId,
                Equipa = escala.Equipa?.Nome,

                FuncaoOperacionalId = escala.FuncaoOperacionalId,
                FuncaoOperacional = escala.FuncaoOperacional?.Nome
            };
        }

        private static DateTime CalcularInicio(
            DateTime data,
            TimeSpan hora)
        {
            return data.Date.Add(hora);
        }

        private static DateTime CalcularFim(
            DateTime data,
            TimeSpan horaInicio,
            TimeSpan horaFim)
        {
            var inicio = CalcularInicio(data, horaInicio);
            var fim = CalcularInicio(data, horaFim);

            // Exemplo:
            // 07:00 -> 15:00 = mesmo dia
            // 22:00 -> 07:00 = dia seguinte
            // 07:00 -> 07:00 = 24 horas

            if (fim <= inicio)
            {
                fim = fim.AddDays(1);
            }

            return fim;
        }

        private static bool HoraValida(TimeSpan hora)
        {
            return hora >= TimeSpan.Zero &&
                   hora < TimeSpan.FromDays(1);
        }

        private static bool ExisteConflito(
            DateTime dataExistente,
            TimeSpan inicioExistente,
            TimeSpan fimExistente,
            DateTime novaData,
            TimeSpan novoInicio,
            TimeSpan novoFim)
        {
            var inicioExistenteDateTime =
                CalcularInicio(dataExistente, inicioExistente);

            var fimExistenteDateTime =
                CalcularFim(
                    dataExistente,
                    inicioExistente,
                    fimExistente);

            var novoInicioDateTime =
                CalcularInicio(novaData, novoInicio);

            var novoFimDateTime =
                CalcularFim(
                    novaData,
                    novoInicio,
                    novoFim);

            return inicioExistenteDateTime < novoFimDateTime &&
                   fimExistenteDateTime > novoInicioDateTime;
        }

        private static bool EscalaExatamenteIgual(
            Escala escala,
            DateTime data,
            TimeSpan horaInicio,
            TimeSpan horaFim,
            int tipoTurnoId,
            int? postoId)
        {
            return escala.Data.Date == data.Date &&
                   escala.HoraInicio == horaInicio &&
                   escala.HoraFim == horaFim &&
                   escala.TipoTurnoId == tipoTurnoId &&
                   escala.PostoId == postoId;
        }

        // ============================================================
        // LOTACAO VALIDA NA DATA
        // ============================================================

        private async Task<LotacaoFuncionario?> ObterLotacaoValidaAsync(
            int funcionarioId,
            DateTime data)
        {
            var dataEscala = data.Date;

            return await _context.LotacoesFuncionarios
                .Include(l => l.Seccao)
                .Include(l => l.UnidadeOperacional)
                .Include(l => l.Equipa)
                .Include(l => l.FuncaoOperacional)
                .Include(l => l.TipoTurno)
                .Include(l => l.Posto)
                .Include(l => l.Funcionario)
                .Where(l =>
                    l.FuncionarioId == funcionarioId &&

                    l.DataInicio.Date <= dataEscala &&

                    (
                        l.DataFim == null ||
                        l.DataFim.Value.Date > dataEscala
                    )
                )
                .OrderByDescending(l => l.DataInicio)
                .FirstOrDefaultAsync();
        }

        // ============================================================
        // CARREGAR ESCALAS COM SNAPSHOT
        // ============================================================

        private IQueryable<Escala> QueryEscalasComSnapshot()
        {
            return _context.Escalas
                .Include(e => e.Funcionario)
                .Include(e => e.TipoTurno)
                .Include(e => e.Posto)

                // Snapshot
                .Include(e => e.Seccao)
                .Include(e => e.UnidadeOperacional)
                .Include(e => e.Equipa)
                .Include(e => e.FuncaoOperacional);
        }

        // ============================================================
        // VALIDAR ESCALA MANUAL
        // ============================================================

        private async Task<(
            bool Valido,
            string? Erro,
            Funcionario? Funcionario,
            TipoTurno? TipoTurno,
            LotacaoFuncionario? Lotacao,
            Posto? Posto
        )> ValidarEscalaAsync(EscalaDto dados)
        {
            var funcionario =
                await _context.Funcionarios
                    .FirstOrDefaultAsync(f => f.Id == dados.FuncionarioId);

            if (funcionario == null)
            {
                return (
                    false,
                    "Funcionário não encontrado.",
                    null,
                    null,
                    null,
                    null
                );
            }

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
                    null
                );
            }

            // ========================================================
            // IMPORTANTE:
            // Procuramos a lotação válida NA DATA da escala.
            // Não usamos simplesmente Ativo = true.
            // ========================================================

            var lotacao =
                await ObterLotacaoValidaAsync(
                    dados.FuncionarioId,
                    dados.Data);

            if (lotacao == null)
            {
                return (
                    false,
                    $"O funcionário {funcionario.NomeCompleto} não possui uma lotação válida para a data {dados.Data:dd/MM/yyyy}.",
                    funcionario,
                    turno,
                    null,
                    null
                );
            }

            if (lotacao.FuncaoOperacionalId <= 0)
            {
                return (
                    false,
                    "A lotação do funcionário não possui uma função operacional válida.",
                    funcionario,
                    turno,
                    lotacao,
                    null
                );
            }

            // ========================================================
            // VALIDAR TURNO DA LOTACAO
            // ========================================================

            if (lotacao.TipoTurnoId.HasValue &&
                lotacao.TipoTurnoId.Value != dados.TipoTurnoId)
            {
                return (
                    false,
                    $"O funcionário está lotado no turno '{lotacao.TipoTurno?.Nome}', mas foi solicitado o turno '{turno.Nome}'.",
                    funcionario,
                    turno,
                    lotacao,
                    null
                );
            }

            // ========================================================
            // POSTO
            // ========================================================

            Posto? posto = null;

            var postoEfetivo =
                dados.PostoId ?? lotacao.PostoId;

            if (postoEfetivo.HasValue)
            {
                posto =
                    await _context.Postos
                        .FirstOrDefaultAsync(p =>
                            p.Id == postoEfetivo.Value &&
                            p.Ativo);

                if (posto == null)
                {
                    return (
                        false,
                        "Posto não encontrado ou está inativo.",
                        funcionario,
                        turno,
                        lotacao,
                        null
                    );
                }

                if (lotacao.PostoId.HasValue &&
                    lotacao.PostoId.Value != posto.Id)
                {
                    return (
                        false,
                        "O posto informado não corresponde ao posto da lotação do funcionário.",
                        funcionario,
                        turno,
                        lotacao,
                        posto
                    );
                }
            }

            // ========================================================
            // HORAS
            // ========================================================

            if (!HoraValida(dados.HoraInicio) ||
                !HoraValida(dados.HoraFim))
            {
                return (
                    false,
                    "Hora de início ou hora de fim inválida.",
                    funcionario,
                    turno,
                    lotacao,
                    posto
                );
            }

            var inicio =
                CalcularInicio(
                    dados.Data,
                    dados.HoraInicio);

            var fim =
                CalcularFim(
                    dados.Data,
                    dados.HoraInicio,
                    dados.HoraFim);

            var duracao =
                fim - inicio;

            // ========================================================
            // START == END
            // Só é permitido quando o turno tem pelo menos 24 horas.
            // ========================================================

            if (dados.HoraInicio == dados.HoraFim &&
                turno.HorasTrabalho < 24)
            {
                return (
                    false,
                    "Hora de início e hora de fim não podem ser iguais para um turno inferior a 24 horas.",
                    funcionario,
                    turno,
                    lotacao,
                    posto
                );
            }

            if (duracao <= TimeSpan.Zero)
            {
                return (
                    false,
                    "A duração da escala deve ser superior a zero.",
                    funcionario,
                    turno,
                    lotacao,
                    posto
                );
            }

            if (duracao > TimeSpan.FromHours(turno.HorasTrabalho))
            {
                return (
                    false,
                    $"A duração da escala ({duracao.TotalHours:0.##}h) ultrapassa a duração configurada do turno ({turno.HorasTrabalho}h).",
                    funcionario,
                    turno,
                    lotacao,
                    posto
                );
            }

            return (
                true,
                null,
                funcionario,
                turno,
                lotacao,
                posto
            );
        }

        // ============================================================
        // GET - TODAS
        // ============================================================

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EscalaRespostaDto>>> GetEscalas()
        {
            var escalas =
                await QueryEscalasComSnapshot()
                    .OrderByDescending(e => e.Data)
                    .ThenBy(e => e.HoraInicio)
                    .ToListAsync();

            return Ok(
                escalas.Select(ParaDto)
            );
        }

        // ============================================================
        // GET - POR ID
        // ============================================================

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EscalaRespostaDto>> GetEscala(int id)
        {
            var escala =
                await QueryEscalasComSnapshot()
                    .FirstOrDefaultAsync(e => e.Id == id);

            if (escala == null)
            {
                return NotFound(
                    new
                    {
                        mensagem = "Escala não encontrada."
                    }
                );
            }

            return Ok(ParaDto(escala));
        }

        // ============================================================
        // GET - POR DATA
        // ============================================================

        [HttpGet("data/{data}")]
        public async Task<ActionResult<IEnumerable<EscalaRespostaDto>>> GetPorData(
            DateTime data)
        {
            var dataFiltro = data.Date;

            var escalas =
                await QueryEscalasComSnapshot()
                    .Where(e => e.Data.Date == dataFiltro)
                    .OrderBy(e => e.HoraInicio)
                    .ThenBy(e => e.Funcionario!.NomeCompleto)
                    .ToListAsync();

            return Ok(
                escalas.Select(ParaDto)
            );
        }

        // ============================================================
        // GET - HOJE
        // ============================================================

        [HttpGet("hoje")]
        public async Task<ActionResult<IEnumerable<EscalaRespostaDto>>> Hoje()
        {
            var data = DateTime.Today;

            var escalas =
                await QueryEscalasComSnapshot()
                    .Where(e => e.Data.Date == data)
                    .OrderBy(e => e.HoraInicio)
                    .ThenBy(e => e.Funcionario!.NomeCompleto)
                    .ToListAsync();

            return Ok(
                escalas.Select(ParaDto)
            );
        }

        // ============================================================
        // GET - ONTEM
        // ============================================================

        [HttpGet("ontem")]
        public async Task<ActionResult<IEnumerable<EscalaRespostaDto>>> Ontem()
        {
            var data = DateTime.Today.AddDays(-1);

            var escalas =
                await QueryEscalasComSnapshot()
                    .Where(e => e.Data.Date == data)
                    .OrderBy(e => e.HoraInicio)
                    .ThenBy(e => e.Funcionario!.NomeCompleto)
                    .ToListAsync();

            return Ok(
                escalas.Select(ParaDto)
            );
        }

        // ============================================================
        // GET - AMANHA
        // ============================================================

        [HttpGet("amanha")]
        public async Task<ActionResult<IEnumerable<EscalaRespostaDto>>> Amanha()
        {
            var data = DateTime.Today.AddDays(1);

            var escalas =
                await QueryEscalasComSnapshot()
                    .Where(e => e.Data.Date == data)
                    .OrderBy(e => e.HoraInicio)
                    .ThenBy(e => e.Funcionario!.NomeCompleto)
                    .ToListAsync();

            return Ok(
                escalas.Select(ParaDto)
            );
        }

        // ============================================================
        // GET - POR FUNCIONARIO
        // ============================================================

        [HttpGet("funcionario/{funcionarioId:int}")]
        public async Task<ActionResult<IEnumerable<EscalaRespostaDto>>> GetPorFuncionario(
            int funcionarioId)
        {
            var escalas =
                await QueryEscalasComSnapshot()
                    .Where(e => e.FuncionarioId == funcionarioId)
                    .OrderByDescending(e => e.Data)
                    .ThenBy(e => e.HoraInicio)
                    .ToListAsync();

            return Ok(
                escalas.Select(ParaDto)
            );
        }

        // ============================================================
        // GET - POR POSTO
        // ============================================================

        [HttpGet("posto/{postoId:int}")]
        public async Task<ActionResult<IEnumerable<EscalaRespostaDto>>> GetPorPosto(
            int postoId)
        {
            var escalas =
                await QueryEscalasComSnapshot()
                    .Where(e => e.PostoId == postoId)
                    .OrderByDescending(e => e.Data)
                    .ThenBy(e => e.HoraInicio)
                    .ToListAsync();

            return Ok(
                escalas.Select(ParaDto)
            );
        }

        // ============================================================
        // GET - POR TIPO DE TURNO
        // ============================================================

        [HttpGet("turno/{tipoTurnoId:int}")]
        public async Task<ActionResult<IEnumerable<EscalaRespostaDto>>> GetPorTurno(
            int tipoTurnoId)
        {
            var escalas =
                await QueryEscalasComSnapshot()
                    .Where(e => e.TipoTurnoId == tipoTurnoId)
                    .OrderByDescending(e => e.Data)
                    .ThenBy(e => e.HoraInicio)
                    .ToListAsync();

            return Ok(
                escalas.Select(ParaDto)
            );
        }

        // ============================================================
        // POST - CRIAR ESCALA MANUAL
        // ============================================================

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<EscalaRespostaDto>> CriarEscala(
            [FromBody] EscalaDto dados)
        {
            var validacao =
                await ValidarEscalaAsync(dados);

            if (!validacao.Valido)
            {
                return BadRequest(
                    new
                    {
                        mensagem = validacao.Erro
                    }
                );
            }

            var lotacao = validacao.Lotacao!;
            var turno = validacao.TipoTurno!;

            var postoEfetivo =
                dados.PostoId ?? lotacao.PostoId;

            // ========================================================
            // VERIFICAR CONFLITO
            // ========================================================

            var escalasFuncionario =
                await _context.Escalas
                    .Where(e =>
                        e.FuncionarioId == dados.FuncionarioId)
                    .ToListAsync();

            foreach (var escalaExistente in escalasFuncionario)
            {
                if (ExisteConflito(
                    escalaExistente.Data,
                    escalaExistente.HoraInicio,
                    escalaExistente.HoraFim,
                    dados.Data.Date,
                    dados.HoraInicio,
                    dados.HoraFim))
                {
                    return Conflict(
                        new
                        {
                            mensagem =
                                $"O funcionário já possui uma escala que entra em conflito no período informado.",
                            escalaId = escalaExistente.Id,
                            data = escalaExistente.Data,
                            horaInicio = escalaExistente.HoraInicio,
                            horaFim = escalaExistente.HoraFim
                        }
                    );
                }
            }

            // ========================================================
            // CRIAR COM SNAPSHOT
            // ========================================================

            var escala = new Escala
            {
                FuncionarioId = dados.FuncionarioId,

                Data = dados.Data.Date,

                HoraInicio = dados.HoraInicio,
                HoraFim = dados.HoraFim,

                TipoTurnoId = turno.Id,

                PostoId = postoEfetivo,

                Observacao = dados.Observacao,

                // SNAPSHOT
                SeccaoId = lotacao.SeccaoId,
                UnidadeOperacionalId = lotacao.UnidadeOperacionalId,
                EquipaId = lotacao.EquipaId,
                FuncaoOperacionalId = lotacao.FuncaoOperacionalId
            };

            _context.Escalas.Add(escala);

            await _context.SaveChangesAsync();

            // Recarregar navegações
            escala =
                await QueryEscalasComSnapshot()
                    .FirstAsync(e => e.Id == escala.Id);

            return CreatedAtAction(
                nameof(GetEscala),
                new { id = escala.Id },
                ParaDto(escala)
            );
        }

        // ============================================================
        // PUT - EDITAR ESCALA
        // ============================================================

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EditarEscala(
            int id,
            [FromBody] EscalaDto dados)
        {
            var escala =
                await _context.Escalas
                    .FirstOrDefaultAsync(e => e.Id == id);

            if (escala == null)
            {
                return NotFound(
                    new
                    {
                        mensagem = "Escala não encontrada."
                    }
                );
            }

            // ========================================================
            // A NOVA DATA/EMPREGADO PRECISA TER UMA LOTACAO VALIDA
            // ========================================================

            var validacao =
                await ValidarEscalaAsync(dados);

            if (!validacao.Valido)
            {
                return BadRequest(
                    new
                    {
                        mensagem = validacao.Erro
                    }
                );
            }

            var lotacao = validacao.Lotacao!;
            var turno = validacao.TipoTurno!;

            var postoEfetivo =
                dados.PostoId ?? lotacao.PostoId;

            // ========================================================
            // VERIFICAR CONFLITOS COM OUTRAS ESCALAS
            // ========================================================

            var outrasEscalas =
                await _context.Escalas
                    .Where(e =>
                        e.FuncionarioId == dados.FuncionarioId &&
                        e.Id != id)
                    .ToListAsync();

            foreach (var outra in outrasEscalas)
            {
                if (ExisteConflito(
                    outra.Data,
                    outra.HoraInicio,
                    outra.HoraFim,
                    dados.Data.Date,
                    dados.HoraInicio,
                    dados.HoraFim))
                {
                    return Conflict(
                        new
                        {
                            mensagem =
                                "A alteração cria conflito com outra escala do funcionário.",
                            escalaId = outra.Id
                        }
                    );
                }
            }

            // ========================================================
            // ATUALIZAR ESCALA
            //
            // Como Escala é um SNAPSHOT, se alterarmos funcionário,
            // data, unidade, equipa ou função, atualizamos também
            // o snapshot.
            // ========================================================

            escala.FuncionarioId = dados.FuncionarioId;

            escala.Data = dados.Data.Date;

            escala.HoraInicio = dados.HoraInicio;
            escala.HoraFim = dados.HoraFim;

            escala.TipoTurnoId = turno.Id;

            escala.PostoId = postoEfetivo;

            escala.Observacao = dados.Observacao;

            // SNAPSHOT
            escala.SeccaoId = lotacao.SeccaoId;
            escala.UnidadeOperacionalId = lotacao.UnidadeOperacionalId;
            escala.EquipaId = lotacao.EquipaId;
            escala.FuncaoOperacionalId = lotacao.FuncaoOperacionalId;

            await _context.SaveChangesAsync();

            var escalaAtualizada =
                await QueryEscalasComSnapshot()
                    .FirstAsync(e => e.Id == id);

            return Ok(
                ParaDto(escalaAtualizada)
            );
        }

        // ============================================================
        // DELETE
        // ============================================================

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EliminarEscala(int id)
        {
            var escala =
                await _context.Escalas
                    .FirstOrDefaultAsync(e => e.Id == id);

            if (escala == null)
            {
                return NotFound(
                    new
                    {
                        mensagem = "Escala não encontrada."
                    }
                );
            }

            _context.Escalas.Remove(escala);

            await _context.SaveChangesAsync();

            return Ok(
                new
                {
                    mensagem = "Escala eliminada com sucesso."
                }
            );
        }

        // ============================================================
        // GERAR ESCALA AUTOMÁTICA
        // ============================================================

        [HttpPost("gerar-automatica")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<GerarEscalaResultadoDto>>
            GerarEscalaAutomatica(
                [FromBody] GerarEscalaDto dados)
        {
            try
            {
                // ========================================================
                // VALIDACOES INICIAIS
                // ========================================================

                if (dados.UnidadeOperacionalId <= 0)
                {
                    return BadRequest(
                        new
                        {
                            mensagem = "Unidade operacional inválida."
                        }
                    );
                }

                if (!HoraValida(dados.HoraInicio))
                {
                    return BadRequest(
                        new
                        {
                            mensagem = "Hora de início inválida."
                        }
                    );
                }

                var dataEscala = dados.Data.Date;

                // ========================================================
                // UNIDADE
                // ========================================================

                var unidade =
                    await _context.UnidadesOperacionais
                        .FirstOrDefaultAsync(u =>
                            u.Id == dados.UnidadeOperacionalId &&
                            u.Ativo);

                if (unidade == null)
                {
                    return NotFound(
                        new
                        {
                            mensagem =
                                "Unidade operacional não encontrada ou está inativa."
                        }
                    );
                }

                // ========================================================
                // GRUPOS DE ROTACAO
                // ========================================================

                var grupos =
                    await _context.GruposEscala
                        .Include(g => g.UnidadeOperacional)
                        .Include(g => g.Equipa)
                        .Include(g => g.TipoTurno)
                        .Where(g =>
                            g.Ativo &&
                            g.UnidadeOperacionalId ==
                            dados.UnidadeOperacionalId)
                        .OrderBy(g => g.OrdemRotacao)
                        .ToListAsync();

                if (grupos.Count == 0)
                {
                    return BadRequest(
                        new
                        {
                            mensagem =
                                "Não existem grupos de escala ativos para esta unidade operacional."
                        }
                    );
                }

                // ========================================================
                // VALIDAR ORDENS DE ROTACAO
                //
                // Devemos ter:
                // 1, 2, 3...
                // ========================================================

                for (int i = 0; i < grupos.Count; i++)
                {
                    var ordemEsperada = i + 1;

                    if (grupos[i].OrdemRotacao != ordemEsperada)
                    {
                        return BadRequest(
                            new
                            {
                                mensagem =
                                    $"A rotação da unidade '{unidade.Nome}' está inválida. " +
                                    $"Esperada ordem {ordemEsperada}, encontrada {grupos[i].OrdemRotacao}."
                            }
                        );
                    }
                }

                // ========================================================
                // GRUPO DE REFERENCIA
                //
                // Exemplo Escolta C:
                //
                // 09/07 = Charlie ordem 1
                // 09/08 = Alfa    ordem 2
                // 09/09 = Beta    ordem 3
                // 09/10 = Charlie ordem 1
                //
                // ========================================================

                var grupoReferencia =
                    grupos.First();

                var dias =
                    (dataEscala.Date -
                     grupoReferencia.DataReferencia.Date).Days;

                var indiceRotacao =
                    ((dias % grupos.Count) + grupos.Count) %
                    grupos.Count;

                var ordemEmServico =
                    indiceRotacao + 1;

                var grupoServico =
                    grupos.FirstOrDefault(g =>
                        g.OrdemRotacao == ordemEmServico);

                if (grupoServico == null)
                {
                    return BadRequest(
                        new
                        {
                            mensagem =
                                $"Não foi possível determinar o grupo de rotação para a data {dataEscala:dd/MM/yyyy}."
                        }
                    );
                }

                // ========================================================
                // EQUIPA
                // ========================================================

                if (grupoServico.Equipa == null)
                {
                    return BadRequest(
                        new
                        {
                            mensagem =
                                $"O grupo '{grupoServico.Nome}' não possui uma equipa associada."
                        }
                    );
                }

                if (grupoServico.Equipa.UnidadeOperacionalId !=
                    dados.UnidadeOperacionalId)
                {
                    return BadRequest(
                        new
                        {
                            mensagem =
                                "A equipa do grupo de escala não pertence à unidade operacional selecionada."
                        }
                    );
                }

                // ========================================================
                // TURNO
                // ========================================================

                var turnoId =
                    dados.TipoTurnoId ??
                    grupoServico.TipoTurnoId;

                if (dados.TipoTurnoId.HasValue &&
                    dados.TipoTurnoId.Value != grupoServico.TipoTurnoId)
                {
                    return BadRequest(
                        new
                        {
                            mensagem =
                                $"O turno informado não corresponde ao turno configurado no grupo '{grupoServico.Nome}'."
                        }
                    );
                }

                var turno =
                    await _context.TiposTurno
                        .FirstOrDefaultAsync(t =>
                            t.Id == turnoId &&
                            t.Ativo);

                if (turno == null)
                {
                    return BadRequest(
                        new
                        {
                            mensagem =
                                "O tipo de turno não foi encontrado ou está inativo."
                        }
                    );
                }

                if (turno.HorasTrabalho <= 0)
                {
                    return BadRequest(
                        new
                        {
                            mensagem =
                                "O tipo de turno não possui uma quantidade de horas de trabalho válida."
                        }
                    );
                }

                // ========================================================
                // CALCULAR HORA FINAL
                // ========================================================

                var inicioDateTime =
                    CalcularInicio(
                        dataEscala,
                        dados.HoraInicio);

                var fimDateTime =
                    inicioDateTime.AddHours(
                        turno.HorasTrabalho);

                var horaFim =
                    fimDateTime.TimeOfDay;

                // ========================================================
                // LOTACOES VALIDAS NA DATA DA ESCALA
                //
                // ESTA É UMA CORREÇÃO MUITO IMPORTANTE.
                //
                // Não usamos:
                //
                //     l.Ativo
                //
                // Porque uma lotação antiga pode estar inativa hoje,
                // mas ter sido perfeitamente válida na data histórica
                // da escala.
                // ========================================================

                var lotacoes =
                    await _context.LotacoesFuncionarios
                        .Include(l => l.Funcionario)
                        .Include(l => l.Seccao)
                        .Include(l => l.UnidadeOperacional)
                        .Include(l => l.Equipa)
                        .Include(l => l.FuncaoOperacional)
                        .Include(l => l.TipoTurno)
                        .Include(l => l.Posto)
                        .Where(l =>
                            l.UnidadeOperacionalId ==
                                dados.UnidadeOperacionalId &&

                            l.EquipaId ==
                                grupoServico.EquipaId &&

                            l.DataInicio.Date <= dataEscala &&

                            (
                                l.DataFim == null ||
                                l.DataFim.Value.Date > dataEscala
                            )
                        )
                        .OrderBy(l => l.Funcionario!.NomeCompleto)
                        .ToListAsync();

                // ========================================================
                // RESULTADO
                // ========================================================

                var resultado =
                    new GerarEscalaResultadoDto
                    {
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
                            dataEscala,

                        HoraInicio =
                            dados.HoraInicio,

                        HoraFim =
                            horaFim,

                        TipoTurnoId =
                            turno.Id,

                        TipoTurnoNome =
                            turno.Nome,

                        FuncionariosNaEquipa =
                            lotacoes.Count
                    };

                if (lotacoes.Count == 0)
                {
                    resultado.Mensagem =
                        $"Não existem funcionários com lotação válida na equipa '{grupoServico.Equipa.Nome}' para a data {dataEscala:dd/MM/yyyy}.";

                    return Ok(resultado);
                }

                // ========================================================
                // SEPARAR LOTACOES COM TURNO VALIDO / INVALIDO
                // ========================================================

                var lotacoesValidas =
                    new List<LotacaoFuncionario>();

                foreach (var lotacao in lotacoes)
                {
                    var dtoFuncionario =
                        new FuncionarioEscalaResultadoDto
                        {
                            FuncionarioId =
                                lotacao.FuncionarioId,

                            Nome =
                                lotacao.Funcionario?.NomeCompleto,

                            Nip =
                                lotacao.Funcionario?.Nip,

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

                    if (lotacao.FuncaoOperacionalId <= 0)
                    {
                        resultado.FuncionariosComTurnoInvalido
                            .Add(dtoFuncionario);

                        continue;
                    }

                    if (lotacao.TipoTurnoId.HasValue &&
                        lotacao.TipoTurnoId.Value != turno.Id)
                    {
                        resultado.FuncionariosComTurnoInvalido
                            .Add(dtoFuncionario);

                        continue;
                    }

                    lotacoesValidas.Add(lotacao);
                }

                resultado.FuncionariosComLotacaoValida =
                    lotacoesValidas.Count;

                resultado.QuantidadeFuncionariosComTurnoInvalido =
                    resultado.FuncionariosComTurnoInvalido.Count;

                // ========================================================
                // BUSCAR ESCALAS EXISTENTES
                // ========================================================

                var funcionarioIds =
                    lotacoesValidas
                        .Select(l => l.FuncionarioId)
                        .Distinct()
                        .ToList();

                var escalasExistentes =
                    await _context.Escalas
                        .Where(e =>
                            funcionarioIds.Contains(e.FuncionarioId))
                        .ToListAsync();

                // ========================================================
                // PROCESSAR FUNCIONARIO POR FUNCIONARIO
                // ========================================================

                foreach (var lotacao in lotacoesValidas)
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
                                funcionario.Id,

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

                    var postoEfetivo =
                        dados.PostoId ??
                        lotacao.PostoId;

                    // ====================================================
                    // 1. PRIMEIRO: VERIFICAR DUPLICADO EXATO
                    //
                    // Se a mesma geração for executada novamente,
                    // não devemos chamar isso de conflito.
                    // ====================================================

                    var escalaIgual =
                        escalasExistentes.FirstOrDefault(e =>
                            e.FuncionarioId ==
                                funcionario.Id &&

                            EscalaExatamenteIgual(
                                e,
                                dataEscala,
                                dados.HoraInicio,
                                horaFim,
                                turno.Id,
                                postoEfetivo));

                    if (escalaIgual != null)
                    {
                        resultado.EscalasJaExistentes++;

                        resultado.FuncionariosJaEscalados
                            .Add(resultadoFuncionario);

                        continue;
                    }

                    // ====================================================
                    // 2. DEPOIS: VERIFICAR CONFLITO
                    // ====================================================

                    var conflito =
                        escalasExistentes.FirstOrDefault(e =>
                            e.FuncionarioId ==
                                funcionario.Id &&

                            ExisteConflito(
                                e.Data,
                                e.HoraInicio,
                                e.HoraFim,
                                dataEscala,
                                dados.HoraInicio,
                                horaFim));

                    if (conflito != null)
                    {
                        resultado.QuantidadeFuncionariosComConflito++;

                        resultado.FuncionariosComConflito
                            .Add(resultadoFuncionario);

                        continue;
                    }

                    // ====================================================
                    // 3. CRIAR ESCALA
                    //
                    // Guardamos o snapshot da lotação.
                    // ====================================================

                    var novaEscala =
                        new Escala
                        {
                            FuncionarioId =
                                funcionario.Id,

                            Data =
                                dataEscala,

                            HoraInicio =
                                dados.HoraInicio,

                            HoraFim =
                                horaFim,

                            TipoTurnoId =
                                turno.Id,

                            PostoId =
                                postoEfetivo,

                            Observacao =
                                dados.Observacao,

                            // SNAPSHOT
                            SeccaoId =
                                lotacao.SeccaoId,

                            UnidadeOperacionalId =
                                lotacao.UnidadeOperacionalId,

                            EquipaId =
                                lotacao.EquipaId,

                            FuncaoOperacionalId =
                                lotacao.FuncaoOperacionalId
                        };

                    _context.Escalas.Add(novaEscala);

                    // Adicionar imediatamente à lista para impedir
                    // que outro funcionário/iteração gere duplicação
                    // dentro da mesma operação.
                    escalasExistentes.Add(novaEscala);

                    resultado.EscalasCriadas++;

                    resultado.FuncionariosEscalados
                        .Add(resultadoFuncionario);
                }

                // ========================================================
                // GUARDAR
                // ========================================================

                await _context.SaveChangesAsync();

                // ========================================================
                // MENSAGEM FINAL
                // ========================================================

                if (resultado.EscalasCriadas > 0 &&
                    resultado.EscalasJaExistentes == 0 &&
                    resultado.QuantidadeFuncionariosComConflito == 0)
                {
                    resultado.Mensagem =
                        $"Escala automática gerada com sucesso para {resultado.EscalasCriadas} funcionário(s).";
                }
                else
                {
                    resultado.Mensagem =
                        $"Processamento concluído. " +
                        $"Criadas: {resultado.EscalasCriadas}; " +
                        $"já existentes: {resultado.EscalasJaExistentes}; " +
                        $"conflitos: {resultado.QuantidadeFuncionariosComConflito}; " +
                        $"turno inválido: {resultado.QuantidadeFuncionariosComTurnoInvalido}.";
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    erro = "Erro ao gerar escala automática.",
                    tipo = ex.GetType().FullName,
                    mensagem = ex.Message,
                    innerException = ex.InnerException?.Message,
                    innerException2 = ex.InnerException?.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
            }

        }
        // ============================================================
        // DIAGNÓSTICO TEMPORÁRIO DA ESTRUTURA DE ESCALAS
        // REMOVER DEPOIS DA REPARAÇÃO DA BASE DE DADOS
        // ============================================================

        [HttpGet("diagnostico-banco")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DiagnosticoBanco()
        {
            var totalEscalas = await _context.Escalas.CountAsync();

            var funcoes = await _context.FuncoesOperacionais
                .OrderBy(f => f.Id)
                .Select(f => new
                {
                    f.Id,
                    f.Nome,
                    f.Ativo
                })
                .ToListAsync();

            var escalas = await _context.Escalas
                .OrderBy(e => e.Id)
                .Select(e => new
                {
                    e.Id,
                    e.FuncionarioId,
                    e.Data,
                    e.FuncaoOperacionalId,
                    e.EquipaId,
                    e.SeccaoId,
                    e.UnidadeOperacionalId
                })
                .ToListAsync();

            var funcoesUsadas = escalas
                .GroupBy(e => e.FuncaoOperacionalId)
                .Select(g => new
                {
                    FuncaoOperacionalId = g.Key,
                    Quantidade = g.Count()
                })
                .OrderBy(x => x.FuncaoOperacionalId)
                .ToList();

            var funcoesInvalidas = escalas
                .Where(e =>
                    e.FuncaoOperacionalId <= 0 ||
                    !funcoes.Any(f => f.Id == e.FuncaoOperacionalId))
                .Select(e => new
                {
                    e.Id,
                    e.FuncionarioId,
                    e.Data,
                    e.FuncaoOperacionalId
                })
                .ToList();

            var lotacoes = await _context.LotacoesFuncionarios
                .Where(l => l.Ativo)
                .OrderBy(l => l.FuncionarioId)
                .Select(l => new
                {
                    l.Id,
                    l.FuncionarioId,
                    l.FuncaoOperacionalId,
                    l.DataInicio,
                    l.DataFim,
                    l.Ativo
                })
                .ToListAsync();

            return Ok(new
            {
                totalEscalas,

                funcoes,

                funcoesUsadas,

                funcoesInvalidas,

                lotacoesAtivas = lotacoes
            });
        }
    }
}