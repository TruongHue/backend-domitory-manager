using API_dormitory.Models.common;
using API_dormitory.Models.Rooms;
using API_dormitory.Models.Users;

namespace API_dormitory.Models.DTO.Room
{
    public class AddRoomDTO
    {
        public int IdRoom { get; set; }
        public int? IdBuilding { get; set; }
        public string? RoomName { get; set; }
        public int? NumberOfBed { get; set; }
        public OperatingStatusEnum? Status { get; set; }
        public GenderEnum Gender { get;set; }
    }
}
