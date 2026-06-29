using System;
using System.Collections.Generic;
using System.Linq;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.ViewModels;
using Microsoft.Extensions.Logging;

namespace DuAnTotNghiep.Services
{
    /// <summary>
    /// Triển khai tất cả logic Rule-based để tính điểm năng lực học sinh
    /// từ dữ liệu thô AssessmentInputDto (Task 2 output).
    /// 
    /// Service này KHÔNG gọi database và KHÔNG gọi AI.
    /// Nó là tầng xử lý thuần túy (Pure Computation Layer) đảm bảo:
    ///   - An toàn phép chia cho 0 ở mọi nơi.
    ///   - Kết quả hoàn toàn tái tạo được (deterministic) với cùng đầu vào.
    ///   - Output đủ tường minh để làm đầu vào Prompt cho Task 4.
    /// </summary>
    public class CompetencyScoreCalculatorService : ICompetencyScoreCalculatorService
    {
        // ──────────────────────────────────────────────────────────────
        //  Hằng số ngưỡng Rule-based
        // ──────────────────────────────────────────────────────────────

        /// <summary>Ngưỡng phần trăm tối đa để xếp loại WEAK (yếu).</summary>
        private const decimal WeaknessThreshold = 50.0m;

        /// <summary>Ngưỡng phần trăm tối thiểu để xếp loại STRONG (mạnh).</summary>
        private const decimal StrengthThreshold = 80.0m;

        /// <summary>Ngưỡng phần trăm tối đa để xếp loại CRITICAL (rất yếu).</summary>
        private const decimal CriticalThreshold = 40.0m;

        /// <summary>Ngưỡng tỷ lệ đúng tối đa để đánh dấu một cấp độ khó là "điểm gãy".</summary>
        private const decimal DifficultyBreakPointThreshold = 50.0m;

        /// <summary>Số Topic tối đa trong danh sách ưu tiên.</summary>
        private const int MaxPriorityTopics = 5;

        /// <summary>Số Topic tối thiểu trong danh sách ưu tiên.</summary>
        private const int MinPriorityTopics = 3;

        /// <summary>
        /// Mức giảm điểm ưu tiên cho mỗi cấp độ ưu tiên Onboarding.
        /// Ví dụ: PriorityLevel = 1 → giảm 1 * 20 = 20 điểm ưu tiên,
        /// giúp topic này được xếp lên đầu danh sách ưu tiên.
        /// </summary>
        private const decimal OnboardingWeightMultiplier = 20.0m;

        private readonly ILogger<CompetencyScoreCalculatorService> _logger;

