namespace InventarioUPS.API.Models
{
    public class Agencia : BaseEntity
    {
        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string Departamento { get; set; } = string.Empty;

        public string Ciudad { get; set; } = string.Empty;

        public string Direccion { get; set; } = string.Empty;

        public string TipoAgencia { get; set; } = string.Empty;

        public bool Activa { get; set; } = true;
    }
}