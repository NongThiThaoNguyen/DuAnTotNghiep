using DuAnTotNghiep.Areas.Admin.ViewModels;
using DuAnTotNghiep.Data;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Services.Interfaces;

namespace DuAnTotNghiep.Services;

public class AdminReportService : IAdminReportService
{
    private readonly ApplicationDbContext _context;

    public AdminReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminReportsIndexViewModel> GetOverviewAsync()
    {
        return new AdminReportsIndexViewModel
        {
            StudentCount = await _context.Users.AsNoTracking().CountAsync(u => u.Role.RoleCode == "STUDENT"),
            TeacherCount = await _context.Users.AsNoTracking().CountAsync(u => u.Role.RoleCode == "TEACHER"),
            AttendanceCount = await _context.Attendances.AsNoTracking().CountAsync(),
            QuizAttemptCount = await _context.QuizAttempts.AsNoTracking().CountAsync()
        };
    }

    public async Task<StudentProgressReportViewModel> GetStudentProgressReportAsync()
    {
        var students = await _context.Users.AsNoTracking()
            .Include(u => u.Role)
            .Include(u => u.StudentLearningProfile)
                .ThenInclude(p => p!.CurrentLevel)
            .Where(u => u.Role.RoleCode == "STUDENT")
            .OrderBy(u => u.FullName)
            .ToListAsync();

        var pathGroups = await _context.StudentLearningPaths.AsNoTracking()
            .GroupBy(p => p.StudentId)
            .Select(g => new
            {
                StudentId = g.Key,
                Total = g.Count(),
                Completed = g.Count(p => p.Status == "COMPLETED" || p.Status == "DONE")
            })
            .ToDictionaryAsync(x => x.StudentId);

        var progressGroups = await _context.StudentProgressSnapshots.AsNoTracking()
            .GroupBy(s => s.StudentId)
            .Select(g => new { StudentId = g.Key, Average = g.Average(s => s.ProgressPercent) })
            .ToDictionaryAsync(x => x.StudentId, x => x.Average);

        return new StudentProgressReportViewModel
        {
            Items = students.Select(student =>
            {
                pathGroups.TryGetValue(student.Id, out var paths);
                return new StudentProgressReportRowViewModel
                {
                    StudentId = student.Id,
                    StudentName = student.FullName,
                    Email = student.Email,
                    LevelName = student.StudentLearningProfile?.CurrentLevel?.Name ?? "Chưa có",
                    LearningPathCount = paths?.Total ?? 0,
                    CompletedPathCount = paths?.Completed ?? 0,
                    AverageProgress = Math.Round(progressGroups.GetValueOrDefault(student.Id), 2)
                };
            }).ToList()
        };
    }

    public async Task<TeacherActivityReportViewModel> GetTeacherActivityReportAsync()
    {
        var teachers = await _context.Users.AsNoTracking()
            .Include(u => u.Role)
            .Where(u => u.Role.RoleCode == "TEACHER")
            .OrderBy(u => u.FullName)
            .ToListAsync();

        var topicCounts = await _context.Schedules.AsNoTracking()
            .Where(s => s.TopicId != null)
            .GroupBy(s => s.TeacherId)
            .Select(g => new { TeacherId = g.Key, Count = g.Select(s => s.TopicId).Distinct().Count() })
            .ToDictionaryAsync(x => x.TeacherId, x => x.Count);

        var quizCounts = await _context.Quizzes.AsNoTracking()
            .Where(q => q.CreatedBy != null)
            .GroupBy(q => q.CreatedBy!.Value)
            .Select(g => new { TeacherId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TeacherId, x => x.Count);

        var assignmentCounts = await _context.PracticeTasks.AsNoTracking()
            .Where(t => t.CreatedBy != null)
            .GroupBy(t => t.CreatedBy!.Value)
            .Select(g => new { TeacherId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TeacherId, x => x.Count);

        var resourceCounts = await _context.ReferenceSources.AsNoTracking()
            .Where(r => r.CreatedBy != null)
            .GroupBy(r => r.CreatedBy!.Value)
            .Select(g => new { TeacherId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TeacherId, x => x.Count);

        return new TeacherActivityReportViewModel
        {
            Items = teachers.Select(teacher => new TeacherActivityReportRowViewModel
            {
                TeacherId = teacher.Id,
                TeacherName = teacher.FullName,
                TopicCount = topicCounts.GetValueOrDefault(teacher.Id),
                QuizCount = quizCounts.GetValueOrDefault(teacher.Id),
                AssignmentCount = assignmentCounts.GetValueOrDefault(teacher.Id),
                ResourceCount = resourceCounts.GetValueOrDefault(teacher.Id)
            }).ToList()
        };
    }

    public async Task<AttendanceSummaryReportViewModel> GetAttendanceSummaryAsync()
    {
        var attendances = await _context.Attendances.AsNoTracking()
            .Include(a => a.Topic)
            .ToListAsync();

        return new AttendanceSummaryReportViewModel
        {
            Items = attendances
                .GroupBy(a => a.Topic.Title)
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    var total = g.Count();
                    var present = g.Count(a => a.Status == "PRESENT" || a.Status == "LATE");
                    return new AttendanceSummaryRowViewModel
                    {
                        TopicTitle = g.Key,
                        PresentCount = g.Count(a => a.Status == "PRESENT"),
                        AbsentCount = g.Count(a => a.Status == "ABSENT"),
                        LateCount = g.Count(a => a.Status == "LATE"),
                        AttendanceRate = total == 0 ? 0 : Math.Round(present * 100m / total, 2)
                    };
                })
                .ToList()
        };
    }

    public async Task<QuizPerformanceReportViewModel> GetQuizPerformanceReportAsync()
    {
        var quizzes = await _context.Quizzes.AsNoTracking()
            .Include(q => q.Topic)
            .Include(q => q.QuizAttempts)
            .OrderBy(q => q.Title)
            .ToListAsync();

        return new QuizPerformanceReportViewModel
        {
            Items = quizzes.Select(quiz =>
            {
                var scoredAttempts = quiz.QuizAttempts.Where(a => a.Score != null).ToList();
                var passScore = quiz.PassingScore ?? 0;
                return new QuizPerformanceReportRowViewModel
                {
                    QuizId = quiz.Id,
                    QuizTitle = quiz.Title,
                    TopicTitle = quiz.Topic?.Title ?? "Chưa gắn topic",
                    AttemptCount = quiz.QuizAttempts.Count,
                    AverageScore = scoredAttempts.Count == 0 ? 0 : Math.Round(scoredAttempts.Average(a => a.Score!.Value), 2),
                    PassRate = scoredAttempts.Count == 0 ? 0 : Math.Round(scoredAttempts.Count(a => a.Score >= passScore) * 100m / scoredAttempts.Count, 2)
                };
            }).ToList()
        };
    }
}
