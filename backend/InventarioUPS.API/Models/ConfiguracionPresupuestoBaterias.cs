namespace InventarioUPS.API.Models
{
    public class ConfiguracionPresupuestoBaterias : BaseEntity
    {
        public int Anio { get; set; }

        public int MesesReemplazoLocal { get; set; }

        public int MesesReemplazoForanea { get; set; }

        public decimal CostoUnitarioBateria { get; set; }

        public bool Activo { get; set; } = true;
    }
}