using Microsoft.EntityFrameworkCore;

namespace WebEngineering.Buildings
{
    public class BuildingService : IBuildingService
    {
        private readonly AppDbContext _dbContext;

        public BuildingService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Building>> GetAllBuildingsAsync(bool includeDeleted = false)
        {
            var query = _dbContext.buildings.AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(b => b.deleted_at == null); // Filtert gelöschte Gebäude aus
            }

            return await query.ToListAsync();
        }
        public async Task<Building> CreateBuildingAsync(Building building)
        {
            if (building.deleted_at == null)
            {
                building.deleted_at = null;
            }
            _dbContext.buildings.Add(building);
            await _dbContext.SaveChangesAsync();
            return building;
        }

        public async Task<Building> GetBuildingByIdAsync(Guid id)
        {
            // Gebäude anhand der ID suchen
            var building = await _dbContext.buildings.FindAsync(id);


            // Gebäude zurückgeben (oder null, wenn nicht gefunden)
            return building;
        }

        public async Task<(bool Success, string ErrorMessage)> DeleteBuildingAsync(Guid id, bool permanent = false)
        {
            var building = await GetBuildingByIdAsync(id);

            if (building == null)
                return (false, "Building not found");

            var activeStoreys = await _dbContext.storeys
                .Where(s => s.building_id == id)
                .ToListAsync();

            if (activeStoreys.Any())
                return (false, "Building has active storeys and cannot be deleted");

            if (permanent)
            {
                _dbContext.buildings.Remove(building);
            }
            else
            {
                building.deleted_at = DateTime.UtcNow;
            }

            try
            {
                await _dbContext.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                // Logging hinzufügen
                Console.Error.WriteLine($"Error deleting building: {ex.Message}");
                return (false, "An error occurred while deleting the building");
            }
        }


        public async Task<(Building building, bool isCreated)> UpdateBuildingAsync(Building building, Guid id)
        {
            var existingBuilding = await _dbContext.buildings.FindAsync(id);

            if (existingBuilding == null)
            {
                var createdBuilding = await CreateBuildingAsync(building);
                return (createdBuilding, true); // Tupel (Gebäude, true) wird zurückgegeben
            }

            existingBuilding.city = building.city;
            existingBuilding.housenumber = building.housenumber;
            existingBuilding.country_code = building.country_code;
            existingBuilding.deleted_at = building.deleted_at;
            existingBuilding.postalcode = building.postalcode;
            existingBuilding.streetname = building.streetname;
            existingBuilding.name = building.name;

            await _dbContext.SaveChangesAsync();

            return (existingBuilding, false); // Tupel (Gebäude, false) wird zurückgegeben
        }



    }
}
