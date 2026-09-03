namespace DuAnTotNghiep.Services.Interfaces;

public interface IGoogleMeetService
{
    string GenerateMeetUrl(int scheduleId, string? title = null);
}
