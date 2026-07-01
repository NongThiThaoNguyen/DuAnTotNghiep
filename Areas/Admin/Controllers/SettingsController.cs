using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DuAnTotNghiep.Services.Interfaces;

namespace DuAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN")]
    public class SettingsController : SystemSettingsController
    {
        public SettingsController(ISystemSettingService systemSettingService)
            : base(systemSettingService)
        {
        }
    }
}
