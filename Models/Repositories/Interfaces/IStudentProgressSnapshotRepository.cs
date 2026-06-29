using DuAnTotNghiep.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Models.Repositories.Interfaces
{
    public interface IStudentProgressSnapshotRepository : IGenericRepository<StudentProgressSnapshot>
    {
        Task<StudentProgressSnapshot?> GetLatestOverallSnapshotAsync(int studentId);
        Task<List<StudentProgressSnapshot>> GetLatestSkillSnapshotsAsync(int studentId);
        Task<List<StudentProgressSnapshot>> GetLatestTopicSnapshotsAsync(int studentId);
        Task<List<StudentProgressSnapshot>> GetHistoricalSnapshotsAsync(int studentId, DateOnly startDate, DateOnly endDate);
    }
}
