using InventarioUPS.API.Data;
using InventarioUPS.API.Models;
using InventarioUPS.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace InventarioUPS.API.Services
{
    public class AgenciaService : IAgenciaService
    {
        private readonly IAgenciaRepository _repository;
        private readonly InventarioUPSDbContext _context;

        public AgenciaService(IAgenciaRepository repository, InventarioUPSDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        public Task<List<Agencia>> ObtenerTodasAsync() => _repository.ObtenerTodasAsync();
        public Task<Agencia?> ObtenerPorIdAsync(int id) => _repository.ObtenerPorIdAsync(id);

        public async Task CrearAsync(Agencia agencia)
        {
            if (agencia == null)
                throw new ArgumentException("La información de la agencia es obligatoria.");

            LimpiarDatos(agencia);
            ValidarDatosBasicos(agencia);

            var ciudad = await ObtenerCiudadActivaAsync(agencia.CiudadId);
            agencia.TipoAgenciaId = await ObtenerTipoAgenciaSegunCiudadAsync(ciudad);

            var codigo = agencia.Codigo.ToUpper();

            if (await _context.Agencias.AsNoTracking()
                .AnyAsync(a => a.Codigo.ToUpper() == codigo))
                throw new InvalidOperationException("Ya existe una agencia con ese código.");

            agencia.Ciudad = null;
            agencia.TipoAgencia = null;
            agencia.UPSs = new List<UPS>();
            agencia.Activa = true;
            await _repository.CrearAsync(agencia);
        }

        public async Task ActualizarAsync(Agencia agencia)
        {
            if (agencia == null || agencia.Id <= 0)
                throw new ArgumentException("El identificador de la agencia no es válido.");

            LimpiarDatos(agencia);
            ValidarDatosBasicos(agencia);

            var existente = await _repository.ObtenerPorIdAsync(agencia.Id);

            if (existente == null)
                throw new InvalidOperationException("La agencia no existe.");

            var ciudad = await ObtenerCiudadActivaAsync(agencia.CiudadId, existente.Activa || !agencia.Activa ? existente.CiudadId : null);
            agencia.TipoAgenciaId = await ObtenerTipoAgenciaSegunCiudadAsync(ciudad);

            var codigo = agencia.Codigo.ToUpper();

            if (await _context.Agencias.AsNoTracking()
                .AnyAsync(a => a.Id != agencia.Id && a.Codigo.ToUpper() == codigo))
                throw new InvalidOperationException("Ya existe otra agencia con ese código.");

            if (existente.Activa && !agencia.Activa)
                await PrepararDesactivacionAgenciaAsync(agencia.Id);

            await _repository.ActualizarAsync(agencia);
        }

        public Task EliminarAsync(int id) => CambiarEstadoAsync(id, false);

        public async Task CambiarEstadoAsync(int id, bool activa)
        {
            if (id <= 0)
                throw new ArgumentException("El identificador de la agencia no es válido.");

            var agencia = await _context.Agencias.FirstOrDefaultAsync(a => a.Id == id);

            if (agencia == null)
                throw new InvalidOperationException("La agencia no existe.");

            if (agencia.Activa == activa)
                throw new InvalidOperationException(
                    activa ? "La agencia ya está activa." : "La agencia ya está desactivada.");

            if (activa) await ObtenerCiudadActivaAsync(agencia.CiudadId);
            if (!activa)
                await PrepararDesactivacionAgenciaAsync(id);

            agencia.Activa = activa;
            agencia.FechaModificacion = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        private async Task<Ciudad> ObtenerCiudadActivaAsync(int ciudadId, int? anterior = null)
        {
            var ciudad = await _context.Ciudades.AsNoTracking()
                .Include(c => c.Departamento).FirstOrDefaultAsync(c => c.Id == ciudadId);

            if (ciudad == null)
                throw new ArgumentException("La ciudad seleccionada no existe.");

            if (ciudadId != anterior && (!ciudad.Activa || ciudad.Departamento?.Activo != true))
                throw new InvalidOperationException("La ciudad seleccionada está inactiva.");

            return ciudad;
        }

        private async Task<int> ObtenerTipoAgenciaSegunCiudadAsync(Ciudad ciudad)
        {
            var esLocal = ciudad.EsLocal;

            var tipos = await _context.TiposAgencia.AsNoTracking()
                .Where(t => t.Activo)
                .ToListAsync();

            var tipo = tipos.FirstOrDefault(t =>
                esLocal
                    ? t.Nombre.Equals("Local", StringComparison.OrdinalIgnoreCase)
                    : t.Nombre.Equals("Foránea", StringComparison.OrdinalIgnoreCase) ||
                      t.Nombre.Equals("Foranea", StringComparison.OrdinalIgnoreCase));

            if (tipo == null)
                throw new InvalidOperationException(
                    esLocal
                        ? "No existe un tipo de agencia Local activo."
                        : "No existe un tipo de agencia Foránea activo.");

            return tipo.Id;
        }

        private async Task PrepararDesactivacionAgenciaAsync(int agenciaId)
        {
            var mantenimientoEnProceso = await _context.Mantenimientos
                .AsNoTracking()
                .AnyAsync(m =>
                    m.IncluyeCambioBaterias &&
                    m.UPS != null &&
                    m.UPS.AgenciaId == agenciaId &&
                    m.EstadoMantenimiento != null &&
                    m.EstadoMantenimiento.Nombre == "En proceso");

            if (mantenimientoEnProceso)
                throw new InvalidOperationException(
                    "No se puede desactivar la agencia mientras tenga cambios de baterías En proceso.");

            var estadoResguardo = await _context.EstadosUPS
                .FirstOrDefaultAsync(e => e.Activo && e.Nombre == "En resguardo");

            if (estadoResguardo == null)
                throw new InvalidOperationException(
                    "No existe el estado de UPS 'En resguardo'. Reinicie la API para cargar los catálogos.");

            var equipos = await _context.UPSs
                .Include(u => u.EstadoUPS)
                .Where(u => u.AgenciaId == agenciaId)
                .ToListAsync();

            foreach (var equipo in equipos)
            {
                if (equipo.EstadoUPS?.Nombre.Equals(
                        "De baja", StringComparison.OrdinalIgnoreCase) == true)
                    continue;

                equipo.EstadoUPSId = estadoResguardo.Id;
                equipo.FechaModificacion = DateTime.Now;
            }
        }

        private static void ValidarDatosBasicos(Agencia agencia)
        {
            if (string.IsNullOrWhiteSpace(agencia.Codigo))
                throw new ArgumentException("El código de la agencia es obligatorio.");
            if (string.IsNullOrWhiteSpace(agencia.Nombre))
                throw new ArgumentException("El nombre de la agencia es obligatorio.");
            if (agencia.CiudadId <= 0)
                throw new ArgumentException("Debe seleccionar una ciudad.");
            if (string.IsNullOrWhiteSpace(agencia.Direccion))
                throw new ArgumentException("La dirección de la agencia es obligatoria.");
        }

        private static void LimpiarDatos(Agencia agencia)
        {
            agencia.Codigo = agencia.Codigo?.Trim() ?? string.Empty;
            agencia.Nombre = agencia.Nombre?.Trim() ?? string.Empty;
            agencia.Direccion = agencia.Direccion?.Trim() ?? string.Empty;
        }
    }
}
