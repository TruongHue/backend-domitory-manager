using API_dormitory.Models.Rooms;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using API_dormitory.Models.registerRoom;
using API_dormitory.Models.common;

namespace API_dormitory.Models.Bills
{
    public class WaterBillModels
    {
        [Key]
        [Column("idWater")]
        public int IdWater { get; set; }

        [Required]
        [ForeignKey("InfoRoom")]
        [Column("idRoom")]
        public int IdRoom { get; set; }

        [Required]
        [Column("beforeIndex")]
        public int BeforeIndex { get; set; }

        [Required]
        [Column("afterIndex")]
        public int AfterIndex { get; set; }
        [Required]
        [Column("price")]
        public decimal Price { get; set; }
        [Required]
        [Column("indexLimit")]
        public decimal IndexLimit { get; set; }
        [Required]
        [Column("priceLimit")]
        public decimal PriceLimit { get; set; }
        [Required]
        [Column("dateOfRecord")]
        public DateTime DateOfRecord { get; set; }
        [Required]
        [Column("total")]
        public decimal Total { get; set; }
        [Required]
        [Column("status")]
        public PaymentStatusEnum Status { get; set; }

        // Quan hệ: Một hóa đơn thuộc về một phòng
        public virtual InfoRoomModels InfoRoom { get; set; }
    }


}

