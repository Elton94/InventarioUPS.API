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
    public class UPSController : ControllerBase
    {
        private readonly IUPSService _service;

        public UPSController(IUPSService service)
        {
            _service = service;
        }

        // =====================================================
        // CONSULTAR TODOS
        // =====================================================

        [HttpGet]
        [PermisoAuthorize("VER_UPS")]
        public async Task<ActionResult<List<UPS>>> ObtenerTodas()
        {
            var ups = await _service.ObtenerTodasAsync();

            return Ok(ups);
        }

        // =====================================================
        // CONSULTAR POR ID
        // =====================================================

        [HttpGet("{id:int}")]
        [PermisoAuthorize("VER_UPS")]
        public async Task<ActionResult<UPS>> ObtenerPorId(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "El identificador del UPS no es válido."
                });
            }

            var ups = await _service.ObtenerPorIdAsync(id);

            if (ups == null)
            {
                return NotFound(new
                {
                    mensaje = "El UPS no existe."
                });
            }

            return Ok(ups);
        }

        // =====================================================
        // CREAR
        // =====================================================

        [HttpPost]
        [PermisoAuthorize("CREAR_UPS")]
        public async Task<ActionResult> Crear(UPS ups)
        {
            if (ups == null)
            {
                return BadRequest(new
                {
                    mensaje = "La información del UPS es obligatoria."
                });
            }

            try
            {
                await _service.CrearAsync(ups);

                return Ok(new
                {
                    mensaje = "UPS creado correctamente."
                });
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
        // =====================================================

        [HttpPut("{id:int}")]
        [PermisoAuthorize("EDITAR_UPS")]
        public async Task<ActionResult> Actualizar(
            int id,
            UPS ups)
        {
            if (ups == null)
            {
                return BadRequest(new
                {
                    mensaje = "La información del UPS es obligatoria."
                });
            }

            if (id <= 0 || id != ups.Id)
            {
                return BadRequest(new
                {
                    mensaje = "El identificador del UPS no es válido."
                });
            }

            try
            {
                await _service.ActualizarAsync(ups);

                return Ok(new
                {
                    mensaje = "UPS actualizado correctamente."
                });
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
        // CAMBIAR ESTADO
        // =====================================================

        [HttpPatch("{id:int}/estado")]
        [PermisoAuthorize("EDITAR_UPS")]
        public async Task<ActionResult> CambiarEstado(
            int id,
            [FromQuery] string estado)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "El identificador del UPS no es válido."
                });
            }

            if (string.IsNullOrWhiteSpace(estado))
            {
                return BadRequest(new
                {
                    mensaje = "El estado es obligatorio."
                });
            }

            try
            {
                await _service.CambiarEstadoAsync(
                    id,
                    estado);

                return Ok(new
                {
                    mensaje =
                        "Estado del UPS actualizado correctamente."
                });
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
    }
}