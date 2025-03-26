using API_dormitory.Models.common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_dormitory.Models.Registrations
{
    public class RegistrationPeriodModels
    {
        [Key]
        [Column("idRegistrationPeriod")]
        public int IdRegistrationPeriod { get; set; }



        [Required]
        [Column("actionDate")]
        public DateTime ActionDate { get; set; } // Ngày thực hiện thao tác

        [Required]
        [Column("startDate")]
        public DateTime StartDate { get; set; } // Ngày bắt đầu đăng ký

        [Required]
        [Column("endDate")]
        public DateTime EndDate { get; set; } // Ngày kết thúc đăng ký

        // Thêm trạng thái học kỳ
        [Required]
        [Column("semesterStatus")]
        public SemesterStatusEnum SemesterStatus { get; set; }

        [Required]
        [Column("status")]
        public RegistrationStatusEnum Status { get; set; } // Mở đăng ký hoặc Đóng đăng ký
    }


}