        public CompetencyScoreCalculatorService(ILogger<CompetencyScoreCalculatorService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ──────────────────────────────────────────────────────────────
        //  Entry Point
        // ──────────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public CalculatedCompetencyResult CalculateScores(
            AssessmentInputDto inputData,
            IEnumerable<OnboardingSkillPreferenceDto>? onboardingSkillPreferences,
            IEnumerable<LearningTopicDto> topicCatalog)
        {
            if (inputData == null)
            {
                throw new ArgumentNullException(nameof(inputData), "AssessmentInputDto cannot be null. Task 2 must complete successfully before invoking Task 3.");
            }

            _logger.LogInformation(
                "[M7.Task3] Starting Rule-based competency calculation for Attempt ID: {AttemptId}, Student ID: {StudentId}.",
                inputData.AttemptId, inputData.StudentId);

            var warnings = new List<string>();

            // Chuẩn hóa danh sách onboarding preference để tra cứu nhanh O(1).
            // Key = SkillCode (UpperCase), Value = PriorityLevel (1 = cao nhất).
            var onboardingPreferenceMap = BuildOnboardingPreferenceMap(
                onboardingSkillPreferences, warnings);

            // ── Bước 1: Tính điểm theo Skill ────────────────────────────
            var skillScores = CalculateSkillScores(inputData.SkillScores, warnings);

            // ── Bước 2: Tính điểm theo Topic ────────────────────────────
            var topicScores = CalculateTopicScores(
                inputData.TopicScores, skillScores, onboardingPreferenceMap, warnings);

            // ── Bước 3: Phân tích theo Cấp độ khó ───────────────────────
            var difficultyBreakdowns = CalculateDifficultyBreakdowns(
                inputData.DifficultyScores, warnings);

            // ── Bước 4: Xác định KnowledgeGapPattern ────────────────────
            string knowledgeGapPattern = DetermineKnowledgeGapPattern(difficultyBreakdowns);

            // ── Bước 5: Tính điểm tổng quan ─────────────────────────────
            decimal overallAccuracy = inputData.MaxPossibleScore > 0
                ? Math.Round((inputData.TotalScore / inputData.MaxPossibleScore) * 100, 2)
                : 0m;

            // ── Bước 6: Ước lượng CEFR sơ bộ ────────────────────────────
            string estimatedCefr = EstimateCefrLevel(overallAccuracy);

            // ── Bước 7: Xác định Topic ưu tiên ──────────────────────────
            var priorityTopics = DeterminePriorityTopics(topicScores, skillScores, topicCatalog, onboardingPreferenceMap);

            // ── Bước 8: Sắp xếp TopicScores từ yếu nhất → mạnh nhất ─────
            var sortedTopicScores = topicScores
                .OrderBy(t => t.AccuracyPercentage)
                .ToList();

            // ── Bước 9: Lắp ráp kết quả cuối cùng ──────────────────────
            var result = new CalculatedCompetencyResult
            {
                AttemptId = inputData.AttemptId,
                StudentId = inputData.StudentId,
                TestTitle = inputData.TestTitle,
                CalculatedAt = DateTime.UtcNow,
                TotalEarnedScore = inputData.TotalScore,
                TotalMaxScore = inputData.MaxPossibleScore,
                OverallAccuracyPercentage = overallAccuracy,
                EstimatedCefrLevel = estimatedCefr,
                SkillScores = skillScores,
                TopicScores = sortedTopicScores,
                DifficultyBreakdowns = difficultyBreakdowns,
                KnowledgeGapPattern = knowledgeGapPattern,
                PriorityTopics = priorityTopics,
                // Pass-through danh sách câu sai từ Task 2 để Prompt AI (Task 4) phân tích lỗi cụ thể.
                WrongAnswers = inputData.WrongAnswers ?? new List<WrongAnswerDto>(),
                CalculationWarnings = warnings
            };

            _logger.LogInformation(
                "[M7.Task3] Calculation complete for Attempt ID: {AttemptId}. " +
                "Overall accuracy: {Accuracy}%, CEFR: {Cefr}, Gap pattern: {GapPattern}, " +
                "Priority topics count: {PriorityCount}, Warnings: {WarningCount}.",
                result.AttemptId,
                result.OverallAccuracyPercentage,
                result.EstimatedCefrLevel,
                result.KnowledgeGapPattern,
                result.PriorityTopics.Count,
                result.CalculationWarnings.Count);

            return result;
        }

        // ──────────────────────────────────────────────────────────────
        //  Bước 1: Tính điểm theo Skill
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Với mỗi SkillScoreDto từ Task 2, tính tỷ lệ % chính xác và gắn nhãn.
        /// An toàn phép chia: nếu TotalQuestions = 0, AccuracyPercentage = 0.
        /// </summary>
        private List<SkillCompetencyScore> CalculateSkillScores(
            List<SkillScoreDto> rawSkillScores,
            List<string> warnings)
        {
            var result = new List<SkillCompetencyScore>();

            if (rawSkillScores == null || rawSkillScores.Count == 0)
            {
                warnings.Add("No skill score data found in AssessmentInputDto. Skill analysis will be empty.");
                return result;
            }

            foreach (var rawSkill in rawSkillScores)
            {
                // ── An toàn phép chia: kiểm tra TotalQuestions trước khi chia ──
                decimal accuracyPercentage;
                if (rawSkill.TotalQuestions > 0)
                {
                    accuracyPercentage = Math.Round(
                        ((decimal)rawSkill.CorrectAnswers / rawSkill.TotalQuestions) * 100, 2);
                }
                else
                {
                    accuracyPercentage = 0m;
                    warnings.Add($"Skill '{rawSkill.SkillCode}' (ID: {rawSkill.SkillId}) has 0 questions in this test. Accuracy defaults to 0%.");
                }

                // ── Gắn nhãn IsWeakness / IsStrength ────────────────────────
                bool isWeakness = accuracyPercentage < WeaknessThreshold;
                bool isStrength = accuracyPercentage >= StrengthThreshold;

                // ── Gắn nhãn PerformanceLabel ────────────────────────────────
                string performanceLabel = DetermineSkillPerformanceLabel(accuracyPercentage);

                result.Add(new SkillCompetencyScore
                {
                    SkillId = rawSkill.SkillId,
                    SkillCode = rawSkill.SkillCode,
                    SkillName = rawSkill.SkillName,
                    TotalQuestions = rawSkill.TotalQuestions,
                    CorrectAnswers = rawSkill.CorrectAnswers,
                    EarnedScore = rawSkill.EarnedScore,
                    MaxScore = rawSkill.MaxScore,
                    AccuracyPercentage = accuracyPercentage,
                    IsWeakness = isWeakness,
                    IsStrength = isStrength,
                    PerformanceLabel = performanceLabel
                });
            }

            _logger.LogDebug(
                "[M7.Task3] Skill scores calculated: {Count} skills. Weaknesses: {WeakCount}, Strengths: {StrongCount}.",
                result.Count,
                result.Count(s => s.IsWeakness),
                result.Count(s => s.IsStrength));

            return result;
        }

