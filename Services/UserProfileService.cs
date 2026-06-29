using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.Exceptions;
using DuAnTotNghiep.Models.ViewModels.Profile;
using DuAnTotNghiep.Areas.Admin.ViewModels;
using DuAnTotNghiep.Services.Interfaces;
using BCrypt.Net;

namespace DuAnTotNghiep.Services;

public class UserProfileService : IUserProfileService
{
    private readonly ApplicationDbContext _context;

    public UserProfileService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProfileViewModel> GetProfileAsync(int userId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.UserProfile)
            .Include(u => u.UserSetting)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new NotFoundException("Người dùng không tồn tại.");

        return new ProfileViewModel
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            AvatarUrl = user.AvatarUrl,
            Phone = user.Phone,
            DateOfBirth = user.UserProfile?.DateOfBirth,
            Gender = user.UserProfile?.Gender,
            Country = user.UserProfile?.Country,
            Bio = user.UserProfile?.Bio,
            Language = user.UserSetting?.Language,
            Timezone = user.UserSetting?.Timezone,
            EmailNotifications = user.UserSetting?.EmailNotifications ?? true,
            StudyReminderEnabled = user.UserSetting?.StudyReminderEnabled ?? true,
            Theme = user.UserSetting?.Theme
        };
    }

    public async Task UpdateProfileAsync(int userId, EditProfileViewModel model)
    {
        var user = await _context.Users
            .Include(u => u.UserProfile)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new NotFoundException("Người dùng không tồn tại.");

        user.FullName = model.FullName?.Trim();
        user.Phone = model.Phone?.Trim();

        if (user.UserProfile == null)
        {
            user.UserProfile = new UserProfile
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.UserProfiles.Add(user.UserProfile);
        }

        user.UserProfile.DateOfBirth = model.DateOfBirth;
        user.UserProfile.Gender = model.Gender;
        user.UserProfile.Country = model.Country?.Trim();
        user.UserProfile.Bio = model.Bio?.Trim();
        user.UserProfile.UpdatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        var auditLog = new AuditLog
        {
            UserId = userId,
            Action = "Update Profile",
            EntityName = "UserProfile",
            EntityId = user.UserProfile.Id > 0 ? user.UserProfile.Id : userId,
            OldValue = "Hidden",
            NewValue = "Hidden",
            IpAddress = "System",
            CreatedAt = DateTime.UtcNow
        };
        _context.AuditLogs.Add(auditLog);

        await _context.SaveChangesAsync();
    }

    public async Task<AccountSettingViewModel> GetAccountSettingsAsync(int userId)
    {
        var setting = await _context.UserSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (setting == null)
        {
            return new AccountSettingViewModel
            {
                Language = "vi-VN",
                Timezone = "Asia/Ho_Chi_Minh",
                EmailNotifications = true,
                StudyReminderEnabled = true,
                Theme = "light"
            };
        }

        return new AccountSettingViewModel
        {
            Language = setting.Language,
            Timezone = setting.Timezone,
            EmailNotifications = setting.EmailNotifications,
            StudyReminderEnabled = setting.StudyReminderEnabled,
            Theme = setting.Theme
        };
    }

    public async Task UpdateAccountSettingsAsync(int userId, AccountSettingViewModel model)
    {
        var user = await _context.Users
            .Include(u => u.UserSetting)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new NotFoundException("Người dùng không tồn tại.");

        if (user.UserSetting == null)
        {
            user.UserSetting = new UserSetting
            {
                UserId = userId
            };
            _context.UserSettings.Add(user.UserSetting);
        }

        user.UserSetting.Language = model.Language;
        user.UserSetting.Timezone = model.Timezone;
        user.UserSetting.EmailNotifications = model.EmailNotifications;
        user.UserSetting.StudyReminderEnabled = model.StudyReminderEnabled;
        user.UserSetting.Theme = model.Theme;

        var auditLog = new AuditLog
        {
            UserId = userId,
            Action = "Update Account Settings",
            EntityName = "UserSetting",
            EntityId = user.UserSetting.Id > 0 ? user.UserSetting.Id : userId,
            OldValue = "Hidden",
            NewValue = "Hidden",
            IpAddress = "System",
            CreatedAt = DateTime.UtcNow
        };
        _context.AuditLogs.Add(auditLog);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateAvatarAsync(int userId, string avatarUrl)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new NotFoundException("Người dùng không tồn tại.");

        var history = new UserAvatarHistory
        {
            UserId = userId,
            OldAvatarUrl = user.AvatarUrl,
            NewAvatarUrl = avatarUrl,
            ChangedAt = DateTime.UtcNow
        };
        _context.UserAvatarHistories.Add(history);

        var auditLog = new AuditLog
        {
            UserId = userId,
            Action = "Change Avatar",
            EntityName = "User",
            EntityId = userId,
            OldValue = user.AvatarUrl ?? "None",
            NewValue = avatarUrl,
            IpAddress = "System",
            CreatedAt = DateTime.UtcNow
        };
        _context.AuditLogs.Add(auditLog);

        user.AvatarUrl = avatarUrl;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(int userId, string oldPassword, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new NotFoundException("Người dùng không tồn tại.");

        bool isPasswordCorrect = false;

        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            throw new BusinessException("Tài khoản này chưa có mật khẩu (có thể được tạo qua Google/Facebook).");
        }

        try
        {
            isPasswordCorrect = BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash);
        }
        catch (Exception)
        {
            // Fallback cho dữ liệu seed mẫu nếu PasswordHash không phải là mã băm BCrypt hợp lệ (VD: "123456")
            if (user.PasswordHash == oldPassword)
            {
                isPasswordCorrect = true;
            }
        }

        if (!isPasswordCorrect)
            throw new BusinessException("Mật khẩu hiện tại không chính xác.");

        if (oldPassword == newPassword)
            throw new BusinessException("Mật khẩu mới không được trùng với mật khẩu hiện tại.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;

        var auditLog = new AuditLog
        {
            UserId = userId,
            Action = "Change Password",
            EntityName = "User",
            EntityId = userId,
            OldValue = "Hidden",
            NewValue = "Hidden",
            IpAddress = "System",
            CreatedAt = DateTime.UtcNow
        };
        _context.AuditLogs.Add(auditLog);

        await _context.SaveChangesAsync();
    }

    public async Task<AdminUserProfileViewModel> GetAdminUserProfileAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.UserProfile)
            .Include(u => u.UserSetting)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new NotFoundException("Người dùng không tồn tại.");

        return new AdminUserProfileViewModel
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            AvatarUrl = user.AvatarUrl,
            Phone = user.Phone,
            RoleName = user.Role.RoleName,
            Status = user.Status,
            DateOfBirth = user.UserProfile?.DateOfBirth,
            Gender = user.UserProfile?.Gender,
            Country = user.UserProfile?.Country,
            Bio = user.UserProfile?.Bio,
            Language = user.UserSetting?.Language,
            Timezone = user.UserSetting?.Timezone,
            EmailNotifications = user.UserSetting?.EmailNotifications ?? true,
            StudyReminderEnabled = user.UserSetting?.StudyReminderEnabled ?? true,
            Theme = user.UserSetting?.Theme,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            FailedLoginCount = user.FailedLoginCount,
            LockoutUntil = user.LockoutUntil
        };
    }
}
