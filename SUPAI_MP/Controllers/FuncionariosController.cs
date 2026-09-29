using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using supai_mp.Data;
using supai_mp.DTOs;
using supai_mp.Models;
using supai_mp.Models.DTOs;
using SUPAI_MP.Data.DTOs;
using System.Security.Claims;

namespace supai_mp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FuncionariosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FuncionariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // ORDENAÇÃO GLOBAL
        // ============================================================

        private static int OrdemGrupoChefia(Funcionario f)
        {
            if (string.IsNullOrWhiteSpace(f.Funcao))
                return 1;

            var funcao = f.Funcao.Trim().ToUpperInvariant();

            return funcao switch
            {
                "COMANDANTE DA SUBUNIDADE" => 0,
                "CHEFE DAS OPERACOES" => 0,
                "CHEFE DA DOUTRINA E ETICA" => 0,
                "CHEFE DA PEAC" => 0,
                "CHEFE DE SEGURANCA PESSOAL" => 0,
                "CHEFE DE PROTECCAO DE OBJECTOS" => 0,
                "CHEFE DA LOGISTICA E FINANCAS" => 0,
                "CHEFE DE GESTAO DE PESSOAL E FORMACAO" => 0,
                "CHEFE DA SECRETARIA" => 0,
                "CHEFE DE INFORMACAO OPERATIVA" => 0,
                "CHEFE DE INFORMACAO INTERNA" => 0,
                _ => 1
            };
        }

        private static int OrdemChefia(Funcionario f)
        {
            if (string.IsNullOrWhiteSpace(f.Funcao))
                return 999;

            var funcao = f.Funcao.Trim().ToUpperInvariant();

            return funcao switch
            {
                "COMANDANTE DA SUBUNIDADE" => 1,
                "CHEFE DAS OPERACOES" => 2,
                "CHEFE DA DOUTRINA E ETICA" => 3,
                "CHEFE DA PEAC" => 4,
                "CHEFE DE SEGURANCA PESSOAL" => 5,
                "CHEFE DE PROTECCAO DE OBJECTOS" => 6,
                "CHEFE DA LOGISTICA E FINANCAS" => 7,
                "CHEFE DE GESTAO DE PESSOAL E FORMACAO" => 8,
                "CHEFE DA SECRETARIA" => 9,
                "CHEFE DE INFORMACAO OPERATIVA" => 10,
                "CHEFE DE INFORMACAO INTERNA" => 11,
                _ => 999
            };
        }

        private static int OrdemCategoria(Categoria categoria)
        {
            return categoria switch
            {
                Categoria.IPG => 1,
                Categoria.COM => 2,
                Categoria.PAC => 3,
                Categoria.AJC => 4,
                Categoria.SPP => 5,
                Categoria.SUP => 6,
                Categoria.ASP => 7,
                Categoria.INP => 8,
                Categoria.INS => 9,
                Categoria.SUB => 10,
                Categoria.SAP => 11,
                Categoria.SAR => 12,
                Categoria.PC => 13,
                Categoria.SC => 14,
                Categoria.GUA => 15,
                _ => 999
            };
        }

        private static int ObterOrdemChefiaMemoria(Funcionario f)
        {
            return OrdemChefia(f);
        }

        private static int ObterOrdemCategoriaMemoria(Categoria categoria)
        {
            return OrdemCategoria(categoria);
        }

        private static List<Funcionario> OrdenarFuncionariosEmMemoria(
            IEnumerable<Funcionario> funcionarios)
        {
            return funcionarios
                .OrderBy(f => OrdemGrupoChefia(f))
                .ThenBy(f => OrdemChefia(f))
                .ThenBy(f => OrdemCategoria(f.Categoria))
                .ThenBy(
                    f => f.NomeCompleto,
                    StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }

        // ============================================================
        // GET: api/Funcionarios
        // ============================================================

        [Authorize(
            Roles =
                PerfisUsuario.Administrador + "," +
                PerfisUsuario.Gestor + "," +
                PerfisUsuario.Supervisor + "," +
                PerfisUsuario.Operador + "," +
                PerfisUsuario.Consultor)]
        [HttpGet]
        public async Task<IActionResult> GetFuncionarios()
        {
            var query = _context.Funcionarios
                .Where(f =>
                    f.Estado == EstadoFuncionario.ACTIVO)
                .AsQueryable();

            var funcionarios =
                await query.ToListAsync();

            // A ordenação contém regras C# próprias do SUPAI-MP.
            // Ela é aplicada somente depois de materializar os dados,
            // evitando que o Entity Framework tente traduzir os métodos
            // de ordenação personalizados para SQL.

            funcionarios =
                OrdenarFuncionariosEmMemoria(funcionarios);

            return Ok(funcionarios);
        }

        // ============================================================
        // GET: api/Funcionarios/meu-perfil
        // ============================================================

        [Authorize]
        [HttpGet("meu-perfil")]
        public async Task<IActionResult> MeuPerfil()
        {
            var claimId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(claimId, out int usuarioId))
            {
                return Unauthorized(new
                {
                    mensagem =
                        "Não foi possível identificar o usuário autenticado."
                });
            }

            var usuario =
                await _context.Usuarios
                    .Include(u => u.Funcionario)
                    .FirstOrDefaultAsync(
                        u => u.Id == usuarioId);

            if (usuario == null)
            {
                return Unauthorized(new
                {
                    mensagem = "Usuário não encontrado."
                });
            }

            if (!usuario.Ativo)
            {
                return Unauthorized(new
                {
                    mensagem = "O usuário está inativo."
                });
            }

            if (usuario.Funcionario == null)
            {
                return NotFound(new
                {
                    mensagem =
                        "O usuário não possui funcionário associado."
                });
            }

            return Ok(usuario.Funcionario);
        }

        // ============================================================
        // PUT: api/Funcionarios/meu-perfil
        // ============================================================

        [Authorize]
        [HttpPut("meu-perfil")]
        public async Task<IActionResult> AtualizarMeuPerfil(
            [FromBody] FuncionarioUpdateDto dto)
        {
            var claimId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(claimId, out int usuarioId))
            {
                return Unauthorized(new
                {
                    mensagem =
                        "Não foi possível identificar o usuário autenticado."
                });
            }

            var usuario =
                await _context.Usuarios
                    .Include(u => u.Funcionario)
                    .FirstOrDefaultAsync(
                        u => u.Id == usuarioId);

            if (usuario == null || !usuario.Ativo)
            {
                return Unauthorized(new
                {
                    mensagem =
                        "Usuário inválido ou inativo."
                });
            }

            if (usuario.Funcionario == null)
            {
                return NotFound(new
                {
                    mensagem =
                        "O usuário não possui funcionário associado."
                });
            }

            var funcionario = usuario.Funcionario;

            // O funcionário comum só altera os seus dados
            // de contacto/endereço através deste endpoint.
            funcionario.Contacto = dto.Contacto;
            funcionario.C_Alternativo = dto.C_Alternativo;
            funcionario.C_Familiar = dto.C_Familiar;
            funcionario.Bairro = dto.Bairro;
            funcionario.Quarterao_N = dto.Quarterao_N;
            funcionario.Casa_N = dto.Casa_N;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem =
                    "Perfil atualizado com sucesso.",
                funcionario
            });
        }

        // ============================================================
        // GET: api/Funcionarios/pesquisar
        // ============================================================

        [Authorize(
            Roles =
                PerfisUsuario.Administrador + "," +
                PerfisUsuario.Gestor + "," +
                PerfisUsuario.Supervisor + "," +
                PerfisUsuario.Consultor)]
        [HttpGet("pesquisar")]
        public async Task<IActionResult> Pesquisar(
            [FromQuery] string termo)
        {
            if (string.IsNullOrWhiteSpace(termo))
            {
                return BadRequest(new
                {
                    mensagem =
                        "Informe um termo para pesquisa."
                });
            }

            termo = termo.Trim();

            var query = _context.Funcionarios
                .Where(f =>
                    f.Estado == EstadoFuncionario.ACTIVO &&
                    (
                        f.NomeCompleto.Contains(termo) ||
                        f.Nip.Contains(termo) ||
                        f.Bi.Contains(termo) ||
                        f.Nuit.Contains(termo) ||
                        f.Contacto.Contains(termo) ||
                        f.LocalTrabalho.Contains(termo)
                    ))
                .AsQueryable();

            var funcionarios =
                await query.ToListAsync();

            funcionarios =
                OrdenarFuncionariosEmMemoria(funcionarios);

            return Ok(funcionarios);
        }

        // ============================================================
        // POST: api/Funcionarios/minha-fotografia
        // ============================================================

        [Authorize]
        [HttpPost("minha-fotografia")]
        public async Task<IActionResult> AtualizarMinhaFotografia(
            IFormFile ficheiro)
        {
            if (ficheiro == null ||
                ficheiro.Length == 0)
            {
                return BadRequest(new
                {
                    mensagem =
                        "Nenhuma fotografia foi enviada."
                });
            }

            const long tamanhoMaximo =
                5 * 1024 * 1024;

            if (ficheiro.Length > tamanhoMaximo)
            {
                return BadRequest(new
                {
                    mensagem =
                        "A fotografia não pode exceder 5 MB."
                });
            }

            var extensao =
                Path.GetExtension(
                    ficheiro.FileName)
                    .ToLowerInvariant();

            var extensoesPermitidas =
                new[]
                {
                    ".jpg",
                    ".jpeg",
                    ".png"
                };

            if (!extensoesPermitidas.Contains(extensao))
            {
                return BadRequest(new
                {
                    mensagem =
                        "Formato de fotografia não permitido. " +
                        "Use JPG, JPEG ou PNG."
                });
            }

            var claimId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(claimId, out int usuarioId))
            {
                return Unauthorized();
            }

            var usuario =
                await _context.Usuarios
                    .Include(u => u.Funcionario)
                    .FirstOrDefaultAsync(
                        u => u.Id == usuarioId);

            if (usuario == null || !usuario.Ativo)
            {
                return Unauthorized();
            }

            if (usuario.Funcionario == null)
            {
                return NotFound(new
                {
                    mensagem =
                        "O usuário não possui funcionário associado."
                });
            }

            var funcionario =
                usuario.Funcionario;

            var caminhoFotos =
                Environment.GetEnvironmentVariable(
                    "FOTOS_PATH");

            if (string.IsNullOrWhiteSpace(caminhoFotos))
            {
                caminhoFotos =
                    Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "fotos");
            }

            Directory.CreateDirectory(
                caminhoFotos);

            if (!string.IsNullOrWhiteSpace(
                funcionario.FotografiaUrl))
            {
                var nomeAntigo =
                    Path.GetFileName(
                        funcionario.FotografiaUrl);

                if (!string.IsNullOrWhiteSpace(nomeAntigo))
                {
                    var caminhoAntigo =
                        Path.Combine(
                            caminhoFotos,
                            nomeAntigo);

                    if (System.IO.File.Exists(
                        caminhoAntigo))
                    {
                        System.IO.File.Delete(
                            caminhoAntigo);
                    }
                }
            }

            var novoNome =
                $"{Guid.NewGuid()}{extensao}";

            var caminhoNovo =
                Path.Combine(
                    caminhoFotos,
                    novoNome);

            await using (var stream =
                new FileStream(
                    caminhoNovo,
                    FileMode.Create))
            {
                await ficheiro.CopyToAsync(stream);
            }

            funcionario.FotografiaUrl =
                $"/fotos/{novoNome}";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Fotografia atualizada com sucesso.",
                fotografiaUrl = funcionario.FotografiaUrl,
                caminhoFisico = caminhoNovo,
                ficheiroExiste = System.IO.File.Exists(caminhoNovo),
                tamanho = ficheiro.Length
            });
        }

        // ============================================================
        // PUT: api/Funcionarios/alterar-senha
        // ============================================================

        [Authorize]
        [HttpPut("alterar-senha")]
        public async Task<IActionResult> AlterarSenha(
            [FromBody] AlterarSenhaDto dto)
        {
            var claimId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(claimId, out int usuarioId))
            {
                return Unauthorized();
            }

            var usuario =
                await _context.Usuarios
                    .FirstOrDefaultAsync(
                        u => u.Id == usuarioId);

            if (usuario == null || !usuario.Ativo)
            {
                return Unauthorized();
            }

            if (!BCrypt.Net.BCrypt.Verify(
                dto.SenhaAtual,
                usuario.SenhaHash))
            {
                return BadRequest(new
                {
                    mensagem =
                        "A senha atual está incorreta."
                });
            }

            usuario.SenhaHash =
                BCrypt.Net.BCrypt.HashPassword(
                    dto.NovaSenha);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem =
                    "Senha alterada com sucesso."
            });
        }

        // ============================================================
        // GET: api/Funcionarios/pesquisa-avancada
        // ============================================================

        [Authorize(
            Roles =
                PerfisUsuario.Administrador + "," +
                PerfisUsuario.Gestor + "," +
                PerfisUsuario.Supervisor + "," +
                PerfisUsuario.Consultor)]
        [HttpGet("pesquisa-avancada")]
        public async Task<IActionResult> PesquisaAvancada(
            [FromQuery] FuncionarioPesquisaDto filtro)
        {
            var query =
                _context.Funcionarios
                    .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro.Nome))
            {
                query = query.Where(f =>
                    f.NomeCompleto.Contains(
                        filtro.Nome));
            }

            if (!string.IsNullOrWhiteSpace(filtro.Nip))
            {
                query = query.Where(f =>
                    f.Nip.Contains(
                        filtro.Nip));
            }

            if (!string.IsNullOrWhiteSpace(filtro.Bi))
            {
                query = query.Where(f =>
                    f.Bi.Contains(
                        filtro.Bi));
            }

            if (!string.IsNullOrWhiteSpace(filtro.Nuit))
            {
                query = query.Where(f =>
                    f.Nuit.Contains(
                        filtro.Nuit));
            }

            if (!string.IsNullOrWhiteSpace(filtro.Contacto))
            {
                query = query.Where(f =>
                    f.Contacto.Contains(
                        filtro.Contacto));
            }

            if (filtro.Categoria.HasValue)
            {
                query = query.Where(f =>
                    f.Categoria ==
                    filtro.Categoria.Value);
            }

            if (!string.IsNullOrWhiteSpace(filtro.Funcao))
            {
                query = query.Where(f =>
                    f.Funcao.Contains(
                        filtro.Funcao));
            }

            if (!string.IsNullOrWhiteSpace(filtro.LocalTrabalho))
            {
                query = query.Where(f =>
                    f.LocalTrabalho.Contains(
                        filtro.LocalTrabalho));
            }

            if (filtro.EstadoFuncionario.HasValue)
            {
                query = query.Where(f =>
                    f.Estado ==
                    filtro.EstadoFuncionario.Value);
            }

            var funcionarios =
                await query.ToListAsync();

            funcionarios =
                OrdenarFuncionariosEmMemoria(funcionarios);

            return Ok(funcionarios);
        }

        // ============================================================
        // GET: api/Funcionarios/acessos/{id}
        // ============================================================

        [Authorize(Roles = PerfisUsuario.Administrador)]
        [HttpGet("acessos/{id}")]
        public async Task<IActionResult> ObterAcesso(int id)
        {
            var usuario =
                await _context.Usuarios
                    .Include(u => u.Funcionario)
                    .FirstOrDefaultAsync(
                        u => u.Id == id);

            if (usuario == null)
            {
                return NotFound(new
                {
                    mensagem =
                        "Usuário não encontrado."
                });
            }

            return Ok(new
            {
                usuario.Id,

                FuncionarioId =
                    usuario.FuncionarioId,

                NomeCompleto =
                    usuario.Funcionario?.NomeCompleto,

                Nip =
                    usuario.Funcionario?.Nip,

                usuario.NomeUsuario,

                usuario.Perfil,

                usuario.Ativo
            });
        }

        // ============================================================
        // PUT: api/Funcionarios/acessos/{id}
        // ============================================================

        [Authorize(Roles = PerfisUsuario.Administrador)]
        [HttpPut("acessos/{id}")]
        public async Task<IActionResult> AtualizarAcesso(
    int id,
    [FromBody] EditarUsuarioDto dto)
        {
            // ============================================================
            // VALIDAR DADOS RECEBIDOS
            // ============================================================

            if (dto == null)
            {
                return BadRequest(new
                {
                    mensagem = "Os dados do usuário não foram enviados."
                });
            }

            // ============================================================
            // LOCALIZAR USUÁRIO
            // ============================================================

            var usuario =
                await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
            {
                return NotFound(new
                {
                    mensagem = "Usuário não encontrado."
                });
            }

            // ============================================================
            // VALIDAR PERFIL
            // ============================================================

            if (!PerfisUsuario.EhValido(dto.Perfil))
            {
                return BadRequest(new
                {
                    mensagem = "Perfil de usuário inválido.",
                    perfisPermitidos = PerfisUsuario.Todos
                });
            }

            // ============================================================
            // VALIDAR NOME DE USUÁRIO
            // ============================================================

            var nomeUsuario = dto.NomeUsuario?.Trim();

            if (string.IsNullOrWhiteSpace(nomeUsuario))
            {
                return BadRequest(new
                {
                    mensagem = "O nome de usuário é obrigatório."
                });
            }

            // ============================================================
            // VERIFICAR NOME DE USUÁRIO DUPLICADO
            // ============================================================

            var utilizadorExistente =
                await _context.Usuarios
                    .FirstOrDefaultAsync(u =>
                        u.Id != id &&
                        u.NomeUsuario == nomeUsuario);

            if (utilizadorExistente != null)
            {
                return Conflict(new
                {
                    mensagem =
                        "Já existe outro usuário com este nome de usuário."
                });
            }

            // ============================================================
            // VALIDAR FUNCIONÁRIO
            // ============================================================

            if (dto.FuncionarioId > 0)
            {
                var funcionarioSelecionado =
                    await _context.Funcionarios
                        .FirstOrDefaultAsync(f =>
                            f.Id == dto.FuncionarioId);

                if (funcionarioSelecionado == null)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "O funcionário selecionado não existe."
                    });
                }

                // Um funcionário só pode possuir um usuário.
                var outroUsuario =
                    await _context.Usuarios
                        .FirstOrDefaultAsync(u =>
                            u.Id != id &&
                            u.FuncionarioId == dto.FuncionarioId);

                if (outroUsuario != null)
                {
                    return Conflict(new
                    {
                        mensagem =
                            "O funcionário selecionado já possui um usuário associado."
                    });
                }
            }

            // ============================================================
            // IMPEDIR REMOÇÃO DO ÚLTIMO ADMINISTRADOR ATIVO
            // ============================================================

            if (usuario.Perfil == PerfisUsuario.Administrador &&
                !string.Equals(
                    dto.Perfil,
                    PerfisUsuario.Administrador,
                    StringComparison.OrdinalIgnoreCase))
            {
                var administradoresAtivos =
                    await _context.Usuarios
                        .CountAsync(u =>
                            u.Ativo &&
                            u.Perfil == PerfisUsuario.Administrador);

                if (administradoresAtivos <= 1)
                {
                    return Conflict(new
                    {
                        mensagem =
                            "Não é possível remover o perfil do último Administrador ativo."
                    });
                }
            }

            // ============================================================
            // ATUALIZAR USUÁRIO
            // ============================================================

            usuario.NomeUsuario = nomeUsuario;

            usuario.Perfil = dto.Perfil;

            usuario.FuncionarioId = dto.FuncionarioId;

            // ============================================================
            // GUARDAR ALTERAÇÕES
            // ============================================================

            await _context.SaveChangesAsync();

            // ============================================================
            // RESPOSTA
            // ============================================================

            return Ok(new
            {
                mensagem = "Acesso atualizado com sucesso.",
                usuario.Id,
                usuario.NomeUsuario,
                usuario.Perfil,
                usuario.FuncionarioId
            });
        }

        // ============================================================
        // GET: api/Funcionarios/paginado
        // ============================================================

        [Authorize(
    Roles =
        PerfisUsuario.Administrador + "," +
        PerfisUsuario.Gestor + "," +
        PerfisUsuario.Supervisor + "," +
        PerfisUsuario.Consultor)]
        [HttpGet("paginado")]
        public async Task<IActionResult> GetFuncionariosPaginado(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? nome = null,
            [FromQuery] int? pagina = null,
            [FromQuery] int? tamanhoPagina = null)
        {
            // Compatibilidade com o MVC:
            // /api/Funcionarios/paginado?pagina=1&tamanhoPagina=20
            if (pagina.HasValue)
                page = pagina.Value;

            if (tamanhoPagina.HasValue)
                pageSize = tamanhoPagina.Value;

            if (page < 1)
                page = 1;

            if (pageSize < 1)
                pageSize = 20;

            if (pageSize > 100)
                pageSize = 100;

            nome = nome?.Trim();

            // ============================================================
            // CARREGAR OS FUNCIONÁRIOS
            // ============================================================
            //
            // Esta versão evita GroupBy/Count múltiplos e projeções
            // complexas diretamente no MySQL. A versão anterior podia
            // provocar HTTP 500 durante a gestão de funcionários,
            // dependendo da tradução EF/Pomelo da consulta.
            //
            // ============================================================

            var query = _context.Funcionarios
                .AsNoTracking()
                .Include(f => f.Seccao)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(nome))
            {
                query = query.Where(f =>
                    f.NomeCompleto != null &&
                    f.NomeCompleto.Contains(nome));
            }

            var todosFuncionarios =
                await query.ToListAsync();

            // ============================================================
            // ORDENAR EM MEMÓRIA
            // ============================================================

            todosFuncionarios =
                OrdenarFuncionariosEmMemoria(todosFuncionarios);

            // ============================================================
            // ESTATÍSTICAS EM MEMÓRIA
            // ============================================================

            var total = todosFuncionarios.Count;

            var totalMasculino =
                todosFuncionarios.Count(f =>
                    f.G == Genero.M);

            var totalFeminino =
                todosFuncionarios.Count(f =>
                    f.G == Genero.F);

            var totalAtivos =
                todosFuncionarios.Count(f =>
                    f.Estado == EstadoFuncionario.ACTIVO);

            var totalPorCategoria =
                todosFuncionarios
                    .GroupBy(f => f.Categoria)
                    .Select(g => new
                    {
                        Categoria = g.Key,
                        Quantidade = g.Count()
                    })
                    .OrderBy(x => x.Categoria)
                    .ToList();

            var totalPaginas =
                total == 0
                    ? 0
                    : (int)Math.Ceiling(
                        total / (double)pageSize);

            // Se a página solicitada ultrapassar o total,
            // regressar à última página válida.
            if (totalPaginas > 0 && page > totalPaginas)
                page = totalPaginas;

            if (totalPaginas == 0)
                page = 1;

            // ============================================================
            // PAGINAÇÃO EM MEMÓRIA
            // ============================================================

            var funcionariosPagina =
                todosFuncionarios
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

            // ============================================================
            // VERIFICAR QUAIS POSSUEM USUÁRIO
            // ============================================================

            var idsPagina =
                funcionariosPagina
                    .Select(f => f.Id)
                    .ToList();

            var idsComUsuario =
                idsPagina.Count == 0
                    ? new HashSet<int>()
                    : (await _context.Usuarios
                        .AsNoTracking()
                        .Where(u =>
                            u.FuncionarioId.HasValue &&
                            idsPagina.Contains(
                                u.FuncionarioId.Value))
                        .Select(u =>
                            u.FuncionarioId!.Value)
                        .ToListAsync())
                        .ToHashSet();

            // ============================================================
            // PROJEÇÃO FINAL
            // ============================================================

            var funcionarios =
                funcionariosPagina
                    .Select(f => new
                    {
                        f.Id,
                        f.NomeCompleto,
                        f.Nip,
                        f.Bi,
                        f.Nuit,

                        G = f.G,

                        EstadoCivil =
                            f.estado_civil,

                        NivelAcademico =
                            f.nivelAcademico,

                        f.Categoria,
                        f.Funcao,
                        f.Contacto,
                        f.C_Alternativo,
                        f.C_Familiar,
                        f.Bairro,
                        f.Quarterao_N,
                        f.Casa_N,
                        f.LocalTrabalho,
                        f.Estado,
                        f.FotografiaUrl,
                        f.SeccaoId,

                        SeccaoNome =
                            f.Seccao != null
                                ? f.Seccao.Nome
                                : null,

                        TemUsuario =
                            idsComUsuario.Contains(f.Id)
                    })
                    .ToList();

            // ============================================================
            // RESPOSTA
            // ============================================================

            return Ok(new
            {
                page,
                pageSize,

                // Nomes usados pela versão atual do MVC.
                total,
                totalPaginas,

                // Nomes antigos mantidos por compatibilidade.
                pagina = page,
                tamanhoPagina = pageSize,
                totalRegistros = total,

                estatisticas = new
                {
                    totalMasculino,
                    totalFeminino,
                    totalAtivos,
                    totalPorCategoria
                },

                funcionarios
            });
        }

        // ============================================================
        // POST: api/Funcionarios
        // ============================================================

        [Authorize(
            Roles =
                PerfisUsuario.Administrador + "," +
                PerfisUsuario.Gestor)]
        [HttpPost]
        public async Task<IActionResult> CriarFuncionario(
            [FromBody] FuncionarioCreateDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new
                {
                    mensagem =
                        "Os dados do funcionário não foram enviados."
                });
            }

            if (!string.IsNullOrWhiteSpace(dto.NomeUsuario))
            {
                var usuarioExistente =
                    await _context.Usuarios
                        .AnyAsync(
                            u =>
                                u.NomeUsuario ==
                                dto.NomeUsuario.Trim());

                if (usuarioExistente)
                {
                    return Conflict(new
                    {
                        mensagem =
                            "O nome de usuário já existe."
                    });
                }
            }

            var funcionario =
                new Funcionario
                {
                    NomeCompleto =
                        dto.NomeCompleto,

                    Nip =
                        dto.Nip,

                    Bi =
                        dto.Bi,

                    Nuit =
                        dto.Nuit,

                    G =
                        dto.Genero,

                    estado_civil =
                        dto.EstadoCivil,

                    nivelAcademico =
                        dto.NivelAcademico,

                    grauParentesco =
                        dto.GrauParentesco,

                    Contacto =
                        dto.Contacto,

                    C_Alternativo =
                        dto.C_Alternativo,

                    C_Familiar =
                        dto.C_Familiar,

                    DataNascimento =
                        dto.DataNascimento,

                    DataIngresso =
                        dto.DataIngresso,

                    Categoria =
                        dto.Categoria,

                    Funcao =
                        dto.Funcao,

                    LocalTrabalho =
                        dto.LocalTrabalho,

                    Estado =
                        dto.EstadoFuncionario,

                    Bairro =
                        dto.Bairro,

                    Quarterao_N =
                        dto.Quarterao_N,

                    Casa_N =
                        dto.Casa_N,

                    FotografiaUrl =
                        dto.FotografiaUrl,

                    DataCadastro =
                        DateTime.Now
                };

            _context.Funcionarios.Add(
                funcionario);

            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(dto.NomeUsuario) &&
                !string.IsNullOrWhiteSpace(dto.Senha))
            {
                var usuario =
                    new Usuario
                    {
                        NomeUsuario =
                            dto.NomeUsuario.Trim(),

                        SenhaHash =
                            BCrypt.Net.BCrypt.HashPassword(
                                dto.Senha),

                        Perfil =
                            PerfisUsuario.Funcionario,

                        FuncionarioId =
                            funcionario.Id,

                        Ativo = true,

                        DataCadastro =
                            DateTime.UtcNow
                    };

                _context.Usuarios.Add(usuario);

                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(
                nameof(GetFuncionario),
                new
                {
                    id = funcionario.Id
                },
                funcionario);
        }

        // ============================================================
        // GET: api/Funcionarios/{id}
        // ============================================================

        [Authorize(
            Roles =
                PerfisUsuario.Administrador + "," +
                PerfisUsuario.Gestor + "," +
                PerfisUsuario.Supervisor + "," +
                PerfisUsuario.Consultor)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFuncionario(int id)
        {
            var funcionario =
                await _context.Funcionarios
                    .FindAsync(id);

            if (funcionario == null)
            {
                return NotFound(new
                {
                    mensagem =
                        "Funcionário não encontrado."
                });
            }

            return Ok(funcionario);
        }

        // ============================================================
        // PUT: api/Funcionarios/{id}
        // ============================================================

        [Authorize(
            Roles =
                PerfisUsuario.Administrador + "," +
                PerfisUsuario.Gestor)]
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarFuncionario(
            int id,
            [FromBody] FuncionarioUpdateDto dto)
        {
            var funcionario =
                await _context.Funcionarios
                    .FindAsync(id);

            if (funcionario == null)
            {
                return NotFound(new
                {
                    mensagem =
                        "Funcionário não encontrado."
                });
            }

            funcionario.NomeCompleto =
                dto.NomeCompleto;

            funcionario.Nip =
                dto.Nip;

            funcionario.Bi =
                dto.Bi;

            funcionario.Nuit =
                dto.Nuit;

            funcionario.G =
                dto.Genero;

            funcionario.estado_civil =
                dto.EstadoCivil;

            funcionario.nivelAcademico =
                dto.NivelAcademico;

            funcionario.grauParentesco =
                dto.GrauParentesco;

            funcionario.Contacto =
                dto.Contacto;

            funcionario.C_Alternativo =
                dto.C_Alternativo;

            funcionario.C_Familiar =
                dto.C_Familiar;

            funcionario.DataNascimento =
                dto.DataNascimento;

            funcionario.DataIngresso =
                dto.DataIngresso;

            funcionario.Categoria =
                dto.Categoria;

            funcionario.Funcao =
                dto.Funcao;

            funcionario.LocalTrabalho =
                dto.LocalTrabalho;

            funcionario.Estado =
                dto.EstadoFuncionario;

            funcionario.Bairro =
                dto.Bairro;

            funcionario.Quarterao_N =
                dto.Quarterao_N;

            funcionario.Casa_N =
                dto.Casa_N;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem =
                    "Funcionário atualizado com sucesso.",
                funcionario
            });
        }

        // ============================================================
        // POST: api/Funcionarios/{id}/criar-acesso
        // ============================================================

        [Authorize(Roles = PerfisUsuario.Administrador)]
        [HttpPost("{id}/criar-acesso")]
        public async Task<IActionResult> CriarAcesso(
            int id,
            [FromBody] CriarAcessoFuncionarioDto dto)
        {
            var funcionario =
                await _context.Funcionarios
                    .FindAsync(id);

            if (funcionario == null)
            {
                return NotFound(new
                {
                    mensagem =
                        "Funcionário não encontrado."
                });
            }

            var acessoExistente =
                await _context.Usuarios
                    .FirstOrDefaultAsync(
                        u =>
                            u.FuncionarioId == id);

            if (acessoExistente != null)
            {
                return Conflict(new
                {
                    mensagem =
                        "Este funcionário já possui um usuário."
                });
            }

            var nomeUsuario =
                dto.NomeUsuario?.Trim();

            if (string.IsNullOrWhiteSpace(nomeUsuario))
            {
                return BadRequest(new
                {
                    mensagem =
                        "O nome de usuário é obrigatório."
                });
            }

            var usernameExiste =
                await _context.Usuarios
                    .AnyAsync(
                        u =>
                            u.NomeUsuario ==
                            nomeUsuario);

            if (usernameExiste)
            {
                return Conflict(new
                {
                    mensagem =
                        "O nome de usuário já existe."
                });
            }

            var usuario =
                new Usuario
                {
                    NomeUsuario =
                        nomeUsuario,

                    SenhaHash =
                        BCrypt.Net.BCrypt.HashPassword(
                            dto.Senha),

                    Perfil =
                        PerfisUsuario.Funcionario,

                    FuncionarioId =
                        funcionario.Id,

                    Ativo = true,

                    DataCadastro =
                        DateTime.UtcNow
                };

            _context.Usuarios.Add(
                usuario);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem =
                    "Acesso criado com sucesso.",
                usuario.Id,
                usuario.NomeUsuario,
                usuario.Perfil,
                usuario.FuncionarioId
            });
        }

        // ============================================================
        // GET: api/Funcionarios/acessos
        // ============================================================

        [Authorize(
            Roles =
                PerfisUsuario.Administrador + "," +
                PerfisUsuario.Gestor)]
        [HttpGet("acessos")]
        public async Task<IActionResult> ListarAcessos()
        {
            var usuarios =
                await _context.Usuarios
                    .Include(u => u.Funcionario)
                    .ToListAsync();

            var acessos =
                usuarios
                    .OrderBy(u =>
                        u.Funcionario != null &&
                        ObterOrdemChefiaMemoria(
                            u.Funcionario) < 999
                            ? 0
                            : 1)
                    .ThenBy(u =>
                        u.Funcionario != null
                            ? ObterOrdemChefiaMemoria(
                                u.Funcionario)
                            : 999)
                    .ThenBy(u =>
                        u.Funcionario != null
                            ? ObterOrdemCategoriaMemoria(
                                u.Funcionario.Categoria)
                            : 999)
                    .ThenBy(
                        u =>
                            u.Funcionario != null
                                ? u.Funcionario.NomeCompleto
                                : string.Empty,
                        StringComparer.CurrentCultureIgnoreCase)
                    .Select(u => new
                    {
                        u.Id,
                        u.NomeUsuario,
                        u.Perfil,
                        u.Ativo,
                        u.DataCadastro,

                        FuncionarioId =
                            u.FuncionarioId,

                        NomeCompleto =
                            u.Funcionario != null
                                ? u.Funcionario.NomeCompleto
                                : null,

                        Nip =
                            u.Funcionario != null
                                ? u.Funcionario.Nip
                                : null,

                        FotografiaUrl =
                            u.Funcionario != null
                                ? u.Funcionario.FotografiaUrl
                                : null,

                        Categoria =
                            u.Funcionario != null
                                ? u.Funcionario.Categoria
                                : (Categoria?)null,

                        Funcao =
                            u.Funcionario != null
                                ? u.Funcionario.Funcao
                                : null,

                        SeccaoId =
                            u.Funcionario != null
                                ? u.Funcionario.SeccaoId
                                : (int?)null
                    })
                    .ToList();

            return Ok(acessos);
        }

        // ============================================================
        // PUT: api/Funcionarios/{id}/seccao
        // ============================================================

        [Authorize(
            Roles =
                PerfisUsuario.Administrador + "," +
                PerfisUsuario.Gestor)]
        [HttpPut("{id}/seccao")]
        public async Task<IActionResult> AtribuirSeccao(
            int id,
            [FromBody] int seccaoId)
        {
            var funcionario =
                await _context.Funcionarios
                    .FirstOrDefaultAsync(
                        f => f.Id == id);

            if (funcionario == null)
            {
                return NotFound(new
                {
                    mensagem =
                        "Funcionário não encontrado."
                });
            }

            var seccao =
                await _context.Seccoes
                    .FirstOrDefaultAsync(
                        s =>
                            s.Id == seccaoId &&
                            s.Ativo);

            if (seccao == null)
            {
                return BadRequest(new
                {
                    mensagem =
                        "A secção selecionada não existe ou está inativa."
                });
            }

            funcionario.SeccaoId =
                seccaoId;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem =
                    "Secção atribuída com sucesso.",

                funcionarioId =
                    funcionario.Id,

                funcionario =
                    funcionario.NomeCompleto,

                seccaoId =
                    seccao.Id,

                seccao =
                    seccao.Nome
            });
        }

        // ============================================================
        // GET: api/Funcionarios/sem-seccao
        // ============================================================

        [Authorize(
            Roles =
                PerfisUsuario.Administrador + "," +
                PerfisUsuario.Gestor)]
        [HttpGet("sem-seccao")]
        public async Task<IActionResult>
            GetFuncionariosSemSeccao()
        {
            var query =
                _context.Funcionarios
                    .AsNoTracking()
                    .Where(f =>
                        f.SeccaoId == null)
                    .AsQueryable();

            var entidades =
                await query.ToListAsync();

            entidades =
                OrdenarFuncionariosEmMemoria(entidades);

            var idsFuncionarios =
                entidades
                    .Select(f => f.Id)
                    .ToList();

            var idsComUsuario =
                idsFuncionarios.Count == 0
                    ? new HashSet<int>()
                    : (await _context.Usuarios
                        .AsNoTracking()
                        .Where(u =>
                            u.FuncionarioId.HasValue &&
                            idsFuncionarios.Contains(
                                u.FuncionarioId.Value))
                        .Select(u =>
                            u.FuncionarioId!.Value)
                        .ToListAsync())
                        .ToHashSet();

            var funcionarios =
                entidades
                    .Select(f => new
                    {
                        f.Id,
                        f.NomeCompleto,
                        f.Nip,
                        f.Bi,
                        f.Nuit,
                        f.Contacto,
                        f.Funcao,
                        f.Categoria,
                        f.LocalTrabalho,
                        f.Estado,
                        f.FotografiaUrl,

                        TemUsuario =
                            idsComUsuario.Contains(f.Id),

                        f.SeccaoId
                    })
                    .ToList();

            return Ok(funcionarios);
        }

        // ============================================================
        // GET: api/Funcionarios/com-seccao
        // ============================================================

        [Authorize(
            Roles =
                PerfisUsuario.Administrador + "," +
                PerfisUsuario.Gestor)]
        [HttpGet("com-seccao")]
        public async Task<IActionResult>
            GetFuncionariosComSeccao(
                int? seccaoId = null)
        {
            var consulta =
                _context.Funcionarios
                    .AsNoTracking()
                    .Include(f => f.Seccao)
                    .Where(f =>
                        f.SeccaoId != null)
                    .AsQueryable();

            if (seccaoId.HasValue &&
                seccaoId.Value > 0)
            {
                consulta =
                    consulta.Where(
                        f =>
                            f.SeccaoId ==
                            seccaoId.Value);
            }

            var entidades =
                await consulta.ToListAsync();

            entidades =
                OrdenarFuncionariosEmMemoria(entidades);

            var funcionarios =
                entidades
                    .Select(f => new
                    {
                        f.Id,
                        f.NomeCompleto,
                        f.Nip,
                        f.Bi,
                        f.Nuit,
                        f.Contacto,
                        f.Funcao,
                        f.Categoria,
                        f.LocalTrabalho,
                        f.Estado,
                        f.FotografiaUrl,

                        SeccaoId =
                            f.SeccaoId,

                        Seccao =
                            f.Seccao != null
                                ? f.Seccao.Nome
                                : null
                    })
                    .ToList();

            return Ok(funcionarios);
        }

        // ============================================================
        // GET: api/Funcionarios/duplicados
        // ============================================================

        [Authorize(
            Roles =
                PerfisUsuario.Administrador + "," +
                PerfisUsuario.Gestor)]
        [HttpGet("duplicados")]
        public async Task<IActionResult> ObterDuplicados()
        {
            var funcionarios =
                await _context.Funcionarios
                    .Include(f => f.Seccao)
                    .AsNoTracking()
                    .ToListAsync();

            var duplicados =
                funcionarios
                    .Where(f =>
                        !string.IsNullOrWhiteSpace(
                            f.NomeCompleto))
                    .GroupBy(f =>
                        f.NomeCompleto!
                            .Trim()
                            .ToUpper())
                    .Where(g =>
                        g.Count() > 1)
                    .OrderBy(g => g.Key)
                    .Select(g =>
                        new GrupoFuncionarioDuplicadoDto
                        {
                            Nome =
                                g.First()
                                    .NomeCompleto!
                                    .Trim(),

                            Quantidade =
                                g.Count(),

                            Funcionarios =
                                g.OrderBy(f => f.Id)
                                    .Select(f =>
                                        new FuncionarioDuplicadoItemDto
                                        {
                                            Id = f.Id,

                                            NomeCompleto =
                                                f.NomeCompleto,

                                            Nip =
                                                f.Nip,

                                            Categoria =
                                                (int)f.Categoria,

                                            Funcao =
                                                f.Funcao,

                                            SeccaoId =
                                                f.SeccaoId,

                                            SeccaoNome =
                                                f.Seccao != null
                                                    ? f.Seccao.Nome
                                                    : "Sem secção",

                                            Estado =
                                                (int)f.Estado
                                        })
                                    .ToList()
                        })
                    .ToList();

            return Ok(duplicados);
        }

        // ============================================================
        // GET: api/Funcionarios/duplicados/detalhes
        // ============================================================

        [Authorize(
            Roles =
                PerfisUsuario.Administrador + "," +
                PerfisUsuario.Gestor)]
        [HttpGet("duplicados/detalhes")]
        public async Task<IActionResult>
            ObterDuplicadosDetalhes()
        {
            var funcionarios =
                await _context.Funcionarios
                    .Include(f => f.Seccao)
                    .AsNoTracking()
                    .ToListAsync();

            var gruposDuplicados =
                funcionarios
                    .Where(f =>
                        !string.IsNullOrWhiteSpace(
                            f.NomeCompleto))
                    .GroupBy(f =>
                        f.NomeCompleto!
                            .Trim()
                            .ToUpper())
                    .Where(g =>
                        g.Count() > 1)
                    .OrderBy(g => g.Key)
                    .ToList();

            var resultado =
                new List<FuncionarioDuplicadoDetalheDto>();

            var entidadesDependentes =
                _context.Model
                    .GetEntityTypes()
                    .Where(e =>
                        e.GetForeignKeys()
                            .Any(fk =>
                                fk.PrincipalEntityType.ClrType ==
                                typeof(Funcionario)))
                    .ToList();

            foreach (var grupo in gruposDuplicados)
            {
                foreach (var funcionario in
                    grupo.OrderBy(f => f.Id))
                {
                    var detalhe =
                        new FuncionarioDuplicadoDetalheDto
                        {
                            Id =
                                funcionario.Id,

                            NomeCompleto =
                                funcionario.NomeCompleto,

                            Nip =
                                funcionario.Nip,

                            Categoria =
                                (int)funcionario.Categoria,

                            Funcao =
                                funcionario.Funcao,

                            SeccaoId =
                                funcionario.SeccaoId,

                            SeccaoNome =
                                funcionario.Seccao?.Nome ??
                                "Sem secção",

                            Estado =
                                (int)funcionario.Estado
                        };

                    foreach (var entidade
                        in entidadesDependentes)
                    {
                        var foreignKeys =
                            entidade.GetForeignKeys()
                                .Where(fk =>
                                    fk.PrincipalEntityType.ClrType ==
                                    typeof(Funcionario))
                                .ToList();

                        foreach (var fk in foreignKeys)
                        {
                            if (fk.Properties.Count != 1)
                                continue;

                            var propriedadeFk =
                                fk.Properties[0];

                            var tabela =
                                entidade.GetTableName();

                            if (string.IsNullOrWhiteSpace(
                                tabela))
                                continue;

                            var coluna =
                                propriedadeFk.GetColumnName(
                                    StoreObjectIdentifier.Table(
                                        tabela,
                                        entidade.GetSchema()));

                            if (string.IsNullOrWhiteSpace(
                                coluna))
                                continue;

                            var sql = $@"
SELECT COUNT(*)
FROM `{tabela}`
WHERE `{coluna}` = @funcionarioId";

                            await using var command =
                                _context.Database
                                    .GetDbConnection()
                                    .CreateCommand();

                            command.CommandText =
                                sql;

                            var parameter =
                                command.CreateParameter();

                            parameter.ParameterName =
                                "@funcionarioId";

                            parameter.Value =
                                funcionario.Id;

                            command.Parameters.Add(
                                parameter);

                            if (command.Connection!.State !=
                                System.Data.ConnectionState.Open)
                            {
                                await command.Connection
                                    .OpenAsync();
                            }

                            var valor =
                                await command
                                    .ExecuteScalarAsync();

                            var quantidade =
                                Convert.ToInt32(valor);

                            if (quantidade > 0)
                            {
                                detalhe.Dependencias.Add(
                                    new DependenciaFuncionarioDto
                                    {
                                        Entidade =
                                            entidade.ClrType.Name,

                                        Tabela =
                                            tabela,

                                        Quantidade =
                                            quantidade
                                    });
                            }
                        }
                    }

                    detalhe.TotalDependencias =
                        detalhe.Dependencias
                            .Sum(d =>
                                d.Quantidade);

                    resultado.Add(
                        detalhe);
                }
            }

            return Ok(resultado);
        }

        // ============================================================
        // GET: api/Funcionarios/duplicados/usuarios
        // ============================================================

        [Authorize(
            Roles =
                PerfisUsuario.Administrador + "," +
                PerfisUsuario.Gestor)]
        [HttpGet("duplicados/usuarios")]
        public async Task<IActionResult>
            ObterDuplicadosComUsuarios()
        {
            var funcionarios =
                await _context.Funcionarios
                    .Include(f => f.Seccao)
                    .AsNoTracking()
                    .ToListAsync();

            var idsDuplicados =
                funcionarios
                    .Where(f =>
                        !string.IsNullOrWhiteSpace(
                            f.NomeCompleto))
                    .GroupBy(f =>
                        f.NomeCompleto!
                            .Trim()
                            .ToUpper())
                    .Where(g =>
                        g.Count() > 1)
                    .SelectMany(g =>
                        g.Select(f => f.Id))
                    .ToList();

            if (!idsDuplicados.Any())
            {
                return Ok(
                    new List<FuncionarioDuplicadoUsuarioDto>());
            }

            var resultado =
                new List<FuncionarioDuplicadoUsuarioDto>();

            foreach (var funcionario in
                funcionarios
                    .Where(f =>
                        idsDuplicados.Contains(f.Id))
                    .OrderBy(f =>
                        f.NomeCompleto)
                    .ThenBy(f =>
                        f.Id))
            {
                var item =
                    new FuncionarioDuplicadoUsuarioDto
                    {
                        Id =
                            funcionario.Id,

                        NomeCompleto =
                            funcionario.NomeCompleto,

                        Nip =
                            funcionario.Nip,

                        SeccaoId =
                            funcionario.SeccaoId,

                        SeccaoNome =
                            funcionario.Seccao?.Nome ??
                            "Sem secção"
                    };

                var usuarios =
                    await _context.Usuarios
                        .AsNoTracking()
                        .Where(u =>
                            u.FuncionarioId ==
                            funcionario.Id)
                        .Select(u =>
                            new UsuarioFuncionarioDuplicadoDto
                            {
                                UsuarioId =
                                    u.Id,

                                NomeUsuario =
                                    u.NomeUsuario,

                                Perfil =
                                    u.Perfil
                            })
                        .ToListAsync();

                item.Usuarios.AddRange(
                    usuarios);

                resultado.Add(item);
            }

            return Ok(resultado);
        }

        // ============================================================
        // GET: api/Funcionarios/duplicados/analise
        // ============================================================

        [Authorize(
            Roles =
                PerfisUsuario.Administrador + "," +
                PerfisUsuario.Gestor)]
        [HttpGet("duplicados/analise")]
        public async Task<IActionResult>
            AnalisarDuplicados()
        {
            var funcionarios =
                await _context.Funcionarios
                    .Include(f => f.Seccao)
                    .AsNoTracking()
                    .ToListAsync();

            var gruposDuplicados =
                funcionarios
                    .Where(f =>
                        !string.IsNullOrWhiteSpace(
                            f.NomeCompleto))
                    .GroupBy(f =>
                        f.NomeCompleto!
                            .Trim()
                            .ToUpper())
                    .Where(g =>
                        g.Count() > 1)
                    .OrderBy(g => g.Key)
                    .ToList();

            var resultado =
                new List<GrupoDuplicadoAnaliseDto>();

            var entidadesDependentes =
                _context.Model
                    .GetEntityTypes()
                    .Where(e =>
                        e.GetForeignKeys()
                            .Any(fk =>
                                fk.PrincipalEntityType.ClrType ==
                                typeof(Funcionario)))
                    .ToList();

            foreach (var grupo in gruposDuplicados)
            {
                var analiseGrupo =
                    new GrupoDuplicadoAnaliseDto
                    {
                        Nome =
                            grupo.First()
                                .NomeCompleto!
                                .Trim(),

                        Quantidade =
                            grupo.Count()
                    };

                var analisesFuncionarios =
                    new List<FuncionarioDuplicadoAnaliseDto>();

                foreach (var funcionario in grupo)
                {
                    int totalDependencias = 0;

                    foreach (var entidade
                        in entidadesDependentes)
                    {
                        var foreignKeys =
                            entidade.GetForeignKeys()
                                .Where(fk =>
                                    fk.PrincipalEntityType.ClrType ==
                                    typeof(Funcionario))
                                .ToList();

                        foreach (var fk in foreignKeys)
                        {
                            if (fk.Properties.Count != 1)
                                continue;

                            var propriedadeFk =
                                fk.Properties[0];

                            var tabela =
                                entidade.GetTableName();

                            if (string.IsNullOrWhiteSpace(
                                tabela))
                                continue;

                            var coluna =
                                propriedadeFk.GetColumnName(
                                    StoreObjectIdentifier.Table(
                                        tabela,
                                        entidade.GetSchema()));

                            if (string.IsNullOrWhiteSpace(
                                coluna))
                                continue;

                            var sql = $@"
SELECT COUNT(*)
FROM `{tabela}`
WHERE `{coluna}` = @funcionarioId";

                            await using var command =
                                _context.Database
                                    .GetDbConnection()
                                    .CreateCommand();

                            command.CommandText =
                                sql;

                            var parameter =
                                command.CreateParameter();

                            parameter.ParameterName =
                                "@funcionarioId";

                            parameter.Value =
                                funcionario.Id;

                            command.Parameters.Add(
                                parameter);

                            if (command.Connection!.State !=
                                System.Data.ConnectionState.Open)
                            {
                                await command.Connection
                                    .OpenAsync();
                            }

                            var valor =
                                await command
                                    .ExecuteScalarAsync();

                            totalDependencias +=
                                Convert.ToInt32(valor);
                        }
                    }

                    var usuario =
                        await _context.Usuarios
                            .AsNoTracking()
                            .FirstOrDefaultAsync(
                                u =>
                                    u.FuncionarioId ==
                                    funcionario.Id);

                    analisesFuncionarios.Add(
                        new FuncionarioDuplicadoAnaliseDto
                        {
                            Id =
                                funcionario.Id,

                            NomeCompleto =
                                funcionario.NomeCompleto,

                            Nip =
                                funcionario.Nip,

                            SeccaoId =
                                funcionario.SeccaoId,

                            SeccaoNome =
                                funcionario.Seccao?.Nome ??
                                "Sem secção",

                            Categoria =
                                (int)funcionario.Categoria,

                            Funcao =
                                funcionario.Funcao,

                            UsuarioId =
                                usuario?.Id ?? 0,

                            NomeUsuario =
                                usuario?.NomeUsuario,

                            Perfil =
                                usuario?.Perfil,

                            TotalDependencias =
                                totalDependencias,

                            TemSecao =
                                funcionario.SeccaoId.HasValue
                        });
                }

                var maiorDependencias =
                    analisesFuncionarios
                        .Max(f =>
                            f.TotalDependencias);

                var candidatos =
                    analisesFuncionarios
                        .Where(f =>
                            f.TotalDependencias ==
                            maiorDependencias)
                        .ToList();

                var candidato =
                    candidatos.First();

                var comSecao =
                    candidatos
                        .Where(f =>
                            f.TemSecao)
                        .ToList();

                if (comSecao.Count == 1)
                {
                    candidato =
                        comSecao[0];
                }
                else if (comSecao.Count > 1)
                {
                    analiseGrupo.RequerDecisaoManual =
                        true;

                    analiseGrupo.Motivo =
                        "Existem vários registros com dependências " +
                        "e/ou secção atribuída. É necessária decisão manual.";
                }

                if (!analiseGrupo.RequerDecisaoManual)
                {
                    var empateTotal =
                        analisesFuncionarios
                            .Where(f =>
                                f.TotalDependencias ==
                                candidato.TotalDependencias &&
                                f.TemSecao ==
                                candidato.TemSecao)
                            .ToList();

                    if (empateTotal.Count > 1)
                    {
                        candidato =
                            empateTotal
                                .OrderBy(f =>
                                    f.Id)
                                .First();

                        analiseGrupo.Motivo =
                            "Empate entre os registros. " +
                            "Foi selecionado o menor ID como " +
                            "candidato, mas a decisão será " +
                            "confirmada antes da eliminação.";
                    }
                }

                foreach (var funcionario
                    in analisesFuncionarios)
                {
                    funcionario.CandidatoManter =
                        !analiseGrupo.RequerDecisaoManual &&
                        funcionario.Id ==
                        candidato.Id;

                    if (funcionario.Id ==
                        candidato.Id)
                    {
                        if (string.IsNullOrWhiteSpace(
                            analiseGrupo.Motivo))
                        {
                            analiseGrupo.Motivo =
                                "Registro com maior quantidade " +
                                "de dependências e/ou secção atribuída.";
                        }

                        funcionario.Motivo =
                            analiseGrupo.Motivo;
                    }
                    else
                    {
                        funcionario.Motivo =
                            "Candidato a duplicado para eliminação, " +
                            "após transferência segura das relações.";
                    }
                }

                analiseGrupo.IdSugeridoManter =
                    analiseGrupo.RequerDecisaoManual
                        ? null
                        : candidato.Id;

                analiseGrupo.Funcionarios =
                    analisesFuncionarios;

                resultado.Add(
                    analiseGrupo);
            }

            return Ok(resultado);
        }

        // ============================================================
        // POST: api/Funcionarios/duplicados/consolidar
        // ============================================================

        [Authorize(Roles = PerfisUsuario.Administrador)]
        [HttpPost("duplicados/consolidar")]
        public async Task<IActionResult> ConsolidarDuplicado(
            [FromBody] ConsolidarFuncionariosDuplicadosDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new
                {
                    mensagem =
                        "Os dados da consolidação não foram enviados."
                });
            }

            if (dto.IdManter <= 0 ||
                dto.IdEliminar <= 0)
            {
                return BadRequest(new
                {
                    mensagem =
                        "Os IDs dos funcionários devem ser maiores que zero."
                });
            }

            if (dto.IdManter ==
                dto.IdEliminar)
            {
                return BadRequest(new
                {
                    mensagem =
                        "O funcionário a manter e o funcionário a eliminar " +
                        "não podem ser o mesmo."
                });
            }

            var funcionarioManter =
                await _context.Funcionarios
                    .FirstOrDefaultAsync(
                        f =>
                            f.Id ==
                            dto.IdManter);

            var funcionarioEliminar =
                await _context.Funcionarios
                    .FirstOrDefaultAsync(
                        f =>
                            f.Id ==
                            dto.IdEliminar);

            if (funcionarioManter == null)
            {
                return NotFound(new
                {
                    mensagem =
                        $"O funcionário com ID {dto.IdManter} não foi encontrado."
                });
            }

            if (funcionarioEliminar == null)
            {
                return NotFound(new
                {
                    mensagem =
                        $"O funcionário com ID {dto.IdEliminar} não foi encontrado."
                });
            }

            var nomeManter =
                funcionarioManter.NomeCompleto?.Trim();

            var nomeEliminar =
                funcionarioEliminar.NomeCompleto?.Trim();

            if (string.IsNullOrWhiteSpace(nomeManter) ||
                string.IsNullOrWhiteSpace(nomeEliminar))
            {
                return BadRequest(new
                {
                    mensagem =
                        "Um dos funcionários não possui nome válido."
                });
            }

            if (!string.Equals(
                nomeManter,
                nomeEliminar,
                StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    mensagem =
                        "Os dois funcionários não possuem o mesmo nome. " +
                        "A consolidação foi bloqueada."
                });
            }

            var nipManter =
                funcionarioManter.Nip?.Trim();

            var nipEliminar =
                funcionarioEliminar.Nip?.Trim();

            if (!string.IsNullOrWhiteSpace(nipManter) &&
                !string.IsNullOrWhiteSpace(nipEliminar))
            {
                if (!string.Equals(
                    nipManter,
                    nipEliminar,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return Conflict(new
                    {
                        mensagem =
                            "Os funcionários possuem NIP diferentes. " +
                            "A consolidação foi bloqueada para evitar " +
                            "a fusão de pessoas diferentes."
                    });
                }
            }

            var nomeNormalizado =
                nomeManter
                    .Trim()
                    .ToUpper();

            var funcionariosMesmoNome =
                await _context.Funcionarios
                    .Where(f =>
                        f.NomeCompleto != null &&
                        f.NomeCompleto
                            .Trim()
                            .ToUpper() ==
                        nomeNormalizado)
                    .ToListAsync();

            if (funcionariosMesmoNome.Count != 2)
            {
                return Conflict(new
                {
                    mensagem =
                        $"O nome '{nomeManter}' possui " +
                        $"{funcionariosMesmoNome.Count} registros. " +
                        "A consolidação exige exatamente dois registros."
                });
            }

            if (!funcionariosMesmoNome.Any(
                    f => f.Id == dto.IdManter) ||
                !funcionariosMesmoNome.Any(
                    f => f.Id == dto.IdEliminar))
            {
                return Conflict(new
                {
                    mensagem =
                        "Os funcionários enviados não correspondem " +
                        "aos dois registros encontrados para este nome."
                });
            }

            var mesmaCategoria =
                funcionarioManter.Categoria ==
                funcionarioEliminar.Categoria;

            var mesmaFuncao =
                string.Equals(
                    funcionarioManter.Funcao?.Trim(),
                    funcionarioEliminar.Funcao?.Trim(),
                    StringComparison.OrdinalIgnoreCase);

            var mesmaSeccao =
                funcionarioManter.SeccaoId ==
                funcionarioEliminar.SeccaoId;

            if ((!mesmaCategoria ||
                 !mesmaFuncao ||
                 !mesmaSeccao) &&
                !dto.ConfirmacaoManual)
            {
                return Conflict(new
                {
                    mensagem =
                        "Os registros possuem diferenças relevantes " +
                        "de categoria, função ou secção. " +
                        "A consolidação automática foi bloqueada."
                });
            }

            var entidadesDependentes =
                _context.Model
                    .GetEntityTypes()
                    .Where(e =>
                        e.GetForeignKeys()
                            .Any(fk =>
                                fk.PrincipalEntityType.ClrType ==
                                typeof(Funcionario)))
                    .ToList();

            var dependenciasPorFuncionario =
                new Dictionary<int, List<object>>();

            var totalDependenciasPorFuncionario =
                new Dictionary<int, int>();

            foreach (var funcionario
                in funcionariosMesmoNome)
            {
                var dependenciasFuncionario =
                    new List<object>();

                var totalDependencias = 0;

                foreach (var entidade
                    in entidadesDependentes)
                {
                    var foreignKeys =
                        entidade.GetForeignKeys()
                            .Where(fk =>
                                fk.PrincipalEntityType.ClrType ==
                                typeof(Funcionario))
                            .ToList();

                    foreach (var fk in foreignKeys)
                    {
                        if (fk.Properties.Count != 1)
                            continue;

                        var tabela =
                            entidade.GetTableName();

                        if (string.IsNullOrWhiteSpace(
                            tabela))
                            continue;

                        var propriedadeFk =
                            fk.Properties[0];

                        var coluna =
                            propriedadeFk.GetColumnName(
                                StoreObjectIdentifier.Table(
                                    tabela,
                                    entidade.GetSchema()));

                        if (string.IsNullOrWhiteSpace(
                            coluna))
                            continue;

                        var sql = $@"
SELECT COUNT(*)
FROM `{tabela}`
WHERE `{coluna}` = @funcionarioId";

                        await using var command =
                            _context.Database
                                .GetDbConnection()
                                .CreateCommand();

                        command.CommandText =
                            sql;

                        var parameter =
                            command.CreateParameter();

                        parameter.ParameterName =
                            "@funcionarioId";

                        parameter.Value =
                            funcionario.Id;

                        command.Parameters.Add(
                            parameter);

                        if (command.Connection!.State !=
                            System.Data.ConnectionState.Open)
                        {
                            await command.Connection
                                .OpenAsync();
                        }

                        var valor =
                            await command
                                .ExecuteScalarAsync();

                        var quantidade =
                            Convert.ToInt32(valor);

                        if (quantidade > 0)
                        {
                            dependenciasFuncionario.Add(
                                new
                                {
                                    entidade =
                                        entidade.ClrType.Name,

                                    tabela,

                                    coluna,

                                    quantidade
                                });

                            totalDependencias +=
                                quantidade;
                        }
                    }
                }

                dependenciasPorFuncionario[
                    funcionario.Id] =
                    dependenciasFuncionario;

                totalDependenciasPorFuncionario[
                    funcionario.Id] =
                    totalDependencias;
            }

            Funcionario candidatoManter;

            if (dto.ConfirmacaoManual)
            {
                candidatoManter =
                    funcionarioManter;
            }
            else
            {
                candidatoManter =
                    funcionariosMesmoNome
                        .OrderByDescending(x =>
                            totalDependenciasPorFuncionario[
                                x.Id])
                        .ThenByDescending(x =>
                            x.SeccaoId.HasValue)
                        .ThenBy(x =>
                            x.Id)
                        .First();
            }

            if (dto.IdManter !=
                candidatoManter.Id)
            {
                return Conflict(new
                {
                    mensagem =
                        $"O ID {dto.IdManter} não corresponde ao " +
                        "candidato atualmente definido para manutenção.",

                    idCandidatoManter =
                        candidatoManter.Id
                });
            }

            var candidatoEliminar =
                candidatoManter.Id ==
                funcionarioManter.Id
                    ? funcionarioEliminar
                    : funcionarioManter;

            if (dto.IdEliminar !=
                candidatoEliminar.Id)
            {
                return Conflict(new
                {
                    mensagem =
                        $"O ID {dto.IdEliminar} não corresponde ao " +
                        "registro atualmente definido para eliminação."
                });
            }

            var dependenciasEliminar =
                dependenciasPorFuncionario[
                    candidatoEliminar.Id];

            var dependenciasNaoUsuario =
                dependenciasEliminar
                    .Where(d =>
                    {
                        var propriedade =
                            d.GetType()
                                .GetProperty("entidade");

                        var valor =
                            propriedade?
                                .GetValue(d)?
                                .ToString();

                        return !string.Equals(
                            valor,
                            "Usuario",
                            StringComparison.OrdinalIgnoreCase);
                    })
                    .ToList();

            if (dependenciasNaoUsuario.Count > 0)
            {
                return Conflict(new
                {
                    mensagem =
                        "O funcionário a eliminar possui dependências " +
                        "operacionais. A consolidação foi bloqueada.",

                    idFuncionario =
                        candidatoEliminar.Id,

                    dependencias =
                        dependenciasNaoUsuario
                });
            }

            var entidadeUsuario =
                _context.Model
                    .GetEntityTypes()
                    .FirstOrDefault(e =>
                        e.ClrType ==
                        typeof(Usuario));

            if (entidadeUsuario == null)
            {
                return Conflict(new
                {
                    mensagem =
                        "A entidade Usuario não foi encontrada no modelo EF."
                });
            }

            var tabelaUsuario =
                entidadeUsuario.GetTableName();

            if (string.IsNullOrWhiteSpace(
                tabelaUsuario))
            {
                return Conflict(new
                {
                    mensagem =
                        "Não foi possível identificar a tabela Usuarios."
                });
            }

            var fkUsuario =
                entidadeUsuario
                    .GetForeignKeys()
                    .FirstOrDefault(fk =>
                        fk.PrincipalEntityType.ClrType ==
                        typeof(Funcionario));

            if (fkUsuario == null ||
                fkUsuario.Properties.Count != 1)
            {
                return Conflict(new
                {
                    mensagem =
                        "Não foi possível identificar a relação " +
                        "Usuario → Funcionario."
                });
            }

            var colunaFuncionarioUsuario =
                fkUsuario.Properties[0]
                    .GetColumnName(
                        StoreObjectIdentifier.Table(
                            tabelaUsuario,
                            entidadeUsuario.GetSchema()));

            if (string.IsNullOrWhiteSpace(
                colunaFuncionarioUsuario))
            {
                return Conflict(new
                {
                    mensagem =
                        "Não foi possível identificar a coluna " +
                        "FuncionarioId."
                });
            }

            var sqlUsuarioExiste = $@"
SELECT COUNT(*)
FROM `{tabelaUsuario}`
WHERE `{colunaFuncionarioUsuario}` =
      @funcionarioId";

            int quantidadeUsuariosEliminar;

            await using (
                var command =
                    _context.Database
                        .GetDbConnection()
                        .CreateCommand())
            {
                command.CommandText =
                    sqlUsuarioExiste;

                var parameter =
                    command.CreateParameter();

                parameter.ParameterName =
                    "@funcionarioId";

                parameter.Value =
                    candidatoEliminar.Id;

                command.Parameters.Add(
                    parameter);

                if (command.Connection!.State !=
                    System.Data.ConnectionState.Open)
                {
                    await command.Connection
                        .OpenAsync();
                }

                var valor =
                    await command
                        .ExecuteScalarAsync();

                quantidadeUsuariosEliminar =
                    Convert.ToInt32(valor);
            }

            if (quantidadeUsuariosEliminar > 1)
            {
                return Conflict(new
                {
                    mensagem =
                        "Foram encontrados vários usuários associados " +
                        "ao funcionário duplicado. A operação foi bloqueada."
                });
            }

            if (quantidadeUsuariosEliminar == 0)
            {
                return Conflict(new
                {
                    mensagem =
                        "O funcionário duplicado não possui usuário associado. " +
                        "A consolidação foi bloqueada."
                });
            }

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {
                var sqlEliminarUsuario = $@"
DELETE FROM `{tabelaUsuario}`
WHERE `{colunaFuncionarioUsuario}` =
      @funcionarioId";

                int usuariosEliminados;

                await using (
                    var command =
                        _context.Database
                            .GetDbConnection()
                            .CreateCommand())
                {
                    command.Transaction =
                        transaction.GetDbTransaction();

                    command.CommandText =
                        sqlEliminarUsuario;

                    var parameter =
                        command.CreateParameter();

                    parameter.ParameterName =
                        "@funcionarioId";

                    parameter.Value =
                        candidatoEliminar.Id;

                    command.Parameters.Add(
                        parameter);

                    usuariosEliminados =
                        await command
                            .ExecuteNonQueryAsync();
                }

                if (usuariosEliminados != 1)
                {
                    throw new InvalidOperationException(
                        "O sistema esperava eliminar exatamente um usuário.");
                }

                _context.Funcionarios.Remove(
                    candidatoEliminar);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    sucesso = true,

                    mensagem =
                        "Duplicado consolidado com sucesso.",

                    mantido = new
                    {
                        id =
                            candidatoManter.Id,

                        nome =
                            candidatoManter.NomeCompleto,

                        nip =
                            candidatoManter.Nip,

                        categoria =
                            candidatoManter.Categoria.ToString(),

                        funcao =
                            candidatoManter.Funcao,

                        seccaoId =
                            candidatoManter.SeccaoId,

                        totalDependencias =
                            totalDependenciasPorFuncionario[
                                candidatoManter.Id]
                    },

                    eliminado = new
                    {
                        id =
                            candidatoEliminar.Id,

                        nome =
                            candidatoEliminar.NomeCompleto,

                        nip =
                            candidatoEliminar.Nip,

                        categoria =
                            candidatoEliminar.Categoria.ToString(),

                        funcao =
                            candidatoEliminar.Funcao,

                        seccaoId =
                            candidatoEliminar.SeccaoId,

                        totalDependencias =
                            totalDependenciasPorFuncionario[
                                candidatoEliminar.Id]
                    },

                    usuarioEliminado = true,

                    dependenciasEliminadas =
                        dependenciasEliminar
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return StatusCode(
                    500,
                    new
                    {
                        sucesso = false,

                        mensagem =
                            "A consolidação falhou. " +
                            "A transação foi revertida.",

                        detalhe =
                            ex.InnerException?.Message ??
                            ex.Message
                    });
            }
        }

        // ============================================================
        // GET:
        // api/Funcionarios/duplicados/usuario-dependencias
        // ============================================================

        [Authorize(
            Roles =
                PerfisUsuario.Administrador + "," +
                PerfisUsuario.Gestor)]
        [HttpGet("duplicados/usuario-dependencias")]
        public async Task<IActionResult>
            ObterDependenciasUsuariosDuplicados()
        {
            var funcionarios =
                await _context.Funcionarios
                    .AsNoTracking()
                    .ToListAsync();

            var gruposDuplicados =
                funcionarios
                    .Where(f =>
                        !string.IsNullOrWhiteSpace(
                            f.NomeCompleto))
                    .GroupBy(f =>
                        f.NomeCompleto!
                            .Trim()
                            .ToUpper())
                    .Where(g =>
                        g.Count() > 1)
                    .OrderBy(g => g.Key)
                    .ToList();

            var resultado =
                new List<UsuarioDuplicadoDependenciasDto>();

            var entidadeUsuario =
                _context.Model
                    .GetEntityTypes()
                    .FirstOrDefault(e =>
                        e.ClrType ==
                        typeof(Usuario));

            if (entidadeUsuario == null)
            {
                return NotFound(new
                {
                    mensagem =
                        "A entidade Usuario não foi encontrada no modelo EF."
                });
            }

            var entidadesDependentes =
                _context.Model
                    .GetEntityTypes()
                    .Where(e =>
                        e.GetForeignKeys()
                            .Any(fk =>
                                fk.PrincipalEntityType.ClrType ==
                                entidadeUsuario.ClrType))
                    .ToList();

            foreach (var grupo in gruposDuplicados)
            {
                foreach (var funcionario in
                    grupo.OrderBy(f => f.Id))
                {
                    var usuario =
                        await _context.Usuarios
                            .AsNoTracking()
                            .FirstOrDefaultAsync(
                                u =>
                                    u.FuncionarioId ==
                                    funcionario.Id);

                    if (usuario == null)
                    {
                        resultado.Add(
                            new UsuarioDuplicadoDependenciasDto
                            {
                                FuncionarioId =
                                    funcionario.Id,

                                NomeCompleto =
                                    funcionario.NomeCompleto,

                                UsuarioId = 0
                            });

                        continue;
                    }

                    var usuarioId =
                        usuario.Id;

                    var item =
                        new UsuarioDuplicadoDependenciasDto
                        {
                            FuncionarioId =
                                funcionario.Id,

                            NomeCompleto =
                                funcionario.NomeCompleto,

                            UsuarioId =
                                usuarioId
                        };

                    foreach (var entidade
                        in entidadesDependentes)
                    {
                        var foreignKeys =
                            entidade.GetForeignKeys()
                                .Where(fk =>
                                    fk.PrincipalEntityType.ClrType ==
                                    entidadeUsuario.ClrType)
                                .ToList();

                        foreach (var fk in foreignKeys)
                        {
                            if (fk.Properties.Count != 1)
                                continue;

                            var tabela =
                                entidade.GetTableName();

                            if (string.IsNullOrWhiteSpace(
                                tabela))
                                continue;

                            var propriedadeFk =
                                fk.Properties[0];

                            var coluna =
                                propriedadeFk.GetColumnName(
                                    StoreObjectIdentifier.Table(
                                        tabela,
                                        entidade.GetSchema()));

                            if (string.IsNullOrWhiteSpace(
                                coluna))
                                continue;

                            var sql = $@"
SELECT COUNT(*)
FROM `{tabela}`
WHERE `{coluna}` = @usuarioId";

                            await using var command =
                                _context.Database
                                    .GetDbConnection()
                                    .CreateCommand();

                            command.CommandText =
                                sql;

                            var parameter =
                                command.CreateParameter();

                            parameter.ParameterName =
                                "@usuarioId";

                            parameter.Value =
                                usuarioId;

                            command.Parameters.Add(
                                parameter);

                            if (command.Connection!.State !=
                                System.Data.ConnectionState.Open)
                            {
                                await command.Connection
                                    .OpenAsync();
                            }

                            var valor =
                                await command
                                    .ExecuteScalarAsync();

                            var quantidade =
                                Convert.ToInt32(valor);

                            if (quantidade > 0)
                            {
                                item.Dependencias.Add(
                                    new DependenciaUsuarioDto
                                    {
                                        UsuarioId =
                                            usuarioId,

                                        Tabela =
                                            tabela,

                                        Coluna =
                                            coluna,

                                        Quantidade =
                                            quantidade
                                    });
                            }
                        }
                    }

                    item.TotalDependencias =
                        item.Dependencias
                            .Sum(d =>
                                d.Quantidade);

                    resultado.Add(item);
                }
            }

            return Ok(resultado);
        }

        // ============================================================
        // DELETE: api/Funcionarios/{id}
        // ============================================================

        [Authorize(
            Roles =
                PerfisUsuario.Administrador + "," +
                PerfisUsuario.Gestor)]
        [HttpDelete("{id}")]
        public async Task<IActionResult>
            InativarFuncionario(int id)
        {
            var funcionario =
                await _context.Funcionarios
                    .FindAsync(id);

            if (funcionario == null)
            {
                return NotFound(
                    "Funcionário não encontrado.");
            }

            funcionario.Estado =
                EstadoFuncionario.INACTIVO;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem =
                    "Funcionário inativado com sucesso."
            });
        }
    }
}