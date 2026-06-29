using DuAnTotNghiep.Models;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models.Enums;
using DuAnTotNghiep.Models.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Tests;

public class M8RepositoryShapeTests
{
    [Fact]
    public void LearningPathRepositoryInterface_DefinesRequiredMethods()
    {
        var assembly = typeof(StudentLearningPath).Assembly;
        var type = assembly.GetType("DuAnTotNghiep.Models.Repositories.Interfaces.ILearningPathRepository");
        Assert.NotNull(type);

        AssertMethod(type!, "GetActivePathByStudentIdAsync", typeof(Task<StudentLearningPath>), typeof(int));
        AssertMethod(type!, "GetPathWithNodesAsync", typeof(Task<StudentLearningPath>), typeof(int));
        AssertMethod(type!, "GetPathHistoryByStudentIdAsync", typeof(Task<List<StudentLearningPath>>), typeof(int));
        AssertMethod(type!, "AddPathAsync", typeof(Task), typeof(StudentLearningPath));
        AssertMethod(type!, "UpdatePathAsync", typeof(Task), typeof(StudentLearningPath));
        AssertMethod(type!, "AddNodesAsync", typeof(Task), typeof(IEnumerable<LearningPathNode>));
        AssertMethod(
            type!,
            "GetAllPathsPagedAsync",
            typeof(Task<ValueTuple<IEnumerable<StudentLearningPath>, int>>),
            typeof(int),
            typeof(int),
            typeof(string));
    }

    [Fact]
    public async Task LearningPathRepository_GetActivePathByStudentIdAsync_ReturnsDetachedActivePath()
    {
        using var context = CreateContext();
        context.StudentLearningPaths.AddRange(
            CreatePath(1, 10, LearningPathStatus.Active, DateTime.UtcNow),
            CreatePath(2, 10, LearningPathStatus.Archived, DateTime.UtcNow),
            CreatePath(3, 11, LearningPathStatus.Active, DateTime.UtcNow));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = CreateRepository(context);

        var path = await repository.GetActivePathByStudentIdAsync(10);

        Assert.NotNull(path);
        Assert.Equal(1, path!.Id);
        Assert.Equal(EntityState.Detached, context.Entry(path).State);
    }

    [Fact]
    public async Task LearningPathRepository_GetPathWithNodesAsync_LoadsNodes()
    {
        using var context = CreateContext();
        context.StudentLearningPaths.Add(CreatePath(5, 10, LearningPathStatus.Active, DateTime.UtcNow));
        context.LearningPathNodes.AddRange(
            CreateNode(51, 5, 2),
            CreateNode(50, 5, 1));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = CreateRepository(context);

        var path = await repository.GetPathWithNodesAsync(5);

        Assert.NotNull(path);
        Assert.Equal(2, path!.LearningPathNodes.Count);
        Assert.Equal(new[] { 1, 2 }, path.LearningPathNodes.Select(n => n.OrderIndex));
    }

    [Fact]
    public async Task LearningPathRepository_GetAllPathsPagedAsync_FiltersAndPaginates()
    {
        using var context = CreateContext();
        context.Users.AddRange(
            CreateUser(10),
            CreateUser(11),
            CreateUser(12),
            CreateUser(13));
        context.StudentLearningPaths.AddRange(
            CreatePath(1, 10, LearningPathStatus.Active, DateTime.UtcNow.AddDays(-3)),
            CreatePath(2, 11, LearningPathStatus.Active, DateTime.UtcNow.AddDays(-2)),
            CreatePath(3, 12, LearningPathStatus.Active, DateTime.UtcNow.AddDays(-1)),
            CreatePath(4, 13, LearningPathStatus.Archived, DateTime.UtcNow));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = CreateRepository(context);

        var (paths, totalCount) = await repository.GetAllPathsPagedAsync(1, 2, LearningPathStatus.Active);

        Assert.Equal(3, totalCount);
        Assert.Equal(new[] { 3, 2 }, paths.Select(p => p.Id));
    }

    private static void AssertMethod(Type type, string methodName, Type returnType, params Type[] parameterTypes)
    {
        var method = type.GetMethod(methodName, parameterTypes);
        Assert.NotNull(method);
        Assert.Equal(returnType, method!.ReturnType);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static ILearningPathRepository CreateRepository(ApplicationDbContext context)
    {
        var type = typeof(StudentLearningPath).Assembly.GetType("DuAnTotNghiep.Models.Repositories.LearningPathRepository");
        Assert.NotNull(type);

        return (ILearningPathRepository)Activator.CreateInstance(type!, context)!;
    }

    private static StudentLearningPath CreatePath(int id, int studentId, string status, DateTime createdAt)
    {
        return new StudentLearningPath
        {
            Id = id,
            StudentId = studentId,
            Title = $"Path {id}",
            Status = status,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };
    }

    private static User CreateUser(int id)
    {
        return new User
        {
            Id = id,
            Email = $"student{id}@test.com",
            FullName = $"Student {id}",
            PasswordHash = "hash",
            Status = "ACTIVE"
        };
    }

    private static LearningPathNode CreateNode(int id, int pathId, int orderIndex)
    {
        return new LearningPathNode
        {
            Id = id,
            LearningPathId = pathId,
            NodeTitle = $"Node {id}",
            NodeType = NodeType.Lesson,
            Status = ProgressStatus.Locked,
            OrderIndex = orderIndex
        };
    }
}
