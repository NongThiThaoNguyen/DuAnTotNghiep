using System.ComponentModel.DataAnnotations;
using DuAnTotNghiep.Models.Enums;

namespace DuAnTotNghiep.Models.ViewModels
{
    public class GenerateQuizRequestViewModel
    {
        [Required]
        [Display(Name = "Skill")]
        public int SkillId { get; set; }

        [Required]
        [Display(Name = "Topic")]
        public int TopicId { get; set; }

        [Required]
        [Display(Name = "Proficiency Level")]
        public int ProficiencyLevelId { get; set; }

        [Required]
        [Display(Name = "Difficulty")]
        public Difficulty Difficulty { get; set; }

        [Required]
        [Display(Name = "Question Type")]
        public QuestionType QuestionType { get; set; }

        [Required]
        [Range(1,20, ErrorMessage = "Question count must be between {1} and {2}.")]
        [Display(Name = "Number of Questions")]
        public int QuestionCount { get; set; } = 1;

        [StringLength(2000)]
        [Display(Name = "Notes (optional)")]
        public string? Notes { get; set; }
    }
}
