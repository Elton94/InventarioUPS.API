using InventarioUPS.API.DTOs.Presupuesto;
using InventarioUPS.API.Models;
using InventarioUPS.API.Repositories;

namespace InventarioUPS.API.Services
{
    public class ConfiguracionPresupuestoService
        : IConfiguracionPresupuestoService
    {
        private readonly IConfiguracionPresupuestoRepository _repository;

        public ConfiguracionPresupuestoService(
            IConfiguracionPresupuestoRepository repository)
        {
            _repository = repository;
        }


        public async Task<List<ConfiguracionPresupuestoDto>>
            ObtenerTodasAsync()
        {
            var configuraciones =
                await _repository.ObtenerTodasAsync();

            return configuraciones
                .Select(MapearDto)
                .ToList();
        }


        public async Task<ConfiguracionPresupuestoDto?>
            ObtenerPorIdAsync(int id)
        {
            var configuracion =
                await _repository.ObtenerPorIdAsync(id);

            if (configuracion == null)
                return null;

            return MapearDto(configuracion);
        }


        public async Task<ConfiguracionPresupuestoDto?>
            ObtenerPorAnioAsync(int anio)
        {
            var configuracion =
                await _repository.ObtenerPorAnioAsync(anio);

            if (configuracion == null)
                return null;

            return MapearDto(configuracion);
        }


        public async Task CrearAsync(
            CrearConfiguracionPresupuestoDto dto)
        {
            if (dto.Anio < 2020 || dto.Anio > 2100)
                throw new ArgumentException(
                    "El año debe estar entre 2020 y 2100.");

            if (dto.MesesReemplazoLocal <= 0)
                throw new ArgumentException(
                    "El ciclo de reemplazo local debe ser mayor que cero.");

            if (dto.MesesReemplazoForanea <= 0)
                throw new ArgumentException(
                    "El ciclo de reemplazo foráneo debe ser mayor que cero.");

            if (dto.CostoUnitarioBateria < 0)
                throw new ArgumentException(
                    "El costo unitario no puede ser negativo.");

            var existente =
                await _repository.ObtenerPorAnioAsync(dto.Anio);

            if (existente != null)
                throw new InvalidOperationException(
                    $"Ya existe una configuración activa para el año {dto.Anio}.");

            var configuracion =
                new ConfiguracionPresupuestoBaterias
                {
                    Anio = dto.Anio,
                    MesesReemplazoLocal =
                        dto.MesesReemplazoLocal,
                    MesesReemplazoForanea =
                        dto.MesesReemplazoForanea,
                    CostoUnitarioBateria =
                        dto.CostoUnitarioBateria,
                    Activo = true,
                    FechaCreacion = DateTime.Now
                };

            await _repository.CrearAsync(configuracion);
        }


        public async Task ActualizarAsync(
            int id,
            ActualizarConfiguracionPresupuestoDto dto)
        {
            if (dto.MesesReemplazoLocal <= 0)
                throw new ArgumentException(
                    "El ciclo de reemplazo local debe ser mayor que cero.");

            if (dto.MesesReemplazoForanea <= 0)
                throw new ArgumentException(
                    "El ciclo de reemplazo foráneo debe ser mayor que cero.");

            if (dto.CostoUnitarioBateria < 0)
                throw new ArgumentException(
                    "El costo unitario no puede ser negativo.");

            var configuracion =
                await _repository.ObtenerPorIdAsync(id);

            if (configuracion == null)
                throw new KeyNotFoundException(
                    "La configuración de presupuesto no existe.");

            configuracion.MesesReemplazoLocal =
                dto.MesesReemplazoLocal;

            configuracion.MesesReemplazoForanea =
                dto.MesesReemplazoForanea;

            configuracion.CostoUnitarioBateria =
                dto.CostoUnitarioBateria;

            configuracion.Activo = dto.Activo;

            configuracion.FechaModificacion =
                DateTime.Now;

            await _repository.ActualizarAsync(configuracion);
        }


        private static ConfiguracionPresupuestoDto
            MapearDto(
                ConfiguracionPresupuestoBaterias entidad)
        {
            return new ConfiguracionPresupuestoDto
            {
                Id = entidad.Id,
                Anio = entidad.Anio,
                MesesReemplazoLocal =
                    entidad.MesesReemplazoLocal,
                MesesReemplazoForanea =
                    entidad.MesesReemplazoForanea,
                CostoUnitarioBateria =
                    entidad.CostoUnitarioBateria,
                Activo = entidad.Activo
            };
        }
    }
}