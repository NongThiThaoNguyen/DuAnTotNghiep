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

    public async Task<AdminReportsIndexViewModel> GetOverviewAsync(string? period = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        DateTime start;
        DateTime end = DateTime.UtcNow;

        if (period == "7days")
        {
            start = end.AddDays(-7).Date;
        }
        else if (period == "custom" && fromDate.HasValue)
        {
            start = fromDate.Value.Date;
            if (toDate.HasValue)
            {
                end = toDate.Value.Date.AddDays(1).AddTicks(-1);
            }
        }
        else
        {
            period = "30days";
            start = end.AddDays(-30).Date;
        }

        var startDateOnly = DateOnly.FromDateTime(start);
        var endDateOnly = DateOnly.FromDateTime(end);

        var studentCount = await _context.Users.AsNoTracking().CountAsync(u => u.Role.RoleCode == "STUDENT");
        var teacherCount = await _context.Users.AsNoTracking().CountAsync(u => u.Role.RoleCode == "TEACHER");
        var attendanceCount = await _context.Attendances.AsNoTracking().CountAsync(a => a.AttendanceDate >= startDateOnly && a.AttendanceDate <= endDateOnly);
        var quizAttemptCount = await _context.QuizAttempts.AsNoTracking().CountAsync(q => q.StartedAt >= start && q.StartedAt <= end);

        // Daily quiz attempts
        var quizDaily = await _context.QuizAttempts.AsNoTracking()
            .Where(q => q.StartedAt >= start && q.StartedAt <= end)
            .GroupBy(q => q.StartedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Date, x => x.Count);

        // Daily attendances
        var attendanceDaily = await _context.Attendances.AsNoTracking()
            .Where(a => a.AttendanceDate >= startDateOnly && a.AttendanceDate <= endDateOnly)
            .GroupBy(a => a.AttendanceDate)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Date, x => x.Count);

        // Daily new students
        var studentDaily = await _context.Users.AsNoTracking()
            .Where(u => u.Role.RoleCode == "STUDENT" && u.CreatedAt >= start && u.CreatedAt <= end)
            .GroupBy(u => u.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Date, x => x.Count);

        var quizAttemptsOverTime = new List<ChartDataPoint>();
        var attendanceOverTime = new List<ChartDataPoint>();
        var newStudentsOverTime = new List<ChartDataPoint>();

        for (var dt = start.Date; dt <= end.Date; dt = dt.AddDays(1))
        {
            var label = dt.ToString("dd/MM");
            var dtOnly = DateOnly.FromDateTime(dt);
            quizAttemptsOverTime.Add(new ChartDataPoint { Label = label, Value = quizDaily.GetValueOrDefault(dt) });
            attendanceOverTime.Add(new ChartDataPoint { Label = label, Value = attendanceDaily.GetValueOrDefault(dtOnly) });
            newStudentsOverTime.Add(new ChartDataPoint { Label = label, Value = studentDaily.GetValueOrDefault(dt) });
        }

        // Course completion stats
        var totalPaths = await _context.StudentLearningPaths.AsNoTracking().CountAsync();
        var completedPaths = await _context.StudentLearningPaths.AsNoTracking()
            .CountAsync(p => p.Status == "COMPLETED" || p.Status == "DONE");
        var completionRate = totalPaths == 0 ? 0 : Math.Round(completedPaths * 100m / totalPaths, 2);

        // Top 5 students by average quiz score
        var topStudents = await _context.QuizAttempts.AsNoTracking()
            .Include(q => q.Student)
            .Where(q => q.Score != null && q.Student != null)
            .GroupBy(q => new { q.StudentId, q.Student.FullName, q.Student.Email })
            .Select(g => new TopStudentRowViewModel
            {
                StudentId = g.Key.StudentId,
                StudentName = g.Key.FullName,
                Email = g.Key.Email,
                AttemptCount = g.Count(),
                AverageScore = Math.Round(g.Average(q => q.Score!.Value), 2)
            })
            .OrderByDescending(s => s.AverageScore)
            .ThenByDescending(s => s.AttemptCount)
            .Take(5)
            .ToListAsync();

        // Teacher performance summary
        var teachers = await _context.Users.AsNoTracking()
            .Where(u => u.Role.RoleCode == "TEACHER")
            .OrderBy(u => u.FullName)
            .ToListAsync();

        var teacherTopicCounts = await _context.Schedules.AsNoTracking()
            .Where(s => s.TopicId != null)
            .GroupBy(s => s.TeacherId)
            .Select(g => new { TeacherId = g.Key, Count = g.Select(s => s.TopicId).Distinct().Count() })
            .ToDictionaryAsync(x => x.TeacherId, x => x.Count);

        var teacherQuizCounts = await _context.Quizzes.AsNoTracking()
            .Where(q => q.CreatedBy != null)
            .GroupBy(q => q.CreatedBy!.Value)
            .Select(g => new { TeacherId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TeacherId, x => x.Count);

        var teacherAssignmentCounts = await _context.PracticeTasks.AsNoTracking()
            .Where(t => t.CreatedBy != null)
            .GroupBy(t => t.CreatedBy!.Value)
            .Select(g => new { TeacherId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TeacherId, x => x.Count);

        var teacherTaskIds = await _context.PracticeTasks.AsNoTracking()
            .Where(t => t.CreatedBy != null)
            .Select(t => new { TeacherId = t.CreatedBy!.Value, TaskId = t.Id })
            .ToListAsync();

        var taskTeacherDict = teacherTaskIds.ToDictionary(x => x.TaskId, x => x.TeacherId);
        var taskIds = taskTeacherDict.Keys.ToList();

        var gradedSubmissions = await _context.PracticeSubmissions.AsNoTracking()
            .Where(s => taskIds.Contains(s.PracticeTaskId) && s.Score != null)
            .Select(s => s.PracticeTaskId)
            .ToListAsync();

        var teacherGradedCounts = gradedSubmissions
            .GroupBy(taskId => taskTeacherDict[taskId])
            .ToDictionary(g => g.Key, g => g.Count());

        var teacherPerformances = teachers.Select(t => new TeacherPerformanceSummaryRow
        {
            TeacherId = t.Id,
            TeacherName = t.FullName,
            TopicCount = teacherTopicCounts.GetValueOrDefault(t.Id),
            QuizCount = teacherQuizCounts.GetValueOrDefault(t.Id),
            AssignmentCount = teacherAssignmentCounts.GetValueOrDefault(t.Id),
            GradedSubmissionCount = teacherGradedCounts.GetValueOrDefault(t.Id)
        }).ToList();

        return new AdminReportsIndexViewModel
        {
            StudentCount = studentCount,
            TeacherCount = teacherCount,
            AttendanceCount = attendanceCount,
            QuizAttemptCount = quizAttemptCount,
            Period = period,
            FromDate = fromDate,
            ToDate = toDate,
            QuizAttemptsOverTime = quizAttemptsOverTime,
            AttendanceOverTime = attendanceOverTime,
            NewStudentsOverTime = newStudentsOverTime,
            TotalLearningPaths = totalPaths,
            CompletedLearningPaths = completedPaths,
            CourseCompletionRate = completionRate,
            TopStudents = topStudents,
            TeacherPerformances = teacherPerformances
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
