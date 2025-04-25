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
        public string IdRoom { get; set; }
        public string? IdBuilding { get; set; }
        public string? RoomName { get; set; }
        public int? NumberOfBed { get; set; }
        public OperatingStatusEnum? Status { get; set; }
        public GenderEnum Gender { get; internal set; }
    }
}
