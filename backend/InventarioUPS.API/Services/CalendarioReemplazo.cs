namespace InventarioUPS.API.Services;

public sealed record CalendarioReemplazo(List<DateTime> Fechas, DateTime? VencimientoPendiente,
    DateTime? FechaAtencionPendiente = null);

public static class CalculadoraReemplazos
{
    public static CalendarioReemplazo Calcular(DateTime fechaBase, int meses, int anio, DateTime hoy,
        DateTime? fechaAtencion = null)
    {
        if (meses is < 1 or > 120) throw new ArgumentOutOfRangeException(nameof(meses));
        if (anio < hoy.Year || anio > 2100) throw new ArgumentOutOfRangeException(nameof(anio));
        if (fechaBase == default || fechaBase.Date > hoy.Date)
            throw new ArgumentException("La fecha base no es válida.", nameof(fechaBase));

        hoy = hoy.Date;
        var primera = fechaBase.Date.AddMonths(meses);
        var pendiente = primera < hoy ? (DateTime?)primera : null;
        var atencion = pendiente.HasValue && fechaAtencion?.Date >= hoy ? fechaAtencion.Value.Date : (DateTime?)null;
        var fechas = new List<DateTime>();
        if (pendiente.HasValue && !atencion.HasValue)
            return new(fechas, pendiente);

        var inicio = anio == hoy.Year ? hoy : new DateTime(anio, 1, 1);
        var fin = new DateTime(anio, 12, 31);
        var baseCalculo = atencion ?? fechaBase.Date;
        var ciclo = atencion.HasValue ? 0 : 1;
        while (true)
        {
            var mesesSumados = ciclo * meses;
            if (mesesSumados > (9999 - baseCalculo.Year) * 12 + 12 - baseCalculo.Month) break;
            var fecha = baseCalculo.AddMonths(mesesSumados);
            if (fecha > fin) break;
            if (fecha >= inicio) fechas.Add(fecha);
            ciclo++;
        }
        return new(fechas, pendiente, atencion);
    }
}
