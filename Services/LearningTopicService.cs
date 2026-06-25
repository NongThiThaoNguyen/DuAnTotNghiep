using DuAnTotNghiep.DTOs.Common;
using DuAnTotNghiep.DTOs.Topic;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Repositories.Interfaces;
using DuAnTotNghiep.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services
{
    public class LearningTopicService : ILearningTopicService
    {
        private readonly ILearningTopicRepository _topicRepository;
        private readonly ITopicPrerequisiteRepository _prerequisiteRepository;
        private readonly IM4ValidationService _validationService;
        private readonly IEnglishSkillRepository _skillRepository;
        private readonly IEnglishProficiencyLevelRepository _levelRepository;

        public LearningTopicService(
            ILearningTopicRepository topicRepository,
            ITopicPrerequisiteRepository prerequisiteRepository,
            IM4ValidationService validationService,
            IEnglishSkillRepository skillRepository,
            IEnglishProficiencyLevelRepository levelRepository)
        {
            _topicRepository = topicRepository;
            _prerequisiteRepository = prerequisiteRepository;
            _validationService = validationService;
            _skillRepository = skillRepository;
            _levelRepository = levelRepository;
        }

        public async Task<int> CreateTopicAsync(CreateTopicDto dto)
        {
            await ValidateTopicCoreAsync(dto.TopicCode, dto.Name, dto.SkillId, dto.LevelId, dto.DifficultyLevel, dto.ParentTopicId, null);

            var topic = new LearningTopic
            {
                TopicCode = dto.TopicCode.Trim(),
                Title = dto.Name.Trim(),
                Description = dto.Description,
                SkillId = dto.SkillId,
                LevelId = dto.LevelId,
                ParentTopicId = dto.ParentTopicId,
                DifficultyLevel = dto.DifficultyLevel,
                OrderIndex = dto.OrderIndex,
                Status = "Active", // Vòng đời: DRAFT, ACTIVE, INACTIVE, ARCHIVED. Default Active.
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _topicRepository.AddAsync(topic);
            await _topicRepository.SaveChangesAsync();

            return topic.Id;
        }

        public async Task UpdateTopicAsync(UpdateTopicDto dto)
        {
            var topic = await _topicRepository.GetByIdAsync(dto.Id);
            if (topic == null)
                throw new InvalidOperationException("Topic không tồn tại.");

            // SkillId can't be changed typically, so we validate existing ones
            await ValidateTopicCoreAsync(dto.TopicCode, dto.Name, topic.SkillId, dto.LevelId, dto.DifficultyLevel, dto.ParentTopicId, dto.Id);

            if (topic.TopicCode.ToLower() != dto.TopicCode.ToLower())
            {
                if (await _topicRepository.IsTopicUsedAsync(dto.Id))
                {
                    throw new InvalidOperationException("Không thể sửa TopicCode vì Topic đã được tham chiếu.");
                }
                topic.TopicCode = dto.TopicCode.Trim();
            }

            topic.Title = dto.Name.Trim();
            topic.Description = dto.Description;
            topic.LevelId = dto.LevelId;
            topic.ParentTopicId = dto.ParentTopicId;
            topic.DifficultyLevel = dto.DifficultyLevel;
            topic.OrderIndex = dto.OrderIndex;
            topic.Status = dto.Status;
            topic.UpdatedAt = DateTime.UtcNow;

            _topicRepository.Update(topic);
            await _topicRepository.SaveChangesAsync();
        }

        public async Task DeactivateTopicAsync(int topicId)
        {
            var topic = await _topicRepository.GetByIdAsync(topicId);
            if (topic == null)
                throw new InvalidOperationException("Topic không tồn tại.");

            topic.Status = "Inactive";
            topic.UpdatedAt = DateTime.UtcNow;
            
            // Deactivate implies setting status to Inactive. It does not hard delete.
            _topicRepository.Update(topic);
            await _topicRepository.SaveChangesAsync();
        }

        public async Task ArchiveTopicAsync(int topicId)
        {
            var topic = await _topicRepository.GetByIdAsync(topicId);
            if (topic == null)
                throw new InvalidOperationException("Topic không tồn tại.");

            if (await _topicRepository.IsTopicUsedAsync(topicId))
            {
                // We still archive but UI should display warning before this.
                // It's allowed by spec, just need a warning.
            }

            topic.Status = "Archived";
            topic.UpdatedAt = DateTime.UtcNow;

            _topicRepository.Update(topic);
            await _topicRepository.SaveChangesAsync();
        }

        public async Task ReorderTopicsAsync(List<int> orderedIds)
        {
            var allTopics = await _topicRepository.GetAllAsync();
            var validTopics = allTopics.Where(t => orderedIds.Contains(t.Id)).ToList();
            if (!validTopics.Any()) return;

            foreach (var topic in validTopics)
            {
                var newIndex = orderedIds.IndexOf(topic.Id) + 1;
                if (topic.OrderIndex != newIndex)
                {
                    topic.OrderIndex = newIndex;
                    topic.UpdatedAt = DateTime.UtcNow;
                    _topicRepository.Update(topic);
                }
            }

            await _topicRepository.SaveChangesAsync();
        }

        public async Task<TopicDetailDto?> GetDetailAsync(int topicId)
        {
            var topic = await _topicRepository.GetTopicWithPrerequisitesAsync(topicId);
            if (topic == null) return null;

            return new TopicDetailDto
            {
                Id = topic.Id,
                TopicCode = topic.TopicCode ?? string.Empty,
                Name = topic.Title,
                Description = topic.Description,
                SkillName = topic.Skill?.SkillName ?? string.Empty,
                LevelName = topic.Level?.Name ?? string.Empty,
                ParentTopicName = topic.ParentTopic?.Title,
                Difficulty = topic.DifficultyLevel,
                ObjectiveCount = topic.LearningObjectives?.Count ?? 0,
                Status = topic.Status,
                IsActive = topic.Status == "Active"
            };
        }

        public async Task<List<TopicTreeDto>> GetTopicTreeAsync()
        {
            var allTopics = await _topicRepository.GetAllTopicsWithDetailsAsync();
            return BuildTree(allTopics.ToList());
        }

        public async Task<List<TopicTreeDto>> GetTopicTreeBySkillAsync(int skillId)
        {
            var allTopics = await _topicRepository.GetAllTopicsWithDetailsAsync();
            var skillTopics = allTopics.Where(t => t.SkillId == skillId).ToList();
            return BuildTree(skillTopics);
        }

        public async Task<PagedResult<TopicListItemDto>> SearchTopicsAsync(TopicSearchRequest request)
        {
            var topics = await _topicRepository.SearchTopicsAsync(request.Keyword, request.SkillId, request.LevelId);

            if (!string.IsNullOrWhiteSpace(request.DifficultyLevel))
                topics = topics.Where(t => t.DifficultyLevel == request.DifficultyLevel).ToList();

            if (!string.IsNullOrWhiteSpace(request.Status))
                topics = topics.Where(t => t.Status == request.Status).ToList();

            if (request.ParentTopicId.HasValue)
                topics = topics.Where(t => t.ParentTopicId == request.ParentTopicId.Value).ToList();

            var totalCount = topics.Count;
            var pagedTopics = topics.Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();

            return new PagedResult<TopicListItemDto>
            {
                TotalCount = totalCount,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                Items = pagedTopics.Select(t => new TopicListItemDto
                {
                    Id = t.Id,
                    TopicCode = t.TopicCode ?? string.Empty,
                    Name = t.Title,
                    SkillName = t.Skill?.SkillName ?? string.Empty,
                    LevelName = t.Level?.Name ?? string.Empty,
                    Difficulty = t.DifficultyLevel,
                    ObjectiveCount = t.LearningObjectives?.Count ?? 0,
                    Status = t.Status
                }).ToList()
            };
        }

        public async Task<List<TopicOptionDto>> GetActiveTopicOptionsAsync()
        {
            var allTopics = await _topicRepository.GetAllAsync();
            return allTopics.Where(t => t.Status == "Active")
                            .Select(t => new TopicOptionDto { Id = t.Id, Name = t.Title })
                            .OrderBy(t => t.Name).ToList();
        }

        public async Task<bool> IsTopicUsedAsync(int topicId)
        {
            return await _topicRepository.IsTopicUsedAsync(topicId);
        }

        public async Task<List<int>> GetPrerequisiteChainAsync(int topicId)
        {
            var chain = new List<int>();
            var allPrerequisites = await _prerequisiteRepository.GetAllAsync();
            var dict = allPrerequisites.GroupBy(p => p.TopicId)
                                       .ToDictionary(g => g.Key, g => g.Select(p => p.PrerequisiteTopicId).ToList());

            BuildPrerequisiteChainRecursive(topicId, dict, chain, new HashSet<int>());
            chain.Reverse();
            return chain;
        }

        public async Task<bool> HasCircularParentAsync(int topicId, int? parentId)
        {
            return await _topicRepository.HasCircularParentAsync(topicId, parentId);
        }

        public async Task<bool> HasCircularPrerequisiteAsync(int topicId, int prerequisiteId)
        {
            if (topicId == prerequisiteId) return true;

            var allPrerequisites = await _prerequisiteRepository.GetAllAsync();
            var dict = allPrerequisites.GroupBy(p => p.TopicId)
                                       .ToDictionary(g => g.Key, g => g.Select(p => p.PrerequisiteTopicId).ToList());

            if (!dict.ContainsKey(topicId))
                dict[topicId] = new List<int>();
            dict[topicId].Add(prerequisiteId);

            return HasCycleInPrerequisites(topicId, dict, new HashSet<int>(), new HashSet<int>());
        }

        // --- Private Methods ---

        private async Task ValidateTopicCoreAsync(string topicCode, string name, int skillId, int? levelId, string difficultyLevel, int? parentTopicId, int? currentTopicId)
        {
            if (string.IsNullOrWhiteSpace(topicCode) || string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("TopicCode và Name không được để trống.");

            var allowedDifficulties = new[] { "BASIC", "MEDIUM", "ADVANCED" };
            if (!allowedDifficulties.Contains(difficultyLevel?.ToUpper()))
                throw new InvalidOperationException("Difficulty Level không hợp lệ.");

            if (parentTopicId.HasValue && parentTopicId.Value == currentTopicId)
                throw new InvalidOperationException("Topic không thể là Parent của chính nó.");

            if (currentTopicId.HasValue && await HasCircularParentAsync(currentTopicId.Value, parentTopicId))
                throw new InvalidOperationException("Thiết lập Parent này sẽ tạo ra vòng lặp.");

            var allTopics = await _topicRepository.GetAllAsync();
            
            // Check TopicCode Duplicate
            if (allTopics.Any(t => t.TopicCode?.ToLower() == topicCode.ToLower() && t.Id != currentTopicId))
                throw new InvalidOperationException("TopicCode đã tồn tại.");

            // Check Skill
            var skill = await _skillRepository.GetByIdAsync(skillId);
            if (skill == null || !skill.IsActive)
                throw new InvalidOperationException("Skill không tồn tại hoặc đã bị khóa.");

            // Check Level
            if (levelId.HasValue)
            {
                var level = await _levelRepository.GetByIdAsync(levelId.Value);
                if (level == null || !level.IsActive)
                    throw new InvalidOperationException("Level không tồn tại hoặc đã bị khóa.");
            }
        }

        private List<TopicTreeDto> BuildTree(List<LearningTopic> topics)
        {
            var topicDict = topics.ToDictionary(t => t.Id, t => new TopicTreeDto
            {
                Id = t.Id,
                TopicCode = t.TopicCode ?? string.Empty,
                Name = t.Title,
                ParentTopicId = t.ParentTopicId,
                OrderIndex = t.OrderIndex,
                Status = t.Status,
                PrerequisiteTopicIds = t.TopicPrerequisites?.Select(p => p.PrerequisiteTopicId).ToList() ?? new List<int>()
            });

            var rootNodes = new List<TopicTreeDto>();

            foreach (var topic in topics.OrderBy(t => t.OrderIndex))
            {
                var dto = topicDict[topic.Id];

                if (topic.ParentTopicId.HasValue && topicDict.ContainsKey(topic.ParentTopicId.Value))
                {
                    topicDict[topic.ParentTopicId.Value].Children.Add(dto);
                    topicDict[topic.ParentTopicId.Value].Children = topicDict[topic.ParentTopicId.Value].Children.OrderBy(c => c.OrderIndex).ToList();
                }
                else
                {
                    rootNodes.Add(dto);
                }
            }

            return rootNodes.OrderBy(r => r.OrderIndex).ToList();
        }

        private void BuildPrerequisiteChainRecursive(int topicId, Dictionary<int, List<int>> dict, List<int> chain, HashSet<int> visited)
        {
            if (visited.Contains(topicId)) return;
            visited.Add(topicId);

            if (dict.TryGetValue(topicId, out var prereqIds))
            {
                foreach (var prereqId in prereqIds)
                {
                    BuildPrerequisiteChainRecursive(prereqId, dict, chain, visited);
                    if (!chain.Contains(prereqId))
                        chain.Add(prereqId);
                }
            }
        }

        private bool HasCycleInPrerequisites(int current, Dictionary<int, List<int>> dict, HashSet<int> visited, HashSet<int> recursionStack)
        {
            if (recursionStack.Contains(current)) return true;
            if (visited.Contains(current)) return false;

            visited.Add(current);
            recursionStack.Add(current);

            if (dict.TryGetValue(current, out var neighbors))
            {
                foreach (var neighbor in neighbors)
                {
                    if (HasCycleInPrerequisites(neighbor, dict, visited, recursionStack))
                        return true;
                }
            }

            recursionStack.Remove(current);
            return false;
        }
    }
}