        /// <summary>
        /// Xác định nhãn định tính cho Skill dựa trên tỷ lệ chính xác.
        /// WEAK: dưới 50% | DEVELOPING: 50–79% | STRONG: 80% trở lên.
        /// </summary>
        private static string DetermineSkillPerformanceLabel(decimal accuracyPercentage)
        {
            if (accuracyPercentage >= StrengthThreshold)
            {
                return "STRONG";
            }

            if (accuracyPercentage >= WeaknessThreshold)
            {
                return "DEVELOPING";
            }

            return "WEAK";
        }

        // ──────────────────────────────────────────────────────────────
        //  Bước 2: Tính điểm theo Topic
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Với mỗi TopicScoreDto từ Task 2:
        ///   1. Tính tỷ lệ % chính xác an toàn.
        ///   2. Tính PriorityScore = AccuracyPercentage - (OnboardingWeight * multiplier).
        ///   3. Gắn nhãn PerformanceLabel.
        ///   4. Bổ sung SkillCode từ danh sách skillScores đã tính.
        /// </summary>
        private List<TopicCompetencyScore> CalculateTopicScores(
            List<TopicScoreDto> rawTopicScores,
            List<SkillCompetencyScore> skillScores,
            Dictionary<string, int> onboardingPreferenceMap,
            List<string> warnings)
        {
            var result = new List<TopicCompetencyScore>();

            if (rawTopicScores == null || rawTopicScores.Count == 0)
            {
                warnings.Add("No topic score data found in AssessmentInputDto. Topic analysis will be empty.");
                return result;
            }

            // Tạo lookup SkillCode nhanh từ danh sách đã tính ở Bước 1
            var skillCodeLookup = skillScores.ToDictionary(s => s.SkillId, s => s.SkillCode);

            foreach (var rawTopic in rawTopicScores)
            {
                // ── An toàn phép chia ────────────────────────────────────────
                decimal accuracyPercentage;
                if (rawTopic.TotalQuestions > 0)
                {
                    accuracyPercentage = Math.Round(
                        ((decimal)rawTopic.CorrectAnswers / rawTopic.TotalQuestions) * 100, 2);
                }
                else
                {
                    accuracyPercentage = 0m;
                    warnings.Add($"Topic '{rawTopic.TopicTitle}' (ID: {rawTopic.TopicId}) has 0 questions in this test. Accuracy defaults to 0%.");
                }

                // ── Tính trọng số ưu tiên Onboarding ────────────────────────
                // Tra cứu xem skill cha của topic này có được học sinh ưu tiên không.
                string skillCodeForLookup = skillCodeLookup.TryGetValue(rawTopic.SkillId, out var code)
                    ? code.ToUpper()
                    : string.Empty;

                decimal onboardingBonus = 0m;
                if (!string.IsNullOrEmpty(skillCodeForLookup) &&
                    onboardingPreferenceMap.TryGetValue(skillCodeForLookup, out int priorityLevel))
                {
                    // PriorityLevel = 1 (cao nhất) → Bonus = 20 / 1 = 20
                    // PriorityLevel = 2 → Bonus = 20 / 2 = 10 (ưu tiên thấp hơn)
                    // Guard: PriorityLevel phải > 0 để tránh DivideByZeroException.
                    if (priorityLevel > 0)
                    {
                        onboardingBonus = OnboardingWeightMultiplier / priorityLevel;
                    }
                }

                // ── PriorityScore: Giá trị thấp = cần học gấp hơn ──────────
                // Công thức: PriorityScore = AccuracyPercentage - OnboardingBonus
                // → Topic yếu (accuracy thấp) + được onboarding ưu tiên → PriorityScore càng thấp
                decimal priorityScore = Math.Round(accuracyPercentage - onboardingBonus, 2);

                // ── Xác định SkillCode ────────────────────────────────────────
                string skillCode = skillCodeLookup.TryGetValue(rawTopic.SkillId, out var resolvedCode)
                    ? resolvedCode
                    : $"SKILL_{rawTopic.SkillId}";

                // ── Gắn nhãn PerformanceLabel ────────────────────────────────
                string performanceLabel = DetermineTopicPerformanceLabel(accuracyPercentage);

                result.Add(new TopicCompetencyScore
                {
                    TopicId = rawTopic.TopicId,
                    TopicCode = rawTopic.TopicCode,
                    TopicTitle = rawTopic.TopicTitle,
                    SkillId = rawTopic.SkillId,
                    SkillCode = skillCode,
                    TotalQuestions = rawTopic.TotalQuestions,
                    CorrectAnswers = rawTopic.CorrectAnswers,
                    EarnedScore = rawTopic.EarnedScore,
                    MaxScore = rawTopic.MaxScore,
                    AccuracyPercentage = accuracyPercentage,
                    PriorityScore = priorityScore,
                    PerformanceLabel = performanceLabel
                });
            }

            _logger.LogDebug(
                "[M7.Task3] Topic scores calculated: {Count} topics. Critical topics: {CriticalCount}.",
                result.Count,
                result.Count(t => t.PerformanceLabel == "CRITICAL"));

            return result;
        }

