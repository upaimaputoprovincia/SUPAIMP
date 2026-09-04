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
    public class EscalasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EscalasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // DTO DE ENTRADA
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
        // DTO DE RESPOSTA
        // ============================================================
        public class EscalaRespostaDto
        {
            public int Id { get; set; }

            // --------------------------------------------------------
            // FUNCIONÁRIO
            // --------------------------------------------------------
            public int FuncionarioId { get; set; }
            public string? Funcionario { get; set; }

            // --------------------------------------------------------
            // DATA E HORÁRIO
            // --------------------------------------------------------
            public DateTime Data { get; set; }

            public TimeSpan HoraInicio { get; set; }
            public TimeSpan HoraFim { get; set; }

            // --------------------------------------------------------
            // TURNO
            // --------------------------------------------------------
            public int TipoTurnoId { get; set; }
            public string? TipoTurno { get; set; }

            // --------------------------------------------------------
            // POSTO
            // --------------------------------------------------------
            public int? PostoId { get; set; }
            public string? Posto { get; set; }

            // --------------------------------------------------------
            // OBSERVAÇÃO
            // --------------------------------------------------------
            public string? Observacao { get; set; }

            // ========================================================
            // ORGANIZAÇÃO / LOTAÇÃO ATUAL
            // ========================================================

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
        // MÉTODO AUXILIAR
        // Converte Escala + Lotação para DTO seguro
        // ============================================================
        private static EscalaRespostaDto ParaDto(
            Escala escala,
            LotacaoFuncionario? lotacao)
        {
            return new EscalaRespostaDto
            {
                Id = escala.Id,

                // Funcionário
                FuncionarioId = escala.FuncionarioId,
                Funcionario = escala.Funcionario?.NomeCompleto,

                // Data
                Data = escala.Data,

                // Horário
                HoraInicio = escala.HoraInicio,
                HoraFim = escala.HoraFim,

                // Turno
                TipoTurnoId = escala.TipoTurnoId,
                TipoTurno = escala.TipoTurno?.Nome,

                // Posto
                PostoId = escala.PostoId,
                Posto = escala.Posto?.Nome,

                // Observação
                Observacao = escala.Observacao,

                // ====================================================
                // LOTAÇÃO ATUAL
                // ====================================================

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
        // Evita fazer uma consulta à BD para cada escala
        // ============================================================
        private async Task<Dictionary<int, LotacaoFuncionario>>
            CarregarLotacoesAsync(IEnumerable<int> funcionarioIds)
        {
            var ids = funcionarioIds
                .Distinct()
                .ToList();

            if (!ids.Any())
            {
                return new Dictionary<int, LotacaoFuncionario>();
            }

            var lotacoes = await _context.LotacoesFuncionarios
                .AsNoTracking()

                .Include(l => l.Seccao)
                .Include(l => l.UnidadeOperacional)
                .Include(l => l.Equipa)
                .Include(l => l.FuncaoOperacional)

                .Where(l =>
                    ids.Contains(l.FuncionarioId) &&
                    l.Ativo)

                .OrderByDescending(l => l.DataInicio)
                .ToListAsync();

            return lotacoes
                .GroupBy(l => l.FuncionarioId)
                .ToDictionary(
                    g => g.Key,
                    g => g.First());
        }

        // ============================================================
        // GET: api/Escalas
        // ============================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EscalaRespostaDto>>>
            GetEscalas()
        {
            var escalas = await _context.Escalas
                .AsNoTracking()
                .Include(e => e.Funcionario)
                .Include(e => e.TipoTurno)
                .Include(e => e.Posto)
                .OrderByDescending(e => e.Data)
                .ThenBy(e => e.HoraInicio)
                .ToListAsync();

            var lotacoes = await CarregarLotacoesAsync(
                escalas.Select(e => e.FuncionarioId));

            var resultado = escalas
                .Select(e =>
                    lotacoes.TryGetValue(
                        e.FuncionarioId,
                        out var lotacao)
                            ? ParaDto(e, lotacao)
                            : ParaDto(e, null))
                .ToList();

            return Ok(resultado);
        }

        // ============================================================
        // GET: api/Escalas/5
        // ============================================================
        [HttpGet("{id:int}")]
        public async Task<ActionResult<EscalaRespostaDto>>
            GetEscala(int id)
        {
            var escala = await _context.Escalas
                .AsNoTracking()
                .Include(e => e.Funcionario)
                .Include(e => e.TipoTurno)
                .Include(e => e.Posto)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (escala == null)
            {
                return NotFound(new
                {
                    mensagem = "Escala não encontrada."
                });
            }

            var lotacoes = await CarregarLotacoesAsync(
                new[] { escala.FuncionarioId });

            lotacoes.TryGetValue(
                escala.FuncionarioId,
                out var lotacao);

            return Ok(ParaDto(escala, lotacao));
        }

        // ============================================================
        // GET: api/Escalas/data/2026-09-04
        // ============================================================
        [HttpGet("data/{data}")]
        public async Task<ActionResult<IEnumerable<EscalaRespostaDto>>>
            GetPorData(DateTime data)
        {
            var inicio = data.Date;
            var fim = inicio.AddDays(1);

            var escalas = await _context.Escalas
                .AsNoTracking()
                .Include(e => e.Funcionario)
                .Include(e => e.TipoTurno)
                .Include(e => e.Posto)
                .Where(e =>
                    e.Data >= inicio &&
                    e.Data < fim)
                .OrderBy(e => e.HoraInicio)
                .ToListAsync();

            var lotacoes = await CarregarLotacoesAsync(
                escalas.Select(e => e.FuncionarioId));

            var resultado = escalas
                .Select(e =>
                    lotacoes.TryGetValue(
                        e.FuncionarioId,
                        out var lotacao)
                            ? ParaDto(e, lotacao)
                            : ParaDto(e, null))
                .ToList();

            return Ok(resultado);
        }

        // ============================================================
        // GET: api/Escalas/hoje
        // ============================================================
        [HttpGet("hoje")]
        public async Task<ActionResult<IEnumerable<EscalaRespostaDto>>>
            GetHoje()
        {
            var hoje = DateTime.Today;
            var amanha = hoje.AddDays(1);

            var escalas = await _context.Escalas
                .AsNoTracking()
                .Include(e => e.Funcionario)
                .Include(e => e.TipoTurno)
                .Include(e => e.Posto)
                .Where(e =>
                    e.Data >= hoje &&
                    e.Data < amanha)
                .OrderBy(e => e.HoraInicio)
                .ToListAsync();

            var lotacoes = await CarregarLotacoesAsync(
                escalas.Select(e => e.FuncionarioId));

            var resultado = escalas
                .Select(e =>
                    lotacoes.TryGetValue(
                        e.FuncionarioId,
                        out var lotacao)
                            ? ParaDto(e, lotacao)
                            : ParaDto(e, null))
                .ToList();

            return Ok(resultado);
        }

        // ============================================================
        // GET: api/Escalas/ontem
        // ============================================================
        [HttpGet("ontem")]
        public async Task<ActionResult<IEnumerable<EscalaRespostaDto>>>
            GetOntem()
        {
            var ontem = DateTime.Today.AddDays(-1);
            var hoje = DateTime.Today;

            var escalas = await _context.Escalas
                .AsNoTracking()
                .Include(e => e.Funcionario)
                .Include(e => e.TipoTurno)
                .Include(e => e.Posto)
                .Where(e =>
                    e.Data >= ontem &&
                    e.Data < hoje)
                .OrderBy(e => e.HoraInicio)
                .ToListAsync();

            var lotacoes = await CarregarLotacoesAsync(
                escalas.Select(e => e.FuncionarioId));

            var resultado = escalas
                .Select(e =>
                    lotacoes.TryGetValue(
                        e.FuncionarioId,
                        out var lotacao)
                            ? ParaDto(e, lotacao)
                            : ParaDto(e, null))
                .ToList();

            return Ok(resultado);
        }

        // ============================================================
        // GET: api/Escalas/amanha
        // ============================================================
        [HttpGet("amanha")]
        public async Task<ActionResult<IEnumerable<EscalaRespostaDto>>>
            GetAmanha()
        {
            var amanha = DateTime.Today.AddDays(1);
            var depois = amanha.AddDays(2);

            var escalas = await _context.Escalas
                .AsNoTracking()
                .Include(e => e.Funcionario)
                .Include(e => e.TipoTurno)
                .Include(e => e.Posto)
                .Where(e =>
                    e.Data >= amanha &&
                    e.Data < depois)
                .OrderBy(e => e.HoraInicio)
                .ToListAsync();

            var lotacoes = await CarregarLotacoesAsync(
                escalas.Select(e => e.FuncionarioId));

            var resultado = escalas
                .Select(e =>
                    lotacoes.TryGetValue(
                        e.FuncionarioId,
                        out var lotacao)
                            ? ParaDto(e, lotacao)
                            : ParaDto(e, null))
                .ToList();

            return Ok(resultado);
        }

        // ============================================================
        // GET: api/Escalas/funcionario/414
        // ============================================================
        [HttpGet("funcionario/{funcionarioId:int}")]
        public async Task<ActionResult<IEnumerable<EscalaRespostaDto>>>
            GetPorFuncionario(int funcionarioId)
        {
            var funcionarioExiste = await _context.Funcionarios
                .AnyAsync(f => f.Id == funcionarioId);

            if (!funcionarioExiste)
            {
                return NotFound(new
                {
                    mensagem = "Funcionário não encontrado."
                });
            }

            var escalas = await _context.Escalas
                .AsNoTracking()
                .Include(e => e.Funcionario)
                .Include(e => e.TipoTurno)
                .Include(e => e.Posto)
                .Where(e => e.FuncionarioId == funcionarioId)
                .OrderByDescending(e => e.Data)
                .ThenByDescending(e => e.HoraInicio)
                .ToListAsync();

            var lotacoes = await CarregarLotacoesAsync(
                new[] { funcionarioId });

            lotacoes.TryGetValue(
                funcionarioId,
                out var lotacao);

            var resultado = escalas
                .Select(e => ParaDto(e, lotacao))
                .ToList();

            return Ok(resultado);
        }

        // ============================================================
        // GET: api/Escalas/posto/5
        // ============================================================
        [HttpGet("posto/{postoId:int}")]
        public async Task<ActionResult<IEnumerable<EscalaRespostaDto>>>
            GetPorPosto(int postoId)
        {
            var postoExiste = await _context.Postos
                .AnyAsync(p => p.Id == postoId);

            if (!postoExiste)
            {
                return NotFound(new
                {
                    mensagem = "Posto não encontrado."
                });
            }

            var escalas = await _context.Escalas
                .AsNoTracking()
                .Include(e => e.Funcionario)
                .Include(e => e.TipoTurno)
                .Include(e => e.Posto)
                .Where(e => e.PostoId == postoId)
                .OrderByDescending(e => e.Data)
                .ThenBy(e => e.HoraInicio)
                .ToListAsync();

            var lotacoes = await CarregarLotacoesAsync(
                escalas.Select(e => e.FuncionarioId));

            var resultado = escalas
                .Select(e =>
                    lotacoes.TryGetValue(
                        e.FuncionarioId,
                        out var lotacao)
                            ? ParaDto(e, lotacao)
                            : ParaDto(e, null))
                .ToList();

            return Ok(resultado);
        }

        // ============================================================
        // GET: api/Escalas/tipo-turno/1
        // ============================================================
        [HttpGet("tipo-turno/{tipoTurnoId:int}")]
        public async Task<ActionResult<IEnumerable<EscalaRespostaDto>>>
            GetPorTipoTurno(int tipoTurnoId)
        {
            var turnoExiste = await _context.TiposTurno
                .AnyAsync(t => t.Id == tipoTurnoId);

            if (!turnoExiste)
            {
                return NotFound(new
                {
                    mensagem = "Tipo de turno não encontrado."
                });
            }

            var escalas = await _context.Escalas
                .AsNoTracking()
                .Include(e => e.Funcionario)
                .Include(e => e.TipoTurno)
                .Include(e => e.Posto)
                .Where(e => e.TipoTurnoId == tipoTurnoId)
                .OrderByDescending(e => e.Data)
                .ThenBy(e => e.HoraInicio)
                .ToListAsync();

            var lotacoes = await CarregarLotacoesAsync(
                escalas.Select(e => e.FuncionarioId));

            var resultado = escalas
                .Select(e =>
                    lotacoes.TryGetValue(
                        e.FuncionarioId,
                        out var lotacao)
                            ? ParaDto(e, lotacao)
                            : ParaDto(e, null))
                .ToList();

            return Ok(resultado);
        }

        // ============================================================
        // POST: api/Escalas
        // ============================================================
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<EscalaRespostaDto>> Criar(
            [FromBody] EscalaDto dados)
        {
            if (dados == null)
            {
                return BadRequest(new
                {
                    mensagem = "Os dados da escala são obrigatórios."
                });
            }

            // --------------------------------------------------------
            // Validar funcionário
            // --------------------------------------------------------
            var funcionario = await _context.Funcionarios
                .FirstOrDefaultAsync(
                    f => f.Id == dados.FuncionarioId);

            if (funcionario == null)
            {
                return BadRequest(new
                {
                    mensagem = "Funcionário não encontrado."
                });
            }

            // --------------------------------------------------------
            // Validar turno
            // --------------------------------------------------------
            var turno = await _context.TiposTurno
                .FirstOrDefaultAsync(t =>
                    t.Id == dados.TipoTurnoId &&
                    t.Ativo);

            if (turno == null)
            {
                return BadRequest(new
                {
                    mensagem =
                        "Tipo de turno não encontrado ou está inactivo."
                });
            }

            // --------------------------------------------------------
            // Validar posto
            // --------------------------------------------------------
            if (dados.PostoId.HasValue)
            {
                var posto = await _context.Postos
                    .FirstOrDefaultAsync(p =>
                        p.Id == dados.PostoId.Value &&
                        p.Ativo);

                if (posto == null)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "Posto não encontrado ou está inactivo."
                    });
                }
            }

            // --------------------------------------------------------
            // Validar horários
            // --------------------------------------------------------
            if (dados.HoraInicio == dados.HoraFim)
            {
                return BadRequest(new
                {
                    mensagem =
                        "A hora de início não pode ser igual à hora de fim."
                });
            }

            // --------------------------------------------------------
            // Verificar conflito
            // --------------------------------------------------------
            var conflito = await _context.Escalas
                .Where(e =>
                    e.FuncionarioId == dados.FuncionarioId)
                .Where(e =>
                    e.Data.Date == dados.Data.Date)
                .AnyAsync(e =>
                    e.HoraInicio < dados.HoraFim &&
                    e.HoraFim > dados.HoraInicio);

            if (conflito)
            {
                return Conflict(new
                {
                    mensagem =
                        "O funcionário já possui uma escala que entra " +
                        "em conflito com este período."
                });
            }

            // --------------------------------------------------------
            // Criar escala
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
            // Recarregar
            // --------------------------------------------------------
            var criada = await _context.Escalas
                .AsNoTracking()
                .Include(e => e.Funcionario)
                .Include(e => e.TipoTurno)
                .Include(e => e.Posto)
                .FirstAsync(e => e.Id == escala.Id);

            var lotacoes = await CarregarLotacoesAsync(
                new[] { criada.FuncionarioId });

            lotacoes.TryGetValue(
                criada.FuncionarioId,
                out var lotacao);

            return CreatedAtAction(
                nameof(GetEscala),
                new { id = criada.Id },
                ParaDto(criada, lotacao));
        }

        // ============================================================
        // PUT: api/Escalas/5
        // ============================================================
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Editar(
            int id,
            [FromBody] EscalaDto dados)
        {
            if (dados == null)
            {
                return BadRequest(new
                {
                    mensagem = "Os dados da escala são obrigatórios."
                });
            }

            var escala = await _context.Escalas
                .FirstOrDefaultAsync(e => e.Id == id);

            if (escala == null)
            {
                return NotFound(new
                {
                    mensagem = "Escala não encontrada."
                });
            }

            // --------------------------------------------------------
            // Validar funcionário
            // --------------------------------------------------------
            var funcionarioExiste = await _context.Funcionarios
                .AnyAsync(f => f.Id == dados.FuncionarioId);

            if (!funcionarioExiste)
            {
                return BadRequest(new
                {
                    mensagem = "Funcionário não encontrado."
                });
            }

            // --------------------------------------------------------
            // Validar turno
            // --------------------------------------------------------
            var turnoExiste = await _context.TiposTurno
                .AnyAsync(t =>
                    t.Id == dados.TipoTurnoId &&
                    t.Ativo);

            if (!turnoExiste)
            {
                return BadRequest(new
                {
                    mensagem =
                        "Tipo de turno não encontrado ou está inactivo."
                });
            }

            // --------------------------------------------------------
            // Validar posto
            // --------------------------------------------------------
            if (dados.PostoId.HasValue)
            {
                var postoExiste = await _context.Postos
                    .AnyAsync(p =>
                        p.Id == dados.PostoId.Value &&
                        p.Ativo);

                if (!postoExiste)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "Posto não encontrado ou está inactivo."
                    });
                }
            }

            // --------------------------------------------------------
            // Validar horário
            // --------------------------------------------------------
            if (dados.HoraInicio == dados.HoraFim)
            {
                return BadRequest(new
                {
                    mensagem =
                        "A hora de início não pode ser igual à hora de fim."
                });
            }

            // --------------------------------------------------------
            // Verificar conflitos
            // --------------------------------------------------------
            var conflito = await _context.Escalas
                .Where(e => e.Id != id)
                .Where(e =>
                    e.FuncionarioId == dados.FuncionarioId)
                .Where(e =>
                    e.Data.Date == dados.Data.Date)
                .AnyAsync(e =>
                    e.HoraInicio < dados.HoraFim &&
                    e.HoraFim > dados.HoraInicio);

            if (conflito)
            {
                return Conflict(new
                {
                    mensagem =
                        "O funcionário já possui outra escala que " +
                        "entra em conflito com este período."
                });
            }

            // --------------------------------------------------------
            // Atualizar
            // --------------------------------------------------------
            escala.FuncionarioId = dados.FuncionarioId;
            escala.Data = dados.Data.Date;
            escala.HoraInicio = dados.HoraInicio;
            escala.HoraFim = dados.HoraFim;
            escala.TipoTurnoId = dados.TipoTurnoId;
            escala.PostoId = dados.PostoId;
            escala.Observacao = dados.Observacao;

            await _context.SaveChangesAsync();

            // --------------------------------------------------------
            // Recarregar
            // --------------------------------------------------------
            var atualizada = await _context.Escalas
                .AsNoTracking()
                .Include(e => e.Funcionario)
                .Include(e => e.TipoTurno)
                .Include(e => e.Posto)
                .FirstAsync(e => e.Id == id);

            var lotacoes = await CarregarLotacoesAsync(
                new[] { atualizada.FuncionarioId });

            lotacoes.TryGetValue(
                atualizada.FuncionarioId,
                out var lotacao);

            return Ok(new
            {
                mensagem = "Escala actualizada com sucesso.",
                escala = ParaDto(atualizada, lotacao)
            });
        }

        // ============================================================
        // DELETE: api/Escalas/5
        // ============================================================
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Eliminar(int id)
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

            _context.Escalas.Remove(escala);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Escala eliminada com sucesso."
            });
        }
    }
}