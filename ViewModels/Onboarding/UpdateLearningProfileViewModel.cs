using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.ViewModels.Onboarding
{
    public class UpdateLearningProfileViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn mục tiêu học tập")]
        public int? MainGoalId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn trình độ hiện tại")]
        public int? CurrentLevelId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ít nhất 1 kỹ năng ưu tiên")]
        [MinLength(1, ErrorMessage = "Vui lòng chọn ít nhất 1 kỹ năng ưu tiên")]
        public List<string> SelectedSkillCodes { get; set; } = new List<string>();

        [Required(ErrorMessage = "Vui lòng chọn thời gian học mỗi ngày")]
        [Range(1, 1440, ErrorMessage = "Thời gian học mỗi ngày không hợp lệ")]
        public int? DailyStudyMinutes { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn số ngày học mỗi tuần")]
        [Range(1, 7, ErrorMessage = "Số ngày học mỗi tuần phải từ 1 đến 7")]
        public int? WeeklyStudyDays { get; set; }

        public string? PreferredStudyTime { get; set; }

        public decimal? TargetScore { get; set; }

        public int? TargetLevelId { get; set; }

        public string? LearningNote { get; set; }
    }
}
