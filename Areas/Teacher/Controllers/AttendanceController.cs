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

            // Auto-select first active course if no topic is explicitly selected
            if (!topicId.HasValue && topics.Any())
            {
                topicId = topics.First().Id;
            }

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

        // POST: Teacher/Attendance/UpdateRecord
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRecord(int id, string status, string? remarks, int? topicId, DateOnly? startDate, DateOnly? endDate)
        {
            var success = await _attendanceService.UpdateAttendanceRecordAsync(id, status, remarks);
            if (!success)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Không tìm thấy bản ghi điểm danh." });
                }
                TempData["ErrorMessage"] = "Không tìm thấy bản ghi điểm danh.";
                return RedirectToAction(nameof(History), new { topicId, startDate, endDate });
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "Đã cập nhật điểm danh thành công!" });
            }

            TempData["SuccessMessage"] = "Đã cập nhật điểm danh!";
            return RedirectToAction(nameof(History), new { topicId, startDate, endDate });
        }

        // GET: Teacher/Attendance/ExportExcel
        public async Task<IActionResult> ExportExcel(int? topicId, DateOnly? date, DateOnly? startDate, DateOnly? endDate)
        {
            var fileBytes = await _attendanceService.ExportAttendanceToExcelAsync(topicId, date, startDate, endDate);
            
            string fileName;
            if (topicId.HasValue && date.HasValue && !startDate.HasValue && !endDate.HasValue)
            {
                fileName = $"DiemDanh_Topic{topicId}_{date:yyyyMMdd}.xlsx";
            }
            else
            {
                fileName = $"LichSuDiemDanh_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            }

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // GET: Teacher/Attendance/DownloadTemplate
        public async Task<IActionResult> DownloadTemplate(int topicId, DateOnly date)
        {
            var fileBytes = await _attendanceService.GenerateAttendanceExcelTemplateAsync(topicId, date);
            string fileName = $"Mau_DiemDanh_Topic{topicId}_{date:yyyyMMdd}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // POST: Teacher/Attendance/ImportExcel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportExcel(int topicId, DateOnly date, Microsoft.AspNetCore.Http.IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn file Excel để nhập dữ liệu.";
                return RedirectToAction(nameof(Index), new { topicId, date });
            }

            var ext = System.IO.Path.GetExtension(excelFile.FileName).ToLowerInvariant();
            if (ext != ".xlsx" && ext != ".xls")
            {
                TempData["ErrorMessage"] = "Định dạng file không hợp lệ! Vui lòng tải lên file Excel (.xlsx hoặc .xls).";
                return RedirectToAction(nameof(Index), new { topicId, date });
            }

            try
            {
                using var stream = excelFile.OpenReadStream();
                var (successCount, errors) = await _attendanceService.ImportAttendanceFromExcelAsync(topicId, date, stream);

                if (errors.Any())
                {
                    TempData["WarningMessage"] = $"Đã cập nhật điểm danh {successCount} học viên. Tuy nhiên có một số lỗi: " + string.Join(" ", errors.Take(3));
                }
                else
                {
                    TempData["SuccessMessage"] = $"Nhập dữ liệu Excel thành công! Đã cập nhật điểm danh cho {successCount} học viên.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xử lý file Excel: " + ex.Message;
            }

            return RedirectToAction(nameof(Index), new { topicId, date });
        }

        // GET: Teacher/Attendance/ExportPdf
        public async Task<IActionResult> ExportPdf(int? topicId, DateOnly? date, DateOnly? startDate, DateOnly? endDate)
        {
            var topics = await _attendanceService.GetActiveTopicsAsync();
            ViewBag.SelectedTopicName = topicId.HasValue ? topics.FirstOrDefault(t => t.Id == topicId.Value)?.Title : "Tất cả các khóa học";
            ViewBag.TopicId = topicId;
            ViewBag.Date = date;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            if (topicId.HasValue && date.HasValue && !startDate.HasValue && !endDate.HasValue)
            {
                var studentAttendances = await _attendanceService.GetAttendanceListAsync(topicId.Value, date.Value);
                ViewBag.IsDailyReport = true;
                return View("PrintPdf", studentAttendances);
            }
            else
            {
                var records = await _attendanceService.GetAttendanceHistoryAsync(topicId, startDate, endDate);
                ViewBag.IsDailyReport = false;
                ViewBag.HistoryRecords = records;
                return View("PrintPdf", new List<StudentAttendanceViewModel>());
            }
        }
    }
}
