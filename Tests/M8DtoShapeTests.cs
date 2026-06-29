using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Tests;

public class M8DtoShapeTests
{
    [Fact]
    public void LearningPathInputDto_ExposesRequiredInputProperties()
    {
        var assembly = typeof(StudentLearningPath).Assembly;
        var inputType = GetRequiredType(assembly, "DuAnTotNghiep.DTOs.LearningPath.LearningPathInputDto");
        var resourceType = GetRequiredType(assembly, "DuAnTotNghiep.DTOs.LearningPath.LearningPathResourceDto");

        AssertProperty(inputType, "StudentId", typeof(int));
        AssertProperty(inputType, "GoalName", typeof(string));
        AssertProperty(inputType, "TargetLevelName", typeof(string));
        AssertProperty(inputType, "CurrentLevelName", typeof(string));
        AssertProperty(inputType, "AvailableMinutesPerDay", typeof(int));
        AssertProperty(inputType, "Strengths", typeof(string));
        AssertProperty(inputType, "Weaknesses", typeof(string));
        AssertProperty(inputType, "SkillPriorities", typeof(List<string>));
        AssertProperty(inputType, "PriorityTopics", typeof(List<string>));
        AssertProperty(inputType, "AvailableTopics", typeof(List<>).MakeGenericType(resourceType));
        AssertProperty(inputType, "AvailableLessons", typeof(List<>).MakeGenericType(resourceType));
        AssertProperty(inputType, "AvailableQuizzes", typeof(List<>).MakeGenericType(resourceType));
    }

    [Fact]
    public void LearningPathOutputDto_ExposesRequiredAiOutputSchema()
    {
        var assembly = typeof(StudentLearningPath).Assembly;
        var outputType = GetRequiredType(assembly, "DuAnTotNghiep.DTOs.LearningPath.LearningPathOutputDto");
        var phaseType = GetRequiredType(assembly, "DuAnTotNghiep.DTOs.LearningPath.LearningPathOutputPhaseDto");
        var nodeType = GetRequiredType(assembly, "DuAnTotNghiep.DTOs.LearningPath.LearningPathOutputNodeDto");

        AssertProperty(outputType, "PathTitle", typeof(string));
        AssertProperty(outputType, "Summary", typeof(string));
        AssertProperty(outputType, "TotalWeeks", typeof(int));
        AssertProperty(outputType, "Phases", typeof(List<>).MakeGenericType(phaseType));
        AssertProperty(phaseType, "PhaseName", typeof(string));
        AssertProperty(phaseType, "Weeks", typeof(int));
        AssertProperty(phaseType, "Nodes", typeof(List<>).MakeGenericType(nodeType));
        AssertProperty(nodeType, "NodeTitle", typeof(string));
        AssertProperty(nodeType, "NodeDescription", typeof(string));
        AssertProperty(nodeType, "ActionType", typeof(string));
        AssertProperty(nodeType, "TopicId", typeof(int?));
        AssertProperty(nodeType, "LessonId", typeof(int?));
        AssertProperty(nodeType, "QuizId", typeof(int?));
        AssertProperty(nodeType, "PracticeTaskId", typeof(int?));
        AssertProperty(nodeType, "EstimatedMinutes", typeof(int));
        AssertProperty(nodeType, "AiReason", typeof(string));
        AssertProperty(nodeType, "ScheduledDay", typeof(int?));
        AssertProperty(nodeType, "PathPhase", typeof(string));
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
}
