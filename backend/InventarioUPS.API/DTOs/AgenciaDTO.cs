namespace InventarioUPS.API.DTOs
{
    public class AgenciaDTO
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int CiudadId { get; set; }
        public string CiudadNombre { get; set; } = string.Empty;
        public int DepartamentoId { get; set; }
        public string DepartamentoNombre { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public int TipoAgenciaId { get; set; }
        public string TipoAgenciaNombre { get; set; } = string.Empty;
        public bool Activa { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }
}
