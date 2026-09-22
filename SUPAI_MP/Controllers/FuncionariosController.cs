using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using supai_mp.Data;
using supai_mp.DTOs;
using supai_mp.Models;
using supai_mp.Models.DTOs;
using SUPAI_MP.Data.DTOs;
using System.Linq.Expressions;
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
        // ORDENAÇÃO GLOBAL DO SUPAI-MP
        // ============================================================

        /*
         * IMPORTANTE:
         * A base de dados atualmente guarda a função como TEXTO.
         *
         * Exemplos reais encontrados:
         *
         * COMANDANTE DA SUBUNIDADE
         * CHEFE DAS OPERACOES
         * CHEFE DA DOUTRINA E ETICA
         * CHEFE DA PEAC
         * CHEFE DE SEGURANCA PESSOAL
         * CHEFE DE PROTECCAO DE OBJECTOS
         * CHEFE DA LOGISTICA E FINANCAS
         * CHEFE DE GESTAO DE PESSOAL E FORMACAO
         * CHEFE DA SECRETARIA
         * CHEFE DE INFORMACAO OPERATIVA
         * CHEFE DE INFORMACAO INTERNA
         */

        private static readonly Expression<Func<Funcionario, int>>
            OrdemGrupoChefia = f =>
                f.Funcao == "COMANDANTE DA SUBUNIDADE" ||
                f.Funcao == "CHEFE DAS OPERACOES" ||
                f.Funcao == "CHEFE DA DOUTRINA E ETICA" ||
                f.Funcao == "CHEFE DA PEAC" ||
                f.Funcao == "CHEFE DE SEGURANCA PESSOAL" ||
                f.Funcao == "CHEFE DE PROTECCAO DE OBJECTOS" ||
                f.Funcao == "CHEFE DA LOGISTICA E FINANCAS" ||
                f.Funcao == "CHEFE DE GESTAO DE PESSOAL E FORMACAO" ||
                f.Funcao == "CHEFE DA SECRETARIA" ||
                f.Funcao == "CHEFE DE INFORMACAO OPERATIVA" ||
                f.Funcao == "CHEFE DE INFORMACAO INTERNA"
                    ? 0
                    : 1;

        private static readonly Expression<Func<Funcionario, int>>
            OrdemChefia = f =>
                f.Funcao == "COMANDANTE DA SUBUNIDADE"
                    ? 1
                    : f.Funcao == "CHEFE DAS OPERACOES"
                        ? 2
                        : f.Funcao == "CHEFE DA DOUTRINA E ETICA"
                            ? 3
                            : f.Funcao == "CHEFE DA PEAC"
                                ? 4
                                : f.Funcao == "CHEFE DE SEGURANCA PESSOAL"
                                    ? 5
                                    : f.Funcao == "CHEFE DE PROTECCAO DE OBJECTOS"
                                        ? 6
                                        : f.Funcao == "CHEFE DA LOGISTICA E FINANCAS"
                                            ? 7
                                            : f.Funcao == "CHEFE DE GESTAO DE PESSOAL E FORMACAO"
                                                ? 8
                                                : f.Funcao == "CHEFE DA SECRETARIA"
                                                    ? 9
                                                    : f.Funcao == "CHEFE DE INFORMACAO OPERATIVA"
                                                        ? 10
                                                        : f.Funcao == "CHEFE DE INFORMACAO INTERNA"
                                                            ? 11
                                                            : 999;

        private static readonly Expression<Func<Funcionario, int>>
            OrdemCategoria = f =>
                f.Categoria == Categoria.IPG
                    ? 1
                    : f.Categoria == Categoria.COM
                        ? 2
                        : f.Categoria == Categoria.PAC
                            ? 3
                            : f.Categoria == Categoria.AJC
                                ? 4
                                : f.Categoria == Categoria.SPP
                                    ? 5
                                    : f.Categoria == Categoria.SUP
                                        ? 6
                                        : f.Categoria == Categoria.ASP
                                            ? 7
                                            : f.Categoria == Categoria.INP
                                                ? 8
                                                : f.Categoria == Categoria.INS
                                                    ? 9
                                                    : f.Categoria == Categoria.SUB
                                                        ? 10
                                                        : f.Categoria == Categoria.SAP
                                                            ? 11
                                                            : f.Categoria == Categoria.SAR
                                                                ? 12
                                                                : f.Categoria == Categoria.PC
                                                                    ? 13
                                                                    : f.Categoria == Categoria.SC
                                                                        ? 14
                                                                        : f.Categoria == Categoria.GUA
                                                                            ? 15
                                                                            : 999;

        private static IQueryable<Funcionario>
            AplicarOrdenacaoGlobal(IQueryable<Funcionario> query)
        {
            return query
                .OrderBy(OrdemGrupoChefia)
                .ThenBy(OrdemChefia)
                .ThenBy(OrdemCategoria)
                .ThenBy(f => f.NomeCompleto);
        }

        // ============================================================
        // ORDENAÇÃO EM MEMÓRIA
        // Usada quando já carregámos entidades para depois projectar.
        // ============================================================

        private static int ObterOrdemChefiaMemoria(Funcionario funcionario)
        {
            return funcionario.Funcao?.Trim() switch
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

        private static int ObterOrdemCategoriaMemoria(
            Categoria categoria)
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

        private static IEnumerable<Funcionario>
            OrdenarFuncionariosEmMemoria(
                IEnumerable<Funcionario> funcionarios)
        {
            return funcionarios
                .OrderBy(f =>
                    ObterOrdemChefiaMemoria(f) < 999 ? 0 : 1)
                .ThenBy(ObterOrdemChefiaMemoria)
                .ThenBy(f => ObterOrdemCategoriaMemoria(f.Categoria))
                .ThenBy(
                    f => f.NomeCompleto,
                    StringComparer.CurrentCultureIgnoreCase);
        }

        // ============================================================
        // GET: api/Funcionarios
        // ============================================================

        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> GetFuncionarios()
        {
            var query = _context.Funcionarios
                .Where(f =>
                    f.Estado == EstadoFuncionario.ACTIVO)
                .AsQueryable();

            var funcionarios = await AplicarOrdenacaoGlobal(query)
                .ToListAsync();

            return Ok(funcionarios);
        }

        // ============================================================
        // GET: api/Funcionarios/meu-perfil
        // ============================================================

        [Authorize]
        [HttpGet("meu-perfil")]
        public async Task<IActionResult> MeuPerfil()
        {
            var usuarioIdString =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(usuarioIdString))
            {
                return Unauthorized(
                    "Não foi possível identificar o utilizador autenticado.");
            }

            if (!int.TryParse(
                usuarioIdString,
                out int usuarioId))
            {
                return Unauthorized(
                    "Identificador do utilizador inválido.");
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Funcionario)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario == null)
            {
                return NotFound(
                    "Utilizador não encontrado.");
            }

            if (!usuario.Ativo)
            {
                return Unauthorized(
                    "Este utilizador está inativo.");
            }

            if (usuario.Funcionario == null)
            {
                return NotFound(
                    "Este utilizador não está associado a um funcionário.");
            }

            return Ok(usuario.Funcionario);
        }

        // ============================================================
        // PUT: api/Funcionarios/meu-perfil
        // ============================================================

        [Authorize]
        [HttpPut("meu-perfil")]
        public async Task<IActionResult> AtualizarMeuPerfil(
            AtualizarMeuPerfilDto dto)
        {
            var usuarioIdString =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(usuarioIdString))
            {
                return Unauthorized();
            }

            if (!int.TryParse(
                usuarioIdString,
                out int usuarioId))
            {
                return Unauthorized();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Funcionario)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario == null)
            {
                return NotFound(
                    "Utilizador não encontrado.");
            }

            if (usuario.Funcionario == null)
            {
                return NotFound(
                    "Este utilizador ainda não está associado a um funcionário.");
            }

            var funcionario = usuario.Funcionario;

            funcionario.Contacto = dto.Contacto;
            funcionario.C_Alternativo = dto.C_Alternativo;
            funcionario.C_Familiar = dto.C_Familiar;
            funcionario.Bairro = dto.Bairro;
            funcionario.Quarterao_N = dto.Quarterao_N;
            funcionario.Casa_N = dto.Casa_N;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Perfil atualizado com sucesso."
            });
        }

        // ============================================================
        // GET: api/Funcionarios/pesquisar?termo=Joao
        // ============================================================

        [Authorize(Roles = "Administrador")]
        [HttpGet("pesquisar")]
        public async Task<IActionResult> PesquisarFuncionarios(
            string termo)
        {
            if (string.IsNullOrWhiteSpace(termo))
            {
                return BadRequest(
                    "Digite um termo para pesquisar.");
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
                await AplicarOrdenacaoGlobal(query)
                    .ToListAsync();

            return Ok(funcionarios);
        }

        // ============================================================
        // POST: api/Funcionarios/minha-fotografia
        // ============================================================

        [Authorize]
        [HttpPost("minha-fotografia")]
        public async Task<IActionResult>
            AtualizarMinhaFotografia(
                IFormFile fotografia)
        {
            if (fotografia == null ||
                fotografia.Length == 0)
            {
                return BadRequest(
                    "Selecione uma fotografia.");
            }

            var usuarioIdString =
                User.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(usuarioIdString))
            {
                return Unauthorized();
            }

            if (!int.TryParse(
                usuarioIdString,
                out int usuarioId))
            {
                return Unauthorized();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Funcionario)
                .FirstOrDefaultAsync(
                    u => u.Id == usuarioId);

            if (usuario == null)
            {
                return NotFound(
                    "Utilizador não encontrado.");
            }

            if (usuario.Funcionario == null)
            {
                return NotFound(
                    "Este utilizador não está associado a um funcionário.");
            }

            var extensoesPermitidas =
                new[] { ".jpg", ".jpeg", ".png" };

            var extensao = Path
                .GetExtension(fotografia.FileName)
                .ToLowerInvariant();

            if (!extensoesPermitidas.Contains(extensao))
            {
                return BadRequest(
                    "Formato inválido. Use JPG, JPEG ou PNG.");
            }

            if (fotografia.Length >
                5 * 1024 * 1024)
            {
                return BadRequest(
                    "A fotografia não pode ultrapassar 5 MB.");
            }

            var fotosPath =
                Environment.GetEnvironmentVariable(
                    "FOTOS_PATH");

            if (string.IsNullOrWhiteSpace(fotosPath))
            {
                fotosPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "fotos");
            }

            if (!Directory.Exists(fotosPath))
            {
                Directory.CreateDirectory(fotosPath);
            }

            if (!string.IsNullOrEmpty(
                usuario.Funcionario.FotografiaUrl))
            {
                var nomeFotoAntiga =
                    Path.GetFileName(
                        usuario.Funcionario.FotografiaUrl);

                var caminhoFotoAntiga =
                    Path.Combine(
                        fotosPath,
                        nomeFotoAntiga);

                if (System.IO.File.Exists(
                    caminhoFotoAntiga))
                {
                    System.IO.File.Delete(
                        caminhoFotoAntiga);
                }
            }

            var nomeArquivo =
                $"{Guid.NewGuid()}{extensao}";

            var caminhoArquivo =
                Path.Combine(
                    fotosPath,
                    nomeArquivo);

            using (var stream =
                new FileStream(
                    caminhoArquivo,
                    FileMode.Create))
            {
                await fotografia.CopyToAsync(stream);
            }

            usuario.Funcionario.FotografiaUrl =
                $"/fotos/{nomeArquivo}";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem =
                    "Fotografia atualizada com sucesso.",
                fotografiaUrl =
                    usuario.Funcionario.FotografiaUrl
            });
        }

        // ============================================================
        // PUT: api/Funcionarios/alterar-senha
        // ============================================================

        [Authorize]
        [HttpPut("alterar-senha")]
        public async Task<IActionResult> AlterarSenha(
            AlterarSenhaDto dto)
        {
            var usuarioIdString =
                User.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(usuarioIdString))
            {
                return Unauthorized();
            }

            if (!int.TryParse(
                usuarioIdString,
                out int usuarioId))
            {
                return Unauthorized();
            }

            var usuario =
                await _context.Usuarios
                    .FirstOrDefaultAsync(
                        u => u.Id == usuarioId);

            if (usuario == null)
            {
                return NotFound(
                    "Utilizador não encontrado.");
            }

            if (!usuario.Ativo)
            {
                return Unauthorized(
                    "Este utilizador está inativo.");
            }

            var senhaValida =
                BCrypt.Net.BCrypt.Verify(
                    dto.SenhaAtual,
                    usuario.SenhaHash);

            if (!senhaValida)
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

        [Authorize(Roles = "Administrador")]
        [HttpGet("pesquisa-avancada")]
        public async Task<IActionResult> PesquisaAvancada(
            [FromQuery] FuncionarioPesquisaDto filtro)
        {
            var query =
                _context.Funcionarios.AsQueryable();

            if (!string.IsNullOrWhiteSpace(
                filtro.Nome))
            {
                query = query.Where(f =>
                    f.NomeCompleto.Contains(
                        filtro.Nome));
            }

            if (!string.IsNullOrWhiteSpace(
                filtro.Nip))
            {
                query = query.Where(f =>
                    f.Nip.Contains(
                        filtro.Nip));
            }

            if (!string.IsNullOrWhiteSpace(
                filtro.Bi))
            {
                query = query.Where(f =>
                    f.Bi.Contains(
                        filtro.Bi));
            }

            if (!string.IsNullOrWhiteSpace(
                filtro.Nuit))
            {
                query = query.Where(f =>
                    f.Nuit.Contains(
                        filtro.Nuit));
            }

            if (!string.IsNullOrWhiteSpace(
                filtro.Contacto))
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

            if (!string.IsNullOrWhiteSpace(
                filtro.Funcao))
            {
                query = query.Where(f =>
                    f.Funcao.Contains(
                        filtro.Funcao));
            }

            if (!string.IsNullOrWhiteSpace(
                filtro.LocalTrabalho))
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
                await AplicarOrdenacaoGlobal(query)
                    .ToListAsync();

            return Ok(funcionarios);
        }

        // ============================================================
        // GET: api/Funcionarios/acessos/{id}
        // ============================================================

        [Authorize(Roles = "Administrador")]
        [HttpGet("acessos/{id}")]
        public async Task<IActionResult> GetAcesso(
            int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Funcionario)
                .FirstOrDefaultAsync(
                    u => u.Id == id);

            if (usuario == null)
            {
                return NotFound(new
                {
                    mensagem =
                        "Acesso não encontrado."
                });
            }

            return Ok(new
            {
                id = usuario.Id,

                funcionarioId =
                    usuario.FuncionarioId,

                nomeCompleto =
                    usuario.Funcionario != null
                        ? usuario.Funcionario.NomeCompleto
                        : null,

                nip =
                    usuario.Funcionario != null
                        ? usuario.Funcionario.Nip
                        : null,

                nomeUsuario =
                    usuario.NomeUsuario,

                perfil =
                    usuario.Perfil,

                ativo =
                    usuario.Ativo
            });
        }

        // ============================================================
        // PUT: api/Funcionarios/acessos/{id}
        // ============================================================

        [Authorize(Roles = "Administrador")]
        [HttpPut("acessos/{id}")]
        public async Task<IActionResult>
            AtualizarAcesso(
                int id,
                EditarAcessoDto dto)
        {
            var usuario =
                await _context.Usuarios
                    .FirstOrDefaultAsync(
                        u => u.Id == id);

            if (usuario == null)
            {
                return NotFound(new
                {
                    mensagem =
                        "Acesso não encontrado."
                });
            }

            if (string.IsNullOrWhiteSpace(
                dto.NomeUsuario))
            {
                return BadRequest(new
                {
                    mensagem =
                        "O nome de usuário é obrigatório."
                });
            }

            var nomeExiste =
                await _context.Usuarios.AnyAsync(
                    u =>
                        u.NomeUsuario ==
                        dto.NomeUsuario &&
                        u.Id != id);

            if (nomeExiste)
            {
                return BadRequest(new
                {
                    mensagem =
                        "O nome de usuário já está em uso."
                });
            }

            usuario.NomeUsuario =
                dto.NomeUsuario;

            if (!string.IsNullOrWhiteSpace(
                dto.Perfil))
            {
                usuario.Perfil =
                    dto.Perfil;
            }

            if (!string.IsNullOrWhiteSpace(
                dto.NovaSenha))
            {
                usuario.SenhaHash =
                    BCrypt.Net.BCrypt.HashPassword(
                        dto.NovaSenha);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem =
                    "Dados de acesso atualizados com sucesso."
            });
        }

        // ============================================================
        // GET: api/Funcionarios/paginado
        // ============================================================

        [Authorize(Roles = "Administrador")]
        [HttpGet("paginado")]
        public async Task<IActionResult>
            GetFuncionariosPaginado(
                [FromQuery] int pagina = 1,
                [FromQuery] int tamanhoPagina = 20,
                [FromQuery] string? nome = null)
        {
            if (pagina < 1)
                pagina = 1;

            if (tamanhoPagina < 1)
                tamanhoPagina = 20;

            if (tamanhoPagina > 100)
                tamanhoPagina = 100;

            nome = nome?.Trim();

            var query =
                _context.Funcionarios
                    .AsNoTracking()
                    .AsQueryable();

            if (!string.IsNullOrWhiteSpace(nome))
            {
                query = query.Where(f =>
                    f.NomeCompleto != null &&
                    f.NomeCompleto.Contains(nome));
            }

            // ========================================================
            // TOTAIS
            // ========================================================

            var totalRegistros =
                await query.CountAsync();

            var totalMasculino =
                await query.CountAsync(
                    f => f.G == Genero.M);

            var totalFeminino =
                await query.CountAsync(
                    f => f.G == Genero.F);

            var totalAtivos =
                await query.CountAsync(
                    f =>
                        f.Estado ==
                        EstadoFuncionario.ACTIVO);

            var totaisPorCategoria =
                await query
                    .GroupBy(f => f.Categoria)
                    .Select(g => new
                    {
                        Categoria = g.Key,
                        Total = g.Count()
                    })
                    .ToListAsync();

            var totalPaginas =
                (int)Math.Ceiling(
                    totalRegistros /
                    (double)tamanhoPagina);

            if (totalPaginas > 0 &&
                pagina > totalPaginas)
            {
                pagina = totalPaginas;
            }

            if (totalPaginas == 0)
            {
                pagina = 1;
            }

            // ========================================================
            // DADOS PAGINADOS
            //
            // A ORDENAÇÃO ACONTECE ANTES DO SKIP/TAKE.
            // ========================================================

            var funcionarios =
                await AplicarOrdenacaoGlobal(query)
                    .Skip(
                        (pagina - 1) *
                        tamanhoPagina)
                    .Take(tamanhoPagina)
                    .Select(f => new
                    {
                        f.Id,
                        f.NomeCompleto,
                        f.Nip,
                        f.Bi,
                        f.Nuit,

                        Genero = (int)f.G,

                        f.estado_civil,
                        f.nivelAcademico,
                        f.grauParentesco,

                        f.DataNascimento,

                        f.Categoria,
                        f.Funcao,

                        f.DataIngresso,
                        f.LocalTrabalho,

                        f.Contacto,
                        f.C_Alternativo,
                        f.C_Familiar,

                        f.Bairro,
                        f.Quarterao_N,
                        f.Casa_N,

                        f.Estado,
                        f.FotografiaUrl,

                        SeccaoId =
                            f.SeccaoId,

                        SeccaoNome =
                            f.Seccao != null
                                ? f.Seccao.Nome
                                : "Sem secção",

                        TemUsuario =
                            _context.Usuarios.Any(
                                u =>
                                    u.FuncionarioId ==
                                    f.Id)
                    })
                    .ToListAsync();

            // ========================================================
            // CONVERTER CATEGORIAS PARA OS CÓDIGOS UTILIZADOS PELO MVC
            // ========================================================

            var categorias =
                totaisPorCategoria
                    .ToDictionary(
                        x => x.Categoria switch
                        {
                            Categoria.GUA => "GUA",
                            Categoria.SC => "SC",
                            Categoria.PC => "PC",
                            Categoria.SAR => "SAR",
                            Categoria.SAP => "SAP",
                            Categoria.SUB => "SUB",
                            Categoria.INS => "INS",
                            Categoria.INP => "INP",
                            Categoria.ASP => "ASP",
                            Categoria.SUP => "SUP",
                            Categoria.SPP => "SPP",
                            Categoria.IPG => "IGP",
                            Categoria.COM => "COM",
                            Categoria.AJC => "AJC",
                            Categoria.PAC => "PAC",

                            _ => x.Categoria.ToString()
                        },
                        x => x.Total);

            return Ok(new
            {
                pagina,
                tamanhoPagina,
                nome,

                totalRegistros,
                totalPaginas,

                totalMasculino,
                totalFeminino,
                totalAtivos,

                totaisPorCategoria =
                    categorias,

                dados =
                    funcionarios
            });
        }

        // ============================================================
        // POST: api/Funcionarios
        // ============================================================

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult>
            CriarFuncionario(
                FuncionarioCreateDto dto)
        {
            var usuarioExiste =
                await _context.Usuarios.AnyAsync(
                    u =>
                        u.NomeUsuario ==
                        dto.NomeUsuario);

            if (usuarioExiste)
            {
                return BadRequest(new
                {
                    mensagem =
                        "O nome de usuário já está em uso."
                });
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

                    Categoria =
                        dto.Categoria,

                    Funcao =
                        dto.Funcao,

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

                    LocalTrabalho =
                        dto.LocalTrabalho,

                    Bairro =
                        dto.Bairro,

                    Quarterao_N =
                        dto.Quarterao_N,

                    Casa_N =
                        dto.Casa_N,

                    Estado =
                        dto.EstadoFuncionario,

                    FotografiaUrl =
                        dto.FotografiaUrl,

                    DataCadastro =
                        DateTime.Now
                };

            _context.Funcionarios.Add(
                funcionario);

            await _context.SaveChangesAsync();

            var usuario =
                new Usuario
                {
                    NomeUsuario =
                        dto.NomeUsuario,

                    SenhaHash =
                        BCrypt.Net.BCrypt.HashPassword(
                            dto.Senha),

                    Perfil =
                        "Funcionario",

                    FuncionarioId =
                        funcionario.Id,

                    Ativo =
                        true,

                    DataCadastro =
                        DateTime.Now
                };

            _context.Usuarios.Add(
                usuario);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetFuncionario),
                new
                {
                    id = funcionario.Id
                },
                new
                {
                    mensagem =
                        "Funcionário e usuário criados com sucesso.",

                    funcionarioId =
                        funcionario.Id,

                    nomeUsuario =
                        usuario.NomeUsuario
                });
        }

        // ============================================================
        // GET: api/Funcionarios/{id}
        // ============================================================

        [Authorize(Roles = "Administrador")]
        [HttpGet("{id}")]
        public async Task<IActionResult>
            GetFuncionario(int id)
        {
            var funcionario =
                await _context.Funcionarios
                    .FindAsync(id);

            if (funcionario == null)
            {
                return NotFound();
            }

            return Ok(funcionario);
        }

        // ============================================================
        // PUT: api/Funcionarios/{id}
        // ============================================================

        [Authorize(Roles = "Administrador")]
        [HttpPut("{id}")]
        public async Task<IActionResult>
            AtualizarFuncionario(
                int id,
                FuncionarioUpdateDto dto)
        {
            var funcionario =
                await _context.Funcionarios
                    .FindAsync(id);

            if (funcionario == null)
            {
                return NotFound(
                    "Funcionário não encontrado.");
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

            funcionario.LocalTrabalho =
                dto.LocalTrabalho;

            funcionario.Bairro =
                dto.Bairro;

            funcionario.Quarterao_N =
                dto.Quarterao_N;

            funcionario.Casa_N =
                dto.Casa_N;

            funcionario.Categoria =
                dto.Categoria;

            funcionario.Funcao =
                dto.Funcao;

            funcionario.Estado =
                dto.EstadoFuncionario;

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

        [Authorize(Roles = "Administrador")]
        [HttpPost("{id}/criar-acesso")]
        public async Task<IActionResult>
            CriarAcesso(
                int id,
                CriarAcessoFuncionarioDto dto)
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

            var funcionarioTemUsuario =
                await _context.Usuarios
                    .AnyAsync(
                        u =>
                            u.FuncionarioId ==
                            id);

            if (funcionarioTemUsuario)
            {
                return BadRequest(new
                {
                    mensagem =
                        "Este funcionário já possui um usuário."
                });
            }

            var nomeUsuarioExiste =
                await _context.Usuarios
                    .AnyAsync(
                        u =>
                            u.NomeUsuario ==
                            dto.NomeUsuario);

            if (nomeUsuarioExiste)
            {
                return BadRequest(new
                {
                    mensagem =
                        "O nome de usuário já está em uso."
                });
            }

            var usuario =
                new Usuario
                {
                    NomeUsuario =
                        dto.NomeUsuario,

                    SenhaHash =
                        BCrypt.Net.BCrypt.HashPassword(
                            dto.Senha),

                    Perfil =
                        "Funcionario",

                    FuncionarioId =
                        funcionario.Id,

                    Ativo =
                        true,

                    DataCadastro =
                        DateTime.Now
                };

            _context.Usuarios.Add(
                usuario);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem =
                    "Acesso criado com sucesso.",

                usuario =
                    usuario.NomeUsuario,

                funcionarioId =
                    funcionario.Id
            });
        }

        // ============================================================
        // GET: api/Funcionarios/acessos
        // ============================================================

        [Authorize(Roles = "Administrador")]
        [HttpGet("acessos")]
        public async Task<IActionResult>
            ListarAcessos()
        {
            /*
             * Aqui carregamos primeiro os funcionários associados
             * aos usuários e depois aplicamos a mesma regra global.
             *
             * Isso garante que a lista de acessos também respeite
             * a hierarquia dos chefes.
             */

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
                                : null
                    })
                    .ToList();

            return Ok(acessos);
        }

        // ============================================================
        // PUT: api/Funcionarios/{id}/seccao
        // ============================================================

        [Authorize(Roles = "Administrador")]
        [HttpPut("{id}/seccao")]
        public async Task<IActionResult>
            AtribuirSeccao(
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

        [HttpGet("sem-seccao")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult>
            GetFuncionariosSemSeccao()
        {
            var query =
                _context.Funcionarios
                    .AsNoTracking()
                    .Where(f =>
                        f.SeccaoId == null)
                    .AsQueryable();

            var funcionarios =
                await AplicarOrdenacaoGlobal(query)
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
                            _context.Usuarios.Any(
                                u =>
                                    u.FuncionarioId ==
                                    f.Id),

                        f.SeccaoId
                    })
                    .ToListAsync();

            return Ok(funcionarios);
        }

        // ============================================================
        // GET: api/Funcionarios/com-seccao
        // ============================================================

        [HttpGet("com-seccao")]
        [Authorize(Roles = "Administrador")]
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

            var funcionarios =
                await AplicarOrdenacaoGlobal(
                        consulta)
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
                    .ToListAsync();

            return Ok(funcionarios);
        }

        [HttpGet("duplicados")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ObterDuplicados()
        {
            var funcionarios = await _context.Funcionarios
                .Include(f => f.Seccao)
                .AsNoTracking()
                .ToListAsync();

            var duplicados = funcionarios
                .Where(f => !string.IsNullOrWhiteSpace(f.NomeCompleto))
                .GroupBy(f => f.NomeCompleto.Trim().ToUpper())
                .Where(g => g.Count() > 1)
                .OrderBy(g => g.Key)
                .Select(g => new GrupoFuncionarioDuplicadoDto
                {
                    Nome = g.First().NomeCompleto.Trim(),
                    Quantidade = g.Count(),

                    Funcionarios = g
                        .OrderBy(f => f.Id)
                        .Select(f => new FuncionarioDuplicadoItemDto
                        {
                            Id = f.Id,
                            NomeCompleto = f.NomeCompleto,
                            Nip = f.Nip,
                            Categoria = (int)f.Categoria,
                            Funcao = f.Funcao,
                            SeccaoId = f.SeccaoId,
                            SeccaoNome = f.Seccao != null
                                ? f.Seccao.Nome
                                : "Sem secção",
                            Estado = (int)f.Estado
                        })
                        .ToList()
                })
                .ToList();

            return Ok(duplicados);
        }

        [HttpGet("duplicados/detalhes")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ObterDuplicadosDetalhes()
        {
            var funcionarios = await _context.Funcionarios
                .Include(f => f.Seccao)
                .AsNoTracking()
                .ToListAsync();

            var gruposDuplicados = funcionarios
                .Where(f => !string.IsNullOrWhiteSpace(f.NomeCompleto))
                .GroupBy(f => f.NomeCompleto.Trim().ToUpper())
                .Where(g => g.Count() > 1)
                .OrderBy(g => g.Key)
                .ToList();

            var resultado = new List<FuncionarioDuplicadoDetalheDto>();

            // Descobrir todas as entidades que possuem FK para Funcionario
            var entidadesDependentes = _context.Model
                .GetEntityTypes()
                .Where(e =>
                    e.GetForeignKeys()
                     .Any(fk => fk.PrincipalEntityType.ClrType == typeof(Funcionario)))
                .ToList();

            foreach (var grupo in gruposDuplicados)
            {
                foreach (var funcionario in grupo.OrderBy(f => f.Id))
                {
                    var detalhe = new FuncionarioDuplicadoDetalheDto
                    {
                        Id = funcionario.Id,
                        NomeCompleto = funcionario.NomeCompleto,
                        Nip = funcionario.Nip,
                        Categoria = (int)funcionario.Categoria,
                        Funcao = funcionario.Funcao,
                        SeccaoId = funcionario.SeccaoId,
                        SeccaoNome = funcionario.Seccao?.Nome ?? "Sem secção",
                        Estado = (int)funcionario.Estado
                    };

                    foreach (var entidade in entidadesDependentes)
                    {
                        var foreignKeys = entidade
                            .GetForeignKeys()
                            .Where(fk =>
                                fk.PrincipalEntityType.ClrType == typeof(Funcionario))
                            .ToList();

                        foreach (var fk in foreignKeys)
                        {
                            // Neste momento trabalhamos com FKs simples
                            if (fk.Properties.Count != 1)
                                continue;

                            var propriedadeFk = fk.Properties[0];

                            var tabela = entidade.GetTableName();
                            var coluna = propriedadeFk.GetColumnName(
                                StoreObjectIdentifier.Table(
                                    tabela!,
                                    entidade.GetSchema()));

                            if (string.IsNullOrWhiteSpace(tabela) ||
                                string.IsNullOrWhiteSpace(coluna))
                            {
                                continue;
                            }

                            var sql = $@"
                        SELECT COUNT(*)
                        FROM `{tabela}`
                        WHERE `{coluna}` = @funcionarioId";

                            await using var command =
                                _context.Database.GetDbConnection().CreateCommand();

                            command.CommandText = sql;

                            var parameter = command.CreateParameter();
                            parameter.ParameterName = "@funcionarioId";
                            parameter.Value = funcionario.Id;

                            command.Parameters.Add(parameter);

                            if (command.Connection!.State !=
                                System.Data.ConnectionState.Open)
                            {
                                await command.Connection.OpenAsync();
                            }

                            var valor = await command.ExecuteScalarAsync();

                            var quantidade = Convert.ToInt32(valor);

                            if (quantidade > 0)
                            {
                                detalhe.Dependencias.Add(
                                    new DependenciaFuncionarioDto
                                    {
                                        Entidade = entidade.ClrType.Name,
                                        Tabela = tabela,
                                        Quantidade = quantidade
                                    });
                            }
                        }
                    }

                    detalhe.TotalDependencias =
                        detalhe.Dependencias.Sum(d => d.Quantidade);

                    resultado.Add(detalhe);
                }
            }

            return Ok(resultado);
        }

        [HttpGet("duplicados/usuarios")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ObterDuplicadosComUsuarios()
        {
            var funcionarios = await _context.Funcionarios
                .Include(f => f.Seccao)
                .AsNoTracking()
                .ToListAsync();

            var idsDuplicados = funcionarios
                .Where(f => !string.IsNullOrWhiteSpace(f.NomeCompleto))
                .GroupBy(f => f.NomeCompleto.Trim().ToUpper())
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.Select(f => f.Id))
                .ToList();

            if (!idsDuplicados.Any())
                return Ok(new List<FuncionarioDuplicadoUsuarioDto>());

            var resultado = new List<FuncionarioDuplicadoUsuarioDto>();

            foreach (var funcionario in funcionarios
                .Where(f => idsDuplicados.Contains(f.Id))
                .OrderBy(f => f.NomeCompleto)
                .ThenBy(f => f.Id))
            {
                var item = new FuncionarioDuplicadoUsuarioDto
                {
                    Id = funcionario.Id,
                    NomeCompleto = funcionario.NomeCompleto,
                    Nip = funcionario.Nip,
                    SeccaoId = funcionario.SeccaoId,
                    SeccaoNome = funcionario.Seccao?.Nome ?? "Sem secção"
                };

                /*
                 * Procuramos automaticamente a entidade Usuario
                 * através do relacionamento definido no Entity Framework.
                 */
                var entidadeUsuario = _context.Model
                    .GetEntityTypes()
                    .FirstOrDefault(e =>
                        e.ClrType.Name.Equals(
                            "Usuario",
                            StringComparison.OrdinalIgnoreCase));

                if (entidadeUsuario != null)
                {
                    var foreignKey = entidadeUsuario
                        .GetForeignKeys()
                        .FirstOrDefault(fk =>
                            fk.PrincipalEntityType.ClrType ==
                            typeof(Funcionario));

                    if (foreignKey != null &&
                        foreignKey.Properties.Count == 1)
                    {
                        var propriedadeFuncionario =
                            foreignKey.Properties[0];

                        var tabela = entidadeUsuario.GetTableName();

                        if (!string.IsNullOrWhiteSpace(tabela))
                        {
                            var colunaFuncionario =
                                propriedadeFuncionario.GetColumnName(
                                    StoreObjectIdentifier.Table(
                                        tabela,
                                        entidadeUsuario.GetSchema()));

                            if (!string.IsNullOrWhiteSpace(colunaFuncionario))
                            {
                                var sql = $@"
                            SELECT *
                            FROM `{tabela}`
                            WHERE `{colunaFuncionario}` = @funcionarioId";

                                await using var command =
                                    _context.Database
                                        .GetDbConnection()
                                        .CreateCommand();

                                command.CommandText = sql;

                                var parameter =
                                    command.CreateParameter();

                                parameter.ParameterName =
                                    "@funcionarioId";

                                parameter.Value =
                                    funcionario.Id;

                                command.Parameters.Add(parameter);

                                if (command.Connection!.State !=
                                    System.Data.ConnectionState.Open)
                                {
                                    await command.Connection.OpenAsync();
                                }

                                await using var reader =
                                    await command.ExecuteReaderAsync();

                                while (await reader.ReadAsync())
                                {
                                    int usuarioId = 0;
                                    string? nomeUsuario = null;
                                    string? perfil = null;

                                    for (int i = 0;
                                         i < reader.FieldCount;
                                         i++)
                                    {
                                        var nomeColuna =
                                            reader.GetName(i);

                                        if (nomeColuna.Equals(
                                            "Id",
                                            StringComparison.OrdinalIgnoreCase))
                                        {
                                            if (!reader.IsDBNull(i))
                                                usuarioId =
                                                    Convert.ToInt32(
                                                        reader.GetValue(i));
                                        }

                                        if (nomeColuna.Equals(
                                            "Username",
                                            StringComparison.OrdinalIgnoreCase) ||
                                            nomeColuna.Equals(
                                            "NomeUsuario",
                                            StringComparison.OrdinalIgnoreCase) ||
                                            nomeColuna.Equals(
                                            "Nome",
                                            StringComparison.OrdinalIgnoreCase))
                                        {
                                            if (!reader.IsDBNull(i))
                                                nomeUsuario =
                                                    reader.GetValue(i)
                                                        .ToString();
                                        }

                                        if (nomeColuna.Equals(
                                            "Perfil",
                                            StringComparison.OrdinalIgnoreCase) ||
                                            nomeColuna.Equals(
                                            "Role",
                                            StringComparison.OrdinalIgnoreCase))
                                        {
                                            if (!reader.IsDBNull(i))
                                                perfil =
                                                    reader.GetValue(i)
                                                        .ToString();
                                        }
                                    }

                                    item.Usuarios.Add(
                                        new UsuarioFuncionarioDuplicadoDto
                                        {
                                            UsuarioId = usuarioId,
                                            NomeUsuario = nomeUsuario,
                                            Perfil = perfil
                                        });
                                }
                            }
                        }
                    }
                }

                resultado.Add(item);
            }

            return Ok(resultado);
        }

        // ============================================================
        // DELETE: api/Funcionarios/{id}
        // ============================================================

        [Authorize(Roles = "Administrador")]
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