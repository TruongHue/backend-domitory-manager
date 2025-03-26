using API_dormitory.Models.DTO.Account;
using API_dormitory.Models.DTO.User;

public class StudentRequestDTO
{
    public AccountDTOs Account { get; set; }
    public InfoStudentDTOs? InfoStudent { get; set; }
}
