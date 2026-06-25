using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DuAnTotNghiep.ViewModels.Admin.Topics
{
    public class TopicListViewModel
    {
        public int Id { get; set; }
        public string TopicCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string SkillName { get; set; } = string.Empty;
        public string LevelName { get; set; } = string.Empty;
        public string ParentTopicName { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int ObjectiveCount { get; set; }
    }

    public class CreateTopicViewModel
    {
        [Required]
        public string TopicCode { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public int SkillId { get; set; }

        public int? LevelId { get; set; }

        [Required]
        public string Difficulty { get; set; } = "BASIC";

        public int? ParentTopicId { get; set; }

        public List<int> PrerequisiteIds { get; set; } = new List<int>();

        public int OrderIndex { get; set; }

        public IEnumerable<SelectListItem>? AvailableSkills { get; set; }
        public IEnumerable<SelectListItem>? AvailableLevels { get; set; }
        public IEnumerable<SelectListItem>? AvailableTopics { get; set; }
    }

    public class EditTopicViewModel
    {
        public int Id { get; set; }

        // Code is usually read-only in Edit according to prompt, but needed for display
        public string TopicCode { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public int? SkillId { get; set; }

        public int? LevelId { get; set; }

        [Required]
        public string Difficulty { get; set; } = "BASIC";

        public int? ParentTopicId { get; set; }

        public List<int> PrerequisiteIds { get; set; } = new List<int>();

        public int OrderIndex { get; set; }

        [Required]
        public string Status { get; set; } = "Active";

        public IEnumerable<SelectListItem>? AvailableSkills { get; set; }
        public IEnumerable<SelectListItem>? AvailableLevels { get; set; }
        public IEnumerable<SelectListItem>? AvailableTopics { get; set; }
    }
}
