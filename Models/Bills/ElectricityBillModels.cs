using API_dormitory.Models.common;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_dormitory.Models.Rooms
{
    public class ElectricityBillModels
    {
        [Key]
        [Column("idElectricity")]
        public int IdElectricity { get; set; }

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
