using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.ViewModels.Teacher;
using DuAnTotNghiep.Services;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Tests;

public class TeacherCourseServiceTests
{
    private static DbContextOptions<ApplicationDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task CourseCrudMethods_MapLearningTopicsForTeacher()
    {
        var options = CreateOptions();

        using (var context = new ApplicationDbContext(options))
        {
            var skill = new EnglishSkill { Id = 1, SkillCode = "READING", SkillName = "Reading", IsActive = true };
            var otherSkill = new EnglishSkill { Id = 2, SkillCode = "WRITING", SkillName = "Writing", IsActive = true };
            var level = new EnglishProficiencyLevel { Id = 1, Code = "A1", Name = "Beginner", IsActive = true };
            var teacher = new User { Id = 10, Email = "teacher@test.com", FullName = "Teacher", PasswordHash = "hash", RoleId = 2, Status = "ACTIVE" };
            var student = new User { Id = 20, Email = "student@test.com", FullName = "Student", PasswordHash = "hash", RoleId = 3, Status = "ACTIVE" };
            var topic = new LearningTopic
            {
                Id = 100,
                SkillId = skill.Id,
                Skill = skill,
                LevelId = level.Id,
                Level = level,
                Title = "Reading Starter",
                Description = "Skimming and scanning",
                DifficultyLevel = "BASIC",
                EstimatedMinutes = 45,
                Status = "ACTIVE",
                CreatedBy = teacher.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            };
            var otherTeacherTopic = new LearningTopic
            {
                Id = 101,
                SkillId = otherSkill.Id,
                Skill = otherSkill,
                LevelId = level.Id,
                Level = level,
                Title = "Writing Starter",
                DifficultyLevel = "BASIC",
                Status = "ACTIVE",
                CreatedBy = 99
            };

            context.EnglishSkills.AddRange(skill, otherSkill);
            context.EnglishProficiencyLevels.Add(level);
            context.Users.AddRange(teacher, student);
            context.LearningTopics.AddRange(topic, otherTeacherTopic);
            context.OriginalLessons.Add(new OriginalLesson
            {
                Id = 200,
                TopicId = topic.Id,
                Topic = topic,
                Title = "Main idea",
                ContentType = "HTML",
                SourceType = "MANUAL",
                ReviewStatus = "APPROVED",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                EstimatedMinutes = 15
            });
            context.Quizzes.Add(new Quiz
            {
                Id = 300,
                TopicId = topic.Id,
                Topic = topic,
                SkillId = skill.Id,
                Skill = skill,
                Title = "Reading Check",
                QuizType = "PRACTICE",
                Status = "ACTIVE",
                CreatedBy = teacher.Id
            });
            context.StudentProgressSnapshots.Add(new StudentProgressSnapshot
            {
                Id = 400,
                StudentId = student.Id,
                Student = student,
                TopicId = topic.Id,
                Topic = topic,
                ProgressPercent = 25,
                TotalStudyMinutes = 20,
                CompletedNodes = 1,
                SnapshotDate = DateOnly.FromDateTime(DateTime.Today)
            });
            await context.SaveChangesAsync();
        }

        using (var context = new ApplicationDbContext(options))
        {
            var service = new TeacherCourseService(context);

            var courses = await service.GetCoursesAsync(10, "reading", "READING");
            var course = Assert.Single(courses);
            Assert.Equal(100, course.Id);
            Assert.Equal("Reading", course.SkillName);
            Assert.Equal(1, course.LessonCount);
            Assert.Equal(1, course.StudentCount);

            var detail = await service.GetCourseDetailAsync(100);
            Assert.NotNull(detail);
            Assert.Equal("Reading Starter", detail!.Title);
            Assert.Single(detail.Lessons);
            Assert.Single(detail.Quizzes);

            var createdId = await service.CreateCourseAsync(new CreateCourseViewModel
            {
                Title = "New Course",
                Description = "Created from teacher service",
                SkillId = 1,
                ProficiencyLevelId = 1,
                DifficultyLevel = "MEDIUM",
                EstimatedMinutes = 60
            }, 10);

            var created = await context.LearningTopics.FindAsync(createdId);
            Assert.NotNull(created);
            Assert.Equal(10, created!.CreatedBy);
            Assert.Equal("ACTIVE", created.Status);

            await service.UpdateCourseAsync(new EditCourseViewModel
            {
                Id = createdId,
                Title = "Updated Course",
                Description = "Updated",
                SkillId = 1,
                ProficiencyLevelId = 1,
                DifficultyLevel = "ADVANCED",
                EstimatedMinutes = 90,
                IsActive = true
            });
            Assert.Equal("Updated Course", (await context.LearningTopics.FindAsync(createdId))!.Title);

            await service.DeleteCourseAsync(createdId);
            Assert.Equal("INACTIVE", (await context.LearningTopics.FindAsync(createdId))!.Status);
        }
    }
}
