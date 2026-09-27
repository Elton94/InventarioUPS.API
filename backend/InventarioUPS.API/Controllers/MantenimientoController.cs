using System.Security.Claims;
using InventarioUPS.API.Authorization;
using InventarioUPS.API.DTOs;
using InventarioUPS.API.Models;
using InventarioUPS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventarioUPS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MantenimientoController : ControllerBase
    {
        private readonly IMantenimientoService _service;

        public MantenimientoController(IMantenimientoService service)
        {
            _service = service;
        }

        [HttpGet]
        [PermisoAuthorize("VER_MANTENIMIENTO")]
        public async Task<ActionResult<List<MantenimientoDTO>>> ObtenerTodos() =>
            Ok(await _service.ObtenerTodosAsync());

        [HttpGet("{id:int}")]
        [PermisoAuthorize("VER_MANTENIMIENTO")]
        public async Task<ActionResult<MantenimientoDTO>> ObtenerPorId(int id)
        {
            if (id <= 0)
                return BadRequest(new { mensaje = "El identificador no es válido." });

            var resultado = await _service.ObtenerPorIdAsync(id);

            if (resultado == null)
                return NotFound(new { mensaje = "El cambio de baterías no existe." });

            return Ok(resultado);
        }

        [HttpGet("ups/{upsId:int}")]
        [PermisoAuthorize("VER_MANTENIMIENTO")]
        public async Task<ActionResult<List<MantenimientoDTO>>> ObtenerPorUPS(int upsId)
        {
            if (upsId <= 0)
                return BadRequest(new { mensaje = "El identificador del UPS no es válido." });

            return Ok(await _service.ObtenerPorUPSAsync(upsId));
        }

        [HttpPost]
        [PermisoAuthorize("CREAR_MANTENIMIENTO")]
        public async Task<ActionResult<MantenimientoDTO>> Crear([FromBody] MantenimientoDTO dto)
        {
            if (dto == null)
                return BadRequest(new { mensaje = "Los datos del cambio de baterías son obligatorios." });

            try
            {
                var mantenimiento = ConvertirEntidad(dto);
                await _service.CrearAsync(mantenimiento, int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!));
                var creado = await _service.ObtenerPorIdAsync(mantenimiento.Id);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = mantenimiento.Id }, creado);
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        [PermisoAuthorize("EDITAR_MANTENIMIENTO")]
        public async Task<ActionResult<MantenimientoDTO>> Actualizar(int id, [FromBody] MantenimientoDTO dto)
        {
            if (dto == null || id <= 0)
                return BadRequest(new { mensaje = "Los datos del cambio de baterías no son válidos." });

            try
            {
                var mantenimiento = ConvertirEntidad(dto);
                mantenimiento.Id = id;
                await _service.ActualizarAsync(mantenimiento, int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!));
                return Ok(await _service.ObtenerPorIdAsync(id));
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
        }

        private static Mantenimiento ConvertirEntidad(MantenimientoDTO dto) => new()
        {
            Id = dto.Id,
            UPSId = dto.UPSId,
            TipoBateriaId = dto.TipoBateriaId,
            VoltajeBateria = dto.VoltajeBateria,
            CapacidadBateriaAh = dto.CapacidadBateriaAh,
            EstadoMantenimientoId = dto.EstadoMantenimientoId,
            FechaMantenimiento = dto.FechaMantenimiento,
            ResponsableUsuarioId = dto.ResponsableUsuarioId,
            Observaciones = dto.Observaciones?.Trim() ?? string.Empty,
            CantidadBateriasReemplazadas = dto.CantidadBateriasReemplazadas
        };
    }
}
