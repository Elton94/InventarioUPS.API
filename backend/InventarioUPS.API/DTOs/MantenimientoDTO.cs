namespace InventarioUPS.API.DTOs
{
    public class MantenimientoDTO
    {
        public int? ModeloBateriaId { get; set; }
        public string ModeloBateria { get; set; } = string.Empty;
        public string TipoBateria { get; set; } = string.Empty;
        public int? TipoBateriaId { get; set; }
        public int Id { get; set; }
        public int UPSId { get; set; }
        public int EstadoMantenimientoId { get; set; }
        public DateTime FechaMantenimiento { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string TecnicoProveedor { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;

        public int CantidadBateriasReemplazadas { get; set; }

        public int? AgenciaIdRegistro { get; set; }
        public string? NumeroInventarioUPS { get; set; }
        public string? NumeroSerieUPS { get; set; }
        public string? MarcaUPS { get; set; }
        public string? ModeloUPS { get; set; }
        public string? AgenciaNombre { get; set; }
        public string? Estado { get; set; }
    }
}
