using API_dormitory.Models.common;
using API_dormitory.Models.Bills;
using API_dormitory.Models.registerRoom;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace API_dormitory.Models.Rooms
{
    public class InfoRoomModels
    {
        [BsonId]
        public ObjectId IdRoom { get; set; }  // Chuyển từ int -> ObjectId

        [BsonElement("idBuilding")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId IdBuilding { get; set; }  // Giữ nguyên để liên kết

        [BsonElement("roomName")]
        [BsonRequired]
        public string RoomName { get; set; } = string.Empty;

        [BsonElement("numberOfBed")]
        [BsonRequired]
        public int NumberOfBed { get; set; }

        [BsonElement("gender")]
        [BsonRequired]
        public GenderEnum Gender { get; set; }

        [BsonElement("status")]
        [BsonRequired]
        public OperatingStatusEnum Status { get; set; }

        // Lưu trực tiếp toàn bộ Object thay vì chỉ lưu ID
        [BsonElement("building")]
        public BuildingModels Building { get; set; } = new BuildingModels();

        [BsonElement("electricityBillIds")]
        [BsonRepresentation(BsonType.ObjectId)]
        public List<ObjectId> ElectricityBillIds { get; set; } = new List<ObjectId>();
        [BsonElement("waterBillIds")]
        [BsonRepresentation(BsonType.ObjectId)]
        public List<ObjectId> WaterBillIds { get; set; } = new List<ObjectId>();


        [BsonElement("roomBillIds")]
        [BsonRepresentation(BsonType.ObjectId)]
        public List<ObjectId> RoomBillIds { get; set; } = new List<ObjectId>();

        [BsonElement("registerRoomIds")]
        [BsonRepresentation(BsonType.ObjectId)]
        public List<ObjectId> RegisterRoomIds { get; set; } = new List<ObjectId>();

    }
}
