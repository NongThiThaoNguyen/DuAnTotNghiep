using DuAnTotNghiep.Models.ViewModels.Teacher;

namespace DuAnTotNghiep.Services.Interfaces;

public interface ITeacherLessonService
{
    Task<List<LessonListItemViewModel>> GetLessonsByTopicAsync(int topicId);
    Task<LessonDetailViewModel?> GetLessonDetailAsync(int lessonId);
    Task<int> CreateLessonAsync(CreateLessonViewModel model, int teacherId);
    Task UpdateLessonAsync(EditLessonViewModel model);
    Task DeleteLessonAsync(int lessonId);
    Task ReorderLessonsAsync(int topicId, List<int> orderedLessonIds);
}
