using InventarioUPS.API.Data;
using InventarioUPS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace InventarioUPS.API.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly InventarioUPSDbContext _context;

        public UsuarioRepository(InventarioUPSDbContext context)
        {
            _context = context;
        }

        public async Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario)
        {
            var nombreNormalizado = nombreUsuario.Trim().ToUpper();

            return await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u =>
                    u.NombreUsuario.ToUpper() == nombreNormalizado);
        }

        public async Task<Usuario?> ObtenerPorIdAsync(int id)
        {
            return await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<List<Usuario>> ObtenerTodosAsync()
        {
            return await _context.Usuarios
                .Include(u => u.Rol)
                .OrderBy(u => u.NombreUsuario)
                .ToListAsync();
        }

        public async Task CrearAsync(Usuario usuario)
        {
            await _context.Usuarios.AddAsync(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Usuario usuario)
        {
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario)
        {
            var nombreNormalizado = nombreUsuario.Trim().ToUpper();

            return await _context.Usuarios
                .AnyAsync(u =>
                    u.NombreUsuario.ToUpper() == nombreNormalizado);
        }

        public async Task<bool> ExisteNombreUsuarioExceptoIdAsync(
            string nombreUsuario,
            int usuarioId)
        {
            var nombreNormalizado = nombreUsuario.Trim().ToUpper();

            return await _context.Usuarios
                .AnyAsync(u =>
                    u.Id != usuarioId &&
                    u.NombreUsuario.ToUpper() == nombreNormalizado);
        }

        public async Task<bool> ExisteCorreoAsync(string correo)
        {
            var correoNormalizado = correo.Trim().ToUpper();

            return await _context.Usuarios
                .AnyAsync(u =>
                    u.Correo.ToUpper() == correoNormalizado);
        }

        public async Task<bool> ExisteCorreoExceptoIdAsync(
            string correo,
            int usuarioId)
        {
            var correoNormalizado = correo.Trim().ToUpper();

            return await _context.Usuarios
                .AnyAsync(u =>
                    u.Id != usuarioId &&
                    u.Correo.ToUpper() == correoNormalizado);
        }

        public async Task<int> ContarUsuariosActivosPorRolAsync(int rolId)
        {
            return await _context.Usuarios
                .CountAsync(u =>
                    u.RolId == rolId &&
                    u.Activo);
        }

        public async Task<List<Rol>> ObtenerRolesAsync()
        {
            return await _context.Roles
                .OrderBy(r => r.Nombre)
                .ToListAsync();
        }

        public async Task<Rol?> ObtenerRolPorIdAsync(int id)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task CrearRolAsync(Rol rol)
        {
            await _context.Roles.AddAsync(rol);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarRolAsync(Rol rol)
        {
            _context.Roles.Update(rol);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExisteNombreRolAsync(string nombre)
        {
            var nombreNormalizado = nombre.Trim().ToUpper();

            return await _context.Roles
                .AnyAsync(r =>
                    r.Nombre.ToUpper() == nombreNormalizado);
        }

        public async Task<bool> ExisteNombreRolExceptoIdAsync(
            string nombre,
            int rolId)
        {
            var nombreNormalizado = nombre.Trim().ToUpper();

            return await _context.Roles
                .AnyAsync(r =>
                    r.Id != rolId &&
                    r.Nombre.ToUpper() == nombreNormalizado);
        }
    }
}