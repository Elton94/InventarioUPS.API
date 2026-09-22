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
        public async Task<IActionResult> ObtenerDepartamentos() =>
            Ok(await _context.Departamentos
                .Where(d => d.Activo)
                .OrderBy(d => d.Nombre)
                .Select(d => new { id = d.Id, nombre = d.Nombre })
                .ToListAsync());

        [HttpGet("ciudades")]
        public async Task<IActionResult> ObtenerCiudades([FromQuery] int? departamentoId)
        {
            var consulta = _context.Ciudades.Where(c => c.Activa);

            if (departamentoId.HasValue && departamentoId.Value > 0)
                consulta = consulta.Where(c => c.DepartamentoId == departamentoId.Value);

            return Ok(await consulta
                .OrderBy(c => c.Nombre)
                .Select(c => new
                {
                    id = c.Id,
                    nombre = c.Nombre,
                    departamentoId = c.DepartamentoId
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

        [HttpGet("tipos-mantenimiento")]
        public async Task<IActionResult> ObtenerTiposMantenimiento() =>
            Ok(await _context.TiposMantenimiento
                .Where(t => t.Activo)
                .OrderBy(t => t.Nombre)
                .Select(t => new { id = t.Id, nombre = t.Nombre })
                .ToListAsync());

        [HttpGet("estados-mantenimiento")]
        public async Task<IActionResult> ObtenerEstadosMantenimiento() =>
            Ok(await _context.EstadosMantenimiento
                .Where(e => e.Activo)
                .OrderBy(e => e.Nombre)
                .Select(e => new { id = e.Id, nombre = e.Nombre })
                .ToListAsync());

        // El catálogo se obtiene de las marcas y modelos de los UPS registrados.
        [HttpGet("marcas-ups")]
        public async Task<IActionResult> ObtenerMarcasUPS()
        {
            var marcas = await _context.UPSs
                .AsNoTracking()
                .Where(u => u.Marca != "")
                .Select(u => u.Marca)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            return Ok(marcas);
        }

        [HttpGet("modelos-ups")]
        public async Task<IActionResult> ObtenerModelosUPS([FromQuery] string? marca)
        {
            var consulta = _context.UPSs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(marca))
            {
                var valor = marca.Trim().ToUpper();
                consulta = consulta.Where(u => u.Marca.ToUpper() == valor);
            }

            var modelos = await consulta
                .Where(u => u.Modelo != "")
                .Select(u => u.Modelo)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            return Ok(modelos);
        }

        [HttpGet("todos")]
        public async Task<IActionResult> ObtenerTodos()
        {
            var departamentos = await _context.Departamentos
                .Where(d => d.Activo)
                .OrderBy(d => d.Nombre)
                .Select(d => new { id = d.Id, nombre = d.Nombre })
                .ToListAsync();

            var ciudades = await _context.Ciudades
                .Where(c => c.Activa)
                .OrderBy(c => c.Nombre)
                .Select(c => new { id = c.Id, nombre = c.Nombre, departamentoId = c.DepartamentoId })
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

            var tiposMantenimiento = await _context.TiposMantenimiento
                .Where(t => t.Activo)
                .OrderBy(t => t.Nombre)
                .Select(t => new { id = t.Id, nombre = t.Nombre })
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
                tiposMantenimiento,
                estadosMantenimiento
            });
        }
    }
}
