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
            var agenciaExistente = await _context.Agencias.FindAsync(agencia.Id);

            if (agenciaExistente == null)
                throw new Exception("La agencia no existe.");

            agenciaExistente.Codigo = agencia.Codigo;
            agenciaExistente.Nombre = agencia.Nombre;
            agenciaExistente.Departamento = agencia.Departamento;
            agenciaExistente.Ciudad = agencia.Ciudad;
            agenciaExistente.Direccion = agencia.Direccion;
            agenciaExistente.TipoAgencia = agencia.TipoAgencia;
            agenciaExistente.Activa = agencia.Activa;

            agenciaExistente.FechaModificacion = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(Agencia agencia)
        {
            _context.Agencias.Remove(agencia);
            await _context.SaveChangesAsync();
        }
    }
}