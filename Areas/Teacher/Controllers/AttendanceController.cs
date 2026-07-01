using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.ViewModels.Teacher;
using DuAnTotNghiep.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = "TEACHER")]
    public class AttendanceController : Controller
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        // GET: Teacher/Attendance
        public async Task<IActionResult> Index(int? topicId, DateOnly? date)
        {
            var topics = await _attendanceService.GetActiveTopicsAsync();
            ViewBag.TopicsList = new SelectList(topics, "Id", "Title", topicId);

            var selectedDate = date ?? DateOnly.FromDateTime(DateTime.Today);
            ViewBag.SelectedDate = selectedDate.ToString("yyyy-MM-dd");
            ViewBag.TopicId = topicId;

            var studentAttendances = new List<StudentAttendanceViewModel>();

            if (topicId.HasValue)
            {
                studentAttendances = await _attendanceService.GetAttendanceListAsync(topicId.Value, selectedDate);
            }

            return View(studentAttendances);
        }

        // POST: Teacher/Attendance/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(int topicId, DateOnly date, List<StudentAttendanceViewModel> attendances)
        {
            if (attendances == null || !attendances.Any())
            {
                TempData["ErrorMessage"] = "Không có danh sách học viên để điểm danh.";
                return RedirectToAction(nameof(Index), new { topicId, date });
            }

            await _attendanceService.SaveAttendanceAsync(topicId, date, attendances);
            TempData["SuccessMessage"] = "Đã lưu thông tin điểm danh học viên!";
            return RedirectToAction(nameof(Index), new { topicId, date });
        }

        // GET: Teacher/Attendance/History
        public async Task<IActionResult> History(int? topicId, DateOnly? startDate, DateOnly? endDate)
        {
            var topics = await _attendanceService.GetActiveTopicsAsync();
            ViewBag.TopicsList = new SelectList(topics, "Id", "Title", topicId);

            var records = await _attendanceService.GetAttendanceHistoryAsync(topicId, startDate, endDate);

            return View(records);
        }
    }
}
