using System.Data;
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
    [PermisoAuthorize("VER_UPS", "CREAR_UPS", "EDITAR_UPS", "VER_MANTENIMIENTO", "CREAR_MANTENIMIENTO", "EDITAR_MANTENIMIENTO", "VER_PRESUPUESTO", "GESTIONAR_CATALOGOS")]
    public async Task<IActionResult> Catalogo() => Ok(new {
        tipos = await db.TiposBateria.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync()
    });

    [HttpGet("costos-tipo/{anio:int}")]
    [PermisoAuthorize("VER_PRESUPUESTO", "GESTIONAR_PRESUPUESTO")]
    public async Task<IActionResult> Costos(int anio)
    {
        if (anio < DateTime.Today.Year || anio > 2100) return BadRequest(new { mensaje = "El año debe ser actual o futuro, hasta 2100." });
        var referencia = await db.Set<ConfiguracionPresupuestoBaterias>().AsNoTracking().FirstOrDefaultAsync(x => x.Anio == anio && x.Activo);
        if (referencia == null) return BadRequest(new { mensaje = "Configure primero el valor de referencia para este año." });
        var costos = await db.CostosTipoBateria.AsNoTracking().Where(x => x.Anio == anio).ToDictionaryAsync(x => x.TipoBateriaId);
        var tipos = await db.TiposBateria.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync();
        return Ok(tipos.Select(x => new {
            TipoBateriaId = x.Id, x.Nombre, x.Activo,
            CostoUnitario = costos.TryGetValue(x.Id, out var costo) ? costo.CostoUnitario : referencia.CostoUnitarioBateria,
            UsaReferencia = !costos.ContainsKey(x.Id)
        }));
    }

    [HttpPut("costos-tipo/{anio:int}")]
    [PermisoAuthorize("GESTIONAR_PRESUPUESTO")]
    public async Task<IActionResult> GuardarCostos(int anio, List<CostoTipoRequest> request)
    {
        if (anio < DateTime.Today.Year || anio > 2100 || request == null || request.Count == 0 ||
            request.Select(x => x.TipoBateriaId).Distinct().Count() != request.Count ||
            request.Any(x => x.CostoUnitario is <= 0 or >= 10000000000000000m ||
                (x.CostoUnitario.HasValue && decimal.Round(x.CostoUnitario.Value, 2) != x.CostoUnitario)))
            return BadRequest(new { mensaje = "Use tipos sin repetir y costos positivos con máximo dos decimales." });
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try {
            if (!await db.Set<ConfiguracionPresupuestoBaterias>().AnyAsync(x => x.Anio == anio && x.Activo))
                return BadRequest(new { mensaje = "Configure primero el valor de referencia para este año." });
            var ids = request.Select(x => x.TipoBateriaId).ToList();
            if (await db.TiposBateria.CountAsync(x => ids.Contains(x.Id)) != ids.Count)
                return BadRequest(new { mensaje = "Uno de los tipos de batería no existe. Recargue el catálogo." });
            var existentes = await db.CostosTipoBateria.Where(x => x.Anio == anio && ids.Contains(x.TipoBateriaId)).ToDictionaryAsync(x => x.TipoBateriaId);
            foreach (var item in request) {
                existentes.TryGetValue(item.TipoBateriaId, out var costo);
                if (!item.CostoUnitario.HasValue) {
                    if (costo != null) db.CostosTipoBateria.Remove(costo);
                    continue;
                }
                if (costo == null) {
                    costo = new CostoTipoBateria { Anio = anio, TipoBateriaId = item.TipoBateriaId };
                    db.CostosTipoBateria.Add(costo);
                }
                costo.CostoUnitario = item.CostoUnitario.Value;
            }
            await db.SaveChangesAsync();
            await tx.CommitAsync();
            return Ok(new { mensaje = "Costos por tipo guardados." });
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 or 1205 })
        { return Conflict(new { mensaje = "Los costos cambiaron en otra sesión. Recargue e intente nuevamente." }); }
        catch (SqlException ex) when (ex.Number == 1205)
        { return Conflict(new { mensaje = "Los costos se están actualizando. Recargue e intente nuevamente." }); }
    }

    public record CostoTipoRequest(int TipoBateriaId, decimal? CostoUnitario);
}
