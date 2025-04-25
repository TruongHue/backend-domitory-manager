using API_dormitory.Models.common;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API_dormitory.Models.DTO.Bill.BillElectricity
{
    public class AddBillElectricityDTO
    {
        public string IdRoom { get; set; }
        public int AfterIndex { get; set; }
        public DateTime DateOfRecord { get; set; }
    }
}
