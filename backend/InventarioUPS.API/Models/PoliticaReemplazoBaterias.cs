namespace InventarioUPS.API.Models
{
    public class PoliticaReemplazoBaterias : BaseEntity
    {
        public int MesesReemplazoLocal { get; set; }

        public int MesesReemplazoForanea { get; set; }
    }
}