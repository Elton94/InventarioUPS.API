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
        var resultado = CalculadoraReemplazos.Calcular(new DateTime(2025, 12, 15), meses, 2029, new DateTime(2026, 9, 23));
        Assert.Equal(cantidad, resultado.Fechas.Count);
        Assert.All(resultado.Fechas, f => Assert.Equal(2029, f.Year));
    }

    [Fact]
    public void VencidosSeConsolidanSinRepetirCambiosOmitidos()
    {
        var resultado = CalculadoraReemplazos.Calcular(new DateTime(2024, 12, 15), 3, 2026, new DateTime(2026, 9, 23));
        Assert.Equal(new DateTime(2025, 3, 15), resultado.VencimientoPendiente);
        Assert.Equal(new[] { new DateTime(2026, 12, 15) }, resultado.Fechas);
    }

    [Fact]
    public void FechaDeHoyNoSeDuplicaComoVencida()
    {
        var resultado = CalculadoraReemplazos.Calcular(new DateTime(2026, 6, 23), 3, 2026, new DateTime(2026, 9, 23));
        Assert.Null(resultado.VencimientoPendiente);
        Assert.Equal(new[] { new DateTime(2026, 9, 23), new DateTime(2026, 12, 23) }, resultado.Fechas);
    }

    [Fact]
    public void PendienteIncluyeElCicloQueTambienVenceHoy()
    {
        var resultado = CalculadoraReemplazos.Calcular(new DateTime(2025, 12, 23), 3, 2026, new DateTime(2026, 9, 23));
        Assert.NotNull(resultado.VencimientoPendiente);
        Assert.Equal(new[] { new DateTime(2026, 12, 23) }, resultado.Fechas);
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
}
