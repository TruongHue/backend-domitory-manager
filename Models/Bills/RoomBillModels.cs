using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using API_dormitory.Models.Rooms;
using API_dormitory.Models.common;

namespace API_dormitory.Models.Bills
{
    public class RoomBillModels
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId IdRoomBill { get; set; }  // MongoDB sử dụng ObjectId dạng string

        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId IdRoom { get; set; } // Liên kết tới phòng

        [BsonRequired]
        [BsonElement("dailyPrice")]
        public decimal DailyPrice { get; set; }

        [BsonRequired]
        [BsonElement("priceYear")]
        public decimal PriceYear { get; set; }

        [BsonRequired]
        [BsonElement("dateOfRecord")]
        public DateTime DateOfRecord { get; set; }

        [BsonRequired]
        [BsonElement("status")]
        public OperatingStatusEnum Status { get; set; }

        [BsonIgnore]
        public virtual InfoRoomModels InfoRoom { get; set; } // Không lưu trực tiếp trong MongoDB
    }
}
