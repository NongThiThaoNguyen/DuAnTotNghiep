using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DuAnTotNghiep.Models.ViewModels.Admin.Topics
{
    // Pagination metadata
    public class PaginationViewModel
    {
        public int Page { get; set; } = 1; // current page (1‑based)
        public int PageSize { get; set; } = 10; // items per page
        public int TotalItems { get; set; }
        public int TotalPages => (int)System.Math.Ceiling(TotalItems / (double)PageSize);
    }

    // Filter parameters used by the Index action and service
    public class TopicFilterViewModel
    {
        public string? Keyword { get; set; }
        public int? SkillId { get; set; }
        public int? LevelId { get; set; }
        public string? Difficulty { get; set; }
        public string? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    // ViewModel for the Index page aggregating all needed data
    public class TopicIndexViewModel
    {
        public TopicFilterViewModel Filter { get; set; } = new();
        public List<TopicListViewModel> Topics { get; set; } = new();
        public IEnumerable<SelectListItem> Skills { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Levels { get; set; } = new List<SelectListItem>();
        public PaginationViewModel Pagination { get; set; } = new();
    }
}
