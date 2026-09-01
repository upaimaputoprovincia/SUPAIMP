using Microsoft.EntityFrameworkCore;
using supai_mp.Models;

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
    }
}
