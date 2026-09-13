namespace InventarioUPS.API.DTOs
{
    public class ActualizarUsuarioDTO
    {
        public string NombreCompleto { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public int RolId { get; set; }

        public bool Activo { get; set; }
    }
}