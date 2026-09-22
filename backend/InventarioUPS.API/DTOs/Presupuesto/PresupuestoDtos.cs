namespace InventarioUPS.API.DTOs.Presupuesto
{
    public class ConfiguracionPresupuestoDto
    {
        public int Id { get; set; }
        public int Anio { get; set; }
        public decimal CostoUnitarioBateria { get; set; }
        public bool Activo { get; set; }
    }

    public class CrearConfiguracionPresupuestoDto
    {
        public int Anio { get; set; }
        public decimal CostoUnitarioBateria { get; set; }
    }

    public class ActualizarConfiguracionPresupuestoDto
    {
        public decimal CostoUnitarioBateria { get; set; }
        public bool Activo { get; set; }
    }

    public class PresupuestoUpsDetalleDto
    {
        public int UPSId { get; set; }
        public string NumeroInventario { get; set; } = string.Empty;
        public string NumeroSerie { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int AgenciaId { get; set; }
        public string Agencia { get; set; } = string.Empty;
        public string TipoAgencia { get; set; } = string.Empty;
        public string EstadoUPS { get; set; } = string.Empty;
        public int CantidadBaterias { get; set; }
        public DateTime? FechaUltimoCambioBaterias { get; set; }
        public DateTime? FechaInstalacion { get; set; }
        public DateTime? FechaProyectadaReemplazo { get; set; }
        public int MesesCicloReemplazo { get; set; }
        public decimal CostoUnitarioBateria { get; set; }
        public decimal CostoEstimado { get; set; }
        public bool InformacionSuficiente { get; set; }
        public string? Observacion { get; set; }

        public bool EstaVencido { get; set; }
        public bool ReemplazoAnteriorPendiente { get; set; }
        public bool TieneMantenimientoProgramado { get; set; }
        public DateTime? FechaMantenimientoProgramado { get; set; }
    }

    public class PresupuestoAgenciaDto
    {
        public int AgenciaId { get; set; }
        public string Agencia { get; set; } = string.Empty;
        public string TipoAgencia { get; set; } = string.Empty;
        public int CantidadUPS { get; set; }
        public int CantidadUPSConReemplazo { get; set; }
        public int CantidadBaterias { get; set; }
        public decimal Subtotal { get; set; }
        public List<PresupuestoUpsDetalleDto> UPS { get; set; }
            = new List<PresupuestoUpsDetalleDto>();
    }

    public class PresupuestoResumenDto
    {
        public int Anio { get; set; }
        public decimal CostoUnitarioBateria { get; set; }
        public int TotalUPSInventario { get; set; }
        public int TotalUPSConReemplazo { get; set; }
        public int TotalBaterias { get; set; }
        public decimal TotalPresupuesto { get; set; }
        public int TotalUPSVencidos { get; set; }
        public int TotalUPSProgramados { get; set; }
        public int UPSInformacionInsuficiente { get; set; }
        public List<PresupuestoAgenciaDto> Agencias { get; set; }
            = new List<PresupuestoAgenciaDto>();
        public List<PresupuestoUpsDetalleDto> UPSInformacionInsuficienteDetalle { get; set; }
            = new List<PresupuestoUpsDetalleDto>();
    }
}
