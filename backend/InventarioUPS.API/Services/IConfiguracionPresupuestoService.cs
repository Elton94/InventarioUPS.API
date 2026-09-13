using InventarioUPS.API.DTOs.Presupuesto;

namespace InventarioUPS.API.Services
{
    public interface IConfiguracionPresupuestoService
    {
        Task<List<ConfiguracionPresupuestoDto>>
            ObtenerTodasAsync();

        Task<ConfiguracionPresupuestoDto?>
            ObtenerPorIdAsync(int id);

        Task<ConfiguracionPresupuestoDto?>
            ObtenerPorAnioAsync(int anio);

        Task CrearAsync(
            CrearConfiguracionPresupuestoDto dto);

        Task ActualizarAsync(
            int id,
            ActualizarConfiguracionPresupuestoDto dto);
    }
}