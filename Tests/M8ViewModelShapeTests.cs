using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Tests;

public class M8ViewModelShapeTests
{
    [Fact]
    public void M8LearningPathViewModels_ExposeRequiredProperties()
    {
        var assembly = typeof(StudentLearningPath).Assembly;
        var summaryType = GetRequiredType(assembly, "DuAnTotNghiep.Models.ViewModels.LearningPath.M8.LearningPathSummaryViewModel");
        var detailType = GetRequiredType(assembly, "DuAnTotNghiep.Models.ViewModels.LearningPath.M8.LearningPathDetailViewModel");
        var generateType = GetRequiredType(assembly, "DuAnTotNghiep.Models.ViewModels.LearningPath.M8.LearningPathGenerateViewModel");
        var historyType = GetRequiredType(assembly, "DuAnTotNghiep.Models.ViewModels.LearningPath.M8.AdminPathHistoryViewModel");
        var historyItemType = GetRequiredType(assembly, "DuAnTotNghiep.Models.ViewModels.LearningPath.M8.AdminPathHistoryItemViewModel");
        var pathNodeType = GetRequiredType(assembly, "DuAnTotNghiep.Models.ViewModels.LearningPath.PathNodeViewModel");

        Assert.True(summaryType.IsAssignableFrom(detailType));
        AssertSummaryProperties(summaryType);
        AssertProperty(detailType, "Nodes", typeof(List<>).MakeGenericType(pathNodeType));
        AssertGenerateProperties(generateType);
        AssertProperty(historyType, "Paths", typeof(List<>).MakeGenericType(historyItemType));
        AssertHistoryItemProperties(historyItemType);
    }

    [Fact]
    public void StudentLearningPathViewModels_DoNotExposeRawPromptOrTechnicalLogs()
    {
        var assembly = typeof(StudentLearningPath).Assembly;
        var summaryType = GetRequiredType(assembly, "DuAnTotNghiep.Models.ViewModels.LearningPath.M8.LearningPathSummaryViewModel");
        var detailType = GetRequiredType(assembly, "DuAnTotNghiep.Models.ViewModels.LearningPath.M8.LearningPathDetailViewModel");
        var sensitiveFragments = new[] { "Prompt", "ApiKey", "Token", "ErrorMessage", "TechnicalLog" };

        AssertNoSensitiveProperties(summaryType, sensitiveFragments);
        AssertNoSensitiveProperties(detailType, sensitiveFragments);
    }

    private static void AssertSummaryProperties(Type type)
    {
        AssertProperty(type, "PathId", typeof(int));
        AssertProperty(type, "Title", typeof(string));
        AssertProperty(type, "Status", typeof(string));
        AssertProperty(type, "AiPlanSummary", typeof(string));
        AssertProperty(type, "TotalNodes", typeof(int));
        AssertProperty(type, "CompletedNodes", typeof(int));
        AssertProperty(type, "CurrentNodeTitle", typeof(string));
        AssertProperty(type, "NextNodeTitle", typeof(string));
        AssertProperty(type, "StartDate", typeof(DateOnly?));
        AssertProperty(type, "TargetEndDate", typeof(DateOnly?));
        AssertProperty(type, "PriorityTopics", typeof(List<string>));
        AssertProperty(type, "GeneratedByAi", typeof(bool));
        AssertProperty(type, "PathVersion", typeof(int));
    }

    private static void AssertGenerateProperties(Type type)
    {
        AssertProperty(type, "StudentId", typeof(int));
        AssertProperty(type, "HasOnboarding", typeof(bool));
        AssertProperty(type, "HasPlacementTest", typeof(bool));
        AssertProperty(type, "HasCompetencyAnalysis", typeof(bool));
        AssertProperty(type, "HasActivePath", typeof(bool));
        AssertProperty(type, "MissingStep", typeof(string));
    }

    private static void AssertHistoryItemProperties(Type type)
    {
        AssertProperty(type, "PathId", typeof(int));
        AssertProperty(type, "StudentName", typeof(string));
        AssertProperty(type, "Status", typeof(string));
        AssertProperty(type, "CreatedAt", typeof(DateTime));
        AssertProperty(type, "GeneratedByAi", typeof(bool));
        AssertProperty(type, "AiModel", typeof(string));
        AssertProperty(type, "ErrorMessage", typeof(string));
    }

    private static Type GetRequiredType(System.Reflection.Assembly assembly, string typeName)
    {
        var type = assembly.GetType(typeName);
        Assert.NotNull(type);
        return type!;
    }

    private static void AssertProperty(Type type, string propertyName, Type expectedType)
    {
        var property = type.GetProperty(propertyName);
        Assert.NotNull(property);
        Assert.Equal(expectedType, property!.PropertyType);
    }

    private static void AssertNoSensitiveProperties(Type type, string[] sensitiveFragments)
    {
        var propertyNames = type.GetProperties().Select(property => property.Name).ToList();
        foreach (var fragment in sensitiveFragments)
        {
            Assert.DoesNotContain(propertyNames, name => name.Contains(fragment, StringComparison.OrdinalIgnoreCase));
        }
    }
}
