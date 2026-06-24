using System;
using System.Threading.Tasks;
using DuAnTotNghiep.DTOs;
using DuAnTotNghiep.DTOs.PlacementTest;
using DuAnTotNghiep.ViewModels.Admin;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IPlacementAttemptService
    {
        Task<PagedResult<PlacementAttemptListItemViewModel>> GetAttemptsForAdminAsync(PlacementAttemptFilter filter, int currentUserId, string role);
        Task<PlacementAttemptDetailViewModel?> GetAttemptDetailAsync(int attemptId, int currentUserId, string role);
    }
}
