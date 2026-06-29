using DuAnTotNghiep.Models.DTOs;
using DuAnTotNghiep.Models.ViewModels;

namespace DuAnTotNghiep.Services.Mapping
{
    public static class GenerationMapper
    {
        public static GenerateQuestionRequestDto ToDto(GenerateQuizRequestViewModel vm, string? requestedBy = null)
        {
            if (vm == null) return new GenerateQuestionRequestDto();
            return new GenerateQuestionRequestDto
            {
                SkillId = vm.SkillId,
                TopicId = vm.TopicId,
                ProficiencyLevelId = vm.ProficiencyLevelId,
                Difficulty = vm.Difficulty,
                QuestionType = vm.QuestionType,
                QuestionCount = vm.QuestionCount,
                Notes = vm.Notes,
                RequestedBy = requestedBy
            };
        }
    }
}
