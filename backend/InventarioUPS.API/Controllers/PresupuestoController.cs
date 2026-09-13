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

        private readonly IConfiguracionPresupuestoService
            _configuracionService;


        public PresupuestoController(
            InventarioUPSDbContext context,
            IConfiguracionPresupuestoService configuracionService)
        {
            _context = context;
            _configuracionService = configuracionService;
        }


        // =========================================================
        // OBTENER TODAS LAS CONFIGURACIONES ANUALES
        // =========================================================

        [HttpGet("configuracion")]
        [PermisoAuthorize("VER_PRESUPUESTO")]
        public async Task<ActionResult> ObtenerConfiguraciones()
        {
            var configuraciones =
                await _configuracionService
                    .ObtenerTodasAsync();

            return Ok(configuraciones);
        }


        // =========================================================
        // OBTENER CONFIGURACIÓN ANUAL POR AÑO
        // =========================================================

        [HttpGet("configuracion/{anio:int}")]
        [PermisoAuthorize("VER_PRESUPUESTO")]
        public async Task<ActionResult> ObtenerConfiguracion(int anio)
        {
            if (anio < 2020 || anio > 2100)
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "El año debe estar entre 2020 y 2100."
                    });
            }

            var configuracion =
                await _configuracionService
                    .ObtenerPorAnioAsync(anio);

            if (configuracion == null)
            {
                return NotFound(
                    new
                    {
                        mensaje =
                            $"No existe una configuración de costo para el año {anio}."
                    });
            }

            return Ok(configuracion);
        }


        // =========================================================
        // CREAR CONFIGURACIÓN ANUAL
        // =========================================================

        [HttpPost("configuracion")]
        [PermisoAuthorize("GESTIONAR_PRESUPUESTO")]
        public async Task<ActionResult> CrearConfiguracion(
            [FromBody]
            CrearConfiguracionPresupuestoDto dto)
        {
            try
            {
                await _configuracionService
                    .CrearAsync(dto);

                return Ok(
                    new
                    {
                        mensaje =
                            "Configuración anual creada correctamente."
                    });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(
                    new
                    {
                        mensaje = ex.Message
                    });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(
                    new
                    {
                        mensaje = ex.Message
                    });
            }
        }


        // =========================================================
        // ACTUALIZAR CONFIGURACIÓN ANUAL
        // =========================================================

        [HttpPut("configuracion/{id:int}")]
        [PermisoAuthorize("GESTIONAR_PRESUPUESTO")]
        public async Task<ActionResult> ActualizarConfiguracion(
            int id,
            [FromBody]
            ActualizarConfiguracionPresupuestoDto dto)
        {
            try
            {
                await _configuracionService
                    .ActualizarAsync(id, dto);

                return Ok(
                    new
                    {
                        mensaje =
                            "Configuración anual actualizada correctamente."
                    });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(
                    new
                    {
                        mensaje = ex.Message
                    });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(
                    new
                    {
                        mensaje = ex.Message
                    });
            }
        }


        // =========================================================
        // OBTENER POLÍTICA GLOBAL DE REEMPLAZO
        // =========================================================

        [HttpGet("politica")]
        [PermisoAuthorize("VER_PRESUPUESTO")]
        public async Task<ActionResult> ObtenerPolitica()
        {
            var politica =
                await _context
                    .PoliticaReemplazoBaterias
                    .AsNoTracking()
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefaultAsync();

            if (politica == null)
            {
                return NotFound(
                    new
                    {
                        mensaje =
                            "No existe una política de reemplazo de baterías configurada."
                    });
            }

            return Ok(
                new
                {
                    politica.Id,
                    politica.MesesReemplazoLocal,
                    politica.MesesReemplazoForanea
                });
        }


        // =========================================================
        // ACTUALIZAR POLÍTICA GLOBAL DE REEMPLAZO
        // =========================================================

        [HttpPut("politica")]
        [PermisoAuthorize("GESTIONAR_PRESUPUESTO")]
        public async Task<ActionResult> ActualizarPolitica(
            [FromBody]
            ActualizarPoliticaReemplazoRequest request)
        {
            if (
                request.MesesReemplazoLocal <= 0 ||
                request.MesesReemplazoLocal > 120)
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "El ciclo de reemplazo para agencias locales debe estar entre 1 y 120 meses."
                    });
            }


            if (
                request.MesesReemplazoForanea <= 0 ||
                request.MesesReemplazoForanea > 120)
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "El ciclo de reemplazo para agencias foráneas debe estar entre 1 y 120 meses."
                    });
            }


            var politica =
                await _context
                    .PoliticaReemplazoBaterias
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefaultAsync();


            if (politica == null)
            {
                return NotFound(
                    new
                    {
                        mensaje =
                            "No existe una política de reemplazo de baterías configurada."
                    });
            }


            politica.MesesReemplazoLocal =
                request.MesesReemplazoLocal;

            politica.MesesReemplazoForanea =
                request.MesesReemplazoForanea;


            await _context.SaveChangesAsync();


            return Ok(
                new
                {
                    mensaje =
                        "Política de reemplazo actualizada correctamente.",

                    politica.Id,

                    politica.MesesReemplazoLocal,

                    politica.MesesReemplazoForanea
                });
        }


        // =========================================================
        // PROYECCIÓN DE PRESUPUESTO
        // =========================================================

        [HttpGet("proyeccion/{anio:int}")]
        [PermisoAuthorize("VER_PRESUPUESTO")]
        public async Task<ActionResult> ObtenerProyeccion(int anio)
        {
            // -----------------------------------------------------
            // VALIDAR AÑO
            // -----------------------------------------------------

            if (anio < 2020 || anio > 2100)
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "El año debe estar entre 2020 y 2100."
                    });
            }


            // -----------------------------------------------------
            // OBTENER CONFIGURACIÓN ANUAL
            //
            // A partir de ahora utilizamos de esta configuración
            // solamente el costo unitario estimado de la batería.
            // -----------------------------------------------------

            var configuracion =
                await _configuracionService
                    .ObtenerPorAnioAsync(anio);

            if (configuracion == null)
            {
                return NotFound(
                    new
                    {
                        mensaje =
                            $"No existe un costo estimado de batería configurado para el año {anio}."
                    });
            }


            // -----------------------------------------------------
            // OBTENER POLÍTICA GLOBAL
            //
            // Los ciclos Local / Foránea ya NO dependen del año.
            // -----------------------------------------------------

            var politica =
                await _context
                    .PoliticaReemplazoBaterias
                    .AsNoTracking()
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefaultAsync();


            if (politica == null)
            {
                return NotFound(
                    new
                    {
                        mensaje =
                            "No existe una política de reemplazo de baterías configurada."
                    });
            }


            // -----------------------------------------------------
            // OBTENER UPS
            // -----------------------------------------------------

            var ups =
                await _context.UPSs
                    .AsNoTracking()

                    .Include(u => u.Agencia!)
                        .ThenInclude(a => a.Ciudad!)
                            .ThenInclude(c => c.Departamento!)

                    .Include(u => u.Agencia!)
                        .ThenInclude(a => a.TipoAgencia!)

                    .Include(u => u.EstadoUPS!)

                    .Where(
                        u =>
                            u.Agencia != null &&
                            u.Agencia.Activa)

                    .ToListAsync();


            // -----------------------------------------------------
            // RESULTADO
            // -----------------------------------------------------

            var resultado =
                new PresupuestoResumenDto
                {
                    Anio = anio,

                    CostoUnitarioBateria =
                        configuracion.CostoUnitarioBateria,

                    TotalUPSInventario =
                        ups.Count
                };


            // =====================================================
            // PROCESAR CADA UPS
            // =====================================================

            foreach (var equipo in ups)
            {
                var agencia =
                    equipo.Agencia;

                if (agencia == null)
                {
                    continue;
                }


                // -------------------------------------------------
                // TIPO DE AGENCIA
                // -------------------------------------------------

                var tipoAgencia =
                    ObtenerTipoAgencia(equipo);


                var esLocal =
                    string.Equals(
                        tipoAgencia,
                        "Local",
                        StringComparison.OrdinalIgnoreCase);


                // -------------------------------------------------
                // CICLO DESDE POLÍTICA GLOBAL
                // -------------------------------------------------

                var mesesCiclo =
                    esLocal
                        ? politica.MesesReemplazoLocal
                        : politica.MesesReemplazoForanea;


                // -------------------------------------------------
                // ESTADO DEL UPS
                // -------------------------------------------------

                var estadoUPS =
                    equipo.EstadoUPS?.Nombre
                    ?? string.Empty;


                if (
                    string.Equals(
                        estadoUPS,
                        "Fuera de servicio",
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }


                // -------------------------------------------------
                // CANTIDAD DE BATERÍAS
                // -------------------------------------------------

                if (equipo.CantidadBaterias <= 0)
                {
                    continue;
                }


                // -------------------------------------------------
                // FECHA BASE
                //
                // Último cambio si existe.
                // De lo contrario fecha de instalación.
                // -------------------------------------------------

                DateTime fechaBase;


                if (
                    equipo.FechaUltimoCambioBaterias
                        .HasValue)
                {
                    fechaBase =
                        equipo.FechaUltimoCambioBaterias
                            .Value;
                }
                else
                {
                    fechaBase =
                        equipo.FechaInstalacion;
                }


                // -------------------------------------------------
                // PRÓXIMO REEMPLAZO
                // -------------------------------------------------

                var fechaProyectada =
                    fechaBase.AddMonths(
                        mesesCiclo);


                // -------------------------------------------------
                // ¿CORRESPONDE AL AÑO SOLICITADO?
                // -------------------------------------------------

                if (
                    fechaProyectada.Year !=
                    anio)
                {
                    continue;
                }


                // -------------------------------------------------
                // COSTO
                // -------------------------------------------------

                var costoEstimado =
                    equipo.CantidadBaterias *
                    configuracion.CostoUnitarioBateria;


                // -------------------------------------------------
                // DETALLE
                // -------------------------------------------------

                var detalle =
                    CrearDetalle(
                        equipo,
                        agencia,
                        tipoAgencia,
                        mesesCiclo,
                        configuracion
                            .CostoUnitarioBateria);


                detalle.FechaProyectadaReemplazo =
                    fechaProyectada;

                detalle.CostoEstimado =
                    costoEstimado;

                detalle.InformacionSuficiente =
                    true;


                // -------------------------------------------------
                // TOTALES NACIONALES
                // -------------------------------------------------

                resultado.TotalUPSConReemplazo++;

                resultado.TotalBaterias +=
                    equipo.CantidadBaterias;

                resultado.TotalPresupuesto +=
                    costoEstimado;


                // -------------------------------------------------
                // RESUMEN POR AGENCIA
                // -------------------------------------------------

                var resumenAgencia =
                    resultado.Agencias
                        .FirstOrDefault(
                            x =>
                                x.AgenciaId ==
                                equipo.AgenciaId);


                if (resumenAgencia == null)
                {
                    resumenAgencia =
                        new PresupuestoAgenciaDto
                        {
                            AgenciaId =
                                equipo.AgenciaId,

                            Agencia =
                                agencia.Nombre,

                            TipoAgencia =
                                tipoAgencia,

                            CantidadUPS =
                                ups.Count(
                                    x =>
                                        x.AgenciaId ==
                                        equipo.AgenciaId)
                        };


                    resultado.Agencias.Add(
                        resumenAgencia);
                }


                resumenAgencia
                    .CantidadUPSConReemplazo++;

                resumenAgencia
                    .CantidadBaterias +=
                    equipo.CantidadBaterias;

                resumenAgencia
                    .Subtotal +=
                    costoEstimado;

                resumenAgencia.UPS.Add(
                    detalle);
            }


            // =====================================================
            // INFORMACIÓN INSUFICIENTE
            // =====================================================

            resultado.UPSInformacionInsuficiente =
                resultado
                    .UPSInformacionInsuficienteDetalle
                    .Count;


            // =====================================================
            // ORDENAR
            // =====================================================

            resultado.Agencias =
                resultado.Agencias
                    .OrderBy(
                        x => x.Agencia)
                    .ToList();


            foreach (
                var agencia
                in resultado.Agencias)
            {
                agencia.UPS =
                    agencia.UPS
                        .OrderBy(
                            x =>
                                x.FechaProyectadaReemplazo)
                        .ThenBy(
                            x =>
                                x.CodigoUPS)
                        .ToList();
            }


            resultado
                .UPSInformacionInsuficienteDetalle =
                    resultado
                        .UPSInformacionInsuficienteDetalle
                        .OrderBy(
                            x => x.Agencia)
                        .ThenBy(
                            x => x.CodigoUPS)
                        .ToList();


            return Ok(resultado);
        }


        // =========================================================
        // CREAR DETALLE DE UPS
        // =========================================================

        private static PresupuestoUpsDetalleDto CrearDetalle(
            UPS equipo,
            Agencia agencia,
            string tipoAgencia,
            int mesesCiclo,
            decimal costoUnitario)
        {
            return new PresupuestoUpsDetalleDto
            {
                UPSId =
                    equipo.Id,

                CodigoUPS =
                    equipo.Codigo,

                NumeroSerie =
                    equipo.NumeroSerie,

                Marca =
                    equipo.Marca,

                Modelo =
                    equipo.Modelo,

                AgenciaId =
                    equipo.AgenciaId,

                Agencia =
                    agencia.Nombre,

                TipoAgencia =
                    tipoAgencia,

                EstadoUPS =
                    equipo.EstadoUPS?.Nombre
                    ?? string.Empty,

                CantidadBaterias =
                    equipo.CantidadBaterias,

                FechaUltimoCambioBaterias =
                    equipo.FechaUltimoCambioBaterias,

                FechaInstalacion =
                    equipo.FechaInstalacion,

                MesesCicloReemplazo =
                    mesesCiclo,

                CostoUnitarioBateria =
                    costoUnitario,

                InformacionSuficiente =
                    true
            };
        }


        // =========================================================
        // DETERMINAR TIPO DE AGENCIA
        // =========================================================

        private static string ObtenerTipoAgencia(
            UPS equipo)
        {
            var tipoRegistrado =
                equipo.Agencia?
                    .TipoAgencia?
                    .Nombre;


            if (
                !string.IsNullOrWhiteSpace(
                    tipoRegistrado))
            {
                return tipoRegistrado;
            }


            var departamento =
                equipo.Agencia?
                    .Ciudad?
                    .Departamento?
                    .Nombre;


            if (
                string.Equals(
                    departamento,
                    "Cortés",
                    StringComparison.OrdinalIgnoreCase)

                ||

                string.Equals(
                    departamento,
                    "Atlántida",
                    StringComparison.OrdinalIgnoreCase)

                ||

                string.Equals(
                    departamento,
                    "Francisco Morazán",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Local";
            }


            return "Foránea";
        }


        // =========================================================
        // REQUEST INTERNO PARA ACTUALIZAR POLÍTICA
        // =========================================================

        public class ActualizarPoliticaReemplazoRequest
        {
            public int MesesReemplazoLocal
            {
                get;
                set;
            }

            public int MesesReemplazoForanea
            {
                get;
                set;
            }
        }
    }
}