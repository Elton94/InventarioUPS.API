using InventarioUPS.API.Models;

namespace InventarioUPS.API.Repositories
{
    public interface IMantenimientoRepository
    {
        Task<List<Mantenimiento>> ObtenerTodosAsync();

        Task<Mantenimiento?> ObtenerPorIdAsync(int id);

        Task<List<Mantenimiento>> ObtenerPorUPSAsync(int upsId);

        Task CrearAsync(Mantenimiento mantenimiento);

        Task ActualizarAsync(Mantenimiento mantenimiento);
    }
}
