using InventarioUPS.API.Models;

namespace InventarioUPS.API.Repositories
{
    public interface IUPSRepository
    {
        Task<List<UPS>> ObtenerTodasAsync();

        Task<UPS?> ObtenerPorIdAsync(int id);

        Task CrearAsync(UPS ups);

        Task ActualizarAsync(UPS ups);

        Task CambiarEstadoAsync(int id, string estado);
    }
}