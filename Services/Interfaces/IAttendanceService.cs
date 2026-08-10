using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.ViewModels.Teacher;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IAttendanceService
    {
        Task<List<StudentAttendanceViewModel>> GetAttendanceListAsync(int topicId, DateOnly date);
        Task SaveAttendanceAsync(int topicId, DateOnly date, List<StudentAttendanceViewModel> attendances);
        Task<List<Attendance>> GetAttendanceHistoryAsync(int? topicId, DateOnly? startDate, DateOnly? endDate);
        Task<List<LearningTopic>> GetActiveTopicsAsync();
        Task<byte[]> ExportAttendanceToExcelAsync(int? topicId, DateOnly? date, DateOnly? startDate, DateOnly? endDate);
        Task<byte[]> GenerateAttendanceExcelTemplateAsync(int topicId, DateOnly date);
        Task<(int SuccessCount, List<string> Errors)> ImportAttendanceFromExcelAsync(int topicId, DateOnly date, System.IO.Stream fileStream);
        Task<bool> UpdateAttendanceRecordAsync(int id, string status, string? remarks);
    }
}
