using InventarioUPS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace InventarioUPS.API.Data
{
    public class InventarioUPSDbContext : DbContext
    {
        public InventarioUPSDbContext(
            DbContextOptions<InventarioUPSDbContext> options)
            : base(options)
        {
        }

        public DbSet<Agencia> Agencias { get; set; }

        public DbSet<UPS> UPSs { get; set; }

        public DbSet<Mantenimiento> Mantenimientos { get; set; }

        public DbSet<Rol> Roles { get; set; }

        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UPS>()
                .HasIndex(u => u.NumeroSerie)
                .IsUnique();

            modelBuilder.Entity<UPS>()
                .HasOne(u => u.Agencia)
                .WithMany(a => a.UPSs)
                .HasForeignKey(u => u.AgenciaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}