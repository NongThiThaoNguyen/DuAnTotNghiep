using DuAnTotNghiep.Data;
using DuAnTotNghiep.DTOs;
using DuAnTotNghiep.DTOs.PlacementTest;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Repositories.Interfaces;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services
{
    public class PlacementTestManagementService : IPlacementTestManagementService
    {
        private readonly ApplicationDbContext _dbContext;

        public PlacementTestManagementService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(CreatePlacementTestDto dto, int createdBy)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new InvalidOperationException("Tiêu đề không được trống.");

            var targetLevel = await _dbContext.EnglishProficiencyLevels.FindAsync(dto.TargetLevelId);
            if (targetLevel == null)
                throw new InvalidOperationException("Target Level không tồn tại hoặc Inactive.");

            var newTest = new PlacementTest
            {
                Title = dto.Title,
                Description = dto.Description,
                TargetLevelId = dto.TargetLevelId,
                TimeLimitMinutes = dto.TimeLimitMinutes,
                TotalScore = dto.TotalScore,
                Status = "DRAFT",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            _dbContext.PlacementTests.Add(newTest);
            await _dbContext.SaveChangesAsync();

            return newTest.Id;
        }

        public async Task UpdateAsync(UpdatePlacementTestDto dto)
        {
            var test = await _dbContext.PlacementTests.FindAsync(dto.Id);
            if (test == null) throw new InvalidOperationException("Không tìm thấy bài thi.");

            bool hasAttempt = await _dbContext.TestAttempts.AnyAsync(a => a.PlacementTestId == dto.Id);

            if (hasAttempt)
            {
                // Chỉ cho sửa Title, Description
                test.Title = dto.Title;
                test.Description = dto.Description;
            }
            else
            {
                test.Title = dto.Title;
                test.Description = dto.Description;
                test.TargetLevelId = dto.TargetLevelId;
                test.TimeLimitMinutes = dto.TimeLimitMinutes;
                test.TotalScore = dto.TotalScore;
            }

            test.UpdatedAt = DateTime.UtcNow;
            _dbContext.PlacementTests.Update(test);
            await _dbContext.SaveChangesAsync();
        }

        public async Task PublishAsync(int placementTestId)
        {
            var test = await _dbContext.PlacementTests
                .Include(t => t.PlacementTestSections)
                .ThenInclude(s => s.PlacementTestQuestions)
                .FirstOrDefaultAsync(t => t.Id == placementTestId);

            if (test == null) throw new InvalidOperationException("Không tìm thấy bài thi.");
            if (test.Status != "DRAFT") throw new InvalidOperationException("Chỉ có thể Publish bài thi ở trạng thái Draft.");
            
            if (!test.PlacementTestSections.Any())
                throw new InvalidOperationException("Không thể Publish bài thi chưa có Section nào.");
            
            int questionCount = test.PlacementTestSections.Sum(s => s.PlacementTestQuestions.Count);
            if (questionCount == 0)
                throw new InvalidOperationException("Không thể Publish bài thi chưa có Question nào.");

            if (test.TargetLevelId == null)
                throw new InvalidOperationException("Bài thi chưa có Target Level.");

            test.Status = "PUBLISHED";
            test.UpdatedAt = DateTime.UtcNow;

            _dbContext.PlacementTests.Update(test);
            await _dbContext.SaveChangesAsync();
        }

        public async Task ArchiveAsync(int placementTestId)
        {
            var test = await _dbContext.PlacementTests.FindAsync(placementTestId);
            if (test == null) throw new InvalidOperationException("Không tìm thấy bài thi.");

            test.Status = "ARCHIVED";
            test.UpdatedAt = DateTime.UtcNow;

            _dbContext.PlacementTests.Update(test);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<PlacementTestDetailDto?> GetDetailAsync(int id)
        {
            var test = await _dbContext.PlacementTests
                .Include(t => t.TargetLevel)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (test == null) return null;

            int sectionCount = await _dbContext.PlacementTestSections.CountAsync(s => s.PlacementTestId == id);
            int attemptCount = await _dbContext.TestAttempts.CountAsync(a => a.PlacementTestId == id);
            
            var sections = await _dbContext.PlacementTestSections.Where(s => s.PlacementTestId == id).ToListAsync();
            var sectionIds = sections.Select(s => s.Id).ToList();
            int questionCount = await _dbContext.PlacementTestQuestions.CountAsync(q => sectionIds.Contains(q.SectionId));

            return new PlacementTestDetailDto
            {
                Id = test.Id,
                Title = test.Title,
                Description = test.Description,
                TargetLevelId = test.TargetLevelId,
                TargetLevelName = test.TargetLevel?.Name,
                TimeLimitMinutes = test.TimeLimitMinutes,
                TotalScore = test.TotalScore,
                Status = test.Status,
                CreatedAt = test.CreatedAt,
                PublishedAt = null,
                SectionCount = sectionCount,
                QuestionCount = questionCount,
                AttemptCount = attemptCount
            };
        }

        public async Task<PagedResult<PlacementTestListItemDto>> GetListAsync(PlacementTestFilterDto filter)
        {
            var query = _dbContext.PlacementTests.Include(t => t.TargetLevel).AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Title))
                query = query.Where(t => t.Title.Contains(filter.Title));

            if (!string.IsNullOrWhiteSpace(filter.Status))
                query = query.Where(t => t.Status == filter.Status);

            if (filter.TargetLevelId.HasValue)
                query = query.Where(t => t.TargetLevelId == filter.TargetLevelId.Value);

            int totalCount = await query.CountAsync();

            query = filter.SortBy switch
            {
                "Oldest" => query.OrderBy(t => t.CreatedAt),
                "Title" => query.OrderBy(t => t.Title),
                _ => query.OrderByDescending(t => t.CreatedAt) // Mặc định Newest
            };

            var items = await query
                .Skip((filter.PageIndex - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(t => new PlacementTestListItemDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    TargetLevelName = t.TargetLevel != null ? t.TargetLevel.Name : null,
                    TimeLimitMinutes = t.TimeLimitMinutes,
                    TotalScore = t.TotalScore,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt,
                    AttemptCount = t.TestAttempts.Count
                })
                .ToListAsync();

            return new PagedResult<PlacementTestListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = filter.PageIndex,
                PageSize = filter.PageSize
            };
        }
    }
}
