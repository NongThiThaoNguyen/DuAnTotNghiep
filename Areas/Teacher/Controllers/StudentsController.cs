using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DuAnTotNghiep.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = "TEACHER")]
    public class StudentsController : Controller
    {
        private readonly ITeacherStudentService _studentService;

        public StudentsController(ITeacherStudentService studentService)
        {
            _studentService = studentService;
        }

        // GET: Teacher/Students
        public async Task<IActionResult> Index(string? keyword, int page = 1, int pageSize = 12)
        {
            int totalItems = await _studentService.GetTotalStudentsAsync(keyword);
            var items = await _studentService.GetStudentsAsync(keyword, page, pageSize);

            ViewBag.Keyword = keyword;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(items);
        }

        // GET: Teacher/Students/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var student = await _studentService.GetStudentByIdAsync(id);

            if (student == null)
            {
                return NotFound();
            }

            // 1. Get student progress snapshots
            ViewBag.ProgressSnapshots = await _studentService.GetStudentProgressSnapshotsAsync(id);

            // 2. Get student learning path nodes status
            ViewBag.PathNodes = await _studentService.GetStudentLearningPathNodesAsync(id);

            // 3. Get recent study activity logs
            ViewBag.StudyActivities = await _studentService.GetStudentStudyActivitiesAsync(id);

            return View(student);
        }
    }
}
