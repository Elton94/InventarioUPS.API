using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using InventarioUPS.API.Data;
using InventarioUPS.API.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace InventarioUPS.API.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly InventarioUPSDbContext _context;

        public JwtService(
            IConfiguration configuration,
            InventarioUPSDbContext context)
        {
            _configuration = configuration;
            _context = context;
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


            var permisos = _context.RolPermisos
                .AsNoTracking()
                .Where(rp =>
                    rp.RolId == usuario.RolId &&
                    rp.Permiso != null &&
                    rp.Permiso.Activo)
                .Select(rp => rp.Permiso!.Codigo)
                .Distinct()
                .ToList();


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

            foreach (var permiso in permisos)
            {
                claims.Add(
                    new Claim(
                        "Permiso",
                        permiso
                    )
                );
            }

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