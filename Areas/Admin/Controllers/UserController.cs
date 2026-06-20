using DuAnTotNghiep.Areas.Admin.ViewModels;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DuAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> Index([FromQuery] UserFilterViewModel filter)
        {
            var model = await _userService.GetPagedUsersAsync(filter);
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userDetails = await _userService.GetUserDetailsAsync(id);
            if (userDetails == null)
            {
                return NotFound();
            }

            return View(userDetails);
        }
    }
}
