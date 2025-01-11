namespace WebEngineering.Buildings
{
    public interface IBuildingService
    {
        public Task<List<Building>> GetAllBuildingsAsync(bool includeDeleted = false);
        public Task<Building> CreateBuildingAsync(Building building);
        public Task<Building> GetBuildingByIdAsync(Guid id);
        public Task<(bool Success, string ErrorMessage)> DeleteBuildingAsync(Guid id, bool permanent = false);
        public Task<(Building building, bool isCreated)> UpdateBuildingAsync(Building building, Guid id);

    }
}
