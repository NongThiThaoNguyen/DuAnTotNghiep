using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models.DTOs.LearningPath;
using DuAnTotNghiep.Models.Enums;
using DuAnTotNghiep.Models.Exceptions;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.Repositories.Interfaces;
using DuAnTotNghiep.Models.ViewModels.LearningPath.M8;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Helpers;

/// <summary>
/// Builds normalized input and readiness data for M8 learning path generation.
/// </summary>
public static class LearningPathInputBuilder
{
    private static readonly string[] ActiveTopicStatuses = { "ACTIVE", "Active" };
    private static readonly string[] ApprovedLessonStatuses = { "APPROVED", "Approved" };
    private static readonly string[] AvailableQuizStatuses = { "ACTIVE", "PUBLISHED", "APPROVED" };

    public static async Task<LearningPathInputDto> BuildInputAsync(ApplicationDbContext context, int studentId)
    {
        var profile = await GetProfileAsync(context, studentId);
        var analysis = await GetLatestAnalysisAsync(context, studentId);

        return new LearningPathInputDto
        {
            StudentId = studentId,
            GoalName = profile.MainGoal?.GoalName ?? string.Empty,
            TargetLevelName = profile.TargetLevel?.Name ?? string.Empty,
            CurrentLevelName = profile.CurrentLevel?.Name ?? string.Empty,
            AvailableMinutesPerDay = profile.DailyStudyMinutes ?? 30,
            SkillPriorities = await GetSkillPrioritiesAsync(context, analysis.Id),
            Strengths = analysis.Strengths ?? string.Empty,
            Weaknesses = analysis.Weaknesses ?? string.Empty,
            PriorityTopics = SplitCsv(analysis.Weaknesses),
            AvailableTopics = await GetAvailableTopicsAsync(context),
            AvailableLessons = await GetAvailableLessonsAsync(context),
            AvailableQuizzes = await GetAvailableQuizzesAsync(context)
        };
    }

    public static async Task<LearningPathGenerateViewModel> CanGeneratePathAsync(
        ApplicationDbContext context,
        ILearningPathRepository repository,
        int studentId)
    {
        var hasOnboarding = await HasCompletedOnboardingAsync(context, studentId);
        var hasPlacementTest = await HasCompletedPlacementTestAsync(context, studentId);
        var hasCompetencyAnalysis = await HasCompetencyAnalysisAsync(context, studentId);
        var hasActivePath = await repository.GetActivePathByStudentIdAsync(studentId) != null;

        return new LearningPathGenerateViewModel
        {
            StudentId = studentId,
            HasOnboarding = hasOnboarding,
            HasPlacementTest = hasPlacementTest,
            HasCompetencyAnalysis = hasCompetencyAnalysis,
            HasActivePath = hasActivePath,
            MissingStep = GetMissingStep(hasOnboarding, hasPlacementTest, hasCompetencyAnalysis, hasActivePath)
        };
    }

    public static async Task<StudentLearningProfile> GetProfileAsync(ApplicationDbContext context, int studentId)
    {
        var profile = await context.StudentLearningProfiles
            .AsNoTracking()
            .Include(p => p.MainGoal)
            .Include(p => p.CurrentLevel)
            .Include(p => p.TargetLevel)
            .FirstOrDefaultAsync(p => p.UserId == studentId);

        return profile ?? throw new NotFoundException("Không tìm thấy hồ sơ học tập của học viên.");
    }

    public static async Task<CompetencyAnalysis> GetLatestAnalysisAsync(ApplicationDbContext context, int studentId)
    {
        var analysis = await context.CompetencyAnalyses
            .AsNoTracking()
            .Where(a => a.StudentId == studentId)
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync();

        return analysis ?? throw new NotFoundException("Không tìm thấy phân tích năng lực của học viên.");
    }

    private static async Task<List<string>> GetSkillPrioritiesAsync(ApplicationDbContext context, int analysisId)
    {
        return await context.CompetencySkillScores
            .AsNoTracking()
            .Include(s => s.Skill)
            .Where(s => s.CompetencyAnalysisId == analysisId)
            .OrderByDescending(s => s.PriorityLevel)
            .ThenBy(s => s.Score)
            .Select(s => s.Skill.SkillName)
            .ToListAsync();
    }

    private static async Task<List<LearningPathResourceDto>> GetAvailableTopicsAsync(ApplicationDbContext context)
    {
        return await context.LearningTopics
            .AsNoTracking()
            .Where(t => ActiveTopicStatuses.Contains(t.Status))
            .OrderBy(t => t.OrderIndex)
            .Select(t => new LearningPathResourceDto { Id = t.Id, Name = t.Title })
            .ToListAsync();
    }

    private static async Task<List<LearningPathResourceDto>> GetAvailableLessonsAsync(ApplicationDbContext context)
    {
        return await context.OriginalLessons
            .AsNoTracking()
            .Where(l => ApprovedLessonStatuses.Contains(l.ReviewStatus))
            .OrderBy(l => l.Title)
            .Select(l => new LearningPathResourceDto { Id = l.Id, Name = l.Title })
            .ToListAsync();
    }

    private static async Task<List<LearningPathResourceDto>> GetAvailableQuizzesAsync(ApplicationDbContext context)
    {
        return await context.Quizzes
            .AsNoTracking()
            .Where(q => AvailableQuizStatuses.Contains(q.Status))
            .OrderBy(q => q.Title)
            .Select(q => new LearningPathResourceDto { Id = q.Id, Name = q.Title })
            .ToListAsync();
    }

    private static async Task<bool> HasCompletedOnboardingAsync(ApplicationDbContext context, int studentId)
    {
        return await context.StudentLearningProfiles
            .AsNoTracking()
            .AnyAsync(p => p.UserId == studentId && p.OnboardingStatus == "COMPLETED");
    }

    private static async Task<bool> HasCompletedPlacementTestAsync(ApplicationDbContext context, int studentId)
    {
        return await context.TestAttempts
            .AsNoTracking()
            .AnyAsync(a => a.StudentId == studentId &&
                (a.Status == TestAttemptStatus.Submitted || a.Status == TestAttemptStatus.Graded));
    }

    private static async Task<bool> HasCompetencyAnalysisAsync(ApplicationDbContext context, int studentId)
    {
        return await context.CompetencyAnalyses.AsNoTracking().AnyAsync(a => a.StudentId == studentId);
    }

    private static List<string> SplitCsv(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return new List<string>();
        return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }

    private static string GetMissingStep(bool hasOnboarding, bool hasPlacementTest, bool hasAnalysis, bool hasPath)
    {
        if (!hasOnboarding) return "Hoàn thành onboarding trước khi tạo lộ trình.";
        if (!hasPlacementTest) return "Hoàn thành placement test trước khi tạo lộ trình.";
        if (!hasAnalysis) return "Chờ hệ thống phân tích năng lực trước khi tạo lộ trình.";
        if (hasPath) return "Bạn đã có lộ trình học đang hoạt động.";
        return string.Empty;
    }
}
