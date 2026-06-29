using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services
{
    public class PublishingService : IPublishingService
    {
        private readonly ApplicationDbContext _db;

        public PublishingService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<PublishResult> PublishToQuestionBankAsync(int aiContentId, int reviewedByUserId, CancellationToken cancellationToken = default)
        {
            try
            {
                var aiContent = await _db.AiGeneratedContents
                    .Include(a => a.RelatedTopic)
                    .FirstOrDefaultAsync(a => a.Id == aiContentId, cancellationToken);

                if (aiContent == null)
                    return new PublishResult { IsSuccess = false, ErrorMessage = "AI content not found." };

                // Validation: must be APPROVED
                if (aiContent.ReviewStatus != "APPROVED")
                    return new PublishResult { IsSuccess = false, ErrorMessage = $"Content must be APPROVED to publish. Current status: {aiContent.ReviewStatus}" };

                // Already published?
                if (aiContent.PublishedQuestionId.HasValue)
                    return new PublishResult { IsSuccess = false, ErrorMessage = "This content has already been published." };

                // Parse AI generated content JSON
                var questionText = "";
                var optionsList = new List<string>();
                var correctAnswerIndex = -1;
                var explanation = "";
                var difficulty = "MEDIUM";
                var skillTags = new List<string>();

                try
                {
                    using var doc = JsonDocument.Parse(aiContent.GeneratedContent ?? "{}");
                    var root = doc.RootElement;

                    if (root.TryGetProperty("question_text", out var qText)) questionText = qText.GetString();
                    if (root.TryGetProperty("options", out var opts)) optionsList = opts.EnumerateArray().Select(o => o.GetString()).ToList();
                    if (root.TryGetProperty("correct_answer_index", out var cai)) correctAnswerIndex = cai.GetInt32();
                    if (root.TryGetProperty("explanation", out var exp)) explanation = exp.GetString();
                    if (root.TryGetProperty("difficulty", out var diff)) difficulty = diff.GetString();
                    if (root.TryGetProperty("skill_tags", out var tags)) skillTags = tags.EnumerateArray().Select(t => t.GetString()).ToList();
                }
                catch (Exception ex)
                {
                    return new PublishResult { IsSuccess = false, ErrorMessage = $"Failed to parse AI content: {ex.Message}" };
                }

                if (string.IsNullOrWhiteSpace(questionText))
                    return new PublishResult { IsSuccess = false, ErrorMessage = "Question text is empty." };

                // Check for duplicates (exact match on normalized question_text)
                var normalizedNew = NormalizeText(questionText);
                var existingDuplicate = await _db.QuestionBanks
                    .Where(q => q.TopicId == aiContent.RelatedTopicId)
                    .ToListAsync(cancellationToken);

                if (existingDuplicate.Any(q => NormalizeText(q.QuestionText) == normalizedNew))
                    return new PublishResult { IsSuccess = false, ErrorMessage = "A similar question already exists in the Question Bank for this topic." };

                // Get skill_id from topic or from skill_tags
                int? skillId = null;
                if (aiContent.RelatedTopic != null)
                {
                    skillId = aiContent.RelatedTopic.SkillId;
                }

                if (!skillId.HasValue)
                    return new PublishResult { IsSuccess = false, ErrorMessage = "Cannot determine skill for this content." };

                // Start transaction
                using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    // Determine question_type from ContentType
                    var questionType = aiContent.ContentType == "QUESTION" ? "MCQ" : "MCQ"; // Default: MCQ for now
                    
                    // Determine level_id (default to NULL, user can set later)
                    int? levelId = null;

                    // Determine difficulty level value
                    string difficultyValue = difficulty ?? "MEDIUM";

                    // Create QuestionBank entry
                    var qbEntry = new QuestionBank
                    {
                        TopicId = aiContent.RelatedTopicId,
                        SkillId = skillId.Value,
                        LevelId = levelId,
                        QuestionType = questionType,
                        QuestionText = questionText,
                        Explanation = explanation,
                        DifficultyLevel = difficultyValue,
                        SourceType = "AI_GENERATED",
                        ReviewStatus = "PUBLISHED",
                        CreatedBy = reviewedByUserId,
                        ApprovedBy = reviewedByUserId,
                        ApprovedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    };

                    _db.QuestionBanks.Add(qbEntry);
                    await _db.SaveChangesAsync(cancellationToken);

                    int questionId = qbEntry.Id;

                    // Add question options if MCQ
                    if (optionsList.Count > 0 && correctAnswerIndex >= 0 && correctAnswerIndex < optionsList.Count)
                    {
                        foreach (var (optionText, index) in optionsList.Select((o, i) => (o, i)))
                        {
                            var option = new QuestionOption
                            {
                                QuestionId = questionId,
                                OptionText = optionText,
                                IsCorrect = index == correctAnswerIndex,
                                OrderIndex = index + 1
                            };
                            _db.QuestionOptions.Add(option);
                        }

                        await _db.SaveChangesAsync(cancellationToken);
                    }

                    // Update ai_generated_contents with published_question_id
                    aiContent.PublishedQuestionId = questionId;
                    _db.AiGeneratedContents.Update(aiContent);
                    await _db.SaveChangesAsync(cancellationToken);

                    // Commit transaction
                    await transaction.CommitAsync(cancellationToken);

                    return new PublishResult
                    {
                        IsSuccess = true,
                        Message = $"Successfully published to Question Bank (ID: {questionId})",
                        PublishedQuestionId = questionId
                    };
                }
                catch (Exception ex)
                {
                    // Rollback on any error
                    await transaction.RollbackAsync(cancellationToken);
                    return new PublishResult { IsSuccess = false, ErrorMessage = $"Transaction failed: {ex.Message}" };
                }
            }
            catch (Exception ex)
            {
                return new PublishResult { IsSuccess = false, ErrorMessage = $"Unexpected error: {ex.Message}" };
            }
        }

        public async Task<PublishBatchResult> PublishBatchAsync(string batchId, int publishedByUserId, bool createQuizDraft = false, string quizTitle = null, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get all APPROVED items in batch (or all APPROVED if batchId is null/empty)
                IQueryable<AiGeneratedContent> query = _db.AiGeneratedContents
                    .Where(a => a.ReviewStatus == "APPROVED")
                    .Include(a => a.RelatedTopic);

                if (!string.IsNullOrWhiteSpace(batchId))
                {
                    query = query.Where(a => a.BatchId == batchId);
                }

                var batchItems = await query.ToListAsync(cancellationToken);

                if (batchItems.Count == 0)
                    return new PublishBatchResult 
                    { 
                        IsSuccess = false, 
                        ErrorMessage = "No approved items found in batch." 
                    };

                int publishedCount = 0;
                int failureCount = 0;
                var publishedQuestionIds = new List<int>();

                // Publish each item
                foreach (var item in batchItems)
                {
                    var result = await PublishToQuestionBankAsync(item.Id, publishedByUserId, cancellationToken);
                    if (result.IsSuccess && result.PublishedQuestionId.HasValue)
                    {
                        publishedCount++;
                        publishedQuestionIds.Add(result.PublishedQuestionId.Value);
                    }
                    else
                    {
                        failureCount++;
                    }
                }

                // Create quiz draft if requested and we have published items
                int? createdQuizId = null;
                if (createQuizDraft && publishedQuestionIds.Count > 0)
                {
                    try
                    {
                        var firstItem = batchItems.First(a => a.PublishedQuestionId.HasValue);
                        int skillId = firstItem.RelatedTopic?.SkillId ?? 1;

                        var quiz = new Quiz
                        {
                            TopicId = firstItem.RelatedTopicId,
                            SkillId = skillId,
                            Title = quizTitle ?? $"Auto-Generated Quiz from Batch {batchId}",
                            Description = $"Draft quiz created from {publishedCount} published AI-generated questions",
                            QuizType = "PRACTICE",
                            Status = "DRAFT",
                            CreatedBy = publishedByUserId,
                            CreatedAt = DateTime.UtcNow
                        };

                        _db.Quizzes.Add(quiz);
                        await _db.SaveChangesAsync(cancellationToken);

                        createdQuizId = quiz.Id;

                        // Add questions to quiz
                        foreach (var (qId, order) in publishedQuestionIds.Select((q, i) => (q, i + 1)))
                        {
                            var qqEntry = new QuizQuestion
                            {
                                QuizId = createdQuizId.Value,
                                QuestionId = qId,
                                Points = 1  // Default points per question
                            };
                            _db.QuizQuestions.Add(qqEntry);
                        }

                        await _db.SaveChangesAsync(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        // Quiz creation failure doesn't rollback published items
                        return new PublishBatchResult
                        {
                            IsSuccess = false,
                            PublishedCount = publishedCount,
                            FailureCount = failureCount,
                            ErrorMessage = $"Published {publishedCount} items but failed to create quiz: {ex.Message}"
                        };
                    }
                }

                return new PublishBatchResult
                {
                    IsSuccess = true,
                    PublishedCount = publishedCount,
                    FailureCount = failureCount,
                    CreatedQuizId = createdQuizId,
                    Message = $"Successfully published {publishedCount} items to Question Bank" + (createdQuizId.HasValue ? $", created quiz (ID: {createdQuizId})" : "")
                };
            }
            catch (Exception ex)
            {
                return new PublishBatchResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Batch publish failed: {ex.Message}"
                };
            }
        }

        private string NormalizeText(string text)
        {
            // Simple normalization: lowercase + trim extra spaces
            return System.Text.RegularExpressions.Regex.Replace(text?.ToLower().Trim() ?? "", @"\s+", " ");
        }
    }
}
