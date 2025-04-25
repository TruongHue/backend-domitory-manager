using API_dormitory.Models.common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace API_dormitory.Models.Rooms
{
    public class BuildingModels
    {
        [BsonId]
        public ObjectId IdBuilding { get; set; } // Chuyển từ int -> ObjectId

        [BsonIgnore] // Không lưu vào MongoDB, chỉ dùng để hiển thị
        public string Id => IdBuilding.ToString();
        [BsonElement("nameBuilding")]
        [BsonRequired]
        public string NameBuilding { get; set; } = string.Empty;

        [BsonElement("description")]
        public string? Description { get; set; }

        [BsonElement("status")]
        public OperatingStatusEnum? Status { get; set; }

        // Lưu luôn danh sách phòng, thay vì chỉ lưu ID
        [BsonElement("rooms")]
        public List<InfoRoomModels> Rooms { get; set; } = new List<InfoRoomModels>();
    }
}
