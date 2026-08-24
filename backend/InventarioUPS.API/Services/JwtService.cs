using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using InventarioUPS.API.Models;

using Microsoft.IdentityModel.Tokens;

namespace InventarioUPS.API.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(
            IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerarToken(
            Usuario usuario)
        {
            var key =
                _configuration["Jwt:Key"];

            var issuer =
                _configuration["Jwt:Issuer"];

            var audience =
                _configuration["Jwt:Audience"];

            var expirationMinutes =
                int.Parse(
                    _configuration[
                        "Jwt:ExpirationMinutes"
                    ] ?? "60"
                );

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new InvalidOperationException(
                    "La clave JWT no está configurada."
                );
            }

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuario.Id.ToString()
                ),

                new Claim(
                    ClaimTypes.Name,
                    usuario.NombreUsuario
                ),

                new Claim(
                    "NombreCompleto",
                    usuario.NombreCompleto
                ),

                new Claim(
                    ClaimTypes.Role,
                    usuario.Rol?.Nombre ?? string.Empty
                )
            };

            var securityKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(key)
                );

            var credentials =
                new SigningCredentials(
                    securityKey,
                    SecurityAlgorithms.HmacSha256
                );

            var tokenDescriptor =
                new SecurityTokenDescriptor
                {
                    Subject =
                        new ClaimsIdentity(claims),

                    Expires =
                        DateTime.UtcNow.AddMinutes(
                            expirationMinutes
                        ),

                    Issuer = issuer,

                    Audience = audience,

                    SigningCredentials =
                        credentials
                };

            var handler =
                new JwtSecurityTokenHandler();

            var token =
                handler.CreateToken(
                    tokenDescriptor
                );

            return handler.WriteToken(token);
        }
    }
}