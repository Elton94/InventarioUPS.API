using InventarioUPS.API.Data;
using InventarioUPS.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

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

        public Task<PoliticaReemplazoBaterias?> ObtenerPoliticaAsync() =>
            _context.PoliticaReemplazoBaterias.AsNoTracking().OrderByDescending(x => x.Id).FirstOrDefaultAsync();

        public async Task CrearAsync(ConfiguracionPresupuestoBaterias configuracion)
        {
            await _context.ConfiguracionPresupuestoBaterias.AddAsync(configuracion);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
            {
                throw new InvalidOperationException($"Ya existe una configuración para el año {configuracion.Anio}.", ex);
            }
        }

        public async Task ActualizarAsync(ConfiguracionPresupuestoBaterias configuracion)
        {
            _context.ConfiguracionPresupuestoBaterias.Update(configuracion);
            await _context.SaveChangesAsync();
        }
    }
}
