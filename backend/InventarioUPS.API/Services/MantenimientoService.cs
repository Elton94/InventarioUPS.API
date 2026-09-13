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

        public MantenimientoService(
            IMantenimientoRepository repository,
            InventarioUPSDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        // =====================================================
        // OBTENER TODOS
        // =====================================================

        public async Task<List<MantenimientoDTO>> ObtenerTodosAsync()
        {
            var mantenimientos =
                await _repository.ObtenerTodosAsync();

            return mantenimientos
                .Select(ConvertirDTO)
                .ToList();
        }

        // =====================================================
        // OBTENER POR ID
        // =====================================================

        public async Task<MantenimientoDTO?> ObtenerPorIdAsync(
            int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException(
                    "El identificador del mantenimiento no es válido.");
            }

            var mantenimiento =
                await _repository.ObtenerPorIdAsync(id);

            if (mantenimiento == null)
            {
                return null;
            }

            return ConvertirDTO(
                mantenimiento);
        }

        // =====================================================
        // OBTENER POR UPS
        // =====================================================

        public async Task<List<MantenimientoDTO>> ObtenerPorUPSAsync(
            int upsId)
        {
            if (upsId <= 0)
            {
                throw new ArgumentException(
                    "El UPS seleccionado no es válido.");
            }

            var mantenimientos =
                await _repository.ObtenerPorUPSAsync(
                    upsId);

            return mantenimientos
                .Select(ConvertirDTO)
                .ToList();
        }

        // =====================================================
        // CREAR
        // =====================================================

        public async Task CrearAsync(
            Mantenimiento mantenimiento)
        {
            ValidarDatosBasicos(
                mantenimiento);

            // -------------------------------------------------
            // UPS
            // -------------------------------------------------

            var ups =
                await _context.UPSs
                    .FirstOrDefaultAsync(
                        u =>
                            u.Id ==
                            mantenimiento.UPSId);

            if (ups == null)
            {
                throw new ArgumentException(
                    "El UPS seleccionado no existe.");
            }

            // -------------------------------------------------
            // TIPO
            // -------------------------------------------------

            var tipo =
                await _context.TiposMantenimiento
                    .FirstOrDefaultAsync(
                        t =>
                            t.Id ==
                            mantenimiento.TipoMantenimientoId);

            if (tipo == null)
            {
                throw new ArgumentException(
                    "El tipo de mantenimiento seleccionado no existe.");
            }

            if (!tipo.Activo)
            {
                throw new InvalidOperationException(
                    "El tipo de mantenimiento seleccionado está inactivo.");
            }

            // -------------------------------------------------
            // ESTADO
            // -------------------------------------------------

            var estado =
                await _context.EstadosMantenimiento
                    .FirstOrDefaultAsync(
                        e =>
                            e.Id ==
                            mantenimiento.EstadoMantenimientoId);

            if (estado == null)
            {
                throw new ArgumentException(
                    "El estado de mantenimiento seleccionado no existe.");
            }

            if (!estado.Activo)
            {
                throw new InvalidOperationException(
                    "El estado de mantenimiento seleccionado está inactivo.");
            }

            // -------------------------------------------------
            // FECHA
            // -------------------------------------------------

            ValidarFechaSegunEstado(
                mantenimiento.FechaMantenimiento,
                estado.Nombre);

            // -------------------------------------------------
            // CAMBIO DE BATERÍAS
            // -------------------------------------------------

            PrepararCambioBaterias(
                mantenimiento,
                ups);

            // -------------------------------------------------
            // LIMPIAR TEXTO
            // -------------------------------------------------

            mantenimiento.Descripcion =
                LimpiarTexto(
                    mantenimiento.Descripcion);

            mantenimiento.TecnicoProveedor =
                LimpiarTexto(
                    mantenimiento.TecnicoProveedor);

            mantenimiento.Observaciones =
                LimpiarTexto(
                    mantenimiento.Observaciones);

            // -------------------------------------------------
            // AGREGAR MANTENIMIENTO
            // -------------------------------------------------

            _context.Mantenimientos.Add(
                mantenimiento);

            // -------------------------------------------------
            // ACTUALIZAR UPS
            //
            // PROGRAMADO:
            // Guarda la planificación, pero NO modifica
            // FechaUltimoCambioBaterias.
            //
            // REALIZADO:
            // Actualiza la fecha del último cambio.
            // -------------------------------------------------

            if (
                mantenimiento.IncluyeCambioBaterias &&
                !EsEstadoProgramado(
                    estado.Nombre)
            )
            {
                ActualizarDatosBateriasUPS(
                    ups,
                    mantenimiento);
            }

            // -------------------------------------------------
            // GUARDAR TODO JUNTO
            // -------------------------------------------------

            await _context.SaveChangesAsync();
        }

        // =====================================================
        // ACTUALIZAR
        // =====================================================

        public async Task ActualizarAsync(
            Mantenimiento mantenimiento)
        {
            ValidarDatosBasicos(
                mantenimiento);

            // -------------------------------------------------
            // MANTENIMIENTO EXISTENTE
            // -------------------------------------------------

            var existente =
                await _context.Mantenimientos
                    .FirstOrDefaultAsync(
                        m =>
                            m.Id ==
                            mantenimiento.Id);

            if (existente == null)
            {
                throw new InvalidOperationException(
                    "El mantenimiento no existe.");
            }

            // -------------------------------------------------
            // UPS
            // -------------------------------------------------

            var ups =
                await _context.UPSs
                    .FirstOrDefaultAsync(
                        u =>
                            u.Id ==
                            mantenimiento.UPSId);

            if (ups == null)
            {
                throw new ArgumentException(
                    "El UPS seleccionado no existe.");
            }

            // -------------------------------------------------
            // TIPO
            // -------------------------------------------------

            var tipo =
                await _context.TiposMantenimiento
                    .FirstOrDefaultAsync(
                        t =>
                            t.Id ==
                            mantenimiento.TipoMantenimientoId);

            if (tipo == null)
            {
                throw new ArgumentException(
                    "El tipo de mantenimiento seleccionado no existe.");
            }

            if (!tipo.Activo)
            {
                throw new InvalidOperationException(
                    "El tipo de mantenimiento seleccionado está inactivo.");
            }

            // -------------------------------------------------
            // ESTADO
            // -------------------------------------------------

            var estado =
                await _context.EstadosMantenimiento
                    .FirstOrDefaultAsync(
                        e =>
                            e.Id ==
                            mantenimiento.EstadoMantenimientoId);

            if (estado == null)
            {
                throw new ArgumentException(
                    "El estado de mantenimiento seleccionado no existe.");
            }

            if (!estado.Activo)
            {
                throw new InvalidOperationException(
                    "El estado de mantenimiento seleccionado está inactivo.");
            }

            // -------------------------------------------------
            // FECHA
            // -------------------------------------------------

            ValidarFechaSegunEstado(
                mantenimiento.FechaMantenimiento,
                estado.Nombre);

            // -------------------------------------------------
            // CAMBIO DE BATERÍAS
            // -------------------------------------------------

            PrepararCambioBaterias(
                mantenimiento,
                ups);

            // -------------------------------------------------
            // LIMPIAR TEXTO
            // -------------------------------------------------

            mantenimiento.Descripcion =
                LimpiarTexto(
                    mantenimiento.Descripcion);

            mantenimiento.TecnicoProveedor =
                LimpiarTexto(
                    mantenimiento.TecnicoProveedor);

            mantenimiento.Observaciones =
                LimpiarTexto(
                    mantenimiento.Observaciones);

            // -------------------------------------------------
            // ACTUALIZAR MANTENIMIENTO
            // -------------------------------------------------

            existente.UPSId =
                mantenimiento.UPSId;

            existente.TipoMantenimientoId =
                mantenimiento.TipoMantenimientoId;

            existente.EstadoMantenimientoId =
                mantenimiento.EstadoMantenimientoId;

            existente.FechaMantenimiento =
                mantenimiento.FechaMantenimiento;

            existente.Descripcion =
                mantenimiento.Descripcion;

            existente.TecnicoProveedor =
                mantenimiento.TecnicoProveedor;

            existente.Costo =
                mantenimiento.Costo;

            existente.Observaciones =
                mantenimiento.Observaciones;

            existente.IncluyeCambioBaterias =
                mantenimiento.IncluyeCambioBaterias;

            existente.CantidadBateriasReemplazadas =
                mantenimiento.CantidadBateriasReemplazadas;

            existente.CostoBaterias =
                mantenimiento.CostoBaterias;

            existente.FechaModificacion =
                DateTime.Now;

            // -------------------------------------------------
            // ACTUALIZAR INVENTARIO DEL UPS
            // -------------------------------------------------

            if (
                mantenimiento.IncluyeCambioBaterias &&
                !EsEstadoProgramado(
                    estado.Nombre)
            )
            {
                ActualizarDatosBateriasUPS(
                    ups,
                    mantenimiento);
            }

            // -------------------------------------------------
            // GUARDAR
            // -------------------------------------------------

            await _context.SaveChangesAsync();
        }

        // =====================================================
        // VALIDACIONES BÁSICAS
        // =====================================================

        private static void ValidarDatosBasicos(
            Mantenimiento mantenimiento)
        {
            if (mantenimiento == null)
            {
                throw new ArgumentException(
                    "Los datos del mantenimiento son obligatorios.");
            }

            if (mantenimiento.UPSId <= 0)
            {
                throw new ArgumentException(
                    "Debe seleccionar un UPS.");
            }

            if (mantenimiento.TipoMantenimientoId <= 0)
            {
                throw new ArgumentException(
                    "Debe seleccionar un tipo de mantenimiento.");
            }

            if (mantenimiento.EstadoMantenimientoId <= 0)
            {
                throw new ArgumentException(
                    "Debe seleccionar un estado de mantenimiento.");
            }

            if (
                mantenimiento.FechaMantenimiento ==
                default)
            {
                throw new ArgumentException(
                    "La fecha del mantenimiento es obligatoria.");
            }

            if (
                string.IsNullOrWhiteSpace(
                    mantenimiento.Descripcion)
            )
            {
                throw new ArgumentException(
                    "La descripción del mantenimiento es obligatoria.");
            }

            if (
                string.IsNullOrWhiteSpace(
                    mantenimiento.TecnicoProveedor)
            )
            {
                throw new ArgumentException(
                    "El técnico o proveedor es obligatorio.");
            }

            if (mantenimiento.Costo < 0)
            {
                throw new ArgumentException(
                    "El costo del mantenimiento no puede ser negativo.");
            }
        }

        // =====================================================
        // VALIDAR FECHA SEGÚN ESTADO
        // =====================================================

        private static void ValidarFechaSegunEstado(
            DateTime fechaMantenimiento,
            string nombreEstado)
        {
            var esProgramado =
                EsEstadoProgramado(
                    nombreEstado);

            if (
                fechaMantenimiento.Date >
                DateTime.Now.Date &&
                !esProgramado
            )
            {
                throw new ArgumentException(
                    "Una fecha futura solamente puede utilizarse cuando el estado del mantenimiento es Programado.");
            }
        }

        // =====================================================
        // PREPARAR CAMBIO DE BATERÍAS
        // =====================================================

        private static void PrepararCambioBaterias(
            Mantenimiento mantenimiento,
            UPS ups)
        {
            // -------------------------------------------------
            // NO HAY CAMBIO
            // -------------------------------------------------

            if (!mantenimiento.IncluyeCambioBaterias)
            {
                mantenimiento
                    .CantidadBateriasReemplazadas = 0;

                mantenimiento
                    .CostoBaterias = 0;

                return;
            }

            // -------------------------------------------------
            // EL UPS DEBE TENER BATERÍAS REGISTRADAS
            // -------------------------------------------------

            if (ups.CantidadBaterias <= 0)
            {
                throw new InvalidOperationException(
                    "El UPS no tiene una cantidad válida de baterías registrada.");
            }

            // -------------------------------------------------
            // REGLA DE NEGOCIO:
            //
            // SIEMPRE SE REEMPLAZA EL BANCO COMPLETO.
            //
            // IGNORAMOS CUALQUIER CANTIDAD QUE LLEGUE
            // DESDE EL FRONTEND.
            // -------------------------------------------------

            mantenimiento
                .CantidadBateriasReemplazadas =
                ups.CantidadBaterias;

            // -------------------------------------------------
            // COSTO
            // -------------------------------------------------

            if (mantenimiento.CostoBaterias < 0)
            {
                throw new ArgumentException(
                    "El costo de las baterías no puede ser negativo.");
            }
        }

        // =====================================================
        // ACTUALIZAR DATOS DE BATERÍAS DEL UPS
        // =====================================================

        private static void ActualizarDatosBateriasUPS(
            UPS ups,
            Mantenimiento mantenimiento)
        {
            /*
             * La cantidad del UPS NO cambia porque estamos
             * reemplazando el banco completo por otro banco
             * de la misma cantidad.
             *
             * Solamente actualizamos la fecha del último cambio.
             */

            ups.FechaUltimoCambioBaterias =
                mantenimiento.FechaMantenimiento;
        }

        // =====================================================
        // ESTADO PROGRAMADO
        // =====================================================

        private static bool EsEstadoProgramado(
            string? nombreEstado)
        {
            return
                !string.IsNullOrWhiteSpace(
                    nombreEstado) &&
                nombreEstado
                    .Trim()
                    .Equals(
                        "Programado",
                        StringComparison.OrdinalIgnoreCase);
        }

        // =====================================================
        // LIMPIAR TEXTO
        // =====================================================

        private static string LimpiarTexto(
            string? valor)
        {
            return valor?.Trim()
                ?? string.Empty;
        }

        // =====================================================
        // ENTIDAD → DTO
        // =====================================================

        private static MantenimientoDTO ConvertirDTO(
            Mantenimiento mantenimiento)
        {
            return new MantenimientoDTO
            {
                Id =
                    mantenimiento.Id,

                UPSId =
                    mantenimiento.UPSId,

                TipoMantenimientoId =
                    mantenimiento.TipoMantenimientoId,

                EstadoMantenimientoId =
                    mantenimiento.EstadoMantenimientoId,

                FechaMantenimiento =
                    mantenimiento.FechaMantenimiento,

                Descripcion =
                    mantenimiento.Descripcion,

                TecnicoProveedor =
                    mantenimiento.TecnicoProveedor,

                Costo =
                    mantenimiento.Costo,

                Observaciones =
                    mantenimiento.Observaciones,

                IncluyeCambioBaterias =
                    mantenimiento
                        .IncluyeCambioBaterias,

                CantidadBateriasReemplazadas =
                    mantenimiento
                        .CantidadBateriasReemplazadas,

                CostoBaterias =
                    mantenimiento
                        .CostoBaterias,

                CodigoUPS =
                    mantenimiento
                        .UPS?
                        .Codigo,

                NumeroSerieUPS =
                    mantenimiento
                        .UPS?
                        .NumeroSerie,

                ModeloUPS =
                    mantenimiento
                        .UPS?
                        .Modelo,

                AgenciaNombre =
                    mantenimiento
                        .UPS?
                        .Agencia?
                        .Nombre,

                TipoMantenimiento =
                    mantenimiento
                        .Tipo?
                        .Nombre,

                Estado =
                    mantenimiento
                        .EstadoMantenimiento?
                        .Nombre
            };
        }
    }
}