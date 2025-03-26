using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using API_dormitory.Models.common;

namespace API_dormitory.Models.Rooms
{
    public class BuildingModels
    {
        [Column("idBuilding")]
        [Required]
        [Key]
        public int IdBuilding { get; set; }
        [Column("nameBuilding")]
        [Required]
        public string NameBuilding { get; set; }
        [Column("description")]
        [Required]
        public string? Description { get; set; }
        [Column("status")]
        [Required]
        public OperatingStatusEnum? Status {  get; set; }

        public virtual ICollection<InfoRoomModels> Rooms { get; set; } = new List<InfoRoomModels>();

    }


}
