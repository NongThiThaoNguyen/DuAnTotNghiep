namespace DuAnTotNghiep.Models.Enums
{
    /// <summary>
    /// Defines string statuses used by student learning paths.
    /// </summary>
    public static class LearningPathStatus
    {
        public const string Active = "ACTIVE";
        public const string Archived = "ARCHIVED";
        public const string Paused = "PAUSED";
        public const string Failed = "FAILED";
        public const string Generating = "GENERATING";

        public static readonly string[] All = { Active, Archived, Paused, Failed, Generating };

        public static bool IsValid(string status)
        {
            if (string.IsNullOrWhiteSpace(status)) return false;
            return System.Array.Exists(All, s => s.Equals(status, System.StringComparison.OrdinalIgnoreCase));
        }
    }
}
