using System.Threading.Tasks;
using DuAnTotNghiep.Models.ViewModels.AILearn;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IStudentCourseService
    {
        Task<CoursesViewModel> GetCoursesViewModelAsync(int userId, string? category, string? search);
        Task<int?> GetFirstLessonIdAsync(int topicId);
        Task<CourseDetailViewModel?> GetCourseDetailAsync(int topicId, int userId);
    }
}
