using DuAnTotNghiep.Models;
using DuAnTotNghiep.Repositories.Interfaces;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.ViewModels;
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

        public Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ForgotPasswordAsync(string email)
        {
            throw new NotImplementedException();
        }

        public async Task<LoginResultDto> LoginAsync(string email, string password, string ipAddress, string userAgent)
        {
            var user = await _userRepository.GetByEmailAsync(email);

            // Ghi log thất bại dùng chung
            async Task LogFailedLoginAsync(string reason)
            {
                var failedLog = new LoginLog
                {
                    UserId = user?.Id,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    IsSuccess = false,
                    FailureReason = reason,
                    CreatedAt = DateTime.UtcNow
                };
                await _loginLogRepository.AddAsync(failedLog);
                await _loginLogRepository.SaveChangesAsync();
                
                if (user != null)
                {
                    user.FailedLoginCount++;
                    _userRepository.Update(user);
                    await _userRepository.SaveChangesAsync();
                }
            }

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

            if (user.Status == "LOCKED")
            {
                await LogFailedLoginAsync("Account locked");
                return new LoginResultDto { IsSuccess = false, ErrorMessage = "Tài khoản đang bị khóa" };
            }

            var hasher = new PasswordHasher<User>();
            var verificationResult = hasher.VerifyHashedPassword(user, user.PasswordHash, password);

            if (verificationResult == PasswordVerificationResult.Failed)
            {
                await LogFailedLoginAsync("Invalid password");
                return new LoginResultDto { IsSuccess = false, ErrorMessage = "Email hoặc mật khẩu không đúng" };
            }

            // Đăng nhập thành công
            var successLog = new LoginLog
            {
                UserId = user.Id,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                IsSuccess = true,
                CreatedAt = DateTime.UtcNow
            };
            await _loginLogRepository.AddAsync(successLog);
            await _loginLogRepository.SaveChangesAsync();

            var auditLog = new AuditLog
            {
                UserId = user.Id,
                Action = "LOGIN",
                EntityName = "User",
                EntityId = user.Id,
                CreatedAt = DateTime.UtcNow,
                IpAddress = ipAddress
            };
            await _auditLogRepository.AddAsync(auditLog);
            await _auditLogRepository.SaveChangesAsync();

            // Reset failed count and update last login
            user.FailedLoginCount = 0;
            user.LastLoginAt = DateTime.UtcNow;
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
            var hasher = new PasswordHasher<User>();
            user.PasswordHash = hasher.HashPassword(user, request.Password);

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
