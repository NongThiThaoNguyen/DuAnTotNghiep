using System.Collections.Generic;
using DuAnTotNghiep.Models.ViewModels;

namespace DuAnTotNghiep.Services.AI
{
    public class AIGenerationResult
    {
        public bool IsSuccess { get; set; }
        public List<GeneratedQuestionPreviewViewModel>? Items { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string>? ItemErrors { get; set; }
    }
}
