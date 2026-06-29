namespace DuAnTotNghiep.Models.DTOs.PlacementTest
{
    public enum PlacementFlowStatus
    {
        OnboardingRequired,
        PlacementRequired,
        PlacementInProgress,
        Completed
    }

    public class PlacementFlowResultDto
    {
        public PlacementFlowStatus Status { get; set; }
        public string? RedirectUrl { get; set; }
        public int? AttemptId { get; set; }
    }
}
