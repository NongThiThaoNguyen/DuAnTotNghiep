using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.Exceptions;
using DuAnTotNghiep.ViewModels.Profile;

namespace DuAnTotNghiep.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly IUserProfileService _profileService;

    public ProfileController(IUserProfileService profileService)
    {
        _profileService = profileService;
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

    [HttpGet("Profile")]
    public async Task<IActionResult> Index()
    {
        try
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");

            var profile = await _profileService.GetProfileAsync(userId);
            return View(profile);
        }
        catch (NotFoundException)
        {
            TempData["ErrorMessage"] = "Tài khoản của bạn không tồn tại hoặc đã bị xóa.";
            return RedirectToAction("Login", "Account");
        }
        catch (Exception ex)
        {
            // Ideally return View("Error", new ErrorViewModel { ... })
            TempData["ErrorMessage"] = "Lỗi hệ thống: " + ex.Message;
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpGet("Profile/Edit")]
    public async Task<IActionResult> Edit()
    {
        try
        {
            int userId = GetCurrentUserId();
            var profile = await _profileService.GetProfileAsync(userId);

            var model = new EditProfileViewModel
            {
                FullName = profile.FullName,
                Phone = profile.Phone,
                DateOfBirth = profile.DateOfBirth,
                Gender = profile.Gender,
                Country = profile.Country,
                Bio = profile.Bio
            };

            return View(model);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Không thể tải thông tin cập nhật hồ sơ.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("Profile/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProfileViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            int userId = GetCurrentUserId();
            await _profileService.UpdateProfileAsync(userId, model);
            
            TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (NotFoundException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi cập nhật: " + ex.Message);
            return View(model);
        }
    }

    [HttpPost("Profile/UploadAvatar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadAvatar(AvatarUploadViewModel model)
    {
        if (model.AvatarFile == null || model.AvatarFile.Length == 0)
        {
            TempData["ErrorMessage"] = "Vui lòng chọn file hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        if (model.AvatarFile.Length > 5 * 1024 * 1024)
        {
            TempData["ErrorMessage"] = "Dung lượng file không được vượt quá 5MB.";
            return RedirectToAction(nameof(Index));
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var ext = Path.GetExtension(model.AvatarFile.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext))
        {
            TempData["ErrorMessage"] = "Chỉ hỗ trợ file ảnh (jpg, png, gif).";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            // Giả lập logic upload file lên wwwroot/uploads hoặc Cloud (cần FileService)
            // Lấy tên file an toàn
            string fileName = Guid.NewGuid().ToString() + ext;
            string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
            
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            string filePath = Path.Combine(uploadFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.AvatarFile.CopyToAsync(stream);
            }

            string newAvatarUrl = $"/uploads/avatars/{fileName}";

            int userId = GetCurrentUserId();
            await _profileService.UpdateAvatarAsync(userId, newAvatarUrl);
            
            TempData["SuccessMessage"] = "Cập nhật ảnh đại diện thành công!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi khi upload ảnh: " + ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Profile/ChangePassword")]
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    [HttpPost("Profile/ChangePassword")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            int userId = GetCurrentUserId();
            await _profileService.ChangePasswordAsync(userId, model.OldPassword, model.NewPassword);
            
            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessException ex)
        {
            // Lỗi mật khẩu cũ không đúng
            ModelState.AddModelError("OldPassword", ex.Message);
            return View(model);
        }
        catch (NotFoundException)
        {
            return RedirectToAction("Login", "Account");
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Lỗi hệ thống.");
            return View(model);
        }
    }

    [HttpGet("Profile/Settings")]
    public async Task<IActionResult> Settings()
    {
        try
        {
            int userId = GetCurrentUserId();
            var settings = await _profileService.GetAccountSettingsAsync(userId);
            return View(settings);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Không thể tải cài đặt.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("Profile/Settings")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(AccountSettingViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            int userId = GetCurrentUserId();
            await _profileService.UpdateAccountSettingsAsync(userId, model);
            
            TempData["SuccessMessage"] = "Cập nhật cài đặt thành công!";
            return RedirectToAction(nameof(Settings));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "Lỗi: " + ex.Message);
            return View(model);
        }
    }
}
