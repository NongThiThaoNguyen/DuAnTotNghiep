using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.Models.DTOs.PlacementTestSection
{
    public class UpdateSectionDto : CreateSectionDto
    {
        [Required]
        public int Id { get; set; }
    }
}
