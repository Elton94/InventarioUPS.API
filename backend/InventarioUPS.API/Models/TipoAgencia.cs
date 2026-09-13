namespace InventarioUPS.API.Models
{
    public class TipoAgencia : BaseEntity
    {
        public string Nombre { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;

        public ICollection<Agencia> Agencias { get; set; } = new List<Agencia>();
    }
}