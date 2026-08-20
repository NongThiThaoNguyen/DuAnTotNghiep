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

            // Nếu đang PENDING_TEACHER → cần chọn giáo viên
            vm.NeedsTeacherSelection = true;

            return View(vm);
        }

        // ─────────────────────────────────────────────────────────────
        // PHẦN 4: CHỌN GIÁO VIÊN
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// GET: /Student/ClassEnrollment/SelectTeacher
        /// Hiển thị trang chọn giáo viên
        /// </summary>
        public async Task<IActionResult> SelectTeacher()
        {
            int userId = GetUserId();

            // Nếu đã AWAITING_CONFIRMATION → chuyển đến trang xác nhận
            var confirmed = await _enrollmentService.GetTeacherSelectedSuccessAsync(userId);
            if (confirmed != null)
                return RedirectToAction("TeacherConfirmed");

            var vm = await _enrollmentService.GetTeacherSelectionAsync(userId);
            if (vm == null)
            {
                TempData["ErrorMessage"] = "Bạn cần chọn lớp trước khi chọn giáo viên.";
                return RedirectToAction("Index");
            }

            return View(vm);
        }

        /// <summary>
        /// POST: /Student/ClassEnrollment/PickTeacher
        /// Xử lý khi Student chọn giáo viên cụ thể
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PickTeacher(int classroomId)
        {
            int userId = GetUserId();

            var (success, errorMessage) = await _enrollmentService.SelectTeacherAsync(userId, classroomId);

            if (!success)
            {
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction("SelectTeacher");
            }

            return RedirectToAction("TeacherConfirmed");
        }

        /// <summary>
        /// POST: /Student/ClassEnrollment/RandomTeacher
        /// Hệ thống tự chọn giáo viên ngẫu nhiên
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RandomTeacher()
        {
            int userId = GetUserId();

            var (success, errorMessage) = await _enrollmentService.SelectRandomTeacherAsync(userId);

            if (!success)
            {
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction("SelectTeacher");
            }

            return RedirectToAction("TeacherConfirmed");
        }

        /// <summary>
        /// GET: /Student/ClassEnrollment/TeacherConfirmed
        /// Trang xác nhận đã chọn giáo viên (AWAITING_CONFIRMATION)
        /// </summary>
        public async Task<IActionResult> TeacherConfirmed()
        {
            int userId = GetUserId();

            // Nếu đã confirm rồi (ACTIVE) → sang thẳng FinalSuccess
            var finalInfo = await _enrollmentService.GetFinalSuccessInfoAsync(userId);
            if (finalInfo != null)
            {
                return RedirectToAction("FinalSuccess");
            }

            var vm = await _enrollmentService.GetTeacherSelectedSuccessAsync(userId);
            if (vm == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin xác nhận. Vui lòng thử lại.";
                return RedirectToAction("SelectTeacher");
            }

            return View(vm);
        }

        // ─────────────────────────────────────────────────────────────
        // PHẦN 5: XÁC NHẬN PHÂN LỚP VÀ CẬP NHẬT DANH SÁCH
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// POST: /Student/ClassEnrollment/ConfirmFinal
        /// Xử lý bấm "Xác nhận đăng ký" -> Chuyển Enrollment -> ACTIVE, Student -> STUDYING
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmFinal()
        {
            int userId = GetUserId();

            var (success, errorMessage) = await _enrollmentService.ConfirmFinalEnrollmentAsync(userId);
            if (!success)
            {
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction("TeacherConfirmed");
            }

            TempData["SuccessMessage"] = "Xác nhận phân lớp và giáo viên thành công!";
            return RedirectToAction("FinalSuccess");
        }

        /// <summary>
        /// GET: /Student/ClassEnrollment/FinalSuccess
        /// Trang kết quả chính thức sau khi bấm "Xác nhận đăng ký"
        /// </summary>
        public async Task<IActionResult> FinalSuccess()
        {
            int userId = GetUserId();

            var vm = await _enrollmentService.GetFinalSuccessInfoAsync(userId);
            if (vm == null)
            {
                return RedirectToAction("Index");
            }

            return View(vm);
        }
    }
}
