using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.Areas.Admin.ViewModels;

public class AdminTeacherScheduleFilterViewModel
{
    public string? Keyword { get; set; }
    public int? TeacherId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string SortBy { get; set; } = "startTime";
    public string SortDirection { get; set; } = "asc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class AdminTeacherScheduleListItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string TeacherEmail { get; set; } = string.Empty;
    public string TopicName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Classroom { get; set; }
}

public class AdminTeacherScheduleListViewModel
{
    public List<AdminTeacherScheduleListItemViewModel> Items { get; set; } = new();
    public AdminTeacherScheduleFilterViewModel Filter { get; set; } = new();
    public List<SelectListItem> AvailableTeachers { get; set; } = new();
    public List<SelectListItem> AvailableTopics { get; set; } = new();
    public AdminTeacherScheduleAiRequestViewModel AiRequest { get; set; } = new();
    public List<AdminTeacherScheduleAiSuggestionViewModel> AiSuggestions { get; set; } = new();
    public string? AiSummary { get; set; }
    public string? AiErrorMessage { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages => Filter.PageSize <= 0 ? 1 : (int)Math.Ceiling((double)TotalItems / Filter.PageSize);
}

public class AdminTeacherScheduleFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn giáo viên.")]
    public int TeacherId { get; set; }

    public int? TopicId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tiêu đề lịch dạy.")]
    [StringLength(255)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn thời gian bắt đầu.")]
    public DateTime StartTime { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn thời gian kết thúc.")]
    public DateTime EndTime { get; set; }

    [StringLength(255)]
    public string? Classroom { get; set; }

    public List<SelectListItem> AvailableTeachers { get; set; } = new();
    public List<SelectListItem> AvailableTopics { get; set; } = new();
}

public class AdminTeacherScheduleAiRequestViewModel
{
    public int? TeacherId { get; set; }
    public int? TopicId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu.")]
    public DateTime FromDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc.")]
    public DateTime ToDate { get; set; } = DateTime.Today.AddDays(7);

    [Range(1, 20)]
    public int SessionsCount { get; set; } = 3;

    [Range(15, 240)]
    public int DurationMinutes { get; set; } = 60;

    [Range(0, 23)]
    public int PreferredStartHour { get; set; } = 8;

    [Range(1, 24)]
    public int PreferredEndHour { get; set; } = 17;

    [StringLength(255)]
    public string? Classroom { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}

public class AdminTeacherScheduleAiSuggestionViewModel
{
    public string Title { get; set; } = string.Empty;
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string TeacherEmail { get; set; } = string.Empty;
    public int? TopicId { get; set; }
    public string TopicName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Classroom { get; set; }
    public string? Description { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class AdminTeacherScheduleAiResultViewModel
{
    public AdminTeacherScheduleAiRequestViewModel Request { get; set; } = new();
    public List<AdminTeacherScheduleAiSuggestionViewModel> Suggestions { get; set; } = new();
    public string? Summary { get; set; }
    public string Model { get; set; } = string.Empty;
}
