using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.DTOs;
using DuAnTotNghiep.DTOs.PlacementTest;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.ViewModels.Admin;

namespace DuAnTotNghiep.Services
{
    public class PlacementAttemptService : IPlacementAttemptService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ITestScoringService _scoringService;

        public PlacementAttemptService(ApplicationDbContext dbContext, ITestScoringService scoringService)
        {
            _dbContext = dbContext;
            _scoringService = scoringService;
        }

        public async Task<PagedResult<PlacementAttemptListItemViewModel>> GetAttemptsForAdminAsync(PlacementAttemptFilter filter, int currentUserId, string role)
        {
            var query = _dbContext.TestAttempts
                .AsNoTracking()
                .Include(a => a.Student)
                .Include(a => a.PlacementTest)
                .Include(a => a.EstimatedLevel)
                .AsQueryable();

            // Authorization: Teacher scope
            if (role == "TEACHER")
            {
                // TODO: Implement actual Teacher-Student scope filtering when Class management is ready.
                // For now, they see everything or we can restrict them. Assuming they see all for MVP.
            }

            // Apply Filters
            if (filter.TestId.HasValue && filter.TestId.Value > 0)
            {
                query = query.Where(a => a.PlacementTestId == filter.TestId.Value);
            }

            if (filter.StudentId.HasValue && filter.StudentId.Value > 0)
            {
                query = query.Where(a => a.StudentId == filter.StudentId.Value);
            }

            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(a => a.Status == filter.Status);
            }

            if (filter.FromDate.HasValue)
            {
                query = query.Where(a => a.SubmittedAt >= filter.FromDate.Value || (a.SubmittedAt == null && a.StartedAt >= filter.FromDate.Value));
            }

            if (filter.ToDate.HasValue)
            {
                query = query.Where(a => a.SubmittedAt <= filter.ToDate.Value || (a.SubmittedAt == null && a.StartedAt <= filter.ToDate.Value));
            }

            if (!string.IsNullOrEmpty(filter.Keyword))
            {
                string kw = filter.Keyword.ToLower();
                query = query.Where(a => 
                    a.Student.FullName.ToLower().Contains(kw) || 
                    a.Student.Email.ToLower().Contains(kw));
            }

            int totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(a => a.SubmittedAt).ThenByDescending(a => a.StartedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(a => new PlacementAttemptListItemViewModel
                {
                    AttemptId = a.Id,
                    StudentId = a.StudentId,
                    StudentName = a.Student.FullName,
                    Email = a.Student.Email,
                    PlacementTestId = a.PlacementTestId,
                    TestTitle = a.PlacementTest.Title,
                    TotalScore = a.TotalScore ?? 0,
                    EstimatedLevel = a.EstimatedLevel != null ? a.EstimatedLevel.Name : null,
                    Status = a.Status,
                    SubmittedAt = a.SubmittedAt
                })
                .ToListAsync();

            return new PagedResult<PlacementAttemptListItemViewModel>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<PlacementAttemptDetailViewModel?> GetAttemptDetailAsync(int attemptId, int currentUserId, string role)
        {
            var attempt = await _dbContext.TestAttempts
                .AsNoTracking()
                .Include(a => a.Student)
                .Include(a => a.PlacementTest)
                .Include(a => a.EstimatedLevel)
                .FirstOrDefaultAsync(a => a.Id == attemptId);

            if (attempt == null)
            {
                return null;
            }

            if (role == "TEACHER")
            {
                // TODO: Verify if the student is in Teacher's scope
            }

            // Get Skill/Topic Scores
            var skillScores = await _scoringService.CalculateSkillScoresAsync(attemptId);
            var topicScores = await _scoringService.CalculateTopicScoresAsync(attemptId);

            // Get AI Status
            string aiStatus = "Not Available";
            var aiAnalysis = await _dbContext.CompetencyAnalyses.FirstOrDefaultAsync(c => c.TestAttemptId == attemptId);
            if (aiAnalysis != null)
            {
                aiStatus = "Completed";
            }
            else
            {
                var aiLog = await _dbContext.AiUsageLogs
                    .OrderByDescending(l => l.CreatedAt)
                    .FirstOrDefaultAsync(l => l.UserId == attempt.StudentId && l.ModuleCode == $"M6_ATTEMPT_{attemptId}");
                
                if (aiLog != null)
                {
                    aiStatus = aiLog.RequestStatus switch
                    {
                        "PENDING" => "Pending",
                        "RUNNING" => "Running",
                        "FAILED" => "Failed",
                        _ => "Analyzing"
                    };
                }
            }

            return new PlacementAttemptDetailViewModel
            {
                AttemptId = attempt.Id,
                StudentName = attempt.Student.FullName,
                TestTitle = attempt.PlacementTest.Title,
                TotalScore = attempt.TotalScore ?? 0,
                EstimatedLevel = attempt.EstimatedLevel?.Name,
                StartedAt = attempt.StartedAt,
                SubmittedAt = attempt.SubmittedAt,
                Status = attempt.Status,
                SkillScores = skillScores.OrderByDescending(s => s.Percentage).ToList(),
                TopicScores = topicScores.OrderByDescending(t => t.Percentage).ToList(),
                AiAnalysisStatus = aiStatus
            };
        }
    }
}
