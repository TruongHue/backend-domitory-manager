using API_dormitory.Models.common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace API_dormitory.Models.Users
{
    public class AccountModels
    {
        [BsonId] // Định nghĩa ID cho MongoDB
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId AccountId { get; set; } // Đổi `string` thành `ObjectId`

        [BsonElement("userName")] // Giữ nguyên tên trường như trong SQL
        public string? UserName { get; set; }

        [BsonElement("userCode")]
        public string? UserCode { get; set; }

        [BsonElement("numberPhone")]
        [BsonRequired] // Đảm bảo dữ liệu không null
        public string NumberPhone { get; set; } = string.Empty;

        [BsonElement("password")]
        [BsonRequired]
        public string Password { get; set; } = string.Empty;

        [BsonElement("roles")]
        [BsonRequired]
        public RoleTypeStatusEnum Roles { get; set; }

        [BsonElement("status")]
        [BsonRequired]
        public OperatingStatusEnum Status { get; set; }

        [BsonElement("infoStudent")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId? InfoStudentId { get; set; } // Đổi `string` thành `ObjectId`

        [BsonIgnore] // Không lưu vào MongoDB, chỉ dùng khi truy vấn
        public InfoStudentModels? InfoStudent { get; set; }
    }
}
