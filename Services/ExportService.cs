using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Services
{
    public class ExportService : IExportService
    {
        private readonly ApplicationDbContext _context;

        public ExportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> ExportUsersToExcelAsync()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Users");

            // Header
            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Họ và tên";
            worksheet.Cell(1, 3).Value = "Email";
            worksheet.Cell(1, 4).Value = "Phân quyền";
            worksheet.Cell(1, 5).Value = "Trạng thái";
            worksheet.Cell(1, 6).Value = "Ngày tạo";
            worksheet.Cell(1, 7).Value = "Lần đăng nhập cuối";

            var headerRow = worksheet.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

            // Data
            int row = 2;
            foreach (var user in users)
            {
                worksheet.Cell(row, 1).Value = user.Id;
                worksheet.Cell(row, 2).Value = user.FullName;
                worksheet.Cell(row, 3).Value = user.Email;
                worksheet.Cell(row, 4).Value = user.Role?.RoleName;
                worksheet.Cell(row, 5).Value = user.Status;
                worksheet.Cell(row, 6).Value = user.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss");
                worksheet.Cell(row, 7).Value = user.LastLoginAt?.ToString("dd/MM/yyyy HH:mm:ss") ?? "N/A";
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportPlacementResultsToExcelAsync()
        {
            var testAttempts = await _context.TestAttempts
                .Include(t => t.Student)
                .Include(t => t.PlacementTest)
                .Include(t => t.EstimatedLevel)
                .OrderByDescending(t => t.StartedAt)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Placement Results");

            // Header
            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Học viên";
            worksheet.Cell(1, 3).Value = "Email";
            worksheet.Cell(1, 4).Value = "Bài test";
            worksheet.Cell(1, 5).Value = "Điểm số";
            worksheet.Cell(1, 6).Value = "Level đánh giá";
            worksheet.Cell(1, 7).Value = "Ngày thi";
            worksheet.Cell(1, 8).Value = "Trạng thái";

            var headerRow = worksheet.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

            // Data
            int row = 2;
            foreach (var attempt in testAttempts)
            {
                worksheet.Cell(row, 1).Value = attempt.Id;
                worksheet.Cell(row, 2).Value = attempt.Student?.FullName;
                worksheet.Cell(row, 3).Value = attempt.Student?.Email;
                worksheet.Cell(row, 4).Value = attempt.PlacementTest?.Title;
                worksheet.Cell(row, 5).Value = attempt.TotalScore;
                worksheet.Cell(row, 6).Value = attempt.EstimatedLevel?.Name ?? "Chưa đánh giá";
                worksheet.Cell(row, 7).Value = attempt.StartedAt.ToString("dd/MM/yyyy HH:mm:ss");
                worksheet.Cell(row, 8).Value = attempt.Status;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportAuditLogsToExcelAsync(DateTime? from, DateTime? to)
        {
            var query = _context.AuditLogs.Include(a => a.User).AsQueryable();

            if (from.HasValue)
            {
                query = query.Where(a => a.CreatedAt >= from.Value);
            }
            if (to.HasValue)
            {
                query = query.Where(a => a.CreatedAt <= to.Value);
            }

            var logs = await query.OrderByDescending(a => a.CreatedAt).ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Audit Logs");

            // Header
            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Hành động";
            worksheet.Cell(1, 3).Value = "Thực thể";
            worksheet.Cell(1, 4).Value = "Mô tả";
            worksheet.Cell(1, 5).Value = "Người dùng (Email)";
            worksheet.Cell(1, 6).Value = "IP Address";
            worksheet.Cell(1, 7).Value = "Thời gian";

            var headerRow = worksheet.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

            // Data
            int row = 2;
            foreach (var log in logs)
            {
                worksheet.Cell(row, 1).Value = log.Id;
                worksheet.Cell(row, 2).Value = log.Action;
                worksheet.Cell(row, 3).Value = log.EntityName ?? "";
                worksheet.Cell(row, 4).Value = log.Action ?? "";
                worksheet.Cell(row, 5).Value = log.User?.Email ?? "System/Guest";
                worksheet.Cell(row, 6).Value = log.IpAddress;
                worksheet.Cell(row, 7).Value = log.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss");
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportAiUsageLogsToExcelAsync()
        {
            var logs = await _context.AiUsageLogs
                .Include(a => a.User)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("AI Usage Logs");

            // Header
            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Tính năng AI";
            worksheet.Cell(1, 3).Value = "Người dùng (Email)";
            worksheet.Cell(1, 4).Value = "Tokens sử dụng";
            worksheet.Cell(1, 5).Value = "Trạng thái";
            worksheet.Cell(1, 6).Value = "Model";
            worksheet.Cell(1, 7).Value = "Thời gian";

            var headerRow = worksheet.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

            // Data
            int row = 2;
            foreach (var log in logs)
            {
                worksheet.Cell(row, 1).Value = log.Id;
                worksheet.Cell(row, 2).Value = log.ModuleCode ?? "";
                worksheet.Cell(row, 3).Value = log.User?.Email ?? "Unknown";
                worksheet.Cell(row, 4).Value = (log.InputTokens ?? 0) + (log.OutputTokens ?? 0);
                worksheet.Cell(row, 5).Value = log.RequestStatus ?? "";
                worksheet.Cell(row, 6).Value = log.AiModel ?? "";
                worksheet.Cell(row, 7).Value = log.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss");
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
