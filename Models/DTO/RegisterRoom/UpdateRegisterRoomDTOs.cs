using API_dormitory.Models.common;
using API_dormitory.Models.registerRoom;

namespace API_dormitory.Models.DTO.RegisterRoom
{
    public class UpdateRegisterRoomDTOs
    {
        public int IdRegister { get; set; }
        public OperatingStatusEnum status { get; set; }
    }
}
