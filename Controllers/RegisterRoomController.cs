using API_dormitory.Data;
using API_dormitory.Models.Bills;
using API_dormitory.Models.common;
using API_dormitory.Models.DTO.RegisterRoom;
using API_dormitory.Models.registerRoom;
using API_dormitory.Models.Rooms;
using API_dormitory.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_dormitory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterRoomController : ControllerBase
    {
        private readonly IMongoCollection<RegisterRoomModels> _registerRoomCollection;
        private readonly IMongoCollection<RoomBillModels> _roomBillCollection;
        private readonly IMongoCollection<PriceWaterAndElectricity> _priceCollection;
        private readonly IMongoCollection<InfoRoomModels> _roomsCollection;
        private readonly IMongoCollection<BuildingModels> _buildingsCollection;
        private readonly IMongoCollection<AccountModels> _accounts;
        private readonly EmailService _emailService;
        private readonly IMongoCollection<InfoStudentModels> _infoStudents;
        private readonly IMongoClient _mongoClient;

        /*        private readonly IRoomService _roomService;
        */

        public RegisterRoomController(MongoDbContext database, IMongoClient mongoClient, EmailService emailService)
        {
            _registerRoomCollection = database.GetCollection<RegisterRoomModels>("RegisterRoom");
            _roomBillCollection = database.GetCollection<RoomBillModels>("RoomBills");
            _roomsCollection = database.GetCollection<InfoRoomModels>("Rooms");
            _buildingsCollection = database.GetCollection<BuildingModels>("Buildings");
            _accounts = database.GetCollection<AccountModels>("Accounts");
            _infoStudents = database.GetCollection<InfoStudentModels>("InfoStudents");
            _emailService = emailService;
            _mongoClient = mongoClient;
        }

        [Authorize(Roles = "Admin,Student,Staff")]
        [HttpGet]
        public async Task<IActionResult> GetAllRegisterRooms()
        {
            var registerRooms = await _registerRoomCollection.Find(_ => true).ToListAsync();

            if (registerRooms == null || registerRooms.Count == 0)
                return NotFound(new { message = "Không có dữ liệu đăng ký phòng." });

            var result = new List<object>();

            foreach (var r in registerRooms)
            {
                // Lấy thông tin sinh viên
                var student = await _infoStudents.Find(u => u.Id == r.IdStudent).FirstOrDefaultAsync();

                // Nếu có sinh viên, lấy thông tin tài khoản từ `AccountId`
                AccountModels? account = null;
                if (student?.AccountId != null)
                {
                    account = await _accounts.Find(u => u.AccountId == student.AccountId).FirstOrDefaultAsync();
                }

                // Lấy thông tin phòng
                var room = await _roomsCollection.Find(room => room.IdRoom == r.IdRoom).FirstOrDefaultAsync();

                // Lấy thông tin tòa nhà nếu có phòng
                var building = room != null
      ? await _buildingsCollection.Find(b => b.IdBuilding == room.IdBuilding)
                                  .Project(b => new { b.NameBuilding })
                                  .FirstOrDefaultAsync()
      : null;

                var registerRoomWithDetails = new
                {
                    IdRegister = r.IdRegister.ToString(),
                    IdStudent = r.IdStudent.ToString(),
                    StudentInfo = student != null ? new
                    {
                        Id = student.Id.ToString(),
                        student.Email,
                        student.Address,
                        student.NameParent,
                        student.ParentNumberPhone
                    } : null,
                    AccountInfo = account != null ? new
                    {
                        AccountId = account.AccountId.ToString(),
                        account.UserName,
                        account.UserCode,
                        account.NumberPhone,
                        account.Roles,
                        account.Status
                    } : null,
                    IdRoom = r.IdRoom.ToString(),
                    RoomInfo = room != null ? new
                    {
                        room.RoomName
                    } : null,
                    BuildingInfo = building != null ? new
                    {
                        building.NameBuilding
                    } : null,
                    IdRegistrationPeriod = r.IdRegistrationPeriod.ToString(),
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    ActionDate = r.ActionDate,
                    Total = r.Total,
                    PaymentStatus = r.PaymentStatus,
                    Status = r.Status
                };

                result.Add(registerRoomWithDetails);
            }

            return Ok(result);
        }
        [Authorize(Roles = "Admin")]

        [HttpGet("count-by-registration-period")]
        public async Task<IActionResult> GetStudentCountByRegistrationPeriod()
        {
            var registerRooms = await _registerRoomCollection.Find(_ => true).ToListAsync();

            if (registerRooms == null || registerRooms.Count == 0)
                return NotFound(new { message = "Không có dữ liệu đăng ký phòng." });

            var grouped = registerRooms
                .GroupBy(r => r.IdRegistrationPeriod)
                .Select(g => new
                {
                    IdRegistrationPeriod = g.Key.ToString(),
                    StartDate = g.FirstOrDefault()?.StartDate,  // Giả sử có trường StartDate
                    EndDate = g.FirstOrDefault()?.EndDate,    // Giả sử có trường EndDate
                    StudentCount = g.Select(x => x.IdStudent).Distinct().Count()
                })
                .ToList();

            return Ok(grouped);
        }


        [Authorize(Roles = "Admin,Student")]
        [HttpPost]
        public async Task<IActionResult> CreateRegister([FromBody] AddRegisterRoomDTOs registerDto)
        {
            if (registerDto == null)
                return BadRequest(new { message = "Dữ liệu đăng ký không hợp lệ." });

            registerDto.actionDate = DateTime.UtcNow;

            var studentId = ObjectId.Parse(registerDto.idUser);
            var roomId = ObjectId.Parse(registerDto.idRoom);
            var registrationPeriodId = ObjectId.Parse(registerDto.idRegistrationPeriod);

            var client = _mongoClient;
            var session = await client.StartSessionAsync();

            using (session)
            {
                session.StartTransaction();

                try
                {
                    // Kiểm tra sinh viên đã đăng ký 'active' chưa
                    var existingRegister = await _registerRoomCollection
                        .Find(r => r.IdStudent == studentId &&
                                   (r.Status == OperatingStatusEnum.active || r.Status == OperatingStatusEnum.wait))
                        .FirstOrDefaultAsync();
                    // Lấy thông tin phòng
                    var room = await _roomsCollection.Find(r => r.IdRoom == roomId).FirstOrDefaultAsync();
                    if (room == null)
                    {
                        await session.AbortTransactionAsync();
                        return NotFound(new { message = "Phòng không tồn tại." });
                    }

                    // Đếm số lượng đăng ký active hoặc wait trong phòng
                    var activeRegistrationsCount = await _registerRoomCollection
                        .CountDocumentsAsync(r =>
                            (r.IdRoom == roomId) &&
                            (r.Status == OperatingStatusEnum.active || r.Status == OperatingStatusEnum.wait));

                    int actualAvailableBeds = room.NumberOfBed - (int)activeRegistrationsCount;
                    if (actualAvailableBeds <= 0)
                    {
                        await session.AbortTransactionAsync();
                        return BadRequest(new { message = "Phòng này không còn giường trống." });
                    }

                    // Lấy thông tin sinh viên
                    var student = await _infoStudents.Find(s => s.Id == studentId).FirstOrDefaultAsync();
                    if (student == null)
                    {
                        await session.AbortTransactionAsync();
                        return NotFound(new { message = "Sinh viên không tồn tại." });
                    }

                    if (student.Gender != room.Gender)
                    {
                        await session.AbortTransactionAsync();
                        return BadRequest(new { message = "Giới tính của sinh viên không phù hợp với phòng." });
                    }

                    if (existingRegister != null)
                    {
                        await session.AbortTransactionAsync();
                        return BadRequest(new { message = "Bạn đã đăng ký phòng trước đó và vẫn đang hoạt động." });
                    }

                    var newRegister = new RegisterRoomModels
                    {
                        IdRegister = ObjectId.GenerateNewId(),
                        IdStudent = studentId,
                        IdRoom = roomId,
                        IdRegistrationPeriod = registrationPeriodId,
                        StartDate = registerDto.startDate,
                        EndDate = registerDto.endDate,
                        ActionDate = registerDto.actionDate,
                        Total = (double)registerDto.total,
                        PaymentStatus = registerDto.paymentStatus,
                        Status = OperatingStatusEnum.wait
                    };

                    await _registerRoomCollection.InsertOneAsync(session, newRegister);
                    await session.CommitTransactionAsync();

                    // Gửi email thông báo nếu cần
                    var studentInfo = await _infoStudents.Find(s => s.Id == studentId).FirstOrDefaultAsync();
                    if (studentInfo?.Email != null)
                    {
                        var studentName = studentInfo.Account?.UserName ?? "Bạn";
                        string subject = "";
                        string body = "";

                        if (registerDto.paymentStatus == PaymentStatusEnum.paid)
                        {
                            subject = "Thông báo thanh toán thành công";
                            body = $@"
                        <p>Xin chào <strong>{studentName}</strong>,</p>
                        <p>Phòng của bạn đã được thanh toán thành công.</p>
                        <p>Chúc bạn có khoảng thời gian học tập và sinh hoạt thật thoải mái tại Ký túc xá.</p>
                        <p>Trân trọng,<br/>Ban Quản lý Ký túc xá</p>";
                        }
                        else if (registerDto.paymentStatus == PaymentStatusEnum.unpaid)
                        {
                            subject = "Thông báo thanh toán trước ngày hết hạn";
                            body = $@"
                        <p>Xin chào <strong>{studentName}</strong>,</p>
                        <p>Phòng Quản lý Ký túc xá xin thông báo đến bạn về việc thanh toán phí phòng.</p>
                        <p>Vui lòng hoàn tất việc thanh toán trước ngày bắt đầu thời gian ở để đảm bảo quyền lợi và tránh gián đoạn trong quá trình đăng ký.</p>
                        <p>Trân trọng cảm ơn bạn đã hợp tác.</p>
                        <p>Trân trọng,<br/>Ban Quản lý Ký túc xá</p>";
                        }

                        await _emailService.SendEmailAsync(studentInfo.Email, studentName, subject, body);
                    }

                    return Ok(new { message = "Đăng ký phòng thành công!" });
                }
                catch (Exception ex)
                {
                    if (session.IsInTransaction)
                    {
                        await session.AbortTransactionAsync();
                    }

                    Console.WriteLine("❌ Lỗi trong quá trình đăng ký: " + ex.Message);
                    return StatusCode(500, new { message = "Đã xảy ra lỗi khi xử lý đăng ký." });
                }
            }
        }


        [Authorize(Roles = "Admin")]
        [HttpPut("update-payment-status/{idRegister}")]
        public async Task<IActionResult> UpdatePaymentStatus(string idRegister, [FromBody] UpdateStatusPaymentDTO newPaymentStatus)
        {
            if (!ObjectId.TryParse(idRegister, out var objectId))
                return BadRequest(new { message = "IdRegister không hợp lệ." });

            var filter = Builders<RegisterRoomModels>.Filter.Eq(r => r.IdRegister, objectId);

            // Lấy đăng ký phòng hiện tại để biết thông tin sinh viên
            var existingRegister = await _registerRoomCollection.Find(filter).FirstOrDefaultAsync();

            if (existingRegister == null)
                return NotFound(new { message = "Không tìm thấy đăng ký phòng." });

            // Tạo update
            var updateDefinition = Builders<RegisterRoomModels>.Update
                .Set(r => r.PaymentStatus, newPaymentStatus.statusPayment);

            if (newPaymentStatus.statusPayment == PaymentStatusEnum.paid)
            {
                updateDefinition = updateDefinition.Set(r => r.Status, OperatingStatusEnum.active);
            }
            else if (newPaymentStatus.statusPayment == PaymentStatusEnum.cancel)
            {
                updateDefinition = updateDefinition.Set(r => r.Status, OperatingStatusEnum.blocked);
            }

            var result = await _registerRoomCollection.UpdateOneAsync(filter, updateDefinition);

            if (result.MatchedCount == 0)
                return NotFound(new { message = "Không tìm thấy đăng ký phòng." });

            // Gửi email theo trạng thái thanh toán mới
            var studentInfo = await _infoStudents.Find(s => s.Id == existingRegister.IdStudent).FirstOrDefaultAsync();

            if (studentInfo?.Email != null)
            {
                var studentName = studentInfo.Account?.UserName ?? "Bạn";
                string subject = "";
                string body = "";

                switch (newPaymentStatus.statusPayment)
                {
                    case PaymentStatusEnum.paid:
                        subject = "Xác nhận thanh toán thành công";
                        body = $@"
                            <p>Xin chào <strong>{studentName}</strong>,</p>

                            <p>Phòng Quản lý Ký túc xá xin thông báo rằng bạn đã hoàn tất thanh toán phòng thành công.</p>

                            <p>Chúc bạn có một khoảng thời gian học tập và sinh hoạt thật thoải mái tại Ký túc xá.</p>

                            <p>Trân trọng,<br/>Ban Quản lý Ký túc xá</p>";
                        break;

                    case PaymentStatusEnum.unpaid:
                        subject = "Nhắc nhở thanh toán tiền phòng";
                        body = $@"
                            <p>Xin chào <strong>{studentName}</strong>,</p>

                            <p>Hệ thống ghi nhận bạn vẫn chưa hoàn tất thanh toán tiền phòng.</p>

                            <p>Vui lòng thực hiện thanh toán trước thời hạn để đảm bảo quyền lợi lưu trú và tránh gián đoạn trong việc đăng ký.</p>

                            <p>Trân trọng,<br/>Ban Quản lý Ký túc xá</p>";
                        break;

                    case PaymentStatusEnum.cancel:
                        subject = "Thông báo huỷ đăng ký phòng";
                        body = $@"
                            <p>Xin chào <strong>{studentName}</strong>,</p>

                            <p>Rất tiếc, đăng ký phòng của bạn đã bị huỷ do không hoàn tất thanh toán đúng thời hạn hoặc theo yêu cầu.</p>

                            <p>Nếu bạn có bất kỳ thắc mắc nào, vui lòng liên hệ Ban Quản lý Ký túc xá để được hỗ trợ thêm.</p>

                            <p>Trân trọng,<br/>Ban Quản lý Ký túc xá</p>";
                        break;
                }

                try
                {
                    await _emailService.SendEmailAsync(
                        studentInfo.Email,
                        studentName,
                        subject,
                        body
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine("❌ Lỗi khi gửi email sau khi cập nhật trạng thái thanh toán: " + ex.Message);
                }
            }
            return Ok(new { message = "Cập nhật trạng thái thanh toán thành công!" });
        }

        [Authorize(Roles = "Admin,Student")]
        [HttpPut("update-status/{idRegister}")]
        public async Task<IActionResult> UpdateOperatingStatus(string idRegister, [FromBody] UpdateStatusDTO newStatus)
        {
            if (!ObjectId.TryParse(idRegister, out var objectId))
                return BadRequest(new { message = "IdRegister không hợp lệ." });

            var filter = Builders<RegisterRoomModels>.Filter.Eq(r => r.IdRegister, objectId);

            // Lấy bản ghi trước khi update để có thông tin sinh viên
            var register = await _registerRoomCollection.Find(filter).FirstOrDefaultAsync();
            if (register == null)
                return NotFound(new { message = "Không tìm thấy đăng ký phòng." });

            // Cập nhật trạng thái
            var update = Builders<RegisterRoomModels>.Update.Set(r => r.Status, newStatus.status);
            var result = await _registerRoomCollection.UpdateOneAsync(filter, update);

            // Lấy thông tin sinh viên để gửi email
            var studentInfo = await _infoStudents.Find(s => s.Id == register.IdStudent).FirstOrDefaultAsync();
            if (studentInfo?.Email != null)
            {
                var studentName = studentInfo.Account?.UserName ?? "Bạn";
                string subject = "";
                string body = "";

                switch (newStatus.status)
                {
                    case OperatingStatusEnum.active:
                        subject = "Thông báo kích hoạt đăng ký phòng";
                        body = $@"
                            <p>Xin chào <strong>{studentName}</strong>,</p>

                            <p>Đăng ký phòng của bạn đã được <strong>kích hoạt thành công</strong>.</p>

                            <p>Chúc bạn có khoảng thời gian học tập và sinh hoạt thật thuận lợi tại Ký túc xá.</p>

                            <p>Trân trọng,<br/>Ban Quản lý Ký túc xá</p>";
                        break;

                    case OperatingStatusEnum.blocked:
                        subject = "Thông báo huỷ đăng ký phòng";
                        body = $@"
                            <p>Xin chào <strong>{studentName}</strong>,</p>

                            <p>Đăng ký phòng của bạn đã bị <strong>huỷ hoặc chặn</strong>. Nguyên nhân có thể do vi phạm quy định hoặc không đáp ứng điều kiện lưu trú.</p>

                            <p>Vui lòng liên hệ Ban Quản lý Ký túc xá để được hỗ trợ thêm.</p>

                            <p>Trân trọng,<br/>Ban Quản lý Ký túc xá</p>";
                        break;

                    case OperatingStatusEnum.inactive:
                        subject = "Thông báo tạm ngưng đăng ký phòng";
                        body = $@"
                            <p>Xin chào <strong>{studentName}</strong>,</p>

                            <p>Trạng thái đăng ký phòng của bạn hiện đang <strong>tạm ngưng</strong>.</p>

                            <p>Vui lòng kiểm tra lại thông tin cá nhân hoặc liên hệ bộ phận quản lý để được giải đáp.</p>

                            <p>Trân trọng,<br/>Ban Quản lý Ký túc xá</p>";
                        break;

                    case OperatingStatusEnum.wait:
                        subject = "Thông báo chờ duyệt đăng ký phòng";
                        body = $@"
                            <p>Xin chào <strong>{studentName}</strong>,</p>

                            <p>Hệ thống ghi nhận bạn đã gửi yêu cầu đăng ký phòng. Hiện trạng thái đăng ký đang <strong>chờ xét duyệt</strong>.</p>

                            <p>Vui lòng theo dõi email hoặc liên hệ Ban Quản lý để biết thêm thông tin.</p>

                            <p>Trân trọng,<br/>Ban Quản lý Ký túc xá</p>";
                        break;
                }

                try
                {
                    await _emailService.SendEmailAsync(
                        studentInfo.Email,
                        studentName,
                        subject,
                        body
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine("❌ Lỗi khi gửi email sau khi cập nhật trạng thái: " + ex.Message);
                }
            }

            return Ok(new { message = "Cập nhật trạng thái hoạt động thành công!" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete-register/{idRegister}")]
        public async Task<IActionResult> DeleteRegisterRoom(string idRegister)
        {
            if (!ObjectId.TryParse(idRegister, out var objectId))
                return BadRequest(new { message = "IdRegister không hợp lệ." });

            var filter = Builders<RegisterRoomModels>.Filter.Eq(r => r.IdRegister, objectId);
            var result = await _registerRoomCollection.DeleteOneAsync(filter);

            if (result.DeletedCount == 0)
                return NotFound(new { message = "Không tìm thấy đăng ký phòng để xóa." });

            return Ok(new { message = "Xóa đăng ký phòng thành công!" });
        }

        [Authorize(Roles = "Admin,Student")]
        [HttpGet("total-registered-students")]
        public async Task<IActionResult> GetTotalRegisteredStudents()
        {
            // Tổng số sinh viên trong hệ thống
            var totalStudents = await _infoStudents.CountDocumentsAsync(FilterDefinition<InfoStudentModels>.Empty);

            // Tổng số sinh viên đã đăng ký ít nhất 1 lần
            var registeredStudents = await _registerRoomCollection.DistinctAsync<ObjectId>(
                "IdStudent",
                FilterDefinition<RegisterRoomModels>.Empty
            );

            // Số sinh viên đã đăng ký
            var totalRegistered = (await registeredStudents.ToListAsync()).Count;

            // Tính số sinh viên chưa đăng ký
            var unregisteredStudents = totalStudents - totalRegistered;
            if (unregisteredStudents < 0) unregisteredStudents = 0;

            return Ok(new
            {
                TotalStudents = totalStudents,
                TotalRegisteredStudents = totalRegistered,
                UnregisteredStudents = unregisteredStudents // Trả về số sinh viên chưa đăng ký
            });
        }

        [Authorize(Roles = "Admin,Student")]
        [HttpGet("history/{idStudent}")]
        public async Task<IActionResult> GetRegisterRoomHistory(string idStudent)
        {
            if (!ObjectId.TryParse(idStudent, out var studentId))
                return BadRequest(new { message = "IdStudent không hợp lệ." });

            // Lọc các đăng ký phòng của sinh viên theo IdStudent
            var registerRooms = await _registerRoomCollection
                .Find(r => r.IdStudent == studentId)
                .ToListAsync();

            if (registerRooms == null || registerRooms.Count == 0)
                return NotFound(new { message = "Không có lịch sử đăng ký phòng cho sinh viên này." });

            var result = new List<object>();

            foreach (var r in registerRooms)
            {
                // Lấy thông tin sinh viên
                var student = await _infoStudents.Find(u => u.Id == r.IdStudent).FirstOrDefaultAsync();

                // Nếu có sinh viên, lấy thông tin tài khoản từ `AccountId`
                AccountModels? account = null;
                if (student?.AccountId != null)
                {
                    account = await _accounts.Find(u => u.AccountId == student.AccountId).FirstOrDefaultAsync();
                }

                // Lấy thông tin phòng
                var room = await _roomsCollection.Find(room => room.IdRoom == r.IdRoom).FirstOrDefaultAsync();

                // Lấy thông tin tòa nhà nếu có phòng
                var building = room != null
                    ? await _buildingsCollection.Find(b => b.IdBuilding == room.IdBuilding)
                                                .Project(b => new { b.NameBuilding })
                                                .FirstOrDefaultAsync()
                    : null;

                var registerRoomWithDetails = new
                {
                    IdRegister = r.IdRegister.ToString(),
                    IdStudent = r.IdStudent.ToString(),
                    StudentInfo = student != null ? new
                    {
                        Id = student.Id.ToString(),
                        student.Email,
                        student.Address,
                        student.NameParent,
                        student.ParentNumberPhone
                    } : null,
                    AccountInfo = account != null ? new
                    {
                        AccountId = account.AccountId.ToString(),
                        account.UserName,
                        account.UserCode,
                        account.NumberPhone,
                        account.Roles,
                        account.Status
                    } : null,
                    IdRoom = r.IdRoom.ToString(),
                    RoomInfo = room != null ? new
                    {
                        room.RoomName
                    } : null,
                    BuildingInfo = building != null ? new
                    {
                        building.NameBuilding
                    } : null,
                    IdRegistrationPeriod = r.IdRegistrationPeriod.ToString(),
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    ActionDate = r.ActionDate,
                    Total = r.Total,
                    PaymentStatus = r.PaymentStatus,
                    Status = r.Status
                };

                result.Add(registerRoomWithDetails);
            }

            return Ok(result);
        }

        [Authorize(Roles = "Admin,Student,Staff")]
        [HttpGet("students-in-room/{idRoom}")]
        public async Task<IActionResult> GetStudentsInRoom(string idRoom)
        {
            if (!ObjectId.TryParse(idRoom, out var roomId))
                return BadRequest(new { message = "IdRoom không hợp lệ." });

            // Tìm tất cả các đăng ký có cùng IdRoom và trạng thái là active hoặc wait
            var filter = Builders<RegisterRoomModels>.Filter.And(
                Builders<RegisterRoomModels>.Filter.Eq(r => r.IdRoom, roomId),
                Builders<RegisterRoomModels>.Filter.In(r => r.Status, new[] { OperatingStatusEnum.active, OperatingStatusEnum.wait })
            );

            var registerRooms = await _registerRoomCollection.Find(filter).ToListAsync();

            if (registerRooms.Count == 0)
                return NotFound(new { message = "Không có sinh viên nào đăng ký phòng này với trạng thái active hoặc wait." });

            var result = new List<object>();

            foreach (var register in registerRooms)
            {
                var student = await _infoStudents.Find(s => s.Id == register.IdStudent).FirstOrDefaultAsync();
                var account = student != null
                    ? await _accounts.Find(a => a.AccountId == student.AccountId).FirstOrDefaultAsync()
                    : null;

                result.Add(new
                {
                    IdRegister = register.IdRegister.ToString(),
                    IdRoom = register.IdRoom.ToString(),
                    IdStudent = register.IdStudent.ToString(),
                    StudentInfo = student != null ? new
                    {
                        Id = student.Id.ToString(),
                        student.Email,
                        student.Address,
                        student.NameParent,
                        student.Gender,
                        student.ParentNumberPhone
                    } : null,
                    AccountInfo = account != null ? new
                    {
                        AccountId = account.AccountId.ToString(),
                        account.UserName,
                        account.UserCode,
                        account.NumberPhone,
                        account.Roles,

                        account.Status
                    } : null,
                    Status = register.Status
                });
            }

            return Ok(result);
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("students-paybill/{idRoom}")]
        public async Task<IActionResult> GetStudentsPayBill(string idRoom)
        {
            if (!ObjectId.TryParse(idRoom, out var roomId))
                return BadRequest(new { message = "IdRoom không hợp lệ." });

            // Lấy danh sách đăng ký của phòng đó
            var filter = Builders<RegisterRoomModels>.Filter.And(
                Builders<RegisterRoomModels>.Filter.Eq(r => r.IdRoom, roomId),
                Builders<RegisterRoomModels>.Filter.In(r => r.Status, new[] { OperatingStatusEnum.active, OperatingStatusEnum.wait })
            );

            var registerRooms = await _registerRoomCollection.Find(filter).ToListAsync();

            var result = new List<object>();

            var registeredStudentIds = registerRooms.Select(r => r.IdStudent).ToList();

            // Lấy thông tin sinh viên đã đăng ký phòng này
            foreach (var register in registerRooms)
            {
                var student = await _infoStudents.Find(s => s.Id == register.IdStudent).FirstOrDefaultAsync();
                var account = student != null
                    ? await _accounts.Find(a => a.AccountId == student.AccountId).FirstOrDefaultAsync()
                    : null;

                result.Add(new
                {
                    IsInRoom = true,
                    IdRegister = register.IdRegister.ToString(),
                    IdRoom = register.IdRoom.ToString(),
                    IdStudent = register.IdStudent.ToString(),
                    StudentInfo = student != null ? new
                    {
                        Id = student.Id.ToString(),
                        student.Email,
                        student.Address,
                        student.NameParent,
                        student.Gender,
                        student.ParentNumberPhone
                    } : null,
                    AccountInfo = account != null ? new
                    {
                        AccountId = account.AccountId.ToString(),
                        account.UserName,
                        account.UserCode,
                        account.NumberPhone,
                        account.Roles,
                        account.Status
                    } : null,
                    Status = 0 // là sinh viên phòng đó
                });
            }
            var remainingStudentsFilter = Builders<InfoStudentModels>.Filter.Nin(s => s.Id, registeredStudentIds);
            var remainingStudents = await _infoStudents.Find(remainingStudentsFilter).ToListAsync();

            foreach (var student in remainingStudents)
            {
                var account = await _accounts.Find(a => a.AccountId == student.AccountId).FirstOrDefaultAsync();

                result.Add(new
                {
                    IsInRoom = false,
                    IdRegister = (string)null,
                    IdRoom = (string)null,
                    IdStudent = student.Id.ToString(),
                    StudentInfo = new
                    {
                        Id = student.Id.ToString(),
                        student.Email,
                        student.Address,
                        student.NameParent,
                        student.Gender,
                        student.ParentNumberPhone
                    },
                    AccountInfo = account != null ? new
                    {
                        AccountId = account.AccountId.ToString(),
                        account.UserName,
                        account.UserCode,
                        account.NumberPhone,
                        account.Roles,
                        account.Status
                    } : null,
                    Status = 1 // không phải sinh viên phòng đó
                });
            }

            return Ok(result);
        }

    }
}
