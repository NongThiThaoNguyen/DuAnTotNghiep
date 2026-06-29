using DuAnTotNghiep.Models.DTOs.PlacementTestQuestion;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IPlacementTestQuestionService
    {
        Task AttachQuestionToSectionAsync(AttachQuestionDto dto);
        Task RemoveQuestionFromSectionAsync(int sectionId, int questionId);
        Task ReorderQuestionsAsync(int sectionId, List<QuestionOrderDto> items);
        Task<List<QuestionBankItemDto>> SearchAvailableQuestionsAsync(QuestionFilterDto filter);
        Task<List<PlacementTestQuestionDto>> GetSectionQuestionsAsync(int sectionId);
    }
}
