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
                await _service.CrearAsync(mantenimiento);
                var creado = await _service.ObtenerPorIdAsync(mantenimiento.Id);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = mantenimiento.Id }, creado);
            }
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
                await _service.ActualizarAsync(mantenimiento);
                return Ok(await _service.ObtenerPorIdAsync(id));
            }
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
            EstadoMantenimientoId = dto.EstadoMantenimientoId,
            FechaMantenimiento = dto.FechaMantenimiento,
            Descripcion = dto.Descripcion?.Trim() ?? string.Empty,
            TecnicoProveedor = dto.TecnicoProveedor?.Trim() ?? string.Empty,
            Observaciones = dto.Observaciones?.Trim() ?? string.Empty,
            CantidadBateriasReemplazadas = dto.CantidadBateriasReemplazadas
        };
    }
}
