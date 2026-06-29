using System.Security.Claims;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Areas.Admin.Controllers;

/// <summary>
/// Manages reusable learning path templates for administrators.
/// </summary>
[Area("Admin")]
[Authorize(Roles = "ADMIN")]
public class PathTemplatesController : Controller
{
    private const string DraftStatus = "DRAFT";
    private const string PublishedStatus = "PUBLISHED";
    private const string ArchivedStatus = "ARCHIVED";

    private readonly ApplicationDbContext _context;

    public PathTemplatesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var templates = await _context.LearningPathTemplates
            .AsNoTracking()
            .Include(template => template.Goal)
            .Include(template => template.StartLevel)
            .Include(template => template.TargetLevel)
            .OrderByDescending(template => template.CreatedAt)
            .ToListAsync();

        return View(templates);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new LearningPathTemplate { Status = DraftStatus, CreatedAt = DateTime.UtcNow });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LearningPathTemplate template)
    {
        if (!ModelState.IsValid) return View(template);

        template.Status = string.IsNullOrWhiteSpace(template.Status) ? DraftStatus : template.Status;
        template.CreatedAt = template.CreatedAt == default ? DateTime.UtcNow : template.CreatedAt;
        template.CreatedBy = GetCurrentAdminId();
        _context.LearningPathTemplates.Add(template);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var template = await GetTemplateWithNodesAsync(id);
        return template == null ? NotFound() : View(template);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var template = await GetTemplateWithNodesAsync(id);
        return template == null ? NotFound() : View(template);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, LearningPathTemplate template)
    {
        if (id != template.Id) return NotFound();
        if (!ModelState.IsValid) return View(template);

        var existing = await GetTemplateWithNodesAsync(id);
        if (existing == null) return NotFound();
        CopyTemplateFields(template, existing);
        ReplaceTemplateNodes(existing, template.LearningPathTemplateNodes);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(int id)
    {
        var template = await GetTemplateWithNodesAsync(id);
        if (template == null) return NotFound();
        if (!template.LearningPathTemplateNodes.Any())
        {
            TempData["ErrorMessage"] = "Template cần ít nhất 1 node trước khi publish.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        template.Status = PublishedStatus;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Archive(int id)
    {
        var template = await _context.LearningPathTemplates.FindAsync(id);
        if (template == null) return NotFound();

        template.Status = ArchivedStatus;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task<LearningPathTemplate?> GetTemplateWithNodesAsync(int id)
    {
        return await _context.LearningPathTemplates
            .Include(template => template.Goal)
            .Include(template => template.StartLevel)
            .Include(template => template.TargetLevel)
            .Include(template => template.LearningPathTemplateNodes.OrderBy(node => node.OrderIndex))
            .FirstOrDefaultAsync(template => template.Id == id);
    }

    private static void CopyTemplateFields(LearningPathTemplate source, LearningPathTemplate target)
    {
        target.TemplateName = source.TemplateName;
        target.GoalId = source.GoalId;
        target.StartLevelId = source.StartLevelId;
        target.TargetLevelId = source.TargetLevelId;
        target.DurationWeeks = source.DurationWeeks;
        target.Description = source.Description;
        target.Status = source.Status;
    }

    private void ReplaceTemplateNodes(
        LearningPathTemplate template,
        IEnumerable<LearningPathTemplateNode> nodes)
    {
        _context.LearningPathTemplateNodes.RemoveRange(template.LearningPathTemplateNodes);
        foreach (var node in nodes.OrderBy(node => node.OrderIndex))
        {
            template.LearningPathTemplateNodes.Add(node);
        }
    }

    private int? GetCurrentAdminId()
    {
        var principal = ControllerContext.HttpContext?.User;
        if (principal == null) return null;

        var idValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idValue, out var id) ? id : null;
    }
}
