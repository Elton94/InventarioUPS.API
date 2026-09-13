using InventarioUPS.API.Data;
using InventarioUPS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace InventarioUPS.API.Repositories
{
    public class ConfiguracionPresupuestoRepository
        : IConfiguracionPresupuestoRepository
    {
        private readonly InventarioUPSDbContext _context;

        public ConfiguracionPresupuestoRepository(
            InventarioUPSDbContext context)
        {
            _context = context;
        }


        public async Task<List<ConfiguracionPresupuestoBaterias>>
            ObtenerTodasAsync()
        {
            return await _context.ConfiguracionPresupuestoBaterias
                .AsNoTracking()
                .OrderByDescending(x => x.Anio)
                .ToListAsync();
        }


        public async Task<ConfiguracionPresupuestoBaterias?>
            ObtenerPorIdAsync(int id)
        {
            return await _context.ConfiguracionPresupuestoBaterias
                .FirstOrDefaultAsync(x => x.Id == id);
        }


        public async Task<ConfiguracionPresupuestoBaterias?>
            ObtenerPorAnioAsync(int anio)
        {
            return await _context.ConfiguracionPresupuestoBaterias
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Anio == anio && x.Activo);
        }


        public async Task CrearAsync(
            ConfiguracionPresupuestoBaterias configuracion)
        {
            await _context.ConfiguracionPresupuestoBaterias
                .AddAsync(configuracion);

            await _context.SaveChangesAsync();
        }


        public async Task ActualizarAsync(
            ConfiguracionPresupuestoBaterias configuracion)
        {
            _context.ConfiguracionPresupuestoBaterias
                .Update(configuracion);

            await _context.SaveChangesAsync();
        }
    }
}