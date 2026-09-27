namespace InventarioUPS.API.Models;

public class TipoBateria
{
    public bool Activo { get; set; } = true;
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public class ModeloBateria
{
    public bool Activo { get; set; } = true;
    public int Id { get; set; }
    public int TipoBateriaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public TipoBateria? TipoBateria { get; set; }
}

public class CostoModeloBateria
{
    public int Id { get; set; }
    public int ModeloBateriaId { get; set; }
    public int Anio { get; set; }
    public decimal CostoUnitario { get; set; }
    public ModeloBateria? ModeloBateria { get; set; }
}
