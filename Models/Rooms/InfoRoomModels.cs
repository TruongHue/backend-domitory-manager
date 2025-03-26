using API_dormitory.Models.Bills;
using API_dormitory.Models.common;
using API_dormitory.Models.DTO;
using API_dormitory.Models.registerRoom;
using API_dormitory.Models.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_dormitory.Models.Rooms
{
    public class InfoRoomModels
    {
        [Key]
        [Column("idRoom")]
        public int IdRoom { get; set; }

        [Required]
        [ForeignKey("Building")]  // Đổi tên Foreign Key đúng với Navigation Property
        [Column("idBuilding")]
        public int IdBuilding { get; set; }

        [Required]
        [Column("nameRoom")]
        public string RoomName { get; set; } = string.Empty;

        [Required]
        [Column("numberOfBed")]
        public int NumberOfBed { get; set; }
        [Required]
        [Column("gender")]
        public GenderEnum Gender { get; set; }
        [Required]
        [Column("status")]
        public OperatingStatusEnum Status { get; set; }
        // Navigation Property (Một phòng thuộc về một tòa nhà)
        public virtual BuildingModels Building { get; set; }
        public virtual List<ElectricityBillModels> ElectricityBills { get; set; } = new List<ElectricityBillModels>();
        public virtual List<WaterBillModels> WaterBills { get; set; } = new List<WaterBillModels>();
        public virtual List<RoomBillModels> RoomBills { get; set; } = new List<RoomBillModels>();
        public virtual List<RegisterRoomModels> RegisterRooms { get; set; } = new List<RegisterRoomModels>();

    }

}
