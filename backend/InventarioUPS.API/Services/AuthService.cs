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
            if (request == null)
            {
                throw new UnauthorizedAccessException(
                    "Usuario o contraseña incorrectos.");
            }

            var nombreUsuario =
                request.NombreUsuario?.Trim();

            if (string.IsNullOrWhiteSpace(nombreUsuario) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                throw new UnauthorizedAccessException(
                    "Usuario o contraseña incorrectos.");
            }

            var usuario =
                await _usuarioRepository
                    .ObtenerPorNombreUsuarioAsync(
                        nombreUsuario);

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

            if (usuario.Rol == null ||
                !usuario.Rol.Activo)
            {
                throw new UnauthorizedAccessException(
                    "El rol asignado al usuario se encuentra inactivo.");
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
                    usuario.Rol.Nombre
            };
        }
    }
}