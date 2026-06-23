using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.ViewModels.Onboarding
{
    public class OnboardingStep4ViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn thời gian học mỗi ngày")]
        [Range(1, 1440, ErrorMessage = "Thời gian học mỗi ngày không hợp lệ")]
        public int? DailyStudyMinutes { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn số ngày học mỗi tuần")]
        [Range(1, 7, ErrorMessage = "Số ngày học mỗi tuần phải từ 1 đến 7")]
        public int? WeeklyStudyDays { get; set; }

        public string? PreferredStudyTime { get; set; }
    }
}
