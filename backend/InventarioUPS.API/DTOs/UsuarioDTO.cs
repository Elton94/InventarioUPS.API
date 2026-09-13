namespace InventarioUPS.API.DTOs
{
    public class UsuarioDTO
    {
        public int Id { get; set; }

        public string NombreUsuario { get; set; } = string.Empty;

        public string NombreCompleto { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public int RolId { get; set; }

        public string RolNombre { get; set; } = string.Empty;

        public bool Activo { get; set; }
    }
}