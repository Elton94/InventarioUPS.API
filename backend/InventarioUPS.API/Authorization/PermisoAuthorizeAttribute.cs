using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace InventarioUPS.API.Authorization
{
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Method,
        AllowMultiple = true)]
    public class PermisoAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _permiso;

        public PermisoAuthorizeAttribute(string permiso)
        {
            _permiso = permiso;
        }

        public Task OnAuthorizationAsync(
            AuthorizationFilterContext context)
        {
            var usuario = context.HttpContext.User;

            /*
             * Control Total tiene acceso completo al sistema.
             */
            if (usuario.IsInRole("Control Total"))
            {
                return Task.CompletedTask;
            }

            /*
             * Verificar si el JWT contiene el permiso solicitado.
             */
            var tienePermiso = usuario.Claims.Any(
                claim =>
                    claim.Type == "Permiso" &&
                    claim.Value.Equals(
                        _permiso,
                        StringComparison.OrdinalIgnoreCase));

            if (!tienePermiso)
            {
                context.Result = new ForbidResult();
            }

            return Task.CompletedTask;
        }
    }
}