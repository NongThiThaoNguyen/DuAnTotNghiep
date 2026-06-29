using DuAnTotNghiep.Models;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IValidateLicenseService
    {
        Task ValidateLicenseAsync(ReferenceSource source);
    }
}
