using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.ViewModels.Onboarding
{
    public class OnboardingStep1ViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn mục tiêu học tập")]
        public int? MainGoalId { get; set; }
    }
}
