using InventarioUPS.API.Data;
using InventarioUPS.API.Models;

using Microsoft.EntityFrameworkCore;

namespace InventarioUPS.API.Repositories
{
    public class AgenciaRepository : IAgenciaRepository
    {
        private readonly InventarioUPSDbContext _context;

        public AgenciaRepository(
            InventarioUPSDbContext context)
        {
            _context = context;
        }

        public async Task<List<Agencia>> ObtenerTodasAsync()
        {
            return await _context.Agencias
                .Include(a => a.Ciudad)
                    .ThenInclude(c => c!.Departamento)
                .Include(a => a.TipoAgencia)
                .OrderBy(a => a.Codigo)
                .ToListAsync();
        }

        public async Task<Agencia?> ObtenerPorIdAsync(
            int id)
        {
            return await _context.Agencias
                .Include(a => a.Ciudad)
                    .ThenInclude(c => c!.Departamento)
                .Include(a => a.TipoAgencia)
                .FirstOrDefaultAsync(
                    a => a.Id == id);
        }

        public async Task CrearAsync(
            Agencia agencia)
        {
            await _context.Agencias.AddAsync(
                agencia);

            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(
            Agencia agencia)
        {
            var agenciaExistente =
                await _context.Agencias
                    .FirstOrDefaultAsync(
                        a => a.Id == agencia.Id);

            if (agenciaExistente == null)
            {
                throw new InvalidOperationException(
                    "La agencia no existe.");
            }

            agenciaExistente.Codigo =
                agencia.Codigo;

            agenciaExistente.Nombre =
                agencia.Nombre;

            agenciaExistente.CiudadId =
                agencia.CiudadId;

            agenciaExistente.Direccion =
                agencia.Direccion;

            agenciaExistente.TipoAgenciaId =
                agencia.TipoAgenciaId;

            agenciaExistente.Activa =
                agencia.Activa;

            agenciaExistente.FechaModificacion =
                DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(
            Agencia agencia)
        {
            var agenciaExistente =
                await _context.Agencias
                    .FirstOrDefaultAsync(
                        a => a.Id == agencia.Id);

            if (agenciaExistente == null)
            {
                throw new InvalidOperationException(
                    "La agencia no existe.");
            }

            agenciaExistente.Activa = false;

            agenciaExistente.FechaModificacion =
                DateTime.Now;

            await _context.SaveChangesAsync();
        }
    }
}