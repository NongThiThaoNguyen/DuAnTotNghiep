using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models.DTOs.PlacementTest;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services
{
    public class PlacementRequirementService : IPlacementRequirementService
    {
        private readonly ApplicationDbContext _dbContext;

        public PlacementRequirementService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> IsPlacementTestRequiredAsync(int studentId)
        {
            var hasCompleted = await HasCompletedPlacementTestAsync(studentId);
            return !hasCompleted;
        }

        public async Task<bool> HasCompletedPlacementTestAsync(int studentId)
        {
            return await _dbContext.TestAttempts.AnyAsync(a => 
                a.StudentId == studentId && 
                (a.Status == "SUBMITTED" || a.Status == "GRADED"));
        }

        public async Task<PlacementFlowResultDto> GetStudentFlowStatusAsync(int studentId)
        {
            var profile = await _dbContext.StudentLearningProfiles
                .FirstOrDefaultAsync(p => p.UserId == studentId);

            if (profile == null || profile.OnboardingStatus != "COMPLETED")
            {
                return new PlacementFlowResultDto 
                { 
                    Status = PlacementFlowStatus.OnboardingRequired,
                    RedirectUrl = "/Student/Onboarding"
                };
            }

            var attempts = await _dbContext.TestAttempts
                .Where(a => a.StudentId == studentId)
                .OrderByDescending(a => a.StartedAt)
                .ToListAsync();

            // Ưu tiên kiểm tra lượt thi đang làm trước (hỗ trợ retake)
            var inProgressAttempt = attempts.FirstOrDefault(a => a.Status == "IN_PROGRESS");
            if (inProgressAttempt != null)
            {
                return new PlacementFlowResultDto 
                { 
                    Status = PlacementFlowStatus.PlacementInProgress,
                    AttemptId = inProgressAttempt.Id,
                    RedirectUrl = $"/Student/PlacementTest/Take/{inProgressAttempt.Id}"
                };
            }

            var completedAttempt = attempts.FirstOrDefault(a => a.Status == "SUBMITTED" || a.Status == "GRADED");
            if (completedAttempt != null)
            {
                return new PlacementFlowResultDto 
                { 
                    Status = PlacementFlowStatus.Completed,
                    AttemptId = completedAttempt.Id,
                    RedirectUrl = $"/Student/PlacementTest/Result?attemptId={completedAttempt.Id}"
                };
            }

            return new PlacementFlowResultDto 
            { 
                Status = PlacementFlowStatus.PlacementRequired,
                RedirectUrl = "/Student/PlacementTest/Intro"
            };
        }
    }
}
