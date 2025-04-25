using API_dormitory.Models.common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace API_dormitory.Models.registerRoom
{
    [BsonIgnoreExtraElements] // Bỏ qua các trường không có trong model
    public class RegisterRoomModels
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId IdRegister { get; set; } // ID MongoDB dạng ObjectId

        [BsonElement("idStudent")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId IdStudent { get; set; } // Nếu lưu dưới dạng ObjectId

        [BsonElement("idRoom")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId IdRoom { get; set; }

        [BsonElement("idRegistrationPeriod")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId IdRegistrationPeriod { get; set; }

        [BsonElement("startDate")]
        public DateTime StartDate { get; set; }

        [BsonElement("endDate")]
        public DateTime EndDate { get; set; }

        [BsonElement("actionDate")]
        public DateTime ActionDate { get; set; } = DateTime.UtcNow;

        [BsonElement("total")]
        public double Total { get; set; } // Chuyển `decimal` → `double` cho MongoDB

        [BsonElement("paymentStatus")]
        public PaymentStatusEnum PaymentStatus { get; set; }

        [BsonElement("status")]
        public OperatingStatusEnum Status { get; set; }
    }
}
