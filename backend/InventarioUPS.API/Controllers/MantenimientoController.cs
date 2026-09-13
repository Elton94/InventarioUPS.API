using InventarioUPS.API.Authorization;
using InventarioUPS.API.DTOs;
using InventarioUPS.API.Models;
using InventarioUPS.API.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;

namespace InventarioUPS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MantenimientoController : ControllerBase
    {
        private readonly IMantenimientoService _service;

        public MantenimientoController(
            IMantenimientoService service)
        {
            _service = service;
        }

        // =====================================================
        // CONSULTAR TODOS
        // Permiso: VER_MANTENIMIENTO
        // =====================================================

        [HttpGet]
        [PermisoAuthorize("VER_MANTENIMIENTO")]
        public async Task<ActionResult<List<MantenimientoDTO>>>
            ObtenerTodos()
        {
            var resultado =
                await _service.ObtenerTodosAsync();

            return Ok(resultado);
        }

        // =====================================================
        // CONSULTAR POR ID
        // Permiso: VER_MANTENIMIENTO
        // =====================================================

        [HttpGet("{id:int}")]
        [PermisoAuthorize("VER_MANTENIMIENTO")]
        public async Task<ActionResult<MantenimientoDTO>>
            ObtenerPorId(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El identificador no es válido."
                });
            }

            var resultado =
                await _service.ObtenerPorIdAsync(id);

            if (resultado == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "El mantenimiento no existe."
                });
            }

            return Ok(resultado);
        }

        // =====================================================
        // CONSULTAR MANTENIMIENTOS DE UN UPS
        // Permiso: VER_MANTENIMIENTO
        // =====================================================

        [HttpGet("ups/{upsId:int}")]
        [PermisoAuthorize("VER_MANTENIMIENTO")]
        public async Task<ActionResult<List<MantenimientoDTO>>>
            ObtenerPorUPS(int upsId)
        {
            if (upsId <= 0)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El identificador del UPS no es válido."
                });
            }

            var resultado =
                await _service.ObtenerPorUPSAsync(
                    upsId);

            return Ok(resultado);
        }

        // =====================================================
        // CREAR
        // Permiso: CREAR_MANTENIMIENTO
        // =====================================================

        [HttpPost]
        [PermisoAuthorize("CREAR_MANTENIMIENTO")]
        public async Task<ActionResult<MantenimientoDTO>>
            Crear(
                [FromBody] MantenimientoDTO dto)
        {
            if (dto == null)
            {
                return BadRequest(new
                {
                    mensaje =
                        "Los datos del mantenimiento son obligatorios."
                });
            }

            try
            {
                // ---------------------------------------------
                // DETERMINAR TÉCNICO / PROVEEDOR
                // ---------------------------------------------

                dto.TecnicoProveedor =
                    ObtenerTecnicoParaGuardar(
                        dto.TecnicoProveedor);

                // ---------------------------------------------
                // CONVERTIR DTO A ENTIDAD
                // ---------------------------------------------

                var mantenimiento =
                    ConvertirEntidad(dto);

                await _service.CrearAsync(
                    mantenimiento);

                var creado =
                    await _service.ObtenerPorIdAsync(
                        mantenimiento.Id);

                return CreatedAtAction(
                    nameof(ObtenerPorId),
                    new
                    {
                        id = mantenimiento.Id
                    },
                    creado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    mensaje = ex.Message
                });
            }
        }

        // =====================================================
        // ACTUALIZAR
        // Permiso: EDITAR_MANTENIMIENTO
        // =====================================================

        [HttpPut("{id:int}")]
        [PermisoAuthorize("EDITAR_MANTENIMIENTO")]
        public async Task<ActionResult<MantenimientoDTO>>
            Actualizar(
                int id,
                [FromBody] MantenimientoDTO dto)
        {
            if (dto == null)
            {
                return BadRequest(new
                {
                    mensaje =
                        "Los datos del mantenimiento son obligatorios."
                });
            }

            if (id <= 0)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El identificador no es válido."
                });
            }

            try
            {
                // ---------------------------------------------
                // DETERMINAR TÉCNICO / PROVEEDOR
                // ---------------------------------------------

                dto.TecnicoProveedor =
                    ObtenerTecnicoParaGuardar(
                        dto.TecnicoProveedor);

                // ---------------------------------------------
                // CONVERTIR DTO A ENTIDAD
                // ---------------------------------------------

                var mantenimiento =
                    ConvertirEntidad(dto);

                mantenimiento.Id = id;

                await _service.ActualizarAsync(
                    mantenimiento);

                var actualizado =
                    await _service.ObtenerPorIdAsync(
                        id);

                return Ok(actualizado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    mensaje = ex.Message
                });
            }
        }

        // =====================================================
        // DETERMINAR TÉCNICO
        // =====================================================

        private string ObtenerTecnicoParaGuardar(
            string? tecnicoEnviado)
        {
            // -------------------------------------------------
            // TÉCNICO
            // -------------------------------------------------

            if (User.IsInRole("Técnico"))
            {
                var nombreCompleto =
                    User.FindFirst(
                        "NombreCompleto")
                    ?.Value;

                if (!string.IsNullOrWhiteSpace(
                        nombreCompleto))
                {
                    return nombreCompleto.Trim();
                }

                var nombreUsuario =
                    User.FindFirst(
                        ClaimTypes.Name)
                    ?.Value;

                if (!string.IsNullOrWhiteSpace(
                        nombreUsuario))
                {
                    return nombreUsuario.Trim();
                }

                throw new InvalidOperationException(
                    "No se pudo determinar el usuario que registra el mantenimiento.");
            }

            // -------------------------------------------------
            // ADMINISTRADOR / CONTROL TOTAL
            // -------------------------------------------------

            if (
                User.IsInRole("Administrador") ||
                User.IsInRole("Control Total")
            )
            {
                if (string.IsNullOrWhiteSpace(
                        tecnicoEnviado))
                {
                    throw new ArgumentException(
                        "Debe seleccionar el técnico o proveedor responsable.");
                }

                return tecnicoEnviado.Trim();
            }

            // -------------------------------------------------
            // CUALQUIER OTRO CASO
            // -------------------------------------------------

            throw new UnauthorizedAccessException(
                "El usuario no tiene un rol autorizado para registrar el técnico.");
        }

        // =====================================================
        // DTO → ENTIDAD
        // =====================================================

        private static Mantenimiento ConvertirEntidad(
            MantenimientoDTO dto)
        {
            return new Mantenimiento
            {
                Id =
                    dto.Id,

                UPSId =
                    dto.UPSId,

                TipoMantenimientoId =
                    dto.TipoMantenimientoId,

                EstadoMantenimientoId =
                    dto.EstadoMantenimientoId,

                FechaMantenimiento =
                    dto.FechaMantenimiento,

                Descripcion =
                    dto.Descripcion?.Trim()
                    ?? string.Empty,

                TecnicoProveedor =
                    dto.TecnicoProveedor?.Trim()
                    ?? string.Empty,

                Costo =
                    dto.Costo,

                Observaciones =
                    dto.Observaciones?.Trim()
                    ?? string.Empty,

                // =============================================
                // CAMBIO DE BATERÍAS
                // =============================================

                IncluyeCambioBaterias =
                    dto.IncluyeCambioBaterias,

                CantidadBateriasReemplazadas =
                    dto.CantidadBateriasReemplazadas,

                CostoBaterias =
                    dto.CostoBaterias
            };
        }
    }
}