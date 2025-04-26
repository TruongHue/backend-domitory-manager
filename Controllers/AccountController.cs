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


        public AccountController(IConfiguration configuration, MongoDbContext context, EmailService emailService)
        {
            _configuration = configuration;
            _accounts = context.GetCollection<AccountModels>("Accounts");
            _infoStudents = context.GetCollection<InfoStudentModels>("InfoStudents");
            _emailService = emailService;

        }
        [Authorize(Roles = "Admin,Staff")] // Chỉ cho phép người có vai trò Admin và Staff
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



        [HttpGet("All-account-students")]
        public async Task<IActionResult> GetAllAccounts()
        {
            var students = await _infoStudents.Find(_ => true).ToListAsync();
            var accounts = await _accounts.Find(_ => true).ToListAsync();

            var result = students.Select(student =>
            {
                var account = accounts.FirstOrDefault(acc => acc.AccountId == student.AccountId
                                                              && acc.Roles == RoleTypeStatusEnum.Student);
                return new
                {
                    // Hiển thị thông tin tài khoản trước
                    Account = account != null ? new
                    {
                        AccountId = account.AccountId.ToString(),
                        account.UserName,
                        account.UserCode,
                        account.NumberPhone,
                        account.Roles,
                        account.Status
                    } : null,

                    // Sau đó hiển thị thông tin học sinh
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
                };
            }).ToList();

            if (!result.Any())
            {
                return NotFound(new { message = "Không có tài khoản nào" });
            }

            return Ok(result);
        }



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

        [HttpGet("account-student-code/{StudentCode}")]
        public async Task<IActionResult> GetAccountStudentByStudentCode(string studentCode)
        {
            var account = await _accounts.Find(acc => acc.UserCode == studentCode
                                                      && acc.Roles == RoleTypeStatusEnum.Student)
                                         .FirstOrDefaultAsync();

            if (account == null)
            {
                return NotFound(new { message = "Không tìm thấy tài khoản học sinh" });
            }

            var student = await _infoStudents.Find(stu => stu.AccountId.ToString() == account.AccountId.ToString()).FirstOrDefaultAsync();

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

        [HttpGet("all-account-staffs")]
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


        [HttpPost("account-staff")]
        public async Task<IActionResult> CreateStaffAccount([FromBody] AccountDTOs request)
        {
            // 🔹 Kiểm tra xem tài khoản đã tồn tại chưa (dựa trên Email hoặc Số điện thoại)
            var existingAccount = await _accounts.Find(acc => acc.NumberPhone == request.NumberPhone).FirstOrDefaultAsync();
            if (existingAccount != null)
            {
                return BadRequest(new { message = "Tài khoản đã tồn tại!" });
            }

            // 🔹 Mã hóa mật khẩu trước khi lưu (sử dụng BCrypt hoặc thư viện bảo mật)
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // 🔹 Tạo tài khoản mới
            var newAccount = new AccountModels
            {
                AccountId = ObjectId.GenerateNewId(),
                UserName = request.UserName,
                UserCode = request.UserCode,
                NumberPhone = request.NumberPhone,
                Password = hashedPassword,
                Roles = RoleTypeStatusEnum.Staff,  // Gán role Staff
                Status = OperatingStatusEnum.active // Mặc định trạng thái hoạt động
            };

            // 🔹 Thêm tài khoản vào MongoDB
            await _accounts.InsertOneAsync(newAccount);
            return Ok(new { message = "Tạo tài khoản staff thành công!" });
        }


        [HttpGet("account-staff/{idAccount}")]
        public async Task<IActionResult> GetAccountStaffById(string idAccount)
        {
            var account = await _accounts.Find(acc => acc.AccountId.ToString() == idAccount && acc.Roles == RoleTypeStatusEnum.Staff).FirstOrDefaultAsync();

            if (account == null)
            {
                return NotFound(new { message = "Không tìm thấy tài khoản nhân viên" });
            }

            var result = new
            {
                AccountId = account.AccountId.ToString(),
                account.UserName,
                account.UserCode,
                account.NumberPhone,
                account.Roles,
                account.Status
            };

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

            // 🔹 **Lưu file ảnh vào wwwroot/images/**
            string fileName = null;
            if (file != null && file.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName); // Đặt tên file ngẫu nhiên
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
            }

            // 🔹 **Tạo đối tượng InfoStudent**
            var infoUser = new InfoStudentModels
            {
                Email = registerInfoUserDTO.Email,
                Picture = fileName,
                NameParent = registerInfoUserDTO.NameParent,
                ParentNumberPhone = registerInfoUserDTO.ParentNumberPhone,
                Address = registerInfoUserDTO.Address,
                Gender = registerInfoUserDTO.Gender,
            };

            // 🔹 **Lưu thông tin sinh viên vào MongoDB**
            await _infoStudents.InsertOneAsync(infoUser);
            ObjectId infoStudentId = ObjectId.Parse(infoUser.Id.ToString());

            // 🔹 **Tạo đối tượng AccountModels**
            var account = new AccountModels
            {
                UserName = registerAccountDTO.UserName,
                UserCode = registerAccountDTO.UserCode,
                NumberPhone = registerAccountDTO.NumberPhone,
                Password = HashPassword(registerAccountDTO.Password),
                Roles = (RoleTypeStatusEnum)registerAccountDTO.Roles,
                Status = registerAccountDTO.Status ?? OperatingStatusEnum.inactive,
                InfoStudentId = infoStudentId // 🔹 Gán InfoStudentId vào AccountModels
            };

            // 🔹 **Lưu tài khoản vào MongoDB**
            await _accounts.InsertOneAsync(account);
            ObjectId accountId = ObjectId.Parse(account.AccountId.ToString());

            // 🔹 **Cập nhật AccountId vào InfoStudent**
            var update = Builders<InfoStudentModels>.Update.Set(x => x.AccountId, accountId);
            var filter = Builders<InfoStudentModels>.Filter.Eq(x => x.Id, infoStudentId);
            await _infoStudents.UpdateOneAsync(filter, update);

            return Ok(new { message = "Registration successful", imageUrl = $"/images/{fileName}" });
        }

        [HttpPost("import-excel")]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];

            var students = new List<StudentRequestDTO>();
            var duplicateStudents = new List<int>(); // Danh sách lưu trữ số thứ tự của các sinh viên bị trùng

            // Đọc từng dòng
            for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
            {
                var student = new StudentRequestDTO
                {
                    Account = new AccountDTOs
                    {
                        UserName = worksheet.Cells[row, 2].Text,
                        UserCode = worksheet.Cells[row, 3].Text,
                        NumberPhone = worksheet.Cells[row, 4].Text,
                        Password = worksheet.Cells[row, 3].Text,  // Mật khẩu là mã sinh viên
                        Roles = (int)RoleTypeStatusEnum.Student,  // Đặt trạng thái tài khoản là Student
                        Status = (int)OperatingStatusEnum.active  // Đặt trạng thái tài khoản mặc định
                    },
                    InfoStudent = new InfoStudentDTOs
                    {
                        Email = worksheet.Cells[row, 5].Text,
                        Gender = worksheet.Cells[row, 6].Text.Trim().ToLower() == "Nam"
                            ? GenderEnum.male
                            : GenderEnum.female,
                        NameParent = worksheet.Cells[row, 7].Text,
                        ParentNumberPhone = worksheet.Cells[row, 8].Text,
                        Address = worksheet.Cells[row, 9].Text,
                    }
                };

                // Kiểm tra xem số điện thoại hoặc UserName đã tồn tại trong MongoDB chưa
                var existingUser = await _accounts.Find(x => x.NumberPhone == student.Account.NumberPhone).FirstOrDefaultAsync();
                if (existingUser != null)
                {
                    // Nếu đã tồn tại, lưu số thứ tự vào danh sách duplicate
                    duplicateStudents.Add(row); // row là số thứ tự của sinh viên trong Excel (bắt đầu từ 2)
                    continue; // Bỏ qua sinh viên này
                }

                // Lấy ảnh tại vị trí tương ứng
                var pic = worksheet.Drawings.OfType<ExcelPicture>()
                           .FirstOrDefault(p => p.From.Row + 1 == row);
                if (pic != null)
                {
                    var imageBytes = pic.Image?.ImageBytes;
                    student.InfoStudent.Picture = Convert.ToBase64String(imageBytes);  // Lưu ảnh dạng base64
                }

                students.Add(student);
            }

            // Lưu vào MongoDB và tạo tài khoản sinh viên cho những sinh viên không bị trùng
            foreach (var stu in students)
            {
                await Register(stu, null);  // Đoạn này gọi lại hàm Register bạn đã có
            }

            // Kiểm tra nếu có sinh viên bị trùng
            if (duplicateStudents.Count > 0)
            {
                // In ra số lượng và số thứ tự của các sinh viên bị trùng
                return Ok(new { message = "Import thành công, nhưng có tài khoản bị trùng", count = students.Count, duplicates = duplicateStudents });
            }

            return Ok(new { message = "Import thành công!", count = students.Count });
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

        [HttpDelete("account-student/{accountId}")]
        public async Task<IActionResult> DeleteAccountStudent(string accountId)
        {
            var account = await _accounts.Find(acc => acc.AccountId.ToString() == accountId
                                                      && acc.Roles == RoleTypeStatusEnum.Student)
                                         .FirstOrDefaultAsync();

            if (account == null)
            {
                return NotFound(new { message = "Không tìm thấy tài khoản học sinh" });
            }

            // Tìm sinh viên liên kết với tài khoản
            var student = await _infoStudents.Find(stu => stu.AccountId.ToString() == accountId).FirstOrDefaultAsync();

            // Xóa tài khoản trước
            await _accounts.DeleteOneAsync(acc => acc.AccountId == account.AccountId);

            // Nếu có thông tin sinh viên, xóa luôn
            if (student != null)
            {
                await _infoStudents.DeleteOneAsync(stu => stu.AccountId == account.AccountId);
            }

            return Ok(new { message = "Xóa tài khoản sinh viên thành công!" });
        }

        [HttpPut("account/password/{accountId}")]
        public async Task<IActionResult> UpdatePasswordByAdmin(string accountId, [FromBody] updatePassword request)
        {
            // 🔹 Tìm tài khoản theo AccountId
            var account = await _accounts.Find(acc => acc.AccountId.ToString() == accountId)
                                         .FirstOrDefaultAsync();

            if (account == null)
            {
                return NotFound(new { message = "Không tìm thấy tài khoản!" });
            }

            // 🔹 Mã hóa mật khẩu mới
            string hashedPassword = HashPassword(request.Password);

            // 🔹 Cập nhật mật khẩu mới vào MongoDB
            var update = Builders<AccountModels>.Update.Set(acc => acc.Password, hashedPassword);
            await _accounts.UpdateOneAsync(acc => acc.AccountId == account.AccountId, update);

            return Ok(new { message = "Cập nhật mật khẩu thành công!" });
        }



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

        [HttpPut("account-student/full/{accountId}")]
        public async Task<IActionResult> UpdateFullStudentInfo(string accountId, [FromBody] UpdateFullStudentRequest request)
        {
            // 🔹 Tìm tài khoản dựa trên AccountId
            var account = await _accounts.Find(acc => acc.AccountId.ToString() == accountId).FirstOrDefaultAsync();
            if (account == null)
            {
                return NotFound(new { message = "Không tìm thấy tài khoản học sinh" });
            }

            // 🔹 Tìm thông tin sinh viên dựa trên AccountId
            var student = await _infoStudents.Find(stu => stu.AccountId.ToString() == accountId).FirstOrDefaultAsync();
            if (student == null)
            {
                return NotFound(new { message = "Không tìm thấy thông tin sinh viên" });
            }

            // 🔹 Cập nhật thông tin tài khoản (UserName, UserCode, Số điện thoại, Trạng thái)
            var updateAccount = Builders<AccountModels>.Update
                .Set(acc => acc.UserName, request.UserName)
                .Set(acc => acc.UserCode, request.UserCode)
                .Set(acc => acc.NumberPhone, request.NumberPhone);
            await _accounts.UpdateOneAsync(acc => acc.AccountId == account.AccountId, updateAccount);

            // 🔹 Cập nhật thông tin sinh viên (Email, Giới tính, Ảnh, Phụ huynh,...)
            var updateStudent = Builders<InfoStudentModels>.Update
                .Set(stu => stu.Email, request.Email)
                .Set(stu => stu.Gender, request.Gender)
                .Set(stu => stu.Picture, request.Picture)
                .Set(stu => stu.NameParent, request.NameParent)
                .Set(stu => stu.Address, request.Address)
                .Set(stu => stu.ParentNumberPhone, request.ParentNumberPhone);

            await _infoStudents.UpdateOneAsync(stu => stu.AccountId == student.AccountId, updateStudent);

            return Ok(new { message = "Cập nhật tài khoản và thông tin sinh viên thành công!" });
        }


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



