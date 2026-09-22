using InventarioUPS.API.Authorization;
using InventarioUPS.API.Data;
using InventarioUPS.API.DTOs.Presupuesto;
using InventarioUPS.API.Models;
using InventarioUPS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventarioUPS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PresupuestoController : ControllerBase
    {
        private readonly InventarioUPSDbContext _context;
        private readonly IConfiguracionPresupuestoService _configuracionService;

        public PresupuestoController(
            InventarioUPSDbContext context,
            IConfiguracionPresupuestoService configuracionService)
        {
            _context = context;
            _configuracionService = configuracionService;
        }

        [HttpGet("configuracion")]
        [PermisoAuthorize("VER_PRESUPUESTO")]
        public async Task<ActionResult> ObtenerConfiguraciones() =>
            Ok(await _configuracionService.ObtenerTodasAsync());

        [HttpGet("configuracion/{anio:int}")]
        [PermisoAuthorize("VER_PRESUPUESTO")]
        public async Task<ActionResult> ObtenerConfiguracion(int anio)
        {
            if (anio < 2020 || anio > 2100)
                return BadRequest(new { mensaje = "El año debe estar entre 2020 y 2100." });

            var configuracion = await _configuracionService.ObtenerPorAnioAsync(anio);

            if (configuracion == null)
                return NotFound(new
                {
                    mensaje = $"No existe una configuración de costo para el año {anio}."
                });

            return Ok(configuracion);
        }

        [HttpPost("configuracion")]
        [PermisoAuthorize("GESTIONAR_PRESUPUESTO")]
        public async Task<ActionResult> CrearConfiguracion(
            [FromBody] CrearConfiguracionPresupuestoDto dto)
        {
            try
            {
                await _configuracionService.CrearAsync(dto);
                return Ok(new { mensaje = "Configuración anual creada correctamente." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
        }

        [HttpPut("configuracion/{id:int}")]
        [PermisoAuthorize("GESTIONAR_PRESUPUESTO")]
        public async Task<ActionResult> ActualizarConfiguracion(
            int id,
            [FromBody] ActualizarConfiguracionPresupuestoDto dto)
        {
            try
            {
                await _configuracionService.ActualizarAsync(id, dto);
                return Ok(new { mensaje = "Configuración anual actualizada correctamente." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
        }

        [HttpGet("politica")]
        [PermisoAuthorize("VER_PRESUPUESTO")]
        public async Task<ActionResult> ObtenerPolitica()
        {
            var politica = await _context.PoliticaReemplazoBaterias
                .AsNoTracking()
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            if (politica == null)
                return NotFound(new
                {
                    mensaje = "No existe una política de reemplazo de baterías configurada."
                });

            return Ok(new
            {
                politica.Id,
                politica.MesesReemplazoLocal,
                politica.MesesReemplazoForanea
            });
        }

        [HttpPut("politica")]
        [PermisoAuthorize("GESTIONAR_PRESUPUESTO")]
        public async Task<ActionResult> ActualizarPolitica(
            [FromBody] ActualizarPoliticaReemplazoRequest request)
        {
            if (request.MesesReemplazoLocal <= 0 ||
                request.MesesReemplazoLocal > 120)
                return BadRequest(new
                {
                    mensaje = "El ciclo de reemplazo para agencias locales debe estar entre 1 y 120 meses."
                });

            if (request.MesesReemplazoForanea <= 0 ||
                request.MesesReemplazoForanea > 120)
                return BadRequest(new
                {
                    mensaje = "El ciclo de reemplazo para agencias foráneas debe estar entre 1 y 120 meses."
                });

            var politica = await _context.PoliticaReemplazoBaterias
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            if (politica == null)
                return NotFound(new
                {
                    mensaje = "No existe una política de reemplazo de baterías configurada."
                });

            politica.MesesReemplazoLocal = request.MesesReemplazoLocal;
            politica.MesesReemplazoForanea = request.MesesReemplazoForanea;
            politica.FechaModificacion = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Política de reemplazo actualizada correctamente.",
                politica.Id,
                politica.MesesReemplazoLocal,
                politica.MesesReemplazoForanea
            });
        }

        [HttpGet("proyeccion/{anio:int}")]
        [PermisoAuthorize("VER_PRESUPUESTO")]
        public async Task<ActionResult> ObtenerProyeccion(int anio)
        {
            if (anio < 2020 || anio > 2100)
                return BadRequest(new { mensaje = "El año debe estar entre 2020 y 2100." });

            var configuracion = await _configuracionService.ObtenerPorAnioAsync(anio);

            if (configuracion == null || !configuracion.Activo)
                return NotFound(new
                {
                    mensaje = $"No existe un costo estimado de batería activo para el año {anio}."
                });

            if (configuracion.CostoUnitarioBateria <= 0)
                return BadRequest(new
                {
                    mensaje = $"El costo unitario de batería para {anio} debe ser mayor que cero."
                });

            var politica = await _context.PoliticaReemplazoBaterias
                .AsNoTracking()
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            if (politica == null)
                return NotFound(new
                {
                    mensaje = "No existe una política global de reemplazo de baterías."
                });

            var inventario = await _context.UPSs
                .AsNoTracking()
                .Include(u => u.Agencia!)
                    .ThenInclude(a => a.Ciudad!)
                        .ThenInclude(c => c.Departamento!)
                .Include(u => u.Agencia!)
                    .ThenInclude(a => a.TipoAgencia!)
                .Include(u => u.EstadoUPS)
                .Where(u => u.Agencia != null && u.Agencia.Activa)
                .ToListAsync();

            var programados = await _context.Mantenimientos
                .AsNoTracking()
                .Include(m => m.EstadoMantenimiento)
                .Where(m =>
                    m.IncluyeCambioBaterias &&
                    m.EstadoMantenimiento != null &&
                    m.EstadoMantenimiento.Nombre == "Programado")
                .Select(m => new
                {
                    m.UPSId,
                    m.FechaMantenimiento
                })
                .ToListAsync();

            var resultado = new PresupuestoResumenDto
            {
                Anio = anio,
                CostoUnitarioBateria = configuracion.CostoUnitarioBateria,
                TotalUPSInventario = inventario.Count
            };

            var hoy = DateTime.Today;
            var anioActual = hoy.Year;

            foreach (var equipo in inventario)
            {
                var agencia = equipo.Agencia;

                if (agencia == null)
                    continue;

                var estadoUPS = equipo.EstadoUPS?.Nombre ?? string.Empty;

                if (EsEstado(estadoUPS, "Fuera de servicio") ||
                    EsEstado(estadoUPS, "De baja"))
                    continue;

                var tipoAgencia = ObtenerTipoAgencia(equipo);
                var esLocal = tipoAgencia.Equals("Local", StringComparison.OrdinalIgnoreCase);
                var mesesCiclo =
                    esLocal
                        ? politica.MesesReemplazoLocal
                        : politica.MesesReemplazoForanea;

                if (equipo.CantidadBaterias <= 0 ||
                    equipo.FechaInstalacion == default ||
                    mesesCiclo <= 0)
                {
                    var insuficiente = CrearDetalle(
                        equipo,
                        agencia,
                        tipoAgencia,
                        mesesCiclo,
                        configuracion.CostoUnitarioBateria);

                    insuficiente.InformacionSuficiente = false;
                    insuficiente.Observacion =
                        equipo.CantidadBaterias <= 0
                            ? "El UPS no tiene una cantidad válida de baterías."
                            : equipo.FechaInstalacion == default
                                ? "El UPS no tiene una fecha de instalación válida."
                                : "La política de reemplazo no tiene un ciclo válido.";

                    resultado.UPSInformacionInsuficienteDetalle.Add(insuficiente);
                    continue;
                }

                var fechaBase =
                    equipo.FechaUltimoCambioBaterias ??
                    equipo.FechaInstalacion;

                var primeraFecha = fechaBase.AddMonths(mesesCiclo);
                var fechaProyectada = primeraFecha;
                var vencido = false;
                var reemplazoAnteriorPendiente = primeraFecha.Date < hoy;

                if (anio == anioActual && primeraFecha.Date < hoy)
                {
                    // Si el reemplazo debió hacerse y no existe un cambio
                    // completado que moviera la fecha base, sigue pendiente.
                    fechaProyectada = primeraFecha;
                    vencido = true;
                }
                else
                {
                    while (fechaProyectada.Year < anio)
                        fechaProyectada = fechaProyectada.AddMonths(mesesCiclo);

                    if (fechaProyectada.Year != anio)
                        continue;

                    if (anio == anioActual && fechaProyectada.Date < hoy)
                        vencido = true;
                }

                var mantenimientoProgramado = programados
                    .Where(m =>
                        m.UPSId == equipo.Id &&
                        m.FechaMantenimiento.Year == anio)
                    .OrderBy(m => m.FechaMantenimiento)
                    .FirstOrDefault();

                var detalle = CrearDetalle(
                    equipo,
                    agencia,
                    tipoAgencia,
                    mesesCiclo,
                    configuracion.CostoUnitarioBateria);

                detalle.FechaProyectadaReemplazo = fechaProyectada;
                detalle.CostoEstimado =
                    equipo.CantidadBaterias *
                    configuracion.CostoUnitarioBateria;
                detalle.InformacionSuficiente = true;
                detalle.EstaVencido = vencido;
                detalle.ReemplazoAnteriorPendiente =
                    anio > anioActual && reemplazoAnteriorPendiente;
                detalle.TieneMantenimientoProgramado =
                    mantenimientoProgramado != null;
                detalle.FechaMantenimientoProgramado =
                    mantenimientoProgramado?.FechaMantenimiento;

                resultado.TotalUPSConReemplazo++;
                resultado.TotalBaterias += equipo.CantidadBaterias;
                resultado.TotalPresupuesto += detalle.CostoEstimado;

                if (detalle.EstaVencido)
                    resultado.TotalUPSVencidos++;

                if (detalle.TieneMantenimientoProgramado)
                    resultado.TotalUPSProgramados++;

                var resumenAgencia = resultado.Agencias
                    .FirstOrDefault(x => x.AgenciaId == equipo.AgenciaId);

                if (resumenAgencia == null)
                {
                    resumenAgencia = new PresupuestoAgenciaDto
                    {
                        AgenciaId = equipo.AgenciaId,
                        Agencia = agencia.Nombre,
                        TipoAgencia = tipoAgencia,
                        CantidadUPS = inventario.Count(x => x.AgenciaId == equipo.AgenciaId)
                    };

                    resultado.Agencias.Add(resumenAgencia);
                }

                resumenAgencia.CantidadUPSConReemplazo++;
                resumenAgencia.CantidadBaterias += equipo.CantidadBaterias;
                resumenAgencia.Subtotal += detalle.CostoEstimado;
                resumenAgencia.UPS.Add(detalle);
            }

            resultado.UPSInformacionInsuficiente =
                resultado.UPSInformacionInsuficienteDetalle.Count;

            resultado.Agencias = resultado.Agencias
                .OrderBy(x => x.Agencia)
                .ToList();

            foreach (var agencia in resultado.Agencias)
            {
                agencia.UPS = agencia.UPS
                    .OrderByDescending(x => x.EstaVencido)
                    .ThenBy(x => x.FechaProyectadaReemplazo)
                    .ThenBy(x => x.NumeroInventario)
                    .ToList();
            }

            resultado.UPSInformacionInsuficienteDetalle =
                resultado.UPSInformacionInsuficienteDetalle
                    .OrderBy(x => x.Agencia)
                    .ThenBy(x => x.NumeroInventario)
                    .ToList();

            return Ok(resultado);
        }

        private static PresupuestoUpsDetalleDto CrearDetalle(
            UPS equipo,
            Agencia agencia,
            string tipoAgencia,
            int mesesCiclo,
            decimal costoUnitario)
        {
            return new PresupuestoUpsDetalleDto
            {
                UPSId = equipo.Id,
                NumeroInventario = equipo.NumeroInventario,
                NumeroSerie = equipo.NumeroSerie,
                Marca = equipo.Marca,
                Modelo = equipo.Modelo,
                AgenciaId = equipo.AgenciaId,
                Agencia = agencia.Nombre,
                TipoAgencia = tipoAgencia,
                EstadoUPS = equipo.EstadoUPS?.Nombre ?? string.Empty,
                CantidadBaterias = equipo.CantidadBaterias,
                FechaUltimoCambioBaterias = equipo.FechaUltimoCambioBaterias,
                FechaInstalacion = equipo.FechaInstalacion,
                MesesCicloReemplazo = mesesCiclo,
                CostoUnitarioBateria = costoUnitario,
                InformacionSuficiente = true
            };
        }

        private static string ObtenerTipoAgencia(UPS equipo)
        {
            var ciudad =
                equipo.Agencia?.Ciudad?.Nombre?.Trim()
                ?? string.Empty;

            if (ciudad.Equals(
                    "San Pedro Sula",
                    StringComparison.OrdinalIgnoreCase) ||
                ciudad.Equals(
                    "La Ceiba",
                    StringComparison.OrdinalIgnoreCase) ||
                ciudad.Equals(
                    "Distrito Central",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Local";
            }

            return "Foránea";
        }

        private static bool EsEstado(string? valor, string esperado) =>
            string.Equals(valor?.Trim(), esperado, StringComparison.OrdinalIgnoreCase);

        public class ActualizarPoliticaReemplazoRequest
        {
            public int MesesReemplazoLocal { get; set; }
            public int MesesReemplazoForanea { get; set; }
        }
    }
}
