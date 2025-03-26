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


namespace API_dormitory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IMongoCollection<AccountModels> _accounts;
        private readonly IMongoCollection<InfoStudentModels> _infoStudents;

        public AccountController(IConfiguration configuration, MongoDbContext context)
        {
            _configuration = configuration;
            _accounts = context.GetCollection<AccountModels>("Accounts");
            _infoStudents = context.GetCollection<InfoStudentModels>("InfoStudents");
        }

        [HttpGet("active-accounts")]
        public async Task<IActionResult> GetActiveAccounts()
        {
            var students = await _infoStudents.Find(_ => true).ToListAsync();
            var accounts = await _accounts.Find(_ => true).ToListAsync();

            var result = students.Select(student => new
            {
                Id = student.Id.ToString(), // Chuyển ObjectId thành string
                student.Email,
                student.Gender,
                student.Picture,
                student.NameParent,
                student.Address,
                student.ParentNumberPhone,

                // Chỉ lấy các trường cần thiết từ Account
                Account = accounts.Where(acc => acc.AccountId == student.AccountId)
                                  .Select(acc => new
                                  {
                                      acc.UserName,
                                      acc.UserCode,
                                      acc.NumberPhone,
                                      acc.Roles,
                                      acc.Status
                                  })
                                  .FirstOrDefault() // Lấy 1 tài khoản duy nhất
            }).ToList();

            if (!result.Any())
            {
                return NotFound(new { message = "Không có tài khoản nào đang hoạt động với vai trò sinh viên!" });
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
            ObjectId infoStudentId =ObjectId.Parse(infoUser.Id.ToString());

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
                role = user.Roles,
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
            new Claim(ClaimTypes.Role, user.Roles.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(6), // Token có thời hạn 6 giờ
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }




    }
}



/*        [HttpGet("inactive-accounts")]
        public async Task<IActionResult> GetInactiveAccounts()
        {
            var inactiveAccounts = await _context.Accounts
                .Where(a => a.Status == OperatingStatusEnum.inactive && a.Roles == RoleTypeStatusEnum.Student) // Lọc Role = 1
                .Include(a => a.InfoStudent) // Lấy thêm thông tin InfoUser
                .ToListAsync();

            if (!inactiveAccounts.Any())
            {
                return NotFound(new { message = "Không có tài khoản nào chưa kích hoạt với vai trò nhân viên!" });
            }

            return Ok(inactiveAccounts);
        }

        [HttpGet("blocked-accounts")]
        public async Task<IActionResult> GetBlockedAccounts()
        {
            var activeAccounts = await _context.Accounts
                .Where(a => a.Status == OperatingStatusEnum.blocked && a.Roles == RoleTypeStatusEnum.Student) // Lọc Role = 1
                .Include(a => a.InfoStudent) // Lấy thêm thông tin InfoUser
                .ToListAsync();

            if (!activeAccounts.Any())
            {
                return NotFound(new { message = "Không có tài khoản nào đang hoạt động với vai trò nhân viên!" });
            }

            return Ok(activeAccounts);
        }
*/
/*
        [HttpPost("register/staff")]
        public async Task<IActionResult> RegisterStaff([FromBody] AccountDTO registerStaff)
        {
            if (registerStaff== null)
            {
                return BadRequest("Invalid input");
            }

            if (string.IsNullOrEmpty(registerStaff.Password) || string.IsNullOrEmpty(registerStaff.NumberPhone))
            {
                return BadRequest(new { message = "Password and NumberPhone are required" });
            }

            // Kiểm tra nếu số điện thoại đã tồn tại
            var existingUser = await _context.Accounts.FirstOrDefaultAsync(x => x.NumberPhone == registerStaff.NumberPhone);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Số điện thoại đã được đăng ký!" });
            }
            // Tạo đối tượng AccountModels và gán thông tin người dùng
            var account = new AccountDTO
            {
                UserName = registerStaff.UserName,
                UserCode = registerStaff.UserCode,
                NumberPhone = registerStaff.NumberPhone,
                Password = HashPassword(registerStaff.Password),
                Roles = registerStaff.Roles,
                Status = registerStaff.Status,
            };

            // Thêm người dùng vào bảng AccountModels
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registration successful" });
        }


        // Đăng nhập và tạo JWT token
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequestDTO loginDTO)
        {
            if (loginDTO == null || string.IsNullOrEmpty(loginDTO.UserCode) || string.IsNullOrEmpty(loginDTO.Password))
                return BadRequest(new { message = "Tên đăng nhập và mật khẩu không được để trống!" });

            var user = _context.Accounts.FirstOrDefault(x => x.UserCode == loginDTO.UserCode);
            if (user == null || !VerifyPassword(loginDTO.Password, user.Password) )
                return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không đúng!" });

            // Kiểm tra trạng thái tài khoản
            if (user.Status != OperatingStatusEnum.active) // Giả sử Active là giá trị hợp lệ
            {
                return Unauthorized(new { message = "Tài khoản chưa được kích hoạt. Vui lòng liên hệ quản trị viên!" });
            }

            // Tạo token JWT
            var token = GenerateJwtToken(user);

            return Ok(new
            {
                role = user.Roles,
                idAccount = user.IdAccount,
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

        

        [HttpPut("update-status-active/{id}")]
        public async Task<IActionResult> UpdateAccountStatusActive(int id)
        {
            // Tìm tài khoản theo ID
            var account = await _context.Accounts.FindAsync(id);

            if (account == null)
            {
                return NotFound(new { message = "Tài khoản không tồn tại!" });
            }

            // Kiểm tra nếu tài khoản đã active
            if (account.Status == OperatingStatusEnum.active)
            {
                return BadRequest(new { message = "Tài khoản đã được kích hoạt trước đó!" });
            }

            // Cập nhật trạng thái thành active
            account.Status = OperatingStatusEnum.active;
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật trạng thái tài khoản thành công!", account });
        }

        [HttpPut("update-status-blocked/{id}")]
        public async Task<IActionResult> UpdateAccountStatusBlocked(int id)
        {
            // Tìm tài khoản theo ID
            var account = await _context.Accounts.FindAsync(id);

            if (account == null)
            {
                return NotFound(new { message = "Tài khoản không tồn tại!" });
            }

            // Kiểm tra nếu tài khoản đã active
            if (account.Status == OperatingStatusEnum.blocked)
            {
                return BadRequest(new { message = "Tài khoản đã bị chặn trước đó!" });
            }

            // Cập nhật trạng thái thành active
            account.Status = OperatingStatusEnum.blocked;
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật trạng thái tài khoản thành công!", account });
        }

        [HttpPut("update-status-inactive/{id}")]
        public async Task<IActionResult> UpdateAccountStatusInactive(int id)
        {
            // Tìm tài khoản theo ID
            var account = await _context.Accounts.FindAsync(id);

            if (account == null)
            {
                return NotFound(new { message = "Tài khoản không tồn tại!" });
            }

            // Kiểm tra nếu tài khoản đã active
            if (account.Status == OperatingStatusEnum.inactive)
            {
                return BadRequest(new { message = "Tài khoản đã ngưng hoạt động trước đó!" });
            }

            // Cập nhật trạng thái thành active
            account.Status = OperatingStatusEnum.inactive;
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật trạng thái tài khoản thành công!", account });
        }*/
/*    }
}
*/