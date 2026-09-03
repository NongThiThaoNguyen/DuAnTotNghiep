using System;
using System.Security.Cryptography;
using System.Text;
using DuAnTotNghiep.Services.Interfaces;

namespace DuAnTotNghiep.Services;

public class GoogleMeetService : IGoogleMeetService
{
    public string GenerateMeetUrl(int scheduleId, string? title = null)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes($"schedule-{scheduleId}-{title}"));
        var hex = Convert.ToHexString(hash).ToLowerInvariant();

        var part1 = hex.Substring(0, 3);
        var part2 = hex.Substring(3, 4);
        var part3 = hex.Substring(7, 3);

        return $"https://meet.google.com/{part1}-{part2}-{part3}";
    }
}
