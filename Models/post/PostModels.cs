using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace API_dormitory.Models.Post
{
    public class PostModel
    {
        // Mã ID của bài đăng, tự động tạo khi lưu vào MongoDB
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)] // Chuyển đổi ObjectId thành string khi xuất ra JSON
        public string? Id { get; set; }

        // Tiêu đề của bài đăng
        [BsonElement("Title")]
        public string Title { get; set; }

        // Nội dung bài đăng
        [BsonElement("Content")]
        public string Content { get; set; }

        // Ngày thực hiện bài đăng
        [BsonElement("PostDate")]
        public DateTime PostDate { get; set; }
    }
}
