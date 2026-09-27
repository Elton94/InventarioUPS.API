using InventarioUPS.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventarioUPS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CatalogosController : ControllerBase
    {
        private readonly InventarioUPSDbContext _context;

        public CatalogosController(InventarioUPSDbContext context)
        {
            _context = context;
        }

        [HttpGet("departamentos")]
        public async Task<IActionResult> ObtenerDepartamentos([FromQuery] int? actualId) =>
            Ok(await _context.Departamentos
                .Where(d => d.Activo || d.Id == actualId)
                .OrderBy(d => d.Nombre)
                .Select(d => new { id = d.Id, nombre = d.Nombre, activo = d.Activo })
                .ToListAsync());

        [HttpGet("ciudades")]
        public async Task<IActionResult> ObtenerCiudades([FromQuery] int? departamentoId, [FromQuery] int? actualId)
        {
            var consulta = _context.Ciudades.Where(c => (c.Activa && c.Departamento!.Activo) || c.Id == actualId);

            if (departamentoId.HasValue && departamentoId.Value > 0)
                consulta = consulta.Where(c => c.DepartamentoId == departamentoId.Value);

            return Ok(await consulta
                .OrderBy(c => c.Nombre)
                .Select(c => new
                {
                    id = c.Id,
                    nombre = c.Nombre,
                    activa = c.Activa && c.Departamento!.Activo, esLocal = c.EsLocal, departamentoId = c.DepartamentoId
                })
                .ToListAsync());
        }

        [HttpGet("tipos-agencia")]
        public async Task<IActionResult> ObtenerTiposAgencia() =>
            Ok(await _context.TiposAgencia
                .Where(t => t.Activo)
                .OrderBy(t => t.Nombre)
                .Select(t => new { id = t.Id, nombre = t.Nombre })
                .ToListAsync());

        [HttpGet("estados-ups")]
        public async Task<IActionResult> ObtenerEstadosUPS() =>
            Ok(await _context.EstadosUPS
                .Where(e => e.Activo)
                .OrderBy(e => e.Nombre)
                .Select(e => new { id = e.Id, nombre = e.Nombre })
                .ToListAsync());

        [HttpGet("estados-mantenimiento")]
        public async Task<IActionResult> ObtenerEstadosMantenimiento() =>
            Ok(await _context.EstadosMantenimiento
                .Where(e => e.Activo)
                .OrderBy(e => e.Nombre)
                .Select(e => new { id = e.Id, nombre = e.Nombre })
                .ToListAsync());

        [HttpGet("marcas-ups")]
        public async Task<IActionResult> ObtenerMarcasUPS() => Ok(await _context.MarcasUPS.AsNoTracking()
            .Where(x => x.Activo).OrderBy(x => x.Nombre).Select(x => x.Nombre).ToListAsync());

        [HttpGet("modelos-ups")]
        public async Task<IActionResult> ObtenerModelosUPS([FromQuery] string? marca)
        {
            var consulta = _context.ModelosUPS.AsNoTracking().Where(x => x.Activo && x.MarcaUPS!.Activo);
            if (!string.IsNullOrWhiteSpace(marca)) consulta = consulta.Where(x => x.MarcaUPS!.Nombre == marca.Trim());
            return Ok(await consulta.OrderBy(x => x.Nombre).Select(x => x.Nombre).ToListAsync());
        }

        [HttpGet("todos")]
        public async Task<IActionResult> ObtenerTodos()
        {
            var departamentos = await _context.Departamentos
                .Where(d => d.Activo)
                .OrderBy(d => d.Nombre)
                .Select(d => new { id = d.Id, nombre = d.Nombre, activo = d.Activo })
                .ToListAsync();

            var ciudades = await _context.Ciudades
                .Where(c => c.Activa && c.Departamento!.Activo)
                .OrderBy(c => c.Nombre)
                .Select(c => new { id = c.Id, nombre = c.Nombre, activa = c.Activa && c.Departamento!.Activo, esLocal = c.EsLocal, departamentoId = c.DepartamentoId })
                .ToListAsync();

            var tiposAgencia = await _context.TiposAgencia
                .Where(t => t.Activo)
                .OrderBy(t => t.Nombre)
                .Select(t => new { id = t.Id, nombre = t.Nombre })
                .ToListAsync();

            var estadosUPS = await _context.EstadosUPS
                .Where(e => e.Activo)
                .OrderBy(e => e.Nombre)
                .Select(e => new { id = e.Id, nombre = e.Nombre })
                .ToListAsync();

            var estadosMantenimiento = await _context.EstadosMantenimiento
                .Where(e => e.Activo)
                .OrderBy(e => e.Nombre)
                .Select(e => new { id = e.Id, nombre = e.Nombre })
                .ToListAsync();

            return Ok(new
            {
                departamentos,
                ciudades,
                tiposAgencia,
                estadosUPS,
                estadosMantenimiento
            });
        }
    }
}
