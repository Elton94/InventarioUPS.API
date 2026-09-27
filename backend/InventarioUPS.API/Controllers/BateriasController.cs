using InventarioUPS.API.Authorization;
using InventarioUPS.API.Data;
using InventarioUPS.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace InventarioUPS.API.Controllers;

[ApiController, Route("api/baterias"), Authorize]
public class BateriasController(InventarioUPSDbContext db) : ControllerBase
{
    [HttpGet]
    [PermisoAuthorize("VER_UPS", "CREAR_UPS", "EDITAR_UPS", "VER_MANTENIMIENTO", "CREAR_MANTENIMIENTO", "EDITAR_MANTENIMIENTO", "VER_PRESUPUESTO")]
    public async Task<IActionResult> Catalogo() => Ok(new {
        tipos = await db.TiposBateria.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync(),
        modelos = await db.ModelosBateria.AsNoTracking().OrderBy(x => x.Nombre)
            .Select(x => new { x.Id, x.Nombre, x.TipoBateriaId, Tipo = x.TipoBateria!.Nombre }).ToListAsync()
    });

    [HttpPost("modelos")]
    [PermisoAuthorize("CREAR_UPS", "EDITAR_UPS", "CREAR_MANTENIMIENTO", "EDITAR_MANTENIMIENTO")]
    public async Task<IActionResult> CrearModelo(ModeloRequest request)
    {
        var nombre = request.Nombre?.Trim() ?? "";
        if (nombre.Length is < 1 or > 150 || !await db.TiposBateria.AnyAsync(x => x.Id == request.TipoBateriaId))
            return BadRequest(new { mensaje = "Seleccione un tipo y escriba un modelo de 1 a 150 caracteres." });
        var existente = await db.ModelosBateria.FirstOrDefaultAsync(x => x.TipoBateriaId == request.TipoBateriaId && x.Nombre == nombre);
        if (existente != null) return Ok(new { existente.Id, existente.Nombre, existente.TipoBateriaId });
        var modelo = new ModeloBateria { Nombre = nombre, TipoBateriaId = request.TipoBateriaId };
        db.ModelosBateria.Add(modelo);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            return Conflict(new { mensaje = "El modelo ya fue registrado. Actualice el catálogo y selecciónelo." });
        }
        return Ok(new { modelo.Id, modelo.Nombre, modelo.TipoBateriaId });
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
