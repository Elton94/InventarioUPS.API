using InventarioUPS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace InventarioUPS.API.Data
{
    public class InventarioUPSDbContext : DbContext
    {
        public InventarioUPSDbContext(DbContextOptions<InventarioUPSDbContext> options)
            : base(options)
        {
        }

        public DbSet<Agencia> Agencias { get; set; }
    }
}