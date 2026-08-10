using DuAnTotNghiep.Areas.Admin.ViewModels;

namespace DuAnTotNghiep.Services.Interfaces;

public interface IAdminReportService
{
    Task<AdminReportsIndexViewModel> GetOverviewAsync(string? period = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<StudentProgressReportViewModel> GetStudentProgressReportAsync();
    Task<TeacherActivityReportViewModel> GetTeacherActivityReportAsync();
    Task<AttendanceSummaryReportViewModel> GetAttendanceSummaryAsync();
    Task<QuizPerformanceReportViewModel> GetQuizPerformanceReportAsync();
}
