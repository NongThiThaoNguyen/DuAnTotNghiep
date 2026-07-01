using DuAnTotNghiep.Models.ViewModels.Teacher;

namespace DuAnTotNghiep.Services.Interfaces;

public interface ITeacherCourseService
{
    Task<List<CourseListItemViewModel>> GetCoursesAsync(int teacherId, string? search, string? skillFilter);
    Task<CourseDetailViewModel?> GetCourseDetailAsync(int courseId);
    Task<int> CreateCourseAsync(CreateCourseViewModel model, int teacherId);
    Task UpdateCourseAsync(EditCourseViewModel model);
    Task DeleteCourseAsync(int courseId);
    Task<int> GetCourseStudentCountAsync(int courseId);
    Task<List<LessonSummaryViewModel>> GetCourseLessonsAsync(int courseId);
}
