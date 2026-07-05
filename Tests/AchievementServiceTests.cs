using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Tests;

public class AchievementServiceTests
{
    private static DbContextOptions<ApplicationDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task DeleteAsync_DeactivatesAchievementInsteadOfRemovingIt()
    {
        var options = CreateOptions();

        using (var context = new ApplicationDbContext(options))
        {
            context.Achievements.Add(new Achievement
            {
                Id = 1,
                Code = "FIRST_STEP",
                Title = "First step",
                Description = "Start learning",
                IconUrl = "/badge.png",
                XpReward = 100,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }

        using (var context = new ApplicationDbContext(options))
        {
            var service = new AchievementService(context);

            await service.DeleteAsync(1);
        }

        using (var context = new ApplicationDbContext(options))
        {
            var achievement = await context.Achievements.SingleAsync(a => a.Id == 1);

            Assert.False(achievement.IsActive);
            Assert.Equal(1, await context.Achievements.CountAsync());
        }
    }
}
