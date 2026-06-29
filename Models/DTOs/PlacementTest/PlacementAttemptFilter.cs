using System;

namespace DuAnTotNghiep.Models.DTOs.PlacementTest
{
    public class PlacementAttemptFilter
    {
        public int? TestId { get; set; }

        public int? StudentId { get; set; }

        public string? Status { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public string? Keyword { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}
