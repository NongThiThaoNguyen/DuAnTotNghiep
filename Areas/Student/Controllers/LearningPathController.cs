using System.Security.Claims;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Helpers;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.Enums;
using DuAnTotNghiep.Models.Exceptions;
using DuAnTotNghiep.Models.ViewModels.Student;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace DuAnTotNghiep.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "STUDENT")]
    public class LearningPathController : Controller
    {
        private readonly IPathViewService _pathViewService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LearningPathController> _logger;
        private readonly ILearningPathEngineService? _learningPathEngineService;
        private readonly ILevelUpAssessmentService? _levelUpAssessmentService;

        public LearningPathController(IPathViewService pathViewService, ApplicationDbContext context)
            : this(pathViewService, context, NullLogger<LearningPathController>.Instance, null, null)
        {
        }

        public LearningPathController(
            IPathViewService pathViewService,
            ApplicationDbContext context,
            ILogger<LearningPathController> logger)
            : this(pathViewService, context, logger, null, null)
        {
        }

        public LearningPathController(
            IPathViewService pathViewService,
            ApplicationDbContext context,
            ILogger<LearningPathController> logger,
            ILearningPathEngineService learningPathEngineService)
            : this(pathViewService, context, logger, learningPathEngineService, null)
        {
        }

        [ActivatorUtilitiesConstructor]
        public LearningPathController(
            IPathViewService pathViewService,
            ApplicationDbContext context,
            ILogger<LearningPathController> logger,
            ILearningPathEngineService? learningPathEngineService,
            ILevelUpAssessmentService? levelUpAssessmentService)
        {
            _pathViewService = pathViewService;
            _context = context;
            _logger = logger;
            _learningPathEngineService = learningPathEngineService;
            _levelUpAssessmentService = levelUpAssessmentService;
        }

        private int GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out var userId);
            return userId;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var model = await _pathViewService.GetCurrentPathPageAsync(userId);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Generate()
        {
            var userId = GetUserId();
            var model = await LearningPathEngineService.CanGeneratePathAsync(userId);
            return View(model);
        }

        [HttpPost]
        [ActionName("Generate")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GeneratePost()
        {
            var userId = GetUserId();
            var analysisId = await GetLatestCompetencyAnalysisIdAsync(userId);
            if (!analysisId.HasValue) return RedirectWithError(nameof(Generate), "Bạn cần hoàn thành phân tích năng lực trước.");

            try
            {
                await LearningPathEngineService.GenerateInitialPathAsync(userId, analysisId.Value);
                return RedirectToAction(nameof(Summary));
            }
            catch (BusinessException ex)
            {
                return RedirectWithError(nameof(Generate), ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var model = await LearningPathEngineService.GetPathDetailAsync(id, GetUserId());
                return View(model);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Summary()
        {
            try
            {
                var model = await LearningPathEngineService.GetPathSummaryAsync(GetUserId());
                return View(model);
            }
            catch (NotFoundException ex)
            {
                return RedirectWithError(nameof(Generate), ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Regenerate(string reason)
        {
            try
            {
                await LearningPathEngineService.RegeneratePathAsync(GetUserId(), reason);
                return RedirectToAction(nameof(Summary));
            }
            catch (BusinessException ex)
            {
                return RedirectWithError(nameof(Summary), ex.Message);
            }
            catch (NotFoundException ex)
            {
                return RedirectWithError(nameof(Generate), ex.Message);
            }
        }

        [HttpGet("/Student/LearningPath/OpenNode/{nodeId:int}")]
        public async Task<IActionResult> OpenNode(int nodeId)
        {
            var userId = GetUserId();
            var node = await _context.LearningPathNodes
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == nodeId);

            if (node == null)
            {
                return NotFound();
            }

            if (!await _pathViewService.EnsurePathOwnerAsync(node.LearningPathId, userId))
            {
                return Forbid();
            }

            if (!await _pathViewService.CanOpenNodeAsync(nodeId, userId))
            {
                TempData["ErrorMessage"] = "Bai hoc nay hien chua duoc mo khoa.";
                return RedirectToAction(nameof(Index));
            }

            var targetUrl = await _pathViewService.BuildNodeTargetUrlAsync(node);
            if (string.IsNullOrWhiteSpace(targetUrl))
            {
                TempData["ErrorMessage"] = "Noi dung hoc tap nay chua san sang.";
                return RedirectToAction(nameof(Index));
            }

            _logger.LogInformation("Student {StudentId} opened learning path node {NodeId}", userId, nodeId);
            return Redirect(targetUrl);
        }

        // GET: /Student/LearningPath/Topic/{nodeId}
        [HttpGet("/Student/LearningPath/Topic/{nodeId:int}")]
        public async Task<IActionResult> TopicDetail(int nodeId)
        {
            var userId = GetUserId();

            var node = await _context.LearningPathNodes
                .Include(n => n.Topic)
                    .ThenInclude(t => t!.LearningObjectives.OrderBy(o => o.OrderIndex))
                .Include(n => n.Topic)
                    .ThenInclude(t => t!.Skill)
                .Include(n => n.LearningPath)
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == nodeId);

            if (node == null || node.Topic == null)
                return NotFound();

            if (node.LearningPath.StudentId != userId)
                return Forbid();

            // Get sibling nodes in same learning path for navigation
            var allNodes = await _context.LearningPathNodes
                .Where(n => n.LearningPathId == node.LearningPathId)
                .OrderBy(n => n.OrderIndex)
                .AsNoTracking()
                .ToListAsync();

            var nodeIndex = allNodes.FindIndex(n => n.Id == nodeId);
            var prevNodeId = nodeIndex > 0 ? allNodes[nodeIndex - 1].Id : (int?)null;
            var nextNodeId = nodeIndex < allNodes.Count - 1 ? allNodes[nodeIndex + 1].Id : (int?)null;

            // Get child nodes that belong to this topic in the path
            var childNodes = allNodes
                .Where(n => n.TopicId == node.TopicId && n.NodeType != NodeType.Topic && n.NodeType != NodeType.Review)
                .OrderBy(n => n.OrderIndex)
                .ToList();

            var childNodeItems = new List<TopicChildNodeItem>();
            foreach (var child in childNodes)
            {
                var targetUrl = await _pathViewService.BuildNodeTargetUrlAsync(child) ?? "#";
                childNodeItems.Add(new TopicChildNodeItem
                {
                    NodeId = child.Id,
                    Title = child.NodeTitle,
                    NodeType = child.NodeType,
                    Status = child.Status,
                    EstimatedMinutes = child.EstimatedMinutes,
                    ScheduledDate = child.ScheduledDate,
                    IsClickable = child.Status is "AVAILABLE" or "COMPLETED",
                    TargetUrl = targetUrl
                });
            }

            var topic = node.Topic;
            var vm = new TopicDetailViewModel
            {
                TopicId = topic.Id,
                TopicTitle = topic.Title,
                TopicDescription = topic.Description,
                SkillName = topic.Skill?.SkillName ?? "",
                SkillCode = topic.Skill?.SkillCode ?? "",
                DifficultyLevel = topic.DifficultyLevel,
                EstimatedMinutes = topic.EstimatedMinutes,
                ThumbnailUrl = CourseThumbnailHelper.ResolveThumbnailUrl(topic.Title, topic.TopicCode, topic.Skill?.SkillCode),
                NodeId = node.Id,
                NodeStatus = node.Status,
                AiReason = node.AiReason,
                PathPhase = node.PathPhase,
                Objectives = topic.LearningObjectives.Select(o => new TopicObjectiveItem
                {
                    ObjectiveText = o.ObjectiveText,
                    CognitiveLevel = o.CognitiveLevel,
                    OrderIndex = o.OrderIndex
                }).ToList(),
                ChildNodes = childNodeItems,
                PreviousNodeId = prevNodeId,
                NextNodeId = nextNodeId
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> LevelUpAssessment()
        {
            var userId = GetUserId();
            if (_levelUpAssessmentService == null)
            {
                TempData["ErrorMessage"] = "Dịch vụ đánh giá thăng hạng chưa sẵn sàng.";
                return RedirectToAction(nameof(Index));
            }

            var model = await _levelUpAssessmentService.GetLevelUpAssessmentAsync(userId);
            if (model == null)
            {
                TempData["ErrorMessage"] = "Hiện chưa có bài đánh giá thăng hạng khả dụng cho trình độ của bạn.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ReportViolation([FromBody] DuAnTotNghiep.Models.DTOs.Exam.ViolationReportDto dto)
        {
            var userId = GetUserId();
            if (userId <= 0) return Unauthorized();

            var log = new StudyActivityLog
            {
                StudentId = userId,
                ActivityType = "LEVEL_UP_VIOLATION",
                DurationMinutes = 0,
                Metadata = $"Exits: {dto.FullscreenExitCount}, TabSwitches: {dto.TabSwitchCount}, Details: {dto.Details}",
                CreatedAt = DateTime.UtcNow
            };
            _context.StudyActivityLogs.Add(log);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitLevelUpAssessment(int testId, IFormCollection form)
        {
            var userId = GetUserId();
            if (_levelUpAssessmentService == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var answers = new Dictionary<int, string>();
            foreach (var key in form.Keys)
            {
                if (key.StartsWith("question_", StringComparison.OrdinalIgnoreCase) &&
                    int.TryParse(key.Replace("question_", "", StringComparison.OrdinalIgnoreCase), out int qId))
                {
                    answers[qId] = form[key].ToString();
                }
            }

            try
            {
                var result = await _levelUpAssessmentService.SubmitLevelUpAssessmentAsync(userId, testId, answers);

                // Save violation stats if present in form
                if (int.TryParse(form["FullscreenExitCount"], out int fsExits) && fsExits > 0 ||
                    int.TryParse(form["TabSwitchCount"], out int tabSwitches) && tabSwitches > 0)
                {
                    var log = new StudyActivityLog
                    {
                        StudentId = userId,
                        ActivityType = "LEVEL_UP_EXAM_COMPLETED",
                        DurationMinutes = 15,
                        Metadata = $"Exits: {form["FullscreenExitCount"]}, TabSwitches: {form["TabSwitchCount"]}, Logs: {form["ViolationLog"]}",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.StudyActivityLogs.Add(log);
                    await _context.SaveChangesAsync();
                }

                return View("LevelUpResult", result);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction(nameof(LevelUpAssessment));
            }
        }

        private ILearningPathEngineService LearningPathEngineService
        {
            get
            {
                return _learningPathEngineService
                    ?? throw new InvalidOperationException("Learning path engine service is not configured.");
            }
        }

        private async Task<int?> GetLatestCompetencyAnalysisIdAsync(int userId)
        {
            return await _context.CompetencyAnalyses
                .AsNoTracking()
                .Where(analysis => analysis.StudentId == userId)
                .OrderByDescending(analysis => analysis.CreatedAt)
                .Select(analysis => (int?)analysis.Id)
                .FirstOrDefaultAsync();
        }

        private IActionResult RedirectWithError(string actionName, string message)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(actionName);
        }
    }
}
