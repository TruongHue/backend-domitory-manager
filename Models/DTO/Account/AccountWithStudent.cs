using API_dormitory.Models.Users;

namespace API_dormitory.Models.DTO.Account
{
    public class AccountWithStudent
    {
        public AccountModels Account { get; set; }
        public List<InfoStudentModels> InfoStudent { get; set; }
    }
}
