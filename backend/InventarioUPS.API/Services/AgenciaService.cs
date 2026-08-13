using InventarioUPS.API.Models;
using InventarioUPS.API.Repositories;

namespace InventarioUPS.API.Services
{
    public class AgenciaService : IAgenciaService
    {
        private readonly IAgenciaRepository _repository;

        public AgenciaService(IAgenciaRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Agencia>> ObtenerTodasAsync()
        {
            return await _repository.ObtenerTodasAsync();
        }

        public async Task<Agencia?> ObtenerPorIdAsync(int id)
        {
            return await _repository.ObtenerPorIdAsync(id);
        }

        public async Task CrearAsync(Agencia agencia)
        {
            var existente = (await _repository.ObtenerTodasAsync())
                .FirstOrDefault(a => a.Codigo == agencia.Codigo);

            if (existente != null)
                throw new Exception("Ya existe una agencia con ese código.");

            await _repository.CrearAsync(agencia);
        }

        public async Task ActualizarAsync(Agencia agencia)
        {
            await _repository.ActualizarAsync(agencia);
        }

        public async Task EliminarAsync(int id)
        {
            var agencia = await _repository.ObtenerPorIdAsync(id);

            if (agencia == null)
                throw new Exception("La agencia no existe.");

            await _repository.EliminarAsync(agencia);
        }
    }
}