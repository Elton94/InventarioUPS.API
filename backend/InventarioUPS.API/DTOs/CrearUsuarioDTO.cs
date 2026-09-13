namespace InventarioUPS.API.DTOs
{
    public class CrearUsuarioDTO
    {
        public string NombreUsuario { get; set; } = string.Empty;

        public string NombreCompleto { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public int RolId { get; set; }
    }
}