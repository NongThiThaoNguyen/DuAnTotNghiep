using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.ViewModels.Teacher;
using DuAnTotNghiep.Services;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Tests;

public class TeacherLessonServiceTests
{
    private static DbContextOptions<ApplicationDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task LessonCrudMethods_MapOriginalLessonsForTeacher()
    {
        var options = CreateOptions();

        using (var context = new ApplicationDbContext(options))
        {
            var skill = new EnglishSkill { Id = 1, SkillCode = "GRAMMAR", SkillName = "Grammar", IsActive = true };
            var topic = new LearningTopic
            {
                Id = 100,
                SkillId = skill.Id,
                Skill = skill,
                Title = "Grammar Basics",
                DifficultyLevel = "BASIC",
                Status = "ACTIVE",
                CreatedBy = 10
            };
            context.EnglishSkills.Add(skill);
            context.LearningTopics.Add(topic);
            context.OriginalLessons.AddRange(
                new OriginalLesson
                {
                    Id = 200,
                    TopicId = topic.Id,
                    Topic = topic,
                    Title = "Nouns",
                    Summary = "Learn nouns",
                    Content = "<p>Nouns content</p>",
                    ContentType = "HTML",
                    EstimatedMinutes = 10,
                    SourceType = "TEACHER_CREATED",
                    ReviewStatus = "APPROVED",
                    CreatedBy = 10,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    UpdatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new OriginalLesson
                {
                    Id = 201,
                    TopicId = topic.Id,
                    Topic = topic,
                    Title = "Verbs",
                    Summary = "Learn verbs",
                    Content = "<p>Verbs content</p>",
                    ContentType = "HTML",
                    EstimatedMinutes = 15,
                    SourceType = "TEACHER_CREATED",
                    ReviewStatus = "APPROVED",
                    CreatedBy = 10,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                });
            await context.SaveChangesAsync();
        }

        using (var context = new ApplicationDbContext(options))
        {
            var service = new TeacherLessonService(context);

            var lessons = await service.GetLessonsByTopicAsync(100);
            Assert.Equal(new[] { "Nouns", "Verbs" }, lessons.Select(l => l.Title));
            Assert.Equal(new[] { 1, 2 }, lessons.Select(l => l.OrderIndex));

            var detail = await service.GetLessonDetailAsync(200);
            Assert.NotNull(detail);
            Assert.Equal("Grammar Basics", detail!.TopicTitle);
            Assert.Equal("<p>Nouns content</p>", detail.Content);

            var createdId = await service.CreateLessonAsync(new CreateLessonViewModel
            {
                TopicId = 100,
                Title = "Adjectives",
                Summary = "Learn adjectives",
                Content = "<p>Adjectives content</p>",
                EstimatedMinutes = 12,
                DifficultyLevel = "BASIC"
            }, 10);

            var created = await context.OriginalLessons.FindAsync(createdId);
            Assert.NotNull(created);
            Assert.Equal("TEACHER_CREATED", created!.SourceType);
            Assert.Equal("APPROVED", created.ReviewStatus);
            Assert.Equal(10, created.CreatedBy);

            await service.UpdateLessonAsync(new EditLessonViewModel
            {
                Id = createdId,
                TopicId = 100,
                Title = "Updated Adjectives",
                Summary = "Updated",
                Content = "<p>Updated content</p>",
                EstimatedMinutes = 20,
                DifficultyLevel = "MEDIUM"
            });
            Assert.Equal("Updated Adjectives", (await context.OriginalLessons.FindAsync(createdId))!.Title);

            await service.DeleteLessonAsync(createdId);
            Assert.Null(await context.OriginalLessons.FindAsync(createdId));
        }
    }
}
