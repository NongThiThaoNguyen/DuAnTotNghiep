using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = "TEACHER")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null && int.TryParse(claim.Value, out int userId))
            {
                return userId;
            }
            return 0;
        }

        public async Task<IActionResult> Index()
        {
            int teacherId = GetCurrentUserId();
            if (teacherId == 0) return RedirectToAction("Login", "Account", new { area = "" });

            var teacher = await _context.Users.FirstOrDefaultAsync(u => u.Id == teacherId);
            ViewBag.TeacherName = teacher?.FullName ?? "Giáo viên";
            ViewBag.AvatarUrl = string.IsNullOrEmpty(teacher?.AvatarUrl) ? $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(teacher?.FullName ?? "GV")}&background=6C63FF&color=fff" : teacher.AvatarUrl;

            // 1. Stats Calculation
            int coursesCount = await _context.LearningTopics.CountAsync(t => t.Status == "ACTIVE");
            int studentsCount = await _context.Users.CountAsync(u => u.Role.RoleCode == "STUDENT" && u.Status == "ACTIVE");
            int pendingSubmissionsCount = await _context.PracticeSubmissions.CountAsync(s => s.Status == "SUBMITTED");
            
            DateOnly todayDate = DateOnly.FromDateTime(DateTime.Today);
            int todaySchedulesCount = await _context.Schedules.CountAsync(s => s.TeacherId == teacherId && EF.Functions.DateDiffDay(s.StartTime, DateTime.Today) == 0);

            ViewBag.CoursesCount = coursesCount;
            ViewBag.StudentsCount = studentsCount;
            ViewBag.PendingSubmissionsCount = pendingSubmissionsCount;
            ViewBag.TodaySchedulesCount = todaySchedulesCount;

            // 2. Recent Submissions
            var recentSubmissions = await _context.PracticeSubmissions
                .Include(s => s.Student)
                .Include(s => s.PracticeTask)
                .OrderByDescending(s => s.SubmittedAt)
                .Take(5)
                .ToListAsync();
            ViewBag.RecentSubmissions = recentSubmissions;

            // 3. Today's Schedule
            var todaySchedules = await _context.Schedules
                .Include(s => s.Topic)
                .Where(s => s.TeacherId == teacherId && EF.Functions.DateDiffDay(s.StartTime, DateTime.Today) == 0)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
            ViewBag.TodaySchedules = todaySchedules;

            // 4. Course Overview Chart Data (Skill distribution of courses)
            var courseDistribution = await _context.LearningTopics
                .Include(t => t.Skill)
                .GroupBy(t => t.Skill.SkillName)
                .Select(g => new { SkillName = g.Key, Count = g.Count() })
                .ToListAsync();

            ViewBag.ChartLabels = courseDistribution.Select(d => d.SkillName).ToList();
            ViewBag.ChartValues = courseDistribution.Select(d => d.Count).ToList();

            // 5. Recent Student Quiz Attempts
            var recentQuizAttempts = await _context.QuizAttempts
                .Include(q => q.Quiz)
                .Include(q => q.Student)
                .Where(q => q.SubmittedAt.HasValue)
                .OrderByDescending(q => q.SubmittedAt)
                .Take(5)
                .ToListAsync();
            ViewBag.RecentQuizAttempts = recentQuizAttempts;

            return View();
        }
    }
}
