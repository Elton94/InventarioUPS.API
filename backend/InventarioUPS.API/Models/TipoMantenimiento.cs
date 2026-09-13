namespace InventarioUPS.API.Models
{
    public class TipoMantenimiento : BaseEntity
    {
        public string Nombre { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;

        public ICollection<Mantenimiento> Mantenimientos { get; set; }
            = new List<Mantenimiento>();
    }
}