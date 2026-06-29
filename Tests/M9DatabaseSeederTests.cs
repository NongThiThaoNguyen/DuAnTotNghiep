using DuAnTotNghiep.Data;
using DuAnTotNghiep.Data.Seeders;
using DuAnTotNghiep.Models.Enums;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace DuAnTotNghiep.Tests;

public class M9DatabaseSeederTests
{
    [Fact]
    public async Task SeedLearningPathDemoAsync_CreatesOneDemoPathWithTenNodesAndIsIdempotent()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        var student = new User
        {
            Id = 77,
            Email = "student1@aistudyenglish.com",
            FullName = "Student One",
            PasswordHash = "hash",
            RoleId = 3,
            Status = "ACTIVE"
        };
        var skill = new EnglishSkill
        {
            Id = 1,
            SkillCode = "GRAMMAR",
            SkillName = "Grammar",
            IsActive = true,
            OrderIndex = 1
        };
        var level = new EnglishProficiencyLevel
        {
            Id = 1,
            Code = "A2",
            Name = "Elementary",
            IsActive = true,
            OrderIndex = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Users.Add(student);
        context.EnglishSkills.Add(skill);
        context.EnglishProficiencyLevels.Add(level);
        context.PlacementTests.Add(new PlacementTest
        {
            Id = 1,
            Title = "Demo Placement",
            TargetLevelId = level.Id,
            Status = PlacementTestStatus.Published,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        for (var i = 1; i <= 3; i++)
        {
            context.LearningTopics.Add(new LearningTopic
            {
                Id = i,
                SkillId = skill.Id,
                Title = $"Demo Topic {i}",
                DifficultyLevel = "BEGINNER",
                Status = "ACTIVE",
                OrderIndex = i,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        await context.SaveChangesAsync();

        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.GetByEmailAsync("student1@aistudyenglish.com")).ReturnsAsync(student);
        var seeder = CreateSeeder(context, userRepo.Object);

        await seeder.SeedLearningPathDemoAsync();
        await seeder.SeedLearningPathDemoAsync();

        var paths = await context.StudentLearningPaths
            .Include(p => p.LearningPathNodes)
            .Where(p => p.StudentId == student.Id && p.Title == "M9 Demo Learning Path")
            .ToListAsync();

        var path = Assert.Single(paths);
        Assert.Equal("ACTIVE", path.Status);
        Assert.Equal(10, path.LearningPathNodes.Count);
        Assert.Contains(path.LearningPathNodes, n => n.Status == ProgressStatus.Completed);
        Assert.Contains(path.LearningPathNodes, n => n.Status == ProgressStatus.Available);
        Assert.Contains(path.LearningPathNodes, n => n.Status == ProgressStatus.Locked);
        Assert.Contains(path.LearningPathNodes, n => n.NodeType == NodeType.AiTutor);

        var availableNode = Assert.Single(path.LearningPathNodes, n => n.Status == ProgressStatus.Available);
        Assert.Equal(NodeType.Topic, availableNode.NodeType);
        Assert.NotNull(availableNode.TopicId);

        var attempt = Assert.Single(await context.TestAttempts.Where(a => a.StudentId == student.Id).ToListAsync());
        Assert.Equal(TestAttemptStatus.Graded, attempt.Status);
        Assert.Equal(1, attempt.PlacementTestId);
    }

    private static DatabaseSeeder CreateSeeder(ApplicationDbContext context, IUserRepository userRepository)
    {
        return new DatabaseSeeder(
            new Mock<IRoleRepository>().Object,
            userRepository,
            new Mock<IGenericRepository<LearningGoal>>().Object,
            new Mock<IGenericRepository<EnglishProficiencyLevel>>().Object,
            new Mock<IGenericRepository<EnglishSkill>>().Object,
            new Mock<IGenericRepository<StudentLearningProfile>>().Object,
            context);
    }
}
