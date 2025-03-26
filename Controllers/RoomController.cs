using API_dormitory.Data;
using API_dormitory.Models.common;
using API_dormitory.Models.DTO.Building;
using API_dormitory.Models.DTO.Room;
using API_dormitory.Models.Rooms;
using API_dormitory.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
namespace API_dormitory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RoomController(AppDbContext context)
        {
            _context = context;
        }

        // 🔹 Lấy toàn bộ danh sách phòng
        [HttpGet]
        public async Task<IActionResult> GetAllRooms()
        {
            var rooms = await _context.InfoRoom
                .Include(x => x.Building)
                .Select(r => new InfoRoomDTOs
                {
                    IdRoom = r.IdRoom,
                    IdBuilding = r.IdBuilding,
                    Gender = r.Gender,
                    RoomName = r.RoomName,
                    NumberOfBed = r.NumberOfBed,
                    Status = r.Status,
                    Building = new BuildingDTOs
                    {
                        IdBuilding = r.Building.IdBuilding,
                        NameBuilding = r.Building.NameBuilding,
                        Status = r.Building.Status,
                        Description = r.Building.Description
                        
                    }
                })
                .ToListAsync();

            return Ok(rooms);
        }

        // 🔹 Lấy thông tin phòng theo ID
        [HttpGet("by-id/{id}")]
        public async Task<IActionResult> GetRoomById(int id)
        {
            var room = await _context.InfoRoom
                .Include(x => x.Building)
                .Where(r => r.IdRoom == id)
                .Select(r => new InfoRoomDTOs
                {
                    IdRoom = r.IdRoom,
                    Gender = r.Gender,
                    IdBuilding = r.IdBuilding,
                    RoomName = r.RoomName,
                    NumberOfBed = r.NumberOfBed,
                    Status = r.Status,
                    Building = new BuildingDTOs
                    {
                        IdBuilding = r.Building.IdBuilding,
                        NameBuilding = r.Building.NameBuilding,
                        Status = r.Building.Status,
                        Description = r.Building.Description

                    }
                })
                .FirstOrDefaultAsync();

            if (room == null)
                return NotFound(new { message = "Không tìm thấy phòng" });

            return Ok(room);
        }

        // 🔹 Lấy danh sách phòng theo ID tòa nhà
        [HttpGet("by-building/{idBuilding}")]
        public async Task<IActionResult> GetRoomsByBuilding(int idBuilding)
        {
            var rooms = await _context.InfoRoom
                .Where(r => r.IdBuilding == idBuilding)
                .Select(r => new InfoRoomDTOs
                {
                    IdRoom = r.IdRoom,
                    IdBuilding = r.IdBuilding,
                    Gender = r.Gender,
                    RoomName = r.RoomName,
                    NumberOfBed = r.NumberOfBed,
                    Status = r.Status
                })
                .ToListAsync();

            if (rooms.Count == 0)
                return NotFound(new { message = "Không có phòng nào trong tòa nhà này" });

            return Ok(rooms);
        }

        // 🔹 Thêm mới một phòng
        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromBody] AddRoomDTO newRoom)
        {
            if (string.IsNullOrEmpty(newRoom.RoomName))
                return BadRequest(new { message = "Tên phòng không được để trống" });

            if (newRoom.NumberOfBed == null || newRoom.NumberOfBed < 1)
                return BadRequest(new { message = "Số giường phải lớn hơn 0" });

            // Kiểm tra tòa nhà có tồn tại không
            var building = await _context.Buildings.FirstOrDefaultAsync(b => b.IdBuilding == newRoom.IdBuilding);
            if (building == null)
                return BadRequest(new { message = "Tòa nhà không tồn tại" });

            var room = new InfoRoomModels
            {
                IdBuilding = (int)newRoom.IdBuilding,
                Gender = newRoom.Gender,
                RoomName = newRoom.RoomName,
                NumberOfBed = newRoom.NumberOfBed.Value,
                Status = newRoom.Status ?? OperatingStatusEnum.active
            };

            _context.InfoRoom.Add(room);
            await _context.SaveChangesAsync();

            // Trả về thông tin phòng vừa tạo, bao gồm cả thông tin tòa nhà
            var createdRoom = new InfoRoomDTOs
            {
                IdRoom = room.IdRoom,
                IdBuilding = room.IdBuilding,
                RoomName = room.RoomName,
                Gender = room.Gender,
                NumberOfBed = room.NumberOfBed,
                Status = room.Status,
                Building = new BuildingDTOs
                {
                    IdBuilding = building.IdBuilding,
                    NameBuilding = building.NameBuilding,
                    Status = building.Status,
                    Description = building.Description
                }
            };

            return CreatedAtAction(nameof(GetRoomById), new { id = room.IdRoom }, createdRoom);
        }


        // 🔹 Cập nhật thông tin phòng
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoom(int id, [FromBody] InfoRoomDTOs updateRoom)
        {
            var room = await _context.InfoRoom.FindAsync(id);
            if (room == null)
                return NotFound(new { message = "Không tìm thấy phòng" });

            if (updateRoom.IdBuilding.HasValue)
            {
                // Kiểm tra tòa nhà có tồn tại không trước khi cập nhật
                var buildingExists = await _context.Buildings.AnyAsync(b => b.IdBuilding == updateRoom.IdBuilding);
                if (!buildingExists)
                    return BadRequest(new { message = "Tòa nhà không tồn tại" });

                room.IdBuilding = updateRoom.IdBuilding.Value;
            }

            if (!string.IsNullOrEmpty(updateRoom.RoomName))
                room.RoomName = updateRoom.RoomName;

            if (updateRoom.NumberOfBed.HasValue)
                room.NumberOfBed = updateRoom.NumberOfBed.Value;

            if (updateRoom.Status.HasValue)
                room.Status = updateRoom.Status.Value;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật phòng thành công" });
        }

        [HttpPut("status")]
        public async Task<IActionResult> UpdateRoomStatus([FromBody] UpdateStatusRoomDTO status)
        {
            try
            {
                var room = await _context.InfoRoom.FindAsync(status.IdRoom);
                if (room == null)
                    return NotFound(new { message = "Không tìm thấy phòng" });

                // Lấy tòa nhà chứa phòng này
                var building = await _context.Buildings.FindAsync(room.IdBuilding);
                if (building == null)
                    return NotFound(new { message = "Không tìm thấy tòa nhà chứa phòng này" });
                Console.WriteLine($"DEBUG - Trạng thái tòa nhà: {building.Status} ({(int)building.Status})");
                // Kiểm tra trạng thái của tòa nhà
                if ((int)building.Status == 1)  // 1 là inactive
                {
                    return BadRequest(new { message = "Tòa nhà đang Inactive, không thể cập nhật trạng thái phòng." });
                }

                // Nếu tòa nhà Active thì mới cho cập nhật phòng
                room.Status = (OperatingStatusEnum)status.Status;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật trạng thái phòng thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi Server", error = ex.Message });
            }
        }



        // 🔹 Xóa phòng
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _context.InfoRoom.FindAsync(id);
            if (room == null)
                return NotFound(new { message = "Không tìm thấy phòng" });

            _context.InfoRoom.Remove(room);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa phòng thành công" });
        }
    }
}
