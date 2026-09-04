using DuAnTotNghiep.Models.DTOs.PlacementTest;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Filters
{
    public class RequirePlacementTestFilter : IAsyncActionFilter
    {
        private readonly IPlacementRequirementService _placementService;

        public RequirePlacementTestFilter(IPlacementRequirementService placementService)
        {
            _placementService = placementService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = context.HttpContext.User;

            // 1. Skip if not authenticated or not STUDENT
            if (user?.Identity == null || !user.Identity.IsAuthenticated || !user.IsInRole("STUDENT"))
            {
                await next();
                return;
            }

            // 2. Get current route
            var routeValues = context.ActionDescriptor.RouteValues;
            routeValues.TryGetValue("controller", out string? controllerName);

            // 3. Skip for specific controllers
            if (string.Equals(controllerName, "Account", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(controllerName, "Onboarding", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(controllerName, "Profile", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(controllerName, "PlacementTest", StringComparison.OrdinalIgnoreCase))
            {
                await next();
                return;
            }

            // 4. Check Placement Requirement Status
            var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdString, out int userId))
            {
                var flowStatus = await _placementService.GetStudentFlowStatusAsync(userId);

                if (flowStatus.Status != PlacementFlowStatus.Completed)
                {
                    // Handle AJAX
                    if (context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        context.Result = new JsonResult(new { success = false, message = "Bạn cần hoàn thành quá trình kiểm tra đầu vào trước." })
                        {
                            StatusCode = 403
                        };
                        return;
                    }

                    // Redirect appropriately
                    if (flowStatus.Status == PlacementFlowStatus.OnboardingRequired)
                    {
                        context.Result = new RedirectResult(flowStatus.RedirectUrl ?? "/Student/Onboarding");
                    }
                    else if (flowStatus.Status == PlacementFlowStatus.PlacementRequired)
                    {
                        context.Result = new RedirectResult(flowStatus.RedirectUrl ?? "/Student/PlacementTest/Intro");
                    }
                    else if (flowStatus.Status == PlacementFlowStatus.PlacementInProgress)
                    {
                        context.Result = new RedirectResult(flowStatus.RedirectUrl ?? $"/Student/PlacementTest/Take/{flowStatus.AttemptId}");
                    }
                    return;
                }
            }

            // 5. Allowed
            await next();
        }
    }
}
