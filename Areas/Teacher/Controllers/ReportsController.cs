using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = "TEACHER")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. General Metrics
            ViewBag.TotalStudents = await _context.Users.CountAsync(u => u.Role.RoleCode == "STUDENT" && u.Status == "ACTIVE");
            ViewBag.TotalCourses = await _context.LearningTopics.CountAsync(t => t.Status == "ACTIVE");
            ViewBag.TotalLessons = await _context.OriginalLessons.CountAsync();
            ViewBag.TotalSubmissions = await _context.PracticeSubmissions.CountAsync();

            // 2. Attendance Status Distribution (Donut Chart)
            var attendanceStats = await _context.Attendances
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            ViewBag.AttendanceLabels = attendanceStats.Select(s => s.Status == "PRESENT" ? "Đi học" : (s.Status == "ABSENT" ? "Vắng học" : (s.Status == "LATE" ? "Đi muộn" : "Có phép"))).ToList();
            ViewBag.AttendanceValues = attendanceStats.Select(s => s.Count).ToList();

            // 3. Average Score per English Skill (Bar Chart)
            var skillStats = await _context.PracticeSubmissions
                .Include(s => s.PracticeTask)
                .ThenInclude(t => t.Skill)
                .Where(s => s.Score.HasValue)
                .GroupBy(s => s.PracticeTask.Skill.SkillName)
                .Select(g => new { SkillName = g.Key, AvgScore = g.Average(s => s.Score!.Value) })
                .ToListAsync();

            ViewBag.SkillLabels = skillStats.Select(s => s.SkillName).ToList();
            ViewBag.SkillValues = skillStats.Select(s => (double)s.AvgScore).ToList();

            // 4. Learning Path completion status
            var nodeStats = await _context.LearningPathNodes
                .GroupBy(n => n.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            ViewBag.NodeLabels = nodeStats.Select(n => n.Status).ToList();
            ViewBag.NodeValues = nodeStats.Select(n => n.Count).ToList();

            return View();
        }
    }
}
