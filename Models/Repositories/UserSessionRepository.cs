using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Models.Repositories
{
    public class UserSessionRepository : GenericRepository<UserSession>, IUserSessionRepository
    {
        public UserSessionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<UserSession?> GetBySessionTokenAsync(string token)
        {
            return await _context.UserSessions.FirstOrDefaultAsync(s => s.SessionToken == token);
        }
    }
}
