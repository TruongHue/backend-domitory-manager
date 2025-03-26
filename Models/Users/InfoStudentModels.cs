using API_dormitory.Models.common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace API_dormitory.Models.Users
{
    public class InfoStudentModels
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; } // Đổi `string` thành `ObjectId`

        [BsonElement("accountId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId? AccountId { get; set; } // Đổi `string` thành `ObjectId`

        [BsonElement("gender")]
        public GenderEnum Gender { get; set; }

        [BsonElement("picture")]
        public string? Picture { get; set; }

        [BsonElement("nameParent")]
        public string? NameParent { get; set; }

        [BsonElement("address")]
        public string? Address { get; set; }

        [BsonElement("parentNumberPhone")]
        public string? ParentNumberPhone { get; set; }

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonIgnore]
        public AccountModels Account { get; set; } // <-- Thêm dòng này

        [BsonElement("feedbacks")]
        public List<ObjectId> FeedbackIds { get; set; } = new List<ObjectId>(); // Đổi `string` thành `ObjectId`

        [BsonElement("registerRooms")]
        public List<ObjectId> RegisterRoomIds { get; set; } = new List<ObjectId>(); // Đổi `string` thành `ObjectId`
    }
}
