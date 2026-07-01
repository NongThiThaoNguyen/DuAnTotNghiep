using DuAnTotNghiep.Models.ViewModels.Teacher;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace DuAnTotNghiep.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = "TEACHER")]
    public class QuizzesController : Controller
    {
        private readonly ITeacherQuizService _quizService;
        private readonly ITeacherCourseService _courseService;

        public QuizzesController(ITeacherQuizService quizService, ITeacherCourseService courseService)
        {
            _quizService = quizService;
            _courseService = courseService;
        }

        public async Task<IActionResult> Index(int topicId)
        {
            if (topicId <= 0)
            {
                return RedirectToAction("Index", "Courses");
            }

            var course = await _courseService.GetCourseDetailAsync(topicId);
            if (course == null)
            {
                return NotFound();
            }

            return View(new QuizIndexViewModel
            {
                TopicId = topicId,
                TopicTitle = course.Title,
                Quizzes = await _quizService.GetQuizzesByTopicAsync(topicId)
            });
        }

        public async Task<IActionResult> Create(int topicId)
        {
            var course = await _courseService.GetCourseDetailAsync(topicId);
            if (course == null)
            {
                return NotFound();
            }

            return View(new CreateQuizViewModel
            {
                TopicId = topicId,
                TopicTitle = course.Title,
                Questions =
                {
                    new QuizQuestionFormViewModel { Options = { "", "", "", "" } }
                }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateQuizViewModel model)
        {
            var teacherId = GetCurrentUserId();
            if (teacherId == 0) return RedirectToAction("Login", "Account", new { area = "" });

            if (!ModelState.IsValid)
            {
                await PopulateTopicTitleAsync(model);
                return View(model);
            }

            await _quizService.CreateQuizAsync(model, teacherId);
            TempData["SuccessMessage"] = "Quiz đã được tạo thành công!";
            return RedirectToAction(nameof(Index), new { topicId = model.TopicId });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var quiz = await _quizService.GetQuizDetailAsync(id);
            if (quiz == null)
            {
                return NotFound();
            }

            var course = await _courseService.GetCourseDetailAsync(quiz.TopicId);
            return View(new EditQuizViewModel
            {
                Id = quiz.Id,
                TopicId = quiz.TopicId,
                TopicTitle = course?.Title ?? string.Empty,
                Title = quiz.Title,
                Description = quiz.Description,
                TimeLimitMinutes = quiz.TimeLimitMinutes,
                PassScore = quiz.PassScore,
                Status = quiz.Status,
                Questions = quiz.Questions
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditQuizViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await PopulateTopicTitleAsync(model);
                return View(model);
            }

            await _quizService.UpdateQuizAsync(model);
            TempData["SuccessMessage"] = "Cập nhật Quiz thành công!";
            return RedirectToAction(nameof(Index), new { topicId = model.TopicId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int topicId)
        {
            await _quizService.DeleteQuizAsync(id);
            TempData["SuccessMessage"] = "Xóa Quiz thành công!";
            return RedirectToAction(nameof(Index), new { topicId });
        }

        public async Task<IActionResult> Results(int id)
        {
            var quiz = await _quizService.GetQuizDetailAsync(id);
            if (quiz == null)
            {
                return NotFound();
            }

            ViewBag.QuizTitle = quiz.Title;
            ViewBag.TopicId = quiz.TopicId;
            return View(await _quizService.GetQuizAttemptsAsync(id));
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out var userId) ? userId : 0;
        }

        private async Task PopulateTopicTitleAsync(CreateQuizViewModel model)
        {
            var course = await _courseService.GetCourseDetailAsync(model.TopicId);
            model.TopicTitle = course?.Title ?? string.Empty;
        }

        public async Task<IActionResult> ManageQuestions(int id)
        {
            var quiz = await _quizService.GetQuizDetailAsync(id);
            if (quiz == null) return NotFound();
            return View(quiz);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuestion(int quizId, QuizQuestionFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Thông tin câu hỏi không hợp lệ.";
                return RedirectToAction(nameof(ManageQuestions), new { id = quizId });
            }

            var teacherId = GetCurrentUserId();
            await _quizService.AddQuestionAsync(quizId, model, teacherId);
            TempData["SuccessMessage"] = "Đã thêm câu hỏi thành công!";
            return RedirectToAction(nameof(ManageQuestions), new { id = quizId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuestion(int quizId, int questionId)
        {
            await _quizService.DeleteQuestionAsync(quizId, questionId);
            TempData["SuccessMessage"] = "Đã xóa câu hỏi khỏi Quiz!";
            return RedirectToAction(nameof(ManageQuestions), new { id = quizId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportQuestions(int quizId, IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn file Excel hợp lệ.";
                return RedirectToAction(nameof(ManageQuestions), new { id = quizId });
            }

            try
            {
                var teacherId = GetCurrentUserId();
                using var stream = excelFile.OpenReadStream();
                await _quizService.AddQuestionsFromFileAsync(quizId, stream, excelFile.FileName, teacherId);
                TempData["SuccessMessage"] = "Đã Import danh sách câu hỏi thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi import: " + ex.Message;
            }

            return RedirectToAction(nameof(ManageQuestions), new { id = quizId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateAIQuestions(
            int quizId, 
            int topicId, 
            int SkillId, 
            int NumberOfQuestions, 
            string AdditionalInstructions,
            [FromServices] DuAnTotNghiep.Services.AI.AIQuizGenerationService aiService)
        {
            try
            {
                var teacherId = GetCurrentUserId();
                var dto = new DuAnTotNghiep.Models.DTOs.GenerateQuestionRequestDto
                {
                    SkillId = SkillId,
                    TopicId = topicId,
                    ProficiencyLevelId = 1, // Default
                    Difficulty = DuAnTotNghiep.Models.Enums.Difficulty.MEDIUM,
                    QuestionType = DuAnTotNghiep.Models.Enums.QuestionType.MCQ,
                    QuestionCount = NumberOfQuestions,
                    Notes = AdditionalInstructions,
                    RequestedBy = teacherId.ToString()
                };

                var result = await aiService.GenerateQuestionsAsync(dto);

                if (result.IsSuccess && result.Items != null && result.Items.Any())
                {
                    // Add items directly to the quiz
                    await _quizService.AddQuestionsFromAiAsync(quizId, result.Items, teacherId);
                    TempData["SuccessMessage"] = $"Tạo thành công {result.Items.Count} câu hỏi từ AI!";
                }
                else
                {
                    TempData["ErrorMessage"] = "AI gặp lỗi: " + (result.ErrorMessage ?? "Không xác định");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi hệ thống: " + ex.Message;
            }

            return RedirectToAction(nameof(ManageQuestions), new { id = quizId });
        }

        [HttpGet]
        public IActionResult DownloadSample(string format = "excel")
        {
            if (format == "excel")
            {
                using var workbook = new ClosedXML.Excel.XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Sample");
                worksheet.Cell(1, 1).Value = "Câu hỏi (Bắt buộc)";
                worksheet.Cell(1, 2).Value = "Đáp án A (Bắt buộc)";
                worksheet.Cell(1, 3).Value = "Đáp án B (Bắt buộc)";
                worksheet.Cell(1, 4).Value = "Đáp án C";
                worksheet.Cell(1, 5).Value = "Đáp án D";
                worksheet.Cell(1, 6).Value = "Đáp án đúng (Copy y hệt 1 trong 4 cột đáp án)";
                
                worksheet.Cell(2, 1).Value = "Thủ đô của Việt Nam là gì?";
                worksheet.Cell(2, 2).Value = "Hà Nội";
                worksheet.Cell(2, 3).Value = "Hồ Chí Minh";
                worksheet.Cell(2, 4).Value = "Đà Nẵng";
                worksheet.Cell(2, 5).Value = "Hải Phòng";
                worksheet.Cell(2, 6).Value = "Hà Nội";
                
                worksheet.Columns().AdjustToContents();

                using var stream = new System.IO.MemoryStream();
                workbook.SaveAs(stream);
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Sample_Import_Questions.xlsx");
            }
            else
            {
                using var stream = new System.IO.MemoryStream();
                using (var wordDoc = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Create(stream, DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
                {
                    var mainPart = wordDoc.AddMainDocumentPart();
                    mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
                    var body = mainPart.Document.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Body());
                    
                    body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text("Thủ đô của Việt Nam là gì?"))));
                    body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text("Hà Nội"))));
                    body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text("Hồ Chí Minh"))));
                    body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text("Đà Nẵng"))));
                    body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text("Hải Phòng"))));
                    body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text("Hà Nội"))));
                }
                
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "Sample_Import_Questions.docx");
            }
        }
    }
}
