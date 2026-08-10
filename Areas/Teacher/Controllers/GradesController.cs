using ClosedXML.Excel;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Security.Claims;

namespace DuAnTotNghiep.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = "TEACHER")]
    public class GradesController : Controller
    {
        private readonly ITeacherGradingService _gradingService;

        public GradesController(ITeacherGradingService gradingService)
        {
            _gradingService = gradingService;
        }

        public async Task<IActionResult> Index(int? topicId)
        {
            ViewBag.TopicId = topicId;
            var overview = await _gradingService.GetGradesOverviewAsync(topicId);

            var allGrades = topicId.HasValue ? await _gradingService.GetGradesOverviewAsync(null) : overview;
            ViewBag.CourseOptions = allGrades
                .Select(g => g.TopicName)
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            return View(overview);
        }

        public async Task<IActionResult> ExportExcel(string? searchName, string? courseName)
        {
            var grades = await _gradingService.GetGradesOverviewAsync(null);

            if (!string.IsNullOrWhiteSpace(searchName))
            {
                var term = searchName.Trim().ToLower();
                grades = grades.Where(g => g.StudentName.ToLower().Contains(term)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(courseName))
            {
                var course = courseName.Trim().ToLower();
                grades = grades.Where(g => g.TopicName.ToLower() == course || g.TopicName.ToLower().Contains(course)).ToList();
            }

            if (!grades.Any())
            {
                return Content("<script>alert('Không có dữ liệu để xuất');history.back();</script>", "text/html");
            }

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Sổ điểm học viên");

            worksheet.Cell(1, 1).Value = "STT";
            worksheet.Cell(1, 2).Value = "Học viên";
            worksheet.Cell(1, 3).Value = "Khóa học";
            worksheet.Cell(1, 4).Value = "Điểm Quiz";
            worksheet.Cell(1, 5).Value = "Điểm bài tập";
            worksheet.Cell(1, 6).Value = "Tổng hợp";

            var headerRow = worksheet.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int row = 2;
            int stt = 1;
            foreach (var item in grades)
            {
                worksheet.Cell(row, 1).Value = stt++;
                worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 2).Value = item.StudentName;
                worksheet.Cell(row, 3).Value = item.TopicName;

                worksheet.Cell(row, 4).Value = item.QuizScore;
                worksheet.Cell(row, 4).Style.NumberFormat.Format = "0.0";
                worksheet.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 5).Value = item.PracticeScore;
                worksheet.Cell(row, 5).Style.NumberFormat.Format = "0.0";
                worksheet.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 6).Value = item.TotalScore;
                worksheet.Cell(row, 6).Style.NumberFormat.Format = "0.0";
                worksheet.Cell(row, 6).Style.Font.Bold = true;
                worksheet.Cell(row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            string fileName = $"So_diem_hoc_vien_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        public async Task<IActionResult> Pending()
        {
            var teacherId = GetCurrentUserId();
            if (teacherId == 0) return RedirectToAction("Login", "Account", new { area = "" });

            var pending = await _gradingService.GetPendingSubmissionsAsync(teacherId);

            ViewBag.PendingCount = pending.Count;
            ViewBag.CourseOptions = pending
                .Select(p => p.TopicName)
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            return View(pending);
        }

        public async Task<IActionResult> Grade(int id)
        {
            var submission = await _gradingService.GetSubmissionDetailAsync(id);
            if (submission == null)
            {
                return NotFound();
            }

            return View(submission);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Grade(int id, decimal score, string? feedback)
        {
            var teacherId = GetCurrentUserId();
            if (teacherId == 0) return RedirectToAction("Login", "Account", new { area = "" });

            await _gradingService.GradeSubmissionAsync(id, score, feedback, teacherId);
            TempData["SuccessMessage"] = "Chấm bài thành công!";
            return RedirectToAction(nameof(Pending));
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out var userId) ? userId : 0;
        }
    }
}
