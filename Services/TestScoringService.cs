using DuAnTotNghiep.Data;
using DuAnTotNghiep.DTOs.PlacementTest;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Repositories.Interfaces;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services
{
    public class TestScoringService : ITestScoringService
    {
        private readonly ApplicationDbContext _dbContext;

        public TestScoringService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ScoreResultDto> GradeAttemptAsync(int attemptId)
        {
            // 1. Fetch attempt along with answers
            var attempt = await _dbContext.TestAttempts
                .Include(a => a.TestAnswers)
                .FirstOrDefaultAsync(a => a.Id == attemptId);

            if (attempt == null)
            {
                throw new Exception("Attempt not found");
            }

            var questionIds = attempt.TestAnswers.Select(a => a.QuestionId).ToList();
            
            // 2. Fetch questions from QuestionBank
            var questions = await _dbContext.QuestionBanks
                .Where(q => questionIds.Contains(q.Id))
                .ToDictionaryAsync(q => q.Id);
                
            // 3. Fetch point allocation from PlacementTestQuestion mapping
            var ptQuestions = await _dbContext.PlacementTestQuestions
                .Where(pq => pq.Section.PlacementTestId == attempt.PlacementTestId && questionIds.Contains(pq.QuestionId))
                .ToDictionaryAsync(pq => pq.QuestionId);

            int correctCount = 0;
            int wrongCount = 0;
            decimal totalScore = 0;

            // 4. Evaluate each answer
            foreach (var answer in attempt.TestAnswers)
            {
                if (!questions.TryGetValue(answer.QuestionId, out var question))
                {
                    continue; // Question missing in bank
                }

                decimal pointValue = ptQuestions.TryGetValue(answer.QuestionId, out var ptq) ? ptq.Points : 1m;

                bool isCorrect = false;

                if (question.QuestionType == "MCQ" || question.QuestionType == "TRUE_FALSE" || question.QuestionType == "LISTENING")
                {
                    // Check logic based on correct answer id string vs SelectedOptionId
                    // CorrectAnswer is expected to hold OptionId or the text depending on DB setup.
                    // Usually for MCQ, CorrectAnswer holds OptionId or OptionText. 
                    // Let's assume CorrectAnswer holds the OptionId as string.
                    if (answer.SelectedOptionId.HasValue && question.CorrectAnswer == answer.SelectedOptionId.Value.ToString())
                    {
                        isCorrect = true;
                    }
                    else if (!string.IsNullOrEmpty(answer.AnswerText) && question.CorrectAnswer?.Trim().Equals(answer.AnswerText.Trim(), StringComparison.OrdinalIgnoreCase) == true)
                    {
                        isCorrect = true;
                    }
                }
                else if (question.QuestionType == "SHORT_ANSWER")
                {
                    // Basic exact string match
                    if (!string.IsNullOrEmpty(answer.AnswerText) && question.CorrectAnswer?.Trim().Equals(answer.AnswerText.Trim(), StringComparison.OrdinalIgnoreCase) == true)
                    {
                        isCorrect = true;
                    }
                }

                // Cập nhật answer entity
                answer.IsCorrect = isCorrect;
                answer.Score = isCorrect ? pointValue : 0;
                
                if (isCorrect)
                {
                    correctCount++;
                    totalScore += pointValue;
                }
                else
                {
                    wrongCount++;
                }
            }

            // Save changes on answers
            _dbContext.TestAnswers.UpdateRange(attempt.TestAnswers);
            await _dbContext.SaveChangesAsync();

            return new ScoreResultDto
            {
                TotalScore = totalScore,
                CorrectAnswers = correctCount,
                WrongAnswers = wrongCount
            };
        }

        public async Task<List<SkillScoreDto>> CalculateSkillScoresAsync(int attemptId)
        {
            var attempt = await _dbContext.TestAttempts
                .Include(a => a.TestAnswers)
                .FirstOrDefaultAsync(a => a.Id == attemptId);

            if (attempt == null) return new List<SkillScoreDto>();

            var questionIds = attempt.TestAnswers.Select(a => a.QuestionId).ToList();

            var ptQuestions = await _dbContext.PlacementTestQuestions
                .Include(pq => pq.Section)
                .ThenInclude(s => s.Skill)
                .Where(pq => pq.Section.PlacementTestId == attempt.PlacementTestId && questionIds.Contains(pq.QuestionId))
                .ToListAsync();

            var result = new List<SkillScoreDto>();

            var skillGroups = ptQuestions.GroupBy(pq => pq.Section.Skill);

            foreach (var group in skillGroups)
            {
                var skill = group.Key;
                var qIdsInSkill = group.Select(pq => pq.QuestionId).ToList();
                
                var answersInSkill = attempt.TestAnswers.Where(a => qIdsInSkill.Contains(a.QuestionId)).ToList();
                
                int totalQs = qIdsInSkill.Count;
                int correctQs = answersInSkill.Count(a => a.IsCorrect == true);
                decimal score = answersInSkill.Sum(a => a.Score ?? 0);

                result.Add(new SkillScoreDto
                {
                    SkillId = skill.Id,
                    SkillName = skill.SkillName,
                    TotalQuestions = totalQs,
                    CorrectQuestions = correctQs,
                    Score = score
                });
            }

            return result;
        }

        public async Task<EstimatedLevelDto> EstimateLevelAsync(int attemptId)
        {
            var attempt = await _dbContext.TestAttempts
                .Include(a => a.PlacementTest)
                .FirstOrDefaultAsync(a => a.Id == attemptId);

            if (attempt == null) return new EstimatedLevelDto();

            decimal totalScore = attempt.TotalScore ?? 0;
            decimal maxScore = attempt.PlacementTest.TotalScore; // Tổng điểm tối đa của đề thi

            if (maxScore <= 0) maxScore = 1; // Tránh chia cho 0

            decimal percentage = (totalScore / maxScore) * 100;

            // Simple rule: <40% A1, <60% A2, <80% B1, >=80% B2
            string levelCode = "A1";
            if (percentage >= 80) levelCode = "B2";
            else if (percentage >= 60) levelCode = "B1";
            else if (percentage >= 40) levelCode = "A2";

            var level = await _dbContext.EnglishProficiencyLevels
                .FirstOrDefaultAsync(l => l.Code == levelCode);

            return new EstimatedLevelDto
            {
                LevelId = level?.Id,
                LevelName = level?.Name
            };
        }
    }
}
