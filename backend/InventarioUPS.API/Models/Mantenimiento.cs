using System.ComponentModel.DataAnnotations;
namespace InventarioUPS.API.Models
{
    public class Mantenimiento : BaseEntity
    {
        public int UPSId { get; set; }

        public int TipoMantenimientoId { get; set; }

        public int EstadoMantenimientoId { get; set; }

        public DateTime FechaMantenimiento { get; set; }

        public string Descripcion { get; set; } = string.Empty;

        public string TecnicoProveedor { get; set; } = string.Empty;

        public decimal Costo { get; set; }

        public string Observaciones { get; set; } = string.Empty;

        public UPS? UPS { get; set; }

        public TipoMantenimiento? Tipo { get; set; }

        public EstadoMantenimiento? EstadoMantenimiento { get; set; }

        public bool IncluyeCambioBaterias { get; set; } = false;

        public int CantidadBateriasReemplazadas { get; set; } = 0;

        public decimal CostoBaterias { get; set; } = 0;
    }
}