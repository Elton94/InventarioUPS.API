namespace InventarioUPS.API.Models
{
    public class UPS : BaseEntity
    {
        public string Codigo { get; set; } = string.Empty;

        public string Marca { get; set; } = string.Empty;

        public string Modelo { get; set; } = string.Empty;

        public string NumeroSerie { get; set; } = string.Empty;

        public int CapacidadVA { get; set; }

        public int AgenciaId { get; set; }

        public DateTime FechaInstalacion { get; set; }

        public string Estado { get; set; } = "Operativo";

        public int CantidadBaterias { get; set; } = 0;

        public DateTime? FechaUltimoCambioBaterias { get; set; }

        public Agencia? Agencia { get; set; }

        public ICollection<Mantenimiento> Mantenimientos { get; set; } = new List<Mantenimiento>();
    }
}