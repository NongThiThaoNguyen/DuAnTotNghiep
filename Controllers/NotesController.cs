using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DuAnTotNghiep.Models.ViewModels.Student;
using DuAnTotNghiep.Services.Interfaces;

namespace DuAnTotNghiep.Controllers
{
    [Authorize]
    public class NotesController : Controller
    {
        private readonly INotesService _notesService;

        public NotesController(INotesService notesService)
        {
            _notesService = notesService;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null && int.TryParse(claim.Value, out int userId))
            {
                return userId;
            }
            return 0;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");

            var notes = await _notesService.GetNotesAsync(userId);
            return View(notes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] NoteCreateUpdateViewModel model)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _notesService.CreateNoteAsync(userId, model);
            if (result)
            {
                return Ok(new { success = true, message = "Đã lưu ghi chú thành công." });
            }

            return StatusCode(500, new { success = false, message = "Lỗi khi lưu ghi chú." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([FromBody] NoteCreateUpdateViewModel model)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _notesService.UpdateNoteAsync(userId, model);
            if (result)
            {
                return Ok(new { success = true, message = "Đã cập nhật ghi chú thành công." });
            }

            return StatusCode(500, new { success = false, message = "Lỗi khi cập nhật ghi chú." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var result = await _notesService.DeleteNoteAsync(id, userId);
            if (result)
            {
                return Ok(new { success = true, message = "Đã xóa ghi chú thành công." });
            }

            return StatusCode(500, new { success = false, message = "Lỗi khi xóa ghi chú." });
        }
    }
}
