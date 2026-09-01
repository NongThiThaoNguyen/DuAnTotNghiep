using System.Threading;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IGeminiService
    {
        /// <summary>
        /// Gửi prompt tới Gemini API và nhận câu trả lời dưới dạng text.
        /// </summary>
        Task<string> ChatAsync(string userPrompt, string? systemPrompt = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gửi request hoàn chỉnh tới Gemini API và nhận kết quả chi tiết kèm metadata.
        /// </summary>
        Task<GeminiResultDto> SendGenerateContentAsync(string userPrompt, string? systemPrompt = null, string? modelOverride = null, CancellationToken cancellationToken = default);
    }

    public class GeminiResultDto
    {
        public bool IsSuccess { get; set; }
        public string? Content { get; set; }
        public string? ErrorMessage { get; set; }
        public int HttpStatusCode { get; set; }
        public string? Model { get; set; }
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }
}
