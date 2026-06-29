using DuAnTotNghiep.Models.DTOs.LearningPath;
using DuAnTotNghiep.Models.Enums;
using DuAnTotNghiep.Models.Exceptions;
using DuAnTotNghiep.Services.Interfaces;

namespace DuAnTotNghiep.Services
{
    /// <summary>
    /// Provides mock AI generation and validation for M8 learning paths.
    /// </summary>
    public class LearningPathAiService : ILearningPathAiService
    {
        private const int PhaseCount = 3;
        private const int NodesPerPhase = 6;

        private static readonly string[] PhaseNames = { "Foundation", "Practice", "Mastery" };

        public Task<LearningPathOutputDto> GeneratePathFromAiAsync(LearningPathInputDto input)
        {
            if (!HasAnyResource(input))
            {
                throw new BusinessException("Không có nội dung học tập khả dụng để tạo lộ trình.");
            }

            // Mock generation point: replace this block with Gemini API output parsing later.
            var phases = Enumerable.Range(0, PhaseCount)
                .Select(phaseIndex => BuildPhase(input, phaseIndex))
                .ToList();

            var output = new LearningPathOutputDto
            {
                PathTitle = BuildTitle(input),
                Summary = BuildSummary(input),
                TotalWeeks = PhaseCount * 2,
                Phases = phases
            };

            return Task.FromResult(output);
        }

        public Task<(bool IsValid, string[] Errors)> ValidateAiOutputAsync(
            LearningPathOutputDto output,
            LearningPathInputDto input)
        {
            var errors = new List<string>();
            var nodes = output.Phases.SelectMany(phase => phase.Nodes).ToList();

            for (var index = 0; index < nodes.Count; index++)
            {
                ValidateNode(nodes[index], input, index + 1, errors);
            }

            return Task.FromResult((errors.Count == 0, errors.ToArray()));
        }

        private static LearningPathOutputPhaseDto BuildPhase(LearningPathInputDto input, int phaseIndex)
        {
            var phaseName = PhaseNames[phaseIndex];
            var nodes = Enumerable.Range(0, NodesPerPhase)
                .Select(nodeIndex => BuildNode(input, phaseIndex, nodeIndex, phaseName))
                .ToList();

            return new LearningPathOutputPhaseDto { PhaseName = phaseName, Weeks = 2, Nodes = nodes };
        }

        private static LearningPathOutputNodeDto BuildNode(
            LearningPathInputDto input,
            int phaseIndex,
            int nodeIndex,
            string phaseName)
        {
            var sequenceNumber = phaseIndex * NodesPerPhase + nodeIndex;
            var actionType = SelectActionType(input, sequenceNumber);
            var resource = SelectResource(input, actionType, sequenceNumber);

            return new LearningPathOutputNodeDto
            {
                NodeTitle = $"{phaseName}: {resource.Name}",
                NodeDescription = BuildNodeDescription(input, actionType),
                ActionType = actionType,
                TopicId = actionType == NodeType.Topic ? resource.Id : null,
                LessonId = actionType == NodeType.Lesson ? resource.Id : null,
                QuizId = actionType == NodeType.Quiz ? resource.Id : null,
                EstimatedMinutes = Math.Clamp(input.AvailableMinutesPerDay / 2, 15, 45),
                AiReason = BuildAiReason(input, phaseName),
                ScheduledDay = sequenceNumber + 1,
                PathPhase = phaseName
            };
        }

        private static string SelectActionType(LearningPathInputDto input, int index)
        {
            var availableTypes = new[] { NodeType.Topic, NodeType.Lesson, NodeType.Quiz }
                .Where(type => GetResources(input, type).Count > 0)
                .ToList();

            return availableTypes[index % availableTypes.Count];
        }

        private static LearningPathResourceDto SelectResource(
            LearningPathInputDto input,
            string actionType,
            int index)
        {
            var resources = GetResources(input, actionType);
            return resources[index % resources.Count];
        }

        private static List<LearningPathResourceDto> GetResources(
            LearningPathInputDto input,
            string actionType)
        {
            return actionType switch
            {
                NodeType.Topic => input.AvailableTopics,
                NodeType.Lesson => input.AvailableLessons,
                NodeType.Quiz => input.AvailableQuizzes,
                _ => new List<LearningPathResourceDto>()
            };
        }

        private static bool HasAnyResource(LearningPathInputDto input)
        {
            return input.AvailableTopics.Count > 0
                || input.AvailableLessons.Count > 0
                || input.AvailableQuizzes.Count > 0;
        }

        private static string BuildTitle(LearningPathInputDto input)
        {
            return $"{input.GoalName} learning path".Trim();
        }

        private static string BuildSummary(LearningPathInputDto input)
        {
            var focus = string.Join(", ", input.PriorityTopics.DefaultIfEmpty("core English skills"));
            return $"Focus on {focus} from {input.CurrentLevelName} toward {input.TargetLevelName}.";
        }

        private static string BuildNodeDescription(LearningPathInputDto input, string actionType)
        {
            var skill = input.SkillPriorities.FirstOrDefault() ?? "English";
            return $"{actionType} activity for {skill} improvement.";
        }

        private static string BuildAiReason(LearningPathInputDto input, string phaseName)
        {
            var weakness = string.IsNullOrWhiteSpace(input.Weaknesses) ? "priority skills" : input.Weaknesses;
            return $"{phaseName} phase targets {weakness}.";
        }

        private static void ValidateNode(
            LearningPathOutputNodeDto node,
            LearningPathInputDto input,
            int index,
            List<string> errors)
        {
            if (!NodeType.IsValid(node.ActionType)) errors.Add($"Node {index}: ActionType không hợp lệ.");
            if (node.EstimatedMinutes <= 0) errors.Add($"Node {index}: EstimatedMinutes phải lớn hơn 0.");
            ValidateTypedReference(node, input, index, errors);
        }

        private static void ValidateTypedReference(
            LearningPathOutputNodeDto node,
            LearningPathInputDto input,
            int index,
            List<string> errors)
        {
            if (node.ActionType == NodeType.Topic) ValidateReference(node.TopicId, input.AvailableTopics, "TopicId", index, errors);
            if (node.ActionType == NodeType.Lesson) ValidateReference(node.LessonId, input.AvailableLessons, "LessonId", index, errors);
            if (node.ActionType == NodeType.Quiz) ValidateReference(node.QuizId, input.AvailableQuizzes, "QuizId", index, errors);
        }

        private static void ValidateReference(
            int? id,
            List<LearningPathResourceDto> resources,
            string fieldName,
            int index,
            List<string> errors)
        {
            if (!id.HasValue || resources.All(resource => resource.Id != id.Value))
            {
                errors.Add($"Node {index}: {fieldName} không tồn tại trong input.");
            }
        }
    }
}
