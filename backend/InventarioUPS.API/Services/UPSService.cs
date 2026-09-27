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

            if (await _repository.ExisteNumeroInventarioAsync(ups.NumeroInventario))
                throw new InvalidOperationException("Ya existe un UPS con ese número de inventario.");

            if (await _repository.ExisteNumeroSerieAsync(ups.NumeroSerie))
                throw new InvalidOperationException("Ya existe un UPS con ese número de serie.");

            if (!await _repository.ExisteAgenciaActivaAsync(ups.AgenciaId))
                throw new InvalidOperationException("La agencia seleccionada no existe o está inactiva.");

            if (!await _repository.ExisteEstadoUPSActivoAsync(ups.EstadoUPSId))
                throw new InvalidOperationException("El estado de UPS seleccionado no existe o está inactivo.");

            await ValidarEstadoOperativoAsync(0, ups.EstadoUPSId, ups.AgenciaId);
            ups.FechaUltimoCambioBateriasBase = ups.FechaUltimoCambioBaterias;
            await ResolverBateriaAsync(ups);
            ups.TipoBateriaBaseId = ups.TipoBateriaId;
            ups.TipoBateria = null;
            ups.ModeloBateriaBaseId = ups.ModeloBateriaId;
            ups.ModeloBateria = null;
            await ResolverModeloUPSAsync(ups);
            ups.Agencia = null;
            ups.EstadoUPS = null;
            ups.Mantenimientos = new List<Mantenimiento>();
            ups.Id = 0;
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

            if (await _repository.ExisteNumeroInventarioExceptoIdAsync(ups.NumeroInventario, ups.Id))
                throw new InvalidOperationException("Ya existe otro UPS con ese número de inventario.");

            if (await _repository.ExisteNumeroSerieExceptoIdAsync(ups.NumeroSerie, ups.Id))
                throw new InvalidOperationException("Ya existe otro UPS con ese número de serie.");

            if (ups.AgenciaId != existente.AgenciaId && !await _repository.ExisteAgenciaActivaAsync(ups.AgenciaId))
                throw new InvalidOperationException("La agencia seleccionada no existe o está inactiva.");

            await ValidarEstadoOperativoAsync(ups.Id, ups.EstadoUPSId, ups.AgenciaId);
            await ResolverBateriaAsync(ups, existente);
            var enProceso = await TieneCambioEnProcesoAsync(ups.Id);
            if (enProceso && (ups.AgenciaId != existente.AgenciaId || ups.CantidadBaterias != existente.CantidadBaterias || ups.ModeloBateriaId != existente.ModeloBateriaId || ups.TipoBateriaId != existente.TipoBateriaId))
                throw new InvalidOperationException("Finalice o cancele el cambio en proceso antes de trasladar el UPS o modificar su banco de baterías.");
            if (await _context.Mantenimientos.AnyAsync(m => m.UPSId == ups.Id && m.IncluyeCambioBaterias &&
                m.FechaMantenimiento < ups.FechaInstalacion))
                throw new ArgumentException("La instalación no puede ser posterior a los cambios registrados del UPS.");

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
                    existente.FechaUltimoCambioBateriasBase;

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

            var tieneModeloHistorico = await _context.Mantenimientos.AnyAsync(m => m.UPSId == ups.Id &&
                m.IncluyeCambioBaterias && m.ModeloBateriaId != null && m.EstadoMantenimiento!.Nombre == "Completado");
            if (tieneModeloHistorico && (ups.ModeloBateriaId != existente.ModeloBateriaId || ups.TipoBateriaId != existente.TipoBateriaId))
                throw new InvalidOperationException("Registre el nuevo modelo desde Cambios de baterías para conservar el historial.");
            ups.TipoBateriaBaseId = tieneModeloHistorico ? existente.TipoBateriaBaseId : ups.TipoBateriaId;
            ups.ModeloBateriaBaseId = tieneModeloHistorico ? existente.ModeloBateriaBaseId : ups.ModeloBateriaId;
            await ResolverModeloUPSAsync(ups, existente);
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

            await ValidarEstadoOperativoAsync(id, destino.Id, ups.AgenciaId);
            await _repository.CambiarEstadoAsync(id, destino.Nombre);
        }

        private async Task ResolverModeloUPSAsync(UPS ups, UPS? anterior = null)
        {
            var modelo = await _context.ModelosUPS.Include(x => x.MarcaUPS).FirstOrDefaultAsync(x =>
                x.Nombre == ups.Modelo && x.MarcaUPS!.Nombre == ups.Marca);
            if (modelo == null || ((!modelo.Activo || modelo.MarcaUPS?.Activo != true) && modelo.Id != anterior?.ModeloUPSId))
                throw new ArgumentException("Seleccione una marca y modelo de UPS activos del catálogo. Solicite las nuevas opciones al administrador.");
            ups.ModeloUPSId = modelo.Id;
            ups.ModeloUPS = null;
            ups.Marca = modelo.MarcaUPS!.Nombre;
            ups.Modelo = modelo.Nombre;
        }

        private async Task ResolverBateriaAsync(UPS ups, UPS? anterior = null)
        {
            if (ups.ModeloBateriaId.HasValue)
            {
                var modelo = await _context.ModelosBateria.AsNoTracking().Include(x => x.TipoBateria)
                    .FirstOrDefaultAsync(x => x.Id == ups.ModeloBateriaId);
                if (modelo == null || ((!modelo.Activo || modelo.TipoBateria?.Activo != true) && modelo.Id != anterior?.ModeloBateriaId))
                    throw new ArgumentException("Seleccione un modelo de batería activo y válido.");
                if (ups.TipoBateriaId.HasValue && ups.TipoBateriaId != modelo.TipoBateriaId)
                    throw new ArgumentException("El modelo de batería no pertenece al tipo seleccionado.");
                ups.TipoBateriaId = modelo.TipoBateriaId;
            }
            var tipoAnterior = anterior?.TipoBateriaId;
            if (!ups.TipoBateriaId.HasValue || !await _context.TiposBateria.AnyAsync(x => x.Id == ups.TipoBateriaId &&
                (x.Activo || x.Id == tipoAnterior)))
                throw new ArgumentException("Seleccione un tipo de batería válido. El modelo puede quedar pendiente de identificar.");
        }

        private Task<bool> TieneCambioEnProcesoAsync(int upsId) =>
            _context.Mantenimientos.AnyAsync(m => m.UPSId == upsId && m.IncluyeCambioBaterias &&
                m.EstadoMantenimiento != null && m.EstadoMantenimiento.Nombre == "En proceso");

        private async Task ValidarEstadoOperativoAsync(int upsId, int estadoId, int agenciaId)
        {
            var estado = await _context.EstadosUPS.AsNoTracking().FirstAsync(e => e.Id == estadoId);
            var agenciaActiva = await _context.Agencias.AnyAsync(a => a.Id == agenciaId && a.Activa);
            if (!agenciaActiva && !EsEstado(estado.Nombre, "En resguardo") && !EsEstado(estado.Nombre, "De baja"))
                throw new InvalidOperationException("Un UPS en una agencia inactiva debe permanecer en resguardo o de baja.");
            var enProceso = upsId > 0 && await TieneCambioEnProcesoAsync(upsId);
            if (EsEstado(estado.Nombre, "En mantenimiento") != enProceso)
                throw new InvalidOperationException("El estado En mantenimiento se administra desde Cambios de baterías. Inicie o finalice el cambio allí.");
        }

        private async Task ValidarSinMantenimientoPendienteAsync(int upsId)
        {
            var pendiente = await _context.Mantenimientos
                .AsNoTracking()
                .AnyAsync(m =>
                    m.UPSId == upsId &&
                    m.IncluyeCambioBaterias &&
                    m.EstadoMantenimiento != null &&
                    (m.EstadoMantenimiento.Nombre == "Programado" ||
                     m.EstadoMantenimiento.Nombre == "En proceso"));

            if (pendiente)
                throw new InvalidOperationException(
                    "No se puede dar de baja el UPS mientras tenga cambios de baterías Programados o En proceso.");
        }

        private static void ValidarYLimpia(UPS ups)
        {
            if (ups == null)
                throw new ArgumentException("Los datos del UPS son obligatorios.");

            ups.NumeroInventario = ups.NumeroInventario?.Trim() ?? string.Empty;
            ups.Marca = ups.Marca?.Trim() ?? string.Empty;
            ups.Modelo = ups.Modelo?.Trim() ?? string.Empty;
            ups.NumeroSerie = ups.NumeroSerie?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(ups.NumeroInventario))
                throw new ArgumentException("El número de inventario del UPS es obligatorio.");
            if (ups.NumeroInventario.Length > 100)
                throw new ArgumentException("El número de inventario no puede superar 100 caracteres.");
            if (string.IsNullOrWhiteSpace(ups.Marca))
                throw new ArgumentException("La marca del UPS es obligatoria.");
            if (string.IsNullOrWhiteSpace(ups.Modelo))
                throw new ArgumentException("El modelo del UPS es obligatorio.");
            if (string.IsNullOrWhiteSpace(ups.NumeroSerie))
                throw new ArgumentException("El número de serie es obligatorio.");
            if (ups.NumeroSerie.Length > 100)
                throw new ArgumentException("El número de serie no puede superar 100 caracteres.");
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
