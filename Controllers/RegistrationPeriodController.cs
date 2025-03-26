using API_dormitory.Data;
using API_dormitory.Models.common;
using API_dormitory.Models.DTO.RegisterRoom;
using API_dormitory.Models.Registrations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_dormitory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationPeriodController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RegistrationPeriodController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("get-all-registration-periods")]
        public async Task<IActionResult> GetAllRegistrationPeriods()
        {
            var registrationPeriods = await _context.RegistrationPeriods.ToListAsync();
            return Ok(registrationPeriods);
        }


        [HttpGet("get-all-registration-periods-active")]
        public async Task<IActionResult> GetAllRegistrationPeriodsActive()
        {
            var registrationPeriods = await _context.RegistrationPeriods
        .Where(rp => rp.Status == RegistrationStatusEnum.open) // Kiểm tra trạng thái Open
                .FirstOrDefaultAsync();

            if (registrationPeriods == null)
            {
                return NotFound("Không có đợt đăng ký nào đang hoạt động.");
            }

            return Ok(registrationPeriods);
        }

        // Thêm mới kỳ đăng ký
        [HttpPost("add-registration-status")]
        public async Task<IActionResult> UpdateRegistrationStatus([FromBody] RegistrationPeriodModels model)
        {
            if (model == null)
            {
                return BadRequest(new { message = "Dữ liệu không hợp lệ." });
            }

            // Kiểm tra ngày hợp lệ
            if (model.StartDate == null || model.EndDate == null || model.StartDate >= model.EndDate)
            {
                return BadRequest(new {message = "Ngày bắt đầu phải nhỏ hơn ngày kết thúc." });
            }
            if (model.StartDate == null || model.ActionDate == null || model.StartDate <= model.ActionDate)
            {
                return BadRequest(new {message = "Ngày bắt đầu phải lớn hơn ngày hiện tại." });
            }

            // Kiểm tra nếu có kỳ đăng ký nào đang mở
            bool isExistingOpen = await _context.RegistrationPeriods.AnyAsync(rp => rp.Status == RegistrationStatusEnum.open);
            if (isExistingOpen)
            {
                return BadRequest(new {message = "Chỉ có thể có một kỳ đăng ký mở tại một thời điểm." });
            }

            var registrationPeriod = new RegistrationPeriodModels
            {
                Status = model.Status, // Giữ nguyên trạng thái thay vì mặc định là closed
                ActionDate = DateTime.UtcNow,
                StartDate = model.StartDate,
                EndDate = model.EndDate
            };

            _context.RegistrationPeriods.Add(registrationPeriod);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAllRegistrationPeriods), new { id = registrationPeriod.IdRegistrationPeriod },
                new {message = "Thêm mới kỳ đăng ký thành công!", data = registrationPeriod });
        }

        // Xóa kỳ đăng ký
        [HttpDelete("delete-registration-period/{id}")]
        public async Task<IActionResult> DeleteRegistrationPeriod(int id)
        {
            var registrationPeriod = await _context.RegistrationPeriods.FindAsync(id);
            if (registrationPeriod == null)
            {
                return NotFound(new { message = $"Không tìm thấy kỳ đăng ký có ID {id}." });
            }

            _context.RegistrationPeriods.Remove(registrationPeriod);
            await _context.SaveChangesAsync();

            return Ok(new {message = $"Đã xóa kỳ đăng ký thành công." });
        }

        // Cập nhật kỳ đăng ký
        [HttpPut("update-registration-period/{id}")]
        public async Task<IActionResult> UpdateRegistrationPeriod(int id, [FromBody] RegistrationPeriodModels model)
        {
            if (model == null)
            {
                return BadRequest(new {message = "Dữ liệu không hợp lệ." });
            }

            var existingPeriod = await _context.RegistrationPeriods.FindAsync(id);
            if (existingPeriod == null)
            {
                return NotFound(new { message = $"Không tìm thấy kỳ đăng ký có ID {id}." });
            }

            // Kiểm tra ngày hợp lệ
            if (model.StartDate == null || model.EndDate == null || model.StartDate >= model.EndDate)
            {
                return BadRequest(new {  message = "Ngày bắt đầu phải nhỏ hơn ngày kết thúc." });
            }
            if (model.ActionDate == null || model.StartDate < model.ActionDate)
            {
                return BadRequest(new { message = "Ngày bắt đầu phải lớn hơn ngày hiện tại." });
            }

            // Cập nhật thông tin
            existingPeriod.StartDate = model.StartDate;
            existingPeriod.EndDate = model.EndDate;
            existingPeriod.SemesterStatus = model.SemesterStatus;
            existingPeriod.Status = model.Status;
            existingPeriod.ActionDate = DateTime.UtcNow; // Cập nhật ngày thao tác

            _context.RegistrationPeriods.Update(existingPeriod);
            await _context.SaveChangesAsync();

            return Ok(new {message = "Cập nhật kỳ đăng ký thành công!", data = existingPeriod });
        }

    }
}
