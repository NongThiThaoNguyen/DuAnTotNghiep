namespace DuAnTotNghiep.DTOs
{
    public class AiUsageLogDto
    {
        public int? UserId { get; set; }
        public string ModuleCode { get; set; } = "M14";
        public int? PromptTemplateId { get; set; }
        public string? AiModel { get; set; }
        public int? InputTokens { get; set; }
        public int? OutputTokens { get; set; }
        public decimal? CostEstimate { get; set; }
        public string RequestStatus { get; set; } = "SUCCESS";
        public string? ErrorMessage { get; set; }
        public int? DurationMs { get; set; }
    }
}
