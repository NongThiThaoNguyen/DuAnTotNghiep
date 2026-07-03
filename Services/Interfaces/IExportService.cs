using System;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IExportService
    {
        Task<byte[]> ExportUsersToExcelAsync();
        Task<byte[]> ExportPlacementResultsToExcelAsync();
        Task<byte[]> ExportAuditLogsToExcelAsync(DateTime? from, DateTime? to);
        Task<byte[]> ExportAiUsageLogsToExcelAsync();
        Task<byte[]> ExportStudentReportAsync(int? studentId);
        Task<byte[]> ExportTeacherReportAsync(int? teacherId);
        Task<byte[]> ExportAttendanceReportAsync();
    }
}
