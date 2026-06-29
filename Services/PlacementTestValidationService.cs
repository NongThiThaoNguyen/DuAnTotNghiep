using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models.DTOs.PlacementTest.Validation;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services
{
    public class PlacementTestValidationService : IPlacementTestValidationService
    {
        private readonly ApplicationDbContext _dbContext;

        public PlacementTestValidationService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TestValidationResultDto> ValidatePlacementTestAsync(int placementTestId)
        {
            var result = new TestValidationResultDto();

            var test = await _dbContext.PlacementTests
                .Include(t => t.PlacementTestSections)
                    .ThenInclude(s => s.PlacementTestQuestions)
                        .ThenInclude(pq => pq.Question)
                            .ThenInclude(q => q.Topic)
                .Include(t => t.PlacementTestSections)
                    .ThenInclude(s => s.Skill)
                .FirstOrDefaultAsync(t => t.Id == placementTestId);

            if (test == null)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationErrorDto { Message = "Placement Test not found", Source = "System" });
                return result;
            }

            if (test.TargetLevelId == null)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationErrorDto { Message = "Target level is required", Source = "Placement Test" });
            }

            if (!test.PlacementTestSections.Any())
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationErrorDto { Message = "Test must contain at least one section", Source = "Placement Test" });
                return result; // Cannot proceed without sections
            }

            var allQuestions = test.PlacementTestSections.SelectMany(s => s.PlacementTestQuestions).ToList();
            if (!allQuestions.Any())
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationErrorDto { Message = "Test must contain at least one question", Source = "Placement Test" });
            }

            // Statistics computation
            result.Statistics.TotalSections = test.PlacementTestSections.Count;
            result.Statistics.TotalQuestions = allQuestions.Count;
            result.Statistics.TotalScore = test.TotalScore;
            
            decimal calculatedTotalScore = 0;

            // Validate Sections
            var skillNames = new HashSet<string>();
            foreach (var section in test.PlacementTestSections)
            {
                if (section.Skill == null)
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationErrorDto { Message = $"Skill not found for section '{section.SectionName}'", Source = $"Section {section.OrderIndex}" });
                }
                else
                {
                    skillNames.Add(section.Skill.SkillName);
                    // EnglishSkill usually has IsActive, but if it doesn't, we skip this check. 
                    // Let's assume there is an IsActive or Status field. We will rely on MVP rules.
                }

                if (!section.PlacementTestQuestions.Any())
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationErrorDto { Message = "Section contains no questions", Source = $"Section {section.OrderIndex}: {section.SectionName}" });
                }

                decimal sectionScore = section.PlacementTestQuestions.Sum(q => q.Points);
                if (sectionScore != section.MaxScore)
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationErrorDto { Message = $"Section score mismatch. Expected {section.MaxScore}, Actual {sectionScore}", Source = $"Section {section.OrderIndex}: {section.SectionName}" });
                }
                calculatedTotalScore += section.MaxScore;
                
                // Validate Questions within Section
                foreach(var pq in section.PlacementTestQuestions)
                {
                    var q = pq.Question;
                    string questionSource = $"Question {pq.OrderIndex} (Section {section.OrderIndex})";
                    
                    if (pq.Points <= 0)
                    {
                        result.IsValid = false;
                        result.Errors.Add(new ValidationErrorDto { Message = "Question points must be > 0", Source = questionSource });
                    }

                    if (q.ReviewStatus != "APPROVED")
                    {
                        result.IsValid = false;
                        result.Errors.Add(new ValidationErrorDto { Message = "Question is not approved", Source = questionSource });
                    }

                    if (string.IsNullOrWhiteSpace(q.CorrectAnswer))
                    {
                        result.IsValid = false;
                        result.Errors.Add(new ValidationErrorDto { Message = "Missing correct answer", Source = questionSource });
                    }

                    if (string.IsNullOrWhiteSpace(q.Explanation))
                    {
                        result.Warnings.Add(new ValidationWarningDto { Message = "Question explanation missing", Source = questionSource });
                    }
                }
            }

            if (calculatedTotalScore != test.TotalScore)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationErrorDto { Message = $"Test total score mismatch. Expected {test.TotalScore}, Actual {calculatedTotalScore}", Source = "Placement Test" });
            }

            result.Statistics.SkillsCovered = skillNames.ToList();

            // MVP Require Skills: Vocabulary, Grammar, Reading, Listening
            var requiredSkills = new[] { "Vocabulary", "Grammar", "Reading", "Listening" };
            foreach(var req in requiredSkills)
            {
                if (!skillNames.Any(s => s.Contains(req, System.StringComparison.OrdinalIgnoreCase)))
                {
                    result.Warnings.Add(new ValidationWarningDto { Message = $"Missing {req} skill", Source = "Placement Test" });
                }
            }

            // Topics Distribution
            var topics = allQuestions.Where(q => q.Question.Topic != null).Select(q => q.Question.Topic.Title).Distinct().ToList();
            result.Statistics.TopicsCovered = topics;

            if (topics.Count == 1 && allQuestions.Count > 1)
            {
                result.Warnings.Add(new ValidationWarningDto { Message = $"Questions are concentrated in a single topic: {topics.First()}", Source = "Topic Distribution" });
            }
            else if (topics.Count == 0 && allQuestions.Count > 0)
            {
                result.Warnings.Add(new ValidationWarningDto { Message = "No topics assigned to questions", Source = "Topic Distribution" });
            }

            // Difficulty Distribution
            var basicCount = allQuestions.Count(q => q.Question.DifficultyLevel == "BASIC");
            var mediumCount = allQuestions.Count(q => q.Question.DifficultyLevel == "MEDIUM");
            var advancedCount = allQuestions.Count(q => q.Question.DifficultyLevel == "ADVANCED");

            result.Statistics.DifficultyDistribution.BasicCount = basicCount;
            result.Statistics.DifficultyDistribution.MediumCount = mediumCount;
            result.Statistics.DifficultyDistribution.AdvancedCount = advancedCount;

            if (allQuestions.Count > 0)
            {
                if (result.Statistics.DifficultyDistribution.BasicPercentage < 20)
                {
                    result.Warnings.Add(new ValidationWarningDto { Message = $"BASIC difficulty is under 20% ({result.Statistics.DifficultyDistribution.BasicPercentage:0.0}%)", Source = "Difficulty Distribution" });
                }
                if (result.Statistics.DifficultyDistribution.MediumPercentage < 20)
                {
                    result.Warnings.Add(new ValidationWarningDto { Message = $"MEDIUM difficulty is under 20% ({result.Statistics.DifficultyDistribution.MediumPercentage:0.0}%)", Source = "Difficulty Distribution" });
                }
                if (result.Statistics.DifficultyDistribution.AdvancedPercentage < 10)
                {
                    result.Warnings.Add(new ValidationWarningDto { Message = $"ADVANCED difficulty is under 10% ({result.Statistics.DifficultyDistribution.AdvancedPercentage:0.0}%)", Source = "Difficulty Distribution" });
                }
                
                if (basicCount == allQuestions.Count || mediumCount == allQuestions.Count || advancedCount == allQuestions.Count)
                {
                    result.Warnings.Add(new ValidationWarningDto { Message = "Questions are concentrated in a single difficulty level", Source = "Difficulty Distribution" });
                }
            }

            return result;
        }
    }
}
