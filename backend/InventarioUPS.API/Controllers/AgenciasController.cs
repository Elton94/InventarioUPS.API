using InventarioUPS.API.Models;
using InventarioUPS.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventarioUPS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgenciasController : ControllerBase
    {
        private readonly IAgenciaService _service;

        public AgenciasController(IAgenciaService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<Agencia>>> ObtenerTodas()
        {
            var agencias = await _service.ObtenerTodasAsync();
            return Ok(agencias);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Agencia>> ObtenerPorId(int id)
        {
            var agencia = await _service.ObtenerPorIdAsync(id);

            if (agencia == null)
                return NotFound();

            return Ok(agencia);
        }

        [HttpPost]
        public async Task<ActionResult> Crear(Agencia agencia)
        {
            await _service.CrearAsync(agencia);
            return Ok("Agencia creada correctamente.");
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Actualizar(int id, Agencia agencia)
        {
            if (id != agencia.Id)
                return BadRequest();

            await _service.ActualizarAsync(agencia);

            return Ok("Agencia actualizada.");
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Eliminar(int id)
        {
            await _service.EliminarAsync(id);

            return Ok("Agencia eliminada.");
        }
    }
}