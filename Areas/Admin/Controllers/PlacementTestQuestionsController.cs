using DuAnTotNghiep.Models.DTOs.PlacementTestQuestion;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN")]
    [Route("Admin/PlacementTestQuestions")]
    public class PlacementTestQuestionsController : Controller
    {
        private readonly IPlacementTestQuestionService _questionService;
        private readonly IPlacementTestSectionService _sectionService;
        private readonly IPlacementTestManagementService _testService;

        public PlacementTestQuestionsController(
            IPlacementTestQuestionService questionService,
            IPlacementTestSectionService sectionService,
            IPlacementTestManagementService testService)
        {
            _questionService = questionService;
            _sectionService = sectionService;
            _testService = testService;
        }

        [HttpGet("GetBySection/{sectionId}")]
        public async Task<IActionResult> GetBySection(int sectionId)
        {
            try
            {
                var questions = await _questionService.GetSectionQuestionsAsync(sectionId);
                return Ok(new { success = true, data = questions });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("GetAvailable")]
        public async Task<IActionResult> GetAvailable([FromQuery] QuestionFilterDto filter)
        {
            try
            {
                var questions = await _questionService.SearchAvailableQuestionsAsync(filter);
                return Ok(new { success = true, data = questions });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("Attach")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Attach([FromBody] AttachQuestionDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });

            try
            {
                await _questionService.AttachQuestionToSectionAsync(dto);
                return Ok(new { success = true, message = "Thêm câu hỏi thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("Remove/{sectionId}/{questionId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int sectionId, int questionId)
        {
            try
            {
                await _questionService.RemoveQuestionFromSectionAsync(sectionId, questionId);
                return Ok(new { success = true, message = "Đã gỡ câu hỏi khỏi Section." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("Reorder/{sectionId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reorder(int sectionId, [FromBody] List<QuestionOrderDto> orders)
        {
            try
            {
                await _questionService.ReorderQuestionsAsync(sectionId, orders);
                return Ok(new { success = true, message = "Cập nhật thứ tự câu hỏi thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("DownloadQuestionTemplate")]
        public async Task<IActionResult> DownloadQuestionTemplate()
        {
            var bytes = await _questionService.GenerateQuestionExcelTemplateAsync();
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Mau_Import_Cau_Hoi_Placement_Test.xlsx");
        }

        [HttpPost("PreviewExcel/{sectionId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PreviewExcel(int sectionId, Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, message = "Vui lòng chọn file Excel hợp lệ (.xlsx/.xls)." });

            var ext = System.IO.Path.GetExtension(file.FileName).ToLower();
            if (ext != ".xlsx" && ext != ".xls")
                return BadRequest(new { success = false, message = "Chỉ chấp nhận file định dạng Excel (.xlsx, .xls)." });

            try
            {
                using var stream = file.OpenReadStream();
                var previewResult = await _questionService.PreviewQuestionsExcelAsync(sectionId, stream);
                return Ok(new { success = true, data = previewResult });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("ImportExcel/{sectionId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportExcel(int sectionId, [FromBody] List<QuestionExcelRowDto> rows)
        {
            if (rows == null || !rows.Any())
                return BadRequest(new { success = false, message = "Danh sách câu hỏi cần import trống." });

            try
            {
                var count = await _questionService.ImportQuestionsFromExcelAsync(sectionId, rows);
                return Ok(new { success = true, count = count, message = $"Đã import thành công {count} câu hỏi vào Section." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("CreateAndAttach")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAndAttach([FromBody] CreateAndAttachQuestionDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });

            try
            {
                await _questionService.CreateAndAttachQuestionAsync(dto);
                return Ok(new { success = true, message = "Tạo và thêm câu hỏi mới vào Section thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
