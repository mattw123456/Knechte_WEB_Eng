
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using WebEngineering.Buildings;

namespace WebEngineering.Storey
{
    public class StoreyService : IStoreyService
    {
        private readonly AppDbContext _dbContext;

        public StoreyService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Storey>> GetAllStoreysAsync(Guid? building_id = null, bool includeDeleted = false)
        {
            var query = _dbContext.storeys.AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(b => b.deleted_at == null); // Filtert gelöschte Gebäude aus
            }
            if (building_id.HasValue && building_id != Guid.Empty)
            {
                query = query.Where(b => b.building_id == building_id);
            }

            return await query.ToListAsync();
        }

        public async Task<Storey> CreateStoreyAsync(Storey storey)
        {
            _dbContext.storeys.Add(storey);
            await _dbContext.SaveChangesAsync();
            return storey;
        }

        public async Task<Storey> GetStoreyByIdAsync(Guid id)
        {

            // Gebäude anhand der ID suchen
            var storey = await _dbContext.storeys.FindAsync(id);


            // Gebäude zurückgeben (oder null, wenn nicht gefunden)
            return storey;

        }

        public async Task<Storey> UpdateStoreyAsync(Storey storey, Guid id)
        {
            var existingStorey = await _dbContext.storeys.FindAsync(id);
            
            if (existingStorey == null)
                return await CreateStoreyAsync(storey);


            existingStorey.name = storey.name;
            existingStorey.deleted_at = storey.deleted_at;
            existingStorey.building_id = storey.building_id;

            await _dbContext.SaveChangesAsync();

            return existingStorey;
            

        }

        public async Task<bool> DeleteStoreyAsync(Guid id, bool permanent = false)
        {
            var storey = await GetStoreyByIdAsync(id);

            if (storey == null)
                return false; // Storey nicht gefunden

            var rooms = await _dbContext.rooms.Where(s => s.storey_id == id).ToListAsync();

            if (rooms.Any())
                return false; // Wenn das Storey noch aktive Rooms hat

            if (permanent)
            {
                _dbContext.storeys.Remove(storey);
            }
            else
            {
                storey.deleted_at = DateTime.UtcNow;
            }

            try
            {
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }




    }
}
