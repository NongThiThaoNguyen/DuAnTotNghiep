using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DuAnTotNghiep.Models.ViewModels.Onboarding;

public class GoalStepViewModel
{
    public IEnumerable<SelectListItem> AvailableGoals { get; set; } = new List<SelectListItem>();

    [Required(ErrorMessage = "Vui lòng chọn mục tiêu học tập.")]
    [Display(Name = "Mục tiêu chính")]
    public int SelectedGoalId { get; set; }
}
