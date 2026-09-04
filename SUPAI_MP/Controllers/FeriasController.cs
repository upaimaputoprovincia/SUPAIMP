using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using supai_mp.Data;
using supai_mp.Models;
using supai_mp.Models.DTOs;
using System.Globalization;
using System.Security.Claims;

namespace supai_mp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeriasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FeriasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // POST: api/Ferias/minhas
        // Funcionário solicita férias
        // =========================================================
        [Authorize(Roles = "Funcionario")]
        [HttpPost("minhas")]
        public async Task<IActionResult> SolicitarFerias(CriarFeriasDto dto)
        {
            var usuarioId = ObterUsuarioId();

            if (usuarioId == null)
                return Unauthorized("Utilizador não identificado.");

            var usuario = await _context.Usuarios
                .Include(u => u.Funcionario)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario == null)
                return NotFound("Utilizador não encontrado.");

            if (!usuario.Ativo)
                return Unauthorized("Este utilizador está inativo.");

            if (usuario.Funcionario == null)
                return NotFound("Este utilizador não está associado a um funcionário.");

            var funcionarioId = usuario.Funcionario.Id;

            var existe = await _context.Ferias.AnyAsync(f =>
                f.FuncionarioId == funcionarioId &&
                f.Ano == dto.Ano &&
                f.Mes == dto.Mes &&
                f.Estado != EstadoFerias.Cancelada);

            if (existe)
            {
                return BadRequest(new
                {
                    mensagem = "Já existe um registo de férias para este funcionário neste ano e mês."
                });
            }

            var ferias = new Ferias
            {
                FuncionarioId = funcionarioId,
                Ano = dto.Ano,
                Mes = dto.Mes,
                Estado = EstadoFerias.Programada,
                DataMarcacao = DateTime.Now,
                Observacao = dto.Observacao
            };

            _context.Ferias.Add(ferias);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                mensagem = "Férias solicitadas com sucesso.",
                feriasId = ferias.Id,
                ano = ferias.Ano,
                mes = ferias.Mes,
                estado = ferias.Estado.ToString()
            });
        }

       

        // =========================================================
        // GET: api/Ferias/minhas
        // Funcionário consulta as suas férias
        // =========================================================
        [Authorize(Roles = "Funcionario")]
        [HttpGet("minhas")]
        public async Task<IActionResult> MinhasFerias()
        {
            var usuarioId = ObterUsuarioId();

            if (usuarioId == null)
                return Unauthorized("Utilizador não identificado.");

            var usuario = await _context.Usuarios
                .Include(u => u.Funcionario)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario == null)
                return NotFound("Utilizador não encontrado.");

            if (!usuario.Ativo)
                return Unauthorized("Este utilizador está inativo.");

            if (usuario.Funcionario == null)
                return NotFound(
                    "Este utilizador não está associado a um funcionário.");

            var funcionarioId = usuario.Funcionario.Id;

            // Buscar os registos sem fazer cálculos de DateTime dentro do SQL
            var ferias = await _context.Ferias
                .Where(f => f.FuncionarioId == funcionarioId)
                .OrderByDescending(f => f.Ano)
                .ThenByDescending(f => f.Mes)
                .ToListAsync();

            // Transformar os dados em objetos simples depois de sair da BD
            var resultado = ferias.Select(f => new
            {
                id = f.Id,
                funcionarioId = f.FuncionarioId,
                nomeCompleto = usuario.Funcionario.NomeCompleto,
                nip = usuario.Funcionario.Nip,

                ano = f.Ano,
                mes = f.Mes,

                mesNome = System.Globalization.CultureInfo.CurrentCulture
                    .DateTimeFormat
                    .GetMonthName(f.Mes),

                dataInicio = f.DataInicio,
                dataFim = f.DataFim,

                quantidadeDias =
                    f.DataInicio.HasValue && f.DataFim.HasValue
                        ? (int?)(f.DataFim.Value.Date -
                                  f.DataInicio.Value.Date).Days + 1
                        : null,

                estado = f.Estado.ToString(),

                dataMarcacao = f.DataMarcacao,
                dataRegistoInicio = f.DataRegistoInicio,
                dataRegistoFim = f.DataRegistoFim,

                observacao = f.Observacao
            }).ToList();

            return Ok(resultado);
        }



        // =========================================================
        // GET: api/Ferias
        // Administrador consulta todas as férias
        // =========================================================
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> ListarFerias()
        {
            var ferias = await _context.Ferias
                .Include(f => f.Funcionario)
                .OrderByDescending(f => f.Ano)
                .ThenBy(f => f.Mes)
                .ThenBy(f => f.Funcionario!.NomeCompleto)
                .Select(f => new
                {
                    id = f.Id,
                    funcionarioId = f.FuncionarioId,

                    nomeCompleto = f.Funcionario != null
                        ? f.Funcionario.NomeCompleto
                        : null,

                    nip = f.Funcionario != null
                        ? f.Funcionario.Nip
                        : null,

                    ano = f.Ano,
                    mes = f.Mes,

                    dataInicio = f.DataInicio,
                    dataFim = f.DataFim,

                    estado = f.Estado.ToString(),

                    dataMarcacao = f.DataMarcacao,
                    dataRegistoInicio = f.DataRegistoInicio,
                    dataRegistoFim = f.DataRegistoFim,

                    observacao = f.Observacao
                })
                .ToListAsync();

            return Ok(ferias);
        }

        // =========================================================
        // PUT: api/Ferias/{id}/iniciar
        // Administrador inicia as férias
        // =========================================================
        [Authorize(Roles = "Administrador")]
        [HttpPut("{id}/iniciar")]
        public async Task<IActionResult> IniciarFerias(
            int id,
            [FromBody] IniciarFeriasDto dto)
        {
            var ferias = await _context.Ferias
                .Include(f => f.Funcionario)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (ferias == null)
                return NotFound("Registo de férias não encontrado.");

            if (ferias.Estado == EstadoFerias.Cancelada)
                return BadRequest("Não é possível iniciar férias canceladas.");

            if (ferias.Estado == EstadoFerias.EmGozo)
                return BadRequest("Estas férias já estão em gozo.");

            if (ferias.Estado == EstadoFerias.Gozada)
                return BadRequest("Estas férias já foram finalizadas.");

            if (dto.DataInicio == default)
                return BadRequest("Informe a data de início das férias.");

            ferias.DataInicio = dto.DataInicio;
            ferias.DataRegistoInicio = DateTime.Now;
            ferias.Estado = EstadoFerias.EmGozo;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Início das férias registado com sucesso.",
                feriasId = ferias.Id,
                funcionario = ferias.Funcionario?.NomeCompleto,
                dataInicio = ferias.DataInicio,
                estado = ferias.Estado.ToString()
            });
        }

        // =========================================================
        // PUT: api/Ferias/{id}/finalizar
        // Administrador finaliza as férias
        // =========================================================
        [Authorize(Roles = "Administrador")]
        [HttpPut("{id}/finalizar")]
        public async Task<IActionResult> FinalizarFerias(
            int id,
            [FromBody] FinalizarFeriasDto dto)
        {
            var ferias = await _context.Ferias
                .Include(f => f.Funcionario)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (ferias == null)
                return NotFound("Registo de férias não encontrado.");

            if (ferias.Estado == EstadoFerias.Programada)
                return BadRequest("Estas férias ainda não foram iniciadas.");

            if (ferias.Estado == EstadoFerias.Gozada)
                return BadRequest("Estas férias já foram finalizadas.");

            if (ferias.Estado == EstadoFerias.Cancelada)
                return BadRequest("Não é possível finalizar férias canceladas.");

            if (dto.DataFim == default)
                return BadRequest("Informe a data de fim das férias.");

            if (ferias.DataInicio.HasValue &&
                dto.DataFim < ferias.DataInicio.Value)
            {
                return BadRequest(
                    "A data de fim não pode ser anterior à data de início.");
            }

            ferias.DataFim = dto.DataFim;
            ferias.DataRegistoFim = DateTime.Now;
            ferias.Estado = EstadoFerias.Gozada;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Fim das férias registado com sucesso.",
                feriasId = ferias.Id,
                funcionario = ferias.Funcionario?.NomeCompleto,
                dataInicio = ferias.DataInicio,
                dataFim = ferias.DataFim,
                estado = ferias.Estado.ToString()
            });
        }

        // =========================================================
        // Método auxiliar
        // Obtém o ID do utilizador autenticado
        // =========================================================
        private int? ObterUsuarioId()
        {
            var valor = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return int.TryParse(valor, out var id)
                ? id
                : null;
        }
    }
}

