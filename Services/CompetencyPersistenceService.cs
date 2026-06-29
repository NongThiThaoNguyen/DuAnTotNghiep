using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.Repositories.Interfaces;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DuAnTotNghiep.Services
{
    public class CompetencyPersistenceService : ICompetencyPersistenceService
    {
        private readonly ApplicationDbContext _db;
        private readonly ICompetencyAnalysisRepository _repo;
        private readonly ILogger<CompetencyPersistenceService> _log;
        private readonly IAuditService _auditService;

        public CompetencyPersistenceService(
            ApplicationDbContext db,
            ICompetencyAnalysisRepository repo,
            ILogger<CompetencyPersistenceService> log,
            IAuditService auditService)
        {
            _db = db;
            _repo = repo;
            _log = log;
            _auditService = auditService;
        }

        public async Task<PersistenceResultDTO> SaveAiAnalysisTransactionAsync(
            int studentId,
            int attemptId,
            AssessmentAiResponse aiResponse)
        {
            // ── 1. Validation dữ liệu bắt buộc từ AI response ────────────
            if (string.IsNullOrWhiteSpace(aiResponse.Summary))
                return PersistenceResultDTO.Fail("AI response thiếu field bắt buộc: summary.");

            if (string.IsNullOrWhiteSpace(aiResponse.CurrentLevel))
                return PersistenceResultDTO.Fail("AI response thiếu field bắt buộc: currentLevel.");

            if (string.IsNullOrWhiteSpace(aiResponse.RecommendedLevel))
                return PersistenceResultDTO.Fail("AI response thiếu field bắt buộc: recommendedLevel.");

            // ── 2. Ownership & status check (chống tấn công thay ID trên URL) ──
            // Kiểm tra attempt có thuộc về studentId và đã submit/graded chưa
            var attempt = await _db.TestAttempts
                .FirstOrDefaultAsync(t => t.Id == attemptId);

            if (attempt == null)
                return PersistenceResultDTO.Fail($"TestAttempt {attemptId} không tồn tại.");

            // Ownership: nếu không phải chủ sở hữu → từ chối, tránh IDOR attack
            if (attempt.StudentId != studentId)
            {
                _log.LogWarning("StudentId {S} cố lưu analysis cho attemptId {A} không thuộc về mình.", studentId, attemptId);
                return PersistenceResultDTO.Fail("Bạn không có quyền lưu kết quả cho bài thi này.");
            }

            // Trạng thái phải là SUBMITTED hoặc GRADED
            if (attempt.Status != "SUBMITTED" && attempt.Status != "GRADED")
                return PersistenceResultDTO.Fail($"Bài thi đang ở trạng thái '{attempt.Status}', chưa thể lưu phân tích.");

            // ── 3. Resolve level IDs từ code CEFR ────────────────────────
            var levels = await _db.EnglishProficiencyLevels.ToListAsync();
            var curLevel  = levels.FirstOrDefault(l => l.Code == aiResponse.CurrentLevel);
            var recLevel  = levels.FirstOrDefault(l => l.Code == aiResponse.RecommendedLevel);

            // ── 4. Ép kiểu danh sách priority topics → chuỗi JSON ────────
            // priorityTopics từ AI là List<PriorityTopicItem>, ta serialize thành JSON
            // để lưu vào cột prioritized_topics_json (NVARCHAR MAX)
            var priorityJson = JsonSerializer.Serialize(
                aiResponse.PriorityTopics?.Select(p => p.TopicId).ToList() ?? new List<int>());

            var gapJson    = JsonSerializer.Serialize(aiResponse.Gaps ?? new List<GapItem>());
            var actionJson = JsonSerializer.Serialize(aiResponse.RecommendedActions ?? new List<string>());

            // ── 5. Map AI response → CompetencyAnalysis entity (bảng cha) ──
            var analysis = new CompetencyAnalysis
            {
                StudentId            = studentId,
                TestAttemptId        = attemptId,
                Summary              = aiResponse.Summary,
                CurrentLevelId       = curLevel?.Id,
                RecommendedLevelId   = recLevel?.Id,
                Strengths            = JsonSerializer.Serialize(aiResponse.Strengths ?? new List<StrengthItem>()),
                Weaknesses           = JsonSerializer.Serialize(aiResponse.Weaknesses ?? new List<WeaknessItem>()),
                GapAnalysis          = gapJson,
                PrioritizedTopicsJson = priorityJson,
                KnowledgeGapsJson    = gapJson,
                MetadataJson         = JsonSerializer.Serialize(new { RecommendedActions = aiResponse.RecommendedActions }),
                ConfidenceScore      = aiResponse.ConfidenceScore,
                CreatedAt            = DateTime.UtcNow
            };

            // ── 6. Map priority topics → CompetencySkillScore (bảng con) ──
            // Với mỗi skill score từ AI response, validate FK trước khi thêm
            var skillScores = new List<CompetencySkillScore>();

            if (aiResponse.PriorityTopics != null)
            {
                var rank = 1;
                foreach (var pt in aiResponse.PriorityTopics)
                {
                    // Validate topicId tồn tại trước khi gán FK
                    if (!await _repo.ValidateTopicExistsAsync(pt.TopicId))
                    {
                        // Ghi warning, bỏ qua dòng lỗi thay vì crash
                        _log.LogWarning("TopicId {T} từ AI không tồn tại trong hệ thống. Bỏ qua.", pt.TopicId);
                        continue;
                    }

                    skillScores.Add(new CompetencySkillScore
                    {
                        // CompetencyAnalysisId sẽ được gán trong repo sau khi bảng cha insert
                        SkillId       = 0,    // topicId không map trực tiếp sang skillId ở đây
                        TopicId       = pt.TopicId,
                        Score         = 0,    // sẽ được cập nhật bởi Task 6 nếu cần
                        PriorityLevel = rank++,
                        WeaknessNote  = null
                    });
                }
            }

            // Nếu muốn lưu skill scores từ strengths/weaknesses thì bổ sung ở đây
            // Ở đây ta chỉ lưu các topic priority theo yêu cầu Task 7

            // ── 7. Gọi Repository thực hiện transaction bọc 2 bảng cha con ──
            try
            {
                var newId = await _repo.AddAnalysisWithScoresAsync(analysis, skillScores);

                // ── 8. Ghi nhận Audit Log (Action: generate) với cơ chế cô lập lỗi ──
                try
                {
                    var payload = JsonSerializer.Serialize(new
                    {
                        AttemptId = attemptId,
                        ConfidenceScore = aiResponse.ConfidenceScore,
                        SummarySnippet = aiResponse.Summary.Length > 100 ? aiResponse.Summary.Substring(0, 100) + "..." : aiResponse.Summary
                    });

                    // IpAddress được truyền qua HttpContext ở Controller, ở đây tạm để null hoặc lấy từ context nếu có.
                    // Do tầng Service không trực tiếp giữ HttpContext, ta ghi nhận thông tin thực thể cơ bản.
                    await _auditService.LogActionAsync(
                        userId: studentId,
                        action: "generate",
                        entityName: "CompetencyAnalysis",
                        entityId: newId,
                        oldValue: null,
                        newValue: payload,
                        ipAddress: null
                    );
                }
                catch (Exception auditEx)
                {
                    // Fallback Logging: Ghi log thô vào file hệ thống qua ILogger để bảo toàn vết an ninh khi DB Log lỗi
                    _log.LogError(auditEx, "FALLBACK_LOG: Lỗi khi ghi audit log 'generate' vào DB cho StudentId={S}, AnalysisId={A}. Dữ liệu thô: AttemptId={AttemptId}",
                        studentId, newId, attemptId);
                }

                return PersistenceResultDTO.Success(newId);
            }
            catch (Exception ex)
            {
                // Fallback khi lỗi kết nối DB hoặc constraint violation
                _log.LogError(ex, "Lỗi khi ghi CompetencyAnalysis vào DB cho studentId={S}, attemptId={A}.", studentId, attemptId);
                return PersistenceResultDTO.Fail("Lỗi hệ thống khi lưu dữ liệu. Vui lòng thử lại.");
            }
        }
    }
}
