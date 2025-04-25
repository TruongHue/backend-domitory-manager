using API_dormitory.Models.common;

namespace API_dormitory.Models.DTO.Bill.BillRoom
{
    public class WaterBillDto
    {
        public string Id { get; set; }
        public string IdRoom { get; set; }
        public string StudentCode { get; set; }
        public string StudentName { get; set; }
        public int BeforeIndex { get; set; }
        public int AfterIndex { get; set; }
        public decimal Price { get; set; }
        public decimal IndexLimit { get; set; }
        public decimal PriceLimit { get; set; }
        public decimal Total { get; set; }
        public DateTime DateOfRecord { get; set; }
        public PaymentStatusEnum Status { get; set; }
    }
}
