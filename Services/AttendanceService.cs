using ClosedXML.Excel;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.ViewModels.Teacher;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly ApplicationDbContext _context;

        public AttendanceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<StudentAttendanceViewModel>> GetAttendanceListAsync(int topicId, DateOnly date)
        {
            var studentAttendances = new List<StudentAttendanceViewModel>();

            // Find active students associated with this topic
            var students = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role.RoleCode == "STUDENT" && u.Status == "ACTIVE" &&
                    (_context.StudentLearningPaths.Any(slp =>
                        slp.StudentId == u.Id &&
                        slp.Status == "ACTIVE" &&
                        _context.LearningPathNodes.Any(lpn => lpn.LearningPathId == slp.Id && lpn.TopicId == topicId)
                    )
                    || _context.StudentProgressSnapshots.Any(sps => sps.StudentId == u.Id && sps.TopicId == topicId)
                    || _context.StudyActivityLogs.Any(log => log.StudentId == u.Id && log.TopicId == topicId)
                    || _context.Attendances.Any(a => a.StudentId == u.Id && a.TopicId == topicId)
                    )
                )
                .OrderBy(u => u.FullName)
                .ToListAsync();

            // Fallback: If no student is explicitly mapped to this topic yet, fetch all active students in system
            if (!students.Any())
            {
                students = await _context.Users
                    .Include(u => u.Role)
                    .Where(u => u.Role.RoleCode == "STUDENT" && u.Status == "ACTIVE")
                    .OrderBy(u => u.FullName)
                    .ToListAsync();
            }

            var records = await _context.Attendances
                .Where(a => a.TopicId == topicId && a.AttendanceDate == date)
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

            return studentAttendances;
        }

        public async Task SaveAttendanceAsync(int topicId, DateOnly date, List<StudentAttendanceViewModel> attendances)
        {
            if (attendances == null || !attendances.Any())
                return;

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
        }

        public async Task<List<Attendance>> GetAttendanceHistoryAsync(int? topicId, DateOnly? startDate, DateOnly? endDate)
        {
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

            return await query
                .OrderByDescending(a => a.AttendanceDate)
                .ThenBy(a => a.Student.FullName)
                .ToListAsync();
        }

        public async Task<List<LearningTopic>> GetActiveTopicsAsync()
        {
            return await _context.LearningTopics
                .Where(t => t.Status == "ACTIVE")
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<byte[]> ExportAttendanceToExcelAsync(int? topicId, DateOnly? date, DateOnly? startDate, DateOnly? endDate)
        {
            using var workbook = new XLWorkbook();

            if (topicId.HasValue && date.HasValue && !startDate.HasValue && !endDate.HasValue)
            {
                var topic = await _context.LearningTopics.FindAsync(topicId.Value);
                var topicName = topic?.Title ?? $"Topic_{topicId}";
                var list = await GetAttendanceListAsync(topicId.Value, date.Value);

                var ws = workbook.Worksheets.Add("DiemDanh_" + date.Value.ToString("ddMM"));
                
                ws.Cell(1, 1).Value = $"BÁO CÁO ĐIỂM DANH: {topicName.ToUpper()}";
                ws.Cell(1, 1).Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Font.FontSize = 14;
                ws.Range(1, 1, 1, 5).Merge();

                ws.Cell(2, 1).Value = $"Ngày điểm danh: {date.Value:dd/MM/yyyy} | Tổng số học viên: {list.Count}";
                ws.Cell(2, 1).Style.Font.Italic = true;
                ws.Range(2, 1, 2, 5).Merge();

                int startRow = 4;
                ws.Cell(startRow, 1).Value = "STT";
                ws.Cell(startRow, 2).Value = "Email học viên";
                ws.Cell(startRow, 3).Value = "Họ và tên";
                ws.Cell(startRow, 4).Value = "Trạng thái";
                ws.Cell(startRow, 5).Value = "Ghi chú";

                var headerRange = ws.Range(startRow, 1, startRow, 5);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#2563EB");
                headerRange.Style.Font.FontColor = XLColor.White;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                int row = startRow + 1;
                int stt = 1;
                foreach (var item in list)
                {
                    ws.Cell(row, 1).Value = stt++;
                    ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, 2).Value = item.StudentEmail;
                    ws.Cell(row, 3).Value = item.StudentName;

                    var statusCell = ws.Cell(row, 4);
                    statusCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    statusCell.Style.Font.Bold = true;

                    switch (item.Status)
                    {
                        case "PRESENT":
                            statusCell.Value = "Đi học";
                            statusCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#DCFCE7");
                            statusCell.Style.Font.FontColor = XLColor.FromHtml("#15803D");
                            break;
                        case "LATE":
                            statusCell.Value = "Đi muộn";
                            statusCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#FEF3C7");
                            statusCell.Style.Font.FontColor = XLColor.FromHtml("#B45309");
                            break;
                        case "EXCUSED":
                            statusCell.Value = "Có phép";
                            statusCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#DBEAFE");
                            statusCell.Style.Font.FontColor = XLColor.FromHtml("#1D4ED8");
                            break;
                        default:
                            statusCell.Value = "Vắng mặt";
                            statusCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#FEE2E2");
                            statusCell.Style.Font.FontColor = XLColor.FromHtml("#B91C1C");
                            break;
                    }

                    ws.Cell(row, 5).Value = item.Remarks ?? "";
                    row++;
                }

                ws.Columns().AdjustToContents();
            }
            else
            {
                var records = await GetAttendanceHistoryAsync(topicId, startDate, endDate);
                var ws = workbook.Worksheets.Add("LichSuDiemDanh");

                ws.Cell(1, 1).Value = "BÁO CÁO LỊCH SỬ ĐIỂM DANH HỌC VIÊN";
                ws.Cell(1, 1).Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Font.FontSize = 14;
                ws.Range(1, 1, 1, 6).Merge();

                string subtitle = "Thời gian: ";
                if (startDate.HasValue && endDate.HasValue) subtitle += $"Từ {startDate.Value:dd/MM/yyyy} đến {endDate.Value:dd/MM/yyyy}";
                else if (startDate.HasValue) subtitle += $"Từ {startDate.Value:dd/MM/yyyy}";
                else if (endDate.HasValue) subtitle += $"Đến {endDate.Value:dd/MM/yyyy}";
                else subtitle += "Tất cả các ngày";
                subtitle += $" | Tổng bản ghi: {records.Count}";

                ws.Cell(2, 1).Value = subtitle;
                ws.Cell(2, 1).Style.Font.Italic = true;
                ws.Range(2, 1, 2, 6).Merge();

                int startRow = 4;
                ws.Cell(startRow, 1).Value = "STT";
                ws.Cell(startRow, 2).Value = "Ngày điểm danh";
                ws.Cell(startRow, 3).Value = "Khóa học";
                ws.Cell(startRow, 4).Value = "Email / Họ tên học viên";
                ws.Cell(startRow, 5).Value = "Trạng thái";
                ws.Cell(startRow, 6).Value = "Ghi chú";

                var headerRange = ws.Range(startRow, 1, startRow, 6);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E293B");
                headerRange.Style.Font.FontColor = XLColor.White;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                int row = startRow + 1;
                int stt = 1;
                foreach (var rec in records)
                {
                    ws.Cell(row, 1).Value = stt++;
                    ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, 2).Value = rec.AttendanceDate.ToString("dd/MM/yyyy");
                    ws.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, 3).Value = rec.Topic?.Title ?? "";
                    ws.Cell(row, 4).Value = $"{rec.Student?.FullName} ({rec.Student?.Email})";

                    var statusCell = ws.Cell(row, 5);
                    statusCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    statusCell.Style.Font.Bold = true;

                    switch (rec.Status)
                    {
                        case "PRESENT":
                            statusCell.Value = "Đi học";
                            statusCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#DCFCE7");
                            statusCell.Style.Font.FontColor = XLColor.FromHtml("#15803D");
                            break;
                        case "LATE":
                            statusCell.Value = "Đi muộn";
                            statusCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#FEF3C7");
                            statusCell.Style.Font.FontColor = XLColor.FromHtml("#B45309");
                            break;
                        case "EXCUSED":
                            statusCell.Value = "Có phép";
                            statusCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#DBEAFE");
                            statusCell.Style.Font.FontColor = XLColor.FromHtml("#1D4ED8");
                            break;
                        default:
                            statusCell.Value = "Vắng mặt";
                            statusCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#FEE2E2");
                            statusCell.Style.Font.FontColor = XLColor.FromHtml("#B91C1C");
                            break;
                    }

                    ws.Cell(row, 6).Value = rec.Remarks ?? "";
                    row++;
                }

                ws.Columns().AdjustToContents();
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> GenerateAttendanceExcelTemplateAsync(int topicId, DateOnly date)
        {
            var topic = await _context.LearningTopics.FindAsync(topicId);
            var topicName = topic?.Title ?? $"Topic_{topicId}";
            var list = await GetAttendanceListAsync(topicId, date);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Form_DiemDanh");

            ws.Cell(1, 1).Value = $"MẪU NHẬP ĐIỂM DANH - {topicName.ToUpper()} ({date:dd/MM/yyyy})";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 13;
            ws.Range(1, 1, 1, 4).Merge();

            ws.Cell(2, 1).Value = "Hướng dẫn: Cột 'Trạng thái' chấp nhận các giá trị: Đi học (PRESENT), Đi muộn (LATE), Có phép (EXCUSED), Vắng mặt (ABSENT). Không thay đổi cột Email học viên.";
            ws.Cell(2, 1).Style.Font.Italic = true;
            ws.Cell(2, 1).Style.Font.FontColor = XLColor.FromHtml("#475569");
            ws.Range(2, 1, 2, 4).Merge();

            int startRow = 4;
            ws.Cell(startRow, 1).Value = "Email học viên";
            ws.Cell(startRow, 2).Value = "Họ và tên học viên";
            ws.Cell(startRow, 3).Value = "Trạng thái";
            ws.Cell(startRow, 4).Value = "Ghi chú";

            var headerRange = ws.Range(startRow, 1, startRow, 4);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#2563EB");
            headerRange.Style.Font.FontColor = XLColor.White;

            int row = startRow + 1;
            foreach (var item in list)
            {
                ws.Cell(row, 1).Value = item.StudentEmail;
                ws.Cell(row, 2).Value = item.StudentName;
                ws.Cell(row, 3).Value = item.Status switch
                {
                    "LATE" => "Đi muộn",
                    "EXCUSED" => "Có phép",
                    "ABSENT" => "Vắng mặt",
                    _ => "Đi học"
                };
                ws.Cell(row, 4).Value = item.Remarks ?? "";
                row++;
            }

            ws.Columns().AdjustToContents();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<(int SuccessCount, List<string> Errors)> ImportAttendanceFromExcelAsync(int topicId, DateOnly date, Stream fileStream)
        {
            var errors = new List<string>();
            var attendancesToSave = new List<StudentAttendanceViewModel>();

            using var workbook = new XLWorkbook(fileStream);
            var worksheet = workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
            {
                errors.Add("File Excel không chứa bất kỳ trang tính nào.");
                return (0, errors);
            }

            int headerRowIndex = 1;
            for (int r = 1; r <= 10; r++)
            {
                var cellVal = worksheet.Cell(r, 1).GetString().Trim().ToLower();
                if (cellVal.Contains("email"))
                {
                    headerRowIndex = r;
                    break;
                }
            }

            var usedRows = worksheet.RowsUsed().Where(r => r.RowNumber() > headerRowIndex).ToList();
            if (!usedRows.Any())
            {
                errors.Add("Không tìm thấy dữ liệu học viên trong file Excel.");
                return (0, errors);
            }

            var activeStudents = await _context.Users
                .Where(u => u.Role.RoleCode == "STUDENT" && u.Status == "ACTIVE")
                .ToDictionaryAsync(u => u.Email.Trim().ToLower(), u => u);

            foreach (var row in usedRows)
            {
                int rowNum = row.RowNumber();
                string email = row.Cell(1).GetString().Trim();
                string statusRaw = row.Cell(3).GetString().Trim();
                string remarks = row.Cell(4).GetString().Trim();

                if (string.IsNullOrWhiteSpace(email))
                {
                    continue;
                }

                if (!activeStudents.TryGetValue(email.ToLower(), out var student))
                {
                    errors.Add($"Dòng {rowNum}: Không tìm thấy học viên với email '{email}'.");
                    continue;
                }

                string statusNormalized = statusRaw.ToUpper();
                string status = "PRESENT";

                if (statusNormalized.Contains("PRESENT") || statusNormalized.Contains("ĐI HỌC") || statusNormalized.Contains("CÓ MẶT") || statusNormalized == "X")
                {
                    status = "PRESENT";
                }
                else if (statusNormalized.Contains("LATE") || statusNormalized.Contains("MUỘN") || statusNormalized.Contains("TRỄ"))
                {
                    status = "LATE";
                }
                else if (statusNormalized.Contains("EXCUSED") || statusNormalized.Contains("PHÉP") || statusNormalized.Contains("XIN PHÉP"))
                {
                    status = "EXCUSED";
                }
                else if (statusNormalized.Contains("ABSENT") || statusNormalized.Contains("VẮNG") || statusNormalized.Contains("NGHỈ"))
                {
                    status = "ABSENT";
                }
                else if (!string.IsNullOrWhiteSpace(statusRaw))
                {
                    errors.Add($"Dòng {rowNum} ({email}): Trạng thái '{statusRaw}' không hợp lệ.");
                    continue;
                }

                attendancesToSave.Add(new StudentAttendanceViewModel
                {
                    StudentId = student.Id,
                    StudentName = student.FullName,
                    StudentEmail = student.Email,
                    Status = status,
                    Remarks = string.IsNullOrWhiteSpace(remarks) ? null : remarks
                });
            }

            if (attendancesToSave.Any())
            {
                await SaveAttendanceAsync(topicId, date, attendancesToSave);
            }

            return (attendancesToSave.Count, errors);
        }

        public async Task<bool> UpdateAttendanceRecordAsync(int id, string status, string? remarks)
        {
            var record = await _context.Attendances.FindAsync(id);
            if (record == null)
            {
                return false;
            }

            record.Status = status;
            record.Remarks = remarks;
            record.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
