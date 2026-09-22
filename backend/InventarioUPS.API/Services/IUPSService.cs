using InventarioUPS.API.Models;

namespace InventarioUPS.API.Services
{
    public interface IUPSService
    {
        Task<List<UPS>> ObtenerTodasAsync();

        Task<UPS?> ObtenerPorInventarioAsync(string numeroInventario);

        Task CrearAsync(UPS ups);

        Task ActualizarAsync(UPS ups);

        Task CambiarEstadoAsync(string numeroInventario, string estado);
    }
}
