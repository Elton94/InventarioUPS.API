using InventarioUPS.API.Models;

namespace InventarioUPS.API.Services
{
    public interface IJwtService
    {
        string GenerarToken(Usuario usuario);
    }
}