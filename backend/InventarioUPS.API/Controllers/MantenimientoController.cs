using InventarioUPS.API.DTOs;
using InventarioUPS.API.Models;
using InventarioUPS.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventarioUPS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MantenimientoController : ControllerBase
    {
        private readonly IMantenimientoService _service;

        public MantenimientoController(
            IMantenimientoService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<MantenimientoDTO>>>
            ObtenerTodos()
        {
            var mantenimientos =
                await _service.ObtenerTodosAsync();

            return Ok(mantenimientos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MantenimientoDTO>>
            ObtenerPorId(int id)
        {
            var mantenimiento =
                await _service.ObtenerPorIdAsync(id);

            if (mantenimiento == null)
                return NotFound();

            return Ok(mantenimiento);
        }

        [HttpGet("ups/{upsId}")]
        public async Task<ActionResult<List<MantenimientoDTO>>>
            ObtenerPorUPS(int upsId)
        {
            var mantenimientos =
                await _service.ObtenerPorUPSAsync(upsId);

            return Ok(mantenimientos);
        }

        [HttpPost]
        public async Task<ActionResult>
            Crear(Mantenimiento mantenimiento)
        {
            await _service.CrearAsync(
                mantenimiento
            );

            return Ok(
                "Mantenimiento registrado correctamente."
            );
        }

        [HttpPut("{id}")]
        public async Task<ActionResult>
            Actualizar(
                int id,
                Mantenimiento mantenimiento)
        {
            if (id != mantenimiento.Id)
                return BadRequest();

            await _service.ActualizarAsync(
                mantenimiento
            );

            return Ok(
                "Mantenimiento actualizado correctamente."
            );
        }
    }
}