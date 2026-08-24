using InventarioUPS.API.DTOs;

namespace InventarioUPS.API.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDTO> LoginAsync(
            LoginRequestDTO request);
    }
}