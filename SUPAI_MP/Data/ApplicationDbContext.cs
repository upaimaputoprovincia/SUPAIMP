using Microsoft.EntityFrameworkCore;
using supai_mp.Models;
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
        }
    }
}
