using System.Data;
using Microsoft.Data.SqlClient;
using InventarioUPS.API.Data;
using InventarioUPS.API.DTOs;
using InventarioUPS.API.Models;
using InventarioUPS.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace InventarioUPS.API.Services
{
    public class MantenimientoService : IMantenimientoService
    {
        private readonly IMantenimientoRepository _repository;
        private readonly InventarioUPSDbContext _context;

        public MantenimientoService(IMantenimientoRepository repository, InventarioUPSDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        public async Task<List<MantenimientoDTO>> ObtenerTodosAsync() =>
            (await _repository.ObtenerTodosAsync()).Select(ConvertirDTO).ToList();

        public async Task<MantenimientoDTO?> ObtenerPorIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El identificador del cambio de baterías no es válido.");

            var mantenimiento = await _repository.ObtenerPorIdAsync(id);
            return mantenimiento == null ? null : ConvertirDTO(mantenimiento);
        }

        public async Task<List<MantenimientoDTO>> ObtenerPorUPSAsync(int upsId)
        {
            if (upsId <= 0)
                throw new ArgumentException("El UPS seleccionado no es válido.");

            return (await _repository.ObtenerPorUPSAsync(upsId))
                .Select(ConvertirDTO)
                .ToList();
        }

        public Task CrearAsync(Mantenimiento mantenimiento) =>
            EjecutarTransaccionAsync(() => CrearInternoAsync(mantenimiento));

        public Task ActualizarAsync(Mantenimiento mantenimiento) =>
            EjecutarTransaccionAsync(() => ActualizarInternoAsync(mantenimiento));

        private async Task EjecutarTransaccionAsync(Func<Task> operacion)
        {
            await using var transaccion = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                await operacion();
                await transaccion.CommitAsync();
            }
            catch (Exception ex) when (ex is SqlException { Number: 1205 } ||
                ex is DbUpdateException { InnerException: SqlException { Number: 1205 } })
            {
                throw new InvalidOperationException("Otro usuario está actualizando este equipo. Recargue y vuelva a intentar.", ex);
            }
        }

        private async Task CrearInternoAsync(Mantenimiento mantenimiento)
        {
            ValidarDatosBasicos(mantenimiento);

            var ups = await ObtenerUPSAsync(mantenimiento.UPSId);
            ValidarUPS(ups);
            var estado = await ObtenerEstadoAsync(mantenimiento.EstadoMantenimientoId);

            await ValidarMantenimientoActivoDuplicadoAsync(
                mantenimiento.UPSId,
                mantenimiento.Id,
                estado.Nombre);

            ValidarFecha(mantenimiento.FechaMantenimiento, estado.Nombre);
            if (mantenimiento.FechaMantenimiento.Date < ups.FechaInstalacion.Date)
                throw new ArgumentException("El cambio no puede ser anterior a la instalación del UPS.");
            PrepararBaterias(mantenimiento, ups);
            if (!mantenimiento.ModeloBateriaId.HasValue || !await _context.ModelosBateria.AnyAsync(b => b.Id == mantenimiento.ModeloBateriaId && b.Activo && b.TipoBateria!.Activo))
                throw new ArgumentException("Seleccione el tipo y modelo de las baterías del cambio.");
            LimpiarTextos(mantenimiento);

            mantenimiento.AgenciaIdRegistro = ups.AgenciaId;
            mantenimiento.AgenciaNombreRegistro = ups.Agencia?.Nombre ?? string.Empty;

            await SincronizarEstadoUPSAsync(ups, mantenimiento, estado.Nombre, null);
            await RecalcularFechaBateriasAsync(ups, mantenimiento, estado.Nombre);

            _context.Mantenimientos.Add(mantenimiento);
            await _context.SaveChangesAsync();
        }

        private async Task ActualizarInternoAsync(Mantenimiento mantenimiento)
        {
            ValidarDatosBasicos(mantenimiento);

            var existente = await _context.Mantenimientos
                .Include(m => m.EstadoMantenimiento)
                .FirstOrDefaultAsync(m => m.Id == mantenimiento.Id && m.IncluyeCambioBaterias);

            if (existente == null)
                throw new InvalidOperationException("El cambio de baterías no existe.");

            if (mantenimiento.UPSId != existente.UPSId)
                throw new InvalidOperationException(
                    "No se puede cambiar el UPS de un cambio de baterías ya registrado.");

            var ups = await ObtenerUPSAsync(existente.UPSId);
            ValidarUPS(ups);
            var estado = await ObtenerEstadoAsync(mantenimiento.EstadoMantenimientoId);

            await ValidarMantenimientoActivoDuplicadoAsync(
                mantenimiento.UPSId,
                mantenimiento.Id,
                estado.Nombre);

            ValidarFecha(mantenimiento.FechaMantenimiento, estado.Nombre);
            if (mantenimiento.FechaMantenimiento.Date < ups.FechaInstalacion.Date)
                throw new ArgumentException("El cambio no puede ser anterior a la instalación del UPS.");
            PrepararBaterias(mantenimiento, ups);
            if (!mantenimiento.ModeloBateriaId.HasValue || !await _context.ModelosBateria.AnyAsync(b => b.Id == mantenimiento.ModeloBateriaId &&
                    ((b.Activo && b.TipoBateria!.Activo) || (existente.ModeloBateriaId == b.Id &&
                    (estado.Nombre == "Cancelado" || estado.Nombre == "Programado" ||
                    (existente.EstadoMantenimiento!.Nombre == "Completado" && estado.Nombre == "Completado"))))))
                throw new ArgumentException("Seleccione el tipo y modelo de las baterías del cambio.");
            if (EsEstado(existente.EstadoMantenimiento?.Nombre, "Completado") &&
                EsEstado(estado.Nombre, "Completado"))
                mantenimiento.CantidadBateriasReemplazadas = existente.CantidadBateriasReemplazadas;
            LimpiarTextos(mantenimiento);

            mantenimiento.AgenciaIdRegistro = existente.AgenciaIdRegistro ?? ups.AgenciaId;
            mantenimiento.AgenciaNombreRegistro =
                !string.IsNullOrWhiteSpace(existente.AgenciaNombreRegistro)
                    ? existente.AgenciaNombreRegistro
                    : ups.Agencia?.Nombre ?? string.Empty;
            mantenimiento.EstadoUPSAnteriorId = existente.EstadoUPSAnteriorId;

            await SincronizarEstadoUPSAsync(ups, mantenimiento, estado.Nombre, existente.EstadoMantenimiento?.Nombre);
            await RecalcularFechaBateriasAsync(ups, mantenimiento, estado.Nombre);

            existente.ModeloBateriaId = mantenimiento.ModeloBateriaId;
            existente.EstadoMantenimientoId = mantenimiento.EstadoMantenimientoId;
            existente.FechaMantenimiento = mantenimiento.FechaMantenimiento;
            existente.Descripcion = mantenimiento.Descripcion;
            existente.TecnicoProveedor = mantenimiento.TecnicoProveedor;
            existente.Observaciones = mantenimiento.Observaciones;
            existente.IncluyeCambioBaterias = mantenimiento.IncluyeCambioBaterias;
            existente.CantidadBateriasReemplazadas = mantenimiento.CantidadBateriasReemplazadas;
            existente.AgenciaIdRegistro = mantenimiento.AgenciaIdRegistro;
            existente.AgenciaNombreRegistro = mantenimiento.AgenciaNombreRegistro;
            existente.EstadoUPSAnteriorId = mantenimiento.EstadoUPSAnteriorId;
            existente.FechaModificacion = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        private async Task<UPS> ObtenerUPSAsync(int id)
        {
            var ups = await _context.UPSs
                .Include(u => u.Agencia)
                .Include(u => u.EstadoUPS)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (ups == null)
                throw new ArgumentException("El UPS seleccionado no existe.");

            return ups;
        }

        private static void ValidarUPS(UPS ups)
        {
            if (EsEstado(ups.EstadoUPS?.Nombre, "De baja"))
                throw new InvalidOperationException(
                    "No se pueden registrar cambios de baterías para un UPS dado De baja.");
        }

        private async Task<EstadoMantenimiento> ObtenerEstadoAsync(int id)
        {
            var estado = await _context.EstadosMantenimiento.FirstOrDefaultAsync(e => e.Id == id);
            if (estado == null)
                throw new ArgumentException("El estado de cambio de baterías seleccionado no existe.");
            if (!new[] { "Programado", "En proceso", "Completado", "Cancelado" }
                .Any(nombre => EsEstado(estado.Nombre, nombre)))
                throw new ArgumentException("El estado del cambio no está soportado.");
            if (!estado.Activo)
                throw new InvalidOperationException("El estado de cambio de baterías seleccionado está inactivo.");
            return estado;
        }

        private async Task SincronizarEstadoUPSAsync(
            UPS ups, Mantenimiento cambio, string estado, string? estadoAnterior)
        {
            if (EsEstado(estado, "En proceso"))
            {
                if (!EsEstado(estadoAnterior, "En proceso"))
                    cambio.EstadoUPSAnteriorId = ups.EstadoUPSId;
                var enProceso = await ObtenerEstadoUPSAsync("En mantenimiento");
                ups.EstadoUPSId = enProceso.Id;
                ups.EstadoUPS = enProceso;
                ups.FechaModificacion = DateTime.Now;
                return;
            }

            // Completar un reemplazo no demuestra que un equipo averiado esté reparado.
            if (EsEstado(estadoAnterior, "En proceso"))
            {
                var restaurar = await _context.EstadosUPS.FirstOrDefaultAsync(e =>
                    e.Id == cambio.EstadoUPSAnteriorId && e.Activo && e.Nombre != "En mantenimiento");
                if (ups.Agencia?.Activa == false)
                    restaurar = await ObtenerEstadoUPSAsync("En resguardo");
                if (restaurar == null)
                    throw new InvalidOperationException("No se puede recuperar el estado anterior del UPS. Revise su catálogo de estados.");
                ups.EstadoUPSId = restaurar.Id;
                ups.EstadoUPS = restaurar;
                ups.FechaModificacion = DateTime.Now;
            }
            cambio.EstadoUPSAnteriorId = null;
        }

        private async Task ValidarMantenimientoActivoDuplicadoAsync(
            int upsId,
            int mantenimientoId,
            string estado)
        {
            if (!EsEstado(estado, "En proceso") && !EsEstado(estado, "Completado"))
                return;

            var existe = await _context.Mantenimientos
                .AsNoTracking()
                .AnyAsync(m =>
                    m.UPSId == upsId &&
                    m.IncluyeCambioBaterias &&
                    m.Id != mantenimientoId &&
                    m.EstadoMantenimiento != null &&
                    m.EstadoMantenimiento.Nombre == "En proceso");

            if (existe)
            {
                throw new InvalidOperationException(
                    "El UPS ya tiene un cambio de baterías En proceso. Finalice o cancele ese cambio de baterías antes de iniciar otro.");
            }
        }

        private async Task<EstadoUPS> ObtenerEstadoUPSAsync(string nombre)
        {
            var estado = await _context.EstadosUPS.FirstOrDefaultAsync(e =>
                e.Activo && e.Nombre.ToUpper() == nombre.ToUpper());

            return estado ?? throw new InvalidOperationException(
                $"No existe el estado de UPS '{nombre}' activo.");
        }

        private async Task RecalcularFechaBateriasAsync(
            UPS ups,
            Mantenimiento actual,
            string estadoActual)
        {
            var ultimaOtra = await _context.Mantenimientos.AsNoTracking()
                .Where(m =>
                    m.UPSId == ups.Id &&
                    m.Id != actual.Id &&
                    m.IncluyeCambioBaterias &&
                    m.EstadoMantenimiento != null &&
                    m.EstadoMantenimiento.Nombre == "Completado")
                .Select(m => (DateTime?)m.FechaMantenimiento)
                .MaxAsync();

            DateTime? actualCompletada =
                actual.IncluyeCambioBaterias &&
                EsEstado(estadoActual, "Completado")
                    ? actual.FechaMantenimiento
                    : null;

            var otros = await _context.Mantenimientos.AsNoTracking()
                .Where(m => m.UPSId == ups.Id && m.Id != actual.Id && m.IncluyeCambioBaterias &&
                    m.EstadoMantenimiento!.Nombre == "Completado")
                .Select(m => new { m.Id, m.FechaMantenimiento, m.ModeloBateriaId }).ToListAsync();
            if (actualCompletada.HasValue)
                otros.Add(new { Id = actual.Id == 0 ? int.MaxValue : actual.Id, actual.FechaMantenimiento, actual.ModeloBateriaId });
            var ultimo = otros.OrderByDescending(m => m.FechaMantenimiento).ThenByDescending(m => m.Id).FirstOrDefault();
            ups.ModeloBateriaId = ultimo != null && (!ups.FechaUltimoCambioBateriasBase.HasValue ||
                ultimo.FechaMantenimiento >= ups.FechaUltimoCambioBateriasBase.Value)
                ? ultimo.ModeloBateriaId ?? ups.ModeloBateriaBaseId : ups.ModeloBateriaBaseId;

            ups.FechaUltimoCambioBaterias =
                MaxFecha(
                    ups.FechaUltimoCambioBateriasBase,
                    MaxFecha(ultimaOtra, actualCompletada));
        }

        private static void ValidarDatosBasicos(Mantenimiento m)
        {
            if (m == null)
                throw new ArgumentException("Los datos del cambio de baterías son obligatorios.");
            if (m.UPSId <= 0)
                throw new ArgumentException("Debe seleccionar un UPS.");
            if (m.EstadoMantenimientoId <= 0)
                throw new ArgumentException("Debe seleccionar un estado de cambio de baterías.");
            if (m.FechaMantenimiento == default)
                throw new ArgumentException("La fecha del cambio de baterías es obligatoria.");
            if (string.IsNullOrWhiteSpace(m.Descripcion))
                throw new ArgumentException("La descripción del cambio de baterías es obligatoria.");
            if (string.IsNullOrWhiteSpace(m.TecnicoProveedor))
                throw new ArgumentException("El técnico o proveedor es obligatorio.");
        }

        private static void ValidarFecha(DateTime fecha, string estado)
        {
            if (fecha.Date > DateTime.Now.Date && !EsEstado(estado, "Programado"))
                throw new ArgumentException(
                    "Una fecha futura solamente puede utilizarse cuando el estado del cambio de baterías es Programado.");
        }

        private static void PrepararBaterias(Mantenimiento m, UPS ups)
        {
            m.IncluyeCambioBaterias = true;

            if (ups.CantidadBaterias <= 0)
                throw new InvalidOperationException(
                    "El UPS no tiene una cantidad válida de baterías registrada.");

            m.CantidadBateriasReemplazadas = ups.CantidadBaterias;
        }

        private static void LimpiarTextos(Mantenimiento m)
        {
            m.Descripcion = m.Descripcion?.Trim() ?? string.Empty;
            m.TecnicoProveedor = m.TecnicoProveedor?.Trim() ?? string.Empty;
            m.Observaciones = m.Observaciones?.Trim() ?? string.Empty;
        }

        private static bool EsEstado(string? valor, string esperado) =>
            string.Equals(valor?.Trim(), esperado, StringComparison.OrdinalIgnoreCase);

        private static DateTime? MaxFecha(DateTime? a, DateTime? b)
        {
            if (!a.HasValue) return b;
            if (!b.HasValue) return a;
            return a.Value >= b.Value ? a : b;
        }

        private static MantenimientoDTO ConvertirDTO(Mantenimiento m) => new()
        {
            Id = m.Id,
            ModeloBateriaId = m.ModeloBateriaId,
            ModeloBateria = m.ModeloBateria?.Nombre ?? "Sin identificar",
            TipoBateriaId = m.ModeloBateria?.TipoBateriaId,
            TipoBateria = m.ModeloBateria?.TipoBateria?.Nombre ?? "Sin identificar",
            UPSId = m.UPSId,
            EstadoMantenimientoId = m.EstadoMantenimientoId,
            FechaMantenimiento = m.FechaMantenimiento,
            Descripcion = m.Descripcion,
            TecnicoProveedor = m.TecnicoProveedor,
            Observaciones = m.Observaciones,
            CantidadBateriasReemplazadas = m.CantidadBateriasReemplazadas,
            AgenciaIdRegistro = m.AgenciaIdRegistro,
            NumeroInventarioUPS = m.UPS?.NumeroInventario,
            NumeroSerieUPS = m.UPS?.NumeroSerie,
            MarcaUPS = m.UPS?.Marca,
            ModeloUPS = m.UPS?.Modelo,
            AgenciaNombre =
                !string.IsNullOrWhiteSpace(m.AgenciaNombreRegistro)
                    ? m.AgenciaNombreRegistro
                    : m.UPS?.Agencia?.Nombre,
            Estado = m.EstadoMantenimiento?.Nombre
        };
    }
}
