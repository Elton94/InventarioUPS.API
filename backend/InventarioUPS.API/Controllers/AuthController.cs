using InventarioUPS.API.DTOs;
using InventarioUPS.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventarioUPS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(
            IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDTO>> Login(
            LoginRequestDTO request)
        {
            try
            {
                var respuesta =
                    await _authService.LoginAsync(
                        request);

                return Ok(respuesta);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    mensaje = ex.Message
                });
            }
        }
    }
}