using InventarioUPS.API.DTOs;
using InventarioUPS.API.Models;
using InventarioUPS.API.Repositories;

namespace InventarioUPS.API.Services
{
    public class MantenimientoService : IMantenimientoService
    {
        private readonly IMantenimientoRepository _repository;
        private readonly IUPSRepository _upsRepository;

        public MantenimientoService(
            IMantenimientoRepository repository,
            IUPSRepository upsRepository)
        {
            _repository = repository;
            _upsRepository = upsRepository;
        }

        public async Task<List<MantenimientoDTO>> ObtenerTodosAsync()
        {
            var mantenimientos =
                await _repository.ObtenerTodosAsync();

            return mantenimientos.Select(m => new MantenimientoDTO
            {
                Id = m.Id,
                UPSId = m.UPSId,
                TipoMantenimiento = m.TipoMantenimiento,
                FechaMantenimiento = m.FechaMantenimiento,
                Descripcion = m.Descripcion,
                TecnicoProveedor = m.TecnicoProveedor,
                Costo = m.Costo,
                Estado = m.Estado,
                Observaciones = m.Observaciones,

                CodigoUPS = m.UPS?.Codigo,
                NumeroSerieUPS = m.UPS?.NumeroSerie,
                ModeloUPS = m.UPS?.Modelo,
                AgenciaNombre = m.UPS?.Agencia?.Nombre

            }).ToList();
        }

        public async Task<MantenimientoDTO?> ObtenerPorIdAsync(int id)
        {
            var mantenimiento =
                await _repository.ObtenerPorIdAsync(id);

            if (mantenimiento == null)
                return null;

            return ConvertirDTO(mantenimiento);
        }

        public async Task<List<MantenimientoDTO>> ObtenerPorUPSAsync(
            int upsId)
        {
            var mantenimientos =
                await _repository.ObtenerPorUPSAsync(upsId);

            return mantenimientos.Select(m => new MantenimientoDTO
            {
                Id = m.Id,
                UPSId = m.UPSId,
                TipoMantenimiento = m.TipoMantenimiento,
                FechaMantenimiento = m.FechaMantenimiento,
                Descripcion = m.Descripcion,
                TecnicoProveedor = m.TecnicoProveedor,
                Costo = m.Costo,
                Estado = m.Estado,
                Observaciones = m.Observaciones,

                CodigoUPS = m.UPS?.Codigo,
                NumeroSerieUPS = m.UPS?.NumeroSerie,
                ModeloUPS = m.UPS?.Modelo,
                AgenciaNombre = m.UPS?.Agencia?.Nombre

            }).ToList();
        }

        public async Task CrearAsync(
            Mantenimiento mantenimiento)
        {
            var ups =
                await _upsRepository
                    .ObtenerPorIdAsync(mantenimiento.UPSId);

            if (ups == null)
            {
                throw new Exception(
                    "El UPS seleccionado no existe."
                );
            }

            if (mantenimiento.Costo < 0)
            {
                throw new Exception(
                    "El costo no puede ser negativo."
                );
            }

            await _repository.CrearAsync(
                mantenimiento
            );
        }

        public async Task ActualizarAsync(
            Mantenimiento mantenimiento)
        {
            var existente =
                await _repository.ObtenerPorIdAsync(
                    mantenimiento.Id
                );

            if (existente == null)
            {
                throw new Exception(
                    "El mantenimiento no existe."
                );
            }

            var ups =
                await _upsRepository
                    .ObtenerPorIdAsync(
                        mantenimiento.UPSId
                    );

            if (ups == null)
            {
                throw new Exception(
                    "El UPS seleccionado no existe."
                );
            }

            if (mantenimiento.Costo < 0)
            {
                throw new Exception(
                    "El costo no puede ser negativo."
                );
            }

            await _repository.ActualizarAsync(
                mantenimiento
            );
        }

        private MantenimientoDTO ConvertirDTO(
            Mantenimiento mantenimiento)
        {
            return new MantenimientoDTO
            {
                Id = mantenimiento.Id,
                UPSId = mantenimiento.UPSId,
                TipoMantenimiento =
                    mantenimiento.TipoMantenimiento,
                FechaMantenimiento =
                    mantenimiento.FechaMantenimiento,
                Descripcion =
                    mantenimiento.Descripcion,
                TecnicoProveedor =
                    mantenimiento.TecnicoProveedor,
                Costo =
                    mantenimiento.Costo,
                Estado =
                    mantenimiento.Estado,
                Observaciones =
                    mantenimiento.Observaciones,

                CodigoUPS =
                    mantenimiento.UPS?.Codigo,

                NumeroSerieUPS =
                    mantenimiento.UPS?.NumeroSerie,

                ModeloUPS =
                    mantenimiento.UPS?.Modelo,

                AgenciaNombre =
                    mantenimiento.UPS?.Agencia?.Nombre
            };
        }
    }
}    