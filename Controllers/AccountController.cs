using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DuAnTotNghiep.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly DuAnTotNghiep.Repositories.Interfaces.IUserSessionRepository _userSessionRepository;

        public AccountController(IAuthService authService, DuAnTotNghiep.Repositories.Interfaces.IUserSessionRepository userSessionRepository)
        {
            _authService = authService;
            _userSessionRepository = userSessionRepository;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool result = await _authService.RegisterAsync(model);

            if (!result)
            {
                ModelState.AddModelError("Email", "Email đã tồn tại trong hệ thống.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            string userAgent = Request.Headers["User-Agent"].ToString();

            var result = await _authService.LoginAsync(model.Email, model.Password, ipAddress, userAgent);

            if (!result.IsSuccess || result.User == null)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                return View(model);
            }

            var user = result.User;

            // Tạo Claims
            string sessionToken = Guid.NewGuid().ToString();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role.RoleCode),
                new Claim("SessionToken", sessionToken)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Cấu hình properties cho Cookie
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : null
            };

            // Thực hiện Sign In
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, 
                new ClaimsPrincipal(claimsIdentity), 
                authProperties);

            // Ghi nhận UserSession
            var session = new DuAnTotNghiep.Models.UserSession
            {
                UserId = user.Id,
                SessionToken = sessionToken,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                DeviceType = "Web",
                CreatedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow,
                ExpiresAt = authProperties.ExpiresUtc?.UtcDateTime ?? DateTime.UtcNow.AddHours(2)
            };
            await _userSessionRepository.AddAsync(session);
            await _userSessionRepository.SaveChangesAsync();

            // Điều hướng theo Role
            return user.Role.RoleCode switch
            {
                "ADMIN" => RedirectToAction("Index", "Home", new { area = "Admin" }),
                "TEACHER" => RedirectToAction("Index", "Home", new { area = "Teacher" }),
                _ => RedirectToAction("Index", "Home", new { area = "Student" }) // STUDENT
            };
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model, [FromServices] IPasswordResetService passwordResetService)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await passwordResetService.SendOtpAsync(model.Email);
            
            TempData["SuccessMessage"] = "Nếu Email tồn tại trong hệ thống, mã OTP đã được gửi đến bạn.";
            TempData["ResetEmail"] = model.Email;
            return RedirectToAction("VerifyOtp");
        }

        [HttpGet]
        public IActionResult VerifyOtp()
        {
            var email = TempData.Peek("ResetEmail")?.ToString();
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("ForgotPassword");
            }

            return View(new VerifyOtpViewModel { Email = email });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model, [FromServices] IPasswordResetService passwordResetService)
        {
            if (!ModelState.IsValid)
            {
                TempData.Keep("ResetEmail");
                return View(model);
            }

            var result = await passwordResetService.VerifyOtpAsync(model.Email, model.Otp);
            if (!result)
            {
                ModelState.AddModelError("Otp", "Mã OTP không chính xác hoặc đã hết hạn.");
                TempData.Keep("ResetEmail");
                return View(model);
            }

            TempData["VerifiedOtp"] = model.Otp;
            TempData.Keep("ResetEmail");
            return RedirectToAction("ResetPassword");
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            var email = TempData.Peek("ResetEmail")?.ToString();
            var otp = TempData.Peek("VerifiedOtp")?.ToString();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otp))
            {
                return RedirectToAction("ForgotPassword");
            }

            return View(new ResetPasswordViewModel { Email = email, Otp = otp });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model, [FromServices] IPasswordResetService passwordResetService)
        {
            if (!ModelState.IsValid)
            {
                TempData.Keep("ResetEmail");
                TempData.Keep("VerifiedOtp");
                return View(model);
            }

            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            
            var result = await passwordResetService.ResetPasswordAsync(model.Email, model.Otp, model.NewPassword, ipAddress);
            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi đổi mật khẩu, OTP có thể đã hết hạn.");
                return View(model);
            }

            // Xóa TempData sau khi thành công
            TempData.Remove("ResetEmail");
            TempData.Remove("VerifiedOtp");

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công! Vui lòng đăng nhập lại.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            // Hủy session trong DB nếu có
            var sessionToken = User.FindFirstValue("SessionToken");
            if (!string.IsNullOrEmpty(sessionToken))
            {
                var currentSession = await _userSessionRepository.GetBySessionTokenAsync(sessionToken);
                if (currentSession != null)
                {
                    currentSession.RevokedAt = DateTime.UtcNow;
                    _userSessionRepository.Update(currentSession);
                    await _userSessionRepository.SaveChangesAsync();
                }
            }

            await HttpContext.SignOutAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
