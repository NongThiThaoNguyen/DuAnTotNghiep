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
    }
}
