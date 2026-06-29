using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DuAnTotNghiep.Models.ViewModels.QuestionBank
{
    public class QuestionCreateEditViewModel
    {
        public int? Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string QuestionText { get; set; } = string.Empty;

        [Required]
        public int SkillId { get; set; }

        public int? TopicId { get; set; }

        public int? LevelId { get; set; }

        [Required]
        public string DifficultyLevel { get; set; } = "BASIC"; // BASIC, MEDIUM, ADVANCED

        // For dropdowns
        public IEnumerable<SelectListItem>? AvailableSkills { get; set; }
        public IEnumerable<SelectListItem>? AvailableTopics { get; set; }
        public IEnumerable<SelectListItem>? AvailableLevels { get; set; }

        // To retain selected values when validation fails
        public int? SelectedSkillId { get; set; }
        public int? SelectedTopicId { get; set; }
        public int? SelectedLevelId { get; set; }
    }
}
