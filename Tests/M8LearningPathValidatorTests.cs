using DuAnTotNghiep.Models.Enums;
using DuAnTotNghiep.Helpers;
using DuAnTotNghiep.Models;
using Xunit;

namespace DuAnTotNghiep.Tests;

public class M8LearningPathValidatorTests
{
    [Fact]
    public void ValidateNodes_WhenNodesAreValid_ReturnsValid()
    {
        var nodes = new List<LearningPathNode>
        {
            CreateNode(1, NodeType.Topic),
            CreateNode(2, NodeType.Lesson),
            CreateNode(3, NodeType.Quiz)
        };

        var result = LearningPathValidator.ValidateNodes(nodes);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateNodes_WhenNodesAreInvalid_ReturnsErrors()
    {
        var nodes = new List<LearningPathNode>
        {
            CreateNode(1, NodeType.Topic),
            CreateNode(1, "BAD_TYPE")
        };

        var result = LearningPathValidator.ValidateNodes(nodes);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("OrderIndex"));
        Assert.Contains(result.Errors, error => error.Contains("NodeType"));
    }

    [Fact]
    public void ValidateNodes_WhenPathIsEmpty_ReturnsError()
    {
        var result = LearningPathValidator.ValidateNodes(new List<LearningPathNode>());

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("rỗng"));
    }

    private static LearningPathNode CreateNode(int orderIndex, string nodeType)
    {
        return new LearningPathNode
        {
            NodeTitle = $"Node {orderIndex}",
            NodeType = nodeType,
            OrderIndex = orderIndex,
            Status = ProgressStatus.Locked
        };
    }
}
