using API_dormitory.Data;
using API_dormitory.Models.common;
using API_dormitory.Models.DTO;
using API_dormitory.Models.DTO.Account;
using API_dormitory.Models.DTO.User;
using API_dormitory.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Net;


namespace API_dormitory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

/*        public UserController(IConfiguration configuration, AppDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }
        //Lấy toán bộ danh sách user
        //[Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Accounts
                                      .Include(x => x.InfoStudent) // Load thêm InfoUser
                                      .Select (v =>new StudentRequestDTO
                                      {
                                          Account = new AccountDTOs
                                          {
                                              IdAccount = v.IdAccount,
                                              UserCode = v.UserCode,
                                              NumberPhone = v.NumberPhone,
                                              Password = v.Password,
                                              Roles = v.Roles,
                                              Status = v.Status,
                                              UserName = v.UserName,
                                          },
                                          InfoStudent = v.InfoStudent != null ? new InfoStudentDTOs
                                          {
                                              IdStudent = v.InfoStudent.IdStudent,
                                              AccountId = v.InfoStudent.AccountId,
                                              Picture = v.InfoStudent.Picture,
                                              Email = v.InfoStudent.Email,
                                              Gender = v.InfoStudent.Gender,
                                              NameParent = v.InfoStudent.NameParent,
                                              ParentNumberPhone = v.InfoStudent.ParentNumberPhone,
                                              Address = v.InfoStudent.Address,
                                          }:null
                                      })
                                      .ToListAsync();
            
            return Ok(users);
        }*/
        //Lấy danh sách user theo ID
        //[Authorize(Roles = "Admin,Student")]
  /*      [HttpGet("by-id/{idAccount}")]
        public async Task<IActionResult> GetListIdUsers(int idAccount)
        {
            if (idAccount!= null)
            {
                var user = await _context.Accounts
                                         .Include(x => x.InfoStudent)
                                         .Where(u => u.IdAccount == idAccount)
                                         .Select(u => new StudentRequestDTO
                                         {
                                             Account = new AccountDTOs
                                             {
                                                 IdAccount = u.IdAccount,
                                                 UserCode = u.UserCode,
                                                 NumberPhone = u.NumberPhone,
                                                 Password = u.Password,
                                                 Roles = u.Roles,
                                                 Status = u.Status,
                                                 UserName = u.UserName
                                             },
                                             InfoStudent = new InfoStudentDTOs
                                             {
                                                 IdStudent = u.InfoStudent.IdStudent,
                                                 AccountId = u.InfoStudent.AccountId,
                                                 Picture = u.InfoStudent.Picture,
                                                 Gender = u.InfoStudent.Gender,
                                                 Email = u.InfoStudent.Email,
                                                 NameParent = u.InfoStudent.NameParent,
                                                 ParentNumberPhone = u.InfoStudent.ParentNumberPhone,
                                                 Address = u.InfoStudent.Address,
                                             }
                                         })
                                         .FirstOrDefaultAsync();

                if (user == null)
                    return NotFound(new { message = "Không tìm thấy người dùng" });
                if(user.Account == null)
                    return NotFound(new { message = "Người dùng không có tài khoản" });

                return Ok(user);
            }
            else
            {
                return BadRequest(new { message = "Không tìm thấy người dùng" });
            }
        }*/
        //Lấy danh sách user theo mã sinh viên
        //[Authorize(Roles = "Admin,Student")]
     /*   [HttpGet("by-studentCode/{studentCode}")]
        public async Task<IActionResult> GetListStudentCodeUsers(string? studentCode)
        {
            if (!string.IsNullOrEmpty(studentCode)) // Kiểm tra nếu studentCode có giá trị
            {
                var user = await _context.Accounts
                                         .Include(x => x.InfoStudent)
                                         .Where(u => u.UserCode == studentCode)
                                         .Select(u => new StudentRequestDTO
                                         {
                                             Account = new AccountDTOs
                                             {
                                                 IdAccount = u.IdAccount,
                                                 UserCode = u.UserCode,
                                                 NumberPhone = u.NumberPhone,
                                                 Password = u.Password,
                                                 Roles = u.Roles,
                                                 Status = u.Status,
                                             },
                                             InfoStudent= new InfoStudentDTOs
                                             {
                                                 IdStudent = u.InfoStudent.IdStudent,
                                                 AccountId = u.InfoStudent.AccountId,
                                                 Gender = u.InfoStudent.Gender,
                                                 Picture = u.InfoStudent.Picture,
                                                 Email = u.InfoStudent.Email,
                                                 NameParent = u.InfoStudent.NameParent,
                                                 ParentNumberPhone = u.InfoStudent.ParentNumberPhone,
                                                 Address = u.InfoStudent.Address,
                                             }
                                         })
                                         .FirstOrDefaultAsync();

                if (user == null)
                    return NotFound(new { message = "Không tìm thấy người dùng" });

                return Ok(user);
            }
            else
            {
                return BadRequest(new { message = "Không tìm thấy người dùng" });
            }
        }*/

        //Xóa thông tin sinh viên kể cả tài khoản đăng nhập luôn 
        //[Authorize(Roles = "Admin")]
        [HttpDelete("by-studentCode/{studentCode}")]
        public async Task<IActionResult> DeleteUsers(string studentCode)
        {
            if (string.IsNullOrEmpty(studentCode))
            {
                return BadRequest(new { message = "Mã sinh viên không hợp lệ" });
            }

            // Tìm tài khoản theo studentCode
            var user = await _context.Accounts
                                     .Include(x => x.InfoStudent)
                                     .FirstOrDefaultAsync(u => u.UserCode == studentCode);

            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng" });
            }

            // Xóa thông tin liên quan trước (nếu có)
            if (user.InfoStudent != null)
            {
                _context.InfoStudents.Remove(user.InfoStudent);
            }

            // Xóa tài khoản
            _context.Accounts.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa người dùng thành công" });
        }


        //[Authorize(Roles = "Admin,Student")]
        [HttpPut("by-studentCode/{studentCode}")]
        public async Task<IActionResult> UpdateUser(string studentCode, [FromBody] StudentRequestDTO updateUser)
        {
            if (string.IsNullOrEmpty(studentCode))
            {
                return BadRequest(new { message = "Mã sinh viên không hợp lệ" });
            }

            if (updateUser == null || updateUser.Account == null)
            {
                return BadRequest(new { message = "Dữ liệu cập nhật không hợp lệ" });
            }

            var user = await _context.Accounts
                                     .Include(x => x.InfoStudent)
                                     .FirstOrDefaultAsync(u => u.UserCode == studentCode);

            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng" });
            }

            // ✅ Cập nhật thông tin tài khoản nếu có dữ liệu mới
            if (!string.IsNullOrEmpty(updateUser.Account.NumberPhone) && updateUser.Account.NumberPhone != user.NumberPhone)
                user.NumberPhone = updateUser.Account.NumberPhone;

            if (!string.IsNullOrEmpty(updateUser.Account.Password) && updateUser.Account.Password != user.Password)
                user.Password = updateUser.Account.Password;

            if (!string.IsNullOrEmpty(updateUser.Account.UserCode) && updateUser.Account.UserCode != user.UserCode)
                user.UserCode = updateUser.Account.UserCode;

            // ✅ Cập nhật Role nếu có và hợp lệ
            if (updateUser.Account.Roles.HasValue && Enum.IsDefined(typeof(RoleTypeStatusEnum), updateUser.Account.Roles.Value))
            {
                user.Roles = updateUser.Account.Roles.Value;
            }

            // ✅ Cập nhật Status nếu có và hợp lệ
            if (updateUser.Account.Status.HasValue && Enum.IsDefined(typeof(OperatingStatusEnum), updateUser.Account.Status.Value))
            {
                user.Status = updateUser.Account.Status.Value;
            }

            // ✅ Cập nhật thông tin cá nhân nếu có dữ liệu mới
            if (user.InfoStudent != null && updateUser.InfoStudent != null)
            {
/*                if (!string.IsNullOrEmpty(updateUser.InfoStudent.Picture) && updateUser.InfoStudent.Picture != user.InfoStudent.Picture)
                    user.InfoStudent.Picture = updateUser.InfoStudent.Picture;
*/
                if (!string.IsNullOrEmpty(updateUser.InfoStudent.Email) && updateUser.InfoStudent.Email != user.InfoStudent.Email)
                    user.InfoStudent.Email = updateUser.InfoStudent.Email;
                if (!string.IsNullOrEmpty(updateUser.InfoStudent.NameParent) && updateUser.InfoStudent.NameParent != user.InfoStudent.NameParent)
                    user.InfoStudent.NameParent = updateUser.InfoStudent.NameParent;
                if (!string.IsNullOrEmpty(updateUser.InfoStudent.ParentNumberPhone) && updateUser.InfoStudent.ParentNumberPhone != user.InfoStudent.ParentNumberPhone)
                    user.InfoStudent.ParentNumberPhone = updateUser.InfoStudent.ParentNumberPhone;
                if (!string.IsNullOrEmpty(updateUser.InfoStudent.Address) && updateUser.InfoStudent.Address != user.InfoStudent.Address)
                    user.InfoStudent.Address = updateUser.InfoStudent.Address;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật thông tin người dùng thành công" });
        }



    }
}
