using InventarioUPS.API.Models;
using InventarioUPS.API.Repositories;

namespace InventarioUPS.API.Services
{
    public class UPSService : IUPSService
    {
        private readonly IUPSRepository _repository;

        public UPSService(IUPSRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<UPS>> ObtenerTodasAsync()
        {
            return await _repository.ObtenerTodasAsync();
        }

        public async Task<UPS?> ObtenerPorIdAsync(int id)
        {
            return await _repository.ObtenerPorIdAsync(id);
        }

        public async Task CrearAsync(UPS ups)
        {
            var existentes = await _repository.ObtenerTodasAsync();

            var codigoExiste = existentes
                .Any(u =>
                    u.Codigo.Equals(
                        ups.Codigo,
                        StringComparison.OrdinalIgnoreCase
                    )
                );

            if (codigoExiste)
                throw new Exception(
                    "Ya existe un UPS con ese código."
                );

            var serieExiste = existentes
                .Any(u =>
                    u.NumeroSerie.Equals(
                        ups.NumeroSerie,
                        StringComparison.OrdinalIgnoreCase
                    )
                );

            if (serieExiste)
                throw new Exception(
                    "Ya existe un UPS con ese número de serie."
                );

            await _repository.CrearAsync(ups);
        }

        public async Task ActualizarAsync(UPS ups)
        {
            var existentes = await _repository.ObtenerTodasAsync();

            var serieExiste = existentes
                .Any(u =>
                    u.Id != ups.Id &&
                    u.NumeroSerie.Equals(
                        ups.NumeroSerie,
                        StringComparison.OrdinalIgnoreCase
                    )
                );

            if (serieExiste)
                throw new Exception(
                    "Ya existe otro UPS con ese número de serie."
                );

            await _repository.ActualizarAsync(ups);
        }

        public async Task CambiarEstadoAsync(
            int id,
            string estado)
        {
            var estadosPermitidos = new[]
            {
                "Operativo",
                "En mantenimiento",
                "Fuera de servicio"
            };

            if (!estadosPermitidos.Contains(estado))
            {
                throw new Exception(
                    "El estado proporcionado no es válido."
                );
            }

            var ups = await _repository.ObtenerPorIdAsync(id);

            if (ups == null)
            {
                throw new Exception(
                    "El UPS no existe."
                );
            }

            await _repository.CambiarEstadoAsync(
                id,
                estado
            );
        }
    }
}