namespace InventarioUPS.API.Models
{
    public class Ciudad : BaseEntity
    {
        public string Nombre { get; set; } = string.Empty;

        public bool EsLocal { get; set; }

        public int DepartamentoId { get; set; }

        public bool Activa { get; set; } = true;

        public Departamento? Departamento { get; set; }

        public ICollection<Agencia> Agencias { get; set; } = new List<Agencia>();
    }
}