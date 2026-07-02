using DuAnTotNghiep.Areas.Admin.ViewModels;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Services;

public class AdminTeacherScheduleService : IAdminTeacherScheduleService
{
    private readonly ApplicationDbContext _context;

    public AdminTeacherScheduleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminTeacherScheduleListViewModel> GetSchedulesAsync(AdminTeacherScheduleFilterViewModel filter)
    {
        NormalizeFilter(filter);

        var query = _context.Schedules
            .AsNoTracking()
            .Include(s => s.Teacher)
                .ThenInclude(t => t.Role)
            .Include(s => s.Topic)
            .Where(s => s.Teacher.Role.RoleCode == "TEACHER");

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.Trim().ToLower();
            query = query.Where(s =>
                s.Title.ToLower().Contains(keyword)
                || (s.Description != null && s.Description.ToLower().Contains(keyword))
                || (s.Classroom != null && s.Classroom.ToLower().Contains(keyword))
                || s.Teacher.FullName.ToLower().Contains(keyword)
                || s.Teacher.Email.ToLower().Contains(keyword)
                || (s.Topic != null && s.Topic.Title.ToLower().Contains(keyword)));
        }

        if (filter.TeacherId.HasValue)
        {
            query = query.Where(s => s.TeacherId == filter.TeacherId.Value);
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(s => s.StartTime >= filter.FromDate.Value.Date);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(s => s.StartTime < filter.ToDate.Value.Date.AddDays(1));
        }

