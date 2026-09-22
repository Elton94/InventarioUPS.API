namespace InventarioUPS.API.Models
{
    public class UPS : BaseEntity
    {
        public string NumeroInventario { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string NumeroSerie { get; set; } = string.Empty;
        public int CapacidadVA { get; set; }
        public int AgenciaId { get; set; }
        public int EstadoUPSId { get; set; }
        public DateTime FechaInstalacion { get; set; }
        public int CantidadBaterias { get; set; }
        public DateTime? FechaUltimoCambioBaterias { get; set; }
        public DateTime? FechaUltimoCambioBateriasBase { get; set; }

        public Agencia? Agencia { get; set; }
        public EstadoUPS? EstadoUPS { get; set; }

        public ICollection<Mantenimiento> Mantenimientos { get; set; }
            = new List<Mantenimiento>();
    }
}
