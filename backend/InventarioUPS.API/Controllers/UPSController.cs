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
    public class UPSController : ControllerBase
    {
        private readonly IUPSService _service;

        public UPSController(IUPSService service)
        {
            _service = service;
        }

        [HttpGet]
        [PermisoAuthorize("VER_UPS")]
        public async Task<ActionResult<List<UPSDTO>>> ObtenerTodas()
        {
            var equipos = await _service.ObtenerTodasAsync();
            return Ok(equipos.Select(MapearDTO).ToList());
        }

        [HttpGet("detalle")]
        [PermisoAuthorize("VER_UPS")]
        public async Task<ActionResult<UPSDTO>> ObtenerPorInventario([FromQuery] string numeroInventario)
        {
            if (string.IsNullOrWhiteSpace(numeroInventario))
                return BadRequest(new { mensaje = "El identificador del UPS no es válido." });

            var ups = await _service.ObtenerPorInventarioAsync(numeroInventario);

            if (ups == null)
                return NotFound(new { mensaje = "El UPS no existe." });

            return Ok(MapearDTO(ups));
        }

        [HttpPost]
        [PermisoAuthorize("CREAR_UPS")]
        public async Task<ActionResult> Crear([FromBody] UPS ups)
        {
            try
            {
                await _service.CrearAsync(ups);
                return Ok(new { mensaje = "UPS creado correctamente.", numeroInventario = ups.NumeroInventario });
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

        [HttpPut]
        [PermisoAuthorize("EDITAR_UPS")]
        public async Task<ActionResult> Actualizar([FromQuery] string numeroInventario, [FromBody] UPS ups)
        {
            if (ups == null || string.IsNullOrWhiteSpace(numeroInventario) ||
                !string.Equals(numeroInventario.Trim(), ups.NumeroInventario?.Trim(), StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { mensaje = "El número de inventario es la clave primaria y no puede cambiarse al editar." });

            try
            {
                await _service.ActualizarAsync(ups);
                return Ok(new { mensaje = "UPS actualizado correctamente." });
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

        [HttpPatch("estado")]
        [PermisoAuthorize("EDITAR_UPS")]
        public async Task<ActionResult> CambiarEstado([FromQuery] string numeroInventario, [FromQuery] string estado)
        {
            try
            {
                await _service.CambiarEstadoAsync(numeroInventario, estado);
                return Ok(new { mensaje = "Estado del UPS actualizado correctamente." });
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

        private static UPSDTO MapearDTO(UPS equipo) => new()
        {
            NumeroInventario = equipo.NumeroInventario,
            Marca = equipo.Marca,
            Modelo = equipo.Modelo,
            NumeroSerie = equipo.NumeroSerie,
            CapacidadVA = equipo.CapacidadVA,
            AgenciaId = equipo.AgenciaId,
            AgenciaCodigo = equipo.Agencia?.Codigo ?? string.Empty,
            AgenciaNombre = equipo.Agencia?.Nombre ?? string.Empty,
            CiudadNombre = equipo.Agencia?.Ciudad?.Nombre ?? string.Empty,
            DepartamentoNombre = equipo.Agencia?.Ciudad?.Departamento?.Nombre ?? string.Empty,
            EstadoUPSId = equipo.EstadoUPSId,
            Estado = equipo.EstadoUPS?.Nombre ?? string.Empty,
            FechaInstalacion = equipo.FechaInstalacion,
            CantidadBaterias = equipo.CantidadBaterias,
            FechaUltimoCambioBaterias = equipo.FechaUltimoCambioBaterias,
            FechaCreacion = equipo.FechaCreacion,
            FechaModificacion = equipo.FechaModificacion
        };
    }
}
