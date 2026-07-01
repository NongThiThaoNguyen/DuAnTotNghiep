using DuAnTotNghiep.Models.ViewModels.Teacher;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DuAnTotNghiep.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = "TEACHER")]
    public class LessonsController : Controller
    {
        private readonly ITeacherLessonService _lessonService;
        private readonly ITeacherCourseService _courseService;
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;

        public LessonsController(
            ITeacherLessonService lessonService,
            ITeacherCourseService courseService,
            Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            _lessonService = lessonService;
            _courseService = courseService;
            _env = env;
        }

        public async Task<IActionResult> Index(int courseId)
        {
            if (courseId <= 0)
            {
                return RedirectToAction("Index", "Courses");
            }

            var course = await _courseService.GetCourseDetailAsync(courseId);
            if (course == null)
            {
                return NotFound();
            }

            var model = new LessonIndexViewModel
            {
                CourseId = courseId,
                CourseTitle = course.Title,
                Lessons = await _lessonService.GetLessonsByTopicAsync(courseId)
            };

            return View(model);
        }

        public async Task<IActionResult> Create(int courseId)
        {
            if (courseId <= 0)
            {
                return RedirectToAction("Index", "Courses");
            }

            var course = await _courseService.GetCourseDetailAsync(courseId);
            if (course == null)
            {
                return NotFound();
            }

            return View(new CreateLessonViewModel
            {
                TopicId = courseId,
                TopicTitle = course.Title,
                DifficultyLevel = course.DifficultyLevel,
                EstimatedMinutes = 15
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateLessonViewModel model)
        {
            var teacherId = GetCurrentUserId();
            if (teacherId == 0) return RedirectToAction("Login", "Account", new { area = "" });

            if (!ModelState.IsValid)
            {
                await PopulateTopicTitleAsync(model);
                return View(model);
            }

            if (model.InputMethod == "UPLOAD" && model.UploadFile != null && model.UploadFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "lessons");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.UploadFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.UploadFile.CopyToAsync(fileStream);
                }
                model.Content = "/uploads/lessons/" + uniqueFileName;
            }

            if (model.UploadVideo != null && model.UploadVideo.Length > 0)
            {
                string videoFolder = Path.Combine(_env.WebRootPath, "uploads", "lessons", "videos");
                if (!Directory.Exists(videoFolder))
                {
                    Directory.CreateDirectory(videoFolder);
                }
                string uniqueVideoName = Guid.NewGuid().ToString() + "_" + model.UploadVideo.FileName;
                string videoPath = Path.Combine(videoFolder, uniqueVideoName);
                using (var stream = new FileStream(videoPath, FileMode.Create))
                {
                    await model.UploadVideo.CopyToAsync(stream);
                }
                model.VideoUrl = "/uploads/lessons/videos/" + uniqueVideoName;
            }

            await _lessonService.CreateLessonAsync(model, teacherId);
            TempData["SuccessMessage"] = "Bài học đã được tạo thành công!";
            return RedirectToAction(nameof(Index), new { courseId = model.TopicId });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var lesson = await _lessonService.GetLessonDetailAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }

            return View(new EditLessonViewModel
            {
                Id = lesson.Id,
                TopicId = lesson.TopicId,
                TopicTitle = lesson.TopicTitle,
                Title = lesson.Title,
                Summary = lesson.Summary,
                Content = lesson.Content,
                EstimatedMinutes = lesson.EstimatedMinutes,
                DifficultyLevel = "BASIC",
                VideoUrl = lesson.VideoUrl,
                InputMethod = lesson.ContentType == "FILE" ? "UPLOAD" : "EDITOR"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditLessonViewModel model)
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

            if (model.InputMethod == "UPLOAD" && model.UploadFile != null && model.UploadFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "lessons");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.UploadFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.UploadFile.CopyToAsync(fileStream);
                }
                model.Content = "/uploads/lessons/" + uniqueFileName;
            }

            if (model.UploadVideo != null && model.UploadVideo.Length > 0)
            {
                string videoFolder = Path.Combine(_env.WebRootPath, "uploads", "lessons", "videos");
                if (!Directory.Exists(videoFolder))
                {
                    Directory.CreateDirectory(videoFolder);
                }
                string uniqueVideoName = Guid.NewGuid().ToString() + "_" + model.UploadVideo.FileName;
                string videoPath = Path.Combine(videoFolder, uniqueVideoName);
                using (var stream = new FileStream(videoPath, FileMode.Create))
                {
                    await model.UploadVideo.CopyToAsync(stream);
                }
                model.VideoUrl = "/uploads/lessons/videos/" + uniqueVideoName;
            }

            await _lessonService.UpdateLessonAsync(model);
            TempData["SuccessMessage"] = "Cập nhật bài học thành công!";
            return RedirectToAction(nameof(Index), new { courseId = model.TopicId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int courseId)
        {
            await _lessonService.DeleteLessonAsync(id);
            TempData["SuccessMessage"] = "Xóa bài học thành công!";
            return RedirectToAction(nameof(Index), new { courseId });
        }

        public async Task<IActionResult> Detail(int id)
        {
            var lesson = await _lessonService.GetLessonDetailAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }

            return View("Detail", lesson);
        }

        public Task<IActionResult> Details(int id)
        {
            return Detail(id);
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out var userId) ? userId : 0;
        }

        private async Task PopulateTopicTitleAsync(CreateLessonViewModel model)
        {
            var course = await _courseService.GetCourseDetailAsync(model.TopicId);
            model.TopicTitle = course?.Title ?? string.Empty;
        }

        [HttpPost]
        [AllowAnonymous] // Assuming it's called via AJAX, we could restrict it if needed
        public IActionResult GenerateAiContent(string prompt)
        {
            // Simulate AI content generation for the prompt
            string htmlContent = $@"
<div class='ai-generated-content p-4 bg-slate-50 rounded border border-slate-200'>
    <h3 class='text-primary mb-3'>Nội dung sinh tự động bởi AI</h3>
    <p><strong>Chủ đề yêu cầu:</strong> {prompt}</p>
    <p>Chào mừng bạn đến với bài học này. Dưới đây là những nội dung trọng tâm bạn cần nắm vững.</p>
    <div class='alert alert-info shadow-sm mb-4'>
        <h5><i class='fa-solid fa-lightbulb text-warning me-2'></i>Điểm cần lưu ý</h5>
        <ul class='mb-0 mt-2'>
            <li>Hiểu rõ định nghĩa và cách sử dụng cơ bản.</li>
            <li>Ghi nhớ các từ vựng cốt lõi thường xuất hiện.</li>
            <li>Áp dụng vào giao tiếp thực tế.</li>
        </ul>
    </div>
</div>";
            return Json(new { success = true, content = htmlContent });
        }
    }
}
