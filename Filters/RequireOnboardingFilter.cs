using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Filters
{
    public class RequireOnboardingFilter : IAsyncActionFilter
    {
        private readonly ILearningProfileService _profileService;

        public RequireOnboardingFilter(ILearningProfileService profileService)
        {
            _profileService = profileService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = context.HttpContext.User;

            // 1. Bỏ qua nếu chưa đăng nhập hoặc không phải là sinh viên
            if (user?.Identity == null || !user.Identity.IsAuthenticated || !user.IsInRole("STUDENT"))
            {
                await next();
                return;
            }

            // 2. Lấy thông tin route hiện tại
            var routeValues = context.ActionDescriptor.RouteValues;
            routeValues.TryGetValue("controller", out string? controllerName);

            // 3. Ngoại trừ các Controller công khai, Account, Profile, hoặc Controller của chính Onboarding
            if (string.Equals(controllerName, "Account", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(controllerName, "Onboarding", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(controllerName, "Profile", StringComparison.OrdinalIgnoreCase))
            {
                await next();
                return;
            }

            // 4. Kiểm tra trạng thái Onboarding
            var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdString, out int userId))
            {
                var profile = await _profileService.GetProfileSummaryAsync(userId);

                // 5. Nếu chưa có profile hoặc chưa hoàn thành -> Chặn
                if (profile == null || profile.OnboardingStatus != "COMPLETED")
                {
                    // Xử lý riêng cho các request AJAX
                    if (context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        context.Result = new JsonResult(new { success = false, message = "Vui lòng hoàn thành quá trình thiết lập hồ sơ." })
                        {
                            StatusCode = 403
                        };
                        return;
                    }

                    // Redirect về màn hình bắt đầu Onboarding
                    context.Result = new RedirectToActionResult("Index", "Onboarding", new { area = "Student" });
                    return;
                }
            }

            // Đã hoàn thành Onboarding -> Cho phép truy cập
            await next();
        }
    }
}
