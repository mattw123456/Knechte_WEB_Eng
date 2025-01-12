namespace WebEngineering.Room
{
    public interface IRoomService
    {
        public Task<List<Room>> GetAllRoomsAsync(Guid? storey_id = null, bool includeDeleted = false);
        public Task<Room> CreateRoomAsync(Room room);
        public Task<Room> GetRoomByIdAsync(Guid id);
        public Task<Room> UpdateRoomAsync(Room room, Guid id);
        public Task<bool> DeleteRoomAsync(Guid id, bool permanent = false);
    }
}
