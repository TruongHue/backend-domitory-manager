using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace API_dormitory.Models.Bills
{
    public class PriceWaterAndElectricity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; } // ID trong MongoDB là ObjectId

        [BsonElement("electricityPrice")]
        public decimal ElectricityPrice { get; set; } // Giá điện tiêu chuẩn

        [BsonElement("waterPrice")]
        public decimal WaterPrice { get; set; } // Giá nước tiêu chuẩn

        [BsonElement("waterLimit")]
        public int WaterLimit { get; set; } // Giới hạn tiêu thụ nước trước khi giá thay đổi (m³)

        [BsonElement("waterPriceOverLimit")]
        public decimal WaterPriceOverLimit { get; set; } // Giá nước khi tiêu thụ vượt mức giới hạn

        [BsonElement("actionDate")]
        public DateTime ActionDate { get; set; } = DateTime.UtcNow; // Mặc định là ngày hiện tại
    }
}