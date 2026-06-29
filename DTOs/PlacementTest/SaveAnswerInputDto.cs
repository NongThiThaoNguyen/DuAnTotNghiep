using System;

namespace DuAnTotNghiep.DTOs.PlacementTest
{
    public class SaveAnswerInputDto
    {
        public int AttemptId { get; set; }
        public int QuestionId { get; set; }
        public string? AnswerValue { get; set; }
    }
}
