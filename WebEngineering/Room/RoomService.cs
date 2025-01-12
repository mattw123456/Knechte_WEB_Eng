using Microsoft.EntityFrameworkCore;
using WebEngineering.Storey;

namespace WebEngineering.Room
{
    public class RoomService : IRoomService
    {
        private readonly AppDbContext _dbContext;

        public RoomService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Room> CreateRoomAsync(Room room)
        {
            _dbContext.rooms.Add(room);
            await _dbContext.SaveChangesAsync();
            return room;
        }

        public async Task<bool> DeleteRoomAsync(Guid id, bool permanent = false)
        {
            var room = await GetRoomByIdAsync(id);

            if (room == null)
                return false; // Room nicht gefunden

            if (permanent)
            {
                _dbContext.rooms.Remove(room);
            }
            else
            {
                room.deleted_at = DateTime.UtcNow;
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

        public async Task<List<Room>> GetAllRoomsAsync(Guid? storey_id = null, bool includeDeleted = false)
        {
            var query = _dbContext.rooms.AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(b => b.deleted_at == null); // Filtert gelöschte Rooms aus
            }

            if (storey_id.HasValue && storey_id != Guid.Empty)
            {
                query = query.Where(b => b.storey_id == storey_id);
            }

            return await query.ToListAsync();
        }


        public async Task<Room> GetRoomByIdAsync(Guid id)
        {

            // Room anhand der ID suchen
            var room = await _dbContext.rooms.FindAsync(id);


            // Gebäude zurückgeben (oder null, wenn nicht gefunden)
            return room;

        }

        public async Task<Room> UpdateRoomAsync(Room room, Guid id)
        {
            var existingRoom = await _dbContext.rooms.FindAsync(id);

            if (existingRoom == null)
                return await CreateRoomAsync(room);


            existingRoom.name = room.name;
            existingRoom.deleted_at = room.deleted_at;
            existingRoom.storey_id = room.storey_id;

            await _dbContext.SaveChangesAsync();

            return existingRoom;


        }
    }
}
