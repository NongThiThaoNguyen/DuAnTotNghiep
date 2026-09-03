using System;

namespace DuAnTotNghiep.Models.ViewModels.Student
{
    public class TeacherChatSummaryViewModel
    {
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = "";
        public string TeacherAvatar { get; set; } = "";
        public string TeacherEmail { get; set; } = "";
        public string? ClassName { get; set; }
        public int UnreadCount { get; set; }
        public string LastMessageText { get; set; } = "";
        public DateTime? LastMessageTime { get; set; }
    }
}
