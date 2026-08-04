using DuAnTotNghiep.Data;
// Admin courses controller
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.ViewModels.Teacher;
using DuAnTotNghiep.Areas.Admin.ViewModels;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN")]
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEnglishSkillService _skillService;
        private readonly IEnglishProficiencyLevelService _levelService;

        public CoursesController(
            ApplicationDbContext context,
            IEnglishSkillService skillService,
            IEnglishProficiencyLevelService levelService)
        {
            _context = context;
            _skillService = skillService;
            _levelService = levelService;
        }

        // GET: Admin/Courses
        public async Task<IActionResult> Index(string? search, string? skill, string? status, int page = 1, int pageSize = 12)
        {
            var query = _context.LearningTopics
                .AsNoTracking()
                .Include(t => t.Skill)
                .Include(t => t.Level)
                .Include(t => t.OriginalLessons)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var kw = search.Trim().ToLower();
                query = query.Where(t =>
                    t.Title.ToLower().Contains(kw) ||
                    (t.Description != null && t.Description.ToLower().Contains(kw)) ||
                    (t.TopicCode != null && t.TopicCode.ToLower().Contains(kw)));
            }

            if (!string.IsNullOrWhiteSpace(skill))
            {
                query = query.Where(t =>
                    t.Skill.SkillCode.ToLower() == skill.ToLower() ||
                    t.SkillId.ToString() == skill);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(t => t.Status.ToUpper() == status.ToUpper());
            }

            var total = await query.CountAsync();
            var topics = await query
                .OrderBy(t => t.OrderIndex)
                .ThenByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = topics.Select(t => new AdminCourseListItemViewModel
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                TopicCode = t.TopicCode,
                SkillName = t.Skill?.SkillName ?? "-",
                LevelName = t.Level?.Name,
                DifficultyLevel = t.DifficultyLevel,
                LessonCount = t.OriginalLessons.Count,
                Status = t.Status,
                CreatedAt = t.CreatedAt
            }).ToList();

            var skills = await _skillService.GetListAsync();
            ViewBag.SkillOptions = skills.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Name,
                Selected = skill == s.Id.ToString() || skill == s.Code
            }).ToList();

            ViewBag.Search = search;
            ViewBag.Skill = skill;
            ViewBag.Status = status;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.Total = total;

            return View(items);
        }

        // GET: Admin/Courses/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var topic = await _context.LearningTopics
                .AsNoTracking()
                .Include(t => t.Skill)
                .Include(t => t.Level)
                .Include(t => t.OriginalLessons)
                .Include(t => t.Quizzes).ThenInclude(q => q.QuizQuestions)
                .Include(t => t.CreatedByNavigation)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (topic == null) return NotFound();

            // Count students who have studied this course
            var studentCount = await _context.StudentProgressSnapshots
                .AsNoTracking()
                .Where(s => s.TopicId == id)
                .Select(s => s.StudentId)
                .Distinct()
                .CountAsync();

            var vm = new AdminCourseDetailViewModel
            {
                Id = topic.Id,
                Title = topic.Title,
                Description = topic.Description,
                TopicCode = topic.TopicCode,
                SkillName = topic.Skill?.SkillName ?? "-",
                LevelName = topic.Level?.Name,
                DifficultyLevel = topic.DifficultyLevel,
                EstimatedMinutes = topic.EstimatedMinutes,
                Status = topic.Status,
                CreatedAt = topic.CreatedAt,
                CreatedByName = topic.CreatedByNavigation?.FullName,
                StudentCount = studentCount,
                Lessons = topic.OriginalLessons.OrderBy(l => l.Id).Select(l => new LessonSummaryViewModel
                {
                    Id = l.Id,
                    Title = l.Title,
                    OrderIndex = l.Id,
                    EstimatedMinutes = l.EstimatedMinutes,
                    Status = l.ReviewStatus
                }).ToList(),
                Quizzes = topic.Quizzes.OrderBy(q => q.Title).Select(q => new QuizSummaryViewModel
                {
                    Id = q.Id,
                    Title = q.Title,
                    QuestionCount = q.QuizQuestions.Count,
                    Status = q.Status
                }).ToList()
            };

            return View(vm);
        }

        // GET: Admin/Courses/Create
        public async Task<IActionResult> Create()
        {
            var model = new CreateCourseViewModel
            {
                SkillOptions = await _skillService.GetOptionsAsync(),
                LevelOptions = await _levelService.GetOptionsAsync()
            };
            return View(model);
        }

        // POST: Admin/Courses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCourseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.SkillOptions = await _skillService.GetOptionsAsync();
                model.LevelOptions = await _levelService.GetOptionsAsync();
                return View(model);
            }

            var adminId = GetCurrentUserId();
            var now = DateTime.UtcNow;
            var topic = new LearningTopic
            {
                Title = model.Title,
                Description = model.Description,
                SkillId = model.SkillId,
                LevelId = model.ProficiencyLevelId,
                DifficultyLevel = model.DifficultyLevel,
                EstimatedMinutes = model.EstimatedMinutes,
                Status = "ACTIVE",
                CreatedBy = adminId,
                UpdatedBy = adminId,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.LearningTopics.Add(topic);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Tạo khóa học thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Courses/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var topic = await _context.LearningTopics
                .Include(t => t.Skill)
                .Include(t => t.Level)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (topic == null) return NotFound();

            var model = new EditCourseViewModel
            {
                Id = topic.Id,
                Title = topic.Title,
                Description = topic.Description,
                TopicCode = topic.TopicCode,
                SkillId = topic.SkillId,
                ProficiencyLevelId = topic.LevelId,
                DifficultyLevel = topic.DifficultyLevel,
                EstimatedMinutes = topic.EstimatedMinutes,
                OrderIndex = topic.OrderIndex,
                IsActive = topic.Status == "ACTIVE",
                SkillOptions = await _skillService.GetOptionsAsync(),
                LevelOptions = await _levelService.GetOptionsAsync()
            };

            return View(model);
        }

        // POST: Admin/Courses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditCourseViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                model.SkillOptions = await _skillService.GetOptionsAsync();
                model.LevelOptions = await _levelService.GetOptionsAsync();
                return View(model);
            }

            var topic = await _context.LearningTopics.FindAsync(id);
            if (topic == null) return NotFound();

            topic.Title = model.Title;
            topic.Description = model.Description;
            topic.SkillId = model.SkillId;
            topic.LevelId = model.ProficiencyLevelId;
            topic.TopicCode = model.TopicCode;
            topic.DifficultyLevel = model.DifficultyLevel;
            topic.EstimatedMinutes = model.EstimatedMinutes;
            topic.OrderIndex = model.OrderIndex;
            topic.Status = model.IsActive ? "ACTIVE" : "INACTIVE";
            topic.UpdatedBy = GetCurrentUserId();
            topic.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật khóa học thành công!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Courses/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var topic = await _context.LearningTopics.FindAsync(id);
            if (topic == null) return NotFound();

            topic.Status = topic.Status == "ACTIVE" ? "INACTIVE" : "ACTIVE";
            topic.UpdatedAt = DateTime.UtcNow;
            topic.UpdatedBy = GetCurrentUserId();
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Khóa học đã được {(topic.Status == "ACTIVE" ? "kích hoạt" : "tạm dừng")}.";
            return RedirectToAction(nameof(Index));
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
        }
    }
}
