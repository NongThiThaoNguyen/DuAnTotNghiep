using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.Models.Exceptions;
using DuAnTotNghiep.Models.ViewModels.Profile;

namespace DuAnTotNghiep.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly IUserProfileService _profileService;
    private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _webHostEnvironment;

    public ProfileController(IUserProfileService profileService, Microsoft.AspNetCore.Hosting.IWebHostEnvironment webHostEnvironment)
    {
        _profileService = profileService;
        _webHostEnvironment = webHostEnvironment;
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
        catch (Exception)
        {
            // Ideally return View("Error", new ErrorViewModel { ... })
            TempData["ErrorMessage"] = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.";
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpGet("Profile/Edit")]
    public async Task<IActionResult> Edit()
    {
        try
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");

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
            if (userId == 0) return RedirectToAction("Login", "Account");

            await _profileService.UpdateProfileAsync(userId, model);
            
            TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (NotFoundException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi cập nhật. Vui lòng thử lại sau.");
            return View(model);
        }
    }

    [HttpPost("Profile/UploadAvatar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadAvatar(AvatarUploadViewModel model)
    {
        int userId = GetCurrentUserId();
        if (userId == 0) return RedirectToAction("Login", "Account");

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

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var ext = Path.GetExtension(model.AvatarFile.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext) || !model.AvatarFile.ContentType.StartsWith("image/"))
        {
            TempData["ErrorMessage"] = "Chỉ hỗ trợ file ảnh định dạng hợp lệ (jpg, png, gif, webp).";
            return RedirectToAction(nameof(Index));
        }

        using (var stream = model.AvatarFile.OpenReadStream())
        {
            if (!IsValidImageSignature(stream))
            {
                TempData["ErrorMessage"] = "Nội dung file không phải là hình ảnh hợp lệ (nghi ngờ giả mạo).";
                return RedirectToAction(nameof(Index));
            }
        }

        try
        {
            string fileName = Guid.NewGuid().ToString() + ext;
            string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "avatars");
            
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

            await _profileService.UpdateAvatarAsync(userId, newAvatarUrl);
            
            // Refresh authentication cookie so the layout can see the new avatar immediately
            if (User.Identity is ClaimsIdentity identity)
            {
                var existingClaim = identity.FindFirst("AvatarUrl");
                if (existingClaim != null)
                {
                    identity.RemoveClaim(existingClaim);
                }
                identity.AddClaim(new Claim("AvatarUrl", newAvatarUrl));
                
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity));
            }
            
            TempData["SuccessMessage"] = "Cập nhật ảnh đại diện thành công!";
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Đã xảy ra lỗi hệ thống trong quá trình upload ảnh. Vui lòng thử lại.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Profile/ChangePassword")]
    public IActionResult ChangePassword()
    {
        int userId = GetCurrentUserId();
        if (userId == 0) return RedirectToAction("Login", "Account");

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
            if (userId == 0) return RedirectToAction("Login", "Account");

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
            ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.");
            return View(model);
        }
    }

    [HttpGet("Profile/Settings")]
    public async Task<IActionResult> Settings()
    {
        try
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");

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
            if (userId == 0) return RedirectToAction("Login", "Account");

            await _profileService.UpdateAccountSettingsAsync(userId, model);
            
            TempData["SuccessMessage"] = "Cập nhật cài đặt thành công!";
            return RedirectToAction(nameof(Settings));
        }
        catch (NotFoundException)
        {
            return RedirectToAction("Login", "Account");
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi hệ thống trong quá trình lưu cài đặt.");
            return View(model);
        }
    }

    private bool IsValidImageSignature(Stream stream)
    {
        stream.Position = 0;
        using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, true);
        var signatures = new System.Collections.Generic.List<byte[]>
        {
            new byte[] { 0xFF, 0xD8, 0xFF }, // JPEG
            new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, // PNG
            new byte[] { 0x47, 0x49, 0x46, 0x38 }, // GIF
            new byte[] { 0x52, 0x49, 0x46, 0x46 }  // WEBP (starts with RIFF)
        };

        var headerBytes = reader.ReadBytes(8);
        stream.Position = 0;

        return signatures.Any(signature => 
            headerBytes.Take(signature.Length).SequenceEqual(signature));
    }
}
