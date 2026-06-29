using DuAnTotNghiep.Data;
using DuAnTotNghiep.Data.Seeders;
using DuAnTotNghiep.Enums;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace DuAnTotNghiep.Tests;

public class M8DatabaseSeederTests
{
    [Fact]
    public async Task SeedM8LearningPathAssetsAsync_CreatesPromptAndPublishedTemplatesIdempotently()
    {
        using var context = CreateContext();
        SeedCatalog(context);
        await context.SaveChangesAsync();
        var seeder = CreateSeeder(context);

        await seeder.SeedM8LearningPathAssetsAsync();
        await seeder.SeedM8LearningPathAssetsAsync();

        var prompt = Assert.Single(await context.AiPromptTemplates
            .Where(template => template.ModuleCode == "LEARNING_PATH")
            .ToListAsync());
        var templates = await context.LearningPathTemplates
            .Include(template => template.LearningPathTemplateNodes)
            .Where(template => template.Status == "PUBLISHED")
            .ToListAsync();

        Assert.Equal("ACTIVE", prompt.Status);
        Assert.True(templates.Count >= 2);
        Assert.All(templates, template => Assert.NotEmpty(template.LearningPathTemplateNodes));
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static void SeedCatalog(ApplicationDbContext context)
    {
        var goal = new LearningGoal { Id = 1, GoalCode = "IELTS", GoalName = "IELTS", IsActive = true };
        var level = new EnglishProficiencyLevel { Id = 1, Code = "BEGINNER", Name = "Beginner", IsActive = true, OrderIndex = 1 };
        var target = new EnglishProficiencyLevel { Id = 2, Code = "INTERMEDIATE", Name = "Intermediate", IsActive = true, OrderIndex = 2 };
        var skill = new EnglishSkill { Id = 3, SkillCode = "GRAMMAR", SkillName = "Grammar", IsActive = true };
        context.LearningGoals.Add(goal);
        context.EnglishProficiencyLevels.AddRange(level, target);
        context.EnglishSkills.Add(skill);
        context.Users.Add(new User { Id = 9, Email = "admin@aistudyenglish.com", FullName = "Admin", PasswordHash = "hash", Status = "ACTIVE" });
        context.LearningTopics.AddRange(
            CreateTopic(30, skill.Id, level.Id, "Grammar foundations"),
            CreateTopic(31, skill.Id, level.Id, "Daily communication"),
            CreateTopic(32, skill.Id, target.Id, "Exam practice"));
    }

    private static LearningTopic CreateTopic(int id, int skillId, int levelId, string title)
    {
        return new LearningTopic
        {
            Id = id,
            SkillId = skillId,
            LevelId = levelId,
            Title = title,
            DifficultyLevel = "BEGINNER",
            Status = "ACTIVE",
            OrderIndex = id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static DatabaseSeeder CreateSeeder(ApplicationDbContext context)
    {
        return new DatabaseSeeder(
            new Mock<IRoleRepository>().Object,
            new Mock<IUserRepository>().Object,
            new Mock<IGenericRepository<LearningGoal>>().Object,
            new Mock<IGenericRepository<EnglishProficiencyLevel>>().Object,
            new Mock<IGenericRepository<EnglishSkill>>().Object,
            new Mock<IGenericRepository<StudentLearningProfile>>().Object,
            context);
    }
}
