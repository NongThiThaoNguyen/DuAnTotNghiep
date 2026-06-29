using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public IList<string> Errors { get; set; } = new List<string>();
    }

    public interface IResponseValidator
    {
        Task<ValidationResult> ValidateAsync(string json, string schema);
    }
}
