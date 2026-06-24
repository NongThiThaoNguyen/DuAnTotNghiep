using System.Collections.Generic;

namespace DuAnTotNghiep.DTOs.PlacementTest.Validation
{
    public class TestValidationResultDto
    {
        public bool IsValid { get; set; } = true;
        public List<ValidationErrorDto> Errors { get; set; } = new List<ValidationErrorDto>();
        public List<ValidationWarningDto> Warnings { get; set; } = new List<ValidationWarningDto>();
        public List<ValidationInfoDto> Infos { get; set; } = new List<ValidationInfoDto>();
        public TestValidationStatisticsDto Statistics { get; set; } = new TestValidationStatisticsDto();
    }

    public class ValidationErrorDto
    {
        public string Message { get; set; }
        public string Source { get; set; } // e.g., "Section 1", "Question 5"
    }

    public class ValidationWarningDto
    {
        public string Message { get; set; }
        public string Source { get; set; }
    }

    public class ValidationInfoDto
    {
        public string Message { get; set; }
    }

    public class TestValidationStatisticsDto
    {
        public int TotalSections { get; set; }
        public int TotalQuestions { get; set; }
        public decimal TotalScore { get; set; }
        public decimal MaxAllowedScore { get; set; }
        public List<string> SkillsCovered { get; set; } = new List<string>();
        public List<string> TopicsCovered { get; set; } = new List<string>();
        public DifficultyDistributionDto DifficultyDistribution { get; set; } = new DifficultyDistributionDto();
    }

    public class DifficultyDistributionDto
    {
        public int BasicCount { get; set; }
        public int MediumCount { get; set; }
        public int AdvancedCount { get; set; }

        public decimal BasicPercentage => Total > 0 ? (decimal)BasicCount / Total * 100 : 0;
        public decimal MediumPercentage => Total > 0 ? (decimal)MediumCount / Total * 100 : 0;
        public decimal AdvancedPercentage => Total > 0 ? (decimal)AdvancedCount / Total * 100 : 0;

        public int Total => BasicCount + MediumCount + AdvancedCount;
    }
}
