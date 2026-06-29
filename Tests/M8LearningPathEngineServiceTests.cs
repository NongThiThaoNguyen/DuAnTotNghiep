using DuAnTotNghiep.Data;
using DuAnTotNghiep.DTOs.LearningPath;
using DuAnTotNghiep.Enums;
using DuAnTotNghiep.Exceptions;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Repositories;
using DuAnTotNghiep.Services;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace DuAnTotNghiep.Tests;

public class M8LearningPathEngineServiceTests
{
    [Fact]
    public async Task CanGeneratePathAsync_WhenPrerequisitesExist_ReturnsReadyState()
    {
        using var context = CreateContext();
        SeedReadyStudent(context);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.CanGeneratePathAsync(7);

        Assert.True(result.HasOnboarding);
        Assert.True(result.HasPlacementTest);
        Assert.True(result.HasCompetencyAnalysis);
        Assert.False(result.HasActivePath);
        Assert.Equal(string.Empty, result.MissingStep);
    }

    [Fact]
    public async Task BuildInputAsync_ReturnsProfileCompetencyAndActiveCatalog()
    {
        using var context = CreateContext();
        SeedReadyStudent(context);
        SeedCatalog(context);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var input = await service.BuildInputAsync(7);

        Assert.Equal(7, input.StudentId);
        Assert.Equal("IELTS", input.GoalName);
        Assert.Equal("Intermediate", input.TargetLevelName);
        Assert.Equal("Beginner", input.CurrentLevelName);
        Assert.Equal(45, input.AvailableMinutesPerDay);
        Assert.Equal("Vocabulary", input.Strengths);
        Assert.Equal("Grammar, Listening", input.Weaknesses);
        Assert.Contains("Grammar", input.SkillPriorities);
        Assert.Equal(new[] { "Grammar", "Listening" }, input.PriorityTopics);
        Assert.Equal("Present Simple", Assert.Single(input.AvailableTopics).Name);
        Assert.Equal("Present Simple Lesson", Assert.Single(input.AvailableLessons).Name);
        Assert.Equal("Present Simple Quiz", Assert.Single(input.AvailableQuizzes).Name);
    }

