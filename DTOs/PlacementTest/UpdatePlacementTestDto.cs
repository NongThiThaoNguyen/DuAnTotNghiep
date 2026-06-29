using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.DTOs.PlacementTest
{
    public class UpdatePlacementTestDto : CreatePlacementTestDto
    {
        [Required]
        public int Id { get; set; }
    }
}
