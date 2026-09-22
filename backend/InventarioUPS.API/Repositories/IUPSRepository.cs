using InventarioUPS.API.Models;

namespace InventarioUPS.API.Repositories
{
    public interface IUPSRepository
    {
        Task<List<UPS>> ObtenerTodasAsync();

        Task<UPS?> ObtenerPorInventarioAsync(string numeroInventario);

        Task CrearAsync(UPS ups);

        Task ActualizarAsync(UPS ups);

        Task CambiarEstadoAsync(string numeroInventario, string estado);

        Task<bool> ExisteNumeroInventarioAsync(string numeroInventario);

        Task<bool> ExisteNumeroSerieAsync(string numeroSerie);

        Task<bool> ExisteNumeroSerieExceptoInventarioAsync(
            string numeroSerie,
            string numeroInventarioUPS);

        Task<bool> ExisteAgenciaActivaAsync(int agenciaId);

        Task<bool> ExisteEstadoUPSActivoAsync(int estadoUPSId);
    }
}