        var totalItems = await query.CountAsync();
        var items = await ApplySort(query, filter)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(s => new AdminTeacherScheduleListItemViewModel
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                TeacherId = s.TeacherId,
                TeacherName = s.Teacher.FullName,
                TeacherEmail = s.Teacher.Email,
                TopicName = s.Topic != null ? s.Topic.Title : "Không đính kèm",
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                Classroom = s.Classroom
            })
            .ToListAsync();

        return new AdminTeacherScheduleListViewModel
        {
            Items = items,
            Filter = filter,
            AvailableTeachers = await GetTeacherOptionsAsync(filter.TeacherId),
            AvailableTopics = await GetTopicOptionsAsync(null),
            AiRequest = new AdminTeacherScheduleAiRequestViewModel
            {
                TeacherId = filter.TeacherId,
                FromDate = filter.FromDate ?? DateTime.Today,
                ToDate = filter.ToDate ?? DateTime.Today.AddDays(7)
            },
            TotalItems = totalItems
        };
    }

    public async Task<AdminTeacherScheduleFormViewModel> BuildCreateModelAsync(AdminTeacherScheduleFormViewModel? model = null)
    {
        model ??= new AdminTeacherScheduleFormViewModel
        {
            StartTime = DateTime.Today.AddHours(8),
            EndTime = DateTime.Today.AddHours(9)
        };

        await PopulateOptionsAsync(model);
        return model;
    }

    public async Task<AdminTeacherScheduleFormViewModel?> BuildEditModelAsync(int id)
    {
        var schedule = await _context.Schedules
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (schedule == null)
        {
            return null;
        }

        var model = new AdminTeacherScheduleFormViewModel
        {
            Id = schedule.Id,
            TeacherId = schedule.TeacherId,
            TopicId = schedule.TopicId,
            Title = schedule.Title,
            Description = schedule.Description,
            StartTime = schedule.StartTime,
            EndTime = schedule.EndTime,
            Classroom = schedule.Classroom
        };

        await PopulateOptionsAsync(model);
        return model;
    }

    public async Task<int> CreateScheduleAsync(AdminTeacherScheduleFormViewModel model)
    {
        await ValidateScheduleAsync(model);

        var schedule = new Schedule
        {
            TeacherId = model.TeacherId,
            TopicId = model.TopicId,
            Title = model.Title.Trim(),
            Description = model.Description,
            StartTime = model.StartTime,
            EndTime = model.EndTime,
            Classroom = model.Classroom,
            CreatedAt = DateTime.UtcNow
        };

        _context.Schedules.Add(schedule);
        await _context.SaveChangesAsync();
        return schedule.Id;
    }

    public async Task<bool> UpdateScheduleAsync(AdminTeacherScheduleFormViewModel model)
    {
        var schedule = await _context.Schedules.FirstOrDefaultAsync(s => s.Id == model.Id);
        if (schedule == null)
        {
            return false;
        }

        await ValidateScheduleAsync(model);

        schedule.TeacherId = model.TeacherId;
        schedule.TopicId = model.TopicId;
        schedule.Title = model.Title.Trim();
        schedule.Description = model.Description;
        schedule.StartTime = model.StartTime;
        schedule.EndTime = model.EndTime;
        schedule.Classroom = model.Classroom;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteScheduleAsync(int id)
    {
        var schedule = await _context.Schedules.FirstOrDefaultAsync(s => s.Id == id);
        if (schedule == null)
        {
            return false;
        }

        _context.Schedules.Remove(schedule);
        await _context.SaveChangesAsync();
        return true;
    }

    private static void NormalizeFilter(AdminTeacherScheduleFilterViewModel filter)
    {
        filter.Page = Math.Max(1, filter.Page);
        filter.PageSize = filter.PageSize is < 1 or > 100 ? 10 : filter.PageSize;
        filter.SortBy = string.IsNullOrWhiteSpace(filter.SortBy) ? "startTime" : filter.SortBy;
        filter.SortDirection = string.Equals(filter.SortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";
    }

    private static IOrderedQueryable<Schedule> ApplySort(IQueryable<Schedule> query, AdminTeacherScheduleFilterViewModel filter)
    {
        var descending = string.Equals(filter.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return filter.SortBy.Trim().ToLowerInvariant() switch
        {
            "teacher" => descending
                ? query.OrderByDescending(s => s.Teacher.FullName).ThenBy(s => s.StartTime).ThenBy(s => s.Id)
                : query.OrderBy(s => s.Teacher.FullName).ThenBy(s => s.StartTime).ThenBy(s => s.Id),
            "topic" => descending
                ? query.OrderByDescending(s => s.Topic != null ? s.Topic.Title : string.Empty).ThenBy(s => s.StartTime).ThenBy(s => s.Id)
                : query.OrderBy(s => s.Topic != null ? s.Topic.Title : string.Empty).ThenBy(s => s.StartTime).ThenBy(s => s.Id),
            "endtime" => descending
                ? query.OrderByDescending(s => s.EndTime).ThenBy(s => s.Id)
                : query.OrderBy(s => s.EndTime).ThenBy(s => s.Id),
            "classroom" => descending
                ? query.OrderByDescending(s => s.Classroom).ThenBy(s => s.StartTime).ThenBy(s => s.Id)
                : query.OrderBy(s => s.Classroom).ThenBy(s => s.StartTime).ThenBy(s => s.Id),
            _ => descending
                ? query.OrderByDescending(s => s.StartTime).ThenBy(s => s.Id)
                : query.OrderBy(s => s.StartTime).ThenBy(s => s.Id)
        };
    }

    private async Task ValidateScheduleAsync(AdminTeacherScheduleFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Title))
        {
            throw new InvalidOperationException("Vui lòng nhập tiêu đề lịch dạy.");
        }

        if (model.EndTime <= model.StartTime)
        {
            throw new InvalidOperationException("Thời gian kết thúc phải sau thời gian bắt đầu.");
        }

        var isActiveTeacher = await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == model.TeacherId && u.Status == "ACTIVE" && u.Role.RoleCode == "TEACHER");

        if (!isActiveTeacher)
        {
            throw new InvalidOperationException("Giáo viên được chọn không hợp lệ hoặc đang bị khóa.");
        }

        if (model.TopicId.HasValue)
        {
            var topicExists = await _context.LearningTopics
                .AsNoTracking()
                .AnyAsync(t => t.Id == model.TopicId.Value && t.Status == "ACTIVE");

            if (!topicExists)
            {
                throw new InvalidOperationException("Chủ đề học tập được chọn không hợp lệ.");
            }
        }
    }

    private async Task PopulateOptionsAsync(AdminTeacherScheduleFormViewModel model)
    {
        model.AvailableTeachers = await GetTeacherOptionsAsync(model.TeacherId);
        model.AvailableTopics = await GetTopicOptionsAsync(model.TopicId);
    }

    private async Task<List<SelectListItem>> GetTopicOptionsAsync(int? selectedTopicId)
    {
        return await _context.LearningTopics
            .AsNoTracking()
            .Where(t => t.Status == "ACTIVE")
            .OrderBy(t => t.Title)
            .Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = t.Title,
                Selected = selectedTopicId.HasValue && t.Id == selectedTopicId.Value
            })
            .ToListAsync();
    }

    private async Task<List<SelectListItem>> GetTeacherOptionsAsync(int? selectedTeacherId)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.Status == "ACTIVE" && u.Role.RoleCode == "TEACHER")
            .OrderBy(u => u.FullName)
            .Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = $"{u.FullName} ({u.Email})",
                Selected = selectedTeacherId.HasValue && u.Id == selectedTeacherId.Value
            })
            .ToListAsync();
    }
}
