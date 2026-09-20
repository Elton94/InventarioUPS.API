using InventarioUPS.API.Data;
using InventarioUPS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace InventarioUPS.API.Repositories
{
    public class ConfiguracionPresupuestoRepository : IConfiguracionPresupuestoRepository
    {
        private readonly InventarioUPSDbContext _context;

        public ConfiguracionPresupuestoRepository(InventarioUPSDbContext context)
        {
            _context = context;
        }

        public Task<List<ConfiguracionPresupuestoBaterias>> ObtenerTodasAsync() =>
            _context.ConfiguracionPresupuestoBaterias
                .AsNoTracking()
                .OrderByDescending(x => x.Anio)
                .ToListAsync();

        public Task<ConfiguracionPresupuestoBaterias?> ObtenerPorIdAsync(int id) =>
            _context.ConfiguracionPresupuestoBaterias
                .FirstOrDefaultAsync(x => x.Id == id);

        public Task<ConfiguracionPresupuestoBaterias?> ObtenerPorAnioAsync(int anio) =>
            _context.ConfiguracionPresupuestoBaterias
                .FirstOrDefaultAsync(x => x.Anio == anio);

        public async Task CrearAsync(ConfiguracionPresupuestoBaterias configuracion)
        {
            await _context.ConfiguracionPresupuestoBaterias.AddAsync(configuracion);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(ConfiguracionPresupuestoBaterias configuracion)
        {
            _context.ConfiguracionPresupuestoBaterias.Update(configuracion);
            await _context.SaveChangesAsync();
        }
    }
}
