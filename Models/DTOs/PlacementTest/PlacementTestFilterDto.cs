namespace DuAnTotNghiep.Models.DTOs.PlacementTest
{
    public class PlacementTestFilterDto
    {
        public string? Title { get; set; }
        public string? Status { get; set; }
        public int? TargetLevelId { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } // "Newest", "Oldest", "Title"
    }
}
