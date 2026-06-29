using System.Collections.Generic;
using System.Threading.Tasks;
using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Models.Repositories.Interfaces
{
    public interface ICompetencyAnalysisRepository
    {
        // Lưu bảng cha + danh sách điểm kỹ năng (bảng con) trong 1 transaction
        Task<int> AddAnalysisWithScoresAsync(CompetencyAnalysis analysis, List<CompetencySkillScore> scores);

        // Kiểm tra topic tồn tại trong hệ thống trước khi gán FK
        Task<bool> ValidateTopicExistsAsync(int topicId);

        // Kiểm tra skill tồn tại trong hệ thống trước khi gán FK
        Task<bool> ValidateSkillExistsAsync(int skillId);
    }
}
