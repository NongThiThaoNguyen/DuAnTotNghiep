using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Repositories.Interfaces
{
    public interface IRoleRepository : IGenericRepository<Role>
    {
        Task<Role?> GetByCodeAsync(string roleCode);
    }
}
