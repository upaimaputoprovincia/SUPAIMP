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
    public class GrupoEscalaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GrupoEscalaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // DTO DE ENTRADA
        // ============================================================

        public class GrupoEscalaDto
        {
            public string Nome { get; set; } = string.Empty;

            public int OrdemRotacao { get; set; }

            public int UnidadeOperacionalId { get; set; }

            public int EquipaId { get; set; }

            public int TipoTurnoId { get; set; }

            public DateTime DataReferencia { get; set; }

            public bool Ativo { get; set; } = true;

            public string? Observacao { get; set; }
        }

        // ============================================================
        // DTO DE RESPOSTA
        // ============================================================

        public class GrupoEscalaRespostaDto
        {
            public int Id { get; set; }

            public string Nome { get; set; } = string.Empty;

            public int OrdemRotacao { get; set; }

            public int UnidadeOperacionalId { get; set; }

            public string? UnidadeOperacional { get; set; }

            public int EquipaId { get; set; }

            public string? Equipa { get; set; }

            public int TipoTurnoId { get; set; }

            public string? TipoTurno { get; set; }

            public int HorasTrabalho { get; set; }

            public int HorasDescanso { get; set; }

            public DateTime DataReferencia { get; set; }

            public bool Ativo { get; set; }

            public string? Observacao { get; set; }

            public DateTime DataCadastro { get; set; }
        }

        // ============================================================
        // DTO DO SERVIÇO DO DIA
        // ============================================================

        public class ServicoGrupoDto
        {
            public int GrupoId { get; set; }

            public string Grupo { get; set; } = string.Empty;

            public int UnidadeOperacionalId { get; set; }

            public string? UnidadeOperacional { get; set; }

            public int EquipaId { get; set; }

            public string? Equipa { get; set; }

            public int TipoTurnoId { get; set; }

            public string? TipoTurno { get; set; }

            public int HorasTrabalho { get; set; }

            public int HorasDescanso { get; set; }

            public DateTime Data { get; set; }

            public DateTime DataReferencia { get; set; }

            public List<FuncionarioServicoDto> Funcionarios { get; set; }
                = new();
        }

        // ============================================================
        // DTO DO FUNCIONÁRIO EM SERVIÇO
        // ============================================================

        public class FuncionarioServicoDto
        {
            public int Id { get; set; }

            public string NomeCompleto { get; set; } = string.Empty;

            public string? Nip { get; set; }

            public string? Funcao { get; set; }

            public int? PostoId { get; set; }

            public string? Posto { get; set; }
        }

        // ============================================================
        // CONVERTER GRUPO PARA DTO
        // ============================================================

        private static GrupoEscalaRespostaDto ParaDto(GrupoEscala grupo)
        {
            return new GrupoEscalaRespostaDto
            {
                Id = grupo.Id,

                Nome = grupo.Nome,

                OrdemRotacao = grupo.OrdemRotacao,

                UnidadeOperacionalId = grupo.UnidadeOperacionalId,

                UnidadeOperacional = grupo.UnidadeOperacional?.Nome,

                EquipaId = grupo.EquipaId,

                Equipa = grupo.Equipa?.Nome,

                TipoTurnoId = grupo.TipoTurnoId,

                TipoTurno = grupo.TipoTurno?.Nome,

                HorasTrabalho = grupo.TipoTurno?.HorasTrabalho ?? 0,

                HorasDescanso = grupo.TipoTurno?.HorasDescanso ?? 0,

                DataReferencia = grupo.DataReferencia,

                Ativo = grupo.Ativo,

                Observacao = grupo.Observacao,

                DataCadastro = grupo.DataCadastro
            };
        }

        // ============================================================
        // GET: api/GrupoEscala
        //
        // Lista todos os grupos
        // ============================================================

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GrupoEscalaRespostaDto>>>
            GetGrupos()
        {
            var grupos = await _context.GruposEscala
                .AsNoTracking()

                .Include(g => g.UnidadeOperacional)
                .Include(g => g.Equipa)
                .Include(g => g.TipoTurno)

                .OrderBy(g => g.UnidadeOperacionalId)
                .ThenBy(g => g.OrdemRotacao)

                .ToListAsync();

            return Ok(
                grupos.Select(ParaDto).ToList()
            );
        }

        // ============================================================
        // GET: api/GrupoEscala/5
        //
        // Consulta um grupo específico
        // ============================================================

        [HttpGet("{id:int}")]
        public async Task<ActionResult<GrupoEscalaRespostaDto>>
            GetGrupo(int id)
        {
            var grupo = await _context.GruposEscala
                .AsNoTracking()

                .Include(g => g.UnidadeOperacional)
                .Include(g => g.Equipa)
                .Include(g => g.TipoTurno)

                .FirstOrDefaultAsync(g => g.Id == id);

            if (grupo == null)
            {
                return NotFound(new
                {
                    mensagem = "Grupo de escala não encontrado."
                });
            }

            return Ok(ParaDto(grupo));
        }

        // ============================================================
        // GET:
        // api/GrupoEscala/unidade/1
        //
        // Lista os grupos de uma unidade operacional
        // ============================================================

        [HttpGet("unidade/{unidadeOperacionalId:int}")]
        public async Task<ActionResult<IEnumerable<GrupoEscalaRespostaDto>>>
            GetPorUnidade(int unidadeOperacionalId)
        {
            var unidadeExiste = await _context.UnidadesOperacionais
                .AnyAsync(u => u.Id == unidadeOperacionalId);

            if (!unidadeExiste)
            {
                return NotFound(new
                {
                    mensagem = "Unidade operacional não encontrada."
                });
            }

            var grupos = await _context.GruposEscala
                .AsNoTracking()

                .Include(g => g.UnidadeOperacional)
                .Include(g => g.Equipa)
                .Include(g => g.TipoTurno)

                .Where(g =>
                    g.UnidadeOperacionalId == unidadeOperacionalId &&
                    g.Ativo)

                .OrderBy(g => g.OrdemRotacao)

                .ToListAsync();

            return Ok(
                grupos.Select(ParaDto).ToList()
            );
        }

        // ============================================================
        // POST: api/GrupoEscala
        //
        // Criar grupo
        // ADMINISTRADOR
        // ============================================================

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<GrupoEscalaRespostaDto>>
            Criar([FromBody] GrupoEscalaDto dados)
        {
            if (dados == null)
            {
                return BadRequest(new
                {
                    mensagem = "Os dados do grupo são obrigatórios."
                });
            }

            // --------------------------------------------------------
            // Validar nome
            // --------------------------------------------------------

            if (string.IsNullOrWhiteSpace(dados.Nome))
            {
                return BadRequest(new
                {
                    mensagem = "O nome do grupo é obrigatório."
                });
            }

            // --------------------------------------------------------
            // Validar ordem
            // --------------------------------------------------------

            if (dados.OrdemRotacao < 1 || dados.OrdemRotacao > 3)
            {
                return BadRequest(new
                {
                    mensagem =
                        "A ordem de rotação deve estar entre 1 e 3."
                });
            }

            // --------------------------------------------------------
            // Validar unidade
            // --------------------------------------------------------

            var unidade = await _context.UnidadesOperacionais
                .FirstOrDefaultAsync(u =>
                    u.Id == dados.UnidadeOperacionalId);

            if (unidade == null)
            {
                return BadRequest(new
                {
                    mensagem =
                        "Unidade operacional não encontrada."
                });
            }

            // --------------------------------------------------------
            // Validar equipa
            // --------------------------------------------------------

            var equipa = await _context.Equipas
                .FirstOrDefaultAsync(e =>
                    e.Id == dados.EquipaId &&
                    e.Ativo);

            if (equipa == null)
            {
                return BadRequest(new
                {
                    mensagem =
                        "Equipa não encontrada ou está inactiva."
                });
            }

            // --------------------------------------------------------
            // Garantir que a equipa pertence à unidade
            // --------------------------------------------------------

            if (equipa.UnidadeOperacionalId !=
                dados.UnidadeOperacionalId)
            {
                return BadRequest(new
                {
                    mensagem =
                        "A equipa seleccionada não pertence à " +
                        "unidade operacional indicada."
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
            // Verificar ordem duplicada
            // --------------------------------------------------------

            var ordemExiste = await _context.GruposEscala
                .AnyAsync(g =>
                    g.UnidadeOperacionalId ==
                        dados.UnidadeOperacionalId &&
                    g.OrdemRotacao ==
                        dados.OrdemRotacao);

            if (ordemExiste)
            {
                return Conflict(new
                {
                    mensagem =
                        "Já existe um grupo com esta ordem de " +
                        "rotação nesta unidade operacional."
                });
            }

            // --------------------------------------------------------
            // Verificar equipa duplicada na mesma unidade
            // --------------------------------------------------------

            var equipaExiste = await _context.GruposEscala
                .AnyAsync(g =>
                    g.UnidadeOperacionalId ==
                        dados.UnidadeOperacionalId &&
                    g.EquipaId == dados.EquipaId);

            if (equipaExiste)
            {
                return Conflict(new
                {
                    mensagem =
                        "Esta equipa já está associada a um grupo " +
                        "de escala nesta unidade operacional."
                });
            }

            // --------------------------------------------------------
            // Criar grupo
            // --------------------------------------------------------

            var grupo = new GrupoEscala
            {
                Nome = dados.Nome.Trim(),

                OrdemRotacao = dados.OrdemRotacao,

                UnidadeOperacionalId =
                    dados.UnidadeOperacionalId,

                EquipaId = dados.EquipaId,

                TipoTurnoId = dados.TipoTurnoId,

                DataReferencia = dados.DataReferencia.Date,

                Ativo = dados.Ativo,

                Observacao = dados.Observacao,

                DataCadastro = DateTime.Now
            };

            _context.GruposEscala.Add(grupo);

            await _context.SaveChangesAsync();

            // --------------------------------------------------------
            // Recarregar
            // --------------------------------------------------------

            var criado = await _context.GruposEscala
                .AsNoTracking()

                .Include(g => g.UnidadeOperacional)
                .Include(g => g.Equipa)
                .Include(g => g.TipoTurno)

                .FirstAsync(g => g.Id == grupo.Id);

            return CreatedAtAction(
                nameof(GetGrupo),
                new { id = criado.Id },
                ParaDto(criado)
            );
        }

        // ============================================================
        // PUT: api/GrupoEscala/5
        //
        // Editar grupo
        // ADMINISTRADOR
        // ============================================================

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Editar(
            int id,
            [FromBody] GrupoEscalaDto dados)
        {
            if (dados == null)
            {
                return BadRequest(new
                {
                    mensagem = "Os dados do grupo são obrigatórios."
                });
            }

            var grupo = await _context.GruposEscala
                .FirstOrDefaultAsync(g => g.Id == id);

            if (grupo == null)
            {
                return NotFound(new
                {
                    mensagem = "Grupo de escala não encontrado."
                });
            }

            // --------------------------------------------------------
            // Validar ordem
            // --------------------------------------------------------

            if (dados.OrdemRotacao < 1 || dados.OrdemRotacao > 3)
            {
                return BadRequest(new
                {
                    mensagem =
                        "A ordem de rotação deve estar entre 1 e 3."
                });
            }

            // --------------------------------------------------------
            // Validar unidade
            // --------------------------------------------------------

            var unidadeExiste = await _context.UnidadesOperacionais
                .AnyAsync(u =>
                    u.Id == dados.UnidadeOperacionalId);

            if (!unidadeExiste)
            {
                return BadRequest(new
                {
                    mensagem =
                        "Unidade operacional não encontrada."
                });
            }

            // --------------------------------------------------------
            // Validar equipa
            // --------------------------------------------------------

            var equipa = await _context.Equipas
                .FirstOrDefaultAsync(e =>
                    e.Id == dados.EquipaId &&
                    e.Ativo);

            if (equipa == null)
            {
                return BadRequest(new
                {
                    mensagem =
                        "Equipa não encontrada ou está inactiva."
                });
            }

            if (equipa.UnidadeOperacionalId !=
                dados.UnidadeOperacionalId)
            {
                return BadRequest(new
                {
                    mensagem =
                        "A equipa seleccionada não pertence à " +
                        "unidade operacional indicada."
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
            // Verificar ordem duplicada
            // --------------------------------------------------------

            var ordemExiste = await _context.GruposEscala
                .AnyAsync(g =>
                    g.Id != id &&
                    g.UnidadeOperacionalId ==
                        dados.UnidadeOperacionalId &&
                    g.OrdemRotacao ==
                        dados.OrdemRotacao);

            if (ordemExiste)
            {
                return Conflict(new
                {
                    mensagem =
                        "Já existe outro grupo com esta ordem de " +
                        "rotação nesta unidade."
                });
            }

            // --------------------------------------------------------
            // Verificar equipa duplicada
            // --------------------------------------------------------

            var equipaExiste = await _context.GruposEscala
                .AnyAsync(g =>
                    g.Id != id &&
                    g.UnidadeOperacionalId ==
                        dados.UnidadeOperacionalId &&
                    g.EquipaId == dados.EquipaId);

            if (equipaExiste)
            {
                return Conflict(new
                {
                    mensagem =
                        "Esta equipa já está associada a outro " +
                        "grupo nesta unidade."
                });
            }

            // --------------------------------------------------------
            // Atualizar
            // --------------------------------------------------------

            grupo.Nome = dados.Nome.Trim();

            grupo.OrdemRotacao =
                dados.OrdemRotacao;

            grupo.UnidadeOperacionalId =
                dados.UnidadeOperacionalId;

            grupo.EquipaId =
                dados.EquipaId;

            grupo.TipoTurnoId =
                dados.TipoTurnoId;

            grupo.DataReferencia =
                dados.DataReferencia.Date;

            grupo.Ativo =
                dados.Ativo;

            grupo.Observacao =
                dados.Observacao;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem =
                    "Grupo de escala actualizado com sucesso."
            });
        }

        // ============================================================
        // PATCH: api/GrupoEscala/5/estado
        //
        // Ativar / desativar grupo
        // ============================================================

        [HttpPatch("{id:int}/estado")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> AlterarEstado(
            int id,
            [FromQuery] bool ativo)
        {
            var grupo = await _context.GruposEscala
                .FirstOrDefaultAsync(g => g.Id == id);

            if (grupo == null)
            {
                return NotFound(new
                {
                    mensagem = "Grupo de escala não encontrado."
                });
            }

            grupo.Ativo = ativo;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = ativo
                    ? "Grupo activado com sucesso."
                    : "Grupo desactivado com sucesso.",

                ativo
            });
        }

        // ============================================================
        // GET:
        // api/GrupoEscala/servico/2026-09-07
        //
        // DETERMINAR AUTOMATICAMENTE O GRUPO DE SERVIÇO
        // ============================================================

        [HttpGet("servico/{data}")]
        public async Task<ActionResult<IEnumerable<ServicoGrupoDto>>>
            GetServico(DateTime data)
        {
            var dataServico = data.Date;

            var grupos = await _context.GruposEscala
                .AsNoTracking()

                .Include(g => g.UnidadeOperacional)
                .Include(g => g.Equipa)
                .Include(g => g.TipoTurno)

                .Where(g => g.Ativo)

                .ToListAsync();

            if (!grupos.Any())
            {
                return Ok(new List<ServicoGrupoDto>());
            }

            var resultado = new List<ServicoGrupoDto>();

            // --------------------------------------------------------
            // Cada unidade operacional tem a sua própria rotação.
            // --------------------------------------------------------

            foreach (var unidade in grupos
                .GroupBy(g => g.UnidadeOperacionalId))
            {
                var gruposUnidade = unidade
                    .OrderBy(g => g.OrdemRotacao)
                    .ToList();

                if (gruposUnidade.Count == 0)
                {
                    continue;
                }

                // ----------------------------------------------------
                // A referência da rotação é a DataReferencia.
                //
                // A ordem 1 está de serviço na própria
                // DataReferencia.
                //
                // Ordem 2 entra no dia seguinte.
                //
                // Ordem 3 entra no segundo dia.
                //
                // Depois volta para ordem 1.
                // ----------------------------------------------------

                var grupoServico = gruposUnidade
                    .Select(g => new
                    {
                        Grupo = g,

                        DiferencaDias =
                            (dataServico -
                             g.DataReferencia.Date).Days
                    })
                    .Where(x =>
                        x.DiferencaDias >= 0 &&
                        x.DiferencaDias % 3 ==
                            x.Grupo.OrdemRotacao - 1)
                    .Select(x => x.Grupo)
                    .FirstOrDefault();

                // ----------------------------------------------------
                // Se a data estiver antes da referência,
                // precisamos calcular a rotação de forma circular.
                // ----------------------------------------------------

                if (grupoServico == null)
                {
                    grupoServico = gruposUnidade
                        .Select(g => new
                        {
                            Grupo = g,

                            DiferencaDias =
                                (dataServico -
                                 g.DataReferencia.Date).Days
                        })
                        .Select(x => new
                        {
                            x.Grupo,

                            Posicao =
                                ((x.DiferencaDias % 3) + 3) % 3
                        })
                        .Where(x =>
                            x.Posicao ==
                            x.Grupo.OrdemRotacao - 1)
                        .Select(x => x.Grupo)
                        .FirstOrDefault();
                }

                if (grupoServico == null)
                {
                    continue;
                }

                // ----------------------------------------------------
                // Funcionários actualmente lotados na equipa
                // ----------------------------------------------------

                var funcionarios = await _context.LotacoesFuncionarios
                    .AsNoTracking()

                    .Include(l => l.Funcionario)
                    .Include(l => l.FuncaoOperacional)
                    .Include(l => l.Posto)

                    .Where(l =>
                        l.Ativo &&
                        l.UnidadeOperacionalId ==
                            grupoServico.UnidadeOperacionalId &&
                        l.EquipaId ==
                            grupoServico.EquipaId)

                    .OrderBy(l => l.Funcionario!.NomeCompleto)

                    .Select(l => new FuncionarioServicoDto
                    {
                        Id = l.FuncionarioId,

                        NomeCompleto =
                            l.Funcionario!.NomeCompleto,

                        Nip =
                            l.Funcionario.Nip,

                        Funcao =
                            l.FuncaoOperacional != null
                                ? l.FuncaoOperacional.Nome
                                : null,

                        PostoId =
                            l.PostoId,

                        Posto =
                            l.Posto != null
                                ? l.Posto.Nome
                                : null
                    })

                    .ToListAsync();

                // ----------------------------------------------------
                // Adicionar serviço ao resultado
                // ----------------------------------------------------

                resultado.Add(new ServicoGrupoDto
                {
                    GrupoId =
                        grupoServico.Id,

                    Grupo =
                        grupoServico.Nome,

                    UnidadeOperacionalId =
                        grupoServico.UnidadeOperacionalId,

                    UnidadeOperacional =
                        grupoServico.UnidadeOperacional?.Nome,

                    EquipaId =
                        grupoServico.EquipaId,

                    Equipa =
                        grupoServico.Equipa?.Nome,

                    TipoTurnoId =
                        grupoServico.TipoTurnoId,

                    TipoTurno =
                        grupoServico.TipoTurno?.Nome,

                    HorasTrabalho =
                        grupoServico.TipoTurno?.HorasTrabalho ?? 0,

                    HorasDescanso =
                        grupoServico.TipoTurno?.HorasDescanso ?? 0,

                    Data =
                        dataServico,

                    DataReferencia =
                        grupoServico.DataReferencia,

                    Funcionarios =
                        funcionarios
                });
            }

            return Ok(resultado);
        }

        // ============================================================
        // GET:
        // api/GrupoEscala/servico
        //
        // Serviço de HOJE
        // ============================================================

        [HttpGet("servico")]
        public async Task<ActionResult<IEnumerable<ServicoGrupoDto>>>
            GetServicoHoje()
        {
            return await GetServico(DateTime.Today);
        }
    }
}