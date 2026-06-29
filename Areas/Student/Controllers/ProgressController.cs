using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.Repositories.Interfaces;
using DuAnTotNghiep.DTOs.Progress;
using DuAnTotNghiep.ViewModels.Progress;
using DuAnTotNghiep.Enums;
using DuAnTotNghiep.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DuAnTotNghiep.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "STUDENT,TEACHER,ADMIN")]
    public class ProgressController : Controller
    {
        private readonly IStudentProgressService _studentProgressService;
        private readonly IProgressTrackingService _progressTrackingService;
        private readonly IProgressRepository _progressRepo;
        private readonly ApplicationDbContext _context;
        private readonly IPathViewService? _pathViewService;

        public ProgressController(
            IStudentProgressService studentProgressService,
            IProgressTrackingService progressTrackingService,
            IProgressRepository progressRepo,
            ApplicationDbContext context)
            : this(studentProgressService, progressTrackingService, progressRepo, context, null)
        {
        }

        [ActivatorUtilitiesConstructor]
        public ProgressController(
            IStudentProgressService studentProgressService,
            IProgressTrackingService progressTrackingService,
            IProgressRepository progressRepo,
            ApplicationDbContext context,
            IPathViewService? pathViewService)
        {
            _studentProgressService = studentProgressService;
            _progressTrackingService = progressTrackingService;
            _progressRepo = progressRepo;
            _context = context;
            _pathViewService = pathViewService;
        }

        private int GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out int userId);
            return userId;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? studentId = null)
        {
            int currentUserId = GetUserId();
            int targetStudentId = studentId ?? currentUserId;

            if (User.IsInRole("STUDENT") && targetStudentId != currentUserId)
            {
                return Forbid();
            }

            var dashboard = await _studentProgressService.GetDashboardAsync(targetStudentId);
            ViewBag.TargetStudentId = targetStudentId;
            return View(dashboard);
        }

        [HttpGet]
        public async Task<IActionResult> History([FromQuery] HistoryFilter filter, int? studentId = null)
        {
            int currentUserId = GetUserId();
            int targetStudentId = studentId ?? currentUserId;

            if (User.IsInRole("STUDENT") && targetStudentId != currentUserId)
            {
                return Forbid();
            }

            if (filter.Page < 1) filter.Page = 1;
            filter.PageSize = 10;

            var historyItems = await _progressRepo.GetHistory(targetStudentId, filter);
            
            ViewBag.CurrentPage = filter.Page;
            ViewBag.PageSize = filter.PageSize;
            ViewBag.ActivityType = filter.ActivityType;
            ViewBag.StartDate = filter.StartDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = filter.EndDate?.ToString("yyyy-MM-dd");
            ViewBag.TargetStudentId = targetStudentId;
            
            return View(historyItems);
        }

        [HttpPost]
        public async Task<IActionResult> Record([FromBody] ActivityLogCreateDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.ActivityType))
            {
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            int studentId = GetUserId();

            try
            {
                bool handled = false;

                if (_pathViewService != null && dto.LearningPathNodeId.HasValue && IsPathCompletionActivity(dto))
                {
                    handled = await _pathViewService.MarkNodeCompletedAsync(
                        dto.LearningPathNodeId.Value,
                        studentId,
                        dto.ActivityType,
                        dto.DurationMinutes,
                        dto.Score,
                        dto.Metadata);

                    if (!handled)
                    {
                        return BadRequest(new { success = false, message = "Không thể cập nhật node lộ trình." });
                    }

                    await _studentProgressService.UpdateProgressSnapshotsAsync(studentId);
                }
                else if (dto.ActivityType.Equals(ActivityType.Learn, System.StringComparison.OrdinalIgnoreCase) && dto.LearningPathNodeId.HasValue)
                {
                    var node = await _context.LearningPathNodes.FindAsync(dto.LearningPathNodeId.Value);
                    if (node != null && node.LessonId.HasValue)
                    {
                        handled = await _progressTrackingService.RecordLessonCompleted(studentId, node.LessonId.Value, dto.DurationMinutes);
                    }
                }
                else if (dto.ActivityType.Equals(ActivityType.Quiz, System.StringComparison.OrdinalIgnoreCase) && dto.LearningPathNodeId.HasValue)
                {
                    var node = await _context.LearningPathNodes.FindAsync(dto.LearningPathNodeId.Value);
                    if (node != null && node.QuizId.HasValue)
                    {
                        handled = await _progressTrackingService.RecordQuizSubmitted(studentId, node.QuizId.Value, dto.Score ?? 0m, dto.DurationMinutes);
                    }
                }
                else if (dto.ActivityType.Equals(ActivityType.Chat, System.StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(dto.Metadata, out int sessionId))
                    {
                        handled = await _progressTrackingService.RecordTutorMessage(studentId, sessionId, dto.DurationMinutes);
                    }
                }
                else if (dto.ActivityType.Equals(ActivityType.Review, System.StringComparison.OrdinalIgnoreCase) && dto.TopicId.HasValue)
                {
                    var node = await _context.LearningPathNodes
                        .Include(n => n.LearningPath)
                        .FirstOrDefaultAsync(n => n.LearningPath.StudentId == studentId && n.TopicId == dto.TopicId.Value && n.LearningPath.Status == "ACTIVE");
                    
                    if (node != null)
                    {
                        node.Status = ProgressStatus.NeedReview;
                        _context.LearningPathNodes.Update(node);
                        await _context.SaveChangesAsync();
                    }
                    await _studentProgressService.RecordActivityAsync(dto, studentId);
                    handled = true;
                }

                // Nếu hoạt động không đi kèm Node lộ trình hoặc thuộc dạng chung, ghi nhận qua cơ chế cơ bản
                if (!handled)
                {
                    await _studentProgressService.RecordActivityAsync(dto, studentId);
                    handled = true;
                }

                return Ok(new { success = true });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        private static bool IsPathCompletionActivity(ActivityLogCreateDto dto)
        {
            if (dto.ActivityType.Equals(ActivityType.Quiz, System.StringComparison.OrdinalIgnoreCase) &&
                dto.Score.HasValue &&
                dto.Score.Value < 5.0m)
            {
                return false;
            }

            return dto.ActivityType.Equals(ActivityType.Learn, System.StringComparison.OrdinalIgnoreCase) ||
                   dto.ActivityType.Equals(ActivityType.Quiz, System.StringComparison.OrdinalIgnoreCase) ||
                   dto.ActivityType.Equals(ActivityType.Practice, System.StringComparison.OrdinalIgnoreCase);
        }

        [HttpGet]
        public async Task<IActionResult> SkillDetail(int skillId, int? studentId = null)
        {
            int currentUserId = GetUserId();
            int targetStudentId = studentId ?? currentUserId;

            if (User.IsInRole("STUDENT") && targetStudentId != currentUserId)
            {
                return Forbid();
            }

            var skill = await _context.EnglishSkills.FindAsync(skillId);
            if (skill == null) return NotFound();

            var pathNodes = await _context.LearningPathNodes
                .Include(n => n.Topic)
                .Include(n => n.Lesson)
                .Include(n => n.Quiz)
                .Where(n => n.LearningPath.StudentId == targetStudentId && n.Topic != null && n.Topic.SkillId == skillId && n.LearningPath.Status == "ACTIVE")
                .OrderBy(n => n.OrderIndex)
                .ToListAsync();

            var snapshot = await _progressRepo.GetSkillProgress(targetStudentId, skillId);
            
            var viewModel = new SkillProgressViewModel
            {
                SkillId = skill.Id,
                SkillName = skill.SkillName,
                SkillCode = skill.SkillCode,
                ProgressPercent = snapshot?.ProgressPercent ?? 0m,
                AverageScore = snapshot?.AverageScore,
                CompletedNodes = snapshot?.CompletedNodes ?? 0,
                TotalNodes = pathNodes.Count
            };

            ViewBag.Nodes = pathNodes;
            ViewBag.TargetStudentId = targetStudentId;
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> TopicDetail(int topicId, int? studentId = null)
        {
            int currentUserId = GetUserId();
            int targetStudentId = studentId ?? currentUserId;

            if (User.IsInRole("STUDENT") && targetStudentId != currentUserId)
            {
                return Forbid();
            }

            var topic = await _context.LearningTopics
                .Include(t => t.Skill)
                .FirstOrDefaultAsync(t => t.Id == topicId);
            if (topic == null) return NotFound();

            var pathNodes = await _context.LearningPathNodes
                .Include(n => n.Lesson)
                .Include(n => n.Quiz)
                .Where(n => n.LearningPath.StudentId == targetStudentId && n.TopicId == topicId && n.LearningPath.Status == "ACTIVE")
                .OrderBy(n => n.OrderIndex)
                .ToListAsync();

            var snapshot = await _progressRepo.GetTopicProgress(targetStudentId, topicId);

            var viewModel = new TopicProgressViewModel
            {
                TopicId = topic.Id,
                TopicName = topic.Title,
                TopicCode = topic.TopicCode ?? "",
                SkillName = topic.Skill?.SkillName ?? "",
                ProgressPercent = snapshot?.ProgressPercent ?? 0m,
                AverageScore = snapshot?.AverageScore,
                IsWeakArea = pathNodes.Any(n => n.Status == ProgressStatus.NeedReview) || (snapshot?.AverageScore.HasValue == true && snapshot.AverageScore.Value < 5.0m)
            };

            ViewBag.Nodes = pathNodes;
            ViewBag.TargetStudentId = targetStudentId;
            return View(viewModel);
        }

        [HttpGet("api/student/{studentId}/path/{pathId}/replanning-input")]
        [AllowAnonymous]
        public async Task<IActionResult> GetReplanningInput(int studentId, int pathId)
        {
            string? apiKey = Request.Headers["X-API-Key"];
            bool isAdmin = User.Identity?.IsAuthenticated == true && User.IsInRole("ADMIN");
            
            if (apiKey != "M17-AI-REPLANNING-SECRET-KEY" && !isAdmin)
            {
                System.Diagnostics.Debug.WriteLine($"Unauthorized access attempt to GetReplanningInput for student: {studentId}");
                return Forbid();
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"M17 Replanning data fetched for student ID {studentId}, learning path ID {pathId}");
                var result = await _studentProgressService.GetReplanningInputAsync(studentId, pathId);
                return Ok(result);
            }
            catch (System.Collections.Generic.KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
