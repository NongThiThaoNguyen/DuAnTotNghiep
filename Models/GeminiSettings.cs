namespace DuAnTotNghiep.Models
{
    public class GeminiSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com";
        public string Model { get; set; } = "gemini-3.6-flash";
        public int TimeoutSeconds { get; set; } = 120;
        public bool EnableOfflineMock { get; set; } = false;
    }
}
