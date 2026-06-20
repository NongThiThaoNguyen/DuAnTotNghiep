using DuAnTotNghiep.Models;
using DuAnTotNghiep.Repositories.Interfaces;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace DuAnTotNghiep.Services
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordResetTokenRepository _tokenRepository;
        private readonly IEmailService _emailService;
        private readonly IAuditLogRepository _auditLogRepository;

        public PasswordResetService(
            IUserRepository userRepository, 
            IPasswordResetTokenRepository tokenRepository, 
            IEmailService emailService,
            IAuditLogRepository auditLogRepository)
        {
            _userRepository = userRepository;
            _tokenRepository = tokenRepository;
            _emailService = emailService;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<bool> SendOtpAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null || user.Status == "DELETED" || user.Status == "LOCKED")
                return false;

            // Vô hiệu hóa OTP cũ
            await _tokenRepository.InvalidateOldTokensAsync(user.Id);

            // Sinh mã OTP 6 chữ số
            var random = new Random();
            var otp = random.Next(100000, 999999).ToString();

            // Lưu vào DB
            var token = new PasswordResetToken
            {
                UserId = user.Id,
                TokenHash = otp, // Lưu trực tiếp mã OTP dạng string
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5)
            };
            
            await _tokenRepository.AddAsync(token);
            await _tokenRepository.SaveChangesAsync();

            // Ghi Log
            var auditLog = new AuditLog
            {
                UserId = user.Id,
                Action = "FORGOT_PASSWORD",
                EntityName = "PasswordResetToken",
                EntityId = token.Id,
                CreatedAt = DateTime.UtcNow
            };
            await _auditLogRepository.AddAsync(auditLog);
            await _auditLogRepository.SaveChangesAsync();

            // Gửi email
            string subject = "Yêu cầu khôi phục mật khẩu";
            string body = $@"
                <h3>Chào {user.FullName},</h3>
                <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản AI Study English.</p>
                <p>Mã OTP của bạn là: <strong><span style='font-size:24px;color:#2563EB;'>{otp}</span></strong></p>
                <p>Mã này có hiệu lực trong vòng 5 phút. Vui lòng không chia sẻ mã này cho bất kỳ ai.</p>
                <p>Trân trọng!</p>";

            await _emailService.SendEmailAsync(user.Email, subject, body);

            return true;
        }

        public async Task<bool> VerifyOtpAsync(string email, string otp)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) return false;

            var token = await _tokenRepository.GetValidTokenAsync(user.Id, otp);
            
            if (token != null)
            {
                var auditLog = new AuditLog
                {
                    UserId = user.Id,
                    Action = "VERIFY_OTP",
                    EntityName = "PasswordResetToken",
                    EntityId = token.Id,
                    CreatedAt = DateTime.UtcNow
                };
                await _auditLogRepository.AddAsync(auditLog);
                await _auditLogRepository.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> ResetPasswordAsync(string email, string otp, string newPassword, string ipAddress)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) return false;

            var token = await _tokenRepository.GetValidTokenAsync(user.Id, otp);
            if (token == null) return false;

            // Đổi mật khẩu
            var hasher = new PasswordHasher<User>();
            user.PasswordHash = hasher.HashPassword(user, newPassword);
            
            _userRepository.Update(user);

            // Đánh dấu token đã dùng
            token.UsedAt = DateTime.UtcNow;
            _tokenRepository.Update(token); // Nếu RepositoryPattern có Update thì Update, còn không SaveChanges là đủ
            
            // Ghi log
            var auditLog = new AuditLog
            {
                UserId = user.Id,
                Action = "RESET_PASSWORD",
                EntityName = "User",
                EntityId = user.Id,
                CreatedAt = DateTime.UtcNow,
                IpAddress = ipAddress
            };
            await _auditLogRepository.AddAsync(auditLog);

            // Lưu toàn bộ
            await _userRepository.SaveChangesAsync();
            // DbContext sẽ commit cả 3 update nếu dùng chung 1 instance (thông thường là scoped).

            return true;
        }
    }
}
