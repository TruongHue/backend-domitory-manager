using API_dormitory.Models.common;
using API_dormitory.Models.Registrations;

namespace API_dormitory.Models.DTO.RegisterRoom
{
    public class RegistrationStatusDTO
    {
        public int IdRegistrationPeriod { get; set; }
        public RegistrationStatusEnum Status { get; set; } // Mở đăng ký hoặc Đóng đăng ký

    }
}
