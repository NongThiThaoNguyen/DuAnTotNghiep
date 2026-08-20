using DuAnTotNghiep.Models.ViewModels.Student;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    /// <summary>
    /// Service xử lý chọn lớp và enrollment cho Student sau Placement Test
    /// </summary>
    public interface IClassEnrollmentService
    {
        /// <summary>
        /// Lấy danh sách lớp phù hợp với level của Student (dựa trên kết quả Placement Test)
        /// </summary>
        Task<ClassSelectionViewModel> GetAvailableClassesAsync(int studentId);

        /// <summary>
        /// Lấy thông tin chi tiết 1 lớp để hiển thị trang xác nhận
        /// </summary>
        Task<ClassEnrollConfirmViewModel?> GetClassConfirmInfoAsync(int studentId, int classroomId);

        /// <summary>
        /// Thực hiện enrollment Student vào Classroom
        /// Returns (success, errorMessage)
        /// </summary>
        Task<(bool Success, string? ErrorMessage)> EnrollStudentAsync(int studentId, int classroomId);

        /// <summary>
        /// Lấy thông tin enrollment hiện tại của Student (nếu có)
        /// </summary>
        Task<EnrollmentSuccessViewModel?> GetCurrentEnrollmentAsync(int studentId);

        /// <summary>
        /// Kiểm tra Student đã hoàn thành Placement Test chưa
        /// </summary>
        Task<bool> HasCompletedPlacementTestAsync(int studentId);

        // ─────────────────────────────────────────────────────────────
        // PHẦN 4: CHỌN GIÁO VIÊN
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Lấy danh sách giáo viên phụ trách lớp cùng level với lớp Student đã chọn
        /// Chỉ khả dụng khi Enrollment ở trạng thái PENDING_TEACHER
        /// </summary>
        Task<TeacherSelectionViewModel?> GetTeacherSelectionAsync(int studentId);

        /// <summary>
        /// Student chọn giáo viên → cập nhật ClassroomId sang lớp của GV đó
        /// Đổi Enrollment.Status → AWAITING_CONFIRMATION
        /// </summary>
        Task<(bool Success, string? ErrorMessage)> SelectTeacherAsync(int studentId, int classroomId);

        /// <summary>
        /// Chọn giáo viên ngẫu nhiên trong danh sách phù hợp
        /// </summary>
        Task<(bool Success, string? ErrorMessage)> SelectRandomTeacherAsync(int studentId);

        /// <summary>
        /// Lấy thông tin xác nhận sau khi Student đã chọn giáo viên (AWAITING_CONFIRMATION)
        /// </summary>
        Task<TeacherSelectedSuccessViewModel?> GetTeacherSelectedSuccessAsync(int studentId);

        // ─────────────────────────────────────────────────────────────
        // PHẦN 5: XÁC NHẬN PHÂN LỚP VÀ CẬP NHẬT DANH SÁCH
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Xác nhận đăng ký cuối cùng: Đổi Enrollment.Status → ACTIVE, User.Status → STUDYING,
        /// Cập nhật trạng thái Onboarding → COMPLETED
        /// </summary>
        Task<(bool Success, string? ErrorMessage)> ConfirmFinalEnrollmentAsync(int studentId);

        /// <summary>
        /// Lấy thông tin thành công chính thức sau khi bấm "Xác nhận đăng ký"
        /// </summary>
        Task<TeacherSelectedSuccessViewModel?> GetFinalSuccessInfoAsync(int studentId);
    }
}
