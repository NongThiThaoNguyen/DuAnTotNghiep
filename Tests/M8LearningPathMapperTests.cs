using DuAnTotNghiep.DTOs.LearningPath;
using DuAnTotNghiep.Enums;
using DuAnTotNghiep.Helpers;
using Xunit;

namespace DuAnTotNghiep.Tests;

public class M8LearningPathMapperTests
{
    [Fact]
    public void MapOutputToEntities_ShouldCreateActiveAiPathAndSequentialNodes()
    {
        var output = CreateOutput();

        var mapped = LearningPathMapper.MapOutputToEntities(output, studentId: 7, competencyAnalysisId: 13);

        Assert.Equal(7, mapped.Path.StudentId);
        Assert.Equal(13, mapped.Path.CompetencyAnalysisId);
        Assert.Equal(LearningPathStatus.Active, mapped.Path.Status);
        Assert.True(mapped.Path.GeneratedByAi);
        Assert.Equal(3, mapped.Nodes.Count);
        Assert.Equal(new[] { 1, 2, 3 }, mapped.Nodes.Select(node => node.OrderIndex));
        Assert.Equal(ProgressStatus.Available, mapped.Nodes[0].Status);
        Assert.All(mapped.Nodes.Skip(1), node => Assert.Equal(ProgressStatus.Locked, node.Status));
        Assert.All(mapped.Nodes, node => Assert.Same(mapped.Path, node.LearningPath));
    }

    private static LearningPathOutputDto CreateOutput()
    {
        return new LearningPathOutputDto
        {
            PathTitle = "IELTS 6.5 Path",
            Summary = "Focus on listening and speaking.",
            TotalWeeks = 4,
            Phases = new List<LearningPathOutputPhaseDto>
            {
                new()
                {
                    PhaseName = "Foundation",
                    Weeks = 2,
                    Nodes = new List<LearningPathOutputNodeDto>
                    {
                        CreateNode("Topic", NodeType.Topic, topicId: 1),
                        CreateNode("Lesson", NodeType.Lesson, lessonId: 2)
                    }
                },
                new()
                {
                    PhaseName = "Practice",
                    Weeks = 2,
                    Nodes = new List<LearningPathOutputNodeDto>
                    {
                        CreateNode("Quiz", NodeType.Quiz, quizId: 3)
                    }
                }
            }
        };
    }

    private static LearningPathOutputNodeDto CreateNode(
        string title,
        string actionType,
        int? topicId = null,
        int? lessonId = null,
        int? quizId = null)
    {
        return new LearningPathOutputNodeDto
        {
            NodeTitle = title,
            NodeDescription = $"{title} description",
            ActionType = actionType,
            TopicId = topicId,
            LessonId = lessonId,
            QuizId = quizId,
            EstimatedMinutes = 20,
            AiReason = $"{title} reason",
            ScheduledDay = 1,
            PathPhase = "Phase"
        };
    }
}
