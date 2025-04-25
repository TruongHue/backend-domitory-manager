using API_dormitory.Models.Bills;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using API_dormitory.Models.common;

namespace API_dormitory.Models.DTO.Bill.BillRoom
{
    public class AddBillRoomDTOs
    {


        public string IdRoom { get; set; }
        public decimal DailyPrice { get; set; }

        public decimal PriceYear { get; set; }

        public DateTime DateOfRecord { get; set; }

    }
}
