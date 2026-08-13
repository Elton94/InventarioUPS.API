using InventarioUPS.API.Models;

namespace InventarioUPS.API.Repositories
{
    public interface IAgenciaRepository
    {
        Task<List<Agencia>> ObtenerTodasAsync();

        Task<Agencia?> ObtenerPorIdAsync(int id);

        Task CrearAsync(Agencia agencia);

        Task ActualizarAsync(Agencia agencia);

        Task EliminarAsync(Agencia agencia);
    }
}