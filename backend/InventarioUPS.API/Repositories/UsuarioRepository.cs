using InventarioUPS.API.Data;
using InventarioUPS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace InventarioUPS.API.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly InventarioUPSDbContext _context;

        public UsuarioRepository(
            InventarioUPSDbContext context)
        {
            _context = context;
        }

        public async Task<Usuario?> ObtenerPorNombreUsuarioAsync(
            string nombreUsuario)
        {
            return await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(
                    u => u.NombreUsuario == nombreUsuario);
        }

        public async Task<Usuario?> ObtenerPorIdAsync(
            int id)
        {
            return await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(
                    u => u.Id == id);
        }

        public async Task<List<Usuario>> ObtenerTodosAsync()
        {
            return await _context.Usuarios
                .Include(u => u.Rol)
                .ToListAsync();
        }

        public async Task CrearAsync(
            Usuario usuario)
        {
            await _context.Usuarios.AddAsync(
                usuario);

            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(
            Usuario usuario)
        {
            _context.Usuarios.Update(
                usuario);

            await _context.SaveChangesAsync();
        }
    }
}