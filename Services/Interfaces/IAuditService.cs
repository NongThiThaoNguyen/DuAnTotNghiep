using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(int? userId, string action, string entityName, int? entityId, string? oldValue = null, string? newValue = null);
    }
}