    [Fact]
    public async Task GenerateInitialPathAsync_WhenOnboardingMissing_ThrowsBusinessException()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.GenerateInitialPathAsync(7, 20));

        Assert.Contains("onboarding", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GenerateInitialPathAsync_WhenPlacementTestMissing_ThrowsBusinessException()
    {
        using var context = CreateContext();
        SeedLearningProfileOnly(context);
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.GenerateInitialPathAsync(7, 20));

        Assert.Contains("placement test", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GenerateInitialPathAsync_WhenCompetencyAnalysisMissing_ThrowsBusinessException()
    {
        using var context = CreateContext();
        SeedLearningProfileOnly(context);
        SeedPlacementAttempt(context);
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.GenerateInitialPathAsync(7, 20));

        Assert.Contains("phân tích năng lực", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GenerateInitialPathAsync_WhenPrerequisitesExist_SavesPathAndNodes()
    {
        using var context = CreateContext();
        SeedReadyStudent(context);
        SeedCatalog(context);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var path = await service.GenerateInitialPathAsync(7, 20);
        var savedPath = await context.StudentLearningPaths
            .Include(p => p.LearningPathNodes)
            .FirstAsync(p => p.Id == path.Id);

        Assert.Equal(7, savedPath.StudentId);
        Assert.Equal(20, savedPath.CompetencyAnalysisId);
        Assert.Equal(LearningPathStatus.Active, savedPath.Status);
        Assert.True(savedPath.GeneratedByAi);
        Assert.NotEmpty(savedPath.LearningPathNodes);
        Assert.Equal(ProgressStatus.Available, savedPath.LearningPathNodes.OrderBy(n => n.OrderIndex).First().Status);
        Assert.All(savedPath.LearningPathNodes.OrderBy(n => n.OrderIndex).Skip(1), n => Assert.Equal(ProgressStatus.Locked, n.Status));
    }

    [Fact]
    public async Task GenerateInitialPathAsync_WhenAiSucceeds_WritesSuccessUsageLog()
    {
        using var context = CreateContext();
        SeedReadyStudent(context);
        SeedCatalog(context);
        await context.SaveChangesAsync();

        var service = CreateService(context, new FixedLearningPathAiService());

        var path = await service.GenerateInitialPathAsync(7, 20);
        var log = await context.AiUsageLogs.SingleAsync();

        Assert.Equal("AI generated path", path.Title);
        Assert.Equal("LEARNING_PATH", log.ModuleCode);
        Assert.Equal("SUCCESS", log.RequestStatus);
        Assert.Equal("mock-m8", log.AiModel);
        Assert.Null(log.ErrorMessage);
    }

    [Fact]
    public async Task GenerateInitialPathAsync_WhenAiSucceeds_SavesReasoningAndUsageMetadata()
    {
        using var context = CreateContext();
        SeedReadyStudent(context);
        SeedCatalog(context);
        SeedLearningPathPrompt(context);
        await context.SaveChangesAsync();

        var service = CreateService(context, new FixedLearningPathAiService());

        var path = await service.GenerateInitialPathAsync(7, 20);
        var savedPath = await context.StudentLearningPaths
            .Include(p => p.LearningPathNodes)
            .FirstAsync(p => p.Id == path.Id);
        var log = await context.AiUsageLogs.SingleAsync();

        Assert.Equal("AI generated summary", savedPath.AiPlanSummary);
        Assert.Equal("Priority weakness", Assert.Single(savedPath.LearningPathNodes).AiReason);
        Assert.Equal(77, log.PromptTemplateId);
        Assert.True(log.InputTokens > 0);
        Assert.True(log.OutputTokens > 0);
    }

    [Fact]
    public async Task GenerateInitialPathAsync_WhenAiFails_UsesMatchingPublishedTemplate()
    {
        using var context = CreateContext();
        SeedReadyStudent(context);
        SeedCatalog(context);
        SeedFallbackTemplate(context);
        await context.SaveChangesAsync();

        var service = CreateService(context, new ThrowingLearningPathAiService("AI timeout"));

        var path = await service.GenerateInitialPathAsync(7, 20);
        var savedPath = await context.StudentLearningPaths
            .Include(p => p.LearningPathNodes)
            .FirstAsync(p => p.Id == path.Id);
        var log = await context.AiUsageLogs.SingleAsync();

        Assert.Equal(LearningPathStatus.Active, savedPath.Status);
        Assert.False(savedPath.GeneratedByAi);
        Assert.Equal(900, savedPath.TemplateId);
        Assert.Equal("Fallback topic", Assert.Single(savedPath.LearningPathNodes).NodeTitle);
        Assert.Equal("FAILED", log.RequestStatus);
        Assert.Contains("AI timeout", log.ErrorMessage);
    }

    [Fact]
    public async Task GenerateInitialPathAsync_WhenAiFailsAndNoTemplate_SavesFailedPath()
    {
        using var context = CreateContext();
        SeedReadyStudent(context);
        SeedCatalog(context);
        await context.SaveChangesAsync();

        var service = CreateService(context, new ThrowingLearningPathAiService("AI timeout"));

        var path = await service.GenerateInitialPathAsync(7, 20);
        var savedPath = await context.StudentLearningPaths
            .Include(p => p.LearningPathNodes)
            .FirstAsync(p => p.Id == path.Id);
        var log = await context.AiUsageLogs.SingleAsync();

        Assert.Equal(LearningPathStatus.Failed, savedPath.Status);
        Assert.False(savedPath.GeneratedByAi);
        Assert.Empty(savedPath.LearningPathNodes);
        Assert.Contains("AI timeout", savedPath.AiPlanSummary);
        Assert.Equal("FAILED", log.RequestStatus);
    }

    [Fact]
    public async Task GenerateInitialPathAsync_WhenAiOutputUsesReferenceOnlyLesson_DoesNotSavePath()
    {
        using var context = CreateContext();
        SeedReadyStudent(context);
        SeedCatalog(context);
        SeedReferenceOnlyLesson(context);
        await context.SaveChangesAsync();

        var service = CreateService(
            context,
            new ReferenceOnlyLessonAiService(),
            new LearningPathComplianceService(context));

        await Assert.ThrowsAsync<BusinessException>(() => service.GenerateInitialPathAsync(7, 20));
        Assert.Empty(context.StudentLearningPaths);
    }

    [Fact]
    public async Task GetPathSummaryAsync_WhenActivePathExists_MapsProgressAndCurrentNodes()
    {
        using var context = CreateContext();
        SeedExistingPath(context);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var summary = await service.GetPathSummaryAsync(7);

        Assert.Equal(100, summary.PathId);
        Assert.Equal("M8 Existing Path", summary.Title);
        Assert.Equal(3, summary.TotalNodes);
        Assert.Equal(1, summary.CompletedNodes);
        Assert.Equal("Available node", summary.CurrentNodeTitle);
        Assert.Equal("Locked node", summary.NextNodeTitle);
        Assert.Equal(2, summary.PathVersion);
    }

    [Fact]
    public async Task GetPathDetailAsync_WhenOwnerRequestsPath_ReturnsNodes()
    {
        using var context = CreateContext();
        SeedExistingPath(context);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var detail = await service.GetPathDetailAsync(100, 7);

        Assert.Equal(100, detail.PathId);
        Assert.Equal(3, detail.Nodes.Count);
        Assert.Equal(new[] { 1, 2, 3 }, detail.Nodes.Select(n => n.OrderIndex));
    }

    [Fact]
    public async Task GetPathDetailAsync_WhenNonOwnerRequestsPath_ThrowsUnauthorizedAccessException()
    {
        using var context = CreateContext();
        SeedExistingPath(context);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GetPathDetailAsync(100, 8));
    }

    [Fact]
    public async Task ArchivePathAsync_WhenOwnerRequestsPath_SetsArchivedStatus()
    {
        using var context = CreateContext();
        SeedExistingPath(context);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        await service.ArchivePathAsync(100, 7);
        var path = await context.StudentLearningPaths.FindAsync(100);

        Assert.Equal(LearningPathStatus.Archived, path!.Status);
        Assert.NotNull(path.ArchivedAt);
    }

    [Fact]
    public async Task RegeneratePathAsync_WhenActivePathExists_ArchivesOldAndCreatesNewVersion()
    {
        using var context = CreateContext();
        SeedReadyStudent(context);
        SeedCatalog(context);
        SeedExistingPath(context);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var newPath = await service.RegeneratePathAsync(7, "Changed goal");
        var oldPath = await context.StudentLearningPaths.FindAsync(100);

        Assert.Equal(LearningPathStatus.Archived, oldPath!.Status);
        Assert.Equal(newPath.Id, oldPath.ReplacedByPathId);
        Assert.Equal(3, newPath.PathVersion);
        Assert.Equal(LearningPathStatus.Active, newPath.Status);
    }

    [Fact]
    public async Task RegeneratePathAsync_WhenActivePathExists_SavesReplanningReason()
    {
        using var context = CreateContext();
        SeedReadyStudent(context);
        SeedCatalog(context);
        SeedExistingPath(context);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var newPath = await service.RegeneratePathAsync(7, "Changed goal");
        var replanningEvent = await context.AiReplanningEvents.SingleAsync();

        Assert.Equal(7, replanningEvent.StudentId);
        Assert.Equal(newPath.Id, replanningEvent.LearningPathId);
        Assert.Equal("Changed goal", replanningEvent.Reason);
        Assert.Equal("STUDENT_REGENERATE", replanningEvent.TriggerType);
        Assert.Equal("APPLIED", replanningEvent.Status);
        Assert.Equal("Existing AI summary", replanningEvent.OldPlanSummary);
        Assert.Equal(newPath.AiPlanSummary, replanningEvent.NewPlanSummary);
    }

    [Fact]
    public async Task RegeneratePathAsync_WhenDailyLimitReached_ThrowsBusinessException()
    {
        using var context = CreateContext();
        SeedReadyStudent(context);
        SeedCatalog(context);
        SeedExistingPath(context);
        SeedReplanningEventsToday(context, 3);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        await Assert.ThrowsAsync<BusinessException>(() => service.RegeneratePathAsync(7, "Changed goal"));
        var oldPath = await context.StudentLearningPaths.FindAsync(100);
        Assert.Equal(LearningPathStatus.Active, oldPath!.Status);
    }

    [Fact]
    public async Task UnlockNextNodeAsync_WhenNextNodeIsLocked_MarksNextNodeAvailable()
    {
        using var context = CreateContext();
        SeedExistingPath(context);
        await context.SaveChangesAsync();
        var currentNode = await context.LearningPathNodes.FindAsync(202);
        currentNode!.Status = ProgressStatus.Completed;
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var unlocked = await service.UnlockNextNodeAsync(202, 7);
        var nextNode = await context.LearningPathNodes.FindAsync(203);

        Assert.True(unlocked);
        Assert.Equal(ProgressStatus.Available, nextNode!.Status);
    }

    [Fact]
    public async Task UnlockNextNodeAsync_WhenRequiredNodeIsNotCompleted_KeepsNextNodeLocked()
    {
        using var context = CreateContext();
        SeedExistingPath(context);
        await context.SaveChangesAsync();
        var currentNode = await context.LearningPathNodes.FindAsync(202);
        var nextNode = await context.LearningPathNodes.FindAsync(203);
        currentNode!.Status = ProgressStatus.Completed;
        nextNode!.RequiredNodeId = 999;
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var unlocked = await service.UnlockNextNodeAsync(202, 7);

        Assert.False(unlocked);
        Assert.Equal(ProgressStatus.Locked, nextNode.Status);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static ILearningPathEngineService CreateService(ApplicationDbContext context)
    {
        return CreateService(context, null);
    }

    private static ILearningPathEngineService CreateService(
        ApplicationDbContext context,
        ILearningPathAiService? aiService)
    {
        return CreateService(context, aiService, null);
    }

    private static ILearningPathEngineService CreateService(
        ApplicationDbContext context,
        ILearningPathAiService? aiService,
        ILearningPathComplianceService? complianceService)
    {
        var serviceType = typeof(StudentLearningPath).Assembly.GetType("DuAnTotNghiep.Services.LearningPathEngineService");
        Assert.NotNull(serviceType);

        var logger = typeof(NullLogger<>)
            .MakeGenericType(serviceType!)
            .GetField(nameof(NullLogger<object>.Instance))?
            .GetValue(null);

        if (aiService != null && complianceService != null)
        {
            return (ILearningPathEngineService)Activator.CreateInstance(
                serviceType!,
                context,
                new LearningPathRepository(context),
                logger,
                aiService,
                complianceService)!;
        }

        if (aiService != null)
        {
            return (ILearningPathEngineService)Activator.CreateInstance(
                serviceType!,
                context,
                new LearningPathRepository(context),
                logger,
                aiService)!;
        }

        return (ILearningPathEngineService)Activator.CreateInstance(
            serviceType!,
            context,
            new LearningPathRepository(context),
            logger)!;
    }

    private static void SeedReadyStudent(ApplicationDbContext context)
    {
        var now = DateTime.UtcNow;
        context.LearningGoals.Add(new LearningGoal { Id = 1, GoalCode = "IELTS", GoalName = "IELTS", IsActive = true });
        context.EnglishProficiencyLevels.AddRange(
            new EnglishProficiencyLevel { Id = 1, Code = "A1", Name = "Beginner", IsActive = true },
            new EnglishProficiencyLevel { Id = 2, Code = "B1", Name = "Intermediate", IsActive = true });
        context.StudentLearningProfiles.Add(new StudentLearningProfile
        {
            UserId = 7,
            CurrentLevelId = 1,
            TargetLevelId = 2,
            MainGoalId = 1,
            DailyStudyMinutes = 45,
            OnboardingStatus = "COMPLETED",
            CreatedAt = now,
            UpdatedAt = now
        });
        context.TestAttempts.Add(new TestAttempt
        {
            Id = 10,
            StudentId = 7,
            PlacementTestId = 1,
            StartedAt = now.AddHours(-1),
            SubmittedAt = now,
            Status = TestAttemptStatus.Graded
        });
        context.CompetencyAnalyses.Add(new CompetencyAnalysis
        {
            Id = 20,
            StudentId = 7,
            CurrentLevelId = 1,
            RecommendedLevelId = 2,
            Summary = "Needs grammar practice.",
            Strengths = "Vocabulary",
            Weaknesses = "Grammar, Listening",
            CreatedAt = now
        });
        context.EnglishSkills.Add(new EnglishSkill { Id = 3, SkillCode = "GRAMMAR", SkillName = "Grammar", IsActive = true });
        context.CompetencySkillScores.Add(new CompetencySkillScore
        {
            CompetencyAnalysisId = 20,
            SkillId = 3,
            Score = 55,
            PriorityLevel = 3,
            WeaknessNote = "Tenses"
        });
    }

    private static void SeedLearningProfileOnly(ApplicationDbContext context)
    {
        var now = DateTime.UtcNow;
        context.LearningGoals.Add(new LearningGoal { Id = 1, GoalCode = "IELTS", GoalName = "IELTS", IsActive = true });
        context.EnglishProficiencyLevels.AddRange(
            new EnglishProficiencyLevel { Id = 1, Code = "A1", Name = "Beginner", IsActive = true },
            new EnglishProficiencyLevel { Id = 2, Code = "B1", Name = "Intermediate", IsActive = true });
        context.StudentLearningProfiles.Add(new StudentLearningProfile
        {
            UserId = 7,
            CurrentLevelId = 1,
            TargetLevelId = 2,
            MainGoalId = 1,
            DailyStudyMinutes = 45,
            OnboardingStatus = "COMPLETED",
            CreatedAt = now,
            UpdatedAt = now
        });
    }

    private static void SeedPlacementAttempt(ApplicationDbContext context)
    {
        var now = DateTime.UtcNow;
        context.TestAttempts.Add(new TestAttempt
        {
            Id = 10,
            StudentId = 7,
            PlacementTestId = 1,
            StartedAt = now.AddHours(-1),
            SubmittedAt = now,
            Status = TestAttemptStatus.Graded
        });
    }

    private static void SeedCatalog(ApplicationDbContext context)
    {
        var now = DateTime.UtcNow;
        context.LearningTopics.AddRange(
            new LearningTopic
            {
                Id = 30,
                SkillId = 3,
                LevelId = 1,
                Title = "Present Simple",
                DifficultyLevel = "BEGINNER",
                Status = "ACTIVE",
                OrderIndex = 1
            },
            new LearningTopic
            {
                Id = 31,
                SkillId = 3,
                LevelId = 1,
                Title = "Archived Topic",
                DifficultyLevel = "BEGINNER",
                Status = "ARCHIVED",
                OrderIndex = 2
            });
        context.OriginalLessons.Add(new OriginalLesson
        {
            Id = 40,
            TopicId = 30,
            Title = "Present Simple Lesson",
            ContentType = "TEXT",
            SourceType = "INTERNAL",
            ReviewStatus = "APPROVED",
            CreatedAt = now,
            UpdatedAt = now
        });
        context.Quizzes.Add(new Quiz
        {
            Id = 50,
            SkillId = 3,
            TopicId = 30,
            Title = "Present Simple Quiz",
            QuizType = "TOPIC",
            Status = "PUBLISHED",
            CreatedAt = now
        });
    }

    private static void SeedFallbackTemplate(ApplicationDbContext context)
    {
        context.LearningPathTemplates.Add(new LearningPathTemplate
        {
            Id = 900,
            TemplateName = "Fallback IELTS A1 to B1",
            GoalId = 1,
            StartLevelId = 1,
            TargetLevelId = 2,
            DurationWeeks = 4,
            Description = "Template fallback summary",
            Status = "PUBLISHED",
            CreatedAt = DateTime.UtcNow,
            LearningPathTemplateNodes =
            {
                new LearningPathTemplateNode
                {
                    Id = 901,
                    TemplateId = 900,
                    TopicId = 30,
                    SkillId = 3,
                    NodeTitle = "Fallback topic",
                    NodeType = NodeType.Topic,
                    EstimatedMinutes = 25,
                    OrderIndex = 1,
                    UnlockCondition = "FIRST_NODE"
                }
            }
        });
    }

    private static void SeedReferenceOnlyLesson(ApplicationDbContext context)
    {
        context.OriginalLessons.Add(new OriginalLesson
        {
            Id = 41,
            TopicId = 30,
            Title = "Reference-only Lesson",
            ContentType = "TEXT",
            SourceType = "REFERENCE_ONLY",
            ReviewStatus = "APPROVED",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
    }

    private static void SeedLearningPathPrompt(ApplicationDbContext context)
    {
        context.AiPromptTemplates.Add(new AiPromptTemplate
        {
            Id = 77,
            PromptCode = "M8_LEARNING_PATH",
            PromptName = "M8 Learning Path",
            ModuleCode = "LEARNING_PATH",
            SystemPrompt = "Generate a learning path.",
            Status = "ACTIVE",
            VersionNo = 2,
            CreatedAt = DateTime.UtcNow
        });
    }

    private static void SeedReplanningEventsToday(ApplicationDbContext context, int count)
    {
        for (var index = 0; index < count; index++)
        {
            context.AiReplanningEvents.Add(new AiReplanningEvent
            {
                Id = 700 + index,
                StudentId = 7,
                LearningPathId = 100,
                TriggerType = "STUDENT_REGENERATE",
                Reason = $"Existing reason {index}",
                Status = "APPLIED",
                CreatedAt = DateTime.UtcNow.AddHours(-index)
            });
        }
    }

    private static void SeedExistingPath(ApplicationDbContext context)
    {
        var now = DateTime.UtcNow;
        context.Users.Add(new User
        {
            Id = 7,
            Email = "student7@test.com",
            FullName = "Student 7",
            PasswordHash = "hash",
            Status = "ACTIVE"
        });
        context.StudentLearningPaths.Add(new StudentLearningPath
        {
            Id = 100,
            StudentId = 7,
            Title = "M8 Existing Path",
            Status = LearningPathStatus.Active,
            AiPlanSummary = "Existing AI summary",
            GeneratedByAi = true,
            PathVersion = 2,
            CreatedAt = now,
            UpdatedAt = now
        });
        context.LearningPathNodes.AddRange(
            CreateExistingNode(201, 100, "Completed node", ProgressStatus.Completed, 1),
            CreateExistingNode(202, 100, "Available node", ProgressStatus.Available, 2),
            CreateExistingNode(203, 100, "Locked node", ProgressStatus.Locked, 3));
    }

    private static LearningPathNode CreateExistingNode(
        int id,
        int pathId,
        string title,
        string status,
        int orderIndex)
    {
        return new LearningPathNode
        {
            Id = id,
            LearningPathId = pathId,
            NodeTitle = title,
            NodeType = NodeType.Lesson,
            Status = status,
            OrderIndex = orderIndex,
            EstimatedMinutes = 20,
            PathPhase = "Foundation"
        };
    }

    private sealed class FixedLearningPathAiService : ILearningPathAiService
    {
        public Task<LearningPathOutputDto> GeneratePathFromAiAsync(LearningPathInputDto input)
        {
            return Task.FromResult(new LearningPathOutputDto
            {
                PathTitle = "AI generated path",
                Summary = "AI generated summary",
                TotalWeeks = 1,
                Phases =
                {
                    new LearningPathOutputPhaseDto
                    {
                        PhaseName = "Foundation",
                        Weeks = 1,
                        Nodes =
                        {
                            new LearningPathOutputNodeDto
                            {
                                NodeTitle = "AI topic",
                                NodeDescription = "Practice the priority topic.",
                                ActionType = NodeType.Topic,
                                TopicId = 30,
                                EstimatedMinutes = 20,
                                AiReason = "Priority weakness",
                                PathPhase = "Foundation"
                            }
                        }
                    }
                }
            });
        }

        public Task<(bool IsValid, string[] Errors)> ValidateAiOutputAsync(
            LearningPathOutputDto output,
            LearningPathInputDto input)
        {
            return Task.FromResult((true, Array.Empty<string>()));
        }
    }

    private sealed class ReferenceOnlyLessonAiService : ILearningPathAiService
    {
        public Task<LearningPathOutputDto> GeneratePathFromAiAsync(LearningPathInputDto input)
        {
            return Task.FromResult(new LearningPathOutputDto
            {
                PathTitle = "Reference-only path",
                Summary = "Uses a reference-only lesson.",
                TotalWeeks = 1,
                Phases =
                {
                    new LearningPathOutputPhaseDto
                    {
                        PhaseName = "Foundation",
                        Weeks = 1,
                        Nodes =
                        {
                            new LearningPathOutputNodeDto
                            {
                                NodeTitle = "Reference-only lesson",
                                NodeDescription = "Should be blocked by compliance.",
                                ActionType = NodeType.Lesson,
                                LessonId = 41,
                                EstimatedMinutes = 20,
                                AiReason = "Compliance regression",
                                PathPhase = "Foundation"
                            }
                        }
                    }
                }
            });
        }

        public Task<(bool IsValid, string[] Errors)> ValidateAiOutputAsync(
            LearningPathOutputDto output,
            LearningPathInputDto input)
        {
            return Task.FromResult((true, Array.Empty<string>()));
        }
    }

    private sealed class ThrowingLearningPathAiService : ILearningPathAiService
    {
        private readonly string _message;

        public ThrowingLearningPathAiService(string message)
        {
            _message = message;
        }

        public Task<LearningPathOutputDto> GeneratePathFromAiAsync(LearningPathInputDto input)
        {
            return Task.FromException<LearningPathOutputDto>(new TimeoutException(_message));
        }

        public Task<(bool IsValid, string[] Errors)> ValidateAiOutputAsync(
            LearningPathOutputDto output,
            LearningPathInputDto input)
        {
            return Task.FromResult((true, Array.Empty<string>()));
        }
    }
}
