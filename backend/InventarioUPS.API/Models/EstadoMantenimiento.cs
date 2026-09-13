namespace InventarioUPS.API.Models
{
    public class EstadoMantenimiento : BaseEntity
    {
        public string Nombre { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;

        public ICollection<Mantenimiento> Mantenimientos { get; set; }
            = new List<Mantenimiento>();
    }
}