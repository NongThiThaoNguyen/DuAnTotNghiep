using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = "TEACHER")]
    public class GradesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GradesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public class StudentGradeRow
        {
            public int StudentId { get; set; }
            public string StudentName { get; set; } = "";
            public string StudentEmail { get; set; } = "";
            public string StudentAvatar { get; set; } = "";
            
            public int TotalAssignmentsSubmitted { get; set; }
            public decimal AverageAssignmentScore { get; set; }
            
            public int TotalQuizzesAttempted { get; set; }
            public decimal AverageQuizScore { get; set; }
            
            public decimal CombinedAverageScore { get; set; }
        }

        // GET: Teacher/Grades
        public async Task<IActionResult> Index(string? keyword, int page = 1, int pageSize = 15)
        {
            var studentQuery = _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role.RoleCode == "STUDENT" && u.Status == "ACTIVE");

            if (!string.IsNullOrEmpty(keyword))
            {
                studentQuery = studentQuery.Where(u => u.FullName.Contains(keyword) || u.Email.Contains(keyword));
            }

            int totalStudents = await studentQuery.CountAsync();
            var studentsList = await studentQuery
                .OrderBy(u => u.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var gradeRows = new List<StudentGradeRow>();

            foreach (var student in studentsList)
            {
                // 1. Get assignments data
                var subs = await _context.PracticeSubmissions
                    .Where(s => s.StudentId == student.Id && s.Score.HasValue)
                    .Select(s => s.Score!.Value)
                    .ToListAsync();
                
                int assignmentsCount = subs.Count;
                decimal avgAssignment = assignmentsCount > 0 ? subs.Average() : 0;

                // 2. Get quiz attempts data
                var quizAttempts = await _context.QuizAttempts
                    .Where(q => q.StudentId == student.Id && q.Score.HasValue)
                    .Select(q => q.Score!.Value)
                    .ToListAsync();
                
                int quizAttemptsCount = quizAttempts.Count;
                decimal avgQuiz = quizAttemptsCount > 0 ? (decimal)quizAttempts.Average() : 0;

                // 3. Combined Average
                decimal combinedAvg = 0;
                if (assignmentsCount > 0 && quizAttemptsCount > 0)
                {
                    combinedAvg = (avgAssignment + avgQuiz) / 2;
                }
                else if (assignmentsCount > 0)
                {
                    combinedAvg = avgAssignment;
                }
                else if (quizAttemptsCount > 0)
                {
                    combinedAvg = avgQuiz;
                }

                gradeRows.Add(new StudentGradeRow
                {
                    StudentId = student.Id,
                    StudentName = student.FullName,
                    StudentEmail = student.Email,
                    StudentAvatar = student.AvatarUrl ?? "/default-images/avatar.png",
                    TotalAssignmentsSubmitted = assignmentsCount,
                    AverageAssignmentScore = avgAssignment,
                    TotalQuizzesAttempted = quizAttemptsCount,
                    AverageQuizScore = avgQuiz,
                    CombinedAverageScore = combinedAvg
                });
            }

            ViewBag.Keyword = keyword;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalStudents / (double)pageSize);

            return View(gradeRows);
        }
    }
}
