namespace DuAnTotNghiep.Enums
{
    public static class ProgressStatus
    {
        public const string Locked = "LOCKED";
        public const string Available = "AVAILABLE";
        public const string InProgress = "IN_PROGRESS";
        public const string Completed = "COMPLETED";
        public const string NeedReview = "NEED_REVIEW";
        public const string Skipped = "SKIPPED";

        public static readonly string[] All = { Locked, Available, InProgress, Completed, NeedReview, Skipped };

        public static bool IsValid(string status)
        {
            if (string.IsNullOrWhiteSpace(status)) return false;
            return System.Array.Exists(All, s => s.Equals(status, System.StringComparison.OrdinalIgnoreCase));
        }
    }
}
