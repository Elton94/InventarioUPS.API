using System.Security.Claims;
using InventarioUPS.API.Authorization;
using InventarioUPS.API.Data;
using InventarioUPS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace InventarioUPS.API.Controllers;

[ApiController, Route("api/configuracion"), Authorize, PermisoAuthorize("GESTIONAR_CATALOGOS")]
public class ConfiguracionController(InventarioUPSDbContext db) : ControllerBase
{
    [HttpGet("catalogos/{catalogo}")]
    public async Task<IActionResult> Listar(string catalogo)
    {
        try { return Ok(await new CatalogoAdminService(db).ListarAsync(catalogo)); }
        catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
    }
    [HttpPost("catalogos/{catalogo}")]
    public Task<IActionResult> Crear(string catalogo, GuardarCatalogo solicitud) => Guardar(catalogo, 0, solicitud);
    [HttpPut("catalogos/{catalogo}/{id:int}")]
    public Task<IActionResult> Editar(string catalogo, int id, GuardarCatalogo solicitud) => id <= 0
        ? Task.FromResult<IActionResult>(BadRequest(new { mensaje = "El identificador no es válido." })) : Guardar(catalogo, id, solicitud);
    private async Task<IActionResult> Guardar(string catalogo, int id, GuardarCatalogo solicitud)
    {
        try {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(await new CatalogoAdminService(db).GuardarAsync(catalogo, id, solicitud, usuarioId, User.Identity?.Name ?? $"Usuario {usuarioId}"));
        }
        catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new { mensaje = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { mensaje = ex.Message }); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 or 1205 })
        { return Conflict(new { mensaje = "Otra operación modificó el catálogo o el nombre ya existe. Actualice la lista." }); }
        catch (SqlException ex) when (ex.Number == 1205)
        { return Conflict(new { mensaje = "El catálogo se está actualizando en otra sesión. Recargue e intente nuevamente." }); }
    }
    [HttpGet("historial")]
    public async Task<IActionResult> Historial([FromQuery] int pagina = 1)
    {
        if (pagina < 1 || pagina > 100000) return BadRequest();
        var query = db.AuditoriaCatalogos.AsNoTracking();
        return Ok(new { total = await query.CountAsync(), registros = await query.OrderByDescending(x => x.Id).Skip((pagina - 1) * 25).Take(25).ToListAsync() });
    }
}
