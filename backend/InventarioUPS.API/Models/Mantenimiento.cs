using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioUPS.API.Models
{
    public class Mantenimiento : BaseEntity
    {
        [Required]
        public int UPSId { get; set; }

        [Required]
        public string TipoMantenimiento { get; set; } = string.Empty;

        [Required]
        public DateTime FechaMantenimiento { get; set; }

        public string Descripcion { get; set; } = string.Empty;

        public string TecnicoProveedor { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Costo { get; set; }

        public string Estado { get; set; } = "Completado";

        public string Observaciones { get; set; } = string.Empty;

        public UPS? UPS { get; set; }
    }
}