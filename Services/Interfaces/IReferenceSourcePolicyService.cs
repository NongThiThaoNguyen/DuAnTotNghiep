using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public class SourceValidationResult
    {
        public bool IsValid { get; set; }
        public string Level { get; set; } = "OK"; // "OK" | "WARNING" | "BLOCKED"
        public string Message { get; set; } = string.Empty;
    }

    public interface IReferenceSourcePolicyService
    {
        Task<SourceValidationResult> ValidateSourceUsableAsync(int sourceId);
        Task<SourceValidationResult> CheckSourceBeforeUseAsync(int sourceId);
    }
}
