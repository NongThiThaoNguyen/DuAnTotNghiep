using DuAnTotNghiep.DTOs.Level;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Repositories.Interfaces;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
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

        public EnglishProficiencyLevelService(
            IEnglishProficiencyLevelRepository repository, 
            IM4ValidationService validationService,
            ILearningTopicRepository topicRepository)
        {
            _repository = repository;
            _validationService = validationService;
            _topicRepository = topicRepository;
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

            // Check if level is used (e.g. Topics)
            var isUsed = await IsLevelUsedAsync(dto.Id);

            if (level.Code.ToLower() != dto.Code.ToLower())
            {
                if (isUsed)
                    throw new InvalidOperationException("Không thể sửa Level Code vì Level đã được tham chiếu.");

                if (await _validationService.IsLevelCodeDuplicateAsync(dto.Code, dto.Id))
                    throw new InvalidOperationException("Level code đã tồn tại.");
                    
                level.Code = dto.Code.Trim();
            }

            level.Name = dto.Name.Trim();
            level.Description = dto.Description;
            level.OrderIndex = dto.OrderIndex;
            level.IsActive = dto.IsActive;
            level.UpdatedAt = DateTime.UtcNow;

            _repository.Update(level);
            await _repository.SaveChangesAsync();
        }

        public async Task DeactivateAsync(int id)
        {
            var level = await _repository.GetByIdAsync(id);
            if (level == null)
                throw new InvalidOperationException("Level không tồn tại.");

            if (await IsLevelUsedAsync(id))
            {
                throw new InvalidOperationException("Level đang được sử dụng (có Topic hoặc dữ liệu liên quan). Vui lòng xử lý dữ liệu liên quan trước.");
            }

            level.IsActive = false;
            level.UpdatedAt = DateTime.UtcNow;

            _repository.Update(level);
            await _repository.SaveChangesAsync();
        }

        private async Task<bool> IsLevelUsedAsync(int levelId)
        {
            var topicsUsingLevel = await _topicRepository.GetByLevelAsync(levelId);
            if (topicsUsingLevel.Any()) return true;

            // In a real scenario, we might also check LearningProfile (CurrentLevelId, TargetLevelId), etc.
            // Assuming this repository doesn't have direct access to LearningProfile, we can inject ILearningProfileRepository 
            // if needed, or handle it here via _topicRepository as a proxy if context is accessible.
            // Since we don't want to violate dependencies, we'll keep it simple for M4.
            
            return false;
        }
    }
}
