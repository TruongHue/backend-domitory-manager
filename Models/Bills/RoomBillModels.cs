using API_dormitory.Models.Rooms;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using API_dormitory.Models.common;

namespace API_dormitory.Models.Bills
{
    public class RoomBillModels
    {
        [Key]
        [Column("idRoomBill")]
        public int IdRoomBill { get; set; }

        [Required]
        [ForeignKey("InfoRoom")]
        [Column("idRoom")]
        public int IdRoom { get; set; }
        [Required]
        [Column("dailyPrice", TypeName = "decimal(18,2)")]
        public decimal DailyPrice { get; set; }
        [Required]
        [Column("priceYear", TypeName = "decimal(18,2)")]
        public decimal PriceYear { get; set; }
        [Required]
        [Column("dateOfRecord")]
        public DateTime DateOfRecord { get; set; }

        [Required]
        [Column("status")]
        public OperatingStatusEnum Status { get; set; }

        // Quan hệ: Một hóa đơn thuộc về một phòng
        public virtual InfoRoomModels InfoRoom { get; set; }
    }


}
