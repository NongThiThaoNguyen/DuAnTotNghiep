using DuAnTotNghiep.Data;
using DuAnTotNghiep.DTOs.PlacementTest;
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

            if (attempts.Any(a => a.Status == "SUBMITTED" || a.Status == "GRADED"))
            {
                return new PlacementFlowResultDto 
                { 
                    Status = PlacementFlowStatus.Completed
                };
            }

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

            return new PlacementFlowResultDto 
            { 
                Status = PlacementFlowStatus.PlacementRequired,
                RedirectUrl = "/Student/PlacementTest/Intro"
            };
        }
    }
}
