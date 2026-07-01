using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models.ViewModels.Teacher;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Services;

public class TeacherDashboardService : ITeacherDashboardService
{
    private readonly ApplicationDbContext _context;

    public TeacherDashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TeacherDashboardViewModel> GetDashboardAsync(int teacherId)
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var teacher = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == teacherId);

        var activeTeacherTopics = _context.LearningTopics
            .AsNoTracking()
            .Where(t => t.Status == "ACTIVE" && t.CreatedBy == teacherId);

        var model = new TeacherDashboardViewModel
        {
            TeacherName = teacher?.FullName ?? "Giao vien",
            AvatarUrl = BuildAvatarUrl(teacher?.FullName, teacher?.AvatarUrl),
            CoursesCount = await activeTeacherTopics.CountAsync(),
            StudentsCount = await _context.Users
                .AsNoTracking()
                .CountAsync(u => u.Role.RoleCode == "STUDENT" && u.Status == "ACTIVE"),
            PendingSubmissionsCount = await _context.PracticeSubmissions
                .AsNoTracking()
                .CountAsync(s => s.Status == "SUBMITTED"
                    && (s.PracticeTask.CreatedBy == teacherId || s.PracticeTask.Topic!.CreatedBy == teacherId)),
            TodaySchedulesCount = await _context.Schedules
                .AsNoTracking()
                .CountAsync(s => s.TeacherId == teacherId && s.StartTime >= today && s.StartTime < tomorrow),
            RecentSubmissions = await _context.PracticeSubmissions
                .AsNoTracking()
                .Include(s => s.Student)
                .Include(s => s.PracticeTask)
                .Where(s => s.PracticeTask.CreatedBy == teacherId || s.PracticeTask.Topic!.CreatedBy == teacherId)
                .OrderByDescending(s => s.SubmittedAt)
                .Take(5)
                .Select(s => new RecentSubmissionViewModel
                {
                    Id = s.Id,
                    StudentName = s.Student.FullName,
                    TaskTitle = s.PracticeTask.Title,
                    SubmittedAt = s.SubmittedAt,
                    Status = s.Status
                })
                .ToListAsync(),
            TodaySchedules = await _context.Schedules
                .AsNoTracking()
                .Include(s => s.Topic)
                .Where(s => s.TeacherId == teacherId && s.StartTime >= today && s.StartTime < tomorrow)
                .OrderBy(s => s.StartTime)
                .Select(s => new TodayScheduleViewModel
                {
                    Id = s.Id,
                    Title = s.Title,
                    TopicName = s.Topic != null ? s.Topic.Title : string.Empty,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    Classroom = s.Classroom
                })
                .ToListAsync(),
            RecentQuizAttempts = await _context.QuizAttempts
                .AsNoTracking()
                .Include(q => q.Quiz)
                .Include(q => q.Student)
                .Where(q => q.SubmittedAt.HasValue
                    && (q.Quiz.CreatedBy == teacherId || q.Quiz.Topic!.CreatedBy == teacherId))
                .OrderByDescending(q => q.SubmittedAt)
                .Take(5)
                .Select(q => new RecentQuizAttemptViewModel
                {
                    Id = q.Id,
                    StudentName = q.Student.FullName,
                    QuizTitle = q.Quiz.Title,
                    Score = q.Score,
                    SubmittedAt = q.SubmittedAt
                })
                .ToListAsync()
        };

        var courseDistribution = await activeTeacherTopics
            .Include(t => t.Skill)
            .GroupBy(t => t.Skill.SkillName)
            .Select(g => new { SkillName = g.Key, Count = g.Count() })
            .OrderBy(d => d.SkillName)
            .ToListAsync();

        model.ChartLabels = courseDistribution.Select(d => d.SkillName).ToList();
        model.ChartValues = courseDistribution.Select(d => d.Count).ToList();

        return model;
    }

    private static string BuildAvatarUrl(string? fullName, string? avatarUrl)
    {
        if (!string.IsNullOrWhiteSpace(avatarUrl))
        {
            return avatarUrl;
        }

        var displayName = string.IsNullOrWhiteSpace(fullName) ? "GV" : fullName;
        return $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(displayName)}&background=6C63FF&color=fff";
    }
}
