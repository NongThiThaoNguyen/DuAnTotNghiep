using DuAnTotNghiep.Models.Enums;
using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Helpers;

/// <summary>
/// Validates learning path node collections before persistence.
/// </summary>
public static class LearningPathValidator
{
    public static (bool IsValid, List<string> Errors) ValidateNodes(List<LearningPathNode> nodes)
    {
        var errors = new List<string>();
        if (nodes.Count == 0) errors.Add("Lộ trình không được rỗng.");

        AddDuplicateOrderErrors(nodes, errors);
        AddNodeTypeErrors(nodes, errors);

        return (errors.Count == 0, errors);
    }

    private static void AddDuplicateOrderErrors(List<LearningPathNode> nodes, List<string> errors)
    {
        var duplicateOrders = nodes
            .GroupBy(node => node.OrderIndex)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key);

        foreach (var orderIndex in duplicateOrders)
        {
            errors.Add($"OrderIndex {orderIndex} bị trùng.");
        }
    }

    private static void AddNodeTypeErrors(List<LearningPathNode> nodes, List<string> errors)
    {
        foreach (var node in nodes.Where(node => !NodeType.IsValid(node.NodeType)))
        {
            errors.Add($"NodeType không hợp lệ tại OrderIndex {node.OrderIndex}.");
        }
    }
}
