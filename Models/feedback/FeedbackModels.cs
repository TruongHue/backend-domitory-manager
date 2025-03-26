using API_dormitory.Models.common;
using API_dormitory.Models.Rooms;
using API_dormitory.Models.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_dormitory.Models.feedback
{
    public class FeedbackModels
    {
        [Key]
        public int IdFeeedback { get; set; }
        [Required]
        [ForeignKey("InfoUser")]
        public int IdStudent {  get; set; }
        [Required]
        public string Description {  get; set; }
        [Required]
        public PaymentStatusEnum status { get; set; }
        public virtual InfoStudentModels InfoStudent { get; set; }

    }

}
