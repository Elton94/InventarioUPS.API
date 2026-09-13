namespace InventarioUPS.API.Models
{
    public class EstadoUPS : BaseEntity
    {
        public string Nombre { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;

        public ICollection<UPS> UPSs { get; set; }
            = new List<UPS>();
    }
}