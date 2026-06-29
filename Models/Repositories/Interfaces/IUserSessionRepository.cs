using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Models.Repositories.Interfaces
{
    public interface IUserSessionRepository : IGenericRepository<UserSession>
    {
        Task<UserSession?> GetBySessionTokenAsync(string token);
    }
}
