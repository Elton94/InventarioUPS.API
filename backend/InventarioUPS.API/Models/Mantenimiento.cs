namespace InventarioUPS.API.Models
{
    public class Mantenimiento : BaseEntity
    {
        public int UPSId { get; set; }
        public int EstadoMantenimientoId { get; set; }
        public DateTime FechaMantenimiento { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string TecnicoProveedor { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;

        public bool IncluyeCambioBaterias { get; set; }
        public int CantidadBateriasReemplazadas { get; set; }

        public int? AgenciaIdRegistro { get; set; }
        public string AgenciaNombreRegistro { get; set; } = string.Empty;
        public int? EstadoUPSAnteriorId { get; set; }

        public UPS? UPS { get; set; }
        public EstadoMantenimiento? EstadoMantenimiento { get; set; }
    }
}
