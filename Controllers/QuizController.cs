using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.DTOs.Exam;
using DuAnTotNghiep.Services.Interfaces;

namespace DuAnTotNghiep.Controllers
{
    [Authorize]
    public class QuizController : Controller
    {
        private readonly IStudentQuizService _quizService;
        private readonly ApplicationDbContext _context;

        public QuizController(IStudentQuizService quizService, ApplicationDbContext context)
        {
            _quizService = quizService;
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null && int.TryParse(claim.Value, out int userId))
            {
                return userId;
            }
            return 0;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");

            var quizzes = await _quizService.GetAllQuizzesAsync(userId);
            return View(quizzes);
        }

        [HttpGet]
        public async Task<IActionResult> Take(int id) // id is course/topic ID
        {
            var vm = await _quizService.GetQuizForTakingAsync(id);

            if (vm == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài trắc nghiệm cho khóa học này.";
                return RedirectToAction("Index", "Courses");
            }

            int userId = GetCurrentUserId();

                // Track every quiz attempt so fullscreen and violation monitoring work consistently.
            var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.Id == vm.QuizId || q.TopicId == id);
            if (quiz != null)
            {
                vm.IsExamMode = true;
                vm.MaxViolations = quiz.MaxViolations > 0 ? quiz.MaxViolations : 3;

                if (userId > 0)
                {
                    // Create or get active in-progress attempt for tracking
                    var attempt = await _context.QuizAttempts
                        .Where(a => a.QuizId == quiz.Id && a.StudentId == userId && a.Status == "IN_PROGRESS")
                        .OrderByDescending(a => a.StartedAt)
                        .FirstOrDefaultAsync();

                    if (attempt == null)
                    {
                        attempt = new QuizAttempt
                        {
                            QuizId = quiz.Id,
                            StudentId = userId,
                            StartedAt = DateTime.UtcNow,
                            Status = "IN_PROGRESS"
                        };
                        _context.QuizAttempts.Add(attempt);
                        await _context.SaveChangesAsync();
                    }

                    vm.AttemptId = attempt.Id;
                }
            }

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ReportViolation([FromBody] ViolationReportDto dto)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var attempt = await _context.QuizAttempts
                .Include(currentAttempt => currentAttempt.Quiz)
                .FirstOrDefaultAsync(currentAttempt => currentAttempt.Id == dto.AttemptId);
            if (attempt != null && attempt.StudentId == userId)
            {
                attempt.FullscreenExitCount = dto.FullscreenExitCount;
                attempt.TabSwitchCount = dto.TabSwitchCount;
                if (!string.IsNullOrEmpty(dto.Details))
                {
                    attempt.ViolationLog = dto.Details;
                }

                var totalViolations = dto.FullscreenExitCount + dto.TabSwitchCount;
                if (totalViolations >= (attempt.Quiz.MaxViolations > 0 ? attempt.Quiz.MaxViolations : 3))
                {
                    var answers = await _context.QuizAnswers
                        .Where(answer => answer.QuizAttemptId == attempt.Id)
                        .ToListAsync();
                    _context.QuizAnswers.RemoveRange(answers);
                    _context.QuizAttempts.Remove(attempt);
                    await _context.SaveChangesAsync();
                    return Ok(new { success = true, terminated = true });
                }

                await _context.SaveChangesAsync();
            }

            return Ok(new { success = true, terminated = false });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(
            int quizId,
            Dictionary<int, string> answers,
            int fullscreenExitCount = 0,
            int tabSwitchCount = 0,
            string? violationLog = null)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");

            try
            {
                var result = await _quizService.SubmitQuizAsync(quizId, userId, answers);

                // Update violation stats on the latest attempt
                var latestAttempt = await _context.QuizAttempts
                    .Where(a => a.QuizId == quizId && a.StudentId == userId)
                    .OrderByDescending(a => a.SubmittedAt ?? a.StartedAt)
                    .FirstOrDefaultAsync();

                if (latestAttempt != null)
                {
                    latestAttempt.FullscreenExitCount = Math.Max(latestAttempt.FullscreenExitCount, fullscreenExitCount);
                    latestAttempt.TabSwitchCount = Math.Max(latestAttempt.TabSwitchCount, tabSwitchCount);
                    if (!string.IsNullOrEmpty(violationLog))
                    {
                        latestAttempt.ViolationLog = violationLog;
                    }
                    await _context.SaveChangesAsync();
                }

                ViewBag.Score = result.Score;
                ViewBag.CorrectCount = result.CorrectCount;
                ViewBag.TotalCount = result.TotalCount;
                ViewBag.Answers = answers;

                var vm = await _quizService.GetQuizByIdAsync(quizId);
                if (vm == null) return NotFound();

                return View("Result", vm);
            }
            catch (Exception ex)
            {
                var fullMessage = ex.InnerException != null ? $"{ex.Message} ---> Inner: {ex.InnerException.Message}" : ex.Message;
                return Content($"Submit Error: {fullMessage}\n\nTrace:\n{ex}");
            }
        }
    }
}
