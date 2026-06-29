using DuAnTotNghiep.Data;
using DuAnTotNghiep.Enums;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Tests;

public class M8ComplianceTests
{
    [Fact]
    public async Task ValidateContentComplianceAsync_WhenTopicInactive_ReturnsViolation()
    {
        using var context = CreateContext();
        context.LearningTopics.Add(CreateTopic(30, "ARCHIVED"));
        await context.SaveChangesAsync();
        var service = new LearningPathComplianceService(context);

        var result = await service.ValidateContentComplianceAsync(new List<LearningPathNode>
        {
            new() { TopicId = 30, NodeTitle = "Archived topic", NodeType = NodeType.Topic }
        });

        Assert.False(result.IsCompliant);
        Assert.Contains(result.Violations, violation => violation.Contains("Topic"));
    }

    [Fact]
    public async Task ValidateContentComplianceAsync_WhenQuizUsesReferenceOnlyQuestion_ReturnsViolation()
    {
        using var context = CreateContext();
        SeedSkill(context);
        context.Quizzes.Add(CreateQuiz(50, "PUBLISHED"));
        context.QuestionBanks.Add(CreateQuestion(70, "REFERENCE_ONLY"));
        context.QuizQuestions.Add(new QuizQuestion { Id = 80, QuizId = 50, QuestionId = 70, Points = 1, OrderIndex = 1 });
        await context.SaveChangesAsync();
        var service = new LearningPathComplianceService(context);

        var result = await service.ValidateContentComplianceAsync(new List<LearningPathNode>
        {
            new() { QuizId = 50, NodeTitle = "Reference-only quiz", NodeType = NodeType.Quiz }
        });

        Assert.False(result.IsCompliant);
        Assert.Contains(result.Violations, violation => violation.Contains("REFERENCE_ONLY"));
    }

    [Fact]
    public async Task ValidateContentComplianceAsync_WhenQuizIsNotPublished_ReturnsViolation()
    {
        using var context = CreateContext();
        SeedSkill(context);
        context.Quizzes.Add(CreateQuiz(50, "DRAFT"));
        await context.SaveChangesAsync();
        var service = new LearningPathComplianceService(context);

        var result = await service.ValidateContentComplianceAsync(new List<LearningPathNode>
        {
            new() { QuizId = 50, NodeTitle = "Draft quiz", NodeType = NodeType.Quiz }
        });

        Assert.False(result.IsCompliant);
        Assert.Contains(result.Violations, violation => violation.Contains("Quiz"));
    }

    [Fact]
    public async Task ValidateContentComplianceAsync_WhenContentIsApproved_ReturnsCompliant()
    {
        using var context = CreateContext();
        SeedSkill(context);
        context.LearningTopics.Add(CreateTopic(30, "ACTIVE"));
        context.OriginalLessons.Add(CreateLesson(40, "APPROVED", "SELF_CREATED"));
        context.Quizzes.Add(CreateQuiz(50, "PUBLISHED"));
        await context.SaveChangesAsync();
        var service = new LearningPathComplianceService(context);

        var result = await service.ValidateContentComplianceAsync(new List<LearningPathNode>
        {
            new() { TopicId = 30, NodeTitle = "Topic", NodeType = NodeType.Topic },
            new() { LessonId = 40, NodeTitle = "Lesson", NodeType = NodeType.Lesson },
            new() { QuizId = 50, NodeTitle = "Quiz", NodeType = NodeType.Quiz }
        });

        Assert.True(result.IsCompliant);
        Assert.Empty(result.Violations);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static void SeedSkill(ApplicationDbContext context)
    {
        context.EnglishSkills.Add(new EnglishSkill { Id = 3, SkillCode = "GRAMMAR", SkillName = "Grammar", IsActive = true });
    }

    private static LearningTopic CreateTopic(int id, string status)
    {
        return new LearningTopic
        {
            Id = id,
            SkillId = 3,
            Title = $"Topic {id}",
            DifficultyLevel = "BEGINNER",
            Status = status,
            OrderIndex = 1
        };
    }

    private static OriginalLesson CreateLesson(int id, string reviewStatus, string sourceType)
    {
        return new OriginalLesson
        {
            Id = id,
            TopicId = 30,
            Title = $"Lesson {id}",
            ContentType = "TEXT",
            SourceType = sourceType,
            ReviewStatus = reviewStatus,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static Quiz CreateQuiz(int id, string status)
    {
        return new Quiz
        {
            Id = id,
            SkillId = 3,
            Title = $"Quiz {id}",
            QuizType = "TOPIC",
            Status = status,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static QuestionBank CreateQuestion(int id, string sourceType)
    {
        return new QuestionBank
        {
            Id = id,
            SkillId = 3,
            QuestionType = "MCQ",
            QuestionText = $"Question {id}",
            DifficultyLevel = "BEGINNER",
            SourceType = sourceType,
            ReviewStatus = "APPROVED",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
