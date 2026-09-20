using System.Security.Claims;
using InventarioUPS.API.Data;
using Microsoft.EntityFrameworkCore;

namespace InventarioUPS.API.Middleware
{
    public class SesionActivaMiddleware
    {
        private readonly RequestDelegate _next;

        public SesionActivaMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, InventarioUPSDbContext db)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                if (!int.TryParse(
                        context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        out var usuarioId))
                {
                    await NoAutorizado(context);
                    return;
                }

                var usuario = await db.Usuarios
                    .AsNoTracking()
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.Id == usuarioId);

                var rolToken = context.User.FindFirst(ClaimTypes.Role)?.Value;

                if (usuario == null || !usuario.Activo ||
                    usuario.Rol == null || !usuario.Rol.Activo ||
                    !string.Equals(
                        usuario.Rol.Nombre,
                        rolToken,
                        StringComparison.OrdinalIgnoreCase))
                {
                    await NoAutorizado(context);
                    return;
                }
            }

            await _next(context);
        }

        private static async Task NoAutorizado(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                mensaje =
                    "La sesión ya no es válida. El usuario o su rol cambió o fue desactivado. Inicie sesión nuevamente."
            });
        }
    }
}
