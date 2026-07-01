using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.ViewModels.Teacher;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Services;

public class TeacherCourseService : ITeacherCourseService
{
    private readonly ApplicationDbContext _context;

    public TeacherCourseService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CourseListItemViewModel>> GetCoursesAsync(int teacherId, string? search, string? skillFilter)
    {
        var query = _context.LearningTopics
            .AsNoTracking()
            .Include(t => t.Skill)
            .Include(t => t.Level)
            .Include(t => t.OriginalLessons)
            .Where(t => t.CreatedBy == teacherId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim().ToLower();
            query = query.Where(t =>
                t.Title.ToLower().Contains(keyword) ||
                (t.Description != null && t.Description.ToLower().Contains(keyword)) ||
                (t.TopicCode != null && t.TopicCode.ToLower().Contains(keyword)));
        }

        if (!string.IsNullOrWhiteSpace(skillFilter))
        {
            var skill = skillFilter.Trim().ToLower();
            query = query.Where(t =>
                t.Skill.SkillCode.ToLower() == skill ||
                t.Skill.SkillName.ToLower() == skill ||
                t.SkillId.ToString() == skill);
        }

        var topics = await query
            .OrderBy(t => t.OrderIndex)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();

        var items = new List<CourseListItemViewModel>();
        foreach (var topic in topics)
        {
            items.Add(new CourseListItemViewModel
            {
                Id = topic.Id,
                Title = topic.Title,
                Description = topic.Description,
                SkillName = topic.Skill.SkillName,
                LevelName = topic.Level?.Name,
                LessonCount = topic.OriginalLessons.Count,
                StudentCount = await GetCourseStudentCountAsync(topic.Id),
                IsActive = topic.Status == "ACTIVE",
                DifficultyLevel = topic.DifficultyLevel,
                CreatedAt = topic.CreatedAt
            });
        }

        return items;
    }

    public async Task<CourseDetailViewModel?> GetCourseDetailAsync(int courseId)
    {
        var topic = await _context.LearningTopics
            .AsNoTracking()
            .Include(t => t.Skill)
            .Include(t => t.Level)
            .Include(t => t.OriginalLessons)
            .Include(t => t.Quizzes)
                .ThenInclude(q => q.QuizQuestions)
            .FirstOrDefaultAsync(t => t.Id == courseId);

        if (topic == null)
        {
            return null;
        }

        return new CourseDetailViewModel
        {
            Id = topic.Id,
            SkillId = topic.SkillId,
            ProficiencyLevelId = topic.LevelId,
            Title = topic.Title,
            Description = topic.Description,
            TopicCode = topic.TopicCode,
            SkillName = topic.Skill.SkillName,
            LevelName = topic.Level?.Name,
            LevelCode = topic.Level?.Code,
            DifficultyLevel = topic.DifficultyLevel,
            EstimatedMinutes = topic.EstimatedMinutes,
            OrderIndex = topic.OrderIndex,
            Status = topic.Status,
            CreatedAt = topic.CreatedAt,
            Lessons = topic.OriginalLessons
                .OrderBy(l => l.Id)
                .Select(l => new LessonSummaryViewModel
                {
                    Id = l.Id,
                    Title = l.Title,
                    OrderIndex = l.Id,
                    EstimatedMinutes = l.EstimatedMinutes,
                    Status = l.ReviewStatus
                })
                .ToList(),
            Quizzes = topic.Quizzes
                .OrderBy(q => q.Title)
                .Select(q => new QuizSummaryViewModel
                {
                    Id = q.Id,
                    Title = q.Title,
                    QuestionCount = q.QuizQuestions.Count,
                    Status = q.Status
                })
                .ToList()
        };
    }

    public async Task<int> CreateCourseAsync(CreateCourseViewModel model, int teacherId)
    {
        var now = DateTime.UtcNow;
        var topic = new LearningTopic
        {
            Title = model.Title,
            Description = model.Description,
            SkillId = model.SkillId,
            LevelId = model.ProficiencyLevelId,
            DifficultyLevel = model.DifficultyLevel,
            EstimatedMinutes = model.EstimatedMinutes,
            Status = "ACTIVE",
            CreatedBy = teacherId,
            UpdatedBy = teacherId,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.LearningTopics.Add(topic);
        await _context.SaveChangesAsync();
        return topic.Id;
    }

    public async Task UpdateCourseAsync(EditCourseViewModel model)
    {
        var topic = await _context.LearningTopics.FindAsync(model.Id);
        if (topic == null)
        {
            return;
        }

        topic.Title = model.Title;
        topic.Description = model.Description;
        topic.SkillId = model.SkillId;
        topic.LevelId = model.ProficiencyLevelId;
        topic.TopicCode = model.TopicCode;
        topic.DifficultyLevel = model.DifficultyLevel;
        topic.EstimatedMinutes = model.EstimatedMinutes;
        topic.OrderIndex = model.OrderIndex;
        topic.Status = model.IsActive ? "ACTIVE" : "INACTIVE";
        topic.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteCourseAsync(int courseId)
    {
        var topic = await _context.LearningTopics.FindAsync(courseId);
        if (topic == null)
        {
            return;
        }

        topic.Status = "INACTIVE";
        topic.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetCourseStudentCountAsync(int courseId)
    {
        return await _context.StudentProgressSnapshots
            .AsNoTracking()
            .Where(s => s.TopicId == courseId)
            .Select(s => s.StudentId)
            .Distinct()
            .CountAsync();
    }

    public async Task<List<LessonSummaryViewModel>> GetCourseLessonsAsync(int courseId)
    {
        return await _context.OriginalLessons
            .AsNoTracking()
            .Where(l => l.TopicId == courseId)
            .OrderBy(l => l.Id)
            .Select(l => new LessonSummaryViewModel
            {
                Id = l.Id,
                Title = l.Title,
                OrderIndex = l.Id,
                EstimatedMinutes = l.EstimatedMinutes,
                Status = l.ReviewStatus
            })
            .ToListAsync();
    }
}
