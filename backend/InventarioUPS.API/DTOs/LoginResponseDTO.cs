namespace InventarioUPS.API.DTOs
{
    public class LoginResponseDTO
    {
        public string Token { get; set; } = string.Empty;

        public int UsuarioId { get; set; }

        public string NombreUsuario { get; set; } = string.Empty;

        public string NombreCompleto { get; set; } = string.Empty;

        public string Rol { get; set; } = string.Empty;
    }
}   