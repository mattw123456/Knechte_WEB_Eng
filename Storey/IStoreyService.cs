using WebEngineering.Buildings;

namespace WebEngineering.Storey
{
    public interface IStoreyService
    {
        public Task<List<Storey>> GetAllStoreysAsync(Guid? building_id = null, bool includeDeleted = false);
        public Task<Storey> CreateStoreyAsync(Storey storey);
        public Task<Storey> GetStoreyByIdAsync(Guid id);
        public Task<Storey> UpdateStoreyAsync(Storey storey, Guid id);
        public Task<bool> DeleteStoreyAsync(Guid id, bool permanent = false);
    }
}
