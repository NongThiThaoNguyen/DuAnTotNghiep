using DuAnTotNghiep.DTOs.Common;
using DuAnTotNghiep.DTOs.Topic;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Repositories.Interfaces;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
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
        private readonly ApplicationDbContext _db;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IReferenceSourcePolicyService _policyService;


        public LearningTopicService(
            ILearningTopicRepository topicRepository,
            ITopicPrerequisiteRepository prerequisiteRepository,
            IM4ValidationService validationService,
            IEnglishSkillRepository skillRepository,
            IEnglishProficiencyLevelRepository levelRepository,
            ApplicationDbContext db,
            IAuditService auditService,
            IHttpContextAccessor httpContextAccessor,
            IReferenceSourcePolicyService policyService)
        {
            _topicRepository = topicRepository;
            _prerequisiteRepository = prerequisiteRepository;
            _validationService = validationService;
            _skillRepository = skillRepository;
            _levelRepository = levelRepository;
            _db = db;
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
            _policyService = policyService;
        }

        private int? GetCurrentUserId()
        {
            var userIdStr = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdStr, out int userId) ? userId : null;
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

            await _auditService.LogAsync(GetCurrentUserId(), "Create Topic", "LearningTopic", topic.Id, null, topic.Title);

            // Log Audit Link Topic if parent is attached
            if (topic.ParentTopicId.HasValue)
            {
                await _auditService.LogAsync(GetCurrentUserId(), "Link Topic", "LearningTopic", topic.Id, null, $"Parent: {topic.ParentTopicId}");
            }

            return topic.Id;
        }

        public async Task UpdateTopicAsync(UpdateTopicDto dto)
        {
            var topic = await _topicRepository.GetByIdAsync(dto.Id);
            if (topic == null)
                throw new InvalidOperationException("Topic không tồn tại.");

            // SkillId can't be changed typically, so we validate existing ones
            await ValidateTopicCoreAsync(dto.TopicCode, dto.Name, topic.SkillId, dto.LevelId, dto.DifficultyLevel, dto.ParentTopicId, dto.Id);

            var oldTitle = topic.Title;
            var oldParentId = topic.ParentTopicId;

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

            // Log Audit actions
            await _auditService.LogAsync(GetCurrentUserId(), "Update Topic", "LearningTopic", topic.Id, oldTitle, topic.Title);

            if (oldParentId != topic.ParentTopicId)
            {
                await _auditService.LogAsync(GetCurrentUserId(), "Link Topic", "LearningTopic", topic.Id, oldParentId?.ToString(), topic.ParentTopicId?.ToString());
            }
        }

        public async Task DeactivateTopicAsync(int topicId)
        {
            var topic = await _topicRepository.GetByIdAsync(topicId);
            if (topic == null)
                throw new InvalidOperationException("Topic không tồn tại.");

            var isPrereqForActiveTopics = await _db.TopicPrerequisites
                .Include(p => p.Topic)
                .AnyAsync(p => p.PrerequisiteTopicId == topicId && (p.Topic.Status == "Active" || p.Topic.Status == "ACTIVE"));
            if (isPrereqForActiveTopics)
            {
                throw new InvalidOperationException("Không thể khóa Topic này vì đang là điều kiện tiên quyết (prerequisite) của một Topic khác đang hoạt động.");
            }

            topic.Status = "Inactive";
            topic.UpdatedAt = DateTime.UtcNow;
            
            _topicRepository.Update(topic);
            await _topicRepository.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserId(), "Deactivate Topic", "LearningTopic", topicId, "Active", "Inactive");
        }

        public async Task ArchiveTopicAsync(int topicId)
        {
            var topic = await _topicRepository.GetByIdAsync(topicId);
            if (topic == null)
                throw new InvalidOperationException("Topic không tồn tại.");

            var isPrereqForActiveTopics = await _db.TopicPrerequisites
                .Include(p => p.Topic)
                .AnyAsync(p => p.PrerequisiteTopicId == topicId && (p.Topic.Status == "Active" || p.Topic.Status == "ACTIVE"));
            if (isPrereqForActiveTopics)
            {
                throw new InvalidOperationException("Không thể lưu trữ Topic này vì đang là điều kiện tiên quyết (prerequisite) của một Topic khác đang hoạt động.");
            }

            topic.Status = "Archived";
            topic.UpdatedAt = DateTime.UtcNow;

            _topicRepository.Update(topic);
            await _topicRepository.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserId(), "Archive Topic", "LearningTopic", topicId, "Active", "Archived");
        }

        public async Task RestoreTopicAsync(int topicId)
        {
            var topic = await _topicRepository.GetByIdAsync(topicId);
            if (topic == null)
                throw new InvalidOperationException("Topic không tồn tại.");

            string oldStatus = topic.Status;
            topic.Status = "Active";
            topic.UpdatedAt = DateTime.UtcNow;

            _topicRepository.Update(topic);
            await _topicRepository.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserId(), "Restore Topic", "LearningTopic", topicId, oldStatus, "Active");
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
            await _auditService.LogAsync(GetCurrentUserId(), "Reorder Topics", "LearningTopic", null, null, $"Reordered: {validTopics.Count} topics");
        }

        public async Task<TopicDetailDto?> GetDetailAsync(int topicId)
        {
            var topic = await _topicRepository.GetTopicWithPrerequisitesAsync(topicId);
            if (topic == null) return null;

            var prerequisites = await _db.TopicPrerequisites
                .AsNoTracking()
                .Include(p => p.PrerequisiteTopic)
                .Where(p => p.TopicId == topicId)
                .Select(p => new TopicOptionDto
                {
                    Id = p.PrerequisiteTopicId,
                    Name = p.PrerequisiteTopic.Title
                })
                .ToListAsync();

            var objectives = await _db.LearningObjectives
                .AsNoTracking()
                .Where(o => o.TopicId == topicId)
                .OrderBy(o => o.OrderIndex)
                .Select(o => new DuAnTotNghiep.DTOs.Objective.ObjectiveDto
                {
                    Id = o.Id,
                    ObjectiveText = o.ObjectiveText,
                    CognitiveLevel = o.CognitiveLevel,
                    OrderIndex = o.OrderIndex
                })
                .ToListAsync();

            var lessons = await _db.OriginalLessons
                .AsNoTracking()
                .Where(l => l.TopicId == topicId)
                .Select(l => new LessonDto
                {
                    Id = l.Id,
                    Title = l.Title,
                    ContentType = l.ContentType,
                    EstimatedMinutes = l.EstimatedMinutes,
                    ReviewStatus = l.ReviewStatus,
                    IsAiGenerated = l.IsAiGenerated
                })
                .ToListAsync();

            var quizzes = await _db.Quizzes
                .AsNoTracking()
                .Where(q => q.TopicId == topicId)
                .Select(q => new QuizDto
                {
                    Id = q.Id,
                    Title = q.Title,
                    Description = q.Description,
                    QuizType = q.QuizType,
                    TimeLimitMinutes = q.TimeLimitMinutes,
                    PassingScore = q.PassingScore,
                    Status = q.Status
                })
                .ToListAsync();

            var learningPaths = await _db.LearningPathNodes
                .AsNoTracking()
                .Include(n => n.LearningPath)
                .ThenInclude(p => p.Student)
                .Where(n => n.TopicId == topicId)
                .Select(n => n.LearningPath)
                .Distinct()
                .Select(p => new LearningPathDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    Status = p.Status,
                    StudentName = p.Student != null ? p.Student.FullName : string.Empty
                })
                .ToListAsync();

            var questionCount = await _db.QuestionBanks.CountAsync(q => q.TopicId == topicId);

            var placementTestCount = await _db.PlacementTestQuestions
                .Include(pq => pq.Question)
                .Include(pq => pq.Section)
                .Where(pq => pq.Question.TopicId == topicId)
                .Select(pq => pq.Section.PlacementTestId)
                .Distinct()
                .CountAsync();

            var isPrereqForActiveTopics = await _db.TopicPrerequisites
                .Include(p => p.Topic)
                .AnyAsync(p => p.PrerequisiteTopicId == topicId && (p.Topic.Status == "Active" || p.Topic.Status == "ACTIVE"));

            bool canArchive = !isPrereqForActiveTopics;
            bool canDeactivate = !isPrereqForActiveTopics;
            string? reason = isPrereqForActiveTopics
                ? "Topic này đang là điều kiện tiên quyết (prerequisite) của một Topic khác đang hoạt động."
                : null;

            var rawReferences = await _db.TopicReferences
                .AsNoTracking()
                .Include(tr => tr.ReferenceSource)
                .Where(tr => tr.TopicId == topicId)
                .ToListAsync();

            var references = new List<TopicReferenceDto>();
            foreach (var tr in rawReferences)
            {
                var policyResult = await _policyService.CheckSourceBeforeUseAsync(tr.ReferenceSourceId);
                references.Add(new TopicReferenceDto
                {
                    Id = tr.Id,
                    ReferenceSourceId = tr.ReferenceSourceId,
                    SourceName = tr.ReferenceSource != null ? tr.ReferenceSource.SourceName : string.Empty,
                    SourceUrl = tr.ReferenceSource != null ? tr.ReferenceSource.SourceUrl : string.Empty,
                    SourceType = tr.ReferenceSource != null ? tr.ReferenceSource.SourceType.ToString() : string.Empty,
                    LicenseNote = tr.ReferenceSource != null ? tr.ReferenceSource.LicenseNote : string.Empty,
                    Note = tr.Note,
                    IsValid = policyResult.IsValid,
                    ValidationLevel = policyResult.Level,
                    ValidationMessage = policyResult.Message,
                    SourceStatus = tr.ReferenceSource != null ? tr.ReferenceSource.Status.ToString() : string.Empty
                });
            }

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
                DifficultyLevel = topic.DifficultyLevel,
                ObjectiveCount = objectives.Count,
                LessonCount = lessons.Count,
                QuizCount = quizzes.Count,
                PathCount = learningPaths.Count,
                QuestionCount = questionCount,
                PlacementTestCount = placementTestCount,
                CanArchive = canArchive,
                CanDeactivate = canDeactivate,
                Reason = reason,
                Status = topic.Status,
                IsActive = topic.Status == "Active",
                Prerequisites = prerequisites,
                Objectives = objectives,
                Lessons = lessons,
                Quizzes = quizzes,
                LearningPaths = learningPaths,
                References = references
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

        // ---------- NEW METHODS ----------
        public async Task<List<TopicOptionDto>> GetActiveTopicsBySkillAsync(int skillId)
        {
            var topics = await _topicRepository.GetBySkillAsync(skillId);
            return topics
                .Where(t => t.Status == "Active")
                .Select(t => new TopicOptionDto { Id = t.Id, Name = t.Title })
                .OrderBy(t => t.Name)
                .ToList();
        }

        public async Task<List<TopicOptionDto>> GetTopicsForQuestionAsync(int skillId, int? levelId)
        {
            var allTopics = await _topicRepository.GetAllAsync();
            var query = allTopics.AsQueryable()
                .Where(t => t.SkillId == skillId && t.Status == "Active");
            if (levelId.HasValue)
                query = query.Where(t => t.LevelId == levelId.Value);
            return query
                .Select(t => new TopicOptionDto { Id = t.Id, Name = t.Title })
                .OrderBy(t => t.Name)
                .ToList();
        }

        public async Task<bool> IsTopicAvailableAsync(int topicId)
        {
            var topic = await _topicRepository.GetByIdAsync(topicId);
            if (topic == null || topic.Status != "Active")
                return false;

            var inUse = await _db.QuestionBanks
                .AnyAsync(q => q.TopicId == topicId && q.ReviewStatus == "APPROVED");
            return !inUse;
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

            var allowedDifficulties = new[] { "BEGINNER", "ELEMENTARY", "INTERMEDIATE", "UPPER_INTERMEDIATE", "ADVANCED" };
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

        public async Task<List<OriginalLesson>> GetLessonsByTopicAsync(int topicId)
        {
            return await _db.OriginalLessons
                .AsNoTracking()
                .Where(l => l.TopicId == topicId)
                .ToListAsync();
        }

        public async Task<List<Quiz>> GetQuizzesByTopicAsync(int topicId)
        {
            return await _db.Quizzes
                .AsNoTracking()
                .Where(q => q.TopicId == topicId)
                .ToListAsync();
        }

        public async Task<List<StudentLearningPath>> GetLearningPathsByTopicAsync(int topicId)
        {
            return await _db.LearningPathNodes
                .AsNoTracking()
                .Include(n => n.LearningPath)
                .Where(n => n.TopicId == topicId)
                .Select(n => n.LearningPath)
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<LearningTopic>> GetEligibleTopicsForPathAsync()
        {
            return await _db.LearningTopics
                .AsNoTracking()
                .Include(t => t.Skill)
                .Include(t => t.Level)
                .Where(t => t.Status == "Active" || t.Status == "Published" || t.Status == "ACTIVE" || t.Status == "PUBLISHED")
                .ToListAsync();
        }

        public async Task<List<LearningTopic>> GetActiveTopicsAsync()
        {
            return await _db.LearningTopics
                .AsNoTracking()
                .Include(t => t.Skill)
                .Include(t => t.Level)
                .Where(t => t.Status == "Active" || t.Status == "ACTIVE")
                .ToListAsync();
        }

        public async Task<AiTopicPayloadDto?> GetAiPayloadForTopicAsync(int topicId)
        {
            var topic = await _db.LearningTopics
                .AsNoTracking()
                .Include(t => t.Skill)
                .Include(t => t.Level)
                .FirstOrDefaultAsync(t => t.Id == topicId);

            if (topic == null) return null;

            if (topic.Status != "Active" && topic.Status != "Published" && topic.Status != "ACTIVE" && topic.Status != "PUBLISHED")
            {
                return null;
            }

            var objectives = await _db.LearningObjectives
                .AsNoTracking()
                .Where(o => o.TopicId == topicId)
                .OrderBy(o => o.OrderIndex)
                .Select(o => o.ObjectiveText)
                .ToListAsync();

            var prerequisites = await _db.TopicPrerequisites
                .AsNoTracking()
                .Include(p => p.PrerequisiteTopic)
                .Where(p => p.TopicId == topicId)
                .Select(p => p.PrerequisiteTopic.Title)
                .ToListAsync();

            var lessons = await _db.OriginalLessons
                .AsNoTracking()
                .Where(l => l.TopicId == topicId)
                .Select(l => l.Title)
                .ToListAsync();

            var quizzes = await _db.Quizzes
                .AsNoTracking()
                .Where(q => q.TopicId == topicId)
                .Select(q => q.Title)
                .ToListAsync();

            return new AiTopicPayloadDto
            {
                TopicId = topic.Id,
                TopicCode = topic.TopicCode ?? string.Empty,
                Title = topic.Title,
                Difficulty = topic.DifficultyLevel,
                SkillName = topic.Skill?.SkillName ?? string.Empty,
                LevelName = topic.Level?.Name ?? string.Empty,
                Objectives = objectives,
                Prerequisites = prerequisites,
                Lessons = lessons,
                Quizzes = quizzes
            };
        }

        public async Task UpdateLessonTopicAsync(int lessonId, int topicId)
        {
            var lesson = await _db.OriginalLessons.FirstOrDefaultAsync(l => l.Id == lessonId);
            if (lesson == null) throw new InvalidOperationException("Lesson không tồn tại.");

            var oldTopicId = lesson.TopicId;
            if (oldTopicId != topicId)
            {
                lesson.TopicId = topicId;
                lesson.UpdatedAt = DateTime.UtcNow;
                _db.OriginalLessons.Update(lesson);
                await _db.SaveChangesAsync();

                await _auditService.LogAsync(GetCurrentUserId(), "Update Lesson Topic", "OriginalLesson", lessonId, oldTopicId.ToString(), topicId.ToString());
            }
        }

        public async Task UpdateQuizTopicAsync(int quizId, int topicId)
        {
            var quiz = await _db.Quizzes.FirstOrDefaultAsync(q => q.Id == quizId);
            if (quiz == null) throw new InvalidOperationException("Quiz không tồn tại.");

            var oldTopicId = quiz.TopicId;
            if (oldTopicId != topicId)
            {
                quiz.TopicId = topicId;
                _db.Quizzes.Update(quiz);
                await _db.SaveChangesAsync();

                await _auditService.LogAsync(GetCurrentUserId(), "Update Quiz Topic", "Quiz", quizId, oldTopicId?.ToString(), topicId.ToString());
            }
        }

        public async Task UpdateLearningPathNodeTopicAsync(int nodeId, int topicId)
        {
            var node = await _db.LearningPathNodes.FirstOrDefaultAsync(n => n.Id == nodeId);
            if (node == null) throw new InvalidOperationException("Learning Path Node không tồn tại.");

            var oldTopicId = node.TopicId;
            if (oldTopicId != topicId)
            {
                node.TopicId = topicId;
                _db.LearningPathNodes.Update(node);
                await _db.SaveChangesAsync();

                await _auditService.LogAsync(GetCurrentUserId(), "Update Learning Path Topic", "LearningPathNode", nodeId, oldTopicId?.ToString(), topicId.ToString());
            }
        }

        public async Task LinkTopicPrerequisiteAsync(int topicId, int prerequisiteTopicId)
        {
            if (topicId == prerequisiteTopicId)
                throw new InvalidOperationException("Topic không thể làm prerequisite của chính nó.");

            var exists = await _db.TopicPrerequisites.AnyAsync(p => p.TopicId == topicId && p.PrerequisiteTopicId == prerequisiteTopicId);
            if (!exists)
            {
                var prereq = new TopicPrerequisite
                {
                    TopicId = topicId,
                    PrerequisiteTopicId = prerequisiteTopicId,
                    CreatedAt = DateTime.UtcNow
                };
                _db.TopicPrerequisites.Add(prereq);
                await _db.SaveChangesAsync();

                await _auditService.LogAsync(GetCurrentUserId(), "Link Topic", "TopicPrerequisite", prereq.Id, null, $"Topic: {topicId}, Prerequisite: {prerequisiteTopicId}");
            }
        }
    }
}
