namespace API_dormitory.Models.DTO.Bill.BillElectricity
{
    public class UpdateElectricityBillDTO
    {
        public string StudentCode { get; set; } // Có thể null nếu không cập nhật mã sinh viên
        public string StudentName { get; set; } // Có thể null nếu không cập nhật tên sinh viên
    }

}
