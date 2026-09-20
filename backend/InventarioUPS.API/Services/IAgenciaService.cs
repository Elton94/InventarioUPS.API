using InventarioUPS.API.Models;

namespace InventarioUPS.API.Services
{
    public interface IAgenciaService
    {
        Task<List<Agencia>> ObtenerTodasAsync();
        Task<Agencia?> ObtenerPorIdAsync(int id);
        Task CrearAsync(Agencia agencia);
        Task ActualizarAsync(Agencia agencia);
        Task EliminarAsync(int id);
        Task CambiarEstadoAsync(int id, bool activa);
    }
}
