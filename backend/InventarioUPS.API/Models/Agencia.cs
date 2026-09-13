namespace InventarioUPS.API.Models
{
    public class Agencia : BaseEntity
    {
        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public int CiudadId { get; set; }

        public string Direccion { get; set; } = string.Empty;

        public int TipoAgenciaId { get; set; }

        public bool Activa { get; set; } = true;

        public Ciudad? Ciudad { get; set; }

        public TipoAgencia? TipoAgencia { get; set; }

        public ICollection<UPS> UPSs { get; set; }
            = new List<UPS>();
    }
}