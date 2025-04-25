using API_dormitory.Data;
using API_dormitory.Models.common;
using API_dormitory.Models.DTO.RegisterRoom;
using API_dormitory.Models.Registrations;
using API_dormitory.Models.Rooms;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API_dormitory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationPeriodController : ControllerBase
    {
        private readonly IMongoCollection<RegistrationPeriodModels> _registrationPeriods;

        public RegistrationPeriodController(MongoDbContext mongoContext)
        {
            _registrationPeriods = mongoContext.GetCollection<RegistrationPeriodModels>("RegistrationPeriods");
        }

        // Lấy tất cả kỳ đăng ký
        [HttpGet("get-all-registration-periods")]
        public async Task<IActionResult> GetAllRegistrationPeriods()
        {
            var registrationPeriods = await _registrationPeriods.Find(_ => true).ToListAsync();
            return Ok(registrationPeriods);
        }

        // Lấy kỳ đăng ký đang hoạt động
        [HttpGet("get-all-registration-periods-active")]
        public async Task<IActionResult> GetAllRegistrationPeriodsActive()
        {
            var activePeriod = await _registrationPeriods
                .Find(rp => rp.Status == RegistrationStatusEnum.open)
                .FirstOrDefaultAsync();

            if (activePeriod == null)
            {
                return NotFound("Không có đợt đăng ký nào đang hoạt động.");
            }

            return Ok(activePeriod);
        }


        [HttpPost("add-registration-period")]
        public async Task<IActionResult> AddRegistrationPeriod([FromBody] AddRegistrationPeriodDTOs model)
        {
            if (model == null)
                return BadRequest(new { message = "Dữ liệu không hợp lệ." });

            if (model.StartDate >= model.EndDate)
                return BadRequest(new { message = "Ngày bắt đầu phải nhỏ hơn ngày kết thúc." });

            if (model.StartDate <= DateTime.UtcNow)
                return BadRequest(new { message = "Ngày bắt đầu phải lớn hơn ngày hiện tại." });

            // Kiểm tra nếu đã có kỳ đăng ký mở
            var isExistingOpen = await _registrationPeriods
                .Find(rp => rp.Status == RegistrationStatusEnum.open)
                .AnyAsync();
            if (isExistingOpen)
                return BadRequest(new { message = "Chỉ có thể có một kỳ đăng ký mở tại một thời điểm." });

            var registrationPeriod = new RegistrationPeriodModels
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Status = model.Status,
                SemesterStatus = model.SemesterStatus,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                ActionDate = DateTime.UtcNow
            };

            await _registrationPeriods.InsertOneAsync(registrationPeriod);
            return Ok(new { message = "Thêm mới kỳ đăng ký thành công!"});
        }

        // Xóa kỳ đăng ký
        [HttpDelete("delete-registration-period/{id}")]
        public async Task<IActionResult> DeleteRegistrationPeriod(string id)
        {
            var result = await _registrationPeriods.DeleteOneAsync(rp => rp.Id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound(new { message = $"Không tìm thấy kỳ đăng ký có ID {id}." });
            }

            return Ok(new { message = "Đã xóa kỳ đăng ký thành công." });
        }

        // Cập nhật kỳ đăng ký
        [HttpPut("update-registration-period/{id}")]
        public async Task<IActionResult> UpdateRegistrationPeriod(string id, [FromBody] RegistrationPeriodModels model)
        {
            if (model == null)
            {
                return BadRequest(new { message = "Dữ liệu không hợp lệ." });
            }

            var existingPeriod = await _registrationPeriods.Find(rp => rp.Id == id).FirstOrDefaultAsync();
            if (existingPeriod == null)
            {
                return NotFound(new { message = $"Không tìm thấy kỳ đăng ký có ID {id}." });
            }

            // Kiểm tra ngày hợp lệ
            if (model.StartDate >= model.EndDate)
            {
                return BadRequest(new { message = "Ngày bắt đầu phải nhỏ hơn ngày kết thúc." });
            }
            if (model.StartDate <= DateTime.UtcNow)
            {
                return BadRequest(new { message = "Ngày bắt đầu phải lớn hơn ngày hiện tại." });
            }

            // Cập nhật thông tin
            existingPeriod.StartDate = model.StartDate;
            existingPeriod.EndDate = model.EndDate;
            existingPeriod.SemesterStatus = model.SemesterStatus;
            existingPeriod.Status = model.Status;
            existingPeriod.ActionDate = DateTime.UtcNow;

            await _registrationPeriods.ReplaceOneAsync(rp => rp.Id == id, existingPeriod);

            return Ok(new { message = "Cập nhật kỳ đăng ký thành công!", data = existingPeriod });
        }


        [HttpPut("update-registration-period-status/{id}")]
        public async Task<IActionResult> UpdateRegistrationPeriodStatus(string id, [FromBody] RegistrationStatusDTO Registration)
        {
            var existingPeriod = await _registrationPeriods.Find(rp => rp.Id == id).FirstOrDefaultAsync();
            if (existingPeriod == null)
            {
                return NotFound(new { message = $"Không tìm thấy kỳ đăng ký có ID {id}." });
            }

            // Kiểm tra giá trị status có hợp lệ không
            if (!Enum.IsDefined(typeof(RegistrationStatusEnum), Registration.Status))
            {
                return BadRequest(new { message = "Trạng thái không hợp lệ." });
            }

            // Cập nhật trạng thái
            existingPeriod.Status = Registration.Status;
            existingPeriod.ActionDate = DateTime.UtcNow; // Ghi nhận thời gian cập nhật

            await _registrationPeriods.ReplaceOneAsync(rp => rp.Id == id, existingPeriod);

            return Ok(new { message = "Cập nhật trạng thái kỳ đăng ký thành công!"});
        }


    }
}
