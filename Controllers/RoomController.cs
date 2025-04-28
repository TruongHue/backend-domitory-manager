using API_dormitory.Models.Rooms;
using API_dormitory.Models.DTO.Room;
using API_dormitory.Models.DTO.Building;
using API_dormitory.Data;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using API_dormitory.Models.common;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using API_dormitory.Models.Bills;
using API_dormitory.Models.registerRoom;
using Microsoft.AspNetCore.Authorization;

namespace API_dormitory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly IMongoCollection<RoomBillModels> _roomBillCollection;
        private readonly IMongoCollection<PriceWaterAndElectricity> _priceCollection;
        private readonly IMongoCollection<InfoRoomModels> _roomsCollection;
        private readonly IMongoCollection<BuildingModels> _buildingsCollection;
        private readonly IMongoCollection<RegisterRoomModels> _registerRoomCollection;

        public RoomController(MongoDbContext database)
        {
            _roomBillCollection = database.GetCollection<RoomBillModels>("RoomBills");
            _roomsCollection = database.GetCollection<InfoRoomModels>("Rooms");
            _buildingsCollection = database.GetCollection<BuildingModels>("Buildings");
            _registerRoomCollection = database.GetCollection<RegisterRoomModels>("RegisterRoom");

        }

        [Authorize(Roles = "Admin,Student,Staff")]
        [HttpGet]
        public async Task<IActionResult> GetAllRooms()
        {
            var rooms = await _roomsCollection.Find(_ => true).ToListAsync();

            // Lấy danh sách IdBuilding từ các phòng
            var buildingIds = rooms.Select(r => r.IdBuilding).Distinct().ToList();
            var buildingFilter = Builders<BuildingModels>.Filter.In("_id", buildingIds);
            var buildings = await _buildingsCollection.Find(buildingFilter).ToListAsync();

            // Tạo dictionary để tra cứu nhanh thông tin tòa nhà
            var buildingDict = buildings.ToDictionary(b => b.Id.ToString(), b => b);

            // Lấy danh sách IdRoom từ các phòng
            var roomIds = rooms.Select(r => r.IdRoom).ToList();

            // Lọc hóa đơn có trạng thái Active (hoạt động)
            var billFilter = Builders<RoomBillModels>.Filter.In("IdRoom", roomIds) &
                             Builders<RoomBillModels>.Filter.Eq("Status", OperatingStatusEnum.active);
            var roomBills = await _roomBillCollection.Find(billFilter).ToListAsync();

            // Gom nhóm hóa đơn theo từng phòng
            var roomBillDict = roomBills
                .GroupBy(rb => rb.IdRoom)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(rb => rb.DateOfRecord)
                          .Select(rb => new
                          {
                              IdRoomBill = rb.IdRoomBill.ToString(),
                              PriceYear = rb.PriceYear,
                              DailyPrice = rb.DailyPrice,
                              DateOfRecord = rb.DateOfRecord,
                              Status = rb.Status
                          }).FirstOrDefault()
                );

            // Lọc số lượng người đăng ký theo trạng thái `active` hoặc `wait`
            var registerFilter = Builders<RegisterRoomModels>.Filter.In("IdRoom", roomIds) &
                                 Builders<RegisterRoomModels>.Filter.In("Status", new[] { OperatingStatusEnum.active, OperatingStatusEnum.wait });
            var registerRooms = await _registerRoomCollection.Find(registerFilter).ToListAsync();

            // Nhóm số lượng đăng ký theo phòng
            var registerRoomDict = registerRooms
                .GroupBy(r => r.IdRoom)
                .ToDictionary(g => g.Key, g => g.Count());

            // Trả về danh sách phòng kèm hóa đơn active và số lượng người đăng ký
            return Ok(rooms.Select(b => new
            {
                Id = b.IdRoom.ToString(),
                b.RoomName,
                b.Gender,
                b.NumberOfBed,
                b.Status,

                // Thông tin tòa nhà
                Building = buildingDict.TryGetValue(b.IdBuilding.ToString(), out var building) ? new
                {
                    Id = building.Id.ToString(),
                    building.NameBuilding,
                    building.Description,
                    building.Status
                } : null,

                // Danh sách hóa đơn active (nếu có)
                RoomBills = roomBillDict.TryGetValue(b.IdRoom, out var bill)
                    ? new List<dynamic> { bill }
                    : new List<dynamic>(),

                // Số lượng người đăng ký có trạng thái active hoặc wait
                NumberOfRegistrations = registerRoomDict.TryGetValue(b.IdRoom, out var count) ? count : 0
            }));
        }

        [Authorize(Roles = "Admin,Student,Staff")]
        [HttpGet("by-id/{id}")]
        public async Task<IActionResult> GetRoomById(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return BadRequest(new { message = "ID không hợp lệ" });

            var room = await _roomsCollection.Find(r => r.IdRoom == objectId).FirstOrDefaultAsync();
            if (room == null)
                return NotFound(new { message = "Không tìm thấy phòng" });

            // Lấy thông tin tòa nhà tương ứng
            var building = await _buildingsCollection.Find(b => b.IdBuilding == room.IdBuilding).FirstOrDefaultAsync();

            // Đếm số lượng đăng ký có trạng thái active hoặc wait
            var activeCount = await _registerRoomCollection.CountDocumentsAsync(r => r.IdRoom == objectId && r.Status == OperatingStatusEnum.active);
            var waitCount = await _registerRoomCollection.CountDocumentsAsync(r => r.IdRoom == objectId && r.Status == OperatingStatusEnum.wait);

            return Ok(new
            {
                Id = room.IdRoom.ToString(),
                room.RoomName,
                room.Gender,
                room.NumberOfBed,
                room.Status,

                // Thêm số lượng đăng ký active và wait
                RegisterCounts = new
                {
                    Active = activeCount,
                    Wait = waitCount,
                    Total = activeCount + waitCount
                },

                Building = building != null ? new
                {
                    Id = building.Id.ToString(),
                    building.NameBuilding,
                    building.Description,
                    building.Status
                } : null
            });
        }

        // 🔹 Thêm phòng mới
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromBody] AddRoomDTO newRoom)
        {
            if (string.IsNullOrEmpty(newRoom.RoomName))
                return BadRequest(new { message = "Tên phòng không được để trống" });

            if (newRoom.NumberOfBed == null || newRoom.NumberOfBed < 1)
                return BadRequest(new { message = "Số giường phải lớn hơn 0" });
            var objectId = ObjectId.Parse(newRoom.IdBuilding);
            var building = await _buildingsCollection.Find(b => b.IdBuilding == objectId).FirstOrDefaultAsync();
            if (building == null)
                return BadRequest(new { message = "Tòa nhà không tồn tại" });

            var room = new InfoRoomModels
            {
                IdRoom = ObjectId.GenerateNewId(),
                IdBuilding = objectId,
                Gender = newRoom.Gender,
                RoomName = newRoom.RoomName,
                NumberOfBed = newRoom.NumberOfBed.Value,
                Status = newRoom.Status ?? OperatingStatusEnum.active
            };

            await _roomsCollection.InsertOneAsync(room);

            var roomBill = new RoomBillModels
            {
                IdRoomBill = ObjectId.GenerateNewId(),
                IdRoom = room.IdRoom, // Lấy ID phòng vừa tạo
                DailyPrice = newRoom.DailyPrice, // Nếu không có thì mặc định 0
                PriceYear = newRoom.PriceYear,   // Nếu không có thì mặc định 0
                DateOfRecord = DateTime.UtcNow,       // Ngày tạo hóa đơn
                Status = OperatingStatusEnum.active   // Hóa đơn mới luôn Active
            };

            await _roomBillCollection.InsertOneAsync(roomBill);

            return Ok(new { message = "Tạo phòng thành công!" });
        }

        // 🔹 Cập nhật thông tin phòng
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoom(string id, [FromBody] InfoRoomDTOs updateRoom)
        {
            var objectId = ObjectId.Parse(id);
            var room = await _roomsCollection.Find(r => r.IdRoom == objectId).FirstOrDefaultAsync();
            if (room == null)
                return NotFound(new { message = "Không tìm thấy phòng" });

            var updateDefinition = Builders<InfoRoomModels>.Update
                .Set(r => r.RoomName, updateRoom.RoomName ?? room.RoomName)
                .Set(r => r.Gender, updateRoom.Gender)
                .Set(r => r.NumberOfBed, updateRoom.NumberOfBed ?? room.NumberOfBed)
                .Set(r => r.Status, updateRoom.Status ?? room.Status);

            if (!string.IsNullOrEmpty(updateRoom.IdBuilding))
            {
                var objectId1 = ObjectId.Parse(updateRoom.IdBuilding);
                var buildingExists = await _buildingsCollection.Find(b => b.IdBuilding == objectId1).AnyAsync();
                if (!buildingExists)
                    return BadRequest(new { message = "Tòa nhà không tồn tại" });

                updateDefinition = updateDefinition.Set(r => r.IdBuilding, objectId1);
            }
            var objectId2 = ObjectId.Parse(id);
            await _roomsCollection.UpdateOneAsync(r => r.IdRoom == objectId2, updateDefinition);
            return Ok(new { message = "Cập nhật phòng thành công" });
        }

        // 🔹 Cập nhật trạng thái phòng
        [Authorize(Roles = "Admin")]
        [HttpPut("status")]
        public async Task<IActionResult> UpdateRoomStatus([FromBody] UpdateStatusRoomDTO status)
        {
            var objectId = ObjectId.Parse(status.IdRoom);
            var room = await _roomsCollection.Find(r => r.IdRoom == objectId).FirstOrDefaultAsync();
            if (room == null)
                return NotFound(new { message = "Không tìm thấy phòng" });

            var building = await _buildingsCollection.Find(b => b.IdBuilding == room.IdBuilding).FirstOrDefaultAsync();
            if (building == null)
                return NotFound(new { message = "Không tìm thấy tòa nhà chứa phòng này" });

            if (building.Status == OperatingStatusEnum.inactive)
                return BadRequest(new { message = "Tòa nhà đang Inactive, không thể cập nhật trạng thái phòng." });

            var update = Builders<InfoRoomModels>.Update.Set(r => r.Status, (OperatingStatusEnum)status.Status);

            await _roomsCollection.UpdateOneAsync(r => r.IdRoom == objectId, update);

            return Ok(new { message = "Cập nhật trạng thái phòng thành công" });
        }

        // 🔹 Xóa phòng
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(string id)
        {
            var objectId = ObjectId.Parse(id);
            var result = await _roomsCollection.DeleteOneAsync(r => r.IdRoom == objectId);
            if (result.DeletedCount == 0)
                return NotFound(new { message = "Không tìm thấy phòng" });

            return Ok(new { message = "Xóa phòng thành công" });
        }
    }
}
