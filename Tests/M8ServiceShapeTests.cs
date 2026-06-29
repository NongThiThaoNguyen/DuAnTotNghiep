using DuAnTotNghiep.DTOs.LearningPath;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.ViewModels.LearningPath.M8;

namespace DuAnTotNghiep.Tests;

public class M8ServiceShapeTests
{
    [Fact]
    public void LearningPathEngineServiceInterface_DefinesRequiredMethods()
    {
        var assembly = typeof(StudentLearningPath).Assembly;
        var type = assembly.GetType("DuAnTotNghiep.Services.Interfaces.ILearningPathEngineService");
        Assert.NotNull(type);

        AssertMethod(type!, "BuildInputAsync", typeof(Task<LearningPathInputDto>), typeof(int));
        AssertMethod(type!, "GenerateInitialPathAsync", typeof(Task<StudentLearningPath>), typeof(int), typeof(int));
        AssertMethod(type!, "GetActivePathAsync", typeof(Task<StudentLearningPath>), typeof(int));
        AssertMethod(type!, "GetPathDetailAsync", typeof(Task<LearningPathDetailViewModel>), typeof(int), typeof(int));
        AssertMethod(type!, "GetPathSummaryAsync", typeof(Task<LearningPathSummaryViewModel>), typeof(int));
        AssertMethod(type!, "ArchivePathAsync", typeof(Task), typeof(int), typeof(int));
        AssertMethod(type!, "RegeneratePathAsync", typeof(Task<StudentLearningPath>), typeof(int), typeof(string));
        AssertMethod(type!, "CanGeneratePathAsync", typeof(Task<LearningPathGenerateViewModel>), typeof(int));
    }

    [Fact]
    public void Program_ShouldRegisterLearningPathComplianceService()
    {
        var programPath = ProjectFile("Program.cs");
        var content = File.ReadAllText(programPath);

        Assert.Contains(
            "AddScoped<ILearningPathComplianceService, LearningPathComplianceService>",
            content);
    }

    private static void AssertMethod(Type type, string methodName, Type returnType, params Type[] parameterTypes)
    {
        var method = type.GetMethod(methodName, parameterTypes);
        Assert.NotNull(method);
        Assert.Equal(returnType, method!.ReturnType);
    }

    private static string ProjectFile(params string[] segments)
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        return Path.Combine(new[] { root }.Concat(segments).ToArray());
    }
}