        /// <summary>
        /// Xác định nhãn định tính cho Topic.
        /// CRITICAL: dưới 40% | WEAK: 40–59% | DEVELOPING: 60–79% | STRONG: 80%+.
        /// </summary>
        private static string DetermineTopicPerformanceLabel(decimal accuracyPercentage)
        {
            if (accuracyPercentage >= StrengthThreshold)
            {
                return "STRONG";
            }

            if (accuracyPercentage >= WeaknessThreshold)
            {
                return "DEVELOPING";
            }

            if (accuracyPercentage >= CriticalThreshold)
            {
                return "WEAK";
            }

            return "CRITICAL";
        }

        // ──────────────────────────────────────────────────────────────
        //  Bước 3: Phân tích theo Cấp độ khó
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Tính tỷ lệ chính xác theo từng cấp độ khó (BASIC / MEDIUM / ADVANCED).
        /// Đánh dấu IsBreakPoint = true nếu tỷ lệ dưới ngưỡng 50%.
        /// </summary>
        private List<DifficultyBreakdown> CalculateDifficultyBreakdowns(
            List<DifficultyScoreDto> rawDifficultyScores,
            List<string> warnings)
        {
            var result = new List<DifficultyBreakdown>();

            if (rawDifficultyScores == null || rawDifficultyScores.Count == 0)
            {
                warnings.Add("No difficulty score data found. Difficulty breakdown analysis will be empty.");
                return result;
            }

            // Đảm bảo thứ tự hiển thị cố định: BASIC → MEDIUM → ADVANCED
            var orderedDifficultyLevels = new List<string> { "BASIC", "MEDIUM", "ADVANCED" };

            // Tạo dictionary để tra cứu nhanh từ dữ liệu thô
            var rawMap = rawDifficultyScores.ToDictionary(
                d => d.DifficultyLevel.ToUpper(),
                d => d,
                StringComparer.OrdinalIgnoreCase);

            foreach (string level in orderedDifficultyLevels)
            {
                if (!rawMap.TryGetValue(level, out var rawDiff))
                {
                    // Cấp độ này không có câu hỏi trong bài test, bỏ qua
                    continue;
                }

                // ── An toàn phép chia ────────────────────────────────────────
                decimal accuracyPercentage;
                if (rawDiff.TotalQuestions > 0)
                {
                    accuracyPercentage = Math.Round(
                        ((decimal)rawDiff.CorrectAnswers / rawDiff.TotalQuestions) * 100, 2);
                }
                else
                {
                    accuracyPercentage = 0m;
                    warnings.Add($"Difficulty level '{level}' has 0 questions. Accuracy defaults to 0%.");
                }

                result.Add(new DifficultyBreakdown
                {
                    DifficultyLevel = level,
                    TotalQuestions = rawDiff.TotalQuestions,
                    CorrectAnswers = rawDiff.CorrectAnswers,
                    EarnedScore = rawDiff.EarnedScore,
                    MaxScore = rawDiff.MaxScore,
                    AccuracyPercentage = accuracyPercentage,
                    IsBreakPoint = accuracyPercentage < DifficultyBreakPointThreshold
                });
            }

            // Xử lý các cấp độ ngoài danh sách chuẩn (nếu có trong dữ liệu)
            foreach (var rawDiff in rawDifficultyScores)
            {
                string upperLevel = rawDiff.DifficultyLevel.ToUpper();
                if (!orderedDifficultyLevels.Contains(upperLevel))
                {
                    decimal accuracyPercentage = rawDiff.TotalQuestions > 0
                        ? Math.Round(((decimal)rawDiff.CorrectAnswers / rawDiff.TotalQuestions) * 100, 2)
                        : 0m;

                    warnings.Add($"Non-standard difficulty level '{rawDiff.DifficultyLevel}' found and appended to breakdown.");

                    result.Add(new DifficultyBreakdown
                    {
                        DifficultyLevel = upperLevel,
                        TotalQuestions = rawDiff.TotalQuestions,
                        CorrectAnswers = rawDiff.CorrectAnswers,
                        EarnedScore = rawDiff.EarnedScore,
                        MaxScore = rawDiff.MaxScore,
                        AccuracyPercentage = accuracyPercentage,
                        IsBreakPoint = accuracyPercentage < DifficultyBreakPointThreshold
                    });
                }
            }

            return result;
        }

