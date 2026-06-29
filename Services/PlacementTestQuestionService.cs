using DuAnTotNghiep.Data;
using DuAnTotNghiep.DTOs.PlacementTestQuestion;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services
{
    public class PlacementTestQuestionService : IPlacementTestQuestionService
    {
        private readonly ApplicationDbContext _dbContext;

        public PlacementTestQuestionService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AttachQuestionToSectionAsync(AttachQuestionDto dto)
        {
            var section = await _dbContext.PlacementTestSections
                .Include(s => s.PlacementTest)
                .FirstOrDefaultAsync(s => s.Id == dto.SectionId);

            if (section == null) throw new InvalidOperationException("Không tìm thấy Section.");
            if (section.PlacementTest.Status == "ARCHIVED") throw new InvalidOperationException("Không thể sửa đổi Section của bài thi đã lưu trữ.");

            var question = await _dbContext.QuestionBanks.FindAsync(dto.QuestionId);
            if (question == null) throw new InvalidOperationException("Câu hỏi không tồn tại.");
            if (question.ReviewStatus != "APPROVED") throw new InvalidOperationException("Chỉ được sử dụng câu hỏi đã APPROVED.");

            if (question.SkillId != section.SkillId)
                throw new InvalidOperationException("Câu hỏi phải có cùng Kỹ năng (Skill) với Section.");

            var isDuplicate = await _dbContext.PlacementTestQuestions
                .AnyAsync(q => q.SectionId == dto.SectionId && q.QuestionId == dto.QuestionId);
            if (isDuplicate) throw new InvalidOperationException("Câu hỏi đã tồn tại trong Section này.");

            var newOrderIndex = dto.OrderIndex;
            if (newOrderIndex <= 0)
            {
                var maxOrder = await _dbContext.PlacementTestQuestions
                    .Where(q => q.SectionId == dto.SectionId)
                    .MaxAsync(q => (int?)q.OrderIndex) ?? 0;
                newOrderIndex = maxOrder + 1;
            }

            var duplicateOrder = await _dbContext.PlacementTestQuestions
                .AnyAsync(q => q.SectionId == dto.SectionId && q.OrderIndex == newOrderIndex);
            if (duplicateOrder) throw new InvalidOperationException("Thứ tự (OrderIndex) bị trùng.");

            var testQuestion = new PlacementTestQuestion
            {
                SectionId = dto.SectionId,
                QuestionId = dto.QuestionId,
                Points = dto.Points,
                OrderIndex = newOrderIndex
            };

            _dbContext.PlacementTestQuestions.Add(testQuestion);
            await _dbContext.SaveChangesAsync();

            await RecalculateSectionScoreAsync(dto.SectionId);
        }

        public async Task RemoveQuestionFromSectionAsync(int sectionId, int questionId)
        {
            var section = await _dbContext.PlacementTestSections
                .Include(s => s.PlacementTest)
                .FirstOrDefaultAsync(s => s.Id == sectionId);

            if (section == null) throw new InvalidOperationException("Không tìm thấy Section.");
            if (section.PlacementTest.Status == "ARCHIVED") throw new InvalidOperationException("Không thể sửa đổi Section của bài thi đã lưu trữ.");

            var hasAttempt = await _dbContext.TestAttempts.AnyAsync(a => a.PlacementTestId == section.PlacementTestId);
            if (hasAttempt) throw new InvalidOperationException("Bài thi đã có dữ liệu làm bài. Việc gỡ câu hỏi bị khóa để bảo vệ lịch sử.");

            var testQuestion = await _dbContext.PlacementTestQuestions
                .FirstOrDefaultAsync(q => q.SectionId == sectionId && q.QuestionId == questionId);

            if (testQuestion == null) throw new InvalidOperationException("Không tìm thấy câu hỏi trong Section này.");

            _dbContext.PlacementTestQuestions.Remove(testQuestion);
            await _dbContext.SaveChangesAsync();

            await RecalculateSectionScoreAsync(sectionId);
        }

        public async Task ReorderQuestionsAsync(int sectionId, List<QuestionOrderDto> items)
        {
            var section = await _dbContext.PlacementTestSections
                .Include(s => s.PlacementTest)
                .FirstOrDefaultAsync(s => s.Id == sectionId);

            if (section == null) throw new InvalidOperationException("Không tìm thấy Section.");
            if (section.PlacementTest.Status == "ARCHIVED") throw new InvalidOperationException("Không thể sửa đổi Section của bài thi đã lưu trữ.");

            var existingQuestions = await _dbContext.PlacementTestQuestions
                .Where(q => q.SectionId == sectionId)
                .ToListAsync();

            foreach (var item in items)
            {
                var target = existingQuestions.FirstOrDefault(q => q.QuestionId == item.QuestionId);
                if (target != null)
                {
                    target.OrderIndex = item.OrderIndex;
                }
            }

            var duplicateOrder = existingQuestions.GroupBy(x => x.OrderIndex).Any(g => g.Count() > 1);
            if (duplicateOrder) throw new InvalidOperationException("Thứ tự (OrderIndex) bị trùng lặp.");

            _dbContext.PlacementTestQuestions.UpdateRange(existingQuestions);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<QuestionBankItemDto>> SearchAvailableQuestionsAsync(QuestionFilterDto filter)
        {
            var section = await _dbContext.PlacementTestSections.FindAsync(filter.SectionId);
            if (section == null) throw new InvalidOperationException("Không tìm thấy Section.");

            var attachedQuestionIds = await _dbContext.PlacementTestQuestions
                .Where(q => q.SectionId == filter.SectionId)
                .Select(q => q.QuestionId)
                .ToListAsync();

            var query = _dbContext.QuestionBanks
                .Include(q => q.Skill)
                .Include(q => q.Topic)
                .Where(q => q.ReviewStatus == "APPROVED" && q.SkillId == section.SkillId && !attachedQuestionIds.Contains(q.Id));

            if (!string.IsNullOrEmpty(filter.Keyword))
            {
                query = query.Where(q => q.QuestionText.Contains(filter.Keyword));
            }

            if (!string.IsNullOrEmpty(filter.QuestionType))
            {
                query = query.Where(q => q.QuestionType == filter.QuestionType);
            }

            if (!string.IsNullOrEmpty(filter.DifficultyLevel))
            {
                query = query.Where(q => q.DifficultyLevel == filter.DifficultyLevel);
            }

            return await query.Select(q => new QuestionBankItemDto
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                QuestionType = q.QuestionType,
                DifficultyLevel = q.DifficultyLevel,
                SkillName = q.Skill.SkillName,
                TopicName = q.Topic != null ? q.Topic.Title : null
            }).ToListAsync();
        }

        public async Task<List<PlacementTestQuestionDto>> GetSectionQuestionsAsync(int sectionId)
        {
            return await _dbContext.PlacementTestQuestions
                .Where(q => q.SectionId == sectionId)
                .Include(q => q.Question)
                .ThenInclude(qb => qb.Skill)
                .OrderBy(q => q.OrderIndex)
                .Select(q => new PlacementTestQuestionDto
                {
                    Id = q.Id,
                    SectionId = q.SectionId,
                    QuestionId = q.QuestionId,
                    Points = q.Points,
                    OrderIndex = q.OrderIndex,
                    QuestionText = q.Question.QuestionText,
                    QuestionType = q.Question.QuestionType,
                    DifficultyLevel = q.Question.DifficultyLevel,
                    SkillName = q.Question.Skill.SkillName
                })
                .ToListAsync();
        }

        private async Task RecalculateSectionScoreAsync(int sectionId)
        {
            var totalPoints = await _dbContext.PlacementTestQuestions
                .Where(q => q.SectionId == sectionId)
                .SumAsync(q => q.Points);

            var section = await _dbContext.PlacementTestSections
                .Include(s => s.PlacementTest)
                .FirstOrDefaultAsync(s => s.Id == sectionId);

            if (section != null)
            {
                section.MaxScore = totalPoints;
                _dbContext.PlacementTestSections.Update(section);

                var testTotalScore = await _dbContext.PlacementTestSections
                    .Where(s => s.PlacementTestId == section.PlacementTestId)
                    .SumAsync(s => s.MaxScore);
                    
                section.PlacementTest.TotalScore = testTotalScore;
                _dbContext.PlacementTests.Update(section.PlacementTest);

                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
