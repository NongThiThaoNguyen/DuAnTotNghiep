using DuAnTotNghiep.Models.DTOs.Level;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.Repositories.Interfaces;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services
{
    public class EnglishProficiencyLevelService : IEnglishProficiencyLevelService
    {
        private readonly IEnglishProficiencyLevelRepository _repository;
        private readonly IM4ValidationService _validationService;
        private readonly ILearningTopicRepository _topicRepository;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EnglishProficiencyLevelService(
            IEnglishProficiencyLevelRepository repository, 
            IM4ValidationService validationService,
            ILearningTopicRepository topicRepository,
            IAuditService auditService,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _validationService = validationService;
            _topicRepository = topicRepository;
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
        }

        private int? GetCurrentUserId()
        {
            var userIdStr = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdStr, out int userId) ? userId : null;
        }

        public async Task<List<LevelOptionDto>> GetOptionsAsync()
        {
            var levels = await _repository.GetActiveLevelsAsync();
            return levels.Select(l => new LevelOptionDto
            {
                Id = l.Id,
                Code = l.Code,
                Name = l.Name
            }).ToList();
        }

        public async Task<List<LevelListItemDto>> GetListAsync()
        {
            var levels = await _repository.GetAllAsync();
            return levels.Select(l => new LevelListItemDto
            {
                Id = l.Id,
                Code = l.Code,
                Name = l.Name,
                OrderIndex = l.OrderIndex,
                IsActive = l.IsActive
            }).OrderBy(l => l.OrderIndex).ToList();
        }

        public async Task<LevelDetailDto?> GetByIdAsync(int id)
        {
            var level = await _repository.GetByIdAsync(id);
            if (level == null) return null;

            return new LevelDetailDto
            {
                Id = level.Id,
                Code = level.Code,
                Name = level.Name,
                Description = level.Description,
                OrderIndex = level.OrderIndex,
                IsActive = level.IsActive
            };
        }

        public async Task CreateAsync(CreateLevelDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name))
                throw new InvalidOperationException("Code and Name are required.");

            if (dto.OrderIndex < 0)
                throw new InvalidOperationException("OrderIndex cannot be negative.");

            if (await _validationService.IsLevelCodeDuplicateAsync(dto.Code))
                throw new InvalidOperationException("Level code đã tồn tại.");

            var level = new EnglishProficiencyLevel
            {
                Code = dto.Code.Trim(),
                Name = dto.Name.Trim(),
                Description = dto.Description,
                OrderIndex = dto.OrderIndex,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(level);
            await _repository.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserId(), "Create Level", "EnglishProficiencyLevel", level.Id, null, level.Name);
        }

        public async Task UpdateAsync(UpdateLevelDto dto)
        {
            var level = await _repository.GetByIdAsync(dto.Id);
            if (level == null)
                throw new InvalidOperationException("Level không tồn tại.");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new InvalidOperationException("Name is required.");

            if (dto.OrderIndex < 0)
                throw new InvalidOperationException("OrderIndex cannot be negative.");

            var isUsed = await IsLevelUsedAsync(dto.Id);

            if (level.Code.ToLower() != dto.Code.ToLower())
            {
                if (isUsed)
                    throw new InvalidOperationException("Không thể sửa Level Code vì Level đã được tham chiếu.");

                if (await _validationService.IsLevelCodeDuplicateAsync(dto.Code, dto.Id))
                    throw new InvalidOperationException("Level code đã tồn tại.");
                    
                level.Code = dto.Code.Trim();
            }

            var oldName = level.Name;

            level.Name = dto.Name.Trim();
            level.Description = dto.Description;
            level.OrderIndex = dto.OrderIndex;
            level.IsActive = dto.IsActive;
            level.UpdatedAt = DateTime.UtcNow;

            _repository.Update(level);
            await _repository.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserId(), "Update Level", "EnglishProficiencyLevel", level.Id, oldName, level.Name);
        }

        public async Task DeactivateAsync(int id)
        {
            var level = await _repository.GetByIdAsync(id);
            if (level == null)
                throw new InvalidOperationException("Level không tồn tại.");

            if (await HasActiveTopicsAsync(id))
            {
                throw new InvalidOperationException("Không thể khóa Level này vì vẫn còn Topic đang hoạt động (Active). Vui lòng xử lý các Topic liên quan trước.");
            }

            level.IsActive = false;
            level.UpdatedAt = DateTime.UtcNow;

            _repository.Update(level);
            await _repository.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserId(), "Deactivate Level", "EnglishProficiencyLevel", id, "True", "False");
        }

        public async Task<bool> IsLevelUsedAsync(int levelId)
        {
            var topicsUsingLevel = await _topicRepository.GetByLevelAsync(levelId);
            return topicsUsingLevel.Any();
        }

        public async Task DeactivateLevelAsync(int id)
        {
            await DeactivateAsync(id);
        }

        public async Task ArchiveLevelAsync(int id)
        {
            var level = await _repository.GetByIdAsync(id);
            if (level == null)
                throw new InvalidOperationException("Level không tồn tại.");

            if (await HasActiveTopicsAsync(id))
            {
                throw new InvalidOperationException("Không thể lưu trữ (Archive) Level này vì vẫn còn Topic đang hoạt động (Active). Vui lòng xử lý các Topic liên quan trước.");
            }

            level.IsActive = false;
            level.UpdatedAt = DateTime.UtcNow;

            _repository.Update(level);
            await _repository.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserId(), "Archive Level", "EnglishProficiencyLevel", id, "True", "False");
        }

        public async Task RestoreLevelAsync(int id)
        {
            var level = await _repository.GetByIdAsync(id);
            if (level == null)
                throw new InvalidOperationException("Level không tồn tại.");

            level.IsActive = true;
            level.UpdatedAt = DateTime.UtcNow;

            _repository.Update(level);
            await _repository.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserId(), "Restore Level", "EnglishProficiencyLevel", id, "False", "True");
        }

        private async Task<bool> HasActiveTopicsAsync(int levelId)
        {
            var topics = await _topicRepository.GetByLevelAsync(levelId);
            return topics.Any(t => t.Status == "Active" || t.Status == "ACTIVE");
        }
    }
}
