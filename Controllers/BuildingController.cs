using API_dormitory.Data;
using API_dormitory.Models.common;
using API_dormitory.Models.DTO.Building;
using API_dormitory.Models.Rooms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_dormitory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuildingController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BuildingController(AppDbContext context)
        {
            _context = context;
        }

        // 🔹 Lấy toàn bộ danh sách tòa nhà
        [HttpGet]
        public async Task<IActionResult> GetAllBuildings()
        {
            var buildings = await _context.Buildings
                .Select(b => new BuildingDTOs
                {
                    IdBuilding = b.IdBuilding,
                    NameBuilding = b.NameBuilding,
                    Description = b.Description,
                    Status = b.Status
                })
                .ToListAsync();

            return Ok(buildings);
        }

        // 🔹 Lấy thông tin tòa nhà theo ID
        [HttpGet("by-id/{id}")]
        public async Task<IActionResult> GetBuildingById(int id)
        {
            var building = await _context.Buildings
                .Where(b => b.IdBuilding == id)
                .Select(b => new BuildingDTOs
                {
                    IdBuilding = b.IdBuilding,
                    NameBuilding = b.NameBuilding,
                    Description = b.Description,
                    Status = b.Status
                })
                .FirstOrDefaultAsync();

            if (building == null)
                return NotFound(new { message = "Không tìm thấy tòa nhà" });

            return Ok(building);
        }

        // 🔹 Lấy thông tin tòa nhà theo tên
        [HttpGet("by-name/{name}")]
        public async Task<IActionResult> GetBuildingByName(string name)
        {
            var buildings = await _context.Buildings
                .Where(b => b.NameBuilding.Contains(name) )
                .Select(b => new BuildingDTOs
                {
                    IdBuilding = b.IdBuilding,
                    NameBuilding = b.NameBuilding,
                    Description = b.Description,
                    Status = b.Status
                })
                .ToListAsync();

            if (buildings.Count == 0)
                return NotFound(new { message = "Không tìm thấy tòa nhà với tên này" });

            return Ok(buildings);
        }

        // 🔹 Thêm mới một tòa nhà
            [HttpPost]
            public async Task<IActionResult> CreateBuilding([FromBody] BuildingDTOs newBuilding)
            {
                if (string.IsNullOrEmpty(newBuilding.NameBuilding))
                    return BadRequest(new { message = "Tên tòa nhà không được để trống" });

                var building = new BuildingModels
                {
                    NameBuilding = newBuilding.NameBuilding,
                    Description = newBuilding.Description ?? "Chưa có mô tả", // Đặt giá trị mặc định nếu NULL
                    Status = newBuilding.Status
                };

                _context.Buildings.Add(building);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetBuildingById), new { id = building.IdBuilding }, building);
            }

        [HttpPut("status")]
public async Task<IActionResult> UpdateBuildingStatus([FromBody] UpdateStatusBuildingDTO status)
{
    try
    {
        var building = await _context.Buildings.FindAsync(status.IdBuilding);
        if (building == null)
            return NotFound(new { message = "Không tìm thấy tòa nhà" });

        // Kiểm tra nếu giá trị không hợp lệ
        if (!Enum.IsDefined(typeof(OperatingStatusEnum), status.Status))
        {
            return BadRequest(new { message = "Trạng thái không hợp lệ" });
        }

        // Lưu trạng thái cũ của tòa nhà
        var oldStatus = building.Status;

        // Cập nhật trạng thái tòa nhà
        building.Status = (OperatingStatusEnum)status.Status;

        // Nếu tòa nhà trước đó là Active, mới cập nhật trạng thái các phòng
        if (oldStatus == OperatingStatusEnum.active)
        {
            var rooms = await _context.InfoRoom
                .Where(r => r.IdBuilding == status.IdBuilding)
                .ToListAsync();

            foreach (var room in rooms)
            {
                room.Status = (OperatingStatusEnum)status.Status; // Cập nhật trạng thái phòng giống tòa nhà
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "Cập nhật trạng thái tòa nhà thành công" });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { message = "Lỗi Server", error = ex.Message });
    }
}





        // 🔹 Cập nhật thông tin tòa nhà
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBuilding(int id, [FromBody] BuildingDTOs updateBuilding)
        {
            var building = await _context.Buildings.FindAsync(id);
            if (building == null)
                return NotFound(new { message = "Không tìm thấy tòa nhà" });

            // Chỉ cập nhật nếu NameBuilding không rỗng
            if (!string.IsNullOrEmpty(updateBuilding.NameBuilding))
                building.NameBuilding = updateBuilding.NameBuilding;

            if (!string.IsNullOrEmpty(updateBuilding.Description))
                building.Description = updateBuilding.Description;

            // Kiểm tra nếu updateBuilding.Status khác null thì mới cập nhật
            if (updateBuilding.Status != null)
                building.Status = updateBuilding.Status;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thông tin tòa nhà thành công" });
        }

        

        // 🔹 Xóa tòa nhà
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBuilding(int id)
        {
            var building = await _context.Buildings.FindAsync(id);
            if (building == null)
                return NotFound(new { message = "Không tìm thấy tòa nhà" });
            _context.InfoRoom.RemoveRange(building.Rooms); // Xóa tất cả phòng trước
            _context.Buildings.Remove(building);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa tòa nhà thành công" });
        }
    }
}
