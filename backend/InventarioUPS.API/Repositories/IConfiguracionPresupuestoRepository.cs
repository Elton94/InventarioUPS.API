using InventarioUPS.API.Models;

namespace InventarioUPS.API.Repositories
{
    public interface IConfiguracionPresupuestoRepository
    {
        Task<List<ConfiguracionPresupuestoBaterias>> ObtenerTodasAsync();

        Task<ConfiguracionPresupuestoBaterias?> ObtenerPorIdAsync(int id);

        Task<ConfiguracionPresupuestoBaterias?> ObtenerPorAnioAsync(int anio);

        Task<PoliticaReemplazoBaterias?> ObtenerPoliticaAsync();

        Task CrearAsync(
            ConfiguracionPresupuestoBaterias configuracion);

        Task ActualizarAsync(
            ConfiguracionPresupuestoBaterias configuracion);
    }
}