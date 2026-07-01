using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DuAnTotNghiep.Services.Interfaces;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = "TEACHER")]
    public class ReportsController : Controller
    {
        private readonly ITeacherReportService _reportService;

        public ReportsController(ITeacherReportService reportService)
        {
            _reportService = reportService;
        }

        public async Task<IActionResult> Index()
        {
            var report = await _reportService.GetDashboardReportAsync();

            // 1. General Metrics
            ViewBag.TotalStudents = report.TotalStudents;
            ViewBag.TotalCourses = report.TotalCourses;
            ViewBag.TotalLessons = report.TotalLessons;
            ViewBag.TotalSubmissions = report.TotalSubmissions;

            // 2. Attendance Status Distribution
            ViewBag.AttendanceLabels = report.AttendanceLabels;
            ViewBag.AttendanceValues = report.AttendanceValues;

            // 3. Average Score per English Skill
            ViewBag.SkillLabels = report.SkillLabels;
            ViewBag.SkillValues = report.SkillValues;

            // 4. Learning Path completion status
            ViewBag.NodeLabels = report.NodeLabels;
            ViewBag.NodeValues = report.NodeValues;

            return View();
        }
    }
}
