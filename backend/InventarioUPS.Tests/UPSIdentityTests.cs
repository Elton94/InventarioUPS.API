using InventarioUPS.API.Controllers;
using InventarioUPS.API.Data;
using InventarioUPS.API.Models;
using InventarioUPS.API.Repositories;
using InventarioUPS.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using Xunit;

namespace InventarioUPS.Tests;

public sealed class UPSIdentityTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private InventarioUPSDbContext context = null!;
    private UPSService service = null!;
    private int agenciaId;
    private int estadoId;

    public async Task InitializeAsync()
    {
        await connection.OpenAsync();
        connection.CreateCollation("Latin1_General_100_CI_AS", (a, b) =>
            string.Compare(a, b, StringComparison.OrdinalIgnoreCase));
        context = new(new DbContextOptionsBuilder<InventarioUPSDbContext>().UseSqlite(connection).Options);
        await context.Database.EnsureCreatedAsync();
        var agencia = new Agencia
        {
            Codigo = "SPS01", Nombre = "Prueba",
            Ciudad = new Ciudad { EsLocal = true, Nombre = "San Pedro Sula", Departamento = new Departamento { Nombre = "Cortés" } },
            TipoAgencia = new TipoAgencia { Nombre = "Local" }
        };
        var estado = new EstadoUPS { Nombre = "Operativo" };
        context.AddRange(agencia, estado, new ModeloBateria { Id = 1, Nombre = "AGM de prueba", TipoBateria = new TipoBateria { Nombre = "AGM" } });
        context.ModelosUPS.Add(new ModeloUPS { Nombre = "Prueba", MarcaUPS = new MarcaUPS { Nombre = "APC" } });
        context.ResponsablesServicio.Add(new ResponsableServicio { Id = 1, Nombre = "Técnico de prueba" });
        await context.SaveChangesAsync();
        agenciaId = agencia.Id;
        estadoId = estado.Id;
        service = new(new UPSRepository(context), context);
    }

    private UPS Equipo(string inventario, string serie = "SERIE-1") => new()
    {
        ModeloBateriaId = 1, NumeroInventario = inventario, NumeroSerie = serie, Marca = "APC", Modelo = "Prueba",
        AgenciaId = agenciaId, EstadoUPSId = estadoId, CapacidadVA = 1000,
        CantidadBaterias = 2, FechaInstalacion = DateTime.Today.AddYears(-1)
    };

    [Fact]
    public void ModeloUsaIdInternoSinCodigoEIdentificadoresUnicos()
    {
        var entidad = context.Model.FindEntityType(typeof(UPS))!;
        Assert.Equal("Id", Assert.Single(entidad.FindPrimaryKey()!.Properties).Name);
        Assert.Contains(entidad.GetIndexes(), i => i.IsUnique && i.Properties.Single().Name == "NumeroInventario");
        Assert.Contains(entidad.GetIndexes(), i => i.IsUnique && i.Properties.Single().Name == "NumeroSerie");
        Assert.Null(entidad.FindProperty("Codigo"));
        var relacion = context.Model.FindEntityType(typeof(Mantenimiento))!.GetForeignKeys()
            .Single(f => f.PrincipalEntityType.ClrType == typeof(UPS));
        Assert.Equal("UPSId", Assert.Single(relacion.Properties).Name);
    }

    [Theory]
    [InlineData("0000123")]
    [InlineData("INV/001 #A+&")]
    public async Task CrearConsultarYEditarConservaClaveDeTexto(string inventario)
    {
        var nuevo = Equipo(inventario);
        await service.CrearAsync(nuevo);
        context.ChangeTracker.Clear();
        var equipo = await service.ObtenerPorIdAsync(nuevo.Id);
        Assert.NotNull(equipo);
        Assert.Equal(inventario, equipo.NumeroInventario);
        context.ModelosUPS.Add(new ModeloUPS { Nombre = "Modelo editado", MarcaUPSId = (await context.MarcasUPS.SingleAsync()).Id });
        await context.SaveChangesAsync();
        equipo.Modelo = "Modelo editado";
        await service.ActualizarAsync(equipo);
        context.ChangeTracker.Clear();
        Assert.Equal("Modelo editado", (await service.ObtenerPorIdAsync(nuevo.Id))!.Modelo);
    }

    [Fact]
    public async Task RechazaInventarioYSerieRepetidos()
    {
        await service.CrearAsync(Equipo("INV-A"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CrearAsync(Equipo(" inv-a ", "OTRA")));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CrearAsync(Equipo("INV-B", " serie-1 ")));
        Assert.Equal(1, await context.UPSs.CountAsync());
    }

    [Fact]
    public async Task EditarNoPermiteSerieDeOtroUPS()
    {
        await service.CrearAsync(Equipo("INV-A"));
        await service.CrearAsync(Equipo("INV-B", "SERIE-2"));
        context.ChangeTracker.Clear();
        var equipo = await context.UPSs.AsNoTracking().SingleAsync(u => u.NumeroInventario == "INV-B");
        equipo!.NumeroSerie = "SERIE-1";
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ActualizarAsync(equipo));
    }

    [Fact]
    public async Task ControladorRechazaIdDiferenteAlDeLaRuta()
    {
        var controller = new UPSController(service);
        var result = await controller.Actualizar(7, Equipo("INV-B"));
        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Empty(await context.UPSs.ToListAsync());
    }

    [Fact]
    public async Task MantenimientoSeRelacionaConInventarioYBloqueaBajaPendiente()
    {
        var ups = Equipo("0001/A");
        await service.CrearAsync(ups);
        var programado = new EstadoMantenimiento { Nombre = "Programado" };
        context.AddRange(programado, new EstadoUPS { Nombre = "De baja" });
        await context.SaveChangesAsync();
        var mantenimiento = new Mantenimiento
        { ResponsableServicioId = 1, ModeloBateriaId = 1,
            UPSId = ups.Id,
            EstadoMantenimientoId = programado.Id, Descripcion = "Revisión",
            TecnicoProveedor = "Prueba", FechaMantenimiento = DateTime.Today.AddDays(1)
        };
        var mantenimientos = new MantenimientoService(new MantenimientoRepository(context), context);
        await mantenimientos.CrearAsync(mantenimiento);
        context.ChangeTracker.Clear();
        var listado = await mantenimientos.ObtenerPorUPSAsync(ups.Id);
        Assert.Equal("0001/A", Assert.Single(listado).NumeroInventarioUPS);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CambiarEstadoAsync(ups.Id, "De baja"));
    }

    [Fact]
    public async Task ProyeccionAsociaMantenimientoPorInventario()
    {
        var equipo = Equipo("0002/B");
        equipo.FechaInstalacion = new DateTime(DateTime.Today.Year - 1, 1, 1);
        await service.CrearAsync(equipo);
        context.AddRange(
            new ConfiguracionPresupuestoBaterias { Anio = DateTime.Today.Year, CostoUnitarioBateria = 100 },
            new PoliticaReemplazoBaterias { MesesReemplazoLocal = 12, MesesReemplazoForanea = 12 });
        context.Mantenimientos.Add(new Mantenimiento
        { ResponsableServicioId = 1, ModeloBateriaId = 1,
            UPSId = equipo.Id, IncluyeCambioBaterias = true,
            FechaMantenimiento = new DateTime(DateTime.Today.Year, 12, 31),
            EstadoMantenimiento = new EstadoMantenimiento { Nombre = "Programado" }
        });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var config = new ConfiguracionPresupuestoService(new ConfiguracionPresupuestoRepository(context));
        var result = await new PresupuestoController(context, config).ObtenerProyeccion(DateTime.Today.Year);
        var resumen = Assert.IsType<InventarioUPS.API.DTOs.Presupuesto.PresupuestoResumenDto>(
            Assert.IsType<OkObjectResult>(result).Value);
        var detalle = Assert.Single(Assert.Single(resumen.Agencias).UPS);
        Assert.Equal("0002/B", detalle.NumeroInventario);
        Assert.True(detalle.TieneMantenimientoProgramado);
    }

    [Fact]
    public async Task CorregirInventarioYSerieConservaMantenimientoEId()
    {
        var ups = Equipo("INV-PEND-1");
        await service.CrearAsync(ups);
        var mantenimiento = new Mantenimiento
        { ResponsableServicioId = 1, ModeloBateriaId = 1,
            UPSId = ups.Id,
            EstadoMantenimiento = new EstadoMantenimiento { Nombre = "Programado" },
            FechaMantenimiento = DateTime.Today.AddDays(1)
        };
        context.Add(mantenimiento);
        await context.SaveChangesAsync();
        var id = ups.Id;
        context.ChangeTracker.Clear();
        var editar = (await service.ObtenerPorIdAsync(id))!;
        editar.NumeroInventario = "0007/A #+";
        editar.NumeroSerie = "SERIE-CORREGIDA";
        await service.ActualizarAsync(editar);
        context.ChangeTracker.Clear();
        var guardado = await context.Mantenimientos.Include(m => m.UPS).SingleAsync();
        Assert.Equal(id, guardado.UPSId);
        Assert.Equal("0007/A #+", guardado.UPS!.NumeroInventario);
        Assert.Equal("SERIE-CORREGIDA", guardado.UPS.NumeroSerie);
    }

    [Fact]
    public async Task EditarNoPermiteInventarioDeOtroUPS()
    {
        var a = Equipo("INV-A");
        var b = Equipo("INV-B", "SERIE-2");
        await service.CrearAsync(a);
        await service.CrearAsync(b);
        context.ChangeTracker.Clear();
        var editar = (await service.ObtenerPorIdAsync(b.Id))!;
        editar.NumeroInventario = " inv-a ";
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ActualizarAsync(editar));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task MantenimientoSinColumnasDeCostosConservaCambioDeBaterias(bool cambioBaterias)
    {
        var entidad = context.Model.FindEntityType(typeof(Mantenimiento))!;
        Assert.Null(entidad.FindProperty("Costo"));
        Assert.Null(entidad.FindProperty("CostoBaterias"));
        Assert.Null(entidad.FindProperty("TipoMantenimientoId"));
        var ups = Equipo("INV-COSTOS");
        await service.CrearAsync(ups);
        var programado = new EstadoMantenimiento { Nombre = "Programado" };
        var completado = new EstadoMantenimiento { Nombre = "Completado" };
        context.AddRange(programado, completado);
        await context.SaveChangesAsync();
        var mantenimientos = new MantenimientoService(new MantenimientoRepository(context), context);
        var nuevo = new Mantenimiento
        { ResponsableServicioId = 1, ModeloBateriaId = 1,
            UPSId = ups.Id,
            EstadoMantenimientoId = programado.Id, FechaMantenimiento = DateTime.Today,
            Descripcion = "Revisión", TecnicoProveedor = "Técnico", IncluyeCambioBaterias = true
        };
        await mantenimientos.CrearAsync(nuevo);
        Assert.Equal(2, nuevo.CantidadBateriasReemplazadas);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        await mantenimientos.ActualizarAsync(new Mantenimiento
        { ResponsableServicioId = 1, ModeloBateriaId = 1,
            Id = nuevo.Id, UPSId = ups.Id,
            EstadoMantenimientoId = completado.Id, FechaMantenimiento = DateTime.Today,
            Descripcion = "Revisión completada", TecnicoProveedor = "Técnico",
            IncluyeCambioBaterias = cambioBaterias
        });
        context.ChangeTracker.Clear();
        var guardado = await context.Mantenimientos.SingleAsync();
        Assert.True(guardado.IncluyeCambioBaterias);
        Assert.Equal(2, guardado.CantidadBateriasReemplazadas);
        Assert.Equal(DateTime.Today, (await context.UPSs.SingleAsync()).FechaUltimoCambioBaterias);
    }

    [Theory]
    [InlineData("Programado")]
    [InlineData("En proceso")]
    [InlineData("Cancelado")]
    [InlineData("Completado")]
    public async Task SoloCompletadoActualizaFechaYProyeccion(string nombreEstado)
    {
        var ups = Equipo("INV-FECHAS");
        ups.FechaUltimoCambioBaterias = DateTime.Today.AddMonths(-3);
        await service.CrearAsync(ups);
        var estado = new EstadoMantenimiento { Nombre = nombreEstado };
        context.AddRange(estado, new EstadoUPS { Nombre = "En mantenimiento" },
            new ConfiguracionPresupuestoBaterias { Anio = DateTime.Today.Year + 1, CostoUnitarioBateria = 100 },
            new PoliticaReemplazoBaterias { MesesReemplazoLocal = 12, MesesReemplazoForanea = 12 });
        await context.SaveChangesAsync();
        var cambios = new MantenimientoService(new MantenimientoRepository(context), context);
        var cambio = new Mantenimiento
        { ResponsableServicioId = 1, ModeloBateriaId = 1,
            UPSId = ups.Id, EstadoMantenimientoId = estado.Id,
            FechaMantenimiento = DateTime.Today, TecnicoProveedor = "Técnico", Descripcion = "Banco completo",
            IncluyeCambioBaterias = false, CantidadBateriasReemplazadas = 999
        };
        await cambios.CrearAsync(cambio);
        context.ChangeTracker.Clear();
        var guardado = await context.Mantenimientos.SingleAsync();
        Assert.True(guardado.IncluyeCambioBaterias);
        Assert.Equal(2, guardado.CantidadBateriasReemplazadas);
        var esperada = nombreEstado == "Completado" ? DateTime.Today : DateTime.Today.AddMonths(-3);
        Assert.Equal(esperada, (await context.UPSs.SingleAsync()).FechaUltimoCambioBaterias);
        var config = new ConfiguracionPresupuestoService(new ConfiguracionPresupuestoRepository(context));
        var result = await new PresupuestoController(context, config).ObtenerProyeccion(DateTime.Today.Year + 1);
        var resumen = Assert.IsType<InventarioUPS.API.DTOs.Presupuesto.PresupuestoResumenDto>(
            Assert.IsType<OkObjectResult>(result).Value);
        var detalle = Assert.Single(Assert.Single(resumen.Agencias).UPS);
        var proxima = esperada.AddMonths(12);
        while (proxima.Year < DateTime.Today.Year + 1) proxima = proxima.AddMonths(12);
        Assert.Equal(proxima, detalle.FechaProyectadaReemplazo);
        Assert.Equal(200m, detalle.CostoEstimado);
    }

    [Theory]
    [InlineData("Cancelado", false)]
    [InlineData("Programado", false)]
    [InlineData("Cancelado", true)]
    public async Task RevertirCompletadoRecuperaFechaAnterior(string destino, bool conFechaBase)
    {
        var ups = Equipo("INV-REVERTIR");
        DateTime? fechaBase = conFechaBase ? DateTime.Today.AddMonths(-4) : null;
        ups.FechaUltimoCambioBaterias = fechaBase;
        await service.CrearAsync(ups);
        var completado = new EstadoMantenimiento { Nombre = "Completado" };
        var estadoDestino = new EstadoMantenimiento { Nombre = destino };
        context.AddRange(completado, estadoDestino);
        await context.SaveChangesAsync();
        var cambios = new MantenimientoService(new MantenimientoRepository(context), context);
        var cambio = new Mantenimiento
        { ResponsableServicioId = 1, ModeloBateriaId = 1,
            UPSId = ups.Id, EstadoMantenimientoId = completado.Id, FechaMantenimiento = DateTime.Today,
            Descripcion = "Banco completo", TecnicoProveedor = "Técnico"
        };
        await cambios.CrearAsync(cambio);
        context.ChangeTracker.Clear();
        var editarUPS = (await service.ObtenerPorIdAsync(ups.Id))!;
        context.ModelosUPS.Add(new ModeloUPS { Nombre = "Corregido", MarcaUPSId = (await context.MarcasUPS.SingleAsync()).Id });
        await context.SaveChangesAsync();
        editarUPS.Modelo = "Corregido";
        await service.ActualizarAsync(editarUPS);
        context.ChangeTracker.Clear();
        await cambios.ActualizarAsync(new Mantenimiento
        { ResponsableServicioId = 1, ModeloBateriaId = 1,
            Id = cambio.Id, UPSId = ups.Id, EstadoMantenimientoId = estadoDestino.Id,
            FechaMantenimiento = DateTime.Today, Descripcion = "Revertido", TecnicoProveedor = "Técnico"
        });
        context.ChangeTracker.Clear();
        Assert.Equal(fechaBase, (await context.UPSs.SingleAsync()).FechaUltimoCambioBaterias);
    }

    [Fact]
    public async Task RegistrosSinBateriasPermanecenFueraDelModulo()
    {
        var ups = Equipo("INV-HISTORICO");
        await service.CrearAsync(ups);
        var anterior = new Mantenimiento
        { ResponsableServicioId = 1, ModeloBateriaId = 1,
            UPSId = ups.Id, EstadoMantenimiento = new EstadoMantenimiento { Nombre = "Completado" },
            IncluyeCambioBaterias = false, FechaMantenimiento = DateTime.Today
        };
        context.Add(anterior);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var cambios = new MantenimientoService(new MantenimientoRepository(context), context);
        Assert.Empty(await cambios.ObtenerTodosAsync());
        Assert.Empty(await cambios.ObtenerPorUPSAsync(ups.Id));
        Assert.Null(await cambios.ObtenerPorIdAsync(anterior.Id));
        Assert.Equal(1, await context.Mantenimientos.CountAsync());
    }

    [Fact]
    public async Task RechazaUPSsinBateriasYCompletadoConFechaFutura()
    {
        var ups = Equipo("INV-VALIDACION");
        await service.CrearAsync(ups);
        var estado = new EstadoMantenimiento { Nombre = "Completado" };
        context.Add(estado);
        await context.SaveChangesAsync();
        var cambios = new MantenimientoService(new MantenimientoRepository(context), context);
        var cambio = new Mantenimiento
        { ResponsableServicioId = 1, ModeloBateriaId = 1,
            UPSId = ups.Id, EstadoMantenimientoId = estado.Id, FechaMantenimiento = DateTime.Today.AddDays(1),
            Descripcion = "Banco completo", TecnicoProveedor = "Técnico"
        };
        await Assert.ThrowsAsync<ArgumentException>(() => cambios.CrearAsync(cambio));
        ups.CantidadBaterias = 0;
        await context.SaveChangesAsync();
        cambio.FechaMantenimiento = DateTime.Today;
        await Assert.ThrowsAsync<InvalidOperationException>(() => cambios.CrearAsync(cambio));
        Assert.Empty(await context.Mantenimientos.ToListAsync());
    }

    [Fact]
    public async Task EditarCompletadoConservaCantidadYCancelarRecuperaOtroCambio()
    {
        var ups = Equipo("INV-HISTORIAL");
        await service.CrearAsync(ups);
        var completado = new EstadoMantenimiento { Nombre = "Completado" };
        var cancelado = new EstadoMantenimiento { Nombre = "Cancelado" };
        context.AddRange(completado, cancelado);
        await context.SaveChangesAsync();
        var cambios = new MantenimientoService(new MantenimientoRepository(context), context);
        var anterior = new Mantenimiento
        { ResponsableServicioId = 1, ModeloBateriaId = 1,
            UPSId = ups.Id, EstadoMantenimientoId = completado.Id,
            FechaMantenimiento = DateTime.Today.AddDays(-10), TecnicoProveedor = "Técnico", Descripcion = "Anterior"
        };
        await cambios.CrearAsync(anterior);
        var actual = new Mantenimiento
        { ResponsableServicioId = 1, ModeloBateriaId = 1,
            UPSId = ups.Id, EstadoMantenimientoId = completado.Id,
            FechaMantenimiento = DateTime.Today, TecnicoProveedor = "Técnico", Descripcion = "Actual"
        };
        await cambios.CrearAsync(actual);
        ups.CantidadBaterias = 4;
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var editar = new Mantenimiento
        { ResponsableServicioId = 1, ModeloBateriaId = 1,
            Id = actual.Id, UPSId = ups.Id, EstadoMantenimientoId = completado.Id,
            FechaMantenimiento = DateTime.Today, TecnicoProveedor = "Técnico", Descripcion = "Corregido"
        };
        await cambios.ActualizarAsync(editar);
        context.ChangeTracker.Clear();
        Assert.Equal(2, (await cambios.ObtenerPorIdAsync(actual.Id))!.CantidadBateriasReemplazadas);
        editar.EstadoMantenimientoId = cancelado.Id;
        await cambios.ActualizarAsync(editar);
        context.ChangeTracker.Clear();
        Assert.Equal(anterior.FechaMantenimiento, (await context.UPSs.SingleAsync()).FechaUltimoCambioBaterias);
    }

    [Theory]
    [InlineData("San Pedro Sula", 4)]
    [InlineData("La Ceiba", 4)]
    [InlineData("Distrito Central", 4)]
    [InlineData("El Progreso", 2)]
    public async Task Presupuesto2029CuentaLosCiclosPorCiudadYUsaSuCosto(string ciudad, int cambiosEsperados)
    {
        (await context.Ciudades.SingleAsync()).Nombre = ciudad;
        (await context.Ciudades.SingleAsync()).EsLocal = ciudad != "El Progreso";
        await context.SaveChangesAsync();
        var ups = Equipo("INV-CICLICO");
        ups.CantidadBaterias = 4;
        ups.FechaInstalacion = new DateTime(2025, 12, 15);
        await service.CrearAsync(ups);
        context.Add(new PoliticaReemplazoBaterias { MesesReemplazoLocal = 3, MesesReemplazoForanea = 6 });
        await context.SaveChangesAsync();
        var config = new ConfiguracionPresupuestoService(new ConfiguracionPresupuestoRepository(context));
        await config.CrearAsync(new() { Anio = 2029, CostoUnitarioBateria = 500 });
        await config.CrearAsync(new() { Anio = 2028, CostoUnitarioBateria = 100 });
        await Assert.ThrowsAsync<InvalidOperationException>(() => config.CrearAsync(new() { Anio = 2029, CostoUnitarioBateria = 900 }));
        var result = await new PresupuestoController(context, config).ObtenerProyeccion(2029);
        var resumen = Assert.IsType<InventarioUPS.API.DTOs.Presupuesto.PresupuestoResumenDto>(Assert.IsType<OkObjectResult>(result).Value);
        Assert.Equal(1, resumen.TotalUPSConReemplazo);
        Assert.Equal(cambiosEsperados, resumen.TotalCambios);
        Assert.Equal(cambiosEsperados * 4, resumen.TotalBaterias);
        Assert.Equal(cambiosEsperados * 2000m, resumen.TotalPresupuesto);
        var detalle = Assert.Single(Assert.Single(resumen.Agencias).UPS);
        Assert.Equal(cambiosEsperados == 4 ? new[] { 3, 6, 9, 12 } : new[] { 6, 12 }, detalle.FechasReemplazo.Select(f => f.Month));
        Assert.Equal(0m, detalle.CostoPendiente);
    }

    [Fact]
    public async Task SinCostoActivoNoGeneraPresupuestoYRechazaImportesInvalidos()
    {
        var config = new ConfiguracionPresupuestoService(new ConfiguracionPresupuestoRepository(context));
        var controller = new PresupuestoController(context, config);
        Assert.IsType<NotFoundObjectResult>(await controller.ObtenerProyeccion(2029));
        foreach (var costo in new[] { 0m, -1m, 0.001m, 12.345m, 10000000000000000m })
            await Assert.ThrowsAsync<ArgumentException>(() => config.CrearAsync(new() { Anio = 2029, CostoUnitarioBateria = costo }));
        await config.CrearAsync(new() { Anio = 2029, CostoUnitarioBateria = 500 });
        var guardado = await config.ObtenerPorAnioAsync(2029);
        await config.ActualizarAsync(guardado!.Id, new() { CostoUnitarioBateria = 500, Activo = false });
        Assert.IsType<NotFoundObjectResult>(await controller.ObtenerProyeccion(2029));
        Assert.IsType<BadRequestObjectResult>(await controller.ObtenerProyeccion(DateTime.Today.Year - 1));
    }

    [Fact]
    public async Task CambioNoReparaUnUPSniPermiteFechasAnterioresALaInstalacion()
    {
        var ups = Equipo("INV-ESTADOS");
        var fuera = new EstadoUPS { Nombre = "Fuera de servicio" };
        var enProceso = new EstadoUPS { Nombre = "En mantenimiento" };
        var proceso = new EstadoMantenimiento { Nombre = "En proceso" };
        var completo = new EstadoMantenimiento { Nombre = "Completado" };
        context.AddRange(fuera, enProceso, proceso, completo);
        await context.SaveChangesAsync();
        ups.EstadoUPSId = fuera.Id;
        await service.CrearAsync(ups);
        var cambios = new MantenimientoService(new MantenimientoRepository(context), context);
        var cambio = new Mantenimiento { ResponsableServicioId = 1, ModeloBateriaId = 1, UPSId = ups.Id, EstadoMantenimientoId = proceso.Id,
            FechaMantenimiento = ups.FechaInstalacion.AddDays(-1), Descripcion = "Reemplazo", TecnicoProveedor = "Técnico" };
        await Assert.ThrowsAsync<ArgumentException>(() => cambios.CrearAsync(cambio));
        cambio.FechaMantenimiento = DateTime.Today;
        await cambios.CrearAsync(cambio);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CambiarEstadoAsync(ups.Id, "Operativo"));
        context.ChangeTracker.Clear();
        await cambios.ActualizarAsync(new Mantenimiento { ResponsableServicioId = 1, ModeloBateriaId = 1, Id = cambio.Id, UPSId = ups.Id,
            EstadoMantenimientoId = completo.Id, FechaMantenimiento = DateTime.Today, Descripcion = "Completado", TecnicoProveedor = "Técnico" });
        context.ChangeTracker.Clear();
        Assert.Equal(fuera.Id, (await context.UPSs.SingleAsync()).EstadoUPSId);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CambiarEstadoAsync(ups.Id, "En mantenimiento"));
    }

    [Fact]
    public async Task ProyeccionActualConsolidaVencidosYExcluyeEquiposDadosDeBaja()
    {
        var ups = Equipo("INV-VENCIDO");
        ups.FechaInstalacion = DateTime.Today.AddYears(-2);
        await service.CrearAsync(ups);
        var excluido = Equipo("INV-BAJA", "SERIE-BAJA");
        await service.CrearAsync(excluido);
        var baja = new EstadoUPS { Nombre = "De baja" };
        context.Add(baja);
        context.Add(new PoliticaReemplazoBaterias { MesesReemplazoLocal = 3, MesesReemplazoForanea = 6 });
        await context.SaveChangesAsync();
        await service.CambiarEstadoAsync(excluido.Id, "De baja");
        var config = new ConfiguracionPresupuestoService(new ConfiguracionPresupuestoRepository(context));
        await config.CrearAsync(new() { Anio = DateTime.Today.Year, CostoUnitarioBateria = 500 });
        context.ChangeTracker.Clear();
        var result = await new PresupuestoController(context, config).ObtenerProyeccion(DateTime.Today.Year);
        var resumen = Assert.IsType<InventarioUPS.API.DTOs.Presupuesto.PresupuestoResumenDto>(Assert.IsType<OkObjectResult>(result).Value);
        var detalle = Assert.Single(Assert.Single(resumen.Agencias).UPS);
        Assert.Equal(1, resumen.TotalUPSConReemplazo);
        Assert.Equal(1, resumen.TotalUPSExcluidos);
        Assert.Equal(1000m, resumen.TotalPendiente);
        Assert.Equal(detalle.FechasReemplazo.Count + 1, detalle.CantidadCambios);
        Assert.Equal(detalle.CantidadCambios * 1000m, resumen.TotalPresupuesto);
        Assert.All(detalle.FechasReemplazo, f => Assert.True(f > DateTime.Today));
    }

    [Theory]
    [InlineData("MigracionUPS.sql")]
    [InlineData("MantenimientosSinCostos.sql")]
    [InlineData("CambiosBaterias.sql")]
    [InlineData("CatalogoBaterias.sql")]
    [InlineData("AdministracionCatalogos.sql")]
    [InlineData("RevisionCampos.sql")]
    public void ScriptDeMigracionTieneSintaxisTSQLValida(string archivo)
    {
        var parser = new TSql160Parser(true);
        var fragment = parser.Parse(new StringReader(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, archivo))), out var errors);
        Assert.Empty(errors);
        var visitor = new SqlDinamicoVisitor(parser);
        fragment.Accept(visitor);
        Assert.Empty(visitor.Errores);
    }

    [Fact]
    public async Task UPSConTipoSinModeloSeGuardaYProyectaConReferenciaGeneral()
    {
        var ups = Equipo("SOLO-TIPO");
        ups.ModeloBateriaId = null;
        ups.TipoBateriaId = (await context.TiposBateria.SingleAsync()).Id;
        await service.CrearAsync(ups);
        context.ChangeTracker.Clear();
        var guardado = (await service.ObtenerPorIdAsync(ups.Id))!;
        Assert.Null(guardado.ModeloBateriaId);
        Assert.Equal(ups.TipoBateriaId, guardado.TipoBateriaBaseId);
        Assert.Equal("AGM", guardado.TipoBateria!.Nombre);
        guardado.NumeroSerie = "EDITADO";
        await service.ActualizarAsync(guardado);
        context.Add(new ConfiguracionPresupuestoBaterias {
            Anio = 2029, CostoUnitarioBateria = 100, MesesReemplazoLocal = 12, MesesReemplazoForanea = 12, Activo = true });
        await context.SaveChangesAsync();
        context.Add(new PoliticaReemplazoBaterias { MesesReemplazoLocal = 12, MesesReemplazoForanea = 12 });
        await context.SaveChangesAsync();
        var config = new ConfiguracionPresupuestoService(new ConfiguracionPresupuestoRepository(context));
        var resultado = Assert.IsType<OkObjectResult>(await new PresupuestoController(context, config).ObtenerProyeccion(2029));
        var proyeccion = Assert.IsType<InventarioUPS.API.DTOs.Presupuesto.PresupuestoResumenDto>(resultado.Value);
        var detalle = Assert.Single(Assert.Single(proyeccion.Agencias).UPS);
        Assert.Equal("AGM", detalle.TipoBateria);
        Assert.True(detalle.UsaCostoGeneral);
    }

    [Fact]
    public async Task RechazaTipoIncompatibleConModeloEInactivoParaNuevoUPS()
    {
        var tipo = new TipoBateria { Nombre = "Litio", Activo = false };
        context.Add(tipo);
        await context.SaveChangesAsync();
        var ups = Equipo("INCOMPATIBLE");
        ups.TipoBateriaId = tipo.Id;
        await Assert.ThrowsAsync<ArgumentException>(() => service.CrearAsync(ups));
        ups.ModeloBateriaId = null;
        await Assert.ThrowsAsync<ArgumentException>(() => service.CrearAsync(ups));
        Assert.Empty(await context.UPSs.ToListAsync());
    }

    [Fact]
    public async Task CancelarCambioRestauraTipoOriginalSinModelo()
    {
        var ups = Equipo("TIPO-BASE");
        ups.ModeloBateriaId = null;
        ups.TipoBateriaId = (await context.TiposBateria.SingleAsync()).Id;
        await service.CrearAsync(ups);
        var modelo = new ModeloBateria { Nombre = "Litio nuevo", TipoBateria = new TipoBateria { Nombre = "Litio" } };
        var completo = new EstadoMantenimiento { Nombre = "Completado" };
        var cancelado = new EstadoMantenimiento { Nombre = "Cancelado" };
        context.AddRange(modelo, completo, cancelado);
        await context.SaveChangesAsync();
        var cambios = new MantenimientoService(new MantenimientoRepository(context), context);
        var cambio = new Mantenimiento { ResponsableServicioId = 1, UPSId = ups.Id, ModeloBateriaId = modelo.Id,
            EstadoMantenimientoId = completo.Id, FechaMantenimiento = DateTime.Today };
        await cambios.CrearAsync(cambio);
        context.ChangeTracker.Clear();
        Assert.Equal(modelo.TipoBateriaId, (await service.ObtenerPorIdAsync(ups.Id))!.TipoBateriaId);
        cambio.EstadoMantenimientoId = cancelado.Id;
        await cambios.ActualizarAsync(cambio);
        context.ChangeTracker.Clear();
        var restaurado = (await service.ObtenerPorIdAsync(ups.Id))!;
        Assert.Null(restaurado.ModeloBateriaId);
        Assert.Equal(ups.TipoBateriaBaseId, restaurado.TipoBateriaId);
    }

    [Fact]
    public async Task ResponsableRequiereCatalogoYConservaNombreHistorico()
    {
        var ups = Equipo("RESPONSABLE");
        await service.CrearAsync(ups);
        var estado = new EstadoMantenimiento { Nombre = "Programado" };
        context.Add(estado);
        await context.SaveChangesAsync();
        var cambios = new MantenimientoService(new MantenimientoRepository(context), context);
        var cambio = new Mantenimiento { UPSId = ups.Id, ModeloBateriaId = 1, EstadoMantenimientoId = estado.Id,
            FechaMantenimiento = DateTime.Today, TecnicoProveedor = "Texto que no es una opción" };
        await Assert.ThrowsAsync<ArgumentException>(() => cambios.CrearAsync(cambio));
        cambio.ResponsableServicioId = 1;
        await cambios.CrearAsync(cambio);
        Assert.Equal("Técnico de prueba", cambio.TecnicoProveedor);
        var admin = new CatalogoAdminService(context);
        var antes = Assert.Single(await admin.ListarAsync("responsables-servicio"));
        await admin.GuardarAsync("responsables-servicio", antes.Id, new GuardarCatalogo("Nombre corregido", false, null, antes), 1, "Admin");
        context.ChangeTracker.Clear();
        cambio.TecnicoProveedor = "Intento de modificar el historial";
        await cambios.ActualizarAsync(cambio);
        context.ChangeTracker.Clear();
        Assert.Equal("Técnico de prueba", (await cambios.ObtenerPorIdAsync(cambio.Id))!.TecnicoProveedor);
        var nuevo = new Mantenimiento { ResponsableServicioId = 1, UPSId = ups.Id, ModeloBateriaId = 1,
            EstadoMantenimientoId = estado.Id, FechaMantenimiento = DateTime.Today };
        await Assert.ThrowsAsync<ArgumentException>(() => cambios.CrearAsync(nuevo));
        await Assert.ThrowsAsync<InvalidOperationException>(() => admin.GuardarAsync("responsables-servicio", 0,
            new GuardarCatalogo(" nombre   corregido ", true, null), 1, "Admin"));
    }

    [Fact]
    public async Task TipoEsObligatorioYModeloSeleccionadoDebeExistir()
    {
        var ups = Equipo("INV-SIN-MODELO");
        ups.ModeloBateriaId = null;
        await Assert.ThrowsAsync<ArgumentException>(() => service.CrearAsync(ups));
        ups.ModeloBateriaId = 999;
        await Assert.ThrowsAsync<ArgumentException>(() => service.CrearAsync(ups));
    }

    [Fact]
    public async Task ModeloInstaladoCambiaSoloAlCompletarYSeRestauraAlCancelar()
    {
        var ups = Equipo("INV-MODELOS");
        await service.CrearAsync(ups);
        var modelo = new ModeloBateria { Nombre = "Modelo nuevo", TipoBateriaId = (await context.TiposBateria.SingleAsync()).Id };
        var programado = new EstadoMantenimiento { Nombre = "Programado" };
        var completado = new EstadoMantenimiento { Nombre = "Completado" };
        var cancelado = new EstadoMantenimiento { Nombre = "Cancelado" };
        context.AddRange(modelo, programado, completado, cancelado);
        await context.SaveChangesAsync();
        var cambios = new MantenimientoService(new MantenimientoRepository(context), context);
        var cambio = new Mantenimiento { ResponsableServicioId = 1, UPSId = ups.Id, ModeloBateriaId = modelo.Id,
            EstadoMantenimientoId = programado.Id, FechaMantenimiento = DateTime.Today, Descripcion = "Prueba", TecnicoProveedor = "Técnico" };
        await cambios.CrearAsync(cambio);
        context.ChangeTracker.Clear();
        Assert.Equal(1, (await context.UPSs.AsNoTracking().SingleAsync()).ModeloBateriaId);
        cambio.EstadoMantenimientoId = completado.Id;
        await cambios.ActualizarAsync(cambio);
        context.ChangeTracker.Clear();
        Assert.Equal(modelo.Id, (await context.UPSs.AsNoTracking().SingleAsync()).ModeloBateriaId);
        cambio.EstadoMantenimientoId = cancelado.Id;
        await cambios.ActualizarAsync(cambio);
        context.ChangeTracker.Clear();
        Assert.Equal(1, (await context.UPSs.AsNoTracking().SingleAsync()).ModeloBateriaId);
    }

    [Fact]
    public async Task PrecioPorModeloPrevaleceSoloEnSuAnio()
    {
        var ups = Equipo("INV-COSTO-MODELO");
        await service.CrearAsync(ups);
        context.Add(new PoliticaReemplazoBaterias { MesesReemplazoLocal = 3, MesesReemplazoForanea = 6 });
        context.Add(new CostoModeloBateria { ModeloBateriaId = 1, Anio = 2029, CostoUnitario = 750 });
        await context.SaveChangesAsync();
        var config = new ConfiguracionPresupuestoService(new ConfiguracionPresupuestoRepository(context));
        await config.CrearAsync(new() { Anio = 2029, CostoUnitarioBateria = 500 });
        await config.CrearAsync(new() { Anio = 2030, CostoUnitarioBateria = 600 });
        var controlador = new PresupuestoController(context, config);
        var resumen = Assert.IsType<InventarioUPS.API.DTOs.Presupuesto.PresupuestoResumenDto>(Assert.IsType<OkObjectResult>(await controlador.ObtenerProyeccion(2029)).Value);
        var detalle = Assert.Single(Assert.Single(resumen.Agencias).UPS);
        Assert.False(detalle.UsaCostoGeneral);
        Assert.Equal(750m, detalle.CostoUnitarioBateria);
        Assert.Equal(resumen.TotalBaterias * 750m, resumen.TotalPresupuesto);
        resumen = Assert.IsType<InventarioUPS.API.DTOs.Presupuesto.PresupuestoResumenDto>(Assert.IsType<OkObjectResult>(await controlador.ObtenerProyeccion(2030)).Value);
        detalle = Assert.Single(Assert.Single(resumen.Agencias).UPS);
        Assert.True(detalle.UsaCostoGeneral);
        Assert.Equal(600m, detalle.CostoUnitarioBateria);
    }

    [Fact]
    public async Task InicioCalculaVencimientosSinCostosYNoOcultaAtrasos()
    {
        var ups = Equipo("INV-INICIO");
        await service.CrearAsync(ups);
        context.Add(new PoliticaReemplazoBaterias { MesesReemplazoLocal = 3, MesesReemplazoForanea = 6 });
        await context.SaveChangesAsync();
        var response = Assert.IsType<OkObjectResult>(await new ReemplazosController(context).Obtener());
        var json = System.Text.Json.JsonSerializer.SerializeToElement(response.Value);
        var equipo = Assert.Single(json.GetProperty("equipos").EnumerateArray());
        Assert.True(equipo.GetProperty("dias").GetInt32() < 0);
        Assert.Equal(ups.FechaInstalacion.AddMonths(3), equipo.GetProperty("fecha").GetDateTime());
    }

    [Fact]
    public async Task CatalogosEvitanDuplicadosYRegistranAntesYDespues()
    {
        var admin = new CatalogoAdminService(context);
        var creado = await admin.GuardarAsync("tipos-bateria", 0, new("  Tipo   nuevo  ", true, null), 7, "Administrador");
        Assert.Equal("Tipo nuevo", creado.Nombre);
        await Assert.ThrowsAsync<InvalidOperationException>(() => admin.GuardarAsync("tipos-bateria", 0, new("TIPO NUEVO", true, null), 7, "Administrador"));
        var inactivo = await admin.GuardarAsync("tipos-bateria", creado.Id, new("Tipo nuevo", false, null, creado), 7, "Administrador");
        Assert.False(inactivo.Activo);
        var registros = await context.AuditoriaCatalogos.OrderBy(x => x.Id).ToListAsync();
        Assert.Equal(2, registros.Count);
        Assert.Equal("Desactivar", registros[1].Accion);
        Assert.Contains("Tipo nuevo", registros[1].Antes);
        Assert.Equal(7, registros[1].UsuarioId);
        await Assert.ThrowsAsync<InvalidOperationException>(() => admin.GuardarAsync("tipos-bateria", 0, new(" tipo nuevo ", true, null), 7, "Administrador"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => admin.GuardarAsync("tipos-bateria", creado.Id, new("Otro nombre", true, null, creado), 7, "Administrador"));
    }

    [Fact]
    public async Task NoPermiteReclasificarModelosUsadosNiCrearBajoPadresInactivos()
    {
        var equipo = Equipo("INV-CATALOGO");
        await service.CrearAsync(equipo);
        var admin = new CatalogoAdminService(context);
        var otroTipo = await admin.GuardarAsync("tipos-bateria", 0, new("Otro tipo", true, null), 7, "Admin");
        var modelo = (await admin.ListarAsync("modelos-bateria")).Single();
        await Assert.ThrowsAsync<InvalidOperationException>(() => admin.GuardarAsync("modelos-bateria", modelo.Id, new(modelo.Nombre, true, otroTipo.Id, modelo), 7, "Admin"));
        var inactivo = await admin.GuardarAsync("tipos-bateria", otroTipo.Id, new(otroTipo.Nombre, false, null, otroTipo), 7, "Admin");
        await Assert.ThrowsAsync<ArgumentException>(() => admin.GuardarAsync("modelos-bateria", 0, new("Batería nueva", true, inactivo.Id), 7, "Admin"));
        var retirado = await admin.GuardarAsync("modelos-bateria", modelo.Id, new(modelo.Nombre, false, modelo.PadreId, modelo), 7, "Admin");
        Assert.False(retirado.Activo);
        context.ChangeTracker.Clear();
        Assert.Equal(modelo.Id, (await context.UPSs.SingleAsync()).ModeloBateriaId);
        context.ChangeTracker.Clear();
        await Assert.ThrowsAsync<ArgumentException>(() => service.CrearAsync(Equipo("INV-OTRO", "SERIE-OTRA")));
        await service.ActualizarAsync((await service.ObtenerPorIdAsync(equipo.Id))!);
    }

    [Fact]
    public async Task CorregirCiudadNoCambiaSuClasificacionNiSuProyeccion()
    {
        var admin = new CatalogoAdminService(context);
        var ciudad = (await admin.ListarAsync("ciudades")).Single();
        await admin.GuardarAsync("ciudades", ciudad.Id, new("SPS · nombre corregido", true, ciudad.PadreId, ciudad), 7, "Admin");
        await service.CrearAsync(Equipo("INV-CIUDAD"));
        context.Add(new PoliticaReemplazoBaterias { MesesReemplazoLocal = 3, MesesReemplazoForanea = 6 });
        await context.SaveChangesAsync();
        var config = new ConfiguracionPresupuestoService(new ConfiguracionPresupuestoRepository(context));
        await config.CrearAsync(new() { Anio = 2029, CostoUnitarioBateria = 500 });
        var resultado = Assert.IsType<OkObjectResult>(await new PresupuestoController(context, config).ObtenerProyeccion(2029));
        var resumen = Assert.IsType<InventarioUPS.API.DTOs.Presupuesto.PresupuestoResumenDto>(resultado.Value);
        Assert.Equal(3, Assert.Single(Assert.Single(resumen.Agencias).UPS).MesesCicloReemplazo);
        var nueva = await admin.GuardarAsync("ciudades", 0, new("Nueva ciudad", true, ciudad.PadreId), 7, "Admin");
        Assert.False(nueva.EsLocal);
    }

    [Fact]
    public async Task MarcaRenombradaSeReflejaEnUPSperoNoSePuedeReclasificarModeloUtilizado()
    {
        var equipo = Equipo("INV-MARCA");
        await service.CrearAsync(equipo);
        var admin = new CatalogoAdminService(context);
        var marca = (await admin.ListarAsync("marcas-ups")).Single();
        await admin.GuardarAsync("marcas-ups", marca.Id, new("APC corregido", true, null, marca), 7, "Admin");
        context.ChangeTracker.Clear();
        Assert.Equal("APC corregido", (await service.ObtenerPorIdAsync(equipo.Id))!.Marca);
        var otra = await admin.GuardarAsync("marcas-ups", 0, new("Otra marca", true, null), 7, "Admin");
        var modelo = (await admin.ListarAsync("modelos-ups")).Single();
        await Assert.ThrowsAsync<InvalidOperationException>(() => admin.GuardarAsync("modelos-ups", modelo.Id, new(modelo.Nombre, true, otra.Id, modelo), 7, "Admin"));
    }

    [Theory]
    [InlineData("estados-ups")]
    [InlineData("estados-mantenimiento")]
    [InlineData("tipos-agencia")]
    [InlineData("permisos")]
    public async Task CatalogosFuncionalesNoSePuedenModificar(string catalogo)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => new CatalogoAdminService(context).GuardarAsync(catalogo, 0, new("Valor inventado", true, null), 7, "Admin"));
        Assert.Empty(await context.AuditoriaCatalogos.ToListAsync());
    }

    [Fact]
    public async Task PermisoDeCatalogosSeVerificaEnServidor()
    {
        var rol = new Rol { Nombre = "Consulta" };
        var usuario = new Usuario { NombreUsuario = "prueba", NombreCompleto = "Prueba", Correo = "prueba@example.test", PasswordHash = "prueba", Rol = rol };
        var permiso = new Permiso { Codigo = "GESTIONAR_CATALOGOS", Nombre = "Catálogos" };
        context.AddRange(usuario, permiso);
        await context.SaveChangesAsync();
        var http = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        http.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new[] {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, usuario.Id.ToString()) }, "Prueba"));
        var servicios = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddSingleton(servicios, context);
        http.RequestServices = Microsoft.Extensions.DependencyInjection.ServiceCollectionContainerBuilderExtensions.BuildServiceProvider(servicios);
        var action = new Microsoft.AspNetCore.Mvc.ActionContext(http, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
        var filtro = new InventarioUPS.API.Authorization.PermisoAuthorizeAttribute("GESTIONAR_CATALOGOS");
        var primero = new Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext(action, new List<Microsoft.AspNetCore.Mvc.Filters.IFilterMetadata>());
        await filtro.OnAuthorizationAsync(primero);
        Assert.IsType<ForbidResult>(primero.Result);
        context.RolPermisos.Add(new RolPermiso { RolId = rol.Id, PermisoId = permiso.Id });
        await context.SaveChangesAsync();
        var segundo = new Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext(action, new List<Microsoft.AspNetCore.Mvc.Filters.IFilterMetadata>());
        await filtro.OnAuthorizationAsync(segundo);
        Assert.Null(segundo.Result);
    }

    [Fact]
    public async Task CamposAnidadosNoPermitenCrearCatalogosDesdeUPS()
    {
        var equipo = Equipo("INV-RELACIONES");
        equipo.ModeloUPS = new ModeloUPS { Nombre = "Modelo no autorizado", MarcaUPS = new MarcaUPS { Nombre = "Marca no autorizada" } };
        equipo.ModeloBateria = new ModeloBateria { Nombre = "Batería no autorizada", TipoBateria = new TipoBateria { Nombre = "Tipo no autorizado" } };
        await service.CrearAsync(equipo);
        Assert.Single(await context.ModelosUPS.ToListAsync());
        Assert.Single(await context.MarcasUPS.ToListAsync());
        Assert.Single(await context.ModelosBateria.ToListAsync());
        var otro = Equipo("INV-LIBRE", "SERIE-LIBRE");
        otro.Modelo = "Modelo escrito libremente";
        await Assert.ThrowsAsync<ArgumentException>(() => service.CrearAsync(otro));
    }

    [Fact]
    public async Task CiudadInactivaSeConservaEnAgenciaExistentePeroNoSeAsignaANueva()
    {
        var ciudad = await context.Ciudades.SingleAsync();
        ciudad.Activa = false;
        (await context.Agencias.SingleAsync()).Direccion = "Dirección existente";
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var agencias = new AgenciaService(new AgenciaRepository(context), context);
        var agencia = (await agencias.ObtenerPorIdAsync(agenciaId))!;
        agencia.Nombre = "Nombre corregido";
        await agencias.ActualizarAsync(agencia);
        await Assert.ThrowsAsync<InvalidOperationException>(() => agencias.CrearAsync(new Agencia {
            Codigo = "NUEVA", Nombre = "Nueva", Direccion = "Dirección", CiudadId = ciudad.Id }));
    }

    [Fact]
    public async Task ReinicioNoRecreaCiudadesRenombradasNiReactivaDepartamentos()
    {
        var ciudad = await context.Ciudades.SingleAsync();
        ciudad.Nombre = "SPS corregida";
        (await context.Departamentos.SingleAsync()).Activo = false;
        await context.SaveChangesAsync();
        await DbSeeder.SeedAsync(context);
        Assert.Equal("SPS corregida", (await context.Ciudades.SingleAsync()).Nombre);
        Assert.False((await context.Departamentos.SingleAsync()).Activo);
        Assert.True((await context.Ciudades.SingleAsync()).EsLocal);
    }

    private sealed class SqlDinamicoVisitor(TSql160Parser parser) : TSqlFragmentVisitor
    {
        public List<ParseError> Errores { get; } = [];
        public override void ExplicitVisit(ExecuteSpecification node)
        {
            if (node.ExecutableEntity is ExecutableStringList lista)
                foreach (var valor in lista.Strings.OfType<StringLiteral>())
                {
                    parser.Parse(new StringReader(valor.Value), out var errores);
                    Errores.AddRange(errores);
                }
            base.ExplicitVisit(node);
        }
    }

    public async Task DisposeAsync()
    {
        await context.DisposeAsync();
        await connection.DisposeAsync();
    }
}
