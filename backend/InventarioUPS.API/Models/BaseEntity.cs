namespace InventarioUPS.API.Models
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public DateTime? FechaModificacion { get; set; }
    }
}