        // ──────────────────────────────────────────────────────────────
        //  Bước 4: Xác định KnowledgeGapPattern
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Phân tích cấu trúc lỗi học sinh để xác định "pattern" kiến thức hổng:
        /// 
        ///   - BASIC_GAP:        Bị gãy ngay từ cấp BASIC. Kiến thức nền rất yếu.
        ///   - INTERMEDIATE_GAP: Ổn ở BASIC nhưng gãy ở MEDIUM. Hổng kiến thức trung bình.
        ///   - ADVANCED_GAP:     Ổn ở BASIC và MEDIUM, chỉ gãy ở ADVANCED. Cần nâng cao.
        ///   - CONSISTENT_LOW:   Gãy đều ở tất cả cấp độ. Năng lực tổng thể thấp.
        ///   - CONSISTENT_HIGH:  Không gãy ở cấp nào. Năng lực tốt.
        ///   - INSUFFICIENT_DATA: Không đủ dữ liệu để xác định pattern.
        /// </summary>
        private static string DetermineKnowledgeGapPattern(List<DifficultyBreakdown> breakdowns)
        {
            if (breakdowns == null || breakdowns.Count == 0)
            {
                return "INSUFFICIENT_DATA";
            }

            var basicBreakdown = breakdowns.FirstOrDefault(d => d.DifficultyLevel == "BASIC");
            var mediumBreakdown = breakdowns.FirstOrDefault(d => d.DifficultyLevel == "MEDIUM");
            var advancedBreakdown = breakdowns.FirstOrDefault(d => d.DifficultyLevel == "ADVANCED");

            bool basicIsBreakPoint = basicBreakdown?.IsBreakPoint ?? false;
            bool mediumIsBreakPoint = mediumBreakdown?.IsBreakPoint ?? false;
            bool advancedIsBreakPoint = advancedBreakdown?.IsBreakPoint ?? false;

            // Không có đủ dữ liệu các cấp để phân tích
            if (basicBreakdown == null && mediumBreakdown == null && advancedBreakdown == null)
            {
                return "INSUFFICIENT_DATA";
            }

            // Gãy đều tất cả cấp độ
            if (basicIsBreakPoint && mediumIsBreakPoint && advancedIsBreakPoint)
            {
                return "CONSISTENT_LOW";
            }

            // Tốt ở tất cả cấp độ
            if (!basicIsBreakPoint && !mediumIsBreakPoint && !advancedIsBreakPoint)
            {
                return "CONSISTENT_HIGH";
            }

            // Gãy ngay ở BASIC → Kiến thức nền rất yếu
            if (basicIsBreakPoint)
            {
                return "BASIC_GAP";
            }

            // Ổn ở BASIC, gãy ở MEDIUM
            if (!basicIsBreakPoint && mediumIsBreakPoint)
            {
                return "INTERMEDIATE_GAP";
            }

            // Ổn ở BASIC và MEDIUM, chỉ gãy ở ADVANCED
            if (!basicIsBreakPoint && !mediumIsBreakPoint && advancedIsBreakPoint)
            {
                return "ADVANCED_GAP";
            }

            return "MIXED_GAP";
        }

