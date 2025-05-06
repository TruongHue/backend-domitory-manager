using API_dormitory.Data;
using API_dormitory.Models.registerRoom;
using MongoDB.Driver;

namespace API_dormitory.Services
{
    public class RegisterRoomService
    {
        private readonly IMongoCollection<RegisterRoomModels> _registerRooms;

        public RegisterRoomService(MongoDbContext context)
        {
            _registerRooms = context.GetCollection<RegisterRoomModels>("RegisterRooms");
        }

        public Task<List<RegisterRoomModels>> GetAllAsync() =>
            _registerRooms.Find(_ => true).ToListAsync();
    }

}
