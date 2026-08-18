using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using DuAnTotNghiep.Services.Interfaces;

namespace DuAnTotNghiep.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "STUDENT")]
    public class ClassEnrollmentController : Controller
    {
        private readonly IClassEnrollmentService _enrollmentService;

        public ClassEnrollmentController(IClassEnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }

        private int GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out int userId);
            return userId;
        }

        /// <summary>
        /// GET: /Student/ClassEnrollment
        /// Hiển thị danh sách lớp phù hợp với level của Student
        /// </summary>
        public async Task<IActionResult> Index()
        {
            int userId = GetUserId();

            // Kiểm tra đã hoàn thành Placement Test chưa
            var hasCompleted = await _enrollmentService.HasCompletedPlacementTestAsync(userId);
            if (!hasCompleted)
            {
                TempData["ErrorMessage"] = "Bạn cần hoàn thành bài kiểm tra đầu vào trước khi chọn lớp.";
                return RedirectToAction("Intro", "PlacementTest");
            }

            var vm = await _enrollmentService.GetAvailableClassesAsync(userId);
            return View(vm);
        }

        /// <summary>
        /// GET: /Student/ClassEnrollment/Confirm/{classroomId}
        /// Hiển thị trang xác nhận trước khi đăng ký lớp
        /// </summary>
        public async Task<IActionResult> Confirm(int id)
        {
            int userId = GetUserId();

            var vm = await _enrollmentService.GetClassConfirmInfoAsync(userId, id);
            if (vm == null)
            {
                TempData["ErrorMessage"] = "Lớp học không tồn tại hoặc không khả dụng.";
                return RedirectToAction("Index");
            }

            return View(vm);
        }

        /// <summary>
        /// POST: /Student/ClassEnrollment/Enroll
        /// Xử lý đăng ký lớp
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int classroomId)
        {
            int userId = GetUserId();

            var (success, errorMessage) = await _enrollmentService.EnrollStudentAsync(userId, classroomId);

            if (!success)
            {
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction("Index");
            }

            TempData["SuccessMessage"] = "Đăng ký lớp học thành công!";
            return RedirectToAction("Success");
        }

        /// <summary>
        /// GET: /Student/ClassEnrollment/Success
        /// Hiển thị trang thành công sau khi đăng ký
        /// </summary>
        public async Task<IActionResult> Success()
        {
            int userId = GetUserId();

            var vm = await _enrollmentService.GetCurrentEnrollmentAsync(userId);
            if (vm == null)
            {
                return RedirectToAction("Index");
            }

            return View(vm);
        }
    }
}
