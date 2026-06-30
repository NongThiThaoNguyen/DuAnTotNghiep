using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DuAnTotNghiep.Models.ViewModels.StudyPlan;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IStudyPlanService
    {
        Task<TodayStudyPlanViewModel> GetTodayPlanAsync(int studentId);
        Task<WeeklyStudyPlanViewModel> GetWeeklyPlanAsync(int studentId, DateOnly? weekStart = null);
        Task<MonthlyStudyPlanViewModel> GetMonthlyPlanAsync(int studentId, int? year = null, int? month = null);
        Task<bool> MarkTaskSkippedAsync(int nodeId, int studentId, string? reason = null);
        Task<List<DailyStudyTaskViewModel>> GetOverdueTasksAsync(int studentId);
        Task<int> CalculateDailyLoadAsync(int studentId, DateOnly? date = null);
    }
}
