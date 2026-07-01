using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface ITeacherResourceService
    {
        Task<(List<ReferenceSource> Items, int TotalItems)> GetResourcesAsync(string? keyword, ReferenceSourceType? sourceType, int page, int pageSize);
        Task<ReferenceSource?> GetResourceByIdAsync(int id);
        Task CreateResourceAsync(ReferenceSource resource, int teacherId);
        Task UpdateResourceAsync(ReferenceSource resource);
        Task DeleteResourceAsync(int id);
        bool ResourceExists(int id);
    }
}
