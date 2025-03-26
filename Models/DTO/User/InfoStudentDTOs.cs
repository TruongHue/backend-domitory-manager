using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using API_dormitory.Models.Users;
using API_dormitory.Models.common;

namespace API_dormitory.Models.DTO.User
{
    public class InfoStudentDTOs
    {
        public GenderEnum Gender { get; set; }
        public string? Email { get; set; }
        public string? NameParent { get; set; }
        public string? Address { get; set; }
        public string? ParentNumberPhone { get; set; }
    }
}
