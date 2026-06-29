using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models.DTOs;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Services
{
    public class AIContentReviewService : IAIContentReviewService
    {
        private readonly ApplicationDbContext _db;

        public AIContentReviewService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<AiContentListItemViewModel>> GetPendingContentAsync(
            string? skillFilter = null,
            int? topicIdFilter = null,
            string? contentTypeFilter = null,
            int? requestedByFilter = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = _db.AiGeneratedContents
                .Where(a => a.ReviewStatus == "PENDING")
                .Include(a => a.RequestedByNavigation)
                .Include(a => a.RelatedTopic)
                .AsQueryable();

            if (topicIdFilter.HasValue) query = query.Where(a => a.RelatedTopicId == topicIdFilter);
            if (!string.IsNullOrEmpty(contentTypeFilter)) query = query.Where(a => a.ContentType == contentTypeFilter);
            if (requestedByFilter.HasValue) query = query.Where(a => a.RequestedBy == requestedByFilter);

            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AiContentListItemViewModel
                {
                    Id = a.Id,
                    ContentType = a.ContentType,
                    QuestionPreview = a.GeneratedContent != null ? ExtractQuestionPreview(a.GeneratedContent) : null,
                    RequestedBy = a.RequestedBy,
                    RequestedByName = a.RequestedByNavigation != null ? a.RequestedByNavigation.FullName : null,
                    ReviewStatus = a.ReviewStatus,
                    CreatedAt = a.CreatedAt,
                    RelatedTopicId = a.RelatedTopicId,
                    TopicName = a.RelatedTopic != null ? a.RelatedTopic.Title : null
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<AiContentListItemViewModel> { Items = items, TotalCount = total, PageIndex = page, PageSize = pageSize };
        }

        public async Task<AiContentReviewDetailViewModel?> GetContentDetailAsync(int id, CancellationToken cancellationToken = default)
        {
            var item = await _db.AiGeneratedContents
                .Include(a => a.RequestedByNavigation)
                .Include(a => a.RelatedTopic)
                .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

            if (item == null) return null;

            var vm = new AiContentReviewDetailViewModel
            {
                Id = item.Id,
                ContentType = item.ContentType,
                GeneratedContent = item.GeneratedContent,
                PromptText = item.PromptText,
                ReviewStatus = item.ReviewStatus,
                CreatedAt = item.CreatedAt,
                RequestedBy = item.RequestedBy,
                RequestedByName = item.RequestedByNavigation?.FullName,
                RelatedTopicId = item.RelatedTopicId,
                TopicName = item.RelatedTopic?.Title
            };

            // Try to parse GeneratedContent JSON
            if (!string.IsNullOrEmpty(item.GeneratedContent))
            {
                try
                {
                    using var doc = JsonDocument.Parse(item.GeneratedContent);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("question_text", out var q)) vm.QuestionText = q.GetString();
                    if (root.TryGetProperty("options", out var opts)) vm.Options = opts.GetRawText();
                    if (root.TryGetProperty("correct_answer_index", out var idx) && idx.TryGetInt32(out var i)) vm.CorrectAnswerIndex = i;
                    if (root.TryGetProperty("explanation", out var ex)) vm.Explanation = ex.GetString();
                    if (root.TryGetProperty("difficulty", out var df)) vm.Difficulty = df.GetString();
                    if (root.TryGetProperty("skill_tags", out var tags)) vm.SkillTags = tags.GetRawText();
                }
                catch { }
            }

            // Set published question ID if available
            vm.PublishedQuestionId = item.PublishedQuestionId;

            return vm;
        }

        public async Task ApproveAsync(int id, int reviewerId, bool copyrightCheck, string? plagiarismRisk, string? reviewNote, string? editedQuestionText, string? editedExplanation, CancellationToken cancellationToken = default)
        {
            var item = await _db.AiGeneratedContents.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
            if (item == null) throw new Exception("Item not found.");

            // Update content if edited
            if (!string.IsNullOrEmpty(editedQuestionText) || !string.IsNullOrEmpty(editedExplanation))
            {
                try
                {
                    using var doc = JsonDocument.Parse(item.GeneratedContent ?? "{}");
                    var json = JsonSerializer.Deserialize<Dictionary<string, object>>(item.GeneratedContent ?? "{}");
                    if (json != null)
                    {
                        if (!string.IsNullOrEmpty(editedQuestionText)) json["question_text"] = editedQuestionText;
                        if (!string.IsNullOrEmpty(editedExplanation)) json["explanation"] = editedExplanation;
                        item.GeneratedContent = JsonSerializer.Serialize(json);
                    }
                }
                catch { }
            }

            // Update review fields
            item.ReviewStatus = "APPROVED";
            item.ReviewedBy = reviewerId;
            item.ReviewedAt = DateTime.UtcNow;
            item.ReviewNote = reviewNote;

            _db.AiGeneratedContents.Update(item);

            // Log compliance review
            var compliance = new ContentComplianceReview
            {
                ContentId = id,
                ContentType = "AI_GENERATED_CONTENT",
                ReviewerId = reviewerId,
                ReviewStatus = "APPROVED",
                CopyrightCheck = copyrightCheck,
                PlagiarismRisk = plagiarismRisk,
                ReviewNote = reviewNote,
                ReviewedAt = DateTime.UtcNow
            };
            _db.ContentComplianceReviews.Add(compliance);

            // Audit log
            var auditLog = new AuditLog
            {
                EntityName = "ai_generated_contents",
                EntityId = id,
                Action = "APPROVE",
                UserId = reviewerId,
                OldValue = "PENDING",
                NewValue = "APPROVED",
                CreatedAt = DateTime.UtcNow
            };
            _db.AuditLogs.Add(auditLog);

            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task RejectAsync(int id, int reviewerId, string? reviewNote, CancellationToken cancellationToken = default)
        {
            var item = await _db.AiGeneratedContents.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
            if (item == null) throw new Exception("Item not found.");
            if (string.IsNullOrWhiteSpace(reviewNote)) throw new Exception("Review note is required for rejection.");

            item.ReviewStatus = "REJECTED";
            item.ReviewedBy = reviewerId;
            item.ReviewedAt = DateTime.UtcNow;
            item.ReviewNote = reviewNote;

            _db.AiGeneratedContents.Update(item);

            var compliance = new ContentComplianceReview
            {
                ContentId = id,
                ContentType = "AI_GENERATED_CONTENT",
                ReviewerId = reviewerId,
                ReviewStatus = "REJECTED",
                ReviewNote = reviewNote,
                ReviewedAt = DateTime.UtcNow
            };
            _db.ContentComplianceReviews.Add(compliance);

            var auditLog = new AuditLog
            {
                EntityName = "ai_generated_contents",
                EntityId = id,
                Action = "REJECT",
                UserId = reviewerId,
                OldValue = "PENDING",
                NewValue = "REJECTED",
                CreatedAt = DateTime.UtcNow
            };
            _db.AuditLogs.Add(auditLog);

            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task RequestRevisionAsync(int id, int reviewerId, string? reviewNote, CancellationToken cancellationToken = default)
        {
            var item = await _db.AiGeneratedContents.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
            if (item == null) throw new Exception("Item not found.");

            item.ReviewStatus = "NEEDS_REVISION";
            item.ReviewedBy = reviewerId;
            item.ReviewedAt = DateTime.UtcNow;
            item.ReviewNote = reviewNote;

            _db.AiGeneratedContents.Update(item);

            var compliance = new ContentComplianceReview
            {
                ContentId = id,
                ContentType = "AI_GENERATED_CONTENT",
                ReviewerId = reviewerId,
                ReviewStatus = "NEEDS_REVISION",
                ReviewNote = reviewNote,
                ReviewedAt = DateTime.UtcNow
            };
            _db.ContentComplianceReviews.Add(compliance);

            var auditLog = new AuditLog
            {
                EntityName = "ai_generated_contents",
                EntityId = id,
                Action = "REQUEST_REVISION",
                UserId = reviewerId,
                OldValue = "PENDING",
                NewValue = "NEEDS_REVISION",
                CreatedAt = DateTime.UtcNow
            };
            _db.AuditLogs.Add(auditLog);

            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<Dictionary<string, int>> GetPendingCountByTypeAsync(CancellationToken cancellationToken = default)
        {
            var counts = await _db.AiGeneratedContents
                .Where(a => a.ReviewStatus == "PENDING")
                .GroupBy(a => a.ContentType)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            return counts.ToDictionary(x => x.Type ?? "Unknown", x => x.Count);
        }

        private string ExtractQuestionPreview(string generatedContent)
        {
            try
            {
                using var doc = JsonDocument.Parse(generatedContent);
                if (doc.RootElement.TryGetProperty("question_text", out var q))
                    return q.GetString()?.Substring(0, Math.Min(100, q.GetString()?.Length ?? 0)) ?? "";
            }
            catch { }
            return "(No preview)";
        }
    }
}
