using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DuAnTotNghiep.Models.ViewModels.Onboarding;

public class LevelStepViewModel
{
    public IEnumerable<SelectListItem> AvailableLevels { get; set; } = new List<SelectListItem>();

    [Required(ErrorMessage = "Vui lòng chọn trình độ hiện tại.")]
    [Display(Name = "Trình độ hiện tại")]
    public int SelectedLevelId { get; set; }
}
