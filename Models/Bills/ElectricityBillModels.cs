using API_dormitory.Models.common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace API_dormitory.Models.Rooms
{
    public class ElectricityBillModels
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [BsonElement("idRoom")]
        public ObjectId IdRoom { get; set; }


        [BsonElement("studentCode")]
        [BsonIgnoreIfNull] // Bỏ qua nếu giá trị là null
        public string StudentCode { get; set; } // Cho phép null


        [BsonElement("studentName")]
        [BsonIgnoreIfNull] // Bỏ qua nếu giá trị là null
        public string StudentName { get; set; } // Cho phép null
        [BsonElement("beforeIndex")]
        public int BeforeIndex { get; set; }

        [BsonElement("afterIndex")]
        public int AfterIndex { get; set; }

        [BsonElement("price")]
        public decimal Price { get; set; }

        [BsonElement("dateOfRecord")]
        public DateTime DateOfRecord { get; set; }

        [BsonElement("total")]
        public decimal Total { get; set; }

        [BsonElement("status")]
        public PaymentStatusEnum Status { get; set; }
    }
}
