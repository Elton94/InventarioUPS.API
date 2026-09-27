using System.Data;
using System.Text.Json;
using System.Text.RegularExpressions;
using InventarioUPS.API.Data;
using InventarioUPS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace InventarioUPS.API.Services;

public record CatalogoItem(int Id, string Nombre, bool Activo, int? PadreId = null, bool? EsLocal = null);
public record GuardarCatalogo(string Nombre, bool Activo, int? PadreId, CatalogoItem? Anterior = null);

public class CatalogoAdminService(InventarioUPSDbContext db)
{
    private record Definicion(Type Entidad, string Estado, string? Padre, int Longitud);
    private static readonly Dictionary<string, Definicion> Editables = new()
    {
        ["responsables-servicio"] = new(typeof(ResponsableServicio), "Activo", null, 150),
        ["departamentos"] = new(typeof(Departamento), "Activo", null, 100),
        ["ciudades"] = new(typeof(Ciudad), "Activa", "DepartamentoId", 100),
        ["tipos-bateria"] = new(typeof(TipoBateria), "Activo", null, 80),
        ["modelos-bateria"] = new(typeof(ModeloBateria), "Activo", "TipoBateriaId", 150),
        ["marcas-ups"] = new(typeof(MarcaUPS), "Activo", null, 100),
        ["modelos-ups"] = new(typeof(ModeloUPS), "Activo", "MarcaUPSId", 100)
    };
    public static string Normalizar(string? texto) => Regex.Replace(texto?.Trim() ?? "", @"\s+", " ");

    public async Task<List<CatalogoItem>> ListarAsync(string catalogo) => catalogo switch
    {
        "responsables-servicio" => await db.ResponsablesServicio.AsNoTracking().OrderBy(x => x.Nombre).Select(x => new CatalogoItem(x.Id, x.Nombre, x.Activo, null, null)).ToListAsync(),
        "departamentos" => await db.Departamentos.AsNoTracking().OrderBy(x => x.Nombre).Select(x => new CatalogoItem(x.Id, x.Nombre, x.Activo, null, null)).ToListAsync(),
        "ciudades" => await db.Ciudades.AsNoTracking().OrderBy(x => x.Nombre).Select(x => new CatalogoItem(x.Id, x.Nombre, x.Activa, x.DepartamentoId, x.EsLocal)).ToListAsync(),
        "tipos-bateria" => await db.TiposBateria.AsNoTracking().OrderBy(x => x.Nombre).Select(x => new CatalogoItem(x.Id, x.Nombre, x.Activo, null, null)).ToListAsync(),
        "modelos-bateria" => await db.ModelosBateria.AsNoTracking().OrderBy(x => x.Nombre).Select(x => new CatalogoItem(x.Id, x.Nombre, x.Activo, x.TipoBateriaId, null)).ToListAsync(),
        "marcas-ups" => await db.MarcasUPS.AsNoTracking().OrderBy(x => x.Nombre).Select(x => new CatalogoItem(x.Id, x.Nombre, x.Activo, null, null)).ToListAsync(),
        "modelos-ups" => await db.ModelosUPS.AsNoTracking().OrderBy(x => x.Nombre).Select(x => new CatalogoItem(x.Id, x.Nombre, x.Activo, x.MarcaUPSId, null)).ToListAsync(),
        "tipos-agencia" => await db.TiposAgencia.AsNoTracking().Select(x => new CatalogoItem(x.Id, x.Nombre, x.Activo, null, null)).ToListAsync(),
        "estados-ups" => await db.EstadosUPS.AsNoTracking().Select(x => new CatalogoItem(x.Id, x.Nombre, x.Activo, null, null)).ToListAsync(),
        "estados-mantenimiento" => await db.EstadosMantenimiento.AsNoTracking().Select(x => new CatalogoItem(x.Id, x.Nombre, x.Activo, null, null)).ToListAsync(),
        _ => throw new ArgumentException("El catálogo no existe.")
    };