        // ──────────────────────────────────────────────────────────────
        //  Bước 5: Ước lượng CEFR sơ bộ
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Ước lượng trình độ CEFR sơ bộ dựa trên tỷ lệ % chính xác tổng thể.
        /// Đây là ước lượng Rule-based đơn giản, chỉ mang tính tham khảo.
        /// AI ở Task 4 sẽ có thể bác bỏ hoặc tinh chỉnh dựa trên phân tích sâu hơn.
        /// 
        /// Thang đối chiếu (quy ước nội bộ):
        ///   0–24%   → Pre-A1 (hoàn toàn mới bắt đầu)
        ///   25–39%  → A1    (Beginner)
        ///   40–54%  → A2    (Elementary)
        ///   55–69%  → B1    (Intermediate)
        ///   70–84%  → B2    (Upper-Intermediate)
        ///   85–94%  → C1    (Advanced)
        ///   95–100% → C2    (Mastery)
        /// </summary>
        private static string EstimateCefrLevel(decimal overallAccuracyPercentage)
        {
            if (overallAccuracyPercentage >= 95m)
            {
                return "C2";
            }

            if (overallAccuracyPercentage >= 85m)
            {
                return "C1";
            }

            if (overallAccuracyPercentage >= 70m)
            {
                return "B2";
            }

            if (overallAccuracyPercentage >= 55m)
            {
                return "B1";
            }

            if (overallAccuracyPercentage >= 40m)
            {
                return "A2";
            }

            if (overallAccuracyPercentage >= 25m)
            {
                return "A1";
            }

            return "PRE_A1";
        }

