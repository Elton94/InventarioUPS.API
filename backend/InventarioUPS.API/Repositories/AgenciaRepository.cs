using InventarioUPS.API.Data;
using InventarioUPS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace InventarioUPS.API.Repositories
{
    public class AgenciaRepository : IAgenciaRepository
    {
        private readonly InventarioUPSDbContext _context;

        public AgenciaRepository(InventarioUPSDbContext context)
        {
            _context = context;
        }

        public async Task<List<Agencia>> ObtenerTodasAsync()
        {
            return await _context.Agencias.ToListAsync();
        }

        public async Task<Agencia?> ObtenerPorIdAsync(int id)
        {
            return await _context.Agencias.FindAsync(id);
        }

        public async Task CrearAsync(Agencia agencia)
        {
            _context.Agencias.Add(agencia);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Agencia agencia)
        {
            _context.Agencias.Update(agencia);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(Agencia agencia)
        {
            _context.Agencias.Remove(agencia);
            await _context.SaveChangesAsync();
        }
    }
}