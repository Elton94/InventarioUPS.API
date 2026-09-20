using InventarioUPS.API.Data;
using InventarioUPS.API.Models;
using InventarioUPS.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace InventarioUPS.API.Services
{
    public class UPSService : IUPSService
    {
        private readonly IUPSRepository _repository;
        private readonly InventarioUPSDbContext _context;

        public UPSService(IUPSRepository repository, InventarioUPSDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        public Task<List<UPS>> ObtenerTodasAsync() => _repository.ObtenerTodasAsync();
        public Task<UPS?> ObtenerPorIdAsync(int id) => _repository.ObtenerPorIdAsync(id);

        public async Task CrearAsync(UPS ups)
        {
            ValidarYLimpia(ups);

            if (await _repository.ExisteCodigoAsync(ups.Codigo))
                throw new InvalidOperationException("Ya existe un UPS con ese código.");

            if (await _repository.ExisteNumeroSerieAsync(ups.NumeroSerie))
                throw new InvalidOperationException("Ya existe un UPS con ese número de serie.");

            if (!await _repository.ExisteAgenciaActivaAsync(ups.AgenciaId))
                throw new InvalidOperationException("La agencia seleccionada no existe o está inactiva.");

            if (!await _repository.ExisteEstadoUPSActivoAsync(ups.EstadoUPSId))
                throw new InvalidOperationException("El estado de UPS seleccionado no existe o está inactivo.");

            ups.FechaUltimoCambioBateriasBase = ups.FechaUltimoCambioBaterias;
            await _repository.CrearAsync(ups);
        }

        public async Task ActualizarAsync(UPS ups)
        {
            if (ups == null || ups.Id <= 0)
                throw new ArgumentException("El ID del UPS no es válido.");

            ValidarYLimpia(ups);

            var existente = await _repository.ObtenerPorIdAsync(ups.Id);

            if (existente == null)
                throw new InvalidOperationException("El UPS no existe.");

            var estadoDestino = await _context.EstadosUPS.AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == ups.EstadoUPSId && e.Activo);

            if (estadoDestino == null)
                throw new InvalidOperationException("El estado de UPS seleccionado no existe o está inactivo.");

            if (EsEstado(existente.EstadoUPS?.Nombre, "De baja") &&
                !EsEstado(estadoDestino.Nombre, "De baja"))
                throw new InvalidOperationException(
                    "Un UPS dado De baja no puede volver a un estado operativo.");

            if (EsEstado(estadoDestino.Nombre, "De baja") &&
                !EsEstado(existente.EstadoUPS?.Nombre, "De baja"))
            {
                await ValidarSinMantenimientoPendienteAsync(ups.Id);
            }

            if (await _repository.ExisteCodigoExceptoIdAsync(ups.Codigo, ups.Id))
                throw new InvalidOperationException("Ya existe otro UPS con ese código.");

            if (await _repository.ExisteNumeroSerieExceptoIdAsync(ups.NumeroSerie, ups.Id))
                throw new InvalidOperationException("Ya existe otro UPS con ese número de serie.");

            if (!await _repository.ExisteAgenciaActivaAsync(ups.AgenciaId))
                throw new InvalidOperationException("La agencia seleccionada no existe o está inactiva.");

            var tieneCambiosCompletados = await _context.Mantenimientos.AsNoTracking()
                .AnyAsync(m =>
                    m.UPSId == ups.Id &&
                    m.IncluyeCambioBaterias &&
                    m.EstadoMantenimiento != null &&
                    m.EstadoMantenimiento.Nombre == "Completado");

            if (!tieneCambiosCompletados)
            {
                ups.FechaUltimoCambioBateriasBase = ups.FechaUltimoCambioBaterias;
            }
            else
            {
                ups.FechaUltimoCambioBateriasBase =
                    existente.FechaUltimoCambioBateriasBase ??
                    existente.FechaUltimoCambioBaterias;

                var ultima = await _context.Mantenimientos.AsNoTracking()
                    .Where(m =>
                        m.UPSId == ups.Id &&
                        m.IncluyeCambioBaterias &&
                        m.EstadoMantenimiento != null &&
                        m.EstadoMantenimiento.Nombre == "Completado")
                    .Select(m => (DateTime?)m.FechaMantenimiento)
                    .MaxAsync();

                ups.FechaUltimoCambioBaterias =
                    MaxFecha(ups.FechaUltimoCambioBateriasBase, ultima);
            }

            await _repository.ActualizarAsync(ups);
        }

        public async Task CambiarEstadoAsync(int id, string estado)
        {
            if (id <= 0)
                throw new ArgumentException("El identificador del UPS no es válido.");

            if (string.IsNullOrWhiteSpace(estado))
                throw new ArgumentException("El estado es obligatorio.");

            var ups = await _repository.ObtenerPorIdAsync(id);

            if (ups == null)
                throw new InvalidOperationException("El UPS no existe.");

            var destino = await _context.EstadosUPS.AsNoTracking()
                .FirstOrDefaultAsync(e =>
                    e.Activo &&
                    e.Nombre.ToUpper() == estado.Trim().ToUpper());

            if (destino == null)
                throw new ArgumentException("El estado de UPS indicado no existe o está inactivo.");

            if (EsEstado(ups.EstadoUPS?.Nombre, "De baja") &&
                !EsEstado(destino.Nombre, "De baja"))
                throw new InvalidOperationException(
                    "Un UPS dado De baja no puede volver a un estado operativo.");

            if (EsEstado(destino.Nombre, "De baja"))
            {
                await ValidarSinMantenimientoPendienteAsync(id);
            }

            await _repository.CambiarEstadoAsync(id, destino.Nombre);
        }

        private async Task ValidarSinMantenimientoPendienteAsync(int upsId)
        {
            var pendiente = await _context.Mantenimientos
                .AsNoTracking()
                .AnyAsync(m =>
                    m.UPSId == upsId &&
                    m.EstadoMantenimiento != null &&
                    (m.EstadoMantenimiento.Nombre == "Programado" ||
                     m.EstadoMantenimiento.Nombre == "En proceso"));

            if (pendiente)
                throw new InvalidOperationException(
                    "No se puede dar de baja el UPS mientras tenga mantenimientos Programados o En proceso.");
        }

        private static void ValidarYLimpia(UPS ups)
        {
            if (ups == null)
                throw new ArgumentException("Los datos del UPS son obligatorios.");

            ups.Codigo = ups.Codigo?.Trim() ?? string.Empty;
            ups.Marca = ups.Marca?.Trim() ?? string.Empty;
            ups.Modelo = ups.Modelo?.Trim() ?? string.Empty;
            ups.NumeroSerie = ups.NumeroSerie?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(ups.Codigo))
                throw new ArgumentException("El código del UPS es obligatorio.");
            if (string.IsNullOrWhiteSpace(ups.Marca))
                throw new ArgumentException("La marca del UPS es obligatoria.");
            if (string.IsNullOrWhiteSpace(ups.Modelo))
                throw new ArgumentException("El modelo del UPS es obligatorio.");
            if (string.IsNullOrWhiteSpace(ups.NumeroSerie))
                throw new ArgumentException("El número de serie es obligatorio.");
            if (ups.CapacidadVA <= 0)
                throw new ArgumentException("La capacidad VA debe ser mayor que cero.");
            if (ups.AgenciaId <= 0)
                throw new ArgumentException("Debe seleccionar una agencia.");
            if (ups.EstadoUPSId <= 0)
                throw new ArgumentException("Debe seleccionar un estado de UPS.");
            if (ups.CantidadBaterias <= 0)
                throw new ArgumentException("La cantidad de baterías debe ser mayor que cero.");
            if (ups.FechaInstalacion == default)
                throw new ArgumentException("La fecha de instalación es obligatoria.");
            if (ups.FechaInstalacion.Date > DateTime.Now.Date)
                throw new ArgumentException("La fecha de instalación no puede ser futura.");
            if (ups.FechaUltimoCambioBaterias.HasValue &&
                ups.FechaUltimoCambioBaterias.Value.Date > DateTime.Now.Date)
                throw new ArgumentException("La fecha del último cambio de baterías no puede ser futura.");
            if (ups.FechaUltimoCambioBaterias.HasValue &&
                ups.FechaUltimoCambioBaterias.Value.Date < ups.FechaInstalacion.Date)
                throw new ArgumentException(
                    "La fecha del último cambio de baterías no puede ser anterior a la fecha de instalación.");
        }

        private static bool EsEstado(string? actual, string esperado) =>
            string.Equals(actual?.Trim(), esperado, StringComparison.OrdinalIgnoreCase);

        private static DateTime? MaxFecha(DateTime? a, DateTime? b)
        {
            if (!a.HasValue) return b;
            if (!b.HasValue) return a;
            return a.Value >= b.Value ? a : b;
        }
    }
}
