using API_dormitory.Data;
using API_dormitory.Models.common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Threading.Tasks;
using System.Linq;
using API_dormitory.Models.Users;
using System.IO;
using API_dormitory.Config;
using System.Text;
using System.Security.Cryptography;
using MongoDB.Bson;
using API_dormitory.Models.DTO.Account;
using MongoDB.Bson.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CloudinaryDotNet;
using API_dormitory.Models.DTO.User;
using OfficeOpenXml.Drawing;
using OfficeOpenXml;
using Microsoft.AspNetCore.Authorization;
using API_dormitory.Services;
using static System.Net.WebRequestMethods;


namespace API_dormitory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IMongoCollection<AccountModels> _accounts;
        private readonly IMongoCollection<InfoStudentModels> _infoStudents;
        private readonly EmailService _emailService;
        private readonly CloudinaryService _cloudinaryService;


        public AccountController(IConfiguration configuration, MongoDbContext context, EmailService emailService, CloudinaryService cloudinaryService)
        {
            _configuration = configuration;
            _accounts = context.GetCollection<AccountModels>("Accounts");
            _infoStudents = context.GetCollection<InfoStudentModels>("InfoStudents");
            _emailService = emailService;
            _cloudinaryService = cloudinaryService; // ✅ Gán CloudinaryService
        }

        //Có dùng
        [Authorize(Roles = "Admin")] // Chỉ cho phép người có vai trò Admin và Staff
        [HttpGet("active-account-students")]
        public async Task<IActionResult> GetActiveAccounts()
        {
            var students = await _infoStudents.Find(_ => true).ToListAsync();
            var accounts = await _accounts.Find(_ => true).ToListAsync();

            var result = students.Select(student =>
            {
                var account = accounts.FirstOrDefault(acc => acc.AccountId == student.AccountId
                                                              && acc.Status == OperatingStatusEnum.active
                                                              && acc.Roles == RoleTypeStatusEnum.Student);
                return account != null ? new
                {
                    // Hiển thị thông tin tài khoản trước
                    Account = new
                    {
                        AccountId = account.AccountId.ToString(),
                        account.UserName,
                        account.UserCode,
                        account.NumberPhone,
                        account.Roles,
                        account.Status
                    },

                    // Sau đó hiển thị thông tin sinh viên
                    InfoStudent = new
                    {
                        Id = student.Id.ToString(),
                        student.Email,
                        student.Gender,
                        student.Picture,
                        student.NameParent,
                        student.Address,
                        student.ParentNumberPhone
                    }
                } : null;
            })
            .Where(result => result != null) // Loại bỏ student không có tài khoản hợp lệ
            .ToList();

            if (!result.Any())
            {
                return NotFound(new { message = "Không có tài khoản nào đang hoạt động với vai trò sinh viên!" });
            }

            return Ok(result);
        }

        //Có dùng
        [Authorize(Roles = "Admin")]
        [HttpGet("inactive-account-students")]
        public async Task<IActionResult> GetInActiveAccounts()
        {
            var students = await _infoStudents.Find(_ => true).ToListAsync();
            var accounts = await _accounts.Find(_ => true).ToListAsync();

            var result = students.Select(student =>
            {
                var account = accounts.FirstOrDefault(acc => acc.AccountId == student.AccountId
                                                              && acc.Status == OperatingStatusEnum.inactive
                                                              && acc.Roles == RoleTypeStatusEnum.Student);
                return account != null ? new
                {
                    // Hiển thị thông tin tài khoản trước
                    Account = new
                    {
                        AccountId = account.AccountId.ToString(),
                        account.UserName,
                        account.UserCode,
                        account.NumberPhone,
                        account.Roles,
                        account.Status
                    },

                    // Sau đó hiển thị thông tin sinh viên
                    InfoStudent = new
                    {
                        Id = student.Id.ToString(),
                        student.Email,
                        student.Gender,
                        student.Picture,
                        student.NameParent,
                        student.Address,
                        student.ParentNumberPhone
                    }
                } : null;
            })
            .Where(result => result != null) // Loại bỏ student không có tài khoản hợp lệ
            .ToList();

            if (!result.Any())
            {
                return NotFound(new { message = "Không có tài khoản nào không hoạt động với vai trò sinh viên!" });
            }

            return Ok(result);
        }

        //Có dùng
        [Authorize(Roles = "Admin")]
        [HttpGet("blocked-account-students")]
        public async Task<IActionResult> GetBlockedAccounts()
        {
            var students = await _infoStudents.Find(_ => true).ToListAsync();
            var accounts = await _accounts.Find(_ => true).ToListAsync();

            var result = students.Select(student =>
            {
                var account = accounts.FirstOrDefault(acc => acc.AccountId == student.AccountId
                                                              && acc.Status == OperatingStatusEnum.blocked
                                                              && acc.Roles == RoleTypeStatusEnum.Student);
                return account != null ? new
                {
                    // Hiển thị thông tin tài khoản trước
                    Account = new
                    {
                        AccountId = account.AccountId.ToString(),
                        account.UserName,
                        account.UserCode,
                        account.NumberPhone,
                        account.Roles,
                        account.Status
                    },

                    // Sau đó hiển thị thông tin sinh viên
                    InfoStudent = new
                    {
                        Id = student.Id.ToString(),
                        student.Email,
                        student.Gender,
                        student.Picture,
                        student.NameParent,
                        student.Address,
                        student.ParentNumberPhone
                    }
                } : null;
            })
            .Where(result => result != null) // Loại bỏ student không có tài khoản hợp lệ
            .ToList();

            if (!result.Any())
            {
                return NotFound(new { message = "Không có tài khoản nào bị chặn với vai trò sinh viên!" });
            }

            return Ok(result);
        }

        //Có dùng
        [Authorize(Roles = "Admin")]
        [HttpGet("wait-account-students")]
        public async Task<IActionResult> GetWaitAccounts()
        {
            var students = await _infoStudents.Find(_ => true).ToListAsync();
            var accounts = await _accounts.Find(_ => true).ToListAsync();

            var result = students.Select(student =>
            {
                var account = accounts.FirstOrDefault(acc => acc.AccountId == student.AccountId
                                                              && acc.Status == OperatingStatusEnum.wait
                                                              && acc.Roles == RoleTypeStatusEnum.Student);
                return account != null ? new
                {
                    // Hiển thị thông tin tài khoản trước
                    Account = new
                    {
                        AccountId = account.AccountId.ToString(),
                        account.UserName,
                        account.UserCode,
                        account.NumberPhone,
                        account.Roles,
                        account.Status
                    },

                    // Sau đó hiển thị thông tin sinh viên
                    InfoStudent = new
                    {
                        Id = student.Id.ToString(),
                        student.Email,
                        student.Gender,
                        student.Picture,
                        student.NameParent,
                        student.Address,
                        student.ParentNumberPhone
                    }
                } : null;
            })
            .Where(result => result != null) // Loại bỏ student không có tài khoản hợp lệ
            .ToList();

            if (!result.Any())
            {
                return NotFound(new { message = "Không có tài khoản nào đang đợi với vai trò sinh viên!" });
            }

            return Ok(result);
        }


        //Có dùng
        [HttpGet("account-student-id/{accountId}")]
        public async Task<IActionResult> GetAccountStudentByAccountId(string accountId)
        {
            var account = await _accounts.Find(acc => acc.AccountId.ToString() == accountId
                                                      && acc.Roles == RoleTypeStatusEnum.Student)
                                         .FirstOrDefaultAsync();

            if (account == null)
            {
                return NotFound(new { message = "Không tìm thấy tài khoản học sinh" });
            }

            var student = await _infoStudents.Find(stu => stu.AccountId.ToString() == accountId).FirstOrDefaultAsync();

            var result = new
            {
                // Hiển thị thông tin tài khoản trước
                Account = new
                {
                    AccountId = account.AccountId.ToString(),
                    account.UserName,
                    account.UserCode,
                    account.NumberPhone,
                    account.Roles,
                    account.Status
                },

                // Sau đó hiển thị thông tin học sinh
                InfoStudent = new
                {
                    Id = student?.Id.ToString(),
                    student?.Email,
                    student?.Gender,
                    student?.Picture,
                    student?.NameParent,
                    student?.Address,
                    student?.ParentNumberPhone
                }
            };

            return Ok(result);
        }

        

        [Authorize(Roles = "Admin")]
        [HttpGet("All-account-staffs")]
        public async Task<IActionResult> GetAllAccountStaffs()
        {
            var accounts = await _accounts.Find(acc => acc.Roles == RoleTypeStatusEnum.Staff).ToListAsync();

            var result = accounts.Select(acc => new
            {
                AccountId = acc.AccountId.ToString(),
                acc.UserName,
                acc.UserCode,
                acc.NumberPhone,
                acc.Roles,
                acc.Status
            }).ToList();

            if (!result.Any())
            {
                return NotFound(new { message = "Không có tài khoản nhân viên nào" });
            }

            return Ok(result);
        }



        // 🔹 API đăng ký tài khoản sinh viên
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] StudentRequestDTO registerRequest, IFormFile? file)
        {
            if (registerRequest == null || registerRequest.Account == null || registerRequest.InfoStudent == null)
            {
                return BadRequest("Invalid input");
            }

            var registerAccountDTO = registerRequest.Account;
            var registerInfoUserDTO = registerRequest.InfoStudent;

            if (string.IsNullOrEmpty(registerAccountDTO.Password) || string.IsNullOrEmpty(registerAccountDTO.NumberPhone))
            {
                return BadRequest(new { message = "Password and NumberPhone are required" });
            }

            // 🔹 Kiểm tra nếu số điện thoại đã tồn tại trong MongoDB
            var existingUser = await _accounts.Find(x => x.NumberPhone == registerAccountDTO.NumberPhone).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                return BadRequest(new { message = "Số điện thoại đã được đăng ký!" });
            }

            // 🔹 Upload ảnh lên Cloudinary (nếu có)
            string uploadedImageUrl = null;
            if (file != null && file.Length > 0)
            {
                uploadedImageUrl = await _cloudinaryService.UploadImageAsync(file); // Gọi service đã cấu hình
            }

            // 🔹 Tạo đối tượng InfoStudent
            var infoUser = new InfoStudentModels
            {
                Email = registerInfoUserDTO.Email,
                Picture = uploadedImageUrl, // ✅ Dùng link ảnh từ Cloudinary
                NameParent = registerInfoUserDTO.NameParent,
                ParentNumberPhone = registerInfoUserDTO.ParentNumberPhone,
                Address = registerInfoUserDTO.Address,
                Gender = registerInfoUserDTO.Gender,
            };

            await _infoStudents.InsertOneAsync(infoUser);
            ObjectId infoStudentId = ObjectId.Parse(infoUser.Id.ToString());

            var account = new AccountModels
            {
                UserName = registerAccountDTO.UserName,
                UserCode = registerAccountDTO.UserCode,
                NumberPhone = registerAccountDTO.NumberPhone,
                Password = HashPassword(registerAccountDTO.Password),
                Roles = (RoleTypeStatusEnum)registerAccountDTO.Roles,
                Status = registerAccountDTO.Status ?? OperatingStatusEnum.inactive,
                InfoStudentId = infoStudentId
            };
            //
            await _accounts.InsertOneAsync(account);
            ObjectId accountId = ObjectId.Parse(account.AccountId.ToString());

            var update = Builders<InfoStudentModels>.Update.Set(x => x.AccountId, accountId);
            var filter = Builders<InfoStudentModels>.Filter.Eq(x => x.Id, infoStudentId);
            await _infoStudents.UpdateOneAsync(filter, update);
            // Gửi email thông báo chờ xét duyệt
            await _emailService.SendEmailAsync(
                registerInfoUserDTO.Email,
                registerAccountDTO.UserName ?? "Sinh viên", // fallback nếu tên null
                "Thông báo chờ duyệt đăng ký phòng",
                $@"<p>Xin chào <strong>{registerAccountDTO.UserName}</strong>,</p>

                <p>Hệ thống đã ghi nhận thông tin đăng ký của bạn.</p>

                <p>Hiện tại, đăng ký đang <strong>chờ xét duyệt</strong>. Vui lòng theo dõi email để nhận kết quả sớm nhất.</p>

                <p>Trân trọng,<br/>Ban Quản lý Ký túc xá</p>"
            );

            return Ok(new { message = "Registration successful", imageUrl = uploadedImageUrl });
        }


        [Authorize(Roles = "Admin")]
        [HttpPost("import-excel-with-images")]
        public async Task<IActionResult> ImportFromExcelWithImages(IFormFile excelFile, IFormFileCollection imageFiles)
        {
            if (excelFile == null || excelFile.Length == 0)
                return BadRequest("No Excel file uploaded");

            using var stream = new MemoryStream();
            await excelFile.CopyToAsync(stream);
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];

            var students = new List<StudentRequestDTO>();
            var duplicateStudents = new List<(int Row, StudentRequestDTO Student)>();

            // ✅ Upload toàn bộ ảnh, không cần map gì cả
            foreach (var imageFile in imageFiles)
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(imageFile.FileName);
                await _cloudinaryService.UploadImageAsync(imageFile, fileNameWithoutExtension);
            }

            for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
            {
                var pictureFileName = worksheet.Cells[row, 10].Text?.Trim();
                var uploadedPictureUrl = $"https://res.cloudinary.com/dx39lepss/image/upload/v1746577647/student-images/{pictureFileName}";

                var student = new StudentRequestDTO
                {
                    Account = new AccountDTOs
                    {
                        UserName = worksheet.Cells[row, 1].Text,
                        UserCode = worksheet.Cells[row, 2].Text,
                        NumberPhone = worksheet.Cells[row, 3].Text,
                        Password = worksheet.Cells[row, 4].Text,
                        Roles = (int)RoleTypeStatusEnum.Student,
                        Status = (int)OperatingStatusEnum.active
                    },
                    InfoStudent = new InfoStudentDTOs
                    {
                        Email = worksheet.Cells[row, 5].Text,
                        Gender = worksheet.Cells[row, 6].Text.Trim().ToLower() == "nam"
                            ? GenderEnum.male
                            : GenderEnum.female,
                        NameParent = worksheet.Cells[row, 7].Text,
                        ParentNumberPhone = worksheet.Cells[row, 8].Text,
                        Address = worksheet.Cells[row, 9].Text,
                        Picture = uploadedPictureUrl
                    }


                };
            Console.WriteLine($"Student Picture: {student.InfoStudent.Picture}");

            var existingUser = await _accounts.Find(x =>
                    x.NumberPhone == student.Account.NumberPhone || x.UserName == student.Account.UserName
                ).FirstOrDefaultAsync();

                if (existingUser != null)
                {
                    duplicateStudents.Add((row, student));
                    continue;
                }

                students.Add(student);
            }

            foreach (var stu in students)
            {
                await AddStudentToDatabase(stu);
            }

            if (duplicateStudents.Any())
            {
                using var resultPackage = new ExcelPackage();
                var resultSheet = resultPackage.Workbook.Worksheets.Add("DuplicateStudents");

                resultSheet.Cells[1, 1].Value = "UserName";
                resultSheet.Cells[1, 2].Value = "UserCode";
                resultSheet.Cells[1, 3].Value = "NumberPhone";
                resultSheet.Cells[1, 4].Value = "Password";
                resultSheet.Cells[1, 5].Value = "Email";
                resultSheet.Cells[1, 6].Value = "Gender";
                resultSheet.Cells[1, 7].Value = "NameParent";
                resultSheet.Cells[1, 8].Value = "ParentNumberPhone";
                resultSheet.Cells[1, 9].Value = "Address";
                resultSheet.Cells[1, 10].Value = "Picture";

                int r = 2;
                foreach (var dup in duplicateStudents)
                {
                    var s = dup.Student;
                    resultSheet.Cells[r, 1].Value = s.Account.UserName;
                    resultSheet.Cells[r, 2].Value = s.Account.UserCode;
                    resultSheet.Cells[r, 3].Value = s.Account.NumberPhone;
                    resultSheet.Cells[r, 4].Value = s.Account.Password;
                    resultSheet.Cells[r, 5].Value = s.InfoStudent.Email;
                    resultSheet.Cells[r, 6].Value = s.InfoStudent.Gender.ToString();
                    resultSheet.Cells[r, 7].Value = s.InfoStudent.NameParent;
                    resultSheet.Cells[r, 8].Value = s.InfoStudent.ParentNumberPhone;
                    resultSheet.Cells[r, 9].Value = s.InfoStudent.Address;
                    resultSheet.Cells[r, 10].Value = s.InfoStudent.Picture;
                    r++;
                }

                var resultStream = new MemoryStream();
                resultPackage.SaveAs(resultStream);
                resultStream.Position = 0;

                return File(resultStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DuplicateStudents.xlsx");
            }

            return Ok(new { message = "Import thành công!", count = students.Count });
        }


        private async Task AddStudentToDatabase(StudentRequestDTO student)
        {
            // Tạo đối tượng InfoStudentDTO từ dữ liệu sinh viên
            var infoStudent = new InfoStudentModels
            {
                Email = student.InfoStudent.Email,
                Gender = student.InfoStudent.Gender,
                NameParent = student.InfoStudent.NameParent,
                ParentNumberPhone = student.InfoStudent.ParentNumberPhone,
                Address = student.InfoStudent.Address,
                Picture = student.InfoStudent.Picture // Lưu link ảnh nếu có
            };

            //} Thêm thông tin sinh viên vào cơ sở dữ liệu
            await _infoStudents.InsertOneAsync(infoStudent);
            ObjectId infoStudentId = ObjectId.Parse(infoStudent.Id.ToString()); // Lấy ID của sinh viên sau khi insert

            // Tạo đối tượng AccountDTO từ dữ liệu tài khoản sinh viên
            var account = new AccountModels
            {
                UserName = student.Account.UserName,
                UserCode = student.Account.UserCode,
                NumberPhone = student.Account.NumberPhone,
                Password = HashPassword(student.Account.Password), // Đảm bảo mật khẩu được mã hóa
                Roles = (RoleTypeStatusEnum)student.Account.Roles,
                Status = student.Account.Status ?? OperatingStatusEnum.inactive,
                InfoStudentId = infoStudentId // Liên kết tài khoản với thông tin sinh viên
            };

            // Thêm thông tin tài khoản vào cơ sở dữ liệu
            await _accounts.InsertOneAsync(account);
            ObjectId accountId = ObjectId.Parse(account.AccountId.ToString()); // Lấy ID tài khoản sau khi insert

            // Cập nhật lại thông tin tài khoản cho sinh viên trong bảng InfoStudent
            var update = Builders<InfoStudentModels>.Update.Set(x => x.AccountId, accountId);
            var filter = Builders<InfoStudentModels>.Filter.Eq(x => x.Id, infoStudentId);
            await _infoStudents.UpdateOneAsync(filter, update);

        }

        //Đăng nhập
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequestDTO loginDTO)
        {
            if (loginDTO == null || string.IsNullOrEmpty(loginDTO.UserCode) || string.IsNullOrEmpty(loginDTO.Password))
                return BadRequest(new { message = "Tên đăng nhập và mật khẩu không được để trống!" });

            var user = _accounts.Find(x => x.UserCode == loginDTO.UserCode).FirstOrDefault();
            if (user == null || !VerifyPassword(loginDTO.Password, user.Password))
                return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không đúng!" });
            if (user == null)
            {
                return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không đúng!" });
            }

            // Kiểm tra trạng thái tài khoản
            if (user.Status != OperatingStatusEnum.active) // Giả sử Active là giá trị hợp lệ
            {
                return Unauthorized(new { message = "Tài khoản chưa được kích hoạt. Vui lòng liên hệ quản trị viên!" });
            }

            // Tạo token JWT
            var token = GenerateJwtToken(user);

            return Ok(new
            {
                role = Enum.GetName(typeof(RoleTypeStatusEnum), user.Roles),  // Chuyển role từ số sang tên
                idAccount = user.AccountId.ToString(),
                token
            });
        }

        // Mã hóa mật khẩu
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        // Kiểm tra mật khẩu
        private bool VerifyPassword(string enteredPassword, string storedPassword)
        {
            var hashedEnteredPassword = HashPassword(enteredPassword);
            return hashedEnteredPassword == storedPassword;
        }

        private string GenerateJwtToken(AccountModels user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]); // Lấy khóa bí mật từ appsettings.json

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
            new Claim(ClaimTypes.NameIdentifier, user.AccountId.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, Enum.GetName(typeof(RoleTypeStatusEnum), user.Roles))
                }),
                Expires = DateTime.UtcNow.AddHours(6), // Token có thời hạn 6 giờ
                Issuer = _configuration["Jwt:Issuer"],     // ✅ thêm dòng này
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

      

        [Authorize(Roles = "Admin")]
        [HttpDelete("account-staff/{accountId}")]
        public async Task<IActionResult> DeleteAccountStaff(string accountId)
        {
            var account = await _accounts.Find(acc => acc.AccountId.ToString() == accountId
                                                      && acc.Roles == RoleTypeStatusEnum.Staff)
                                         .FirstOrDefaultAsync();

            if (account == null)
            {
                return NotFound(new { message = "Không tìm thấy tài khoản học sinh" });
            }
            // Xóa tài khoản trước
            await _accounts.DeleteOneAsync(acc => acc.AccountId == account.AccountId);
            return Ok(new { message = "Xóa tài khoản sinh viên thành công!" });
        }

     
        [Authorize(Roles = "Admin")]
        [HttpPut("account-student/status/{accountId}")]
        public async Task<IActionResult> UpdateAccountStatus(string accountId, [FromBody] UpdateStatusRequest request)
        {
            var account = await _accounts.Find(acc => acc.AccountId.ToString() == accountId).FirstOrDefaultAsync();
            var infoStudent = await _infoStudents.Find(info => info.AccountId.ToString() == account.AccountId.ToString()).FirstOrDefaultAsync();

            if (account == null)
            {
                return NotFound(new { message = "Không tìm thấy tài khoản học sinh" });
            }

            // Cập nhật trạng thái tài khoản
            var update = Builders<AccountModels>.Update.Set(acc => acc.Status, request.Status);
            await _accounts.UpdateOneAsync(acc => acc.AccountId.ToString() == account.AccountId.ToString(), update);

            // ✅ Chỉ gửi email khi trạng thái là "active"
            if (request.Status == OperatingStatusEnum.active && infoStudent.Email != null)
            {
                try
                {
                    await _emailService.SendEmailAsync(
                        infoStudent.Email,
                        account.UserName ?? "Bạn",
                        "Tài khoản của bạn đã được kích hoạt",
                        $"<p>Xin chào <strong>{account.UserName}</strong>,</p><p>Tài khoản của bạn hiện đã được <strong>kích hoạt</strong>. Bạn có thể đăng nhập và sử dụng hệ thống.</p><p>Trân trọng.</p>"
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine("❌ Lỗi khi gửi email: " + ex.Message);
                }
            }

            return Ok(new { message = $"Cập nhật trạng thái tài khoản thành công! Trạng thái mới: {request.Status}" });
        }



        [Authorize(Roles = "Admin")]
        [HttpGet("All-account-staff")]
        public async Task<IActionResult> GetAllStaffs()
        {
            // Lọc danh sách tài khoản có role là 2 (RoleTypeStatusEnum.Student)
            var accounts = await _accounts
                .Find(acc => acc.Roles == RoleTypeStatusEnum.Staff)
                .ToListAsync();

            if (!accounts.Any())
            {
                return NotFound(new { message = "Không có tài khoản nào có vai trò là Student." });
            }

            // Lấy thông tin chi tiết của học sinh từ danh sách tài khoản
            var result = accounts.Select(account => new
            {
                Account = new
                {
                    AccountId = account.AccountId.ToString(),
                    account.UserName,
                    account.UserCode,
                    account.NumberPhone,
                    account.Roles,
                    account.Status
                }
            }).ToList();

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("add-account")]
        public async Task<IActionResult> AddAccount([FromBody] AccountDTOs newAccount)
        {
            if (newAccount == null)
            {
                return BadRequest(new { message = "Dữ liệu không hợp lệ!" });
            }

            // Kiểm tra nếu số điện thoại đã tồn tại trong MongoDB
            var existingAccount = await _accounts.Find(x => x.NumberPhone == newAccount.NumberPhone).FirstOrDefaultAsync();
            if (existingAccount != null)
            {
                return BadRequest(new { message = "Số điện thoại đã được đăng ký!" });
            }

           

            // 🔹 **Tạo đối tượng AccountModels**
            var account = new AccountModels
            {
                UserName = newAccount.UserName,
                UserCode = newAccount.UserCode,
                NumberPhone = newAccount.NumberPhone,
                Password = HashPassword(newAccount.Password),
                Roles = RoleTypeStatusEnum.Staff,
                Status = OperatingStatusEnum.active
            };

            // 🔹 **Lưu tài khoản vào MongoDB**
            await _accounts.InsertOneAsync(account);

            return Ok(new { message = "Thêm tài khoản thành công!"});
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete-account/{accountId}")]
        public async Task<IActionResult> DeleteAccount(string accountId)
        {
            if (!ObjectId.TryParse(accountId, out ObjectId objectId))
            {
                return BadRequest(new { message = "AccountId không hợp lệ!" });
            }
             
            var result = await _accounts.DeleteOneAsync(acc => acc.AccountId == objectId);

            if (result.DeletedCount == 0)
            {
                return NotFound(new { message = "Không tìm thấy tài khoản để xóa!" });
            }

            return Ok(new { message = "Xóa tài khoản thành công!" });
        }

    }

}



