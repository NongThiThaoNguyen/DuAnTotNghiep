using DuAnTotNghiep.DTOs.Skill;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Repositories.Interfaces;
using DuAnTotNghiep.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services
{
    public class EnglishSkillService : IEnglishSkillService
    {
        private readonly IEnglishSkillRepository _repository;
        private readonly IM4ValidationService _validationService;

        public EnglishSkillService(IEnglishSkillRepository repository, IM4ValidationService validationService)
        {
            _repository = repository;
            _validationService = validationService;
        }

        public async Task<List<SkillOptionDto>> GetOptionsAsync()
        {
            var skills = await _repository.GetActiveSkillsAsync();
            return skills.Select(s => new SkillOptionDto
            {
                Id = s.Id,
                Code = s.SkillCode,
                Name = s.SkillName
            }).ToList();
        }

        public async Task<List<SkillListItemDto>> GetListAsync()
        {
            var skills = await _repository.GetAllAsync();
            return skills.Select(s => new SkillListItemDto
            {
                Id = s.Id,
                Code = s.SkillCode,
                Name = s.SkillName,
                IsActive = s.IsActive
            }).OrderBy(s => s.Code).ToList();
        }

        public async Task<SkillDetailDto?> GetByIdAsync(int id)
        {
            var skill = await _repository.GetByIdAsync(id);
            if (skill == null) return null;

            return new SkillDetailDto
            {
                Id = skill.Id,
                Code = skill.SkillCode,
                Name = skill.SkillName,
                Description = skill.Description,
                IsActive = skill.IsActive
            };
        }

        public async Task CreateAsync(CreateSkillDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name))
                throw new InvalidOperationException("Code and Name are required.");

            if (await _validationService.IsSkillCodeDuplicateAsync(dto.Code))
                throw new InvalidOperationException("Skill code đã tồn tại.");

            var skill = new EnglishSkill
            {
                SkillCode = dto.Code.Trim(),
                SkillName = dto.Name.Trim(),
                Description = dto.Description,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
                // CreatedBy will be set by interceptor or controller logic if available, skipping for now
            };

            await _repository.AddAsync(skill);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateAsync(UpdateSkillDto dto)
        {
            var skill = await _repository.GetByIdAsync(dto.Id);
            if (skill == null)
                throw new InvalidOperationException("Skill không tồn tại."); // Treat as NotFound in controller

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new InvalidOperationException("Name is required.");

            // Optionally check for code duplicate if code changes are allowed
            if (skill.SkillCode.ToLower() != dto.Code.ToLower())
            {
                if (await _validationService.IsSkillCodeDuplicateAsync(dto.Code, dto.Id))
                    throw new InvalidOperationException("Skill code đã tồn tại.");
                skill.SkillCode = dto.Code.Trim();
            }

            skill.SkillName = dto.Name.Trim();
            skill.Description = dto.Description;
            skill.IsActive = dto.IsActive;
            skill.UpdatedAt = DateTime.UtcNow;

            _repository.Update(skill);
            await _repository.SaveChangesAsync();
        }

        public async Task DeactivateAsync(int id)
        {
            var skill = await _repository.GetByIdAsync(id);
            if (skill == null)
                throw new InvalidOperationException("Skill không tồn tại.");

            if (await _repository.IsSkillUsedAsync(id))
            {
                // Vẫn cho Deactivate, nhưng có thể throw warning hoặc controller tự cảnh báo. 
                // Yêu cầu: "Nếu đã dùng: Hiển thị cảnh báo. Vẫn cho: Deactivate. Không cho: Delete".
                // Deactivate in DB anyway, UI handled the warning before calling.
            }

            skill.IsActive = false;
            skill.UpdatedAt = DateTime.UtcNow;

            _repository.Update(skill);
            await _repository.SaveChangesAsync();
        }
    }
}
