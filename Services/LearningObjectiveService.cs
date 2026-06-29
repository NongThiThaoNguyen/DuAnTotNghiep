using DuAnTotNghiep.Models.DTOs.Objective;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.Repositories.Interfaces;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services
{
    public class LearningObjectiveService : ILearningObjectiveService
    {
        private readonly ILearningObjectiveRepository _objectiveRepository;
        private readonly ILearningTopicRepository _topicRepository;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LearningObjectiveService(
            ILearningObjectiveRepository objectiveRepository,
            ILearningTopicRepository topicRepository,
            IAuditService auditService,
            IHttpContextAccessor httpContextAccessor)
        {
            _objectiveRepository = objectiveRepository;
            _topicRepository = topicRepository;
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
        }

        private int? GetCurrentUserId()
        {
            var userIdStr = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdStr, out int userId) ? userId : null;
        }

        public async Task<int> AddObjectiveAsync(CreateObjectiveDto dto)
        {
            await ValidateObjectiveTextAsync(dto.ObjectiveText);
            await ValidateTopicAsync(dto.TopicId);

            var objective = new LearningObjective
            {
                TopicId = dto.TopicId,
                ObjectiveText = dto.ObjectiveText.Trim(),
                CognitiveLevel = dto.CognitiveLevel.ToString().ToUpper(),
                OrderIndex = dto.OrderIndex,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _objectiveRepository.AddAsync(objective);
            await _objectiveRepository.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserId(), "Create Objective", "LearningObjective", objective.Id, null, objective.ObjectiveText);

            return objective.Id;
        }

        public async Task UpdateObjectiveAsync(UpdateObjectiveDto dto)
        {
            var objective = await _objectiveRepository.GetByIdAsync(dto.Id);
            if (objective == null)
                throw new InvalidOperationException("Objective không tồn tại.");

            await ValidateObjectiveTextAsync(dto.ObjectiveText);

            var oldText = objective.ObjectiveText;

            objective.ObjectiveText = dto.ObjectiveText.Trim();
            objective.CognitiveLevel = dto.CognitiveLevel.ToString().ToUpper();
            objective.OrderIndex = dto.OrderIndex;
            objective.UpdatedAt = DateTime.UtcNow;

            _objectiveRepository.Update(objective);
            await _objectiveRepository.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserId(), "Update Objective", "LearningObjective", objective.Id, oldText, objective.ObjectiveText);
        }

        public async Task DeleteObjectiveAsync(int objectiveId)
        {
            var objective = await _objectiveRepository.GetByIdAsync(objectiveId);
            if (objective == null)
                throw new InvalidOperationException("Objective không tồn tại.");

            if (await _objectiveRepository.IsObjectiveUsedAsync(objectiveId))
            {
                throw new InvalidOperationException("Không thể xóa Objective vì đã được dữ liệu khác (AI, Question Bank...) tham chiếu.");
            }

            _objectiveRepository.Delete(objective);
            await _objectiveRepository.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserId(), "Delete Objective", "LearningObjective", objective.Id, objective.ObjectiveText, null);
        }

        public async Task ReorderObjectivesAsync(int topicId, List<int> orderedIds)
        {
            var objectives = await _objectiveRepository.GetByTopicAsync(topicId);
            
            // Lọc ra các Objective thực sự thuộc về Topic này trong mảng orderedIds
            var validObjectivesToUpdate = objectives.Where(o => orderedIds.Contains(o.Id)).ToList();
            
            if (!validObjectivesToUpdate.Any()) return;

            // Update OrderIndex theo đúng tuần tự index của mảng orderedIds (bắt đầu từ 1)
            foreach (var obj in validObjectivesToUpdate)
            {
                var newIndex = orderedIds.IndexOf(obj.Id) + 1; // 1-based index
                if (obj.OrderIndex != newIndex)
                {
                    obj.OrderIndex = newIndex;
                    obj.UpdatedAt = DateTime.UtcNow;
                    _objectiveRepository.Update(obj);
                }
            }

            await _objectiveRepository.SaveChangesAsync();
            await _auditService.LogAsync(GetCurrentUserId(), "Reorder Objectives", "LearningObjective", null, null, $"Reordered: {validObjectivesToUpdate.Count} objectives for topic {topicId}");
        }

        public async Task<List<ObjectiveDto>> GetObjectivesByTopicAsync(int topicId)
        {
            var objectives = await _objectiveRepository.GetByTopicAsync(topicId);
            
            return objectives.OrderBy(o => o.OrderIndex).Select(o => new ObjectiveDto
            {
                Id = o.Id,
                ObjectiveText = o.ObjectiveText,
                CognitiveLevel = o.CognitiveLevel,
                OrderIndex = o.OrderIndex
            }).ToList();
        }

        public async Task<ObjectiveDetailDto?> GetByIdAsync(int objectiveId)
        {
            var objective = await _objectiveRepository.GetByIdAsync(objectiveId);
            if (objective == null) return null;

            var topic = await _topicRepository.GetByIdAsync(objective.TopicId);

            return new ObjectiveDetailDto
            {
                Id = objective.Id,
                TopicId = objective.TopicId,
                TopicName = topic?.Title ?? string.Empty,
                ObjectiveText = objective.ObjectiveText,
                CognitiveLevel = objective.CognitiveLevel,
                OrderIndex = objective.OrderIndex
            };
        }

        // --- Private Validation Methods ---

        private Task ValidateObjectiveTextAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidOperationException("ObjectiveText không được để trống.");

            if (text.Trim().Length < 10)
                throw new InvalidOperationException("ObjectiveText phải có tối thiểu 10 ký tự.");

            return Task.CompletedTask;
        }

        private async Task ValidateTopicAsync(int topicId)
        {
            var topic = await _topicRepository.GetByIdAsync(topicId);
            if (topic == null)
                throw new InvalidOperationException("Topic không tồn tại.");

            if (topic.Status == "Archived")
                throw new InvalidOperationException("Không thể thêm Objective vào Topic đã bị Archived.");
        }
    }
}
