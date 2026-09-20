using System.Security.Claims;
using System.Text.Json;

namespace InventarioUPS.API.Middleware
{
    public class AuditoriaMiddleware
    {
        private static readonly SemaphoreSlim Semaforo = new(1, 1);
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _environment;

        public AuditoriaMiddleware(RequestDelegate next, IWebHostEnvironment environment)
        {
            _next = next;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            if (context.User.Identity?.IsAuthenticated != true ||
                !EsEscritura(context.Request.Method))
                return;

            var registro = new
            {
                fecha = DateTime.Now,
                usuarioId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                usuario = context.User.FindFirst(ClaimTypes.Name)?.Value,
                rol = context.User.FindFirst(ClaimTypes.Role)?.Value,
                metodo = context.Request.Method,
                ruta = context.Request.Path.Value,
                query = context.Request.QueryString.Value,
                estadoHttp = context.Response.StatusCode
            };

            var directorio = Path.Combine(_environment.ContentRootPath, "App_Data");
            Directory.CreateDirectory(directorio);

            var archivo = Path.Combine(directorio, "auditoria.log");
            var linea = JsonSerializer.Serialize(registro) + Environment.NewLine;

            await Semaforo.WaitAsync();
            try
            {
                await File.AppendAllTextAsync(archivo, linea);
            }
            finally
            {
                Semaforo.Release();
            }
        }

        private static bool EsEscritura(string metodo) =>
            HttpMethods.IsPost(metodo) ||
            HttpMethods.IsPut(metodo) ||
            HttpMethods.IsPatch(metodo) ||
            HttpMethods.IsDelete(metodo);
    }
}
