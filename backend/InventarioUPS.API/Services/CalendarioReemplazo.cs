namespace InventarioUPS.API.Services;

public sealed record CalendarioReemplazo(List<DateTime> Fechas, DateTime? VencimientoPendiente);

public static class CalculadoraReemplazos
{
    public static CalendarioReemplazo Calcular(DateTime fechaBase, int meses, int anio, DateTime hoy)
    {
        if (meses is < 1 or > 120)
            throw new ArgumentOutOfRangeException(nameof(meses));
        if (anio < hoy.Year || anio > 2100)
            throw new ArgumentOutOfRangeException(nameof(anio));
        if (fechaBase == default || fechaBase.Date > hoy.Date)
            throw new ArgumentException("La fecha base no es válida.", nameof(fechaBase));

        var primera = fechaBase.Date.AddMonths(meses);
        var pendiente = primera < hoy.Date ? (DateTime?)primera : null;
        var inicio = anio == hoy.Year ? hoy.Date : new DateTime(anio, 1, 1);
        var fin = new DateTime(anio, 12, 31);
        var fechas = new List<DateTime>();
        var mesesHastaInicio = (inicio.Year - fechaBase.Year) * 12 + inicio.Month - fechaBase.Month;
        var ciclo = Math.Max(1, mesesHastaInicio / meses);

        // Cada fecha parte del mismo día base para no arrastrar el ajuste de febrero.
        for (var fecha = fechaBase.Date.AddMonths(ciclo * meses); fecha <= fin;
             fecha = fechaBase.Date.AddMonths(++ciclo * meses))
        {
            var incluidoEnPendiente = anio == hoy.Year && pendiente.HasValue && fecha == hoy.Date;
            if (fecha >= inicio && !incluidoEnPendiente) fechas.Add(fecha);
        }
        return new CalendarioReemplazo(fechas, pendiente);
    }
}
