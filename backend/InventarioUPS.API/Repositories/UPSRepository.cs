using InventarioUPS.API.Data;
using InventarioUPS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace InventarioUPS.API.Repositories
{
    public class UPSRepository : IUPSRepository
    {
        private readonly InventarioUPSDbContext _context;

        public UPSRepository(InventarioUPSDbContext context)
        {
            _context = context;
        }

        public async Task<List<UPS>> ObtenerTodasAsync()
        {
            return await _context.UPSs
                .Include(u => u.Agencia)
                .ToListAsync();
        }

        public async Task<UPS?> ObtenerPorIdAsync(int id)
        {
            return await _context.UPSs
                .Include(u => u.Agencia)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task CrearAsync(UPS ups)
        {
            _context.UPSs.Add(ups);

            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(UPS ups)
        {
            var upsExistente = await _context.UPSs
                .FindAsync(ups.Id);

            if (upsExistente == null)
                throw new Exception("El UPS no existe.");

            upsExistente.Codigo = ups.Codigo;
            upsExistente.Marca = ups.Marca;
            upsExistente.Modelo = ups.Modelo;
            upsExistente.NumeroSerie = ups.NumeroSerie;
            upsExistente.CapacidadVA = ups.CapacidadVA;
            upsExistente.AgenciaId = ups.AgenciaId;
            upsExistente.FechaInstalacion = ups.FechaInstalacion;
            upsExistente.Estado = ups.Estado;
            upsExistente.CantidadBaterias = ups.CantidadBaterias;
            upsExistente.FechaUltimoCambioBaterias =
                ups.FechaUltimoCambioBaterias;

            upsExistente.FechaModificacion = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task CambiarEstadoAsync(
            int id,
            string estado)
        {
            var ups = await _context.UPSs
                .FindAsync(id);

            if (ups == null)
                throw new Exception("El UPS no existe.");

            ups.Estado = estado;

            ups.FechaModificacion = DateTime.Now;

            await _context.SaveChangesAsync();
        }
    }
}