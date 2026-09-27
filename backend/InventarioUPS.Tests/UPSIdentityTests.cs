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
            Ciudad = new Ciudad { Nombre = "San Pedro Sula", Departamento = new Departamento { Nombre = "Cortés" } },
            TipoAgencia = new TipoAgencia { Nombre = "Local" }
        };
        var estado = new EstadoUPS { Nombre = "Operativo" };
        context.AddRange(agencia, estado, new ModeloBateria { Id = 1, Nombre = "AGM de prueba", TipoBateria = new TipoBateria { Nombre = "AGM" } });
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
        { ModeloBateriaId = 1,
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
        { ModeloBateriaId = 1,
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
        { ModeloBateriaId = 1,
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
        { ModeloBateriaId = 1,
            UPSId = ups.Id,
            EstadoMantenimientoId = programado.Id, FechaMantenimiento = DateTime.Today,
            Descripcion = "Revisión", TecnicoProveedor = "Técnico", IncluyeCambioBaterias = true
        };
        await mantenimientos.CrearAsync(nuevo);
        Assert.Equal(2, nuevo.CantidadBateriasReemplazadas);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        await mantenimientos.ActualizarAsync(new Mantenimiento
        { ModeloBateriaId = 1,
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
        { ModeloBateriaId = 1,
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
        { ModeloBateriaId = 1,
            UPSId = ups.Id, EstadoMantenimientoId = completado.Id, FechaMantenimiento = DateTime.Today,
            Descripcion = "Banco completo", TecnicoProveedor = "Técnico"
        };
        await cambios.CrearAsync(cambio);
        context.ChangeTracker.Clear();
        var editarUPS = (await service.ObtenerPorIdAsync(ups.Id))!;
        editarUPS.Modelo = "Corregido";
        await service.ActualizarAsync(editarUPS);
        context.ChangeTracker.Clear();
        await cambios.ActualizarAsync(new Mantenimiento
        { ModeloBateriaId = 1,
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
        { ModeloBateriaId = 1,
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
        { ModeloBateriaId = 1,
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
        { ModeloBateriaId = 1,
            UPSId = ups.Id, EstadoMantenimientoId = completado.Id,
            FechaMantenimiento = DateTime.Today.AddDays(-10), TecnicoProveedor = "Técnico", Descripcion = "Anterior"
        };
        await cambios.CrearAsync(anterior);
        var actual = new Mantenimiento
        { ModeloBateriaId = 1,
            UPSId = ups.Id, EstadoMantenimientoId = completado.Id,
            FechaMantenimiento = DateTime.Today, TecnicoProveedor = "Técnico", Descripcion = "Actual"
        };
        await cambios.CrearAsync(actual);
        ups.CantidadBaterias = 4;
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var editar = new Mantenimiento
        { ModeloBateriaId = 1,
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
        var cambio = new Mantenimiento { ModeloBateriaId = 1, UPSId = ups.Id, EstadoMantenimientoId = proceso.Id,
            FechaMantenimiento = ups.FechaInstalacion.AddDays(-1), Descripcion = "Reemplazo", TecnicoProveedor = "Técnico" };
        await Assert.ThrowsAsync<ArgumentException>(() => cambios.CrearAsync(cambio));
        cambio.FechaMantenimiento = DateTime.Today;
        await cambios.CrearAsync(cambio);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CambiarEstadoAsync(ups.Id, "Operativo"));
        context.ChangeTracker.Clear();
        await cambios.ActualizarAsync(new Mantenimiento { ModeloBateriaId = 1, Id = cambio.Id, UPSId = ups.Id,
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
    public async Task ModeloEsObligatorioYDebeExistir()
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
        var cambio = new Mantenimiento { UPSId = ups.Id, ModeloBateriaId = modelo.Id,
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
