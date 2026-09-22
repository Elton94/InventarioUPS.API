using InventarioUPS.API.DTOs;
using InventarioUPS.API.Models;

namespace InventarioUPS.API.Services
{
    public interface IMantenimientoService
    {
        Task<List<MantenimientoDTO>> ObtenerTodosAsync();

        Task<MantenimientoDTO?> ObtenerPorIdAsync(int id);

        Task<List<MantenimientoDTO>> ObtenerPorUPSAsync(string numeroInventarioUPS);

        Task CrearAsync(Mantenimiento mantenimiento);

        Task ActualizarAsync(Mantenimiento mantenimiento);
    }
}
