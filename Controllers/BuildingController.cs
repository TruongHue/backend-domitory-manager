using API_dormitory.Data;
using API_dormitory.Models.common;
using API_dormitory.Models.DTO.Building;
using API_dormitory.Models.Rooms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API_dormitory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuildingController : ControllerBase
    {
        private readonly IMongoCollection<BuildingModels> _buildingCollection;

        public BuildingController(MongoDbContext database)
        {
            _buildingCollection = database.GetCollection<BuildingModels>("Buildings");
        }

        // 🔹 Lấy danh sách tòa nhà
        [Authorize(Roles = "Admin,Student,Staff")]
        [HttpGet]
        public async Task<IActionResult> GetAllBuildings()
        {
            var buildings = await _buildingCollection.Find(_ => true).ToListAsync();

            return Ok(buildings.Select(b => new
            {
                Id = b.Id, // Trả về Id dưới dạng string
                b.NameBuilding,
                b.Description,
                b.Status
            }));
        }

        // 🔹 Lấy tòa nhà theo ID
        [Authorize(Roles = "Admin,Student,Staff")]
        [HttpGet("by-id/{id}")] 
        public async Task<IActionResult> GetBuildingById(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId objectId))
                return BadRequest(new { message = "ID không hợp lệ" });

            var filter = Builders<BuildingModels>.Filter.Eq("_id", objectId);
            var building = await _buildingCollection.Find(filter).FirstOrDefaultAsync();

            if (building == null)
                return NotFound(new { message = "Không tìm thấy tòa nhà" });

            return Ok(new
            {
                Id = building.Id, // Trả về Id dưới dạng string
                building.NameBuilding,
                building.Description,
                building.Status
            });
        }

        // 🔹 Thêm tòa nhà mới
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateBuilding([FromBody] BuildingDTOs newBuilding)
        {
            if (string.IsNullOrEmpty(newBuilding.NameBuilding))
                return BadRequest(new { message = "Tên tòa nhà không được để trống" });

            var building = new BuildingModels
            {
                IdBuilding = ObjectId.GenerateNewId(), // Giữ ObjectId trong DB
                NameBuilding = newBuilding.NameBuilding,
                Description = newBuilding.Description ?? "Chưa có mô tả",
                Status = newBuilding.Status
            };

            await _buildingCollection.InsertOneAsync(building);
            return CreatedAtAction(nameof(GetBuildingById), new { id = building.Id }, building);
        }


        // 🔹 Cập nhật trạng thái tòa nhà
        [Authorize(Roles = "Admin")]
        [HttpPut("status")]
        public async Task<IActionResult> UpdateBuildingStatus([FromBody] UpdateStatusBuildingDTO status)
        {
            if (!ObjectId.TryParse(status.IdBuilding, out var objectId))
                return BadRequest(new { message = "ID không hợp lệ" });

            var update = Builders<BuildingModels>.Update.Set(b => b.Status, status.Status);
            var result = await _buildingCollection.UpdateOneAsync(b => b.IdBuilding == objectId, update);

            if (result.MatchedCount == 0)
                return NotFound(new { message = "Không tìm thấy tòa nhà" });

            return Ok(new { message = "Cập nhật trạng thái thành công" });
        }

        // 🔹 Cập nhật thông tin tòa nhà
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBuilding(string id, [FromBody] BuildingDTOs updateBuilding)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return BadRequest(new { message = "ID không hợp lệ" });

            var update = Builders<BuildingModels>.Update
                .Set(b => b.NameBuilding, updateBuilding.NameBuilding)
                .Set(b => b.Description, updateBuilding.Description)
                .Set(b => b.Status, updateBuilding.Status);

            var result = await _buildingCollection.UpdateOneAsync(b => b.IdBuilding == objectId, update);

            if (result.MatchedCount == 0)
                return NotFound(new { message = "Không tìm thấy tòa nhà" });

            return Ok(new { message = "Cập nhật thông tin thành công" });
        }

        // 🔹 Xóa tòa nhà
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBuilding(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return BadRequest(new { message = "ID không hợp lệ" });

            var result = await _buildingCollection.DeleteOneAsync(b => b.IdBuilding == objectId);

            if (result.DeletedCount == 0)
                return NotFound(new { message = "Không tìm thấy tòa nhà" });

            return Ok(new { message = "Xóa tòa nhà thành công" });
        }
    }
}
