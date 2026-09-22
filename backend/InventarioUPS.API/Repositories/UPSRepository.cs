using InventarioUPS.API.Data;
using InventarioUPS.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace InventarioUPS.API.Repositories
{
    public class UPSRepository : IUPSRepository
    {
        private const string ComparacionIdentificadores = "Latin1_General_100_CI_AS";
        private readonly InventarioUPSDbContext _context;

        public UPSRepository(InventarioUPSDbContext context)
        {
            _context = context;
        }

        public async Task<List<UPS>> ObtenerTodasAsync()
        {
            return await _context.UPSs
                .AsNoTracking()
                .Include(u => u.Agencia)
                    .ThenInclude(a => a!.Ciudad)
                        .ThenInclude(c => c!.Departamento)
                .Include(u => u.Agencia)
                    .ThenInclude(a => a!.TipoAgencia)
                .Include(u => u.EstadoUPS)
                .OrderBy(u => u.Codigo)
                .ToListAsync();
        }

        public async Task<UPS?> ObtenerPorIdAsync(int id)
        {
            return await _context.UPSs
                .AsNoTracking()
                .Include(u => u.Agencia)
                    .ThenInclude(a => a!.Ciudad)
                        .ThenInclude(c => c!.Departamento)
                .Include(u => u.Agencia)
                    .ThenInclude(a => a!.TipoAgencia)
                .Include(u => u.EstadoUPS)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task CrearAsync(UPS ups)
        {
            await _context.UPSs.AddAsync(ups);
            await GuardarCambiosAsync();
        }

        public async Task ActualizarAsync(UPS ups)
        {
            var existente = await _context.UPSs.FirstOrDefaultAsync(u => u.Id == ups.Id);

            if (existente == null)
                throw new InvalidOperationException("El UPS no existe.");

            existente.Codigo = ups.Codigo;
            existente.NumeroInventario = ups.NumeroInventario;
            existente.Marca = ups.Marca;
            existente.Modelo = ups.Modelo;
            existente.NumeroSerie = ups.NumeroSerie;
            existente.CapacidadVA = ups.CapacidadVA;
            existente.AgenciaId = ups.AgenciaId;
            existente.EstadoUPSId = ups.EstadoUPSId;
            existente.FechaInstalacion = ups.FechaInstalacion;
            existente.CantidadBaterias = ups.CantidadBaterias;
            existente.FechaUltimoCambioBaterias = ups.FechaUltimoCambioBaterias;
            existente.FechaUltimoCambioBateriasBase = ups.FechaUltimoCambioBateriasBase;
            existente.FechaModificacion = DateTime.Now;

            await GuardarCambiosAsync();
        }

        public async Task CambiarEstadoAsync(int id, string estado)
        {
            var ups = await _context.UPSs.FirstOrDefaultAsync(u => u.Id == id);

            if (ups == null)
                throw new InvalidOperationException("El UPS no existe.");

            var estadoUPS = await _context.EstadosUPS.FirstOrDefaultAsync(e =>
                e.Activo && e.Nombre.ToUpper() == estado.Trim().ToUpper());

            if (estadoUPS == null)
                throw new ArgumentException("El estado de UPS indicado no existe o está inactivo.");

            ups.EstadoUPSId = estadoUPS.Id;
            ups.FechaModificacion = DateTime.Now;
            await GuardarCambiosAsync();
        }

        private async Task GuardarCambiosAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (
                ex.InnerException is SqlException { Number: 2601 or 2627 })
            {
                throw new InvalidOperationException(
                    "No se pudo guardar: el código, el número de inventario o el número de serie ya pertenece a otro UPS.",
                    ex);
            }
        }

        public Task<bool> ExisteCodigoAsync(string codigo)
        {
            var valor = codigo.Trim();
            return _context.UPSs.AnyAsync(u =>
                EF.Functions.Collate(u.Codigo.Trim(), ComparacionIdentificadores) == valor);
        }

        public Task<bool> ExisteCodigoExceptoIdAsync(string codigo, int upsId)
        {
            var valor = codigo.Trim();
            return _context.UPSs.AnyAsync(u =>
                u.Id != upsId &&
                EF.Functions.Collate(u.Codigo.Trim(), ComparacionIdentificadores) == valor);
        }

        public Task<bool> ExisteNumeroInventarioAsync(string numeroInventario)
        {
            var valor = numeroInventario.Trim();
            return _context.UPSs.AnyAsync(u =>
                EF.Functions.Collate(u.NumeroInventario.Trim(), ComparacionIdentificadores) == valor);
        }

        public Task<bool> ExisteNumeroInventarioExceptoIdAsync(
            string numeroInventario,
            int upsId)
        {
            var valor = numeroInventario.Trim();
            return _context.UPSs.AnyAsync(u =>
                u.Id != upsId &&
                EF.Functions.Collate(u.NumeroInventario.Trim(), ComparacionIdentificadores) == valor);
        }

        public Task<bool> ExisteNumeroSerieAsync(string numeroSerie)
        {
            var valor = numeroSerie.Trim();
            return _context.UPSs.AnyAsync(u =>
                EF.Functions.Collate(u.NumeroSerie.Trim(), ComparacionIdentificadores) == valor);
        }

        public Task<bool> ExisteNumeroSerieExceptoIdAsync(string numeroSerie, int upsId)
        {
            var valor = numeroSerie.Trim();
            return _context.UPSs.AnyAsync(u =>
                u.Id != upsId &&
                EF.Functions.Collate(u.NumeroSerie.Trim(), ComparacionIdentificadores) == valor);
        }

        public Task<bool> ExisteAgenciaActivaAsync(int agenciaId) =>
            _context.Agencias.AnyAsync(a => a.Id == agenciaId && a.Activa);

        public Task<bool> ExisteEstadoUPSActivoAsync(int estadoUPSId) =>
            _context.EstadosUPS.AnyAsync(e => e.Id == estadoUPSId && e.Activo);
    }
}
