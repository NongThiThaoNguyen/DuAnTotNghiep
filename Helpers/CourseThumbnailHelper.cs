using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Helpers
{
    public static class CourseThumbnailHelper
    {
        public static string ResolveThumbnailUrl(string? title, string? topicCode, string? skillCode)
        {
            var t = title?.ToUpperInvariant() ?? "";
            var code = topicCode?.ToUpperInvariant() ?? "";
            var sCode = skillCode?.ToUpperInvariant() ?? "";

            if (code.Contains("IELTS") || t.Contains("IELTS")) return "/images/courses/ielts.png";
            if (code.Contains("TOEIC") || t.Contains("TOEIC")) return "/images/courses/toeic.png";
            if (sCode == "GRAMMAR" || t.Contains("NGỮ PHÁP") || t.Contains("THÌ") || t.Contains("TENSES") || t.Contains("PRESENT")) return "/images/courses/grammar.png";
            if (sCode == "VOCABULARY" || t.Contains("TỪ VỰNG") || t.Contains("FAMILY") || t.Contains("SCHOOL") || t.Contains("TRAVEL")) return "/images/courses/vocabulary.png";
            if (sCode == "COMMUNICATION" || sCode == "SPEAKING" || t.Contains("GIAO TIẾP") || t.Contains("SPEAKING") || t.Contains("NÓI")) return "/images/courses/communication.png";
            if (sCode == "LISTENING" || t.Contains("LISTENING") || t.Contains("NGHE")) return "/images/courses/listening.png";
            if (sCode == "READING" || t.Contains("READING") || t.Contains("ĐỌC")) return "/images/courses/reading.svg";
            if (sCode == "WRITING" || t.Contains("WRITING") || t.Contains("VIẾT")) return "/images/courses/writing.svg";

            return "/images/courses/default.svg";
        }

        public static string ResolveThumbnailUrl(LearningTopic? topic)
        {
            if (topic == null) return "/images/courses/default.svg";
            return ResolveThumbnailUrl(topic.Title, topic.TopicCode, topic.Skill?.SkillCode);
        }
    }
}
