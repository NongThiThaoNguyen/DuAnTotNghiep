using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services
{
    public class TeacherStudentService : ITeacherStudentService
    {
        private readonly ApplicationDbContext _context;

        public TeacherStudentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetTotalStudentsAsync(string? keyword)
        {
            var query = _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role.RoleCode == "STUDENT")
                .AsNoTracking();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(u => u.FullName.Contains(keyword) || u.Email.Contains(keyword) || (u.Phone != null && u.Phone.Contains(keyword)));
            }
            return await query.CountAsync();
        }

        public async Task<List<User>> GetStudentsAsync(string? keyword, int page, int pageSize)
        {
            var query = _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role.RoleCode == "STUDENT")
                .AsNoTracking();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(u => u.FullName.Contains(keyword) || u.Email.Contains(keyword) || (u.Phone != null && u.Phone.Contains(keyword)));
            }

            return await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<User?> GetStudentByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id && u.Role.RoleCode == "STUDENT");
        }

        public async Task<List<StudentProgressSnapshot>> GetStudentProgressSnapshotsAsync(int studentId, int take = 10)
        {
            return await _context.StudentProgressSnapshots
                .Include(s => s.Skill)
                .Include(s => s.Topic)
                .Where(s => s.StudentId == studentId)
                .OrderByDescending(s => s.SnapshotDate)
                .Take(take)
                .ToListAsync();
        }

        public async Task<List<LearningPathNode>> GetStudentLearningPathNodesAsync(int studentId)
        {
            return await _context.LearningPathNodes
                .Include(n => n.Topic)
                .Where(n => _context.StudentLearningPaths
                    .Where(p => p.StudentId == studentId && p.Status == "ACTIVE")
                    .Select(p => p.Id)
                    .Contains(n.LearningPathId))
                .OrderBy(n => n.OrderIndex)
                .ToListAsync();
        }

        public async Task<List<StudyActivityLog>> GetStudentStudyActivitiesAsync(int studentId, int take = 10)
        {
            return await _context.StudyActivityLogs
                .Include(a => a.Topic)
                .Where(a => a.StudentId == studentId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(take)
                .ToListAsync();
        }
    }
}
