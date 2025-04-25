using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace API_dormitory.Models.Feedback
{
    public class FeedbackModels
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string AccountId { get; set; } = null!;  // ID người gửi

        public string Title { get; set; } = null!;
        public string Sender { get; set; } = "Bạn";
        public string Content { get; set; } = null!;
        public string? Response { get; set; } = null;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ResponseAt { get; set; } = null;  // 🕒 Thời gian phản hồi
    }
}
