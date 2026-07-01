using DuAnTotNghiep.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface ITeacherStudentService
    {
        Task<int> GetTotalStudentsAsync(string? keyword);
        Task<List<User>> GetStudentsAsync(string? keyword, int page, int pageSize);
        Task<User?> GetStudentByIdAsync(int id);
        Task<List<StudentProgressSnapshot>> GetStudentProgressSnapshotsAsync(int studentId, int take = 10);
        Task<List<LearningPathNode>> GetStudentLearningPathNodesAsync(int studentId);
        Task<List<StudyActivityLog>> GetStudentStudyActivitiesAsync(int studentId, int take = 10);
    }
}
