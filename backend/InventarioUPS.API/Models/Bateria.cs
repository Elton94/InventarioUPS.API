namespace InventarioUPS.API.Models;

public class TipoBateria
{
    public bool Activo { get; set; } = true;
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public class CostoTipoBateria
{
    public int Id { get; set; }
    public int TipoBateriaId { get; set; }
    public int Anio { get; set; }
    public decimal CostoUnitario { get; set; }
    public TipoBateria? TipoBateria { get; set; }
}
