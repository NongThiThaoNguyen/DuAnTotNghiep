using DuAnTotNghiep.Areas.Admin.ViewModels;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
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

    public async Task<TeacherManagementIndexViewModel> GetTeacherListAsync(string? keyword = null, string? status = null)
    {
        var query = _context.Users.AsNoTracking()
            .Include(u => u.Role)
            .Where(u => u.Role.RoleCode == "TEACHER");

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.Trim();
            query = query.Where(u => u.FullName.Contains(k) || u.Email.Contains(k) || (u.Phone != null && u.Phone.Contains(k)));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(u => u.Status == status);
        }

        var teachers = await query.OrderBy(u => u.FullName).ToListAsync();

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
            Keyword = keyword,
            Status = status,
            Items = teachers.Select(teacher => new TeacherListItemViewModel
            {
                TeacherId = teacher.Id,
                FullName = teacher.FullName,
                Email = teacher.Email,
                Phone = teacher.Phone ?? "",
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

        var topicIds = await _context.Schedules.AsNoTracking()
            .Where(s => s.TeacherId == teacherId && s.TopicId != null)
            .Select(s => s.TopicId!.Value)
            .Distinct()
            .ToListAsync();

        var studentCount = await _context.Enrollments.AsNoTracking()
            .Where(e => e.Classroom.TeacherId == teacherId && e.Status == "ACTIVE")
            .Select(e => e.StudentId)
            .Distinct()
            .CountAsync();

        var quizCount = await _context.Quizzes.AsNoTracking().CountAsync(q => q.CreatedBy == teacherId);
        var resourceCount = await _context.ReferenceSources.AsNoTracking().CountAsync(r => r.CreatedBy == teacherId);
        var assignmentCount = await _context.PracticeTasks.AsNoTracking()
            .CountAsync(t => t.CreatedBy == teacherId || (t.TopicId != null && topicIds.Contains(t.TopicId.Value)));

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
            CreatedAt = teacher.CreatedAt,
            LastLoginAt = teacher.LastLoginAt,
            AssignedTopicCount = topics.Count,
            QuizCount = quizCount,
            ResourceCount = resourceCount,
            StudentCount = studentCount,
            AssignmentCount = assignmentCount,
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

    public async Task<(bool Success, string Message)> CreateTeacherAsync(CreateTeacherAdminViewModel model)
    {
        var email = model.Email.Trim().ToLower();
        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email))
        {
            return (false, "Email này đã tồn tại trên hệ thống.");
        }

        var teacherRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleCode == "TEACHER");
        if (teacherRole == null)
        {
            return (false, "Role TEACHER chưa được thiết lập trong hệ thống.");
        }

        var newUser = new User
        {
            FullName = model.FullName.Trim(),
            Email = model.Email.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
            Phone = model.Phone,
            RoleId = teacherRole.Id,
            Status = "ACTIVE",
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        var profile = new UserProfile
        {
            UserId = newUser.Id,
            Gender = model.Gender,
            Country = model.Country,
            DateOfBirth = model.DateOfBirth,
            Bio = model.Bio,
            CreatedAt = DateTime.UtcNow
        };

        _context.UserProfiles.Add(profile);
        await _context.SaveChangesAsync();

        return (true, "Thêm giáo viên mới thành công.");
    }

    public async Task<EditTeacherAdminViewModel?> GetTeacherForEditAsync(int teacherId)
    {
        var teacher = await _context.Users.AsNoTracking()
            .Include(u => u.Role)
            .Include(u => u.UserProfile)
            .FirstOrDefaultAsync(u => u.Id == teacherId && u.Role.RoleCode == "TEACHER");

        if (teacher == null)
        {
            return null;
        }

        return new EditTeacherAdminViewModel
        {
            TeacherId = teacher.Id,
            FullName = teacher.FullName,
            Email = teacher.Email,
            Phone = teacher.Phone,
            Status = teacher.Status,
            Gender = teacher.UserProfile?.Gender,
            Country = teacher.UserProfile?.Country,
            DateOfBirth = teacher.UserProfile?.DateOfBirth,
            Bio = teacher.UserProfile?.Bio
        };
    }

    public async Task<(bool Success, string Message)> UpdateTeacherAsync(EditTeacherAdminViewModel model)
    {
        var teacher = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.UserProfile)
            .FirstOrDefaultAsync(u => u.Id == model.TeacherId && u.Role.RoleCode == "TEACHER");

        if (teacher == null)
        {
            return (false, "Không tìm thấy thông tin giáo viên.");
        }

        var email = model.Email.Trim().ToLower();
        if (await _context.Users.AnyAsync(u => u.Id != model.TeacherId && u.Email.ToLower() == email))
        {
            return (false, "Email này đã được sử dụng bởi tài khoản khác.");
        }

        teacher.FullName = model.FullName.Trim();
        teacher.Email = model.Email.Trim();
        teacher.Phone = model.Phone;
        teacher.Status = model.Status;

        if (teacher.UserProfile == null)
        {
            teacher.UserProfile = new UserProfile
            {
                UserId = teacher.Id,
                CreatedAt = DateTime.UtcNow
            };
            _context.UserProfiles.Add(teacher.UserProfile);
        }

        teacher.UserProfile.Gender = model.Gender;
        teacher.UserProfile.Country = model.Country;
        teacher.UserProfile.DateOfBirth = model.DateOfBirth;
        teacher.UserProfile.Bio = model.Bio;
        teacher.UserProfile.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return (true, "Cập nhật thông tin giáo viên thành công.");
    }

    public async Task<(bool Success, string Message, string NewStatus)> ToggleLockTeacherAsync(int teacherId)
    {
        var teacher = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == teacherId && u.Role.RoleCode == "TEACHER");

        if (teacher == null)
        {
            return (false, "Không tìm thấy giáo viên.", "");
        }

        string newStatus = (teacher.Status == "ACTIVE") ? "INACTIVE" : "ACTIVE";
        teacher.Status = newStatus;
        await _context.SaveChangesAsync();

        string msg = (newStatus == "ACTIVE") ? "Đã mở khóa tài khoản giáo viên." : "Đã khóa tài khoản giáo viên.";
        return (true, msg, newStatus);
    }
}
