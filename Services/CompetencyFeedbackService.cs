using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DuAnTotNghiep.Services
{
    public class CompetencyFeedbackService : ICompetencyFeedbackService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<CompetencyFeedbackService> _log;

        // Ngưỡng số câu hỏi tối thiểu để đảm bảo dữ liệu đủ tin cậy
        private const int MinQuestionThreshold = 10;

        public CompetencyFeedbackService(ApplicationDbContext db, ILogger<CompetencyFeedbackService> log)
        {
            _db = db;
            _log = log;
        }

        public async Task<CompetencyFeedbackDTO> ParseAndFormatAiFeedbackAsync(int analysisId, int currentUserId)
        {
            // ── 1. Load báo cáo từ DB kèm các navigation property ────────
            var analysis = await _db.CompetencyAnalyses
                .Include(a => a.CurrentLevel)
                .Include(a => a.RecommendedLevel)
                .Include(a => a.TestAttempt)
                    .ThenInclude(t => t.TestAnswers)
                .Include(a => a.CompetencySkillScores)
                    .ThenInclude(s => s.Skill)
                .Include(a => a.CompetencySkillScores)
                    .ThenInclude(s => s.Topic)
                .FirstOrDefaultAsync(a => a.Id == analysisId);

            if (analysis == null)
                throw new KeyNotFoundException($"CompetencyAnalysis {analysisId} không tồn tại.");

            // ── 2. Ownership check: học sinh chỉ được xem báo cáo của mình ──
            // Admin/Teacher được bỏ qua check này ở Controller trước khi gọi service
            if (analysis.StudentId != currentUserId)
            {
                _log.LogWarning("User {U} cố truy cập analysis {A} không thuộc về mình.", currentUserId, analysisId);
                throw new UnauthorizedAccessException("Bạn không có quyền xem báo cáo này.");
            }

            var dto = new CompetencyFeedbackDTO
            {
                AnalysisId          = analysis.Id,
                CurrentLevelName    = analysis.CurrentLevel?.Name ?? "Chưa xác định",
                RecommendedLevelName = analysis.RecommendedLevel?.Name ?? "Chưa xác định",
                ConfidenceScore     = analysis.ConfidenceScore ?? 0
            };

            // ── 3. Kiểm tra dữ liệu mỏng (thin data) ────────────────────
            // Nếu attempt có quá ít câu hỏi → không nên kết luận chắc chắn
            var totalAnswers = analysis.TestAttempt?.TestAnswers?.Count ?? 0;
            if (totalAnswers < MinQuestionThreshold)
            {
                dto.IsDataThinWarning = true;
                dto.DataThinMessage   = "Dữ liệu hiện tại chưa đủ để đánh giá sâu. " +
                    "Hệ thống khuyến nghị bạn hoàn thành thêm các bài kiểm tra bổ trợ " +
                    "để nhận được phân tích chính xác nhất.";
                _log.LogWarning("Analysis {Id}: chỉ có {N} câu, dữ liệu quá mỏng.", analysisId, totalAnswers);
            }

            // ── 4. Tạo Dashboard Summary (tối đa 250 ký tự) ──────────────
            var rawSummary = analysis.Summary ?? "";
            dto.DashboardSummary = rawSummary.Length > 250
                ? rawSummary.Substring(0, 250) + "..."
                : rawSummary;

            // ── 5. Parse chuỗi JSON Strengths → List<FeedbackItemDTO> ─────
            // AI lưu strengths dưới dạng JSON array: [{"title":"...","description":"..."}]
            dto.Strengths = ParseFeedbackJson(analysis.Strengths, "strengths", analysisId);

            // ── 6. Parse chuỗi JSON Weaknesses → List<FeedbackItemDTO> ────
            dto.Weaknesses = ParseFeedbackJson(analysis.Weaknesses, "weaknesses", analysisId);

            // ── 7. Tạo Recommended Actions từ weaknesses + gap_analysis ───
            dto.RecommendedActions = await GenerateRecommendedActionsAsync(analysisId);

            return dto;
        }

        public async Task<List<RecommendedActionDTO>> GenerateRecommendedActionsAsync(int analysisId)
        {
            var actions = new List<RecommendedActionDTO>();

            // Load skill scores kèm skill và topic
            var skillScores = await _db.CompetencySkillScores
                .Include(s => s.Skill)
                .Include(s => s.Topic)
                .Where(s => s.CompetencyAnalysisId == analysisId)
                .OrderBy(s => s.PriorityLevel)
                .ToListAsync();

            // Load gap_analysis từ bảng cha để bổ sung action text
            var analysis = await _db.CompetencyAnalyses
                .Where(a => a.Id == analysisId)
                .Select(a => new { a.GapAnalysis, a.MetadataJson })
                .FirstOrDefaultAsync();

            // ── Parse GapItems từ JSON ────────────────────────────────────
            // gap_analysis được lưu dạng JSON array của GapItem {topicId, description}
            var gapItems = new List<GapItem>();
            if (!string.IsNullOrEmpty(analysis?.GapAnalysis))
            {
                try
                {
                    gapItems = JsonSerializer.Deserialize<List<GapItem>>(
                        analysis.GapAnalysis,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        ?? new List<GapItem>();
                }
                catch (Exception ex)
                {
                    _log.LogWarning(ex, "Analysis {Id}: Không parse được GapAnalysis JSON.", analysisId);
                }
            }

            // ── Parse RecommendedActions từ MetadataJson ──────────────────
            // metadata_json chứa {"RecommendedActions": ["...", "..."]}
            var rawActions = new List<string>();
            if (!string.IsNullOrEmpty(analysis?.MetadataJson))
            {
                try
                {
                    using var doc = JsonDocument.Parse(analysis.MetadataJson);
                    if (doc.RootElement.TryGetProperty("RecommendedActions", out var arr))
                        rawActions = JsonSerializer.Deserialize<List<string>>(arr.GetRawText()) ?? new();
                }
                catch (Exception ex)
                {
                    _log.LogWarning(ex, "Analysis {Id}: Không parse được RecommendedActions.", analysisId);
                }
            }

            // ── Mapping: mỗi skill score yếu → 1 action gợi ý ─────────────
            // Ưu tiên theo PriorityLevel (1 = cần làm ngay nhất)
            int rank = 1;
            foreach (var ss in skillScores.Where(s => s.Score < 60))
            {
                // Xây dựng lời khuyên dương tính, tránh lời tiêu cực
                var skillName  = ss.Skill?.SkillName ?? "kỹ năng này";
                var topicTitle = ss.Topic?.Title;

                var actionText = topicTitle != null
                    ? $"Hãy ôn luyện thêm chủ đề '{topicTitle}' để củng cố {skillName}."
                    : $"Cần luyện tập thêm về {skillName} để nâng cao hiệu quả.";

                // Nếu có gap description trùng topicId → thêm vào action text
                var matchedGap = gapItems.FirstOrDefault(g => g.TopicId == ss.TopicId);
                if (matchedGap != null)
                    actionText += $" Lỗ hổng cụ thể: {matchedGap.Description}";

                actions.Add(new RecommendedActionDTO
                {
                    ActionText  = actionText,
                    SkillName   = ss.Skill?.SkillName,
                    TopicTitle  = topicTitle,
                    TopicId     = ss.TopicId,
                    Priority    = rank++
                });
            }

            // ── Bổ sung raw actions từ AI (các action không map được sang topic) ──
            foreach (var raw in rawActions.Take(5))
            {
                // Tránh trùng lặp nếu action đã được map từ skill score
                if (!actions.Any(a => a.ActionText.Contains(raw.Substring(0, Math.Min(30, raw.Length)))))
                {
                    actions.Add(new RecommendedActionDTO
                    {
                        ActionText = raw,
                        Priority   = rank++
                    });
                }
            }

            // Fallback nếu không có action nào (học sinh làm tốt hết)
            if (actions.Count == 0)
            {
                actions.Add(new RecommendedActionDTO
                {
                    ActionText = "Bạn đang học rất tốt! Hãy tiếp tục duy trì và thử thách bản thân với các bài tập nâng cao.",
                    Priority   = 1
                });
            }

            return actions;
        }

        // ── Helper: Parse JSON array thô → List<FeedbackItemDTO> ─────────
        // JSON có dạng: [{"title":"...", "description":"..."}, ...]
        // Nếu AI trả về dạng khác hoặc lỗi JSON → trả về list trống + log warning
        private List<FeedbackItemDTO> ParseFeedbackJson(string? json, string fieldName, int analysisId)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<FeedbackItemDTO>();

            try
            {
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var parsed = JsonSerializer.Deserialize<List<FeedbackItemDTO>>(json, opts);
                return parsed ?? new List<FeedbackItemDTO>();
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Analysis {Id}: Không parse được field '{Field}'. Trả về rỗng.", analysisId, fieldName);
                return new List<FeedbackItemDTO>();
            }
        }
    }
}
