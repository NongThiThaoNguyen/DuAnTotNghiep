using System.Text;
using DuAnTotNghiep.Areas.Admin.ViewModels;
using DuAnTotNghiep.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMIN")]
public class StudentAttendanceController : Controller
{
    private const int PageSize = 20;
    private readonly ApplicationDbContext _context;

    public StudentAttendanceController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? studentId, int? topicId, DateOnly? from, DateOnly? to, int page = 1)
    {
        page = Math.Max(page, 1);
        var query = _context.Attendances.AsNoTracking()
            .Include(a => a.Student)
            .Include(a => a.Topic)
            .AsQueryable();

        if (studentId.HasValue)
        {
            query = query.Where(a => a.StudentId == studentId.Value);
        }

        if (topicId.HasValue)
        {
            query = query.Where(a => a.TopicId == topicId.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(a => a.AttendanceDate >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(a => a.AttendanceDate <= to.Value);
        }

        var totalItems = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)PageSize));
        page = Math.Min(page, totalPages);

        var model = new StudentAttendanceListViewModel
        {
            StudentId = studentId,
            TopicId = topicId,
            From = from,
            To = to,
            CurrentPage = page,
            PageSize = PageSize,
            TotalItems = totalItems,
            Students = await GetStudentOptionsAsync(),
            Topics = await GetTopicOptionsAsync(),
            Items = await query
                .OrderByDescending(a => a.AttendanceDate)
                .ThenBy(a => a.Student.FullName)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(a => new StudentAttendanceRowViewModel
                {
                    Id = a.Id,
                    StudentId = a.StudentId,
                    StudentName = a.Student.FullName,
                    StudentEmail = a.Student.Email,
                    TopicId = a.TopicId,
                    TopicTitle = a.Topic.Title,
                    AttendanceDate = a.AttendanceDate,
                    Status = a.Status,
                    Remarks = a.Remarks
                })
                .ToListAsync()
        };

        return View(model);
    }

    public Task<IActionResult> ByTopic(int topicId)
    {
        return Index(null, topicId, null, null);
    }

    public async Task<IActionResult> Export(int studentId)
    {
        var rows = await _context.Attendances.AsNoTracking()
            .Include(a => a.Student)
            .Include(a => a.Topic)
            .Where(a => a.StudentId == studentId)
            .OrderByDescending(a => a.AttendanceDate)
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("Student,Email,Topic,Date,Status,Remarks");
        foreach (var row in rows)
        {
            csv.AppendLine(string.Join(',',
                Csv(row.Student.FullName),
                Csv(row.Student.Email),
                Csv(row.Topic.Title),
                row.AttendanceDate.ToString("yyyy-MM-dd"),
                Csv(row.Status),
                Csv(row.Remarks ?? string.Empty)));
        }

        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"student-attendance-{studentId}.csv");
    }

    private Task<List<AdminOptionViewModel>> GetStudentOptionsAsync()
    {
        return _context.Users.AsNoTracking()
            .Where(u => u.Role.RoleCode == "STUDENT")
            .OrderBy(u => u.FullName)
            .Select(u => new AdminOptionViewModel { Id = u.Id, Text = u.FullName })
            .ToListAsync();
    }

    private Task<List<AdminOptionViewModel>> GetTopicOptionsAsync()
    {
        return _context.LearningTopics.AsNoTracking()
            .OrderBy(t => t.Title)
            .Select(t => new AdminOptionViewModel { Id = t.Id, Text = t.Title })
            .ToListAsync();
    }

    private static string Csv(string value)
    {
        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
