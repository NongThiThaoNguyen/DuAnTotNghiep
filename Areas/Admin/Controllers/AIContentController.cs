using System;
using System.Security.Claims;
using System.Threading.Tasks;
using DuAnTotNghiep.Services;
using DuAnTotNghiep.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DuAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN,REVIEWER")]
    public class AIContentController : Controller
    {
        private readonly IAIContentReviewService _reviewService;
        private readonly IPublishingService _publishingService;

        public AIContentController(IAIContentReviewService reviewService, IPublishingService publishingService)
        {
            _reviewService = reviewService;
            _publishingService = publishingService;
        }

        private int GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(userIdStr ?? "0");
        }

        public async Task<IActionResult> Index(string? contentTypeFilter = null, int? topicIdFilter = null, int? requestedByFilter = null, int page = 1)
        {
            var result = await _reviewService.GetPendingContentAsync(
                contentTypeFilter: contentTypeFilter,
                topicIdFilter: topicIdFilter,
                requestedByFilter: requestedByFilter,
                page: page,
                pageSize: 20);

            var pendingByType = await _reviewService.GetPendingCountByTypeAsync();

            ViewBag.PendingByType = pendingByType;
            ViewBag.ContentTypeFilter = contentTypeFilter;
            ViewBag.TopicIdFilter = topicIdFilter;
            ViewBag.RequestedByFilter = requestedByFilter;

            return View(result);
        }

        public async Task<IActionResult> Details(int id)
        {
            var item = await _reviewService.GetContentDetailAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, bool copyrightCheck, string? plagiarismRisk, string? reviewNote, string? editedQuestionText, string? editedExplanation)
        {
            try
            {
                var reviewerId = GetCurrentUserId();
                await _reviewService.ApproveAsync(id, reviewerId, copyrightCheck, plagiarismRisk, reviewNote, editedQuestionText, editedExplanation);
                TempData["SuccessMessage"] = "Content approved successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var item = await _reviewService.GetContentDetailAsync(id);
                return View("Details", item);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string? reviewNote)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reviewNote))
                {
                    ModelState.AddModelError(nameof(reviewNote), "Review note is required for rejection.");
                    var item = await _reviewService.GetContentDetailAsync(id);
                    return View("Details", item);
                }

                var reviewerId = GetCurrentUserId();
                await _reviewService.RejectAsync(id, reviewerId, reviewNote);
                TempData["SuccessMessage"] = "Content rejected.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var item = await _reviewService.GetContentDetailAsync(id);
                return View("Details", item);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestRevision(int id, string? reviewNote)
        {
            try
            {
                var reviewerId = GetCurrentUserId();
                await _reviewService.RequestRevisionAsync(id, reviewerId, reviewNote);
                TempData["SuccessMessage"] = "Revision requested.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var item = await _reviewService.GetContentDetailAsync(id);
                return View("Details", item);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int id)
        {
            try
            {
                var publisherId = GetCurrentUserId();
                var result = await _publishingService.PublishToQuestionBankAsync(id, publisherId);

                var vm = new PublishResultViewModel
                {
                    IsSuccess = result.IsSuccess,
                    Message = result.Message,
                    PublishedQuestionId = result.PublishedQuestionId,
                    CreatedQuizId = result.CreatedQuizId,
                    AiContentId = id,
                    ErrorMessage = result.ErrorMessage,
                    ViewQuestionLink = result.PublishedQuestionId.HasValue ? $"/QuestionBank/Details/{result.PublishedQuestionId}" : null
                };

                TempData["PublishResult"] = vm.IsSuccess ? "success" : "error";
                TempData["PublishMessage"] = vm.Message ?? vm.ErrorMessage;

                return RedirectToAction("PublishResult", vm);
            }
            catch (Exception ex)
            {
                TempData["PublishResult"] = "error";
                TempData["PublishMessage"] = $"Unexpected error: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> PublishResult(PublishResultViewModel vm)
        {
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PublishBatch(string? batchId, bool createQuizDraft = false, string? quizTitle = null)
        {
            try
            {
                var publisherId = GetCurrentUserId();
                var result = await _publishingService.PublishBatchAsync(batchId, publisherId, createQuizDraft, quizTitle);

                var vm = new PublishBatchResultViewModel
                {
                    IsSuccess = result.IsSuccess,
                    PublishedCount = result.PublishedCount,
                    FailureCount = result.FailureCount,
                    CreatedQuizId = result.CreatedQuizId,
                    Message = result.Message,
                    ErrorMessage = result.ErrorMessage,
                    BatchId = batchId ?? "ALL_APPROVED"
                };

                TempData["PublishBatchResult"] = vm.IsSuccess ? "success" : "warning";
                TempData["PublishBatchMessage"] = vm.Message ?? vm.ErrorMessage;

                return RedirectToAction("PublishBatchResult", vm);
            }
            catch (Exception ex)
            {
                TempData["PublishBatchResult"] = "error";
                TempData["PublishBatchMessage"] = $"Batch publish failed: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> PublishBatchResult(PublishBatchResultViewModel vm)
        {
            return View(vm);
        }
    }
}
