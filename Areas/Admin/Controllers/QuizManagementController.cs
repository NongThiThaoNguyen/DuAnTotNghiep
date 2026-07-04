using DuAnTotNghiep.Data;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMIN")]
public class QuizManagementController : Controller
{
    private const int PageSize = 20;
    private readonly IAdminQuizManagementService _quizManagementService;
    private readonly ApplicationDbContext _context;

    public QuizManagementController(IAdminQuizManagementService quizManagementService, ApplicationDbContext context)
    {
        _quizManagementService = quizManagementService;
        _context = context;
    }

    public async Task<IActionResult> Index(int? topicId, int? teacherId, int page = 1)
    {
        return View(await _quizManagementService.GetAllQuizzesAsync(topicId, teacherId, page, PageSize));
    }

    public async Task<IActionResult> Details(int quizId)
    {
        var model = await _quizManagementService.GetQuizDetailsAsync(quizId);
        return model == null ? NotFound() : View(model);
    }

    public async Task<IActionResult> Attempts(int quizId)
    {
        var model = await _quizManagementService.GetQuizAttemptsAsync(quizId);
        return model == null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int quizId)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.QuizQuestions)
            .Include(q => q.QuizAttempts)
            .FirstOrDefaultAsync(q => q.Id == quizId);

        if (quiz == null)
        {
            return NotFound();
        }

        if (quiz.QuizAttempts.Any())
        {
            quiz.Status = "ARCHIVED";
            TempData["SuccessMessage"] = "Quiz đã có lượt làm nên được chuyển sang ARCHIVED.";
        }
        else
        {
            _context.QuizQuestions.RemoveRange(quiz.QuizQuestions);
            _context.Quizzes.Remove(quiz);
            TempData["SuccessMessage"] = "Đã xóa quiz.";
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
