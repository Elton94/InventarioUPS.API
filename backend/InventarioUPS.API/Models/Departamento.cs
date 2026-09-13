namespace InventarioUPS.API.Models
{
    public class Departamento : BaseEntity
    {
        public string Nombre { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;

        public ICollection<Ciudad> Ciudades { get; set; } = new List<Ciudad>();
    }
}