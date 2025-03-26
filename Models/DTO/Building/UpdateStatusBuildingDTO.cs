using API_dormitory.Models.common;
using API_dormitory.Models.Rooms;

namespace API_dormitory.Models.DTO.Building
{
    public class UpdateStatusBuildingDTO
    {
        public int IdBuilding { get; set; }
        public OperatingStatusEnum? Status { get; set; }
    }
}
