using API_dormitory.Models.Users;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using API_dormitory.Models.common;

namespace API_dormitory.Models.DTO.Account
{
    public class AccountDTOs
    {
        public string? IdAccount {  get; set; }
        public string? UserName { get; set; }
        public string? UserCode { get; set; }
        public string? NumberPhone { get; set; }
        public string? Password { get; set; }
        public RoleTypeStatusEnum? Roles { get; set; }
        public OperatingStatusEnum? Status { get; set; }
        //public InfoUserDTOs InfoUser { get; set; } // Chứa thông tin từ InfoUser

    }
}
