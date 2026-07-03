using DuAnTotNghiep.Areas.Admin.ViewModels;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Services;

public class AdminTeacherManagementService : IAdminTeacherManagementService
{
    private readonly ApplicationDbContext _context;

    public AdminTeacherManagementService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TeacherManagementIndexViewModel> GetTeacherListAsync()
    {
        var teachers = await _context.Users.AsNoTracking()
            .Include(u => u.Role)
            .Where(u => u.Role.RoleCode == "TEACHER")
            .OrderBy(u => u.FullName)
            .ToListAsync();

        var scheduleCounts = await _context.Schedules.AsNoTracking()
            .Where(s => s.TopicId != null)
            .GroupBy(s => s.TeacherId)
            .Select(g => new { TeacherId = g.Key, Count = g.Select(s => s.TopicId).Distinct().Count() })
            .ToDictionaryAsync(x => x.TeacherId, x => x.Count);

        var resourceCounts = await _context.ReferenceSources.AsNoTracking()
            .Where(r => r.CreatedBy != null)
            .GroupBy(r => r.CreatedBy!.Value)
            .Select(g => new { TeacherId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TeacherId, x => x.Count);

        var quizCounts = await _context.Quizzes.AsNoTracking()
            .Where(q => q.CreatedBy != null)
            .GroupBy(q => q.CreatedBy!.Value)
            .Select(g => new { TeacherId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TeacherId, x => x.Count);

        return new TeacherManagementIndexViewModel
        {
            Items = teachers.Select(teacher => new TeacherListItemViewModel
            {
                TeacherId = teacher.Id,
                FullName = teacher.FullName,
                Email = teacher.Email,
                Status = teacher.Status,
                AssignedTopicCount = scheduleCounts.GetValueOrDefault(teacher.Id),
                ResourceCount = resourceCounts.GetValueOrDefault(teacher.Id),
                QuizCount = quizCounts.GetValueOrDefault(teacher.Id),
                CreatedAt = teacher.CreatedAt
            }).ToList()
        };
    }

    public async Task<TeacherProfileAdminViewModel?> GetTeacherProfileAsync(int teacherId)
    {
        var teacher = await _context.Users.AsNoTracking()
            .Include(u => u.Role)
            .Include(u => u.UserProfile)
            .FirstOrDefaultAsync(u => u.Id == teacherId && u.Role.RoleCode == "TEACHER");

        if (teacher == null)
        {
            return null;
        }

        var topics = await _context.Schedules.AsNoTracking()
            .Include(s => s.Topic)
            .Where(s => s.TeacherId == teacherId && s.Topic != null)
            .Select(s => s.Topic!.Title)
            .Distinct()
            .OrderBy(title => title)
            .ToListAsync();

        return new TeacherProfileAdminViewModel
        {
            TeacherId = teacher.Id,
            FullName = teacher.FullName,
            Email = teacher.Email,
            Phone = teacher.Phone,
            Status = teacher.Status,
            Bio = teacher.UserProfile?.Bio,
            Gender = teacher.UserProfile?.Gender,
            Country = teacher.UserProfile?.Country,
            DateOfBirth = teacher.UserProfile?.DateOfBirth,
            AssignedTopics = topics
        };
    }

    public async Task<TeacherPerformanceAdminViewModel?> GetTeacherPerformanceAsync(int teacherId)
    {
        var teacher = await _context.Users.AsNoTracking()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == teacherId && u.Role.RoleCode == "TEACHER");

        if (teacher == null)
        {
            return null;
        }

        var topicIds = await _context.Schedules.AsNoTracking()
            .Where(s => s.TeacherId == teacherId && s.TopicId != null)
            .Select(s => s.TopicId!.Value)
            .Distinct()
            .ToListAsync();

        var studentIds = await _context.Attendances.AsNoTracking()
            .Where(a => topicIds.Contains(a.TopicId))
            .Select(a => a.StudentId)
            .Distinct()
            .ToListAsync();

        var teacherTasks = _context.PracticeTasks.AsNoTracking()
            .Where(t => t.CreatedBy == teacherId || (t.TopicId != null && topicIds.Contains(t.TopicId.Value)));

        var taskIds = await teacherTasks.Select(t => t.Id).ToListAsync();
        var gradedScores = await _context.PracticeSubmissions.AsNoTracking()
            .Where(s => taskIds.Contains(s.PracticeTaskId) && s.Score != null)
            .Select(s => s.Score!.Value)
            .ToListAsync();

        return new TeacherPerformanceAdminViewModel
        {
            TeacherId = teacher.Id,
            TeacherName = teacher.FullName,
            StudentCount = studentIds.Count,
            AssignedTopicCount = topicIds.Count,
            QuizCount = await _context.Quizzes.AsNoTracking().CountAsync(q => q.CreatedBy == teacherId),
            AssignmentCount = taskIds.Count,
            GradedSubmissionCount = gradedScores.Count,
            AverageSubmissionScore = gradedScores.Count == 0 ? 0 : Math.Round(gradedScores.Average(), 2),
            CompletedLearningPaths = await _context.StudentLearningPaths.AsNoTracking()
                .CountAsync(p => studentIds.Contains(p.StudentId) && (p.Status == "COMPLETED" || p.Status == "DONE"))
        };
    }
}
