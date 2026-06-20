using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Repositories
{
    public class PasswordResetTokenRepository : GenericRepository<PasswordResetToken>, IPasswordResetTokenRepository
    {
        public PasswordResetTokenRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<PasswordResetToken?> GetValidTokenAsync(int userId, string tokenHash)
        {
            var now = DateTime.UtcNow;
            return await _dbSet.FirstOrDefaultAsync(t => 
                t.UserId == userId && 
                t.TokenHash == tokenHash && 
                t.UsedAt == null && 
                t.ExpiresAt > now);
        }

        public async Task InvalidateOldTokensAsync(int userId)
        {
            var now = DateTime.UtcNow;
            var tokens = await _dbSet.Where(t => t.UserId == userId && t.UsedAt == null && t.ExpiresAt > now).ToListAsync();
            foreach (var token in tokens)
            {
                token.UsedAt = now; // Mark as used/invalidated
            }
            // SaveChangesAsync is usually called at the service level, but we can call it here if needed, or leave it to service.
            // Leaving it to service per Unit Of Work pattern, but GenericRepository saves via generic method.
            // Let's just update them. The generic repository pattern here suggests we save via generic save method.
        }
    }
}
