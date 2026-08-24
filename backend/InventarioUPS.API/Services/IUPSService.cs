using InventarioUPS.API.Models;

namespace InventarioUPS.API.Services
{
    public interface IUPSService
    {
        Task<List<UPS>> ObtenerTodasAsync();

        Task<UPS?> ObtenerPorIdAsync(int id);

        Task CrearAsync(UPS ups);

        Task ActualizarAsync(UPS ups);

        Task CambiarEstadoAsync(int id, string estado);
    }
}