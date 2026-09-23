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
                .Where(m => m.IncluyeCambioBaterias)
                .AsNoTracking()
                .Include(m => m.UPS)
                    .ThenInclude(u => u!.Agencia)
                .Include(m => m.UPS)
                    .ThenInclude(u => u!.Agencia!.Ciudad)
                .Include(m => m.EstadoMantenimiento)
                .OrderByDescending(
                    m => m.FechaMantenimiento)
                .ThenByDescending(
                    m => m.Id)
                .ToListAsync();
        }

        public async Task<Mantenimiento?> ObtenerPorIdAsync(
            int id)
        {
            return await _context.Mantenimientos
                .Where(m => m.IncluyeCambioBaterias)
                .Include(m => m.UPS)
                    .ThenInclude(u => u!.Agencia)
                .Include(m => m.UPS)
                    .ThenInclude(u => u!.Agencia!.Ciudad)
                .Include(m => m.EstadoMantenimiento)
                .FirstOrDefaultAsync(
                    m => m.Id == id);
        }

        public async Task<List<Mantenimiento>> ObtenerPorUPSAsync(
            int upsId)
        {
            return await _context.Mantenimientos
                .Where(m => m.IncluyeCambioBaterias)
                .AsNoTracking()
                .Include(m => m.UPS)
                    .ThenInclude(u => u!.Agencia)
                .Include(m => m.UPS)
                    .ThenInclude(u => u!.Agencia!.Ciudad)
                .Include(m => m.EstadoMantenimiento)
                .Where(m => m.UPSId == upsId)
                .OrderByDescending(
                    m => m.FechaMantenimiento)
                .ThenByDescending(
                    m => m.Id)
                .ToListAsync();
        }

        public async Task CrearAsync(
            Mantenimiento mantenimiento)
        {
            _context.Mantenimientos.Add(
                mantenimiento);

            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(
            Mantenimiento mantenimiento)
        {
            var existente =
                await _context.Mantenimientos
                    .FirstOrDefaultAsync(
                        m =>
                            m.Id ==
                            mantenimiento.Id);

            if (existente == null)
            {
                throw new InvalidOperationException(
                    "El cambio de baterías no existe.");
            }

            existente.UPSId =
                mantenimiento.UPSId;

            existente.EstadoMantenimientoId =
                mantenimiento.EstadoMantenimientoId;

            existente.FechaMantenimiento =
                mantenimiento.FechaMantenimiento;

            existente.Descripcion =
                mantenimiento.Descripcion;

            existente.TecnicoProveedor =
                mantenimiento.TecnicoProveedor;

            existente.Observaciones =
                mantenimiento.Observaciones;

            // CAMBIO DE BATERÍAS
            existente.IncluyeCambioBaterias =
                mantenimiento.IncluyeCambioBaterias;

            existente.CantidadBateriasReemplazadas =
                mantenimiento.CantidadBateriasReemplazadas;

            existente.AgenciaIdRegistro =
                mantenimiento.AgenciaIdRegistro;

            existente.AgenciaNombreRegistro =
                mantenimiento.AgenciaNombreRegistro;

            existente.EstadoUPSAnteriorId =
                mantenimiento.EstadoUPSAnteriorId;

            existente.FechaModificacion =
                DateTime.Now;

            await _context.SaveChangesAsync();
        }
    }
}
