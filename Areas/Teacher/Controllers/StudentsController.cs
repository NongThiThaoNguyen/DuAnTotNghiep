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
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Teacher/Students
        public async Task<IActionResult> Index(string? keyword, int page = 1, int pageSize = 12)
        {
            var query = _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role.RoleCode == "STUDENT")
                .AsNoTracking();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(u => u.FullName.Contains(keyword) || u.Email.Contains(keyword) || (u.Phone != null && u.Phone.Contains(keyword)));
            }

            int totalItems = await query.CountAsync();
            var items = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Keyword = keyword;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(items);
        }

        // GET: Teacher/Students/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var student = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id && u.Role.RoleCode == "STUDENT");

            if (student == null)
            {
                return NotFound();
            }

            // 1. Get student progress snapshots
            var snapshots = await _context.StudentProgressSnapshots
                .Include(s => s.Skill)
                .Include(s => s.Topic)
                .Where(s => s.StudentId == id)
                .OrderByDescending(s => s.SnapshotDate)
                .Take(10)
                .ToListAsync();
            ViewBag.ProgressSnapshots = snapshots;

            // 2. Get student learning path nodes status
            var pathNodes = await _context.LearningPathNodes
                .Include(n => n.Topic)
                .Where(n => _context.StudentLearningPaths
                    .Where(p => p.StudentId == id && p.Status == "ACTIVE")
                    .Select(p => p.Id)
                    .Contains(n.LearningPathId))
                .OrderBy(n => n.OrderIndex)
                .ToListAsync();
            ViewBag.PathNodes = pathNodes;

            // 3. Get recent study activity logs
            var activities = await _context.StudyActivityLogs
                .Include(a => a.Topic)
                .Where(a => a.StudentId == id)
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .ToListAsync();
            ViewBag.StudyActivities = activities;

            return View(student);
        }
    }
}
