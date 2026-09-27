namespace InventarioUPS.API.DTOs
{
    public class UPSDTO
    {
        public string TipoBateria { get; set; } = string.Empty;
        public int? TipoBateriaId { get; set; }
        public decimal? VoltajeBateria { get; set; }
        public decimal? CapacidadBateriaAh { get; set; }
        public int Id { get; set; }
        public string NumeroInventario { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string NumeroSerie { get; set; } = string.Empty;
        public int CapacidadVA { get; set; }
        public int AgenciaId { get; set; }
        public string AgenciaCodigo { get; set; } = string.Empty;
        public string AgenciaNombre { get; set; } = string.Empty;
        public string CiudadNombre { get; set; } = string.Empty;
        public string DepartamentoNombre { get; set; } = string.Empty;
        public int EstadoUPSId { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaInstalacion { get; set; }
        public int CantidadBaterias { get; set; }
        public DateTime? FechaUltimoCambioBaterias { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }
}
