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
        context.AddRange(agencia, estado);
        await context.SaveChangesAsync();
        agenciaId = agencia.Id;
        estadoId = estado.Id;
        service = new(new UPSRepository(context), context);
    }

    private UPS Equipo(string inventario, string serie = "SERIE-1") => new()
    {
        NumeroInventario = inventario, NumeroSerie = serie, Marca = "APC", Modelo = "Prueba",
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
        var tipo = new TipoMantenimiento { Nombre = "Preventivo" };
        var programado = new EstadoMantenimiento { Nombre = "Programado" };
        context.AddRange(tipo, programado, new EstadoUPS { Nombre = "De baja" });
        await context.SaveChangesAsync();
        var mantenimiento = new Mantenimiento
        {
            UPSId = ups.Id, TipoMantenimientoId = tipo.Id,
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
        {
            UPSId = equipo.Id, IncluyeCambioBaterias = true,
            FechaMantenimiento = new DateTime(DateTime.Today.Year, 12, 31),
            Tipo = new TipoMantenimiento { Nombre = "Cambio de baterías" },
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
        {
            UPSId = ups.Id,
            Tipo = new TipoMantenimiento { Nombre = "Preventivo" },
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
        var ups = Equipo("INV-COSTOS");
        await service.CrearAsync(ups);
        var tipo = new TipoMantenimiento { Nombre = "Preventivo" };
        var programado = new EstadoMantenimiento { Nombre = "Programado" };
        var completado = new EstadoMantenimiento { Nombre = "Completado" };
        context.AddRange(tipo, programado, completado);
        await context.SaveChangesAsync();
        var mantenimientos = new MantenimientoService(new MantenimientoRepository(context), context);
        var nuevo = new Mantenimiento
        {
            UPSId = ups.Id, TipoMantenimientoId = tipo.Id,
            EstadoMantenimientoId = programado.Id, FechaMantenimiento = DateTime.Today,
            Descripcion = "Revisión", TecnicoProveedor = "Técnico", IncluyeCambioBaterias = true
        };
        await mantenimientos.CrearAsync(nuevo);
        Assert.Equal(2, nuevo.CantidadBateriasReemplazadas);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        await mantenimientos.ActualizarAsync(new Mantenimiento
        {
            Id = nuevo.Id, UPSId = ups.Id, TipoMantenimientoId = tipo.Id,
            EstadoMantenimientoId = completado.Id, FechaMantenimiento = DateTime.Today,
            Descripcion = "Revisión completada", TecnicoProveedor = "Técnico",
            IncluyeCambioBaterias = cambioBaterias
        });
        context.ChangeTracker.Clear();
        var guardado = await context.Mantenimientos.SingleAsync();
        Assert.Equal(cambioBaterias ? 2 : 0, guardado.CantidadBateriasReemplazadas);
        if (cambioBaterias)
            Assert.Equal(DateTime.Today, (await context.UPSs.SingleAsync()).FechaUltimoCambioBaterias);
    }

    [Theory]
    [InlineData("MigracionUPS.sql")]
    [InlineData("MantenimientosSinCostos.sql")]
    public void ScriptDeMigracionTieneSintaxisTSQLValida(string archivo)
    {
        var parser = new TSql160Parser(true);
        var fragment = parser.Parse(new StringReader(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, archivo))), out var errors);
        Assert.Empty(errors);
        var visitor = new SqlDinamicoVisitor(parser);
        fragment.Accept(visitor);
        Assert.Empty(visitor.Errores);
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
