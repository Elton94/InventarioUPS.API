using InventarioUPS.API.DTOs;
using InventarioUPS.API.Repositories;
using Microsoft.AspNetCore.Identity;

namespace InventarioUPS.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPasswordHasher<Models.Usuario> _passwordHasher;
        private readonly IJwtService _jwtService;

        public AuthService(
            IUsuarioRepository usuarioRepository,
            IPasswordHasher<Models.Usuario> passwordHasher,
            IJwtService jwtService)
        {
            _usuarioRepository = usuarioRepository;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
        }

        public async Task<LoginResponseDTO> LoginAsync(
            LoginRequestDTO request)
        {
            var usuario =
                await _usuarioRepository
                    .ObtenerPorNombreUsuarioAsync(
                        request.NombreUsuario);

            if (usuario == null)
            {
                throw new UnauthorizedAccessException(
                    "Usuario o contraseña incorrectos.");
            }

            if (!usuario.Activo)
            {
                throw new UnauthorizedAccessException(
                    "El usuario se encuentra inactivo.");
            }

            var resultado =
                _passwordHasher.VerifyHashedPassword(
                    usuario,
                    usuario.PasswordHash,
                    request.Password);

            if (resultado ==
                PasswordVerificationResult.Failed)
            {
                throw new UnauthorizedAccessException(
                    "Usuario o contraseña incorrectos.");
            }

            var token =
                _jwtService.GenerarToken(usuario);

            return new LoginResponseDTO
            {
                Token = token,

                UsuarioId =
                    usuario.Id,

                NombreUsuario =
                    usuario.NombreUsuario,

                NombreCompleto =
                    usuario.NombreCompleto,

                Rol =
                    usuario.Rol?.Nombre ?? string.Empty
            };
        }
    }
}