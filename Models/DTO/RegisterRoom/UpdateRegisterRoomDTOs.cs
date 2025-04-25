using API_dormitory.Models.common;
using API_dormitory.Models.registerRoom;

namespace API_dormitory.Models.DTO.RegisterRoom
{
    public class UpdateRegisterRoomDTO
    {
        public string IdStudent { get; set; }
        public string IdRoom { get; set; }
        public string IdRegistrationPeriod { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime ActionDate { get; set; }
        public double Total { get; set; }
        public PaymentStatusEnum PaymentStatus { get; set; }
        public OperatingStatusEnum Status { get; set; }
    }

}
