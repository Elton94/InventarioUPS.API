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

        // =====================================================
        // DEPARTAMENTOS
        // =====================================================

        [HttpGet("departamentos")]
        public async Task<IActionResult> ObtenerDepartamentos()
        {
            var departamentos = await _context.Departamentos
                .Where(d => d.Activo)
                .OrderBy(d => d.Nombre)
                .Select(d => new
                {
                    id = d.Id,
                    nombre = d.Nombre
                })
                .ToListAsync();

            return Ok(departamentos);
        }

        // =====================================================
        // CIUDADES
        // =====================================================

        [HttpGet("ciudades")]
        public async Task<IActionResult> ObtenerCiudades(
            [FromQuery] int? departamentoId)
        {
            var consulta = _context.Ciudades
                .Where(c => c.Activa);

            if (departamentoId.HasValue &&
                departamentoId.Value > 0)
            {
                consulta = consulta.Where(c =>
                    c.DepartamentoId == departamentoId.Value);
            }

            var ciudades = await consulta
                .OrderBy(c => c.Nombre)
                .Select(c => new
                {
                    id = c.Id,
                    nombre = c.Nombre,
                    departamentoId = c.DepartamentoId
                })
                .ToListAsync();

            return Ok(ciudades);
        }

        // =====================================================
        // TIPOS DE AGENCIA
        // =====================================================

        [HttpGet("tipos-agencia")]
        public async Task<IActionResult> ObtenerTiposAgencia()
        {
            var tipos = await _context.TiposAgencia
                .Where(t => t.Activo)
                .OrderBy(t => t.Nombre)
                .Select(t => new
                {
                    id = t.Id,
                    nombre = t.Nombre
                })
                .ToListAsync();

            return Ok(tipos);
        }

        // =====================================================
        // ESTADOS DE UPS
        // =====================================================

        [HttpGet("estados-ups")]
        public async Task<IActionResult> ObtenerEstadosUPS()
        {
            var estados = await _context.EstadosUPS
                .Where(e => e.Activo)
                .OrderBy(e => e.Nombre)
                .Select(e => new
                {
                    id = e.Id,
                    nombre = e.Nombre
                })
                .ToListAsync();

            return Ok(estados);
        }

        // =====================================================
        // TIPOS DE MANTENIMIENTO
        // =====================================================

        [HttpGet("tipos-mantenimiento")]
        public async Task<IActionResult> ObtenerTiposMantenimiento()
        {
            var tipos = await _context.TiposMantenimiento
                .Where(t => t.Activo)
                .OrderBy(t => t.Nombre)
                .Select(t => new
                {
                    id = t.Id,
                    nombre = t.Nombre
                })
                .ToListAsync();

            return Ok(tipos);
        }

        // =====================================================
        // ESTADOS DE MANTENIMIENTO
        // =====================================================

        [HttpGet("estados-mantenimiento")]
        public async Task<IActionResult> ObtenerEstadosMantenimiento()
        {
            var estados = await _context.EstadosMantenimiento
                .Where(e => e.Activo)
                .OrderBy(e => e.Nombre)
                .Select(e => new
                {
                    id = e.Id,
                    nombre = e.Nombre
                })
                .ToListAsync();

            return Ok(estados);
        }

        // =====================================================
        // RESUMEN DE CATÁLOGOS
        // =====================================================

        [HttpGet("todos")]
        public async Task<IActionResult> ObtenerTodos()
        {
            var departamentos = await _context.Departamentos
                .Where(d => d.Activo)
                .OrderBy(d => d.Nombre)
                .Select(d => new
                {
                    id = d.Id,
                    nombre = d.Nombre
                })
                .ToListAsync();

            var ciudades = await _context.Ciudades
                .Where(c => c.Activa)
                .OrderBy(c => c.Nombre)
                .Select(c => new
                {
                    id = c.Id,
                    nombre = c.Nombre,
                    departamentoId = c.DepartamentoId
                })
                .ToListAsync();

            var tiposAgencia = await _context.TiposAgencia
                .Where(t => t.Activo)
                .OrderBy(t => t.Nombre)
                .Select(t => new
                {
                    id = t.Id,
                    nombre = t.Nombre
                })
                .ToListAsync();

            var estadosUPS = await _context.EstadosUPS
                .Where(e => e.Activo)
                .OrderBy(e => e.Nombre)
                .Select(e => new
                {
                    id = e.Id,
                    nombre = e.Nombre
                })
                .ToListAsync();

            var tiposMantenimiento =
                await _context.TiposMantenimiento
                    .Where(t => t.Activo)
                    .OrderBy(t => t.Nombre)
                    .Select(t => new
                    {
                        id = t.Id,
                        nombre = t.Nombre
                    })
                    .ToListAsync();

            var estadosMantenimiento =
                await _context.EstadosMantenimiento
                    .Where(e => e.Activo)
                    .OrderBy(e => e.Nombre)
                    .Select(e => new
                    {
                        id = e.Id,
                        nombre = e.Nombre
                    })
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