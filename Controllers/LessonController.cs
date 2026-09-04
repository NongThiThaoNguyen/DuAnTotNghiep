using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Services.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Net;
using System.Text.RegularExpressions;

namespace DuAnTotNghiep.Controllers
{
    [Authorize]
    public class LessonController : Controller
    {
        private readonly IStudentLessonService _lessonService;
        private readonly ApplicationDbContext _context;

        public LessonController(IStudentLessonService lessonService, ApplicationDbContext context)
        {
            _lessonService = lessonService;
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

        public async Task<IActionResult> Detail(int id)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");

            var vm = await _lessonService.GetLessonDetailAsync(id, userId);
            if (vm == null) return NotFound();

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadPdf(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var lesson = await _context.OriginalLessons
                .Include(currentLesson => currentLesson.Topic)
                .FirstOrDefaultAsync(currentLesson => currentLesson.Id == id);
            if (lesson == null) return NotFound();

            QuestPDF.Settings.License = LicenseType.Community;
            var title = WebUtility.HtmlDecode(lesson.Title ?? "Bài học");
            var courseTitle = WebUtility.HtmlDecode(lesson.Topic?.Title ?? "Khóa học");
            var summary = WebUtility.HtmlDecode(lesson.Summary ?? string.Empty);
            var content = HtmlToPlainText(lesson.Content ?? string.Empty);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(42);
                    page.DefaultTextStyle(style => style.FontFamily("Arial").FontSize(11));
                    page.Header().Column(header =>
                    {
                        header.Item().Text("AI STUDY ENGLISH").FontSize(10).Bold().FontColor(Colors.Indigo.Medium);
                        header.Item().PaddingTop(8).Text(courseTitle).FontSize(12).FontColor(Colors.Grey.Darken1);
                    });
                    page.Content().Column(column =>
                    {
                        column.Spacing(12);
                        column.Item().PaddingTop(18).Text(title).FontSize(22).Bold().FontColor(Colors.Indigo.Darken3);
                        if (!string.IsNullOrWhiteSpace(summary))
                        {
                            column.Item().Background(Colors.Grey.Lighten4).Padding(12).Text(summary).Italic();
                        }
                        column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        column.Item().Text(content).LineHeight(1.35f);
                    });
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Tài liệu học tập • ");
                        text.CurrentPageNumber();
                    });
                });
            });

            var fileName = $"{SanitizeFileName(title)}.pdf";
            return File(document.GeneratePdf(), "application/pdf", fileName);
        }

        private static string HtmlToPlainText(string html)
        {
            var withLineBreaks = Regex.Replace(html, "<(br|/p|/h[1-6]|/li|/tr|/div|/ol|/ul)[^>]*>", "\n", RegexOptions.IgnoreCase);
            var withoutTags = Regex.Replace(withLineBreaks, "<[^>]+>", "");
            var decoded = WebUtility.HtmlDecode(withoutTags);
            return Regex.Replace(decoded, "[ \t]+", " ").Replace("\r", string.Empty).Trim();
        }

        private static string SanitizeFileName(string value)
        {
            var invalidCharacters = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            return Regex.Replace(value, $"[{invalidCharacters}]", "_").Trim();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkCompleted(int id)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");

            var success = await _lessonService.MarkLessonCompletedAsync(id, userId);
            if (!success) return NotFound();

            var lesson = await _context.OriginalLessons.FindAsync(id);
            if (lesson == null) return NotFound();

            // Find next sibling lesson in the topic
            var nextLesson = await _context.OriginalLessons
                .Where(l => l.TopicId == lesson.TopicId && l.Id > id)
                .OrderBy(l => l.Id)
                .FirstOrDefaultAsync();

            if (nextLesson != null)
            {
                return RedirectToAction("Detail", new { id = nextLesson.Id });
            }

            TempData["SuccessMessage"] = "Chúc mừng! Bạn đã hoàn thành toàn bộ bài học trong khóa học này!";
            return RedirectToAction("Index", "Courses");
        }
    }
}
