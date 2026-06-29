using System;

namespace DuAnTotNghiep.DTOs.PlacementTest
{
    public class WrongAnswerDto
    {
        public int QuestionId { get; set; }
        public string Skill { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
    }
}
