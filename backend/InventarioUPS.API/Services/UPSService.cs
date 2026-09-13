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
            if (ups == null)
                throw new ArgumentException(
                    "Los datos del UPS son obligatorios.");

            ups.Codigo = ups.Codigo?.Trim() ?? string.Empty;
            ups.Marca = ups.Marca?.Trim() ?? string.Empty;
            ups.Modelo = ups.Modelo?.Trim() ?? string.Empty;
            ups.NumeroSerie = ups.NumeroSerie?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(ups.Codigo))
                throw new ArgumentException(
                    "El código del UPS es obligatorio.");

            if (string.IsNullOrWhiteSpace(ups.Marca))
                throw new ArgumentException(
                    "La marca del UPS es obligatoria.");

            if (string.IsNullOrWhiteSpace(ups.Modelo))
                throw new ArgumentException(
                    "El modelo del UPS es obligatorio.");

            if (string.IsNullOrWhiteSpace(ups.NumeroSerie))
                throw new ArgumentException(
                    "El número de serie es obligatorio.");

            if (ups.CapacidadVA <= 0)
                throw new ArgumentException(
                    "La capacidad VA debe ser mayor que cero.");

            if (ups.AgenciaId <= 0)
                throw new ArgumentException(
                    "Debe seleccionar una agencia.");

            if (ups.EstadoUPSId <= 0)
                throw new ArgumentException(
                    "Debe seleccionar un estado de UPS.");

            if (ups.CantidadBaterias < 0)
                throw new ArgumentException(
                    "La cantidad de baterías no puede ser negativa.");

            // Fecha de instalación es obligatoria
            if (ups.FechaInstalacion.Date > DateTime.Now.Date)
            {
                throw new ArgumentException(
                    "La fecha de instalación no puede ser futura.");
            }

            // Fecha de cambio de baterías es opcional
            if (ups.FechaUltimoCambioBaterias.HasValue &&
                ups.FechaUltimoCambioBaterias.Value.Date >
                DateTime.Now.Date)
            {
                throw new ArgumentException(
                    "La fecha del último cambio de baterías no puede ser futura.");
            }

            if (ups.FechaUltimoCambioBaterias.HasValue &&
                ups.FechaUltimoCambioBaterias.Value.Date <
                ups.FechaInstalacion.Date)
            {
                throw new ArgumentException(
                    "La fecha del último cambio de baterías no puede ser anterior a la fecha de instalación.");
            }

            if (await _repository.ExisteCodigoAsync(ups.Codigo))
            {
                throw new InvalidOperationException(
                    "Ya existe un UPS con ese código.");
            }

            if (await _repository.ExisteNumeroSerieAsync(
                    ups.NumeroSerie))
            {
                throw new InvalidOperationException(
                    "Ya existe un UPS con ese número de serie.");
            }

            if (!await _repository.ExisteAgenciaActivaAsync(
                    ups.AgenciaId))
            {
                throw new InvalidOperationException(
                    "La agencia seleccionada no existe o está inactiva.");
            }

            if (!await _repository.ExisteEstadoUPSActivoAsync(
                    ups.EstadoUPSId))
            {
                throw new InvalidOperationException(
                    "El estado de UPS seleccionado no existe o está inactivo.");
            }

            await _repository.CrearAsync(ups);
        }

        public async Task ActualizarAsync(UPS ups)
        {
            if (ups == null)
                throw new ArgumentException(
                    "Los datos del UPS son obligatorios.");

            if (ups.Id <= 0)
                throw new ArgumentException(
                    "El ID del UPS no es válido.");

            ups.Codigo = ups.Codigo?.Trim() ?? string.Empty;
            ups.Marca = ups.Marca?.Trim() ?? string.Empty;
            ups.Modelo = ups.Modelo?.Trim() ?? string.Empty;
            ups.NumeroSerie = ups.NumeroSerie?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(ups.Codigo))
                throw new ArgumentException(
                    "El código del UPS es obligatorio.");

            if (string.IsNullOrWhiteSpace(ups.Marca))
                throw new ArgumentException(
                    "La marca del UPS es obligatoria.");

            if (string.IsNullOrWhiteSpace(ups.Modelo))
                throw new ArgumentException(
                    "El modelo del UPS es obligatorio.");

            if (string.IsNullOrWhiteSpace(ups.NumeroSerie))
                throw new ArgumentException(
                    "El número de serie es obligatorio.");

            if (ups.CapacidadVA <= 0)
                throw new ArgumentException(
                    "La capacidad VA debe ser mayor que cero.");

            if (ups.AgenciaId <= 0)
                throw new ArgumentException(
                    "Debe seleccionar una agencia.");

            if (ups.EstadoUPSId <= 0)
                throw new ArgumentException(
                    "Debe seleccionar un estado de UPS.");

            if (ups.CantidadBaterias < 0)
                throw new ArgumentException(
                    "La cantidad de baterías no puede ser negativa.");

            if (ups.FechaInstalacion.Date > DateTime.Now.Date)
            {
                throw new ArgumentException(
                    "La fecha de instalación no puede ser futura.");
            }

            if (ups.FechaUltimoCambioBaterias.HasValue &&
                ups.FechaUltimoCambioBaterias.Value.Date >
                DateTime.Now.Date)
            {
                throw new ArgumentException(
                    "La fecha del último cambio de baterías no puede ser futura.");
            }

            if (ups.FechaUltimoCambioBaterias.HasValue &&
                ups.FechaUltimoCambioBaterias.Value.Date <
                ups.FechaInstalacion.Date)
            {
                throw new ArgumentException(
                    "La fecha del último cambio de baterías no puede ser anterior a la fecha de instalación.");
            }

            var existente =
                await _repository.ObtenerPorIdAsync(ups.Id);

            if (existente == null)
                throw new InvalidOperationException(
                    "El UPS no existe.");

            if (await _repository.ExisteCodigoExceptoIdAsync(
                    ups.Codigo,
                    ups.Id))
            {
                throw new InvalidOperationException(
                    "Ya existe otro UPS con ese código.");
            }

            if (await _repository.ExisteNumeroSerieExceptoIdAsync(
                    ups.NumeroSerie,
                    ups.Id))
            {
                throw new InvalidOperationException(
                    "Ya existe otro UPS con ese número de serie.");
            }

            if (!await _repository.ExisteAgenciaActivaAsync(
                    ups.AgenciaId))
            {
                throw new InvalidOperationException(
                    "La agencia seleccionada no existe o está inactiva.");
            }

            if (!await _repository.ExisteEstadoUPSActivoAsync(
                    ups.EstadoUPSId))
            {
                throw new InvalidOperationException(
                    "El estado de UPS seleccionado no existe o está inactivo.");
            }

            await _repository.ActualizarAsync(ups);
        }

        public async Task CambiarEstadoAsync(
            int id,
            string estado)
        {
            if (string.IsNullOrWhiteSpace(estado))
            {
                throw new ArgumentException(
                    "El estado es obligatorio.");
            }

            var ups = await _repository.ObtenerPorIdAsync(id);

            if (ups == null)
            {
                throw new InvalidOperationException(
                    "El UPS no existe.");
            }

            await _repository.CambiarEstadoAsync(
                id,
                estado.Trim());
        }
    }
}