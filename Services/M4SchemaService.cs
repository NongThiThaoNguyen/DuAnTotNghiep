using DuAnTotNghiep.Data;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services
{
    public class M4SchemaService : IM4SchemaService
    {
        private readonly ApplicationDbContext _context;

        public M4SchemaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SchemaHealthStatusDto> GetSchemaHealthStatusAsync()
        {
            var status = new SchemaHealthStatusDto
            {
                TotalSkills = await _context.EnglishSkills.CountAsync(),
                TotalLevels = await _context.EnglishProficiencyLevels.CountAsync(),
                TotalTopics = await _context.LearningTopics.CountAsync(),
                TotalObjectives = await _context.LearningObjectives.CountAsync()
            };

            var topics = await _context.LearningTopics
                .Include(t => t.LearningObjectives)
                .ToListAsync();

            foreach (var topic in topics)
            {
                if (topic.SkillId == 0) // Assuming 0 or null if it was nullable
                {
                    status.TopicsMissingSkill.Add(topic.TopicCode ?? topic.Title);
                }

                if (topic.LevelId == null)
                {
                    status.TopicsMissingLevel.Add(topic.TopicCode ?? topic.Title);
                }

                if (!topic.LearningObjectives.Any())
                {
                    status.TopicsMissingObjective.Add(topic.TopicCode ?? topic.Title);
                }
            }

            // Detect Circular Dependencies using Kahn's algorithm or simple DFS
            var prerequisites = await _context.TopicPrerequisites.ToListAsync();
            var graph = new Dictionary<int, List<int>>();
            foreach (var p in prerequisites)
            {
                if (!graph.ContainsKey(p.PrerequisiteTopicId)) graph[p.PrerequisiteTopicId] = new List<int>();
                graph[p.PrerequisiteTopicId].Add(p.TopicId);
            }

            var visited = new HashSet<int>();
            var recursionStack = new HashSet<int>();

            foreach (var node in graph.Keys)
            {
                if (DetectCycle(node, graph, visited, recursionStack))
                {
                    status.CircularDependencies.Add($"Cycle detected involving Topic ID: {node}");
                    break;
                }
            }

            return status;
        }

        private bool DetectCycle(int node, Dictionary<int, List<int>> graph, HashSet<int> visited, HashSet<int> recursionStack)
        {
            if (recursionStack.Contains(node))
                return true;

            if (visited.Contains(node))
                return false;

            visited.Add(node);
            recursionStack.Add(node);

            if (graph.ContainsKey(node))
            {
                foreach (var neighbor in graph[node])
                {
                    if (DetectCycle(neighbor, graph, visited, recursionStack))
                        return true;
                }
            }

            recursionStack.Remove(node);
            return false;
        }
    }
}
