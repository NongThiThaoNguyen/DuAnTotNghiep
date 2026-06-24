namespace DuAnTotNghiep.Enums
{
    public static class QuestionDifficulty
    {
        public const string Basic = "BASIC";
        public const string Medium = "MEDIUM";
        public const string Advanced = "ADVANCED";

        public static readonly string[] All = { Basic, Medium, Advanced };

        public static bool IsValid(string difficulty)
        {
            return All.Contains(difficulty?.ToUpperInvariant());
        }
    }

    public static class QuestionType
    {
        public const string MCQ = "MCQ";
        public const string TrueFalse = "TRUE_FALSE";
        public const string ShortAnswer = "SHORT_ANSWER";
        public const string Listening = "LISTENING";

        public static readonly string[] All = { MCQ, TrueFalse, ShortAnswer, Listening };

        public static bool IsValid(string type)
        {
            return All.Contains(type?.ToUpperInvariant());
        }
    }
}
