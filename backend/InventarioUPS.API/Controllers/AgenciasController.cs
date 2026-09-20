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
    public class AgenciasController : ControllerBase
    {
        private readonly IAgenciaService _service;

        public AgenciasController(IAgenciaService service)
        {
            _service = service;
        }

        [HttpGet]
        [PermisoAuthorize("VER_AGENCIAS")]
        public async Task<ActionResult<List<AgenciaDTO>>> ObtenerTodas()
        {
            var agencias = await _service.ObtenerTodasAsync();
            return Ok(agencias.Select(MapearDTO).ToList());
        }

        [HttpGet("{id:int}")]
        [PermisoAuthorize("VER_AGENCIAS")]
        public async Task<ActionResult<AgenciaDTO>> ObtenerPorId(int id)
        {
            if (id <= 0)
                return BadRequest(new { mensaje = "El identificador de la agencia no es válido." });

            var agencia = await _service.ObtenerPorIdAsync(id);

            if (agencia == null)
                return NotFound(new { mensaje = "La agencia no existe." });

            return Ok(MapearDTO(agencia));
        }

        [HttpPost]
        [PermisoAuthorize("CREAR_AGENCIAS")]
        public async Task<ActionResult> Crear([FromBody] Agencia agencia)
        {
            if (agencia == null)
                return BadRequest(new { mensaje = "La información de la agencia es obligatoria." });

            try
            {
                await _service.CrearAsync(agencia);
                return Ok(new { mensaje = "Agencia creada correctamente.", agenciaId = agencia.Id });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        [PermisoAuthorize("EDITAR_AGENCIAS")]
        public async Task<ActionResult> Actualizar(int id, [FromBody] Agencia agencia)
        {
            if (agencia == null)
                return BadRequest(new { mensaje = "La información de la agencia es obligatoria." });

            if (id <= 0 || id != agencia.Id)
                return BadRequest(new { mensaje = "El identificador de la agencia no es válido." });

            try
            {
                await _service.ActualizarAsync(agencia);
                return Ok(new { mensaje = "Agencia actualizada correctamente." });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPatch("{id:int}/estado")]
        [PermisoAuthorize("EDITAR_AGENCIAS")]
        public async Task<ActionResult> CambiarEstado(int id, [FromQuery] bool activa)
        {
            if (id <= 0)
                return BadRequest(new { mensaje = "El identificador de la agencia no es válido." });

            try
            {
                await _service.CambiarEstadoAsync(id, activa);
                return Ok(new
                {
                    mensaje = activa
                        ? "Agencia activada correctamente."
                        : "Agencia desactivada correctamente. Sus UPS disponibles quedaron En resguardo."
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        [PermisoAuthorize("EDITAR_AGENCIAS")]
        public async Task<ActionResult> Eliminar(int id)
        {
            try
            {
                await _service.CambiarEstadoAsync(id, false);
                return Ok(new { mensaje = "Agencia desactivada correctamente." });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        private static AgenciaDTO MapearDTO(Agencia agencia)
        {
            return new AgenciaDTO
            {
                Id = agencia.Id,
                Codigo = agencia.Codigo,
                Nombre = agencia.Nombre,
                CiudadId = agencia.CiudadId,
                CiudadNombre = agencia.Ciudad?.Nombre ?? string.Empty,
                DepartamentoId = agencia.Ciudad?.DepartamentoId ?? 0,
                DepartamentoNombre = agencia.Ciudad?.Departamento?.Nombre ?? string.Empty,
                Direccion = agencia.Direccion,
                TipoAgenciaId = agencia.TipoAgenciaId,
                TipoAgenciaNombre = agencia.TipoAgencia?.Nombre ?? string.Empty,
                Activa = agencia.Activa,
                FechaCreacion = agencia.FechaCreacion,
                FechaModificacion = agencia.FechaModificacion
            };
        }
    }
}
