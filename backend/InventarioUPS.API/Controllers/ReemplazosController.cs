using InventarioUPS.API.Authorization;
using InventarioUPS.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventarioUPS.API.Controllers;

[ApiController, Route("api/reemplazos"), Authorize]
public class ReemplazosController(InventarioUPSDbContext db) : ControllerBase
{
    [HttpGet]
    [PermisoAuthorize("VER_UPS")]
    public async Task<IActionResult> Obtener()
    {
        var politica = await db.PoliticaReemplazoBaterias.AsNoTracking().OrderByDescending(x => x.Id).FirstOrDefaultAsync();
        if (politica == null) return NotFound(new { mensaje = "Configure la política de reemplazo para consultar vencimientos." });
        var equipos = await db.UPSs.AsNoTracking().Include(u => u.Agencia)!.ThenInclude(a => a!.Ciudad)
            .Include(u => u.EstadoUPS).Include(u => u.TipoBateria)
            .Where(u => u.Agencia!.Activa && u.EstadoUPS!.Nombre != "De baja" && u.EstadoUPS.Nombre != "Fuera de servicio")
            .ToListAsync();
        var hoy = DateTime.Today;
        var datos = new List<object>();
        var incompletos = 0;
        foreach (var u in equipos)
        {
            var local = u.Agencia!.Ciudad?.EsLocal == true;
            var meses = local ? politica.MesesReemplazoLocal : politica.MesesReemplazoForanea;
            var baseFecha = u.FechaUltimoCambioBaterias ?? u.FechaInstalacion;
            if (baseFecha == default || baseFecha.Date > hoy || baseFecha.Date < u.FechaInstalacion.Date || meses is < 1 or > 120 || u.CantidadBaterias <= 0) { incompletos++; continue; }
            var fecha = baseFecha.Date.AddMonths(meses);
            datos.Add(new { upsId = u.Id, u.NumeroInventario, agencia = u.Agencia.Nombre, fecha,
                dias = (fecha - hoy).Days, u.CantidadBaterias, u.VoltajeBateria, u.CapacidadBateriaAh,
                tipoBateria = u.TipoBateria?.Nombre ?? "Sin identificar" });
        }
        return Ok(new { fechaConsulta = hoy, incompletos, equipos = datos });
    }
}
