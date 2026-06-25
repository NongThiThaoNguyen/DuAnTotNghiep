using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace DuAnTotNghiep.ViewModels.Admin.Objectives
{
    public class ObjectiveCreateViewModel
    {
        [Required]
        [Display(Name = "Topic")]
        public int TopicId { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Title (Objective Text)")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Code (Cognitive Level)")]
        public string Code { get; set; } = "Remembering";

        public int OrderIndex { get; set; }
        
        [Required]
        public bool IsActive { get; set; } = true;

        public IEnumerable<SelectListItem>? AvailableTopics { get; set; }
    }
}
