namespace API_dormitory.Models.DTO.Account
{
    public class AddAccountStaff
    {
        public string UserName { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public string NumberPhone { get; set; } = string.Empty;
        public int Roles { get; set; } // 2 = Student
        public string Status { get; set; } = "Active";
    }
}