    public async Task<CatalogoItem> GuardarAsync(string catalogo, int id, GuardarCatalogo solicitud, int usuarioId, string usuario)
    {
        if (!Editables.TryGetValue(catalogo, out var def))
            throw new ArgumentException("Este catálogo está protegido y no admite modificaciones.");
        var nombre = Normalizar(solicitud.Nombre);
        if (nombre.Length < 1 || nombre.Length > def.Longitud)
            throw new ArgumentException($"El nombre debe contener entre 1 y {def.Longitud} caracteres.");
        if (id < 0) throw new ArgumentException("El identificador no es válido.");
        if (def.Padre == null && solicitud.PadreId.HasValue)
            throw new ArgumentException("Este catálogo no admite una clasificación superior.");
        await using var transaccion = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var listado = await ListarAsync(catalogo);
        var antes = listado.FirstOrDefault(x => x.Id == id);
        if (id > 0 && antes == null) throw new KeyNotFoundException("El registro no existe.");
        if (id > 0 && solicitud.Anterior != antes)
            throw new InvalidOperationException("El registro cambió desde que abrió el formulario. Actualice la lista y vuelva a intentarlo.");
        if (listado.Any(x => x.Id != id && x.PadreId == solicitud.PadreId &&
            Normalizar(x.Nombre).Equals(nombre, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Ya existe una opción con ese nombre, incluso si está inactiva.");
        await ValidarRelacionAsync(catalogo, id, solicitud, antes);
        var entidad = id == 0 ? Activator.CreateInstance(def.Entidad)! : (await db.FindAsync(def.Entidad, id))!;
        var entry = db.Entry(entidad);
        entry.Property("Nombre").CurrentValue = nombre;
        entry.Property(def.Estado).CurrentValue = solicitud.Activo;
        if (def.Padre != null) entry.Property(def.Padre).CurrentValue = solicitud.PadreId!.Value;
        if (id == 0) db.Add(entidad);
        else if (entidad is BaseEntity baseEntity) baseEntity.FechaModificacion = DateTime.Now;
        await db.SaveChangesAsync();
        var registroId = (int)entry.Property("Id").CurrentValue!;
        if (catalogo is "marcas-ups" or "modelos-ups") await SincronizarNombresUPSAsync(catalogo, registroId);
        var despues = new CatalogoItem(registroId, nombre, solicitud.Activo, solicitud.PadreId, entidad is Ciudad c ? c.EsLocal : null);
        db.AuditoriaCatalogos.Add(new AuditoriaCatalogo {
            UsuarioId = usuarioId, Usuario = usuario, Catalogo = catalogo, RegistroId = registroId,
            Accion = id == 0 ? "Crear" : antes!.Activo != solicitud.Activo ? (solicitud.Activo ? "Activar" : "Desactivar") : "Editar",
            Antes = antes == null ? "" : JsonSerializer.Serialize(antes), Despues = JsonSerializer.Serialize(despues)
        });
        await db.SaveChangesAsync();
        await transaccion.CommitAsync();
        return despues;
    }

    private async Task ValidarRelacionAsync(string catalogo, int id, GuardarCatalogo solicitud, CatalogoItem? antes)
    {
        var mismoPadreInactivo = antes != null && antes.PadreId == solicitud.PadreId && (!solicitud.Activo || antes.Activo);
        var padreValido = catalogo switch {
            "ciudades" => await db.Departamentos.AnyAsync(x => x.Id == solicitud.PadreId && (x.Activo || mismoPadreInactivo)),
            "modelos-bateria" => await db.TiposBateria.AnyAsync(x => x.Id == solicitud.PadreId && (x.Activo || mismoPadreInactivo)),
            "modelos-ups" => await db.MarcasUPS.AnyAsync(x => x.Id == solicitud.PadreId && (x.Activo || mismoPadreInactivo)),
            _ => true
        };
        if (!padreValido) throw new ArgumentException("Seleccione una opción superior activa y válida.");
        if (antes != null && antes.PadreId != solicitud.PadreId)
        {
            var usado = catalogo switch {
                "ciudades" => await db.Agencias.AnyAsync(x => x.CiudadId == id) || antes.EsLocal == true,
                "modelos-bateria" => await db.UPSs.AnyAsync(x => x.ModeloBateriaId == id || x.ModeloBateriaBaseId == id) ||
                    await db.Mantenimientos.AnyAsync(x => x.ModeloBateriaId == id) || await db.CostosModeloBateria.AnyAsync(x => x.ModeloBateriaId == id),
                "modelos-ups" => await db.UPSs.AnyAsync(x => x.ModeloUPSId == id),
                _ => false
            };
            if (usado) throw new InvalidOperationException("No se puede cambiar la clasificación de una opción utilizada. Cree una nueva opción y desactive la anterior.");
        }
    }

    private async Task SincronizarNombresUPSAsync(string catalogo, int id)
    {
        var equipos = await db.UPSs.Include(x => x.ModeloUPS)!.ThenInclude(x => x!.MarcaUPS)
            .Where(x => catalogo == "modelos-ups" ? x.ModeloUPSId == id : x.ModeloUPS!.MarcaUPSId == id).ToListAsync();
        foreach (var equipo in equipos) {
            equipo.Modelo = equipo.ModeloUPS!.Nombre;
            equipo.Marca = equipo.ModeloUPS.MarcaUPS!.Nombre;
        }
    }
}
