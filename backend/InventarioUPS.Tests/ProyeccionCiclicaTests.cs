using InventarioUPS.API.Services;
using Xunit;

namespace InventarioUPS.Tests;

public class ProyeccionCiclicaTests
{
    [Theory]
    [InlineData(1, 12)]
    [InlineData(3, 4)]
    [InlineData(6, 2)]
    [InlineData(12, 1)]
    [InlineData(18, 0)]
    [InlineData(24, 1)]
    public void CuentaTodosLosCiclosDelAnio(int meses, int cantidad)
    {
        var resultado = CalculadoraReemplazos.Calcular(new DateTime(2025, 12, 15), meses, 2029, new DateTime(2026, 1, 1));
        Assert.Equal(cantidad, resultado.Fechas.Count);
        Assert.All(resultado.Fechas, f => Assert.Equal(2029, f.Year));
    }

    [Fact]
    public void VencidosSeConsolidanSinRepetirCambiosOmitidos()
    {
        var resultado = CalculadoraReemplazos.Calcular(new DateTime(2024, 12, 15), 3, 2026, new DateTime(2026, 9, 23));
        Assert.Equal(new DateTime(2025, 3, 15), resultado.VencimientoPendiente);
        Assert.Empty(resultado.Fechas);
    }

    [Fact]
    public void FechaDeHoyNoSeDuplicaComoVencida()
    {
        var resultado = CalculadoraReemplazos.Calcular(new DateTime(2026, 6, 23), 3, 2026, new DateTime(2026, 9, 23));
        Assert.Null(resultado.VencimientoPendiente);
        Assert.Equal(new[] { new DateTime(2026, 9, 23), new DateTime(2026, 12, 23) }, resultado.Fechas);
    }

    [Fact]
    public void SinAtencionNoSeReanudaElCalendarioAntiguo()
    {
        var resultado = CalculadoraReemplazos.Calcular(new DateTime(2025, 12, 23), 3, 2026, new DateTime(2026, 9, 23));
        Assert.NotNull(resultado.VencimientoPendiente);
        Assert.Empty(resultado.Fechas);
    }

    [Fact]
    public void FebreroNoDesplazaElDiaOriginalDeLosCiclos()
    {
        var resultado = CalculadoraReemplazos.Calcular(new DateTime(2028, 1, 31), 1, 2028, new DateTime(2028, 1, 31));
        Assert.Equal(new DateTime(2028, 2, 29), resultado.Fechas[0]);
        Assert.Equal(new DateTime(2028, 3, 31), resultado.Fechas[1]);
        Assert.Equal(new DateTime(2028, 4, 30), resultado.Fechas[2]);
    }

    [Fact]
    public void CicloLargoPuedeNoTenerReemplazosEnElAnio()
    {
        var resultado = CalculadoraReemplazos.Calcular(new DateTime(2026, 1, 1), 24, 2027, new DateTime(2026, 9, 23));
        Assert.Empty(resultado.Fechas);
        Assert.Null(resultado.VencimientoPendiente);
    }

    [Theory]
    [InlineData(2026)]
    [InlineData(2027)]
    [InlineData(2030)]
    public void SinFechaNoSeInventanCiclosEnNingunAnio(int anio)
    {
        var r = CalculadoraReemplazos.Calcular(new(2023, 7, 10), 18, anio, new(2026, 1, 1));
        Assert.Equal(new DateTime(2025, 1, 10), r.VencimientoPendiente);
        Assert.Empty(r.Fechas);
        Assert.Null(r.FechaAtencionPendiente);
    }

    [Fact]
    public void AtencionEn2026ProyectaForaneaEnJulio2027()
    {
        var r = CalculadoraReemplazos.Calcular(new(2023, 7, 10), 18, 2027, new(2026, 1, 1), new(2026, 1, 10));
        Assert.Equal(new[] { new DateTime(2027, 7, 10) }, r.Fechas);
    }

    [Fact]
    public void AtencionEn2027SeCuentaUnaVezYLocalSigueEn2029()
    {
        var r = CalculadoraReemplazos.Calcular(new(2023, 1, 10), 24, 2027, new(2026, 1, 1), new(2027, 1, 10));
        Assert.Equal(new[] { new DateTime(2027, 1, 10) }, r.Fechas);
        var siguiente = CalculadoraReemplazos.Calcular(new(2023, 1, 10), 24, 2029, new(2026, 1, 1), new(2027, 1, 10));
        Assert.Equal(new[] { new DateTime(2029, 1, 10) }, siguiente.Fechas);
    }

    [Fact]
    public void ProgramacionVencidaNoSirveComoAtencionFutura()
    {
        var r = CalculadoraReemplazos.Calcular(new(2023, 1, 10), 24, 2027, new(2026, 1, 1), new(2025, 12, 31));
        Assert.Null(r.FechaAtencionPendiente);
        Assert.Empty(r.Fechas);
    }

    [Fact]
    public void AtencionHoySeCuentaUnaSolaVezInclusoConCiclosCortos()
    {
        var r = CalculadoraReemplazos.Calcular(new(2024, 1, 10), 3, 2026, new(2026, 9, 23), new(2026, 9, 23));
        Assert.Equal(new[] { new DateTime(2026, 9, 23), new DateTime(2026, 12, 23) }, r.Fechas);
    }

    [Fact]
    public void CambioRealReemplazaLaEstimacionAnterior()
    {
        var r = CalculadoraReemplazos.Calcular(new(2026, 3, 12), 18, 2027, new(2026, 3, 12));
        Assert.Null(r.VencimientoPendiente);
        Assert.Equal(new[] { new DateTime(2027, 9, 12) }, r.Fechas);
    }
}
