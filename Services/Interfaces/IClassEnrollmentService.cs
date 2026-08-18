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
    }
}
