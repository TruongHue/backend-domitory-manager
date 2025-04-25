using API_dormitory.Models.common;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace API_dormitory.Models.DTO.RegisterRoom
{
    public class AddRegistrationPeriodDTOs
    {

        public DateTime ActionDate { get; set; } 
        public DateTime StartDate { get; set; } 

        public DateTime EndDate { get; set; }

        public SemesterStatusEnum SemesterStatus { get; set; }

        public RegistrationStatusEnum Status { get; set; } 
    }
}
