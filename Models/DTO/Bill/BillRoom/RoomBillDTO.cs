using API_dormitory.Models.Bills;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using API_dormitory.Models.DTO.Room;
using API_dormitory.Models.common;

namespace API_dormitory.Models.DTO
{
    public class RoomBillDTO
    {

        public int IdRoomBill { get; set; }

        public int IdRoom { get; set; }


        public DateTime DateOfRecord { get; set; }

        public OperatingStatusEnum Status { get; set; }
        public decimal PriceYear { get; internal set; }
        public decimal DailyPrice { get; internal set; }
    }
}
