using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models.ViewModels.Teacher;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services
{
    public class TeacherReportService : ITeacherReportService
    {
        private readonly ApplicationDbContext _context;

        public TeacherReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ReportDashboardViewModel> GetDashboardReportAsync()
        {
            var report = new ReportDashboardViewModel();

            // 1. General Metrics
            report.TotalStudents = await _context.Users.CountAsync(u => u.Role.RoleCode == "STUDENT" && u.Status == "ACTIVE");
            report.TotalCourses = await _context.LearningTopics.CountAsync(t => t.Status == "ACTIVE");
            report.TotalLessons = await _context.OriginalLessons.CountAsync();
            report.TotalSubmissions = await _context.PracticeSubmissions.CountAsync();

            // 2. Attendance Status Distribution (Donut Chart)
            var attendanceStats = await _context.Attendances
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            report.AttendanceLabels = attendanceStats.Select(s => s.Status == "PRESENT" ? "Đi học" : (s.Status == "ABSENT" ? "Vắng học" : (s.Status == "LATE" ? "Đi muộn" : "Có phép"))).ToList();
            report.AttendanceValues = attendanceStats.Select(s => s.Count).ToList();

            // 3. Average Score per English Skill (Bar Chart)
            var skillStats = await _context.PracticeSubmissions
                .Include(s => s.PracticeTask)
                .ThenInclude(t => t.Skill)
                .Where(s => s.Score.HasValue)
                .GroupBy(s => s.PracticeTask.Skill.SkillName)
                .Select(g => new { SkillName = g.Key, AvgScore = g.Average(s => s.Score!.Value) })
                .ToListAsync();

            report.SkillLabels = skillStats.Select(s => s.SkillName).ToList();
            report.SkillValues = skillStats.Select(s => (double)s.AvgScore).ToList();

            // 4. Learning Path completion status
            var nodeStats = await _context.LearningPathNodes
                .GroupBy(n => n.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            report.NodeLabels = nodeStats.Select(n => n.Status).ToList();
            report.NodeValues = nodeStats.Select(n => n.Count).ToList();

            return report;
        }
    }
}
