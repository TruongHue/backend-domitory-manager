using API_dormitory.Models.registerRoom;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using API_dormitory.Models.Users;
using API_dormitory.Models.common;

namespace API_dormitory.Models.DTO.RegisterRoom
{
    public class AddRegisterRoomDTOs
    {
        public string idUser { get; set; }
        public string idRoom { get; set; }
        public string idRegistrationPeriod { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public DateTime actionDate { get; set; }

        public decimal total { get; set; }
        public PaymentStatusEnum paymentStatus { get; set; }
        public OperatingStatusEnum status { get; set; }
    }
}
