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
                throw new ArgumentException("El identificador del mantenimiento no es válido.");

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

        public async Task CrearAsync(Mantenimiento mantenimiento)
        {
            ValidarDatosBasicos(mantenimiento);

            var ups = await ObtenerUPSAsync(mantenimiento.UPSId);
            ValidarUPS(ups);
            _ = await ObtenerTipoAsync(mantenimiento.TipoMantenimientoId);
            var estado = await ObtenerEstadoAsync(mantenimiento.EstadoMantenimientoId);

            await ValidarMantenimientoActivoDuplicadoAsync(
                mantenimiento.UPSId,
                mantenimiento.Id,
                estado.Nombre);

            ValidarFecha(mantenimiento.FechaMantenimiento, estado.Nombre);
            PrepararBaterias(mantenimiento, ups);
            LimpiarTextos(mantenimiento);

            mantenimiento.AgenciaIdRegistro = ups.AgenciaId;
            mantenimiento.AgenciaNombreRegistro = ups.Agencia?.Nombre ?? string.Empty;

            await SincronizarEstadoUPSAsync(ups, mantenimiento, estado.Nombre);
            await RecalcularFechaBateriasAsync(ups, mantenimiento, estado.Nombre);

            _context.Mantenimientos.Add(mantenimiento);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Mantenimiento mantenimiento)
        {
            ValidarDatosBasicos(mantenimiento);

            var existente = await _context.Mantenimientos
                .Include(m => m.EstadoMantenimiento)
                .FirstOrDefaultAsync(m => m.Id == mantenimiento.Id);

            if (existente == null)
                throw new InvalidOperationException("El mantenimiento no existe.");

            if (mantenimiento.UPSId != existente.UPSId)
                throw new InvalidOperationException(
                    "No se puede cambiar el UPS de un mantenimiento ya registrado.");

            var ups = await ObtenerUPSAsync(existente.UPSId);
            ValidarUPS(ups);
            _ = await ObtenerTipoAsync(mantenimiento.TipoMantenimientoId);
            var estado = await ObtenerEstadoAsync(mantenimiento.EstadoMantenimientoId);

            ValidarFecha(mantenimiento.FechaMantenimiento, estado.Nombre);
            PrepararBaterias(mantenimiento, ups);
            LimpiarTextos(mantenimiento);

            mantenimiento.AgenciaIdRegistro = existente.AgenciaIdRegistro ?? ups.AgenciaId;
            mantenimiento.AgenciaNombreRegistro =
                !string.IsNullOrWhiteSpace(existente.AgenciaNombreRegistro)
                    ? existente.AgenciaNombreRegistro
                    : ups.Agencia?.Nombre ?? string.Empty;
            mantenimiento.EstadoUPSAnteriorId = existente.EstadoUPSAnteriorId;

            await SincronizarEstadoUPSAsync(ups, mantenimiento, estado.Nombre);
            await RecalcularFechaBateriasAsync(ups, mantenimiento, estado.Nombre);

            existente.TipoMantenimientoId = mantenimiento.TipoMantenimientoId;
            existente.EstadoMantenimientoId = mantenimiento.EstadoMantenimientoId;
            existente.FechaMantenimiento = mantenimiento.FechaMantenimiento;
            existente.Descripcion = mantenimiento.Descripcion;
            existente.TecnicoProveedor = mantenimiento.TecnicoProveedor;
            existente.Costo = mantenimiento.Costo;
            existente.Observaciones = mantenimiento.Observaciones;
            existente.IncluyeCambioBaterias = mantenimiento.IncluyeCambioBaterias;
            existente.CantidadBateriasReemplazadas = mantenimiento.CantidadBateriasReemplazadas;
            existente.CostoBaterias = mantenimiento.CostoBaterias;
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
                    "No se pueden registrar mantenimientos para un UPS dado De baja.");
        }

        private async Task<TipoMantenimiento> ObtenerTipoAsync(int id)
        {
            var tipo = await _context.TiposMantenimiento.FirstOrDefaultAsync(t => t.Id == id);
            if (tipo == null)
                throw new ArgumentException("El tipo de mantenimiento seleccionado no existe.");
            if (!tipo.Activo)
                throw new InvalidOperationException("El tipo de mantenimiento seleccionado está inactivo.");
            return tipo;
        }

        private async Task<EstadoMantenimiento> ObtenerEstadoAsync(int id)
        {
            var estado = await _context.EstadosMantenimiento.FirstOrDefaultAsync(e => e.Id == id);
            if (estado == null)
                throw new ArgumentException("El estado de mantenimiento seleccionado no existe.");
            if (!estado.Activo)
                throw new InvalidOperationException("El estado de mantenimiento seleccionado está inactivo.");
            return estado;
        }

        private async Task SincronizarEstadoUPSAsync(
            UPS ups,
            Mantenimiento mantenimiento,
            string estadoMantenimiento)
        {
            if (EsEstado(estadoMantenimiento, "Programado"))
            {
                // Si el registro estaba En proceso y se devuelve a Programado,
                // el UPS debe recuperar el estado que tenía antes.
                if (EsEstado(ups.EstadoUPS?.Nombre, "En mantenimiento") &&
                    mantenimiento.EstadoUPSAnteriorId.HasValue)
                {
                    var anterior = await _context.EstadosUPS
                        .FirstOrDefaultAsync(e =>
                            e.Id == mantenimiento.EstadoUPSAnteriorId.Value &&
                            e.Activo &&
                            e.Nombre != "De baja");

                    if (anterior != null)
                    {
                        ups.EstadoUPSId = anterior.Id;
                        ups.EstadoUPS = anterior;
                        ups.FechaModificacion = DateTime.Now;
                    }
                }

                mantenimiento.EstadoUPSAnteriorId = null;
                return;
            }

            if (mantenimiento.EstadoUPSAnteriorId == null)
            {
                mantenimiento.EstadoUPSAnteriorId = ups.EstadoUPSId;
            }

            if (EsEstado(estadoMantenimiento, "En proceso"))
            {
                var estado = await ObtenerEstadoUPSAsync("En mantenimiento");
                ups.EstadoUPSId = estado.Id;
                ups.EstadoUPS = estado;
                ups.FechaModificacion = DateTime.Now;
                return;
            }

            if (EsEstado(estadoMantenimiento, "Completado"))
            {
                // Una agencia inactiva conserva sus equipos en resguardo.
                // En una agencia activa, un mantenimiento completado deja
                // el equipo operativo.
                var nombreDestino =
                    ups.Agencia?.Activa == false
                        ? "En resguardo"
                        : "Operativo";

                var estado = await ObtenerEstadoUPSAsync(nombreDestino);
                ups.EstadoUPSId = estado.Id;
                ups.EstadoUPS = estado;
                ups.FechaModificacion = DateTime.Now;
                return;
            }

            if (EsEstado(estadoMantenimiento, "Cancelado"))
            {
                EstadoUPS? restaurar = null;

                if (mantenimiento.EstadoUPSAnteriorId.HasValue)
                {
                    restaurar = await _context.EstadosUPS
                        .FirstOrDefaultAsync(e =>
                            e.Id == mantenimiento.EstadoUPSAnteriorId.Value &&
                            e.Activo &&
                            e.Nombre != "De baja");
                }

                if (restaurar == null)
                {
                    var nombreDestino =
                        ups.Agencia?.Activa == false
                            ? "En resguardo"
                            : "Operativo";

                    restaurar = await ObtenerEstadoUPSAsync(nombreDestino);
                }

                ups.EstadoUPSId = restaurar.Id;
                ups.EstadoUPS = restaurar;
                ups.FechaModificacion = DateTime.Now;
            }
        }

        private async Task ValidarMantenimientoActivoDuplicadoAsync(
            int upsId,
            int mantenimientoId,
            string estado)
        {
            if (!EsEstado(estado, "En proceso"))
                return;

            var existe = await _context.Mantenimientos
                .AsNoTracking()
                .AnyAsync(m =>
                    m.UPSId == upsId &&
                    m.Id != mantenimientoId &&
                    m.EstadoMantenimiento != null &&
                    m.EstadoMantenimiento.Nombre == "En proceso");

            if (existe)
            {
                throw new InvalidOperationException(
                    "El UPS ya tiene un mantenimiento En proceso. Finalice o cancele ese mantenimiento antes de iniciar otro.");
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
            ups.FechaUltimoCambioBateriasBase ??= ups.FechaUltimoCambioBaterias;

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

            ups.FechaUltimoCambioBaterias =
                MaxFecha(
                    ups.FechaUltimoCambioBateriasBase,
                    MaxFecha(ultimaOtra, actualCompletada));
        }

        private static void ValidarDatosBasicos(Mantenimiento m)
        {
            if (m == null)
                throw new ArgumentException("Los datos del mantenimiento son obligatorios.");
            if (m.UPSId <= 0)
                throw new ArgumentException("Debe seleccionar un UPS.");
            if (m.TipoMantenimientoId <= 0)
                throw new ArgumentException("Debe seleccionar un tipo de mantenimiento.");
            if (m.EstadoMantenimientoId <= 0)
                throw new ArgumentException("Debe seleccionar un estado de mantenimiento.");
            if (m.FechaMantenimiento == default)
                throw new ArgumentException("La fecha del mantenimiento es obligatoria.");
            if (string.IsNullOrWhiteSpace(m.Descripcion))
                throw new ArgumentException("La descripción del mantenimiento es obligatoria.");
            if (string.IsNullOrWhiteSpace(m.TecnicoProveedor))
                throw new ArgumentException("El técnico o proveedor es obligatorio.");
            if (m.Costo < 0)
                throw new ArgumentException("El costo del mantenimiento no puede ser negativo.");
        }

        private static void ValidarFecha(DateTime fecha, string estado)
        {
            if (fecha.Date > DateTime.Now.Date && !EsEstado(estado, "Programado"))
                throw new ArgumentException(
                    "Una fecha futura solamente puede utilizarse cuando el estado del mantenimiento es Programado.");
        }

        private static void PrepararBaterias(Mantenimiento m, UPS ups)
        {
            if (!m.IncluyeCambioBaterias)
            {
                m.CantidadBateriasReemplazadas = 0;
                m.CostoBaterias = 0;
                return;
            }

            if (ups.CantidadBaterias <= 0)
                throw new InvalidOperationException(
                    "El UPS no tiene una cantidad válida de baterías registrada.");

            m.CantidadBateriasReemplazadas = ups.CantidadBaterias;

            if (m.CostoBaterias < 0)
                throw new ArgumentException("El costo de las baterías no puede ser negativo.");
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
            UPSId = m.UPSId,
            TipoMantenimientoId = m.TipoMantenimientoId,
            EstadoMantenimientoId = m.EstadoMantenimientoId,
            FechaMantenimiento = m.FechaMantenimiento,
            Descripcion = m.Descripcion,
            TecnicoProveedor = m.TecnicoProveedor,
            Costo = m.Costo,
            Observaciones = m.Observaciones,
            IncluyeCambioBaterias = m.IncluyeCambioBaterias,
            CantidadBateriasReemplazadas = m.CantidadBateriasReemplazadas,
            CostoBaterias = m.CostoBaterias,
            AgenciaIdRegistro = m.AgenciaIdRegistro,
            CodigoUPS = m.UPS?.Codigo,
            NumeroSerieUPS = m.UPS?.NumeroSerie,
            MarcaUPS = m.UPS?.Marca,
            ModeloUPS = m.UPS?.Modelo,
            AgenciaNombre =
                !string.IsNullOrWhiteSpace(m.AgenciaNombreRegistro)
                    ? m.AgenciaNombreRegistro
                    : m.UPS?.Agencia?.Nombre,
            TipoMantenimiento = m.Tipo?.Nombre,
            Estado = m.EstadoMantenimiento?.Nombre
        };
    }
}
