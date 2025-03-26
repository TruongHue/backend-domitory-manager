using API_dormitory.Models.common;
using API_dormitory.Models.common;
using API_dormitory.Models.Rooms;
using API_dormitory.Models.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_dormitory.Models.registerRoom
{
    public class RegisterRoomModels
    {
        [Key]
        public int IdRegister { get; set; }
        [Required]
        [ForeignKey("idStudent")]
        public int idStudent { get; set; }
        [Required]
        [ForeignKey("idRoom")]
        public int idRoom { get; set; }
        [Required]
        [ForeignKey("idRegistrationPeriod")]
        public int idRegistrationPeriod { get; set; }
        [Required]
        public DateTime startDate { get; set; }
        [Required]
        public DateTime endDate { get; set; }
        [Required]
        [Column("actionDate")]
        public DateTime ActionDate { get; set; }
        [Required]
        [Column("price", TypeName = "decimal(18,2)")]
        public decimal total { get; set; }
        [Required]
        public PaymentStatusEnum paymentStatus { get; set; }
        [Required] 
        public OperatingStatusEnum status { get; set; }
        public virtual InfoStudentModels InfoStudent { get; set; }
        public virtual InfoRoomModels InfoRoom { get; set; }


    }


}
