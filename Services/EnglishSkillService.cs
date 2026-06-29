using DuAnTotNghiep.Models.DTOs.Skill;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.Repositories.Interfaces;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
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
        private readonly ApplicationDbContext _db;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EnglishSkillService(
            IEnglishSkillRepository repository, 
            IM4ValidationService validationService,
            ApplicationDbContext db,
            IAuditService auditService,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _validationService = validationService;
            _db = db;
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
        }

        private int? GetCurrentUserId()
        {
            var userIdStr = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdStr, out int userId) ? userId : null;
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
            };

            await _repository.AddAsync(skill);
            await _repository.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserId(), "Create Skill", "EnglishSkill", skill.Id, null, skill.SkillName);
        }

        public async Task UpdateAsync(UpdateSkillDto dto)
        {
            var skill = await _repository.GetByIdAsync(dto.Id);
            if (skill == null)
                throw new InvalidOperationException("Skill không tồn tại.");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new InvalidOperationException("Name is required.");

            if (skill.SkillCode.ToLower() != dto.Code.ToLower())
            {
                if (await _validationService.IsSkillCodeDuplicateAsync(dto.Code, dto.Id))
                    throw new InvalidOperationException("Skill code đã tồn tại.");
                skill.SkillCode = dto.Code.Trim();
            }

            var oldName = skill.SkillName;

            skill.SkillName = dto.Name.Trim();
            skill.Description = dto.Description;
            skill.IsActive = dto.IsActive;
            skill.UpdatedAt = DateTime.UtcNow;

            _repository.Update(skill);
            await _repository.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserId(), "Update Skill", "EnglishSkill", skill.Id, oldName, skill.SkillName);
        }

        public async Task DeactivateAsync(int id)
        {
            var skill = await _repository.GetByIdAsync(id);
            if (skill == null)
                throw new InvalidOperationException("Skill không tồn tại.");

            var activeTopics = await _db.LearningTopics.AnyAsync(t => t.SkillId == id && (t.Status == "Active" || t.Status == "ACTIVE"));
            if (activeTopics)
            {
                throw new InvalidOperationException("Không thể khóa Skill này vì vẫn còn Topic đang hoạt động (Active). Vui lòng xử lý các Topic liên quan trước.");
            }

            skill.IsActive = false;
            skill.UpdatedAt = DateTime.UtcNow;

            _repository.Update(skill);
            await _repository.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserId(), "Deactivate Skill", "EnglishSkill", id, "True", "False");
        }

        public async Task<bool> IsSkillUsedAsync(int id)
        {
            return await _repository.IsSkillUsedAsync(id);
        }

        public async Task DeactivateSkillAsync(int id)
        {
            await DeactivateAsync(id);
        }

        public async Task ArchiveSkillAsync(int id)
        {
            var skill = await _repository.GetByIdAsync(id);
            if (skill == null)
                throw new InvalidOperationException("Skill không tồn tại.");

            var activeTopics = await _db.LearningTopics.AnyAsync(t => t.SkillId == id && (t.Status == "Active" || t.Status == "ACTIVE"));
            if (activeTopics)
            {
                throw new InvalidOperationException("Không thể lưu trữ (Archive) Skill này vì vẫn còn Topic đang hoạt động (Active). Vui lòng xử lý các Topic liên quan trước.");
            }

            skill.IsActive = false;
            skill.UpdatedAt = DateTime.UtcNow;

            _repository.Update(skill);
            await _repository.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserId(), "Archive Skill", "EnglishSkill", id, "True", "False");
        }

        public async Task RestoreSkillAsync(int id)
        {
            var skill = await _repository.GetByIdAsync(id);
            if (skill == null)
                throw new InvalidOperationException("Skill không tồn tại.");

            skill.IsActive = true;
            skill.UpdatedAt = DateTime.UtcNow;

            _repository.Update(skill);
            await _repository.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserId(), "Restore Skill", "EnglishSkill", id, "False", "True");
        }
    }
}
