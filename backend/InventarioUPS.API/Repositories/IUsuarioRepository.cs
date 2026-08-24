using InventarioUPS.API.Models;

namespace InventarioUPS.API.Repositories
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> ObtenerPorNombreUsuarioAsync(
            string nombreUsuario);

        Task<Usuario?> ObtenerPorIdAsync(
            int id);

        Task<List<Usuario>> ObtenerTodosAsync();

        Task CrearAsync(
            Usuario usuario);

        Task ActualizarAsync(
            Usuario usuario);
    }
}