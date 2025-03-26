using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_dormitory.Models.Bills
{
    public class PriceWaterAndElectricity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idPrice { get; set; } // ID tự động tăng

        // Giá điện tiêu chuẩn
        [Column(TypeName = "decimal(18,2)")]
        public decimal electricityPrice { get; set; }

        // Giá nước tiêu chuẩn
        [Column(TypeName = "decimal(18,2)")]
        public decimal waterPrice { get; set; }

        // Giới hạn tiêu thụ nước trước khi giá thay đổi (m³)
        public int waterLimit { get; set; }

        // Giá nước khi tiêu thụ vượt mức giới hạn
        [Column(TypeName = "decimal(18,2)")]
        public decimal waterPriceOverLimit { get; set; }

        // Ngày thực hiện thao tác
        public DateTime ActionDate { get; set; } = DateTime.UtcNow; // Mặc định là ngày hiện tại
    }
}
