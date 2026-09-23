using InventarioUPS.API.DTOs.Presupuesto;
using InventarioUPS.API.Models;
using InventarioUPS.API.Repositories;

namespace InventarioUPS.API.Services
{
    public class ConfiguracionPresupuestoService : IConfiguracionPresupuestoService
    {
        private readonly IConfiguracionPresupuestoRepository _repository;

        public ConfiguracionPresupuestoService(IConfiguracionPresupuestoRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ConfiguracionPresupuestoDto>> ObtenerTodasAsync() =>
            (await _repository.ObtenerTodasAsync()).Select(MapearDto).ToList();

        public async Task<ConfiguracionPresupuestoDto?> ObtenerPorIdAsync(int id)
        {
            var entidad = await _repository.ObtenerPorIdAsync(id);
            return entidad == null ? null : MapearDto(entidad);
        }

        public async Task<ConfiguracionPresupuestoDto?> ObtenerPorAnioAsync(int anio)
        {
            var entidad = await _repository.ObtenerPorAnioAsync(anio);
            return entidad == null ? null : MapearDto(entidad);
        }

        public async Task CrearAsync(CrearConfiguracionPresupuestoDto dto)
        {
            ValidarAnio(dto.Anio);
            ValidarCosto(dto.CostoUnitarioBateria);

            var existente = await _repository.ObtenerPorAnioAsync(dto.Anio);

            if (existente != null)
                throw new InvalidOperationException(
                    $"Ya existe una configuración para el año {dto.Anio}.");

            var configuracion = new ConfiguracionPresupuestoBaterias
            {
                Anio = dto.Anio,
                MesesReemplazoLocal = 0,
                MesesReemplazoForanea = 0,
                CostoUnitarioBateria = dto.CostoUnitarioBateria,
                Activo = true,
                FechaCreacion = DateTime.Now
            };

            await _repository.CrearAsync(configuracion);
        }

        public async Task ActualizarAsync(int id, ActualizarConfiguracionPresupuestoDto dto)
        {
            ValidarCosto(dto.CostoUnitarioBateria);

            var configuracion = await _repository.ObtenerPorIdAsync(id);

            if (configuracion == null)
                throw new KeyNotFoundException(
                    "La configuración de presupuesto no existe.");

            configuracion.CostoUnitarioBateria = dto.CostoUnitarioBateria;
            configuracion.Activo = dto.Activo;
            configuracion.FechaModificacion = DateTime.Now;

            await _repository.ActualizarAsync(configuracion);
        }

        private static void ValidarAnio(int anio)
        {
            if (anio < 2020 || anio > 2100)
                throw new ArgumentException("El año debe estar entre 2020 y 2100.");
        }

        private static void ValidarCosto(decimal costo)
        {
            if (costo < 0.01m || costo > 9999999999999999.99m || decimal.Round(costo, 2) != costo)
                throw new ArgumentException(
                    "El costo debe ser positivo, con máximo dos decimales y menor que 10,000,000,000,000,000.");
        }

        private static ConfiguracionPresupuestoDto MapearDto(
            ConfiguracionPresupuestoBaterias entidad) => new()
        {
            Id = entidad.Id,
            Anio = entidad.Anio,
            CostoUnitarioBateria = entidad.CostoUnitarioBateria,
            Activo = entidad.Activo
        };
    }
}
