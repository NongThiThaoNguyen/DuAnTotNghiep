using System;

namespace DuAnTotNghiep.Models.ViewModels.Teacher
{
    public class StudentChatSummaryViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = "";
        public string StudentAvatar { get; set; } = "";
        public int UnreadCount { get; set; }
        public string LastMessageText { get; set; } = "";
        public DateTime? LastMessageTime { get; set; }
    }
}
