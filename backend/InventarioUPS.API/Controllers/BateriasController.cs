using InventarioUPS.API.Authorization;
using InventarioUPS.API.Data;
using InventarioUPS.API.Models;
using InventarioUPS.API.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace InventarioUPS.API.Controllers;

[ApiController, Route("api/baterias"), Authorize]
public class BateriasController(InventarioUPSDbContext db) : ControllerBase
{
    [HttpGet]
    [PermisoAuthorize("VER_UPS", "CREAR_UPS", "EDITAR_UPS", "VER_MANTENIMIENTO", "CREAR_MANTENIMIENTO", "EDITAR_MANTENIMIENTO", "VER_PRESUPUESTO", "GESTIONAR_CATALOGOS")]
    public async Task<IActionResult> Catalogo() => Ok(new {
        tipos = await db.TiposBateria.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync(),
        modelos = await db.ModelosBateria.AsNoTracking().OrderBy(x => x.Nombre)
            .Select(x => new { x.Id, x.Nombre, x.TipoBateriaId, Tipo = x.TipoBateria!.Nombre, Activo = x.Activo && x.TipoBateria.Activo }).ToListAsync()
    });

    [HttpPost("modelos")]
    [PermisoAuthorize("GESTIONAR_CATALOGOS")]
    public async Task<IActionResult> CrearModelo(ModeloRequest request)
    {
        try {
            var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var modelo = await new CatalogoAdminService(db).GuardarAsync("modelos-bateria", 0,
                new GuardarCatalogo(request.Nombre, true, request.TipoBateriaId), id, User.Identity?.Name ?? $"Usuario {id}");
            return Ok(new { modelo.Id, modelo.Nombre, TipoBateriaId = modelo.PadreId, Activo = true });
        }
        catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { mensaje = ex.Message }); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 or 1205 })
        { return Conflict(new { mensaje = "El catálogo cambió. Actualícelo e intente nuevamente." }); }
        catch (SqlException ex) when (ex.Number == 1205)
        { return Conflict(new { mensaje = "El catálogo se está actualizando en otra sesión. Recargue e intente nuevamente." }); }
    }

    [HttpGet("costos/{anio:int}")]
    [PermisoAuthorize("VER_PRESUPUESTO")]
    public async Task<IActionResult> Costos(int anio) => Ok(await db.CostosModeloBateria.AsNoTracking()
        .Where(x => x.Anio == anio).Select(x => new { x.ModeloBateriaId, x.CostoUnitario }).ToListAsync());

    [HttpPut("costos/{anio:int}/{modeloId:int}")]
    [PermisoAuthorize("GESTIONAR_PRESUPUESTO")]
    public async Task<IActionResult> GuardarCosto(int anio, int modeloId, CostoRequest request)
    {
        if (anio < DateTime.Today.Year || anio > 2100 || request.CostoUnitario <= 0 ||
            request.CostoUnitario >= 10000000000000000m || decimal.Round(request.CostoUnitario, 2) != request.CostoUnitario)
            return BadRequest(new { mensaje = "Use un año actual o futuro y un costo positivo con máximo dos decimales." });
        if (!await db.ModelosBateria.AnyAsync(x => x.Id == modeloId))
            return BadRequest(new { mensaje = "El modelo de batería no existe." });
        var costo = await db.CostosModeloBateria.FirstOrDefaultAsync(x => x.Anio == anio && x.ModeloBateriaId == modeloId);
        if (costo == null) {
            costo = new CostoModeloBateria { Anio = anio, ModeloBateriaId = modeloId };
            db.CostosModeloBateria.Add(costo);
        }
        costo.CostoUnitario = request.CostoUnitario;
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        { return Conflict(new { mensaje = "El costo cambió en otra sesión. Recargue e intente nuevamente." }); }
        return Ok(new { mensaje = "Costo por modelo guardado." });
    }

    public record ModeloRequest(int TipoBateriaId, string Nombre);
    public record CostoRequest(decimal CostoUnitario);
}
