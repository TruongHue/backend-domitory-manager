using API_dormitory.Models.Rooms;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using API_dormitory.Models.DTO.Room;
using API_dormitory.Models.common;

namespace API_dormitory.Models.DTO.Building
{
    public class BuildingDTOs
    {
        public string? IdBuilding { get; set; }
        public string? NameBuilding { get; set; }
        public string? Description { get; set; }
        public OperatingStatusEnum? Status { get; set; }
    }
}
