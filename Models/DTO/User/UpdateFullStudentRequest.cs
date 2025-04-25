using API_dormitory.Models.common;

namespace API_dormitory.Models.DTO.User
{
    public class UpdateFullStudentRequest
    {
        public string UserName { get; set; }
        public string UserCode { get; set; }
        public string NumberPhone { get; set; }
        public OperatingStatusEnum Status { get; set; }

        // Thông tin sinh viên
        public string Email { get; set; }
        public GenderEnum Gender { get; set; }
        public string Picture { get; set; }
        public string NameParent { get; set; }
        public string Address { get; set; }
        public string ParentNumberPhone { get; set; }
    }
}
