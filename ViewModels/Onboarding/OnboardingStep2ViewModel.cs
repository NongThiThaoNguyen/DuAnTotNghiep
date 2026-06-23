using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.ViewModels.Onboarding
{
    public class OnboardingStep2ViewModel : IValidatableObject
    {
        public int? CurrentLevelId { get; set; }

        public int? TargetLevelId { get; set; }

        public decimal? TargetScore { get; set; }

        // Cờ ẩn để biết người dùng đang thiết lập cho Goal nào
        public string? GoalCode { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Phải có ít nhất 1 mục tiêu (Level hoặc Điểm số)
            if (!TargetLevelId.HasValue && !TargetScore.HasValue)
            {
                yield return new ValidationResult("Vui lòng nhập mục tiêu mong muốn của bạn.", new[] { nameof(TargetLevelId), nameof(TargetScore) });
            }

            if (!string.IsNullOrEmpty(GoalCode))
            {
                if (GoalCode == "IELTS")
                {
                    if (TargetScore.HasValue && (TargetScore < 0 || TargetScore > 9))
                    {
                        yield return new ValidationResult("Điểm IELTS mục tiêu phải nằm trong khoảng từ 0 đến 9.0", new[] { nameof(TargetScore) });
                    }
                }
                else if (GoalCode == "TOEIC")
                {
                    if (TargetScore.HasValue && (TargetScore < 0 || TargetScore > 990))
                    {
                        yield return new ValidationResult("Điểm TOEIC mục tiêu phải nằm trong khoảng từ 0 đến 990", new[] { nameof(TargetScore) });
                    }
                }
            }
        }
    }
}
