using System.Security.Claims;
using InventarioUPS.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace InventarioUPS.API.Authorization
{
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Method,
        AllowMultiple = false)]
    public class PermisoAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string[] _permisos;

        public PermisoAuthorizeAttribute(params string[] permisos)
        {
            _permisos = permisos ?? Array.Empty<string>();
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var principal = context.HttpContext.User;

            if (principal.Identity?.IsAuthenticated != true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            if (!int.TryParse(
                    principal.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    out var usuarioId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var db = context.HttpContext.RequestServices
                .GetRequiredService<InventarioUPSDbContext>();

            var usuario = await db.Usuarios
                .AsNoTracking()
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario == null || !usuario.Activo ||
                usuario.Rol == null || !usuario.Rol.Activo)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            if (usuario.Rol.Nombre.Equals(
                    "Control Total",
                    StringComparison.OrdinalIgnoreCase))
                return;

            if (_permisos.Length == 0)
                return;

            var requeridos = _permisos
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim().ToUpper())
                .ToList();

            var autorizado = await db.RolPermisos
                .AsNoTracking()
                .AnyAsync(rp =>
                    rp.RolId == usuario.RolId &&
                    rp.Permiso != null &&
                    rp.Permiso.Activo &&
                    requeridos.Contains(rp.Permiso.Codigo.ToUpper()));

            if (!autorizado)
                context.Result = new ForbidResult();
        }
    }
}
