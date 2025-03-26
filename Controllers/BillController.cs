using API_dormitory.Data;
using API_dormitory.Models.Bills;
using API_dormitory.Models.common;
using API_dormitory.Models.DTO;
using API_dormitory.Models.DTO.Bill.BillElectricity;
using API_dormitory.Models.DTO.Bill.BillRoom;
using API_dormitory.Models.DTO.Building;
using API_dormitory.Models.DTO.Room;
using API_dormitory.Models.Rooms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_dormitory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BillController : ControllerBase
    {

        private readonly AppDbContext _context;

        public BillController(AppDbContext context)
        {
            _context = context;
        }


        [HttpGet("Room")]
        public async Task<IActionResult> GetAllBillRooms()
        {
            var bills = await _context.RoomBill
                .Select(rb => new RoomBillDTO
                {
                    IdRoomBill = rb.IdRoomBill,
                    IdRoom = rb.IdRoom,
                    PriceYear = rb.PriceYear,
                    DailyPrice = rb.DailyPrice,
                    DateOfRecord = rb.DateOfRecord,
                    Status = rb.Status,
                })
                .ToListAsync();
            if (!bills.Any())
            {
                return NoContent();
            }
            return Ok(bills);
        }

        [HttpGet("Room/{idRoom}")]
        public async Task<IActionResult> GetBillByRoomId(int idRoom)
        {
            var bill = await _context.RoomBill
                .Where(rb => rb.IdRoom == idRoom) // Tìm theo IdRoom
                .Select(rb => new RoomBillDTO
                {
                    IdRoomBill = rb.IdRoomBill,
                    IdRoom = rb.IdRoom,
                    PriceYear = rb.PriceYear,
                    DailyPrice = rb.DailyPrice,
                    DateOfRecord = rb.DateOfRecord,
                    Status = rb.Status
                })
                .FirstOrDefaultAsync(); // Dùng FirstOrDefaultAsync() để tránh lỗi

            if (bill == null)
            {
                return NoContent(); // Trả về 204 nếu không có dữ liệu
            }

            return Ok(bill);
        }



        [HttpPost("Room")]
        public async Task<IActionResult> CreateRoomBill([FromBody] AddBillRoomDTOs roomBillDto)
        {
            if (roomBillDto == null)
            {
                return BadRequest("Dữ liệu hóa đơn phòng không hợp lệ.");
            }

            var room = await _context.InfoRoom.FindAsync(roomBillDto.IdRoom);
            if (room == null)
            {
                return NotFound($"Không tìm thấy phòng có ID {roomBillDto.IdRoom}.");
            }

            // Lấy danh sách hóa đơn có trạng thái Active của phòng này
            var activeBills = _context.RoomBill.Where(rb => rb.IdRoom == roomBillDto.IdRoom && rb.Status == OperatingStatusEnum.active);

            // Cập nhật tất cả hóa đơn Active thành Inactive
            foreach (var bill in activeBills)
            {
                bill.Status = OperatingStatusEnum.inactive;
            }

            // Thêm hóa đơn mới với trạng thái Active
            var newRoomBill = new RoomBillModels
            {
                IdRoom = roomBillDto.IdRoom,
                //Price = roomBillDto.Price,
                DateOfRecord = roomBillDto.DateOfRecord,
                Status = OperatingStatusEnum.active,
            };

            _context.RoomBill.Add(newRoomBill);
            await _context.SaveChangesAsync();

            return Ok("Thêm hóa đơn phòng thành công!");
        }

        [HttpPost("price/add-or-update-price")]
        public async Task<IActionResult> AddOrUpdatePrice([FromBody] PriceWaterAndElectricity request)
        {
            if (request == null)
            {
                return BadRequest("Dữ liệu đầu vào không hợp lệ.");
            }

            // Tạo mới một bản ghi với thời gian hiện tại
            var newPrice = new PriceWaterAndElectricity
            {
                electricityPrice = request.electricityPrice,
                waterPrice = request.waterPrice,
                waterLimit = request.waterLimit,
                waterPriceOverLimit = request.waterPriceOverLimit,
                ActionDate = DateTime.UtcNow // Lưu thời gian thực hiện thao tác
            };

            // Thêm vào CSDL
            _context.PriceWaterAndElectricities.Add(newPrice);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Thêm bảng giá điện nước thành công!", Price = newPrice });
        }


        [HttpGet("price")]
        public async Task<IActionResult> GetAllPrices()
        {
            var prices = await _context.PriceWaterAndElectricities.ToListAsync();
            return Ok(prices);
        }

        [HttpDelete("price/{id}")]
        public async Task<IActionResult> DeletePrice(int id)
        {
            var price = await _context.PriceWaterAndElectricities.FindAsync(id);
            if (price == null)
            {
                return NotFound("Không tìm thấy bảng giá.");
            }

            _context.PriceWaterAndElectricities.Remove(price);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Xóa bảng giá thành công!" });
        }

        [HttpPost("electricity/add-electricity-bill")]
        public async Task<IActionResult> AddElectricityBill(AddBillElectricityDTO request)
        {
            var room = await _context.InfoRoom.FindAsync(request.IdRoom);
            if (room == null)
            {
                return NotFound("Phòng không tồn tại.");
            }

            var latestBill = await _context.ElectricityBill
                .Where(b => b.IdRoom == request.IdRoom)
                .OrderByDescending(b => b.DateOfRecord)
                .FirstOrDefaultAsync();

            int beforeIndex = latestBill != null ? latestBill.AfterIndex : 0;
            int consumption = request.AfterIndex - beforeIndex;

            var priceConfig = await _context.PriceWaterAndElectricities
                .OrderByDescending(p => p.idPrice)
                .FirstOrDefaultAsync();
            if (priceConfig == null)
            {
                return BadRequest("Chưa thiết lập giá điện.");
            }

            decimal totalPrice = consumption * priceConfig.electricityPrice;



            var bill = new ElectricityBillModels
            {
                IdRoom = request.IdRoom,
                BeforeIndex = beforeIndex,
                AfterIndex = request.AfterIndex,
                Price = priceConfig.electricityPrice,
                DateOfRecord = DateTime.UtcNow,
                Total = Convert.ToInt32(totalPrice),
                Status = PaymentStatusEnum.unpaid,
            };

            _context.ElectricityBill.Add(bill);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Thêm hóa đơn điện thành công!", Bill = bill });
        }

        [HttpPost("water/add-water-bill")]
        public async Task<IActionResult> AddWaterBill(AddBillElectricityDTO request)
        {
            var room = await _context.InfoRoom.FindAsync(request.IdRoom);
            if (room == null)
            {
                return NotFound("Phòng không tồn tại.");
            }

            var latestBill = await _context.WaterBill
                .Where(b => b.IdRoom == request.IdRoom)
                .OrderByDescending(b => b.DateOfRecord)
                .FirstOrDefaultAsync();

            int beforeIndex = latestBill != null ? latestBill.AfterIndex : 0;
            int consumption = request.AfterIndex - beforeIndex;

            var priceConfig = await _context.PriceWaterAndElectricities
                .OrderByDescending(p => p.idPrice)
                .FirstOrDefaultAsync();
            if (priceConfig == null)
            {
                return BadRequest("Chưa thiết lập giá nước.");
            }

            decimal totalPrice;
            if (consumption <= priceConfig.waterLimit)
            {
                totalPrice = consumption * priceConfig.waterPrice;
            }
            else
            {
                totalPrice = (priceConfig.waterLimit * priceConfig.waterPrice) +
                             ((consumption - priceConfig.waterLimit) * priceConfig.waterPriceOverLimit);
            }

            var bill = new WaterBillModels
            {
                IdRoom = request.IdRoom,
                BeforeIndex = beforeIndex,
                AfterIndex = request.AfterIndex,
                Price = priceConfig.waterPrice,
                IndexLimit = priceConfig.waterLimit,
                PriceLimit = priceConfig.waterPriceOverLimit,
                DateOfRecord = DateTime.UtcNow,
                Total = totalPrice,
                Status = PaymentStatusEnum.unpaid
            };

            _context.WaterBill.Add(bill);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Thêm hóa đơn nước thành công!", Bill = bill });
        }

        [HttpGet("electricity/room/{idRoom}")]
        public async Task<IActionResult> GetElectricityBillsByRoom(int idRoom)
        {
            var bills = await _context.ElectricityBill
                .Where(b => b.IdRoom == idRoom) // Lọc theo RoomId
                                 .OrderByDescending(b => b.DateOfRecord)
                .ToListAsync();

            if (!bills.Any())
            {
                return NotFound(new { message = "Phòng này không có hóa đơn điện nào." });
            }

            return Ok(bills);
        }


        [HttpGet("water/room/{idRoom}")]
        public async Task<IActionResult> GetWaterBillsByRoom(int idRoom)
        {
            var bills = await _context.WaterBill
                 .Where(b => b.IdRoom == idRoom)
                 .OrderByDescending(b => b.DateOfRecord)
                 .ToListAsync();

            if (!bills.Any())
            {
                return NotFound(new { message = "Phòng này không có hóa đơn nước nào." });
            }

            return Ok(bills);
        }



        [HttpGet("electricity/latest/{idRoom}")]
        public async Task<IActionResult> GetLatestElectricityBill(int idRoom)
        {
            var bill = await _context.ElectricityBill
                .Where(b => b.IdRoom == idRoom)
                .OrderByDescending(b => b.DateOfRecord) // Sắp xếp theo ngày gần nhất
                .FirstOrDefaultAsync();

            if (bill == null)
            {
                return NotFound(new { message = "Phòng này không có hóa đơn điện nào." });
            }

            return Ok(bill);
        }

        [HttpGet("water/latest/{idRoom}")]
        public async Task<IActionResult> GetLatestWaterBill(int idRoom)
        {
            var bill = await _context.WaterBill
                .Where(b => b.IdRoom == idRoom)
                .OrderByDescending(b => b.DateOfRecord) // Sắp xếp theo ngày gần nhất
                .FirstOrDefaultAsync();

            if (bill == null)
            {
                return NotFound(new { message = "Phòng này không có hóa đơn nước nào." });
            }

            return Ok(bill);
        }


        [HttpPut("electricity/pay/{idBill}")]
        public async Task<IActionResult> PayElectricityBill(int idBill)
        {
            var bill = await _context.ElectricityBill.FindAsync(idBill);
            if (bill == null)
            {
                return NotFound("Không tìm thấy hóa đơn điện.");
            }

            bill.Status = PaymentStatusEnum.paid;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Hóa đơn điện đã được thanh toán." });
        }

        [HttpPut("water/pay/{idBill}")]
        public async Task<IActionResult> PayWaterBill(int idBill)
        {
            var bill = await _context.WaterBill.FindAsync(idBill);
            if (bill == null)
            {
                return NotFound("Không tìm thấy hóa đơn nước.");
            }

            bill.Status = PaymentStatusEnum.paid;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Hóa đơn nước đã được thanh toán." });
        }

        [HttpDelete("electricity/{idBill}")]
        public async Task<IActionResult> DeleteElectricityBill(int idBill)
        {
            var bill = await _context.ElectricityBill.FindAsync(idBill);
            if (bill == null)
            {
                return NotFound("Không tìm thấy hóa đơn điện.");
            }

            _context.ElectricityBill.Remove(bill);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Xóa hóa đơn điện thành công!" });
        }

        [HttpDelete("water/{idBill}")]
        public async Task<IActionResult> DeleteWaterBill(int idBill)
        {
            var bill = await _context.WaterBill.FindAsync(idBill);
            if (bill == null)
            {
                return NotFound("Không tìm thấy hóa đơn nước.");
            }

            _context.WaterBill.Remove(bill);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Xóa hóa đơn nước thành công!" });
        }

        [HttpGet("room/{idRoom}/has-unpaid-bill")]
        public async Task<IActionResult> CheckUnpaidBill(int idRoom)
        {
            bool hasUnpaidBill = await _context.ElectricityBill.AnyAsync(b => b.IdRoom == idRoom && b.Status == PaymentStatusEnum.unpaid)
                              || await _context.WaterBill.AnyAsync(b => b.IdRoom == idRoom && b.Status == PaymentStatusEnum.unpaid);

            return Ok(new { hasUnpaidBill });
        }

    }




}