using System.Threading.Tasks;
using DuAnTotNghiep.Models.ViewModels.AILearn;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IStudentLessonService
    {
        Task<LessonViewModel?> GetLessonDetailAsync(int lessonId, int userId);
        Task<bool> MarkLessonCompletedAsync(int lessonId, int userId);
    }
}
