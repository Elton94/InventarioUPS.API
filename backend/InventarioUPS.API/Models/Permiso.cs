namespace InventarioUPS.API.Models
{
    public class Permiso : BaseEntity
    {
        public string Nombre { get; set; } = string.Empty;

        public string Codigo { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;

        public ICollection<RolPermiso> RolPermisos { get; set; }
            = new List<RolPermiso>();
    }
}