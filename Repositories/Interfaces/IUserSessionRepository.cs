using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Repositories.Interfaces
{
    public interface IUserSessionRepository : IGenericRepository<UserSession>
    {
        Task<UserSession?> GetBySessionTokenAsync(string token);
    }
}
