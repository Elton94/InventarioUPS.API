namespace InventarioUPS.API.Models
{
    public class Usuario : BaseEntity
    {
        public string NombreUsuario { get; set; } = string.Empty;

        public string NombreCompleto { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public int RolId { get; set; }

        public bool Activo { get; set; } = true;

        public Rol? Rol { get; set; }
    }
}