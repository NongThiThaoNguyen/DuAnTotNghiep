using System.Collections.Generic;
using DuAnTotNghiep.Enums;

namespace DuAnTotNghiep.DTOs
{
    public class GenerateQuestionRequestDto
    {
        public int SkillId { get; set; }
        public int TopicId { get; set; }
        public int ProficiencyLevelId { get; set; }
        public Difficulty Difficulty { get; set; }
        public QuestionType QuestionType { get; set; }
        public int QuestionCount { get; set; }
        public string? Notes { get; set; }
        public string? RequestedBy { get; set; }
    }
}
