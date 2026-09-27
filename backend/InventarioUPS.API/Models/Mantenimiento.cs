namespace InventarioUPS.API.Models
{
    public class Mantenimiento : BaseEntity
    {
        public int UPSId { get; set; }
        public int EstadoMantenimientoId { get; set; }
        public DateTime FechaMantenimiento { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public int? ResponsableUsuarioId { get; set; }
        public string TecnicoProveedor { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;

        public int? TipoBateriaId { get; set; }
        public decimal? VoltajeBateria { get; set; }
        public decimal? CapacidadBateriaAh { get; set; }
        public TipoBateria? TipoBateria { get; set; }
        public bool IncluyeCambioBaterias { get; set; }
        public int CantidadBateriasReemplazadas { get; set; }

        public int? AgenciaIdRegistro { get; set; }
        public string AgenciaNombreRegistro { get; set; } = string.Empty;
        public int? EstadoUPSAnteriorId { get; set; }

        public UPS? UPS { get; set; }
        public EstadoMantenimiento? EstadoMantenimiento { get; set; }
    }
}
