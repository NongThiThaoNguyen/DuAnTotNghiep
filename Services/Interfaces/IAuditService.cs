<<<<<<< HEAD
<<<<<<< HEAD
=======
using System.Threading.Tasks;

>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
=======
using System.Threading.Tasks;

>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IAuditService
    {
<<<<<<< HEAD
<<<<<<< HEAD
=======
        Task LogAsync(int? userId, string action, string entityName, int? entityId, string? oldValue = null, string? newValue = null);
        Task LogActionAsync(int? userId, string action, string? entityName, int? entityId, string? oldValue, string? newValue, string? ipAddress);
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
=======
        Task LogAsync(int? userId, string action, string entityName, int? entityId, string? oldValue = null, string? newValue = null);
        Task LogActionAsync(int? userId, string action, string? entityName, int? entityId, string? oldValue, string? newValue, string? ipAddress);
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
    }
}
