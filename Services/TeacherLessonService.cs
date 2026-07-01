using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.ViewModels.Teacher;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Services;

public class TeacherLessonService : ITeacherLessonService
{
    private readonly ApplicationDbContext _context;

    public TeacherLessonService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<LessonListItemViewModel>> GetLessonsByTopicAsync(int topicId)
    {
        var lessons = await _context.OriginalLessons
            .AsNoTracking()
            .Include(l => l.Topic)
            .Where(l => l.TopicId == topicId)
            .OrderBy(l => l.Id)
            .ToListAsync();

        return lessons
            .Select((lesson, index) => new LessonListItemViewModel
            {
                Id = lesson.Id,
                TopicId = lesson.TopicId,
                TopicTitle = lesson.Topic.Title,
                Title = lesson.Title,
                Summary = lesson.Summary,
                EstimatedMinutes = lesson.EstimatedMinutes,
                OrderIndex = index + 1,
                Status = lesson.ReviewStatus,
                CreatedAt = lesson.CreatedAt
            })
            .ToList();
    }

    public async Task<LessonDetailViewModel?> GetLessonDetailAsync(int lessonId)
    {
        var lesson = await _context.OriginalLessons
            .AsNoTracking()
            .Include(l => l.Topic)
            .FirstOrDefaultAsync(l => l.Id == lessonId);

        if (lesson == null)
        {
            return null;
        }

        return new LessonDetailViewModel
        {
            Id = lesson.Id,
            TopicId = lesson.TopicId,
            TopicTitle = lesson.Topic.Title,
            Title = lesson.Title,
            Summary = lesson.Summary,
            Content = lesson.Content,
            ContentType = lesson.ContentType,
            EstimatedMinutes = lesson.EstimatedMinutes,
            VideoUrl = lesson.VideoUrl,
            Status = lesson.ReviewStatus,
            CreatedAt = lesson.CreatedAt,
            UpdatedAt = lesson.UpdatedAt
        };
    }

    public async Task<int> CreateLessonAsync(CreateLessonViewModel model, int teacherId)
    {
        var now = DateTime.UtcNow;
        var lesson = new OriginalLesson
        {
            TopicId = model.TopicId,
            Title = model.Title,
            Summary = model.Summary,
            Content = model.Content,
            ContentType = model.InputMethod == "UPLOAD" ? "FILE" : (!string.IsNullOrWhiteSpace(model.VideoUrl) ? "VIDEO_LINK" : "HTML"),
            EstimatedMinutes = model.EstimatedMinutes,
            VideoUrl = model.VideoUrl,
            SourceType = "TEACHER_CREATED",
            ReviewStatus = "APPROVED",
            IsAiGenerated = model.InputMethod == "AI",
            CreatedBy = teacherId,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.OriginalLessons.Add(lesson);
        await _context.SaveChangesAsync();
        return lesson.Id;
    }

    public async Task UpdateLessonAsync(EditLessonViewModel model)
    {
        var lesson = await _context.OriginalLessons.FindAsync(model.Id);
        if (lesson == null)
        {
            return;
        }

        lesson.TopicId = model.TopicId;
        lesson.Title = model.Title;
        lesson.Summary = model.Summary;
        lesson.Content = model.Content;
        lesson.ContentType = model.InputMethod == "UPLOAD" ? "FILE" : (!string.IsNullOrWhiteSpace(model.VideoUrl) ? "VIDEO_LINK" : "HTML");
        if (model.InputMethod == "AI") lesson.IsAiGenerated = true;
        lesson.EstimatedMinutes = model.EstimatedMinutes;
        lesson.VideoUrl = model.VideoUrl;
        lesson.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteLessonAsync(int lessonId)
    {
        var lesson = await _context.OriginalLessons.FindAsync(lessonId);
        if (lesson == null)
        {
            return;
        }

        _context.OriginalLessons.Remove(lesson);
        await _context.SaveChangesAsync();
    }

    public async Task ReorderLessonsAsync(int topicId, List<int> orderedLessonIds)
    {
        var lessons = await _context.OriginalLessons
            .Where(l => l.TopicId == topicId && orderedLessonIds.Contains(l.Id))
            .ToListAsync();

        var now = DateTime.UtcNow;
        foreach (var lesson in lessons)
        {
            lesson.UpdatedAt = now.AddMilliseconds(orderedLessonIds.IndexOf(lesson.Id));
        }

        await _context.SaveChangesAsync();
    }
}
