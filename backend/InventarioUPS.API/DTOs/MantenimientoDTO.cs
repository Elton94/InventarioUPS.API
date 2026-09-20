namespace InventarioUPS.API.DTOs
{
    public class MantenimientoDTO
    {
        public int Id { get; set; }
        public int UPSId { get; set; }
        public int TipoMantenimientoId { get; set; }
        public int EstadoMantenimientoId { get; set; }
        public DateTime FechaMantenimiento { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string TecnicoProveedor { get; set; } = string.Empty;
        public decimal Costo { get; set; }
        public string Observaciones { get; set; } = string.Empty;

        public bool IncluyeCambioBaterias { get; set; }
        public int CantidadBateriasReemplazadas { get; set; }
        public decimal CostoBaterias { get; set; }

        public int? AgenciaIdRegistro { get; set; }
        public string? CodigoUPS { get; set; }
        public string? NumeroSerieUPS { get; set; }
        public string? MarcaUPS { get; set; }
        public string? ModeloUPS { get; set; }
        public string? AgenciaNombre { get; set; }
        public string? TipoMantenimiento { get; set; }
        public string? Estado { get; set; }
    }
}
