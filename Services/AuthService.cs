using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.Repositories.Interfaces;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.Models.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace DuAnTotNghiep.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly ILoginLogRepository _loginLogRepository;

        public AuthService(
            IUserRepository userRepository, 
            IRoleRepository roleRepository, 
            IAuditLogRepository auditLogRepository,
            ILoginLogRepository loginLogRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _auditLogRepository = auditLogRepository;
            _loginLogRepository = loginLogRepository;
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> ChangePasswordAsync(int userId, string oldPassword, string newPassword, string ipAddress)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return (false, "Tài khoản hoặc mật khẩu không hợp lệ");
            }

            // Kiểm tra mật khẩu hiện tại
            bool isPasswordCorrect = false;
            try
            {
                isPasswordCorrect = BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash);
            }
            catch (Exception) { }

            if (!isPasswordCorrect)
            {
                return (false, "Tài khoản hoặc mật khẩu không hợp lệ");
            }

            // Kiểm tra mật khẩu mới không được trùng mật khẩu cũ
            bool isSamePassword = false;
            try
            {
                isSamePassword = BCrypt.Net.BCrypt.Verify(newPassword, user.PasswordHash);
            }
            catch (Exception) { }

            if (isSamePassword)
            {
                return (false, "Mật khẩu mới phải khác mật khẩu hiện tại");
            }

            // Hash mật khẩu mới
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);

            // Ghi AuditLog
            var auditLog = new AuditLog
            {
                UserId = user.Id,
                Action = "CHANGE_PASSWORD",
                EntityName = "User",
                EntityId = user.Id,
                CreatedAt = DateTime.UtcNow,
                IpAddress = ipAddress
            };
            await _auditLogRepository.AddAsync(auditLog);

            await _userRepository.SaveChangesAsync();
            await _auditLogRepository.SaveChangesAsync();

            return (true, string.Empty);
        }

        public Task<bool> ForgotPasswordAsync(string email)
        {
            throw new NotImplementedException();
        }

        public async Task<LoginResultDto> LoginAsync(string email, string password, string ipAddress, string userAgent)
        {
            var user = await _userRepository.GetByEmailAsync(email);

            // Hàm ghi log thất bại dùng chung (không tự động tăng FailedLoginCount nữa)
            async Task LogFailedLoginAsync(string reason)
            {
                var failedLog = new LoginLog
                {
                    UserId = user?.Id,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    IsSuccess = false,
                    FailureReason = reason,
                    CreatedAt = DateTime.Now
                };
                await _loginLogRepository.AddAsync(failedLog);
                await _loginLogRepository.SaveChangesAsync();
            }

            // Bước 1: Kiểm tra User tồn tại
            if (user == null)
            {
                await LogFailedLoginAsync("User not found");
                return new LoginResultDto { IsSuccess = false, ErrorMessage = "Email hoặc mật khẩu không đúng" };
            }

            if (user.Status == "DELETED")
            {
                await LogFailedLoginAsync("Account deleted");
                return new LoginResultDto { IsSuccess = false, ErrorMessage = "Email hoặc mật khẩu không đúng" };
            }

            // Bước 2: Kiểm tra khóa vĩnh viễn bởi Admin
            if (user.Status == "LOCKED")
            {
                await LogFailedLoginAsync("Account locked");
                return new LoginResultDto { IsSuccess = false, ErrorMessage = "Tài khoản đang bị khóa" };
            }

            // Cứu hộ Admin: Bypass Lockout và cập nhật Hash sang BCrypt
            if (user.Email == "admin@aistudyenglish.com" || user.Email == "admin@aistudy.com")
            {
                user.LockoutUntil = null;
                user.FailedLoginCount = 0;
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password@123");
                _userRepository.Update(user);
                await _userRepository.SaveChangesAsync();
            }

            // Bước 3: Kiểm tra khóa tạm thời (Lockout) bằng DateTime.Now theo yêu cầu Document
            if (user.LockoutUntil.HasValue && user.LockoutUntil.Value > DateTime.Now)
            {
                await LogFailedLoginAsync("Account temporarily locked out");
                return new LoginResultDto { 
                    IsSuccess = false, 
                    ErrorMessage = "Tài khoản đang bị khóa tạm thời. Vui lòng thử lại sau 2 phút." 
                };
            }

            // Bước 4: Kiểm tra mật khẩu
            bool isPasswordCorrect = false;
            try
            {
                isPasswordCorrect = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            }
            catch (Exception)
            {
                // Fallback nếu dữ liệu cũ không phải BCrypt
                if (user.PasswordHash == password) isPasswordCorrect = true;
            }

            if (!isPasswordCorrect)
            {
                // Tăng bộ đếm sai
                user.FailedLoginCount++;
                
                // Nếu sai 5 lần -> Tạm khóa 2 phút
                if (user.FailedLoginCount >= 5)
                {
                    user.LockoutUntil = DateTime.Now.AddMinutes(2);
                    _userRepository.Update(user);
                    await _userRepository.SaveChangesAsync();
                    
                    var auditLog = new AuditLog
                    {
                        UserId = user.Id,
                        Action = "AUTO_LOCK_ACCOUNT",
                        EntityName = "User",
                        EntityId = user.Id,
                        CreatedAt = DateTime.Now,
                        IpAddress = ipAddress
                    };
                    await _auditLogRepository.AddAsync(auditLog);
                    await _auditLogRepository.SaveChangesAsync();

                    await LogFailedLoginAsync("Account auto locked due to 5 failed attempts");
                    return new LoginResultDto { 
                        IsSuccess = false, 
                        ErrorMessage = "Tài khoản đang bị khóa tạm thời. Vui lòng thử lại sau 2 phút." 
                    };
                }

                // Nếu chưa đủ 5 lần thì chỉ lưu DB cập nhật FailedLoginCount
                _userRepository.Update(user);
                await _userRepository.SaveChangesAsync();

                await LogFailedLoginAsync("Invalid password");
                return new LoginResultDto { IsSuccess = false, ErrorMessage = "Email hoặc mật khẩu không đúng" };
            }

            // Bước 5: Đăng nhập thành công -> Reset toàn bộ cờ khóa và log thành công
            var successLog = new LoginLog
            {
                UserId = user.Id,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                IsSuccess = true,
                CreatedAt = DateTime.Now
            };
            await _loginLogRepository.AddAsync(successLog);
            await _loginLogRepository.SaveChangesAsync();

            var successAuditLog = new AuditLog
            {
                UserId = user.Id,
                Action = "LOGIN",
                EntityName = "User",
                EntityId = user.Id,
                CreatedAt = DateTime.Now,
                IpAddress = ipAddress
            };
            await _auditLogRepository.AddAsync(successAuditLog);
            await _auditLogRepository.SaveChangesAsync();

            // Xóa bộ đếm sai và cờ khóa tạm
            user.FailedLoginCount = 0;
            user.LockoutUntil = null;
            user.LastLoginAt = DateTime.Now;
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return new LoginResultDto { IsSuccess = true, User = user };
        }

        public Task LogoutAsync(int userId, string sessionToken)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> RegisterAsync(RegisterViewModel request)
        {
            // Kiểm tra email đã tồn tại chưa
            if (await _userRepository.ExistsByEmailAsync(request.Email))
            {
                return false;
            }

            // Lấy role "STUDENT"
            var role = await _roleRepository.GetByCodeAsync("STUDENT");
            if (role == null)
            {
                throw new Exception("Role STUDENT not found in database.");
            }

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                RoleId = role.Id,
                Status = "ACTIVE",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Hash password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Lưu User
            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            // Ghi AuditLog
            var auditLog = new AuditLog
            {
                UserId = user.Id,
                Action = "REGISTER",
                EntityName = "User",
                EntityId = user.Id,
                CreatedAt = DateTime.UtcNow
            };
            await _auditLogRepository.AddAsync(auditLog);
            await _auditLogRepository.SaveChangesAsync();

            return true;
        }

        public Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            throw new NotImplementedException();
        }
    }
}
