using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models.Exceptions;
using DuAnTotNghiep.Models;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Helpers;

/// <summary>
/// Applies M8 learning path regeneration limits and records replanning events.
/// </summary>
public static class LearningPathRegenerationHelper
{
    private const int DailyRegenerationLimit = 3;
    private const string TriggerType = "STUDENT_REGENERATE";
    private const string AppliedStatus = "APPLIED";

    public static async Task EnsureDailyLimitAsync(ApplicationDbContext context, int studentId)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var count = await context.AiReplanningEvents.AsNoTracking().CountAsync(ev =>
            ev.StudentId == studentId
            && ev.TriggerType == TriggerType
            && ev.CreatedAt >= today
            && ev.CreatedAt < tomorrow);

        if (count >= DailyRegenerationLimit)
        {
            throw new BusinessException("Bạn chỉ có thể tạo lại lộ trình tối đa 3 lần mỗi ngày.");
        }
    }

    public static void AddReplanningEvent(
        ApplicationDbContext context,
        int studentId,
        StudentLearningPath oldPath,
        StudentLearningPath newPath,
        string reason)
    {
        context.AiReplanningEvents.Add(new AiReplanningEvent
        {
            StudentId = studentId,
            LearningPathId = newPath.Id,
            TriggerType = TriggerType,
            OldPlanSummary = oldPath.AiPlanSummary,
            NewPlanSummary = newPath.AiPlanSummary,
            Reason = reason,
            Status = AppliedStatus,
            CreatedAt = DateTime.UtcNow
        });
    }
}
