namespace DuAnTotNghiep.Models.Enums
{
    public static class NodeType
    {
        public const string Topic = "TOPIC";
        public const string Lesson = "LESSON";
        public const string Quiz = "QUIZ";
        public const string Practice = "PRACTICE";
        public const string Review = "REVIEW";
        public const string AiTutor = "AI_TUTOR";

        public static readonly string[] All = { Topic, Lesson, Quiz, Practice, Review, AiTutor };

        public static bool IsValid(string type)
        {
            if (string.IsNullOrWhiteSpace(type)) return false;
            return System.Array.Exists(All, t => t.Equals(type, System.StringComparison.OrdinalIgnoreCase));
        }
    }
}
