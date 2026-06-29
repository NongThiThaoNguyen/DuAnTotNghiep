using DuAnTotNghiep.DTOs.LearningPath;
using DuAnTotNghiep.Enums;
using DuAnTotNghiep.Services;
using DuAnTotNghiep.Services.Interfaces;
using Xunit;

namespace DuAnTotNghiep.Tests;

public class M8AiServiceShapeTests
{
    [Fact]
    public void ILearningPathAiService_ShouldExposeGenerationAndValidationContracts()
    {
        var type = typeof(ILearningPathAiService);

        var generateMethod = type.GetMethod(nameof(ILearningPathAiService.GeneratePathFromAiAsync));
        var validateMethod = type.GetMethod(nameof(ILearningPathAiService.ValidateAiOutputAsync));

        Assert.NotNull(generateMethod);
        Assert.Equal(typeof(Task<LearningPathOutputDto>), generateMethod.ReturnType);
        Assert.Single(generateMethod.GetParameters());
        Assert.Equal(typeof(LearningPathInputDto), generateMethod.GetParameters()[0].ParameterType);

        Assert.NotNull(validateMethod);
        Assert.Equal(typeof(Task<(bool IsValid, string[] Errors)>), validateMethod.ReturnType);
        Assert.Equal(2, validateMethod.GetParameters().Length);
        Assert.Equal(typeof(LearningPathOutputDto), validateMethod.GetParameters()[0].ParameterType);
        Assert.Equal(typeof(LearningPathInputDto), validateMethod.GetParameters()[1].ParameterType);
    }

    [Fact]
    public async Task GeneratePathFromAiAsync_ShouldCreateThreePhasesWithValidNodeCounts()
    {
        var service = new LearningPathAiService();

        var output = await service.GeneratePathFromAiAsync(CreateInput());

        Assert.Equal(3, output.Phases.Count);
        Assert.All(output.Phases, phase => Assert.InRange(phase.Nodes.Count, 5, 10));
        Assert.All(output.Phases.SelectMany(p => p.Nodes), node =>
        {
            Assert.True(NodeType.IsValid(node.ActionType));
            Assert.True(node.EstimatedMinutes > 0);
            Assert.False(string.IsNullOrWhiteSpace(node.NodeTitle));
        });
    }

    [Fact]
    public async Task ValidateAiOutputAsync_WhenOutputIsGenerated_ReturnsValid()
    {
        var service = new LearningPathAiService();
        var input = CreateInput();
        var output = await service.GeneratePathFromAiAsync(input);

        var result = await service.ValidateAiOutputAsync(output, input);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateAiOutputAsync_WhenOutputHasInvalidValues_ReturnsErrors()
    {
        var service = new LearningPathAiService();
        var input = CreateInput();
        var output = await service.GeneratePathFromAiAsync(input);
        output.Phases[0].Nodes[0].ActionType = "BAD_TYPE";
        output.Phases[0].Nodes[1].EstimatedMinutes = 0;
        var topicNode = output.Phases.SelectMany(phase => phase.Nodes)
            .First(node => node.ActionType == NodeType.Topic);
        topicNode.TopicId = 999;

        var result = await service.ValidateAiOutputAsync(output, input);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("ActionType"));
        Assert.Contains(result.Errors, error => error.Contains("EstimatedMinutes"));
        Assert.Contains(result.Errors, error => error.Contains("TopicId"));
    }

    private static LearningPathInputDto CreateInput()
    {
        return new LearningPathInputDto
        {
            StudentId = 7,
            GoalName = "IELTS 6.5",
            TargetLevelName = "B2",
            CurrentLevelName = "B1",
            AvailableMinutesPerDay = 45,
            SkillPriorities = new List<string> { "Listening", "Speaking" },
            Strengths = "Vocabulary",
            Weaknesses = "Pronunciation",
            PriorityTopics = new List<string> { "Travel", "Work" },
            AvailableTopics = CreateResources("Topic", 1),
            AvailableLessons = CreateResources("Lesson", 101),
            AvailableQuizzes = CreateResources("Quiz", 201)
        };
    }

    private static List<LearningPathResourceDto> CreateResources(string prefix, int startId)
    {
        return Enumerable.Range(0, 6)
            .Select(index => new LearningPathResourceDto { Id = startId + index, Name = $"{prefix} {index + 1}" })
            .ToList();
    }
}
