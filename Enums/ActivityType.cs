namespace DuAnTotNghiep.Enums
{
    public static class ActivityType
    {
        public const string Login = "LOGIN";
        public const string Learn = "LEARN";
        public const string Quiz = "QUIZ";
        public const string Practice = "PRACTICE";
        public const string Chat = "CHAT";
        public const string Review = "REVIEW";
        public const string Test = "TEST";
        public const string FeedbackView = "FEEDBACK_VIEW";

        public static readonly string[] All = { Login, Learn, Quiz, Practice, Chat, Review, Test, FeedbackView };

        public static bool IsValid(string type)
        {
            if (string.IsNullOrWhiteSpace(type)) return false;
            return System.Array.Exists(All, t => t.Equals(type, System.StringComparison.OrdinalIgnoreCase));
        }
    }
}
