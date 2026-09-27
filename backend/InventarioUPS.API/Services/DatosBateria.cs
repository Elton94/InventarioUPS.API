namespace InventarioUPS.API.Services;

public static class DatosBateria
{
    public static void Validar(decimal? voltaje, decimal? capacidad)
    {
        if (!EsValido(voltaje) || !EsValido(capacidad))
            throw new ArgumentException("Indique voltaje (V) y capacidad (Ah) por batería, mayores que cero, menores de 1000000 y con hasta tres decimales.");
    }

    private static bool EsValido(decimal? valor) => valor.HasValue && valor > 0 && valor < 1000000m && decimal.Round(valor.Value, 3) == valor;
}
