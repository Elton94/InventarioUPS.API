namespace InventarioUPS.API.Models;

public class MarcaUPS
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
public class ModeloUPS
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public int MarcaUPSId { get; set; }
    public MarcaUPS? MarcaUPS { get; set; }
}
public class AuditoriaCatalogo
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public int UsuarioId { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Catalogo { get; set; } = string.Empty;
    public int RegistroId { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string Antes { get; set; } = string.Empty;
    public string Despues { get; set; } = string.Empty;
}
