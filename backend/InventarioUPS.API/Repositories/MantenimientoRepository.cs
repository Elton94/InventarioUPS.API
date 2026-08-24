using InventarioUPS.API.Data;
using InventarioUPS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace InventarioUPS.API.Repositories
{
    public class MantenimientoRepository : IMantenimientoRepository
    {
        private readonly InventarioUPSDbContext _context;

        public MantenimientoRepository(
            InventarioUPSDbContext context)
        {
            _context = context;
        }

        public async Task<List<Mantenimiento>> ObtenerTodosAsync()
        {
            return await _context.Mantenimientos
                .Include(m => m.UPS)
                .ThenInclude(u => u!.Agencia)
                .ToListAsync();
        }

        public async Task<Mantenimiento?> ObtenerPorIdAsync(int id)
        {
            return await _context.Mantenimientos
                .Include(m => m.UPS)
                .ThenInclude(u => u!.Agencia)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<List<Mantenimiento>> ObtenerPorUPSAsync(
            int upsId)
        {
            return await _context.Mantenimientos
                .Include(m => m.UPS)
                .Where(m => m.UPSId == upsId)
                .OrderByDescending(
                    m => m.FechaMantenimiento
                )
                .ToListAsync();
        }

        public async Task CrearAsync(
            Mantenimiento mantenimiento)
        {
            _context.Mantenimientos.Add(
                mantenimiento
            );

            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(
            Mantenimiento mantenimiento)
        {
            var existente =
                await _context.Mantenimientos
                    .FindAsync(mantenimiento.Id);

            if (existente == null)
                throw new Exception(
                    "El mantenimiento no existe."
                );

            existente.UPSId =
                mantenimiento.UPSId;

            existente.TipoMantenimiento =
                mantenimiento.TipoMantenimiento;

            existente.FechaMantenimiento =
                mantenimiento.FechaMantenimiento;

            existente.Descripcion =
                mantenimiento.Descripcion;

            existente.TecnicoProveedor =
                mantenimiento.TecnicoProveedor;

            existente.Costo =
                mantenimiento.Costo;

            existente.Estado =
                mantenimiento.Estado;

            existente.Observaciones =
                mantenimiento.Observaciones;

            existente.FechaModificacion =
                DateTime.Now;

            await _context.SaveChangesAsync();
        }
    }
}