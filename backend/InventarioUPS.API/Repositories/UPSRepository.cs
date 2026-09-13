using InventarioUPS.API.Data;
using InventarioUPS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace InventarioUPS.API.Repositories
{
    public class UPSRepository : IUPSRepository
    {
        private readonly InventarioUPSDbContext _context;

        public UPSRepository(InventarioUPSDbContext context)
        {
            _context = context;
        }

        public async Task<List<UPS>> ObtenerTodasAsync()
        {
            return await _context.UPSs
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
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(UPS ups)
        {
            var existente = await _context.UPSs
                .FirstOrDefaultAsync(u => u.Id == ups.Id);

            if (existente == null)
            {
                throw new InvalidOperationException(
                    "El UPS no existe.");
            }

            existente.Codigo = ups.Codigo;
            existente.Marca = ups.Marca;
            existente.Modelo = ups.Modelo;
            existente.NumeroSerie = ups.NumeroSerie;
            existente.CapacidadVA = ups.CapacidadVA;
            existente.AgenciaId = ups.AgenciaId;
            existente.EstadoUPSId = ups.EstadoUPSId;
            existente.FechaInstalacion = ups.FechaInstalacion;
            existente.CantidadBaterias = ups.CantidadBaterias;
            existente.FechaUltimoCambioBaterias =
                ups.FechaUltimoCambioBaterias;
            existente.FechaModificacion = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task CambiarEstadoAsync(
            int id,
            string estado)
        {
            if (string.IsNullOrWhiteSpace(estado))
            {
                throw new ArgumentException(
                    "El estado es obligatorio.");
            }

            var ups = await _context.UPSs
                .FirstOrDefaultAsync(u => u.Id == id);

            if (ups == null)
            {
                throw new InvalidOperationException(
                    "El UPS no existe.");
            }

            var estadoUPS = await _context.EstadosUPS
                .FirstOrDefaultAsync(e =>
                    e.Nombre.ToUpper() ==
                    estado.Trim().ToUpper());

            if (estadoUPS == null)
            {
                throw new ArgumentException(
                    "El estado de UPS indicado no existe.");
            }

            if (!estadoUPS.Activo)
            {
                throw new InvalidOperationException(
                    "El estado de UPS indicado está inactivo.");
            }

            ups.EstadoUPSId = estadoUPS.Id;
            ups.FechaModificacion = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExisteCodigoAsync(string codigo)
        {
            var valor = codigo.Trim().ToUpper();

            return await _context.UPSs
                .AnyAsync(u =>
                    u.Codigo.ToUpper() == valor);
        }

        public async Task<bool> ExisteCodigoExceptoIdAsync(
            string codigo,
            int upsId)
        {
            var valor = codigo.Trim().ToUpper();

            return await _context.UPSs
                .AnyAsync(u =>
                    u.Id != upsId &&
                    u.Codigo.ToUpper() == valor);
        }

        public async Task<bool> ExisteNumeroSerieAsync(
            string numeroSerie)
        {
            var valor = numeroSerie.Trim().ToUpper();

            return await _context.UPSs
                .AnyAsync(u =>
                    u.NumeroSerie.ToUpper() == valor);
        }

        public async Task<bool> ExisteNumeroSerieExceptoIdAsync(
            string numeroSerie,
            int upsId)
        {
            var valor = numeroSerie.Trim().ToUpper();

            return await _context.UPSs
                .AnyAsync(u =>
                    u.Id != upsId &&
                    u.NumeroSerie.ToUpper() == valor);
        }

        public async Task<bool> ExisteAgenciaActivaAsync(
            int agenciaId)
        {
            return await _context.Agencias
                .AnyAsync(a =>
                    a.Id == agenciaId &&
                    a.Activa);
        }

        public async Task<bool> ExisteEstadoUPSActivoAsync(
            int estadoUPSId)
        {
            return await _context.EstadosUPS
                .AnyAsync(e =>
                    e.Id == estadoUPSId &&
                    e.Activo);
        }
    }
}