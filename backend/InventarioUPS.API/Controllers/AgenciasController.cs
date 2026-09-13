using InventarioUPS.API.Authorization;
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

        public AgenciasController(
            IAgenciaService service)
        {
            _service = service;
        }

        // =====================================================
        // OBTENER TODAS LAS AGENCIAS
        // Permiso: VER_AGENCIAS
        // =====================================================

        [HttpGet]
        [PermisoAuthorize("VER_AGENCIAS")]
        public async Task<ActionResult<List<Agencia>>>
            ObtenerTodas()
        {
            var agencias =
                await _service.ObtenerTodasAsync();

            return Ok(agencias);
        }

        // =====================================================
        // OBTENER AGENCIA POR ID
        // Permiso: VER_AGENCIAS
        // =====================================================

        [HttpGet("{id:int}")]
        [PermisoAuthorize("VER_AGENCIAS")]
        public async Task<ActionResult<Agencia>>
            ObtenerPorId(int id)
        {
            if (id <= 0)
            {
                return BadRequest(
                    "El identificador de la agencia no es válido.");
            }

            var agencia =
                await _service.ObtenerPorIdAsync(id);

            if (agencia == null)
            {
                return NotFound(
                    "La agencia no existe.");
            }

            return Ok(agencia);
        }

        // =====================================================
        // CREAR AGENCIA
        // Permiso: CREAR_AGENCIAS
        // =====================================================

        [HttpPost]
        [PermisoAuthorize("CREAR_AGENCIAS")]
        public async Task<ActionResult>
            Crear([FromBody] Agencia agencia)
        {
            if (agencia == null)
            {
                return BadRequest(
                    "La información de la agencia es obligatoria.");
            }

            try
            {
                await _service.CrearAsync(
                    agencia);

                return Ok(
                    "Agencia creada correctamente.");
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    mensaje = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
        }

        // =====================================================
        // ACTUALIZAR AGENCIA
        // Permiso: EDITAR_AGENCIAS
        // =====================================================

        [HttpPut("{id:int}")]
        [PermisoAuthorize("EDITAR_AGENCIAS")]
        public async Task<ActionResult>
            Actualizar(
                int id,
                [FromBody] Agencia agencia)
        {
            if (agencia == null)
            {
                return BadRequest(
                    "La información de la agencia es obligatoria.");
            }

            if (id <= 0)
            {
                return BadRequest(
                    "El identificador de la agencia no es válido.");
            }

            if (id != agencia.Id)
            {
                return BadRequest(
                    "El identificador de la URL no coincide con el de la agencia.");
            }

            try
            {
                await _service.ActualizarAsync(
                    agencia);

                return Ok(
                    "Agencia actualizada correctamente.");
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    mensaje = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
        }

        // =====================================================
        // DESACTIVAR AGENCIA
        // Permiso: EDITAR_AGENCIAS
        // =====================================================

        [HttpDelete("{id:int}")]
        [PermisoAuthorize("EDITAR_AGENCIAS")]
        public async Task<ActionResult>
            Eliminar(int id)
        {
            if (id <= 0)
            {
                return BadRequest(
                    "El identificador de la agencia no es válido.");
            }

            try
            {
                await _service.EliminarAsync(id);

                return Ok(
                    "Agencia desactivada correctamente.");
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    mensaje = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
        }
    }
}