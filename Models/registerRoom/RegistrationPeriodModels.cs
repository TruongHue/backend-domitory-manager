using API_dormitory.Models.common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace API_dormitory.Models.Registrations
{
    public class RegistrationPeriodModels
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } // MongoDB sử dụng ObjectId thay vì int

        [BsonElement("actionDate")]
        public DateTime ActionDate { get; set; } // Ngày thực hiện thao tác

        [BsonElement("startDate")]
        public DateTime StartDate { get; set; } // Ngày bắt đầu đăng ký

        [BsonElement("endDate")]
        public DateTime EndDate { get; set; } // Ngày kết thúc đăng ký

        [BsonElement("semesterStatus")]
        public SemesterStatusEnum SemesterStatus { get; set; } // Trạng thái học kỳ

        [BsonElement("status")]
        public RegistrationStatusEnum Status { get; set; } // Trạng thái đăng ký (Mở/Đóng)
    }
}
