using API_dormitory.Data;
using API_dormitory.Models.Bills;
using API_dormitory.Models.common;
using API_dormitory.Models.DTO.Building;
using API_dormitory.Models.DTO.RegisterRoom;
using API_dormitory.Models.DTO.Room;
using API_dormitory.Models.DTO.User;
using API_dormitory.Models.registerRoom;
using API_dormitory.Models.Registrations;
using API_dormitory.Models.Rooms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;

namespace API_dormitory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterRoomController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RegisterRoomController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRegister([FromBody] AddRegisterRoomDTOs registerDto)
        {
            try
            {
                if (registerDto == null)
                {
                    return BadRequest(new { message = "Dữ liệu đăng ký không hợp lệ." });
                }
            registerDto.actionDate = DateTime.Now;

                var user = await _context.InfoStudents.FindAsync(registerDto.idUser);
                if (user == null)
                {
                    return NotFound(new { message = $"Không tìm thấy người dùng có ID {registerDto.idUser}." });
                }
                var genderRoom = await _context.InfoRoom.FindAsync(registerDto.idRoom);
                var genderUser = await _context.InfoStudents.FindAsync(registerDto.idUser);

                if (genderUser.Gender != genderRoom.Gender) 
                {
                    return NotFound(new { message = $"Phòng không phù hợp với giới tính của người đăng ký: Phòng dành cho {genderRoom.Gender} và giới tính của người đăng ký là {genderUser.Gender}" });
                }

                var room = await _context.InfoRoom.FindAsync(registerDto.idRoom);

                // Kiểm tra trạng thái phòng
                if (room.Status == OperatingStatusEnum.inactive)
                {
                    return BadRequest(new { message = "Phòng hiện không mở." });
                }
                if (room == null)
                {
                    return NotFound(new { message = $"Không tìm thấy phòng có ID {registerDto.idRoom}." });
                }

                // Kiểm tra trạng thái đăng ký
                var registrationStatus = await _context.RegistrationPeriods
                    .OrderByDescending(rp => rp.Status == RegistrationStatusEnum.open)
                    .FirstOrDefaultAsync();

                if (registrationStatus == null || registrationStatus.Status == RegistrationStatusEnum.closed)
                {
                    return BadRequest(new { message = "Đăng ký hiện đang đóng." });
                }

                // Kiểm tra xem người dùng đã có phòng đang hoạt động chưa
                var existingRegister = await _context.RegisterRoom
                    .Where(r => r.idStudent == registerDto.idUser && r.status == OperatingStatusEnum.active)
                    .FirstOrDefaultAsync();

                if (existingRegister != null &&
                   (existingRegister.paymentStatus == PaymentStatusEnum.unpaid ||
                    existingRegister.paymentStatus == PaymentStatusEnum.paid))
                {
                    return BadRequest(new { message = "Bạn đã đăng ký phòng trước đó và vẫn đang hoạt động." });
                }



                // Lấy hóa đơn phòng đang active
                var roomBill = await _context.RoomBill
                    .Where(rb => rb.IdRoom == registerDto.idRoom && rb.Status == OperatingStatusEnum.active)
                    .OrderByDescending(rb => rb.DateOfRecord)
                    .FirstOrDefaultAsync();

                if (roomBill == null)
                {
                    return BadRequest(new { message = "Không tìm thấy hóa đơn phòng hợp lệ." });
                }

                // Tính toán tổng giá
                decimal totalPrice;
                if (registrationStatus.SemesterStatus == SemesterStatusEnum.inSemester)
                {
                    totalPrice = roomBill.PriceYear;
                }
                else
                {
                    int totalDays = (registerDto.endDate - registerDto.startDate).Days;
                    totalPrice = roomBill.DailyPrice * totalDays;
                }

                var newRegister = new RegisterRoomModels
                {
                    idStudent = registerDto.idUser,
                    idRoom = registerDto.idRoom,
                    idRegistrationPeriod = registerDto.idRegistrationPeriod,
                    startDate = registerDto.startDate,
                    endDate = registerDto.endDate,
                    ActionDate = registerDto.actionDate,
                    total = totalPrice,
                    paymentStatus = registerDto.paymentStatus,
                    status = OperatingStatusEnum.wait
                };

                _context.RegisterRoom.Add(newRegister);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Đăng ký phòng thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPut("update-status")]
        public async Task<IActionResult> UpdateRegisterRoomStatus()
        {
            var currentDate = DateTime.UtcNow;

            // Lấy tất cả các đăng ký có ngày kết thúc nhỏ hơn ngày hiện tại
            var expiredRegistrations = await _context.RegisterRoom
                .Where(r => r.endDate < currentDate && r.status != OperatingStatusEnum.inactive)
                .ToListAsync();

            if (!expiredRegistrations.Any())
            {
                return Ok("Không có đăng ký nào cần cập nhật.");
            }

            // Cập nhật trạng thái thành Inactive
            foreach (var registration in expiredRegistrations)
            {
                registration.status = OperatingStatusEnum.inactive;
            }

            await _context.SaveChangesAsync();

            return Ok($"Đã cập nhật {expiredRegistrations.Count} đăng ký thành Inactive.");
        }

        [HttpGet("get-all-registers")]
        public async Task<IActionResult> GetAllRegisters()
        {
            var registers = await _context.RegisterRoom
                .Include(r => r.InfoStudent)  // Lấy thông tin User
                .Include(r => r.InfoRoom)  // Lấy thông tin Room
                .ThenInclude(r => r.Building) // Nếu muốn lấy thông tin tòa nhà
                .Select(r => new
                {
                    idRegister = r.IdRegister,
                    idUser = r.idStudent,
                    idRoom = r.idRoom,
                    idRegistrationPeriod = r.idRegistrationPeriod,
                    startDate = r.startDate,
                    endDate = r.endDate,
                    total = r.total,
                    paymentStatus = r.paymentStatus,
                    status = r.status,
                    
                })
                .ToListAsync();

            if (!registers.Any())
            {
                return NotFound("Không có dữ liệu đăng ký.");
            }

            return Ok(registers);
        }



        [HttpGet("get-active-registers")]
        public async Task<IActionResult> GetActiveRegisters()
        {
            var activeRegisters = await _context.RegisterRoom
                .Where(r => r.status == OperatingStatusEnum.active) // Lọc các đăng ký đang hoạt động
                .Select(r => new
                {
                    idRegister = r.IdRegister,
                    idUser = r.idStudent,
                    idRoom = r.idRoom,
                    idRegistrationPeriod = r.idRegistrationPeriod,
                    startDate = r.startDate,
                    endDate = r.endDate,
                    actionDate = r.ActionDate,
                    total = r.total,
                    paymentStatus = r.paymentStatus,
                    status = r.status
                })
                .ToListAsync();

            if (!activeRegisters.Any())
            {
                return NotFound("Danh sách rỗng");
            }

            return Ok(activeRegisters);
        }

        [HttpGet("get-active-registers-byIdRoom/{idRoom}")]
        public async Task<IActionResult> GetActiveRegistersByRoom(int idRoom)
        {
            var activeRegisters = await _context.RegisterRoom
.Where(r =>
    (r.status == OperatingStatusEnum.active || r.status == OperatingStatusEnum.wait)
    && r.idRoom == idRoom)
                .Select(r => new
                {
                    idRegister = r.IdRegister,
                    idUser = r.idStudent,
                    idRoom = r.idRoom,
                    idRegistrationPeriod = r.idRegistrationPeriod,
                    startDate = r.startDate,
                    endDate = r.endDate,
                    actionDate = r.ActionDate,
                    total = r.total,
                    paymentStatus = r.paymentStatus,
                    status = r.status
                })
                .ToListAsync();

            if (!activeRegisters.Any())
            {
                return NotFound($"Không có đăng ký nào cho phòng ID {idRoom}.");
            }

            return Ok(activeRegisters);
        }

        [HttpGet("get-active-registers-byIdUser/{idUser}")]
        public async Task<IActionResult> GetActiveRegistersByUser(int idUser)
        {
            var activeRegisters = await _context.RegisterRoom
                .Where(r => r.status == OperatingStatusEnum.active && r.idStudent == idUser) // Lọc theo status và idUser
                .Select(r => new
                {
                    idRegister = r.IdRegister,
                    idUser = r.idStudent,
                    idRoom = r.idRoom,
                    startDate = r.startDate,
                    endDate = r.endDate,
                    actionDate = r.ActionDate,
                    total = r.total,
                    paymentStatus = r.paymentStatus,
                    status = r.status
                })
                .ToListAsync();

            if (!activeRegisters.Any())
            {
                return NotFound($"Không có đăng ký nào cho người dùng ID {idUser}.");
            }

            return Ok(activeRegisters);
        }

        [HttpGet("get-registers-byUser/{idUser}")]
        public async Task<IActionResult> GetRegistersByUser(int idUser)
        {
            var registers = await _context.RegisterRoom
                .Where(r => r.idStudent == idUser)
                .Include(r => r.InfoRoom) // Lấy thông tin phòng
                .ThenInclude(room => room.Building) // Lấy thông tin tòa nhà
                .Select(r => new
                {
                    idRegister = r.IdRegister,
                    idUser = r.idStudent,
                    idRoom = r.idRoom,
                    startDate = r.startDate,
                    endDate = r.endDate,
                    actionDate = r.ActionDate,
                    total = r.total,
                    paymentStatus = r.paymentStatus,
                    status = r.status,
                    roomName = r.InfoRoom.RoomName, // Tên phòng
                    buildingName = r.InfoRoom.Building.NameBuilding // Tên tòa nhà
                })
                .ToListAsync();

            if (!registers.Any())
            {
                return NotFound("Không có đăng ký nào.");
            }

            return Ok(registers);
        }



        [HttpGet("get-all-registers-byIdRoom/{idRoom}")]
        public async Task<IActionResult> GetAllRegistersByRoom(int idRoom)
        {
            var activeRegisters = await _context.RegisterRoom
                .Where(r => r.idRoom == idRoom) // Lọc theo status và idRoom
                .Select(r => new
                {
                    idRegister = r.IdRegister,
                    idUser = r.idStudent,
                    idRoom = r.idRoom,
                    startDate = r.startDate,
                    endDate = r.endDate,
                    total = r.total,
                    paymentStatus = r.paymentStatus,
                    status = r.status
                })
                .ToListAsync();

            if (!activeRegisters.Any())
            {
                return NotFound($"Không có đăng ký nào cho phòng ID {idRoom}.");
            }

            return Ok(activeRegisters);
        }


        [HttpPut("update-status-payment/{idRegister}/{newPaymentStatus}")]
        public async Task<IActionResult> UpdateStatusByPayment(int idRegister, int newPaymentStatus)
        {
            try
            {
                var registerRoom = await _context.RegisterRoom.FindAsync(idRegister);
                if (registerRoom == null)
                {
                    return NotFound(new { message = $"Không tìm thấy đăng ký có ID {idRegister}." });
                }
                if(newPaymentStatus == 0)
                {
                    registerRoom.paymentStatus = PaymentStatusEnum.paid;
                    registerRoom.status = OperatingStatusEnum.active;
                }else
                if (newPaymentStatus == 1)
                {
                    registerRoom.paymentStatus = PaymentStatusEnum.unpaid;
                    registerRoom.status = OperatingStatusEnum.wait;
                }
                else if (newPaymentStatus == 2)
                {
                    registerRoom.paymentStatus = PaymentStatusEnum.cancel;
                    registerRoom.status = OperatingStatusEnum.blocked;
                }


                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật trạng thái đăng ký thành công.", status = registerRoom.status });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }

}