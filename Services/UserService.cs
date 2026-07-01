using DuAnTotNghiep.Areas.Admin.ViewModels;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.Repositories.Interfaces;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Text;

using DuAnTotNghiep.Data;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IEmailService _emailService;
        private readonly ApplicationDbContext _context;

        public UserService(
            IUserRepository userRepository,
            IAuditLogRepository auditLogRepository,
            IEmailService emailService,
            ApplicationDbContext context)
        {
            _userRepository = userRepository;
            _auditLogRepository = auditLogRepository;
            _emailService = emailService;
            _context = context;
        }

        public async Task<UserListViewModel> GetPagedUsersAsync(UserFilterViewModel filter)
        {
            var (users, totalCount) = await _userRepository.GetPagedUsersAsync(
                filter.Keyword,
                filter.Role,
                filter.Status,
                filter.Page,
                filter.PageSize);

            var userViewModels = users.Select(u => new UserListItemViewModel
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                RoleName = u.Role?.RoleName ?? "Unknown",
                RoleCode = u.Role?.RoleCode ?? "UNKNOWN",
                Status = u.Status,
                CreatedAt = u.CreatedAt
            }).ToList();

            return new UserListViewModel
            {
                Users = userViewModels,
                TotalItems = totalCount,
                Filter = filter
            };
        }

        public async Task<UserDetailsViewModel?> GetUserDetailsAsync(int id)
        {
            var user = await _userRepository.GetUserWithRoleByIdAsync(id);
            if (user == null) return null;

            return new UserDetailsViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                RoleName = user.Role?.RoleName ?? "Unknown",
                Status = user.Status,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                FailedLoginCount = user.FailedLoginCount,
                LockoutUntil = user.LockoutUntil
            };
        }

        public async Task<bool> LockUserAsync(int userId, int adminId, string ipAddress)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.Status == "LOCKED" || user.Status == "DELETED")
                return false;

            // Đặt thời gian khóa vĩnh viễn (100 năm)
            await _userRepository.LockUserAsync(userId, DateTime.UtcNow.AddYears(100));

            var auditLog = new AuditLog
            {
                UserId = adminId,
                Action = "LOCK_USER",
                EntityName = "User",
                EntityId = userId,
                CreatedAt = DateTime.UtcNow,
                IpAddress = ipAddress
            };
            await _auditLogRepository.AddAsync(auditLog);

            await _userRepository.SaveChangesAsync();
            await _auditLogRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnlockUserAsync(int userId, int adminId, string ipAddress)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.Status != "LOCKED")
                return false;

            await _userRepository.UnlockUserAsync(userId);

            var auditLog = new AuditLog
            {
                UserId = adminId,
                Action = "UNLOCK_USER",
                EntityName = "User",
                EntityId = userId,
                CreatedAt = DateTime.UtcNow,
                IpAddress = ipAddress
            };
            await _auditLogRepository.AddAsync(auditLog);

            await _userRepository.SaveChangesAsync();
            await _auditLogRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResetPasswordAsync(int userId, int adminId, string ipAddress)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.Status == "DELETED")
                return false;

            // 1. Sinh mật khẩu ngẫu nhiên
            string newPassword = GenerateRandomPassword();

            // 2. Mã hóa mật khẩu
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);

            // 3. Ghi log
            var auditLog = new AuditLog
            {
                UserId = adminId,
                Action = "RESET_PASSWORD",
                EntityName = "User",
                EntityId = userId,
                CreatedAt = DateTime.UtcNow,
                IpAddress = ipAddress
            };
            await _auditLogRepository.AddAsync(auditLog);

            await _userRepository.SaveChangesAsync();
            await _auditLogRepository.SaveChangesAsync();

            // 4. Gửi email
            string subject = "Mật khẩu của bạn đã được đặt lại";
            string body = $@"
                <h3>Chào {user.FullName},</h3>
                <p>Quản trị viên hệ thống đã đặt lại mật khẩu cho tài khoản AI Study English của bạn.</p>
                <p>Mật khẩu mới của bạn là: <strong><span style='font-size:20px;color:#EF4444;'>{newPassword}</span></strong></p>
                <p>Vui lòng đăng nhập bằng mật khẩu này và đổi mật khẩu mới ngay lập tức để đảm bảo an toàn.</p>
                <p>Trân trọng!</p>";

            await _emailService.SendEmailAsync(user.Email, subject, body);

            return true;
        }

        private string GenerateRandomPassword()
        {
            // Sinh chuỗi mật khẩu mạnh gồm Chữ hoa, chữ thường, số và ký tự đặc biệt
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string special = "@#$!%*?&";

            var random = new Random();
            var password = new StringBuilder();

            password.Append(upper[random.Next(upper.Length)]);
            password.Append(lower[random.Next(lower.Length)]);
            password.Append(digits[random.Next(digits.Length)]);
            password.Append(special[random.Next(special.Length)]);

            const string allChars = upper + lower + digits;
            for (int i = 0; i < 6; i++)
            {
                password.Append(allChars[random.Next(allChars.Length)]);
            }

            // Trộn các ký tự để không bị đoán trước vị trí
            return new string(password.ToString().OrderBy(x => random.Next()).ToArray());
        }

        public async Task<bool> ChangeRoleAsync(int userId, int newRoleId, int adminId, string ipAddress)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            var oldRoleId = user.RoleId;
            user.RoleId = newRoleId;
            user.UpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);

            var auditLog = new AuditLog
            {
                UserId = adminId,
                Action = "CHANGE_ROLE",
                EntityName = "User",
                EntityId = userId,
                OldValue = $"Role {oldRoleId}",
                NewValue = $"Role {newRoleId}",
                CreatedAt = DateTime.UtcNow,
                IpAddress = ipAddress
            };
            await _auditLogRepository.AddAsync(auditLog);

            await _userRepository.SaveChangesAsync();
            await _auditLogRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleLockAsync(int userId, int adminId, string ipAddress)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            if (user.Status == "ACTIVE")
            {
                user.Status = "LOCKED";
                user.LockoutUntil = DateTime.UtcNow.AddYears(100);
            }
            else if (user.Status == "LOCKED")
            {
                user.Status = "ACTIVE";
                user.LockoutUntil = null;
            }
            else
            {
                return false; // DELETED or other
            }

            user.UpdatedAt = DateTime.UtcNow;
            _userRepository.Update(user);

            var auditLog = new AuditLog
            {
                UserId = adminId,
                Action = user.Status == "LOCKED" ? "LOCK_USER" : "UNLOCK_USER",
                EntityName = "User",
                EntityId = userId,
                CreatedAt = DateTime.UtcNow,
                IpAddress = ipAddress
            };
            await _auditLogRepository.AddAsync(auditLog);

            await _userRepository.SaveChangesAsync();
            await _auditLogRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AdminResetPasswordAsync(int userId, string newPassword, int adminId, string ipAddress)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.Status == "DELETED")
                return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);

            var auditLog = new AuditLog
            {
                UserId = adminId,
                Action = "ADMIN_RESET_PASSWORD",
                EntityName = "User",
                EntityId = userId,
                CreatedAt = DateTime.UtcNow,
                IpAddress = ipAddress
            };
            await _auditLogRepository.AddAsync(auditLog);

            await _userRepository.SaveChangesAsync();
            await _auditLogRepository.SaveChangesAsync();

            return true;
        }

        public async Task<UserStatisticsViewModel> GetUserStatisticsAsync(int userId)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;

            var quizzesCompleted = await _context.TestAttempts
                .AsNoTracking()
                .Where(t => t.StudentId == userId && (t.Status == "SUBMITTED" || t.Status == "GRADED"))
                .CountAsync();

            var lessonsCompleted = await _context.LearningPathNodes
                .AsNoTracking()
                .Include(n => n.LearningPath)
                .Where(n => n.LearningPath.StudentId == userId && n.Status == "COMPLETED")
                .CountAsync();

            var activeDays = await _context.LearningPathNodes
                .AsNoTracking()
                .Include(n => n.LearningPath)
                .Where(n => n.LearningPath.StudentId == userId && n.Status == "COMPLETED" && n.CompletedAt.HasValue)
                .Select(n => n.CompletedAt.Value.Date)
                .Distinct()
                .CountAsync();

            var totalStudyMinutes = await _context.LearningPathNodes
                .AsNoTracking()
                .Include(n => n.LearningPath)
                .Where(n => n.LearningPath.StudentId == userId && n.Status == "COMPLETED")
                .SumAsync(n => n.EstimatedMinutes ?? 0);

            return new UserStatisticsViewModel
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                QuizzesCompleted = quizzesCompleted,
                LessonsCompleted = lessonsCompleted,
                ActiveDays = activeDays,
                TotalStudyMinutes = totalStudyMinutes,
                LastLoginAt = user.LastLoginAt,
                CompetencyScore = 0
            };
        }
    }
}
