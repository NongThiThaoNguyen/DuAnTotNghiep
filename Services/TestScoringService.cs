using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models.DTOs.PlacementTest;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.Repositories.Interfaces;
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

            // 2. Fetch all placement test questions for this test along with options
            var ptQuestions = await _dbContext.PlacementTestQuestions
                .Include(pq => pq.Question)
                .ThenInclude(q => q.QuestionOptions)
                .Where(pq => pq.Section.PlacementTestId == attempt.PlacementTestId)
                .ToListAsync();

            var existingAnswers = attempt.TestAnswers.ToDictionary(a => a.QuestionId);

            int correctCount = 0;
            int wrongCount = 0;
            decimal totalScore = 0;

            // 3. Evaluate each question in the test
            foreach (var ptq in ptQuestions)
            {
                var question = ptq.Question;
                decimal pointValue = ptq.Points;

                if (!existingAnswers.TryGetValue(question.Id, out var answer))
                {
                    // Create answer record for un-answered question
                    answer = new TestAnswer
                    {
                        AttemptId = attemptId,
                        QuestionId = question.Id,
                        IsCorrect = false,
                        Score = 0,
                        AnsweredAt = DateTime.UtcNow
                    };
                    _dbContext.TestAnswers.Add(answer);
                    wrongCount++;
                    continue;
                }

                bool isCorrect = false;

                if (question.QuestionType == "MCQ" || question.QuestionType == "TRUE_FALSE" || question.QuestionType == "LISTENING")
                {
                    if (answer.SelectedOptionId.HasValue)
                    {
                        var selectedOption = question.QuestionOptions.FirstOrDefault(o => o.Id == answer.SelectedOptionId.Value);
                        if (selectedOption != null)
                        {
                            if (selectedOption.IsCorrect)
                            {
                                isCorrect = true;
                            }
                            else if (!string.IsNullOrEmpty(question.CorrectAnswer))
                            {
                                var trimmedCorrect = question.CorrectAnswer.Trim();
                                if (selectedOption.OptionText.Trim().Equals(trimmedCorrect, StringComparison.OrdinalIgnoreCase) ||
                                    selectedOption.Id.ToString() == trimmedCorrect)
                                {
                                    isCorrect = true;
                                }
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(answer.AnswerText) && !string.IsNullOrEmpty(question.CorrectAnswer))
                    {
                        if (answer.AnswerText.Trim().Equals(question.CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            isCorrect = true;
                        }
                    }
                }
                else if (question.QuestionType == "SHORT_ANSWER")
                {
                    if (!string.IsNullOrEmpty(answer.AnswerText) && !string.IsNullOrEmpty(question.CorrectAnswer))
                    {
                        if (answer.AnswerText.Trim().Equals(question.CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            isCorrect = true;
                        }
                    }
                }

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

                _dbContext.TestAnswers.Update(answer);
            }

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

            var ptQuestions = await _dbContext.PlacementTestQuestions
                .Include(pq => pq.Section)
                .ThenInclude(s => s.Skill)
                .Where(pq => pq.Section.PlacementTestId == attempt.PlacementTestId)
                .ToListAsync();

            var answersDict = attempt.TestAnswers.ToDictionary(a => a.QuestionId);

            var result = new List<SkillScoreDto>();
            var skillGroups = ptQuestions.GroupBy(pq => pq.Section.Skill);

            foreach (var group in skillGroups)
            {
                var skill = group.Key;
                decimal maxScore = group.Sum(pq => pq.Points);
                decimal earnedScore = 0;

                foreach (var pq in group)
                {
                    if (answersDict.TryGetValue(pq.QuestionId, out var ans))
                    {
                        earnedScore += ans.Score ?? 0;
                    }
                }

                decimal percentage = maxScore > 0 ? (earnedScore / maxScore) * 100 : 0;

                result.Add(new SkillScoreDto
                {
                    SkillId = skill.Id,
                    SkillName = skill.SkillName,
                    EarnedScore = earnedScore,
                    MaxScore = maxScore,
                    Percentage = percentage
                });
            }

            return result;
        }

        public async Task<List<TopicScoreDto>> CalculateTopicScoresAsync(int attemptId)
        {
            var attempt = await _dbContext.TestAttempts
                .Include(a => a.TestAnswers)
                .FirstOrDefaultAsync(a => a.Id == attemptId);

            if (attempt == null) return new List<TopicScoreDto>();

            var ptQuestions = await _dbContext.PlacementTestQuestions
                .Include(pq => pq.Question)
                .ThenInclude(q => q.Topic)
                .Where(pq => pq.Section.PlacementTestId == attempt.PlacementTestId)
                .ToListAsync();

            var answersDict = attempt.TestAnswers.ToDictionary(a => a.QuestionId);

            var result = new List<TopicScoreDto>();
            var topicGroups = ptQuestions
                .Where(pq => pq.Question.Topic != null)
                .GroupBy(pq => pq.Question.Topic!);

            foreach (var group in topicGroups)
            {
                var topic = group.Key;
                decimal maxScore = group.Sum(pq => pq.Points);
                decimal earnedScore = 0;

                foreach (var pq in group)
                {
                    if (answersDict.TryGetValue(pq.QuestionId, out var ans))
                    {
                        earnedScore += ans.Score ?? 0;
                    }
                }

                decimal percentage = maxScore > 0 ? (earnedScore / maxScore) * 100 : 0;

                result.Add(new TopicScoreDto
                {
                    TopicId = topic.Id,
                    TopicName = topic.Title,
                    EarnedScore = earnedScore,
                    MaxScore = maxScore,
                    Percentage = percentage
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

            // Level mapping according to user requirement:
            // 0–39% Beginner
            // 40–59% Elementary
            // 60–74% Intermediate
            // 75–89% Upper-Intermediate
            // 90–100% Advanced
            string levelCode;
            string levelName;
            string description;

            if (percentage >= 90)
            {
                levelCode = "ADVANCED";
                levelName = "Advanced";
                description = "Trình độ cao cấp (Advanced).";
            }
            else if (percentage >= 75)
            {
                levelCode = "UPPER_INTERMEDIATE";
                levelName = "Upper-Intermediate";
                description = "Trình độ trung cấp trên (Upper-Intermediate).";
            }
            else if (percentage >= 60)
            {
                levelCode = "INTERMEDIATE";
                levelName = "Intermediate";
                description = "Trình độ trung cấp (Intermediate).";
            }
            else if (percentage >= 40)
            {
                levelCode = "ELEMENTARY";
                levelName = "Elementary";
                description = "Trình độ sơ cấp (Elementary).";
            }
            else
            {
                levelCode = "BEGINNER";
                levelName = "Beginner";
                description = "Trình độ người mới bắt đầu (Beginner).";
            }

            // Find matching level entity in DB by Code or Name
            var level = await _dbContext.EnglishProficiencyLevels
                .FirstOrDefaultAsync(l => l.Code == levelCode || 
                                          l.Name == levelName ||
                                          (levelCode == "BEGINNER" && (l.Code == "A1" || l.Name == "Beginner")) ||
                                          (levelCode == "ELEMENTARY" && (l.Code == "A2" || l.Name == "Elementary")) ||
                                          (levelCode == "INTERMEDIATE" && (l.Code == "B1" || l.Name == "Intermediate")) ||
                                          (levelCode == "UPPER_INTERMEDIATE" && (l.Code == "B2" || l.Name == "Upper Intermediate" || l.Name == "Upper-Intermediate")) ||
                                          (levelCode == "ADVANCED" && (l.Code == "C1" || l.Code == "C2" || l.Name == "Advanced")));

            return new EstimatedLevelDto
            {
                LevelId = level?.Id,
                LevelName = levelName,
                Percentage = percentage,
                Description = description
            };
        }
    }
}
