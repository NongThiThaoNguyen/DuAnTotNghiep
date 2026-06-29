using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Models.Repositories.Interfaces
{
    public interface IRoleRepository : IGenericRepository<Role>
    {
        Task<Role?> GetByCodeAsync(string roleCode);
    }
}
