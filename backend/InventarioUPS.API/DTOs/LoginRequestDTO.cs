namespace InventarioUPS.API.DTOs
{
    public class LoginRequestDTO
    {
        public string NombreUsuario { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}