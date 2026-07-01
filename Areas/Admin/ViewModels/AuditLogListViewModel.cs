using System;
using System.Collections.Generic;
using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Areas.Admin.ViewModels
{
    public class AuditLogListViewModel
    {
        public List<AuditLog> Logs { get; set; } = new();
        public List<string> AvailableActions { get; set; } = new();

        // Filters
        public string? ActionFilter { get; set; }
        public string? UserFilter { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // Pagination
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
    }
}