        // ──────────────────────────────────────────────────────────────
        //  Bước 6: Xác định Topic ưu tiên
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Xác định danh sách 3–5 Topic cần học ưu tiên nhất bằng thuật toán:
        /// 
        ///   1. Lọc các topic có AccuracyPercentage dưới ngưỡng STRONG (80%).
        ///      Các topic STRONG không cần ưu tiên.
        ///   2. Sắp xếp theo PriorityScore tăng dần (giá trị thấp = cần học gấp hơn).
        ///      PriorityScore đã tích hợp onboarding weight từ Bước 2.
        ///   3. Lấy tối đa MaxPriorityTopics (5) topic đầu.
        ///      Nếu có ít hơn MinPriorityTopics (3), lấy tất cả.
        ///   4. Sinh PriorityReason mô tả ngắn gọn lý do ưu tiên.
        ///   5. Gán Rank thứ tự 1, 2, 3...
        /// </summary>
        private List<PriorityTopicItem> DeterminePriorityTopics(
            List<TopicCompetencyScore> topicScores,
            List<SkillCompetencyScore> skillScores,
            IEnumerable<LearningTopicDto> topicCatalog,
            Dictionary<string, int> onboardingPreferenceMap)
        {
            var result = new List<PriorityTopicItem>();

            if (topicScores == null || topicScores.Count == 0)
            {
                _logger.LogWarning("[M7.Task3] No topic scores available to build priority list.");
                return result;
            }

            // ── Bước 1: Quét các topic hổng kiến thức ─────────────────────
            var gapTopicIds = topicScores
                .Where(t => t.AccuracyPercentage < StrengthThreshold)
                .Select(t => t.TopicId)
                .ToHashSet();

            if (gapTopicIds.Count == 0)
            {
                _logger.LogInformation("[M7.Task3] All topics above strength threshold. No priority topics needed.");
                return result;
            }

            var catalogList = topicCatalog?.ToList() ?? new List<LearningTopicDto>();
            var catalogMap = catalogList.ToDictionary(t => t.TopicId);

            // ── Bước 2: Truy hồi ngược Prerequisite ───────────────────────
            var expandedGapTopicIds = new HashSet<int>(gapTopicIds);
            var queue = new Queue<int>(gapTopicIds);

            while (queue.Count > 0)
            {
                int currentId = queue.Dequeue();
                if (catalogMap.TryGetValue(currentId, out var topicNode))
                {
                    if (topicNode.ParentTopicId.HasValue)
                    {
                        int parentId = topicNode.ParentTopicId.Value;
                        var parentScore = topicScores.FirstOrDefault(t => t.TopicId == parentId);
                        
                        // Parent chưa hoàn thành: điểm thấp hoặc chưa học
                        bool parentIsGap = parentScore == null || parentScore.AccuracyPercentage < StrengthThreshold;
                        
                        if (parentIsGap && !expandedGapTopicIds.Contains(parentId))
                        {
                            expandedGapTopicIds.Add(parentId);
                            queue.Enqueue(parentId);
                        }
                    }
                }
            }

            // ── Bước 3: Lọc topic Inactive và tính Dependency Weight ──────
            var validGapTopics = expandedGapTopicIds
                .Select(id => catalogMap.TryGetValue(id, out var t) ? t : null)
                .Where(t => t != null && string.Equals(t.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var childrenMap = new Dictionary<int, List<int>>();
            foreach (var t in validGapTopics!)
            {
                if (t.ParentTopicId.HasValue)
                {
                    if (!childrenMap.ContainsKey(t.ParentTopicId.Value)) 
                        childrenMap[t.ParentTopicId.Value] = new List<int>();
                    childrenMap[t.ParentTopicId.Value].Add(t.TopicId);
                }
            }

            int CountDescendants(int rootId)
            {
                if (!childrenMap.ContainsKey(rootId)) return 0;
                int count = childrenMap[rootId].Count;
                foreach (var childId in childrenMap[rootId])
                {
                    count += CountDescendants(childId);
                }
                return count;
            }

            // ── Bước 4: Tập hợp Ready-to-Learn ────────────────────────────
            var readyToLearnTopics = validGapTopics.Where(t => 
                !t.ParentTopicId.HasValue || !validGapTopics.Any(g => g.TopicId == t.ParentTopicId.Value)
            ).ToList();

            // ── Bước 5: Tính Priority Score ───────────────────────────────
            var scoredTopics = new List<PriorityTopicItem>();
            var skillCodeLookup = skillScores.ToDictionary(s => s.SkillId, s => s.SkillCode);
            decimal maxDependencyCount = Math.Max(validGapTopics.Count, 1m);

            foreach (var r in readyToLearnTopics)
            {
                var ts = topicScores.FirstOrDefault(x => x.TopicId == r.TopicId);
                decimal accuracy = ts != null ? ts.AccuracyPercentage : 0m;
                
                // (G) Gap
                decimal gNorm = (100m - accuracy) / 100m;
                
                // (P) Profile Priority
                decimal pNorm = 0.1m; 
                string skillCode = ts?.SkillCode ?? (skillCodeLookup.TryGetValue(r.SkillId, out var sc) ? sc : $"SKILL_{r.SkillId}");
                string skillCodeUpper = skillCode.ToUpper();

                if (onboardingPreferenceMap.TryGetValue(skillCodeUpper, out int pLevel))
                {
                    if (pLevel == 1) pNorm = 1.0m;
                    else if (pLevel == 2) pNorm = 0.66m;
                    else if (pLevel >= 3) pNorm = 0.33m;
                }

                // (D) Dependency Weight
                int descCount = CountDescendants(r.TopicId);
                decimal dNorm = Math.Min(descCount / maxDependencyCount, 1.0m);

                // Formula parameters
                decimal alpha = 0.5m;
                decimal beta = 0.3m;
                decimal gamma = 0.2m;

                decimal priorityScore = (alpha * gNorm) + (beta * pNorm) + (gamma * dNorm);

                // Generate reasons
                var reasons = new List<string>();
                if (accuracy < CriticalThreshold) reasons.Add($"Hổng kiến thức nghiêm trọng ({accuracy}%)");
                else if (accuracy < WeaknessThreshold) reasons.Add($"Hổng kiến thức ({accuracy}%)");
                else if (ts == null) reasons.Add("Kiến thức nền tảng chưa được học");
                
                if (pNorm >= 0.66m) reasons.Add($"Kỹ năng trọng điểm ({skillCode})");
                if (descCount > 0) reasons.Add($"Mở khóa {descCount} bài học khác");

                string reasonCode = "STANDARD_GAP";
                if (descCount > 0 && accuracy < WeaknessThreshold) reasonCode = "CRITICAL_FOUNDATION_GAP";
                else if (pNorm >= 0.66m && accuracy < WeaknessThreshold) reasonCode = "PROFILE_FOCUSED_WEAKNESS";
                else if (ts == null) reasonCode = "MISSING_PREREQUISITE";

                scoredTopics.Add(new PriorityTopicItem
                {
                    TopicId = r.TopicId,
                    TopicCode = r.TopicCode,
                    TopicTitle = r.TopicTitle,
                    SkillCode = skillCode,
                    AccuracyPercentage = accuracy,
                    PriorityScore = Math.Round(priorityScore, 2),
                    ReasonCode = reasonCode,
                    PriorityReason = string.Join("; ", reasons),
                    PrerequisitesMet = true
                });
            }

            // ── Bước 6: Sắp xếp và Cắt tỉa (Truncation) ───────────────────
            var sortedList = scoredTopics.OrderByDescending(x => x.PriorityScore).Take(MaxPriorityTopics).ToList();
            for (int i = 0; i < sortedList.Count; i++)
            {
                sortedList[i].Rank = i + 1;
            }

            _logger.LogInformation("[M7.Task3] Topic prioritization complete. Identified {Count} priority topics.", sortedList.Count);
            return sortedList;
        }

        // ──────────────────────────────────────────────────────────────
        //  Helper: Xây dựng Onboarding Preference Map
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Chuyển danh sách OnboardingSkillPreferenceDto thành Dictionary
        /// để tra cứu O(1) theo SkillCode (key = SkillCode.ToUpper()).
        /// 
        /// Nếu có nhiều preference trùng SkillCode, giữ lại cái có PriorityLevel thấp nhất
        /// (PriorityLevel thấp = ưu tiên cao hơn).
        /// </summary>
        private Dictionary<string, int> BuildOnboardingPreferenceMap(
            IEnumerable<OnboardingSkillPreferenceDto>? preferences,
            List<string> warnings)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            if (preferences == null)
            {
                warnings.Add("No Onboarding skill preferences provided. Topic priority will be based solely on accuracy score.");
                return map;
            }

            var preferenceList = preferences.ToList();

            if (preferenceList.Count == 0)
            {
                warnings.Add("Onboarding skill preferences list is empty. Topic priority will be based solely on accuracy score.");
                return map;
            }

            foreach (var preference in preferenceList)
            {
                if (string.IsNullOrWhiteSpace(preference.SkillCode))
                {
                    warnings.Add("An Onboarding preference entry has an empty SkillCode and was skipped.");
                    continue;
                }

                string key = preference.SkillCode.ToUpper();

                // Nếu SkillCode đã tồn tại, giữ lại cái có PriorityLevel thấp hơn (ưu tiên hơn)
                if (map.TryGetValue(key, out int existingLevel))
                {
                    if (preference.PriorityLevel < existingLevel)
                    {
                        map[key] = preference.PriorityLevel;
                    }
                }
                else
                {
                    map[key] = preference.PriorityLevel;
                }
            }

            _logger.LogDebug(
                "[M7.Task3] Onboarding preference map built with {Count} skill entries: [{Skills}].",
                map.Count,
                string.Join(", ", map.Keys));

            return map;
        }
    }
}
