using InventarioUPS.API.Models;

namespace InventarioUPS.API.Repositories
{
    public interface IUsuarioRepository
    {
        // USUARIOS
        Task<Usuario?> ObtenerPorNombreUsuarioAsync(
            string nombreUsuario);

        Task<Usuario?> ObtenerPorIdAsync(
            int id);

        Task<List<Usuario>> ObtenerTodosAsync();

        Task CrearAsync(
            Usuario usuario);

        Task ActualizarAsync(
            Usuario usuario);

        Task<bool> ExisteNombreUsuarioAsync(
            string nombreUsuario);

        Task<bool> ExisteNombreUsuarioExceptoIdAsync(
            string nombreUsuario,
            int id);

        Task<bool> ExisteCorreoAsync(
            string correo);

        Task<bool> ExisteCorreoExceptoIdAsync(
            string correo,
            int id);

        Task<int> ContarUsuariosActivosPorRolAsync(
            int rolId);

        // ROLES
        Task<List<Rol>> ObtenerRolesAsync();

        Task<Rol?> ObtenerRolPorIdAsync(
            int id);

        Task CrearRolAsync(
            Rol rol);

        Task ActualizarRolAsync(
            Rol rol);

        Task<bool> ExisteNombreRolAsync(
            string nombre);

        Task<bool> ExisteNombreRolExceptoIdAsync(
            string nombre,
            int id);
    }
}