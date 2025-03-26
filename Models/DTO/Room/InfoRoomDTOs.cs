using API_dormitory.Models.Rooms;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using API_dormitory.Models.DTO.Building;
using API_dormitory.Models.Users;
using API_dormitory.Models.common;

namespace API_dormitory.Models.DTO.Room
{
    public class InfoRoomDTOs
    {
        public int IdRoom { get; set; }
        public int? IdBuilding { get; set; }
        public string? RoomName { get; set; }
        public int? NumberOfBed { get; set; }
        public OperatingStatusEnum? Status { get; set; }
        public BuildingDTOs Building { get; set; } // Chứa thông tin của tòa nhà
        public GenderEnum Gender { get; internal set; }
    }
}
