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

        public Task CrearAsync(Mantenimiento mantenimiento, int usuarioActualId) =>
            EjecutarTransaccionAsync(() => CrearInternoAsync(mantenimiento, usuarioActualId));

        public Task ActualizarAsync(Mantenimiento mantenimiento, int usuarioActualId) =>
            EjecutarTransaccionAsync(() => ActualizarInternoAsync(mantenimiento, usuarioActualId));

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

        private async Task CrearInternoAsync(Mantenimiento mantenimiento, int usuarioActualId)
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
            await ValidarBateriaAsync(mantenimiento);
            LimpiarTextos(mantenimiento);

            await ResolverResponsableAsync(mantenimiento, usuarioActualId);
            mantenimiento.AgenciaIdRegistro = ups.AgenciaId;
            mantenimiento.AgenciaNombreRegistro = ups.Agencia?.Nombre ?? string.Empty;

            await SincronizarEstadoUPSAsync(ups, mantenimiento, estado.Nombre, null);
            await RecalcularFechaBateriasAsync(ups, mantenimiento, estado.Nombre);

            _context.Mantenimientos.Add(mantenimiento);
            await _context.SaveChangesAsync();
        }

        private async Task ActualizarInternoAsync(Mantenimiento mantenimiento, int usuarioActualId)
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
            await ValidarBateriaAsync(mantenimiento, existente, estado.Nombre);
            if (EsEstado(existente.EstadoMantenimiento?.Nombre, "Completado") &&
                EsEstado(estado.Nombre, "Completado"))
                mantenimiento.CantidadBateriasReemplazadas = existente.CantidadBateriasReemplazadas;
            LimpiarTextos(mantenimiento);

            await ResolverResponsableAsync(mantenimiento, usuarioActualId, existente);
            mantenimiento.AgenciaIdRegistro = existente.AgenciaIdRegistro ?? ups.AgenciaId;
            mantenimiento.AgenciaNombreRegistro =
                !string.IsNullOrWhiteSpace(existente.AgenciaNombreRegistro)
                    ? existente.AgenciaNombreRegistro
                    : ups.Agencia?.Nombre ?? string.Empty;
            mantenimiento.EstadoUPSAnteriorId = existente.EstadoUPSAnteriorId;

            await SincronizarEstadoUPSAsync(ups, mantenimiento, estado.Nombre, existente.EstadoMantenimiento?.Nombre);
            await RecalcularFechaBateriasAsync(ups, mantenimiento, estado.Nombre);

            existente.TipoBateriaId = mantenimiento.TipoBateriaId;
            existente.VoltajeBateria = mantenimiento.VoltajeBateria;
            existente.CapacidadBateriaAh = mantenimiento.CapacidadBateriaAh;
            existente.EstadoMantenimientoId = mantenimiento.EstadoMantenimientoId;
            existente.FechaMantenimiento = mantenimiento.FechaMantenimiento;
            existente.ResponsableUsuarioId = mantenimiento.ResponsableUsuarioId;
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
                .Select(m => new { m.Id, m.FechaMantenimiento, m.TipoBateriaId, m.VoltajeBateria, m.CapacidadBateriaAh }).ToListAsync();
            if (actualCompletada.HasValue)
                otros.Add(new { Id = actual.Id == 0 ? int.MaxValue : actual.Id, actual.FechaMantenimiento, actual.TipoBateriaId, actual.VoltajeBateria, actual.CapacidadBateriaAh });
            var ultimo = otros.OrderByDescending(m => m.FechaMantenimiento).ThenByDescending(m => m.Id).FirstOrDefault();
            var usarUltimo = ultimo?.TipoBateriaId != null && (!ups.FechaUltimoCambioBateriasBase.HasValue ||
                ultimo.FechaMantenimiento >= ups.FechaUltimoCambioBateriasBase.Value);
            ups.TipoBateriaId = usarUltimo ? ultimo!.TipoBateriaId : ups.TipoBateriaBaseId;
            ups.VoltajeBateria = usarUltimo ? ultimo!.VoltajeBateria : ups.VoltajeBateriaBase;
            ups.CapacidadBateriaAh = usarUltimo ? ultimo!.CapacidadBateriaAh : ups.CapacidadBateriaAhBase;

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
            if ((m.Observaciones?.Length ?? 0) > 1000)
                throw new ArgumentException("Las observaciones admiten hasta 1000 caracteres.");
        }

        private async Task ValidarBateriaAsync(Mantenimiento cambio, Mantenimiento? anterior = null, string? estado = null)
        {
            var tipoAnterior = anterior?.TipoBateriaId;
            var conservar = anterior != null && tipoAnterior == cambio.TipoBateriaId &&
                (estado is "Cancelado" or "Programado" || (estado == "Completado" && anterior.EstadoMantenimiento?.Nombre == "Completado"));
            if (!cambio.TipoBateriaId.HasValue || !await _context.TiposBateria.AnyAsync(x => x.Id == cambio.TipoBateriaId && (x.Activo || conservar)))
                throw new ArgumentException("Seleccione un tipo de batería activo y válido.");
            if (!(conservar && cambio.VoltajeBateria == anterior!.VoltajeBateria && cambio.CapacidadBateriaAh == anterior.CapacidadBateriaAh))
                DatosBateria.Validar(cambio.VoltajeBateria, cambio.CapacidadBateriaAh);
        }

        private async Task ResolverResponsableAsync(Mantenimiento cambio, int usuarioActualId, Mantenimiento? anterior = null)
        {
            var actor = await _context.Usuarios.AsNoTracking().Include(x => x.Rol).FirstOrDefaultAsync(x => x.Id == usuarioActualId);
            if (actor?.Activo != true || actor.Rol?.Activo != true)
                throw new UnauthorizedAccessException();
            var administrador = actor.Rol.Nombre is "Administrador" or "Control Total";
            if (!administrador)
            {
                if ((anterior != null && anterior.ResponsableUsuarioId != actor.Id) ||
                    (cambio.ResponsableUsuarioId.HasValue && cambio.ResponsableUsuarioId != actor.Id))
                    throw new UnauthorizedAccessException("Solo puede registrar y modificar cambios asignados a su usuario.");
                cambio.ResponsableUsuarioId = actor.Id;
            }
            else cambio.ResponsableUsuarioId ??= anterior?.ResponsableUsuarioId ?? actor.Id;
            var responsable = await _context.Usuarios.AsNoTracking().Include(x => x.Rol)
                .FirstOrDefaultAsync(x => x.Id == cambio.ResponsableUsuarioId);
            var conservar = anterior != null && anterior.ResponsableUsuarioId == cambio.ResponsableUsuarioId;
            var habilitado = responsable?.Activo == true && responsable.Rol?.Activo == true &&
                (responsable.Rol.Nombre is "Control Total" or "Administrador" || await _context.RolPermisos.AnyAsync(x =>
                    x.RolId == responsable.RolId && x.Permiso!.Activo &&
                    (x.Permiso.Codigo == "CREAR_MANTENIMIENTO" || x.Permiso.Codigo == "EDITAR_MANTENIMIENTO")));
            if (responsable == null || (!conservar && !habilitado))
                throw new ArgumentException("Seleccione un usuario activo habilitado para realizar cambios de baterías.");
            cambio.TecnicoProveedor = conservar ? anterior!.TecnicoProveedor : responsable.NombreCompleto;
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
            TipoBateriaId = m.TipoBateriaId,
            VoltajeBateria = m.VoltajeBateria,
            CapacidadBateriaAh = m.CapacidadBateriaAh,
            TipoBateria = m.TipoBateria?.Nombre ?? "Sin identificar",
            UPSId = m.UPSId,
            EstadoMantenimientoId = m.EstadoMantenimientoId,
            FechaMantenimiento = m.FechaMantenimiento,
            ResponsableUsuarioId = m.ResponsableUsuarioId,
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
