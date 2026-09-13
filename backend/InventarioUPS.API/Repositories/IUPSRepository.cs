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

        Task<bool> ExisteCodigoAsync(string codigo);

        Task<bool> ExisteCodigoExceptoIdAsync(string codigo, int upsId);

        Task<bool> ExisteNumeroSerieAsync(string numeroSerie);

        Task<bool> ExisteNumeroSerieExceptoIdAsync(
            string numeroSerie,
            int upsId);

        Task<bool> ExisteAgenciaActivaAsync(int agenciaId);

        Task<bool> ExisteEstadoUPSActivoAsync(int estadoUPSId);
    }
}