using InventarioUPS.API.Data;
using InventarioUPS.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventarioUPS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioInicialController : ControllerBase
    {
        private readonly InventarioUPSDbContext _context;
        private readonly IPasswordHasher<Usuario> _passwordHasher;

        public UsuarioInicialController(
            InventarioUPSDbContext context,
            IPasswordHasher<Usuario> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [HttpPost]
        public async Task<IActionResult> Crear()
        {
            var rol = await _context.Roles
                .FirstOrDefaultAsync(
                    r => r.Nombre == "Control Total");

            if (rol == null)
            {
                rol = new Rol
                {
                    Nombre = "Control Total",
                    Descripcion =
                        "Acceso completo al sistema",
                    Activo = true
                };

                _context.Roles.Add(rol);

                await _context.SaveChangesAsync();
            }

            var usuarioExistente =
                await _context.Usuarios
                    .FirstOrDefaultAsync(
                        u => u.NombreUsuario == "admin");

            if (usuarioExistente != null)
            {
                return BadRequest(
                    "El usuario admin ya existe."
                );
            }

            var usuario = new Usuario
            {
                NombreUsuario = "admin",
                NombreCompleto =
                    "Administrador del Sistema",
                Correo =
                    "admin@inventarioups.com",
                RolId = rol.Id,
                Activo = true
            };

            usuario.PasswordHash =
                _passwordHasher.HashPassword(
                    usuario,
                    "Admin123*"
                );

            _context.Usuarios.Add(usuario);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje =
                    "Usuario inicial creado correctamente."
            });
        }
    }
}