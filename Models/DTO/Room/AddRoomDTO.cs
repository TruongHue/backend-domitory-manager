using API_dormitory.Models.common;
using API_dormitory.Models.Rooms;
using API_dormitory.Models.Users;

namespace API_dormitory.Models.DTO.Room
{
    public class AddRoomDTO
    {
        public string? IdBuilding { get; set; }
        public string? RoomName { get; set; }
        public int? NumberOfBed { get; set; }
        public OperatingStatusEnum? Status { get; set; }
        public GenderEnum Gender { get;set; }
        public decimal DailyPrice { get; set; }

        public decimal PriceYear { get; set; }

        public DateTime DateOfRecord { get; set; }
    }
}
