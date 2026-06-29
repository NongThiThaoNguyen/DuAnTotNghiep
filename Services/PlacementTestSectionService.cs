using DuAnTotNghiep.Data;
using DuAnTotNghiep.DTOs.PlacementTestSection;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services
{
    public class PlacementTestSectionService : IPlacementTestSectionService
    {
        private readonly ApplicationDbContext _dbContext;

        public PlacementTestSectionService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> AddSectionAsync(CreateSectionDto dto)
        {
            var test = await _dbContext.PlacementTests.FindAsync(dto.PlacementTestId);
            if (test == null) throw new InvalidOperationException("Placement Test không tồn tại.");
            if (test.Status == "ARCHIVED") throw new InvalidOperationException("Không thể thêm Section vào bài thi đã lưu trữ (ARCHIVED).");

            var skill = await _dbContext.EnglishSkills.FindAsync(dto.SkillId);
            if (skill == null || !skill.IsActive)
                throw new InvalidOperationException("Skill không tồn tại hoặc đã bị khóa.");

            var isOrderExist = await _dbContext.PlacementTestSections
                .AnyAsync(s => s.PlacementTestId == dto.PlacementTestId && s.OrderIndex == dto.OrderIndex);
            
            if (isOrderExist)
                throw new InvalidOperationException($"OrderIndex {dto.OrderIndex} đã tồn tại trong bài thi này.");

            var section = new PlacementTestSection
            {
                PlacementTestId = dto.PlacementTestId,
                SkillId = dto.SkillId,
                SectionName = dto.SectionName,
                Instruction = dto.Instruction,
                OrderIndex = dto.OrderIndex,
                MaxScore = dto.MaxScore
            };

            _dbContext.PlacementTestSections.Add(section);
            await _dbContext.SaveChangesAsync();

            return section.Id;
        }

        public async Task UpdateSectionAsync(UpdateSectionDto dto)
        {
            var section = await _dbContext.PlacementTestSections.FindAsync(dto.Id);
            if (section == null) throw new InvalidOperationException("Không tìm thấy Section.");
            if (section.PlacementTestId != dto.PlacementTestId) throw new InvalidOperationException("Dữ liệu không hợp lệ.");

            var test = await _dbContext.PlacementTests.FindAsync(section.PlacementTestId);
            if (test?.Status == "ARCHIVED") throw new InvalidOperationException("Không thể sửa Section trong bài thi đã lưu trữ (ARCHIVED).");

            var hasAttempt = await _dbContext.TestAttempts.AnyAsync(a => a.PlacementTestId == section.PlacementTestId);

            if (hasAttempt && section.SkillId != dto.SkillId)
            {
                throw new InvalidOperationException("Bài thi đã có người làm, không thể thay đổi Kỹ năng (Skill) của Section.");
            }

            if (!hasAttempt && section.SkillId != dto.SkillId)
            {
                var skill = await _dbContext.EnglishSkills.FindAsync(dto.SkillId);
                if (skill == null || !skill.IsActive)
                    throw new InvalidOperationException("Skill không tồn tại hoặc đã bị khóa.");
            }

            if (section.OrderIndex != dto.OrderIndex)
            {
                var isOrderExist = await _dbContext.PlacementTestSections
                    .AnyAsync(s => s.PlacementTestId == section.PlacementTestId && s.OrderIndex == dto.OrderIndex && s.Id != section.Id);
                if (isOrderExist)
                    throw new InvalidOperationException($"OrderIndex {dto.OrderIndex} đã tồn tại trong bài thi này.");
            }

            if (!hasAttempt) section.SkillId = dto.SkillId;
            section.SectionName = dto.SectionName;
            section.Instruction = dto.Instruction;
            section.OrderIndex = dto.OrderIndex;
            
            // Ở task này ta cho phép cập nhật MaxScore tạm thời. Khi có câu hỏi sẽ validate lại.
            section.MaxScore = dto.MaxScore;

            _dbContext.PlacementTestSections.Update(section);
            await _dbContext.SaveChangesAsync();
        }

        public async Task ReorderSectionAsync(int placementTestId, List<SectionOrderDto> sections)
        {
            var existingSections = await _dbContext.PlacementTestSections
                .Where(s => s.PlacementTestId == placementTestId)
                .ToListAsync();

            var test = await _dbContext.PlacementTests.FindAsync(placementTestId);
            if (test?.Status == "ARCHIVED") throw new InvalidOperationException("Không thể sửa thứ tự Section trong bài thi đã lưu trữ.");

            foreach (var s in sections)
            {
                var target = existingSections.FirstOrDefault(es => es.Id == s.SectionId);
                if (target != null)
                {
                    target.OrderIndex = s.OrderIndex;
                }
            }

            var duplicateOrder = existingSections.GroupBy(x => x.OrderIndex).Any(g => g.Count() > 1);
            if (duplicateOrder) throw new InvalidOperationException("Thứ tự (OrderIndex) bị trùng lặp.");

            _dbContext.PlacementTestSections.UpdateRange(existingSections);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteSectionIfUnusedAsync(int sectionId)
        {
            var section = await _dbContext.PlacementTestSections
                .Include(s => s.PlacementTestQuestions)
                .FirstOrDefaultAsync(s => s.Id == sectionId);
                
            if (section == null) throw new InvalidOperationException("Không tìm thấy Section.");

            var test = await _dbContext.PlacementTests.FindAsync(section.PlacementTestId);
            if (test?.Status == "ARCHIVED") throw new InvalidOperationException("Không thể xóa Section trong bài thi đã lưu trữ.");

            var hasAttempt = await _dbContext.TestAttempts.AnyAsync(a => a.PlacementTestId == section.PlacementTestId);
            if (hasAttempt) throw new InvalidOperationException("Bài thi đã có dữ liệu làm bài. Không thể xóa Section.");

            if (section.PlacementTestQuestions.Any())
                throw new InvalidOperationException("Section đã chứa câu hỏi. Vui lòng xóa hết câu hỏi trước khi xóa Section.");

            _dbContext.PlacementTestSections.Remove(section);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<PlacementTestSectionDto>> GetSectionsAsync(int placementTestId)
        {
            return await _dbContext.PlacementTestSections
                .Where(s => s.PlacementTestId == placementTestId)
                .Include(s => s.Skill)
                .OrderBy(s => s.OrderIndex)
                .Select(s => new PlacementTestSectionDto
                {
                    Id = s.Id,
                    PlacementTestId = s.PlacementTestId,
                    SkillId = s.SkillId,
                    SkillName = s.Skill.SkillName,
                    SectionName = s.SectionName,
                    Instruction = s.Instruction,
                    OrderIndex = s.OrderIndex,
                    MaxScore = s.MaxScore,
                    QuestionCount = s.PlacementTestQuestions.Count
                })
                .ToListAsync();
        }
    }
}
