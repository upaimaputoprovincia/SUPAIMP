using Microsoft.EntityFrameworkCore;
using supai_mp.Models;
using supai_mp.Models.Institucional;
using supai_mp.Models.Organizacao;


namespace supai_mp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Funcionario> Funcionarios { get; set; }

        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<Ferias> Ferias { get; set; }

        // Organização
        public DbSet<Seccao> Seccoes { get; set; }
        public DbSet<Sector> Sectores { get; set; }
        public DbSet<Equipa> Equipas { get; set; }
        public DbSet<Posto> Postos { get; set; }
        public DbSet<FuncaoOperacional> FuncoesOperacionais { get; set; }
        public DbSet<TipoTurno> TiposTurno { get; set; }
        public DbSet<LotacaoFuncionario> LotacoesFuncionarios { get; set; }
        public DbSet<Escala> Escalas { get; set; }
        public DbSet<UnidadeOperacional> UnidadesOperacionais { get; set; }
        public DbSet<GrupoEscala> GruposEscala { get; set; }

        public DbSet<EntidadeInstitucional> EntidadesInstitucionais { get; set; }

        public DbSet<UnidadeInstitucional> UnidadesInstitucionais { get; set; }

        public DbSet<TipoUnidadeInstitucional> TiposUnidadeInstitucional { get; set; }

        public DbSet<TipoPermissao> TiposPermissao { get; set; }

        public DbSet<PermissaoAcesso> PermissoesAcesso { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // =========================================================
            // SECCAO -> FUNCIONARIOS
            // =========================================================

            modelBuilder.Entity<Funcionario>()
                 .HasOne(f => f.Seccao)
                 .WithMany(s => s.Funcionarios)
                 .HasForeignKey(f => f.SeccaoId)
                 .OnDelete(DeleteBehavior.Restrict);

            // =========================================================
            // SECCAO -> SECTORES
            // =========================================================

            modelBuilder.Entity<Sector>()
                .HasOne(s => s.Seccao)
                .WithMany(s => s.Sectores)
                .HasForeignKey(s => s.SeccaoId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // SECCAO -> POSTOS
            // =========================================================

            modelBuilder.Entity<Posto>()
                .HasOne(p => p.Seccao)
                .WithMany(s => s.Postos)
                .HasForeignKey(p => p.SeccaoId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // SECTOR -> EQUIPAS
            // =========================================================

            modelBuilder.Entity<Equipa>()
                .HasOne(e => e.Sector)
                .WithMany(s => s.Equipas)
                .HasForeignKey(e => e.SectorId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // SECTOR -> POSTOS
            // =========================================================

            modelBuilder.Entity<Posto>()
                .HasOne(p => p.Sector)
                .WithMany(s => s.Postos)
                .HasForeignKey(p => p.SectorId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // FUNCIONARIO -> LOTACAO
            // =========================================================

            modelBuilder.Entity<LotacaoFuncionario>()
                .HasOne(l => l.Funcionario)
                .WithMany()
                .HasForeignKey(l => l.FuncionarioId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // SECCAO -> LOTACAO
            // =========================================================

            modelBuilder.Entity<LotacaoFuncionario>()
                .HasOne(l => l.Seccao)
                .WithMany(s => s.Lotacoes)
                .HasForeignKey(l => l.SeccaoId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // SECTOR -> LOTACAO
            // =========================================================

            modelBuilder.Entity<LotacaoFuncionario>()
                .HasOne(l => l.Sector)
                .WithMany(s => s.Lotacoes)
                .HasForeignKey(l => l.SectorId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // EQUIPA -> LOTACAO
            // =========================================================

            modelBuilder.Entity<LotacaoFuncionario>()
                .HasOne(l => l.Equipa)
                .WithMany(e => e.Lotacoes)
                .HasForeignKey(l => l.EquipaId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // POSTO -> LOTACAO
            // =========================================================

            modelBuilder.Entity<LotacaoFuncionario>()
                .HasOne(l => l.Posto)
                .WithMany(p => p.Lotacoes)
                .HasForeignKey(l => l.PostoId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // FUNCAO OPERACIONAL -> LOTACAO
            // =========================================================

            modelBuilder.Entity<LotacaoFuncionario>()
                .HasOne(l => l.FuncaoOperacional)
                .WithMany(f => f.Lotacoes)
                .HasForeignKey(l => l.FuncaoOperacionalId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // TIPO DE TURNO -> LOTACAO
            // =========================================================

            modelBuilder.Entity<LotacaoFuncionario>()
                .HasOne(l => l.TipoTurno)
                .WithMany()
                .HasForeignKey(l => l.TipoTurnoId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // CONFIGURACOES DOS CAMPOS
            // =========================================================

            modelBuilder.Entity<Seccao>()
                .Property(s => s.Nome)
                .HasMaxLength(150)
                .IsRequired();

            modelBuilder.Entity<Sector>()
                .Property(s => s.Nome)
                .HasMaxLength(150)
                .IsRequired();

            modelBuilder.Entity<Equipa>()
                .Property(e => e.Nome)
                .HasMaxLength(150)
                .IsRequired();

            modelBuilder.Entity<Posto>()
                .Property(p => p.Codigo)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Posto>()
                .Property(p => p.Nome)
                .HasMaxLength(200)
                .IsRequired();

            modelBuilder.Entity<FuncaoOperacional>()
                .Property(f => f.Nome)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<TipoTurno>()
                .Property(t => t.Nome)
                .HasMaxLength(50)
                .IsRequired();


            // =========================================================
            // INDICES
            // =========================================================

            modelBuilder.Entity<Posto>()
                .HasIndex(p => p.Codigo)
                .IsUnique();

            modelBuilder.Entity<Seccao>()
                .HasIndex(s => s.Nome)
                .IsUnique();

            modelBuilder.Entity<FuncaoOperacional>()
                .HasIndex(f => f.Nome)
                .IsUnique();

            modelBuilder.Entity<TipoTurno>()
                .HasIndex(t => t.Nome)
                .IsUnique();

            modelBuilder.Entity<Escala>()
            .HasOne(e => e.Funcionario)
            .WithMany()
            .HasForeignKey(e => e.FuncionarioId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Escala>()
                .HasOne(e => e.TipoTurno)
                .WithMany()
                .HasForeignKey(e => e.TipoTurnoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Escala>()
                .HasOne(e => e.Posto)
                .WithMany()
                .HasForeignKey(e => e.PostoId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Escala>()
            .HasIndex(e => new
            {
                e.FuncionarioId,
                e.Data,
                e.HoraInicio
            });
            modelBuilder.Entity<UnidadeOperacional>()
                .HasOne(u => u.Seccao)
                .WithMany()
                .HasForeignKey(u => u.SeccaoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UnidadeOperacional>()
                .HasOne(u => u.UnidadePai)
                .WithMany(u => u.UnidadesFilhas)
                .HasForeignKey(u => u.UnidadePaiId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Equipa>()
                .HasOne(e => e.UnidadeOperacional)
                .WithMany(u => u.Equipas)
                .HasForeignKey(e => e.UnidadeOperacionalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Posto>()
                .HasOne(p => p.UnidadeOperacional)
                .WithMany(u => u.Postos)
                .HasForeignKey(p => p.UnidadeOperacionalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LotacaoFuncionario>()
                .HasOne(l => l.UnidadeOperacional)
                .WithMany(u => u.Lotacoes)
                .HasForeignKey(l => l.UnidadeOperacionalId)
                .OnDelete(DeleteBehavior.Restrict);

            // GRUPO DE ESCALA -> UNIDADE OPERACIONAL
            modelBuilder.Entity<GrupoEscala>()
                .HasOne(g => g.UnidadeOperacional)
                .WithMany()
                .HasForeignKey(g => g.UnidadeOperacionalId)
                .OnDelete(DeleteBehavior.Restrict);

            // GRUPO DE ESCALA -> EQUIPA
            modelBuilder.Entity<GrupoEscala>()
                .HasOne(g => g.Equipa)
                .WithMany()
                .HasForeignKey(g => g.EquipaId)
                .OnDelete(DeleteBehavior.Restrict);

            // GRUPO DE ESCALA -> TIPO DE TURNO
            modelBuilder.Entity<GrupoEscala>()
                .HasOne(g => g.TipoTurno)
                .WithMany()
                .HasForeignKey(g => g.TipoTurnoId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================================================
            // ESCALA -> SECCAO
            // =========================================================

            modelBuilder.Entity<Escala>()
                .HasOne(e => e.Seccao)
                .WithMany()
                .HasForeignKey(e => e.SeccaoId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // ESCALA -> UNIDADE OPERACIONAL
            // =========================================================

            modelBuilder.Entity<Escala>()
                .HasOne(e => e.UnidadeOperacional)
                .WithMany()
                .HasForeignKey(e => e.UnidadeOperacionalId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // ESCALA -> EQUIPA
            // =========================================================

            modelBuilder.Entity<Escala>()
                .HasOne(e => e.Equipa)
                .WithMany()
                .HasForeignKey(e => e.EquipaId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // ESCALA -> FUNCAO OPERACIONAL
            // =========================================================

            modelBuilder.Entity<Escala>()
                .HasOne(e => e.FuncaoOperacional)
                .WithMany()
                .HasForeignKey(e => e.FuncaoOperacionalId)
                .OnDelete(DeleteBehavior.Restrict);


            // Evita duplicar a mesma ordem de rotação
            // dentro da mesma unidade operacional.
            modelBuilder.Entity<GrupoEscala>()
                .HasIndex(g => new
                {
                    g.UnidadeOperacionalId,
                    g.OrdemRotacao
                })
                .IsUnique();

            // =========================================================
            // USUARIO -> FUNCIONARIO
            // =========================================================

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Funcionario)
                .WithMany()
                .HasForeignKey(u => u.FuncionarioId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UnidadeOperacional>()
            .HasOne(u => u.Posto)
            .WithMany(p => p.UnidadesInternas)
            .HasForeignKey(u => u.PostoId)
            .OnDelete(DeleteBehavior.Restrict);

            // ============================================================
            // USUARIO -> UNIDADE INSTITUCIONAL
            // ============================================================

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.UnidadeInstitucional)
                .WithMany()
                .HasForeignKey(u => u.UnidadeInstitucionalId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================================
            // INFRAESTRUTURA INSTITUCIONAL
            // ============================================================

            // ------------------------------------------------------------
            // ENTIDADE INSTITUCIONAL -> UNIDADES INSTITUCIONAIS
            // ------------------------------------------------------------

            modelBuilder.Entity<UnidadeInstitucional>()
                .HasOne(u => u.EntidadeInstitucional)
                .WithMany(e => e.Unidades)
                .HasForeignKey(u => u.EntidadeInstitucionalId)
                .OnDelete(DeleteBehavior.Restrict);

            // ------------------------------------------------------------
            // TIPO DE UNIDADE INSTITUCIONAL -> UNIDADES
            // ------------------------------------------------------------

            modelBuilder.Entity<UnidadeInstitucional>()
                .HasOne(u => u.TipoUnidadeInstitucional)
                .WithMany(t => t.Unidades)
                .HasForeignKey(u => u.TipoUnidadeInstitucionalId)
                .OnDelete(DeleteBehavior.Restrict);

            // ------------------------------------------------------------
            // UNIDADE INSTITUCIONAL -> UNIDADE PAI
            // ------------------------------------------------------------

            modelBuilder.Entity<UnidadeInstitucional>()
                .HasOne(u => u.UnidadePai)
                .WithMany(u => u.UnidadesFilhas)
                .HasForeignKey(u => u.UnidadePaiId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // ------------------------------------------------------------
            // CONFIGURAÇÃO DOS CAMPOS
            // ------------------------------------------------------------

            modelBuilder.Entity<EntidadeInstitucional>()
                .Property(e => e.Nome)
                .HasMaxLength(200)
                .IsRequired();

            modelBuilder.Entity<EntidadeInstitucional>()
                .Property(e => e.Sigla)
                .HasMaxLength(50);

            modelBuilder.Entity<EntidadeInstitucional>()
                .Property(e => e.Descricao)
                .HasMaxLength(300);

            modelBuilder.Entity<TipoUnidadeInstitucional>()
                .Property(t => t.Nome)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<TipoUnidadeInstitucional>()
                .Property(t => t.Descricao)
                .HasMaxLength(300);

            modelBuilder.Entity<UnidadeInstitucional>()
                .Property(u => u.Nome)
                .HasMaxLength(200)
                .IsRequired();

            modelBuilder.Entity<UnidadeInstitucional>()
                .Property(u => u.Sigla)
                .HasMaxLength(50);

            modelBuilder.Entity<UnidadeInstitucional>()
                .Property(u => u.Descricao)
                .HasMaxLength(300);

            // ------------------------------------------------------------
            // ÍNDICES
            // ------------------------------------------------------------

            modelBuilder.Entity<EntidadeInstitucional>()
                .HasIndex(e => e.Nome)
                .IsUnique();

            modelBuilder.Entity<TipoUnidadeInstitucional>()
                .HasIndex(t => t.Nome)
                .IsUnique();

            modelBuilder.Entity<UnidadeInstitucional>()
                .HasIndex(u => new
                {
                    u.EntidadeInstitucionalId,
                    u.Nome
                })
                .IsUnique();

            // ============================================================
            // SEED - INFRAESTRUTURA INSTITUCIONAL INICIAL
            // ============================================================

            // ------------------------------------------------------------
            // ENTIDADE INSTITUCIONAL
            // ------------------------------------------------------------

            modelBuilder.Entity<EntidadeInstitucional>().HasData(
                new EntidadeInstitucional
                {
                    Id = 1,
                    Nome = "Comando Provincial da PRM – Maputo",
                    Sigla = "CPRM-Maputo",
                    Descricao = "Comando Provincial da Polícia da República de Moçambique – Maputo",
                    Ativo = true
                }
            );

            // ------------------------------------------------------------
            // TIPOS DE UNIDADE INSTITUCIONAL
            // ------------------------------------------------------------

            modelBuilder.Entity<TipoUnidadeInstitucional>().HasData(
                new TipoUnidadeInstitucional
                {
                    Id = 1,
                    Nome = "Comando Provincial",
                    Descricao = "Unidade institucional correspondente ao Comando Provincial.",
                    Ativo = true
                },

                new TipoUnidadeInstitucional
                {
                    Id = 2,
                    Nome = "Subunidade",
                    Descricao = "Subunidade institucional subordinada ao Comando Provincial.",
                    Ativo = true
                },

                new TipoUnidadeInstitucional
                {
                    Id = 3,
                    Nome = "Esquadra",
                    Descricao = "Esquadra policial integrada na estrutura institucional.",
                    Ativo = true
                },

                new TipoUnidadeInstitucional
                {
                    Id = 4,
                    Nome = "Posto",
                    Descricao = "Posto policial integrado na estrutura institucional.",
                    Ativo = true
                }
            );

            // ------------------------------------------------------------
            // UNIDADES INSTITUCIONAIS
            // ------------------------------------------------------------

            // COMANDO PROVINCIAL
            modelBuilder.Entity<UnidadeInstitucional>().HasData(
                new UnidadeInstitucional
                {
                    Id = 1,
                    Nome = "Comando Provincial da PRM – Maputo",
                    Sigla = "CPRM-Maputo",
                    Descricao = "Comando Provincial da Polícia da República de Moçambique – Maputo.",
                    TipoUnidadeInstitucionalId = 1,
                    EntidadeInstitucionalId = 1,
                    UnidadePaiId = null,
                    Ativo = true,
                    DataCadastro = new DateTime(2026, 10, 3)
                },

                // SUPAI-MP
                new UnidadeInstitucional
                {
                    Id = 2,
                    Nome = "SUPAI-MP",
                    Sigla = "SUPAI-MP",
                    Descricao = "Subunidade de Protecção de Altas Individualidades.",
                    TipoUnidadeInstitucionalId = 2,
                    EntidadeInstitucionalId = 1,
                    UnidadePaiId = 1,
                    Ativo = true,
                    DataCadastro = new DateTime(2026, 10, 3)
                }
            );

            // ============================================================
            // PERMISSÕES INSTITUCIONAIS
            // ============================================================

            // ------------------------------------------------------------
            // TIPO DE PERMISSÃO -> PERMISSÕES
            // ------------------------------------------------------------

            modelBuilder.Entity<PermissaoAcesso>()
                .HasOne(p => p.TipoPermissao)
                .WithMany(t => t.Permissoes)
                .HasForeignKey(p => p.TipoPermissaoId)
                .OnDelete(DeleteBehavior.Restrict);

            // ------------------------------------------------------------
            // USUARIO -> PERMISSÕES
            // ------------------------------------------------------------

            modelBuilder.Entity<PermissaoAcesso>()
                .HasOne(p => p.Usuario)
                .WithMany()
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // ------------------------------------------------------------
            // UNIDADE INSTITUCIONAL -> PERMISSÕES
            // ------------------------------------------------------------

            modelBuilder.Entity<PermissaoAcesso>()
                .HasOne(p => p.UnidadeInstitucional)
                .WithMany()
                .HasForeignKey(p => p.UnidadeInstitucionalId)
                .OnDelete(DeleteBehavior.Restrict);

            // ------------------------------------------------------------
            // CONFIGURAÇÃO DOS CAMPOS
            // ------------------------------------------------------------

            modelBuilder.Entity<TipoPermissao>()
                .Property(t => t.Codigo)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<TipoPermissao>()
                .Property(t => t.Nome)
                .HasMaxLength(150)
                .IsRequired();

            modelBuilder.Entity<TipoPermissao>()
                .Property(t => t.Descricao)
                .HasMaxLength(300);

            // ------------------------------------------------------------
            // ÍNDICES
            // ------------------------------------------------------------

            modelBuilder.Entity<TipoPermissao>()
                .HasIndex(t => t.Codigo)
                .IsUnique();

            // ------------------------------------------------------------
            // EVITAR PERMISSÕES DUPLICADAS
            // ------------------------------------------------------------

            modelBuilder.Entity<PermissaoAcesso>()
                .HasIndex(p => new
                {
                    p.UsuarioId,
                    p.TipoPermissaoId,
                    p.UnidadeInstitucionalId
                })
                .IsUnique();

            // ============================================================
            // SEED - TIPOS DE PERMISSÃO
            // ============================================================

            modelBuilder.Entity<TipoPermissao>().HasData(

                new TipoPermissao
                {
                    Id = 1,
                    Codigo = "CONSULTAR_EFECTIVO",
                    Nome = "Consultar Efectivo",
                    Descricao = "Permite consultar a informação do efectivo da unidade autorizada.",
                    Ativo = true
                },

                new TipoPermissao
                {
                    Id = 2,
                    Codigo = "CONSULTAR_POSTOS",
                    Nome = "Consultar Postos",
                    Descricao = "Permite consultar a situação dos postos da unidade autorizada.",
                    Ativo = true
                },

                new TipoPermissao
                {
                    Id = 3,
                    Codigo = "CONSULTAR_SEGURANCA",
                    Nome = "Consultar Segurança",
                    Descricao = "Permite consultar informação de situação de segurança autorizada.",
                    Ativo = true
                },

                new TipoPermissao
                {
                    Id = 4,
                    Codigo = "CONSULTAR_RELATORIOS",
                    Nome = "Consultar Relatórios",
                    Descricao = "Permite consultar relatórios disponibilizados à unidade autorizada.",
                    Ativo = true
                },

                new TipoPermissao
                {
                    Id = 5,
                    Codigo = "CONSULTAR_ESCALAS",
                    Nome = "Consultar Escalas",
                    Descricao = "Permite consultar escalas da unidade autorizada.",
                    Ativo = true
                }
            );
        }


    }
}
