using DuAnTotNghiep.DTOs.Topic;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.ViewModels.Admin.Topics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Teacher")]
    public class LearningTopicsController : Controller
    {
        private readonly ILearningTopicService _topicService;
        private readonly IEnglishSkillService _skillService;
        private readonly IEnglishProficiencyLevelService _levelService;

        public LearningTopicsController(
            ILearningTopicService topicService,
            IEnglishSkillService skillService,
            IEnglishProficiencyLevelService levelService)
        {
            _topicService = topicService;
            _skillService = skillService;
            _levelService = levelService;
        }

        public async Task<IActionResult> Index(string? keyword, int? skillId, int? levelId, string? difficulty, string? status, int page = 1, int pageSize = 10)
        {
            var request = new TopicSearchRequest
            {
                Keyword = keyword,
                SkillId = skillId,
                LevelId = levelId,
                DifficultyLevel = difficulty,
                Status = status,
                Page = page,
                PageSize = pageSize
            };

            var pagedResult = await _topicService.SearchTopicsAsync(request);
            
            ViewBag.Keyword = keyword;
            ViewBag.SkillId = skillId;
            ViewBag.LevelId = levelId;
            ViewBag.Difficulty = difficulty;
            ViewBag.Status = status;

            // Load dropdowns for filter
            var skills = await _skillService.GetListAsync();
            var levels = await _levelService.GetListAsync();
            ViewBag.Skills = skills.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList();
            ViewBag.Levels = levels.Select(l => new SelectListItem { Value = l.Id.ToString(), Text = l.Name }).ToList();

            var listViewModel = pagedResult.Items.Select(t => new TopicListViewModel
            {
                Id = t.Id,
                TopicCode = t.TopicCode,
                Name = t.Name,
                SkillName = t.SkillName,
                LevelName = t.LevelName,
                ParentTopicName = t.ParentTopicName ?? string.Empty,
                Difficulty = t.Difficulty,
                Status = t.Status,
                ObjectiveCount = t.ObjectiveCount
            }).ToList();

            ViewBag.TotalPages = (int)Math.Ceiling(pagedResult.TotalCount / (double)pageSize);
            ViewBag.CurrentPage = page;

            return View(listViewModel);
        }

        public async Task<IActionResult> Tree()
        {
            var tree = await _topicService.GetTopicTreeAsync();
            return View(tree);
        }

        public async Task<IActionResult> Details(int id)
        {
            var detailDto = await _topicService.GetDetailAsync(id);
            if (detailDto == null) return NotFound();
            var viewModel = new TopicDetailDto
            {
                Id = detailDto.Id,
                TopicCode = detailDto.TopicCode,
                Name = detailDto.Name,
                Description = detailDto.Description,
                SkillName = detailDto.SkillName,
                LevelName = detailDto.LevelName,
                ParentTopicName = detailDto.ParentTopicName,
                Difficulty = detailDto.Difficulty,
                Status = detailDto.Status,
                IsActive = detailDto.IsActive,
                ObjectiveCount = detailDto.ObjectiveCount
            };
            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var model = new CreateTopicViewModel();
            await PopulateDropdowns(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateTopicViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            try
            {
                var dto = new CreateTopicDto
                {
                    TopicCode = model.TopicCode,
                    Name = model.Name,
                    Description = model.Description,
                    SkillId = model.SkillId,
                    LevelId = model.LevelId,
                    ParentTopicId = model.ParentTopicId,
                    DifficultyLevel = model.Difficulty,
                    OrderIndex = model.OrderIndex
                };

                await _topicService.CreateTopicAsync(dto);
                TempData["SuccessMessage"] = "Topic created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                await PopulateDropdowns(model);
                return View(model);
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var detailDto = await _topicService.GetDetailAsync(id);
            if (detailDto == null) return NotFound();

            var model = new EditTopicViewModel
            {
                Id = detailDto.Id,
                TopicCode = detailDto.TopicCode,
                Name = detailDto.Name,
                Description = detailDto.Description ?? string.Empty,
                SkillId = detailDto.SkillId,
                LevelId = detailDto.LevelId,
                ParentTopicId = detailDto.ParentTopicId,
                Difficulty = detailDto.DifficultyLevel,
                OrderIndex = detailDto.OrderIndex,
                Status = detailDto.Status,
                PrerequisiteIds = detailDto.Prerequisites.Select(p => p.Id).ToList()
            };

            await PopulateDropdowns(model, id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, EditTopicViewModel model)
        {
            if (id != model.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model, id);
                return View(model);
            }

            try
            {
                var dto = new UpdateTopicDto
                {
                    Id = model.Id,
                    TopicCode = model.TopicCode,
                    Name = model.Name,
                    Description = model.Description,
                    LevelId = model.LevelId,
                    ParentTopicId = model.ParentTopicId,
                    DifficultyLevel = model.Difficulty,
                    OrderIndex = model.OrderIndex,
                    Status = model.Status
                };

                await _topicService.UpdateTopicAsync(dto);
                TempData["SuccessMessage"] = "Topic updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                await PopulateDropdowns(model, id);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Deactivate(int id)
        {
            try
            {
                await _topicService.DeactivateTopicAsync(id);
                TempData["SuccessMessage"] = "Topic deactivated successfully.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Archive(int id)
        {
            try
            {
                await _topicService.ArchiveTopicAsync(id);
                TempData["SuccessMessage"] = "Topic archived successfully.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reorder(List<int> orderedIds)
        {
            try
            {
                await _topicService.ReorderTopicsAsync(orderedIds);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private async Task PopulateDropdowns(CreateTopicViewModel model)
        {
            var skills = await _skillService.GetListAsync();
            var levels = await _levelService.GetListAsync();
            var topics = await _topicService.GetActiveTopicOptionsAsync();

            model.AvailableSkills = skills.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name });
            model.AvailableLevels = levels.Select(l => new SelectListItem { Value = l.Id.ToString(), Text = l.Name });
            model.AvailableTopics = topics.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name });
        }

        private async Task PopulateDropdowns(EditTopicViewModel model, int currentTopicId)
        {
            var skills = await _skillService.GetListAsync();
            var levels = await _levelService.GetListAsync();
            var topics = await _topicService.GetActiveTopicOptionsAsync();

            model.AvailableSkills = skills.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name });
            model.AvailableLevels = levels.Select(l => new SelectListItem { Value = l.Id.ToString(), Text = l.Name });
            // Exclude self from parent/prerequisite options
            model.AvailableTopics = topics.Where(t => t.Id != currentTopicId)
                                          .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name });
        }
    }
}
