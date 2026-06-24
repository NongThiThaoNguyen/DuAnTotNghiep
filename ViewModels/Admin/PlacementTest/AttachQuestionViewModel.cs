using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.ViewModels.Admin.PlacementTest
{
    public class AttachQuestionViewModel
    {
        [Required]
        public int SectionId { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [Range(0, 100)]
        public decimal Points { get; set; }

        public int OrderIndex { get; set; }
    }
}
