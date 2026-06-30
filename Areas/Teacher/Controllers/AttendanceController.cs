using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    public class AttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        public class StudentAttendanceViewModel
        {
            public int StudentId { get; set; }
            public string StudentName { get; set; } = "";
            public string StudentEmail { get; set; } = "";
            public string Status { get; set; } = "PRESENT";
            public string? Remarks { get; set; }
        }

        // GET: Teacher/Attendance
        public async Task<IActionResult> Index(int? topicId, DateOnly? date)
        {
            var topics = await _context.LearningTopics.Where(t => t.Status == "ACTIVE").ToListAsync();
            ViewBag.TopicsList = new SelectList(topics, "Id", "Title", topicId);

            var selectedDate = date ?? DateOnly.FromDateTime(DateTime.Today);
            ViewBag.SelectedDate = selectedDate.ToString("yyyy-MM-dd");
            ViewBag.TopicId = topicId;

            var studentAttendances = new List<StudentAttendanceViewModel>();

            if (topicId.HasValue)
            {
                var students = await _context.Users
                    .Include(u => u.Role)
                    .Where(u => u.Role.RoleCode == "STUDENT" && u.Status == "ACTIVE")
                    .OrderBy(u => u.FullName)
                    .ToListAsync();

                var records = await _context.Attendances
                    .Where(a => a.TopicId == topicId.Value && a.AttendanceDate == selectedDate)
                    .ToDictionaryAsync(a => a.StudentId);

                foreach (var student in students)
                {
                    var status = "PRESENT";
                    string? remarks = null;

                    if (records.TryGetValue(student.Id, out var rec))
                    {
                        status = rec.Status;
                        remarks = rec.Remarks;
                    }

                    studentAttendances.Add(new StudentAttendanceViewModel
                    {
                        StudentId = student.Id,
                        StudentName = student.FullName,
                        StudentEmail = student.Email,
                        Status = status,
                        Remarks = remarks
                    });
                }
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

            foreach (var att in attendances)
            {
                var record = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.TopicId == topicId && a.StudentId == att.StudentId && a.AttendanceDate == date);

                if (record == null)
                {
                    record = new Attendance
                    {
                        TopicId = topicId,
                        StudentId = att.StudentId,
                        AttendanceDate = date,
                        Status = att.Status,
                        Remarks = att.Remarks,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Add(record);
                }
                else
                {
                    record.Status = att.Status;
                    record.Remarks = att.Remarks;
                    record.UpdatedAt = DateTime.UtcNow;
                    _context.Update(record);
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã lưu thông tin điểm danh học viên!";
            return RedirectToAction(nameof(Index), new { topicId, date });
        }

        // GET: Teacher/Attendance/History
        public async Task<IActionResult> History(int? topicId, DateOnly? startDate, DateOnly? endDate)
        {
            ViewBag.TopicsList = new SelectList(await _context.LearningTopics.Where(t => t.Status == "ACTIVE").ToListAsync(), "Id", "Title", topicId);

            var query = _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Topic)
                .AsNoTracking();

            if (topicId.HasValue)
            {
                query = query.Where(a => a.TopicId == topicId);
            }
            if (startDate.HasValue)
            {
                query = query.Where(a => a.AttendanceDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(a => a.AttendanceDate <= endDate.Value);
            }

            var records = await query
                .OrderByDescending(a => a.AttendanceDate)
                .ThenBy(a => a.Student.FullName)
                .ToListAsync();

            return View(records);
        }
    }
}
