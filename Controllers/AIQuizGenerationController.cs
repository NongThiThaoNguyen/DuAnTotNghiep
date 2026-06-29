using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.DTOs;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services.AI;
using DuAnTotNghiep.Services;
using DuAnTotNghiep.Services.Mapping;
using DuAnTotNghiep.Services.Validators;
using DuAnTotNghiep.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DuAnTotNghiep.Controllers
{
    [Authorize]
    public class AIQuizGenerationController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly AIQuizGenerationService _genService;
        private readonly IPromptTemplateService _promptService;
        private readonly ITaxonomyService _taxonomyService;

        public AIQuizGenerationController(ApplicationDbContext db, AIQuizGenerationService genService, IPromptTemplateService promptService, ITaxonomyService taxonomyService)
        {
            _db = db;
            _genService = genService;
            _promptService = promptService;
            _taxonomyService = taxonomyService;
        }

        [HttpGet]
        public IActionResult Create()
        {
            var vm = new GenerateQuizRequestViewModel();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(GenerateQuizRequestViewModel vm)
        {
            if (!ModelState.IsValid) return View("Create", vm);

            // service-level validation: topic belongs to skill and is active
            var validation = await _taxonomyService.ValidateTopicBelongsToSkillAsync(vm.TopicId, vm.SkillId);
            if (!validation.IsValid)
            {
                ModelState.AddModelError(string.Empty, validation.ErrorMessage ?? "Invalid topic selection.");
                return View("Create", vm);
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = int.Parse(userIdStr ?? "0");

            var dto = GenerationMapper.ToDto(vm, requestedBy: userId.ToString());

            // fetch active prompt template for logging and to ensure prompt exists
            var prompt = await _promptService.GetActivePromptByModuleAsync("M14_QUIZ_GENERATION");
            if (prompt == null)
            {
                ModelState.AddModelError(string.Empty, "No active prompt template configured for quiz generation. Please contact admin.");
                return View("Create", vm);
            }

            // Call generation service
            var result = await _genService.GenerateQuestionsAsync(dto);

            var batchId = Guid.NewGuid().ToString();
            if (result.Items != null && result.Items.Any())
            {
                foreach (var item in result.Items)
                {
                    var contentJson = JsonSerializer.Serialize(item);
                    var entity = new AiGeneratedContent
                    {
                        RequestedBy = userId,
                        BatchId = batchId,
                        ContentType = "QUESTION",
                        RelatedTopicId = vm.TopicId,
                        PromptText = JsonSerializer.Serialize(dto),
                        GeneratedContent = contentJson,
                        AiModel = "gpt-4o-mini",
                        ReviewStatus = "PENDING",
                        CreatedAt = DateTime.UtcNow
                    };
                    _db.AiGeneratedContents.Add(entity);
                }
                await _db.SaveChangesAsync();
            }

            if (result.ItemErrors != null && result.ItemErrors.Any())
            {
                var invalidContent = new AiGeneratedContent
                {
                    RequestedBy = userId,
                    BatchId = batchId,
                    ContentType = "QUESTION",
                    RelatedTopicId = vm.TopicId,
                    PromptText = JsonSerializer.Serialize(dto),
                    GeneratedContent = string.Join("\n", result.ItemErrors),
                    AiModel = "gpt-4o-mini",
                    ReviewStatus = "NEEDS_REVISION",
                    ReviewNote = "Some AI-generated items were invalid and require revision.",
                    CreatedAt = DateTime.UtcNow
                };
                _db.AiGeneratedContents.Add(invalidContent);
                await _db.SaveChangesAsync();
            }

            if (!result.IsSuccess && (result.Items == null || !result.Items.Any()))
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "We could not complete the AI generation request.");
                return View("Create", vm);
            }

            // Redirect to preview
            return RedirectToAction("Preview", new { batchId = batchId });
        }

        [HttpGet]
        public IActionResult Preview(string batchId)
        {
            if (string.IsNullOrWhiteSpace(batchId)) return NotFound();

            var items = _db.AiGeneratedContents.Where(a => a.BatchId == batchId).OrderBy(a => a.Id).ToList();
            return View(items);
        }
    }
}
