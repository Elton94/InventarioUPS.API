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

        public AgenciaService(
            IAgenciaRepository repository,
            InventarioUPSDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        // =====================================================
        // OBTENER TODAS
        // =====================================================

        public async Task<List<Agencia>> ObtenerTodasAsync()
        {
            return await _repository
                .ObtenerTodasAsync();
        }

        // =====================================================
        // OBTENER POR ID
        // =====================================================

        public async Task<Agencia?> ObtenerPorIdAsync(int id)
        {
            return await _repository
                .ObtenerPorIdAsync(id);
        }

        // =====================================================
        // CREAR
        // =====================================================

        public async Task CrearAsync(Agencia agencia)
        {
            if (agencia == null)
            {
                throw new ArgumentException(
                    "La información de la agencia es obligatoria.");
            }

            LimpiarDatos(agencia);

            ValidarDatosBasicos(agencia);

            // -------------------------------------------------
            // VALIDAR CIUDAD
            // -------------------------------------------------

            var ciudad = await _context.Ciudades
                .AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.Id == agencia.CiudadId);

            if (ciudad == null)
            {
                throw new ArgumentException(
                    "La ciudad seleccionada no existe.");
            }

            if (!ciudad.Activa)
            {
                throw new InvalidOperationException(
                    "La ciudad seleccionada está inactiva.");
            }

            // -------------------------------------------------
            // VALIDAR TIPO DE AGENCIA
            // -------------------------------------------------

            var tipoAgencia = await _context.TiposAgencia
                .AsNoTracking()
                .FirstOrDefaultAsync(t =>
                    t.Id == agencia.TipoAgenciaId);

            if (tipoAgencia == null)
            {
                throw new ArgumentException(
                    "El tipo de agencia seleccionado no existe.");
            }

            if (!tipoAgencia.Activo)
            {
                throw new InvalidOperationException(
                    "El tipo de agencia seleccionado está inactivo.");
            }

            // -------------------------------------------------
            // VALIDAR CÓDIGO ÚNICO
            // -------------------------------------------------

            var codigoNormalizado =
                agencia.Codigo.ToUpper();

            var codigoExiste =
                await _context.Agencias
                    .AsNoTracking()
                    .AnyAsync(a =>
                        a.Codigo.ToUpper() ==
                        codigoNormalizado);

            if (codigoExiste)
            {
                throw new InvalidOperationException(
                    "Ya existe una agencia con ese código.");
            }

            // -------------------------------------------------
            // NUEVA AGENCIA SIEMPRE ACTIVA
            // -------------------------------------------------

            agencia.Activa = true;

            await _repository.CrearAsync(agencia);
        }

        // =====================================================
        // ACTUALIZAR
        // =====================================================

        public async Task ActualizarAsync(Agencia agencia)
        {
            if (agencia == null)
            {
                throw new ArgumentException(
                    "La información de la agencia es obligatoria.");
            }

            if (agencia.Id <= 0)
            {
                throw new ArgumentException(
                    "El identificador de la agencia no es válido.");
            }

            LimpiarDatos(agencia);

            ValidarDatosBasicos(agencia);

            // -------------------------------------------------
            // VALIDAR QUE LA AGENCIA EXISTA
            // -------------------------------------------------

            var agenciaExistente =
                await _repository.ObtenerPorIdAsync(
                    agencia.Id);

            if (agenciaExistente == null)
            {
                throw new InvalidOperationException(
                    "La agencia no existe.");
            }

            // -------------------------------------------------
            // VALIDAR CIUDAD
            // -------------------------------------------------

            var ciudad = await _context.Ciudades
                .AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.Id == agencia.CiudadId);

            if (ciudad == null)
            {
                throw new ArgumentException(
                    "La ciudad seleccionada no existe.");
            }

            if (!ciudad.Activa)
            {
                throw new InvalidOperationException(
                    "La ciudad seleccionada está inactiva.");
            }

            // -------------------------------------------------
            // VALIDAR TIPO DE AGENCIA
            // -------------------------------------------------

            var tipoAgencia = await _context.TiposAgencia
                .AsNoTracking()
                .FirstOrDefaultAsync(t =>
                    t.Id == agencia.TipoAgenciaId);

            if (tipoAgencia == null)
            {
                throw new ArgumentException(
                    "El tipo de agencia seleccionado no existe.");
            }

            if (!tipoAgencia.Activo)
            {
                throw new InvalidOperationException(
                    "El tipo de agencia seleccionado está inactivo.");
            }

            // -------------------------------------------------
            // VALIDAR CÓDIGO ÚNICO
            // -------------------------------------------------

            var codigoNormalizado =
                agencia.Codigo.ToUpper();

            var codigoExiste =
                await _context.Agencias
                    .AsNoTracking()
                    .AnyAsync(a =>
                        a.Id != agencia.Id &&
                        a.Codigo.ToUpper() ==
                        codigoNormalizado);

            if (codigoExiste)
            {
                throw new InvalidOperationException(
                    "Ya existe otra agencia con ese código.");
            }

            await _repository.ActualizarAsync(
                agencia);
        }

        // =====================================================
        // DESACTIVAR
        // =====================================================

        public async Task EliminarAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException(
                    "El identificador de la agencia no es válido.");
            }

            var agencia =
                await _repository.ObtenerPorIdAsync(id);

            if (agencia == null)
            {
                throw new InvalidOperationException(
                    "La agencia no existe.");
            }

            if (!agencia.Activa)
            {
                throw new InvalidOperationException(
                    "La agencia ya está desactivada.");
            }

            await _repository.EliminarAsync(
                agencia);
        }

        // =====================================================
        // VALIDACIONES BÁSICAS
        // =====================================================

        private static void ValidarDatosBasicos(
            Agencia agencia)
        {
            if (string.IsNullOrWhiteSpace(
                agencia.Codigo))
            {
                throw new ArgumentException(
                    "El código de la agencia es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(
                agencia.Nombre))
            {
                throw new ArgumentException(
                    "El nombre de la agencia es obligatorio.");
            }

            if (agencia.CiudadId <= 0)
            {
                throw new ArgumentException(
                    "Debe seleccionar una ciudad.");
            }

            if (agencia.TipoAgenciaId <= 0)
            {
                throw new ArgumentException(
                    "Debe seleccionar el tipo de agencia.");
            }
        }

        // =====================================================
        // LIMPIAR DATOS
        // =====================================================

        private static void LimpiarDatos(
            Agencia agencia)
        {
            agencia.Codigo =
                agencia.Codigo?.Trim()
                ?? string.Empty;

            agencia.Nombre =
                agencia.Nombre?.Trim()
                ?? string.Empty;

            agencia.Direccion =
                agencia.Direccion?.Trim()
                ?? string.Empty;
        }
    }
}