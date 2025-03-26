using API_dormitory.Models.common;
using API_dormitory.Models.Rooms;

namespace API_dormitory.Models.DTO.Room
{
    public class UpdateStatusRoomDTO
    {
        public int IdRoom { get; set; }
        public OperatingStatusEnum? Status { get; set; }
    }
}
