using InventarioUPS.API.Models;
using InventarioUPS.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventarioUPS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UPSController : ControllerBase
    {
        private readonly IUPSService _service;

        public UPSController(IUPSService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<UPS>>> ObtenerTodas()
        {
            var ups = await _service.ObtenerTodasAsync();

            return Ok(ups);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UPS>> ObtenerPorId(int id)
        {
            var ups = await _service.ObtenerPorIdAsync(id);

            if (ups == null)
                return NotFound();

            return Ok(ups);
        }

        [HttpPost]
        public async Task<ActionResult> Crear(UPS ups)
        {
            await _service.CrearAsync(ups);

            return Ok("UPS creado correctamente.");
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Actualizar(
            int id,
            UPS ups)
        {
            if (id != ups.Id)
                return BadRequest();

            await _service.ActualizarAsync(ups);

            return Ok("UPS actualizado.");
        }

        [HttpPatch("{id}/estado")]
        public async Task<ActionResult> CambiarEstado(
            int id,
            [FromQuery] string estado)
        {
            await _service.CambiarEstadoAsync(
                id,
                estado
            );

            return Ok(
                "Estado del UPS actualizado correctamente."
            );
        }
    }
}