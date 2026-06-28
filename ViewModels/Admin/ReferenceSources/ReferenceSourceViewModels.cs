using DuAnTotNghiep.Enums;
using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.ViewModels.Admin.ReferenceSources
{
    public class ReferenceSourceListItemViewModel
    {
        public int Id { get; set; }
        public string SourceName { get; set; } = string.Empty;
        public string? SourceUrl { get; set; }
        public ReferenceSourceType SourceType { get; set; }
        public ReferenceReviewStatus Status { get; set; }
        public int? CreatedById { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class ReferenceSourceSearchViewModel
    {
        public string? Keyword { get; set; }
        public ReferenceSourceType? SourceType { get; set; }
        public ReferenceReviewStatus? Status { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class ReferenceSourcePagedViewModel
    {
        public IEnumerable<ReferenceSourceListItemViewModel> Items { get; set; } = new List<ReferenceSourceListItemViewModel>();
        public int TotalItems { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public string? Keyword { get; set; }
        public ReferenceSourceType? SourceType { get; set; }
        public ReferenceReviewStatus? Status { get; set; }
        public Dictionary<ReferenceReviewStatus, int> StatusSummary { get; set; } = new Dictionary<ReferenceReviewStatus, int>();
    }
}
