using System;
using System.Threading.Tasks;
using DuAnTotNghiep.Models.DTOs;
using DuAnTotNghiep.Models.DTOs.PlacementTest;
using DuAnTotNghiep.Models.ViewModels.Admin;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IPlacementAttemptService
    {
        Task<PagedResult<PlacementAttemptListItemViewModel>> GetAttemptsForAdminAsync(PlacementAttemptFilter filter, int currentUserId, string role);
        Task<PlacementAttemptDetailViewModel?> GetAttemptDetailAsync(int attemptId, int currentUserId, string role);
    }
}
