using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.ViewModels.Onboarding
{
    public class OnboardingStep3ViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Vui lòng chọn ít nhất 1 kỹ năng ưu tiên.")]
        [MinLength(1, ErrorMessage = "Vui lòng chọn ít nhất 1 kỹ năng ưu tiên.")]
        [MaxLength(4, ErrorMessage = "Bạn chỉ được chọn tối đa 4 kỹ năng ưu tiên.")]
        public List<string> SelectedSkillCodes { get; set; } = new List<string>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (SelectedSkillCodes != null && SelectedSkillCodes.Count > 0)
            {
                var distinctCount = SelectedSkillCodes.Distinct().Count();
                if (distinctCount != SelectedSkillCodes.Count)
                {
                    yield return new ValidationResult("Có kỹ năng bị trùng lặp, vui lòng kiểm tra lại.", new[] { nameof(SelectedSkillCodes) });
                }
            }
        }
    }
}
