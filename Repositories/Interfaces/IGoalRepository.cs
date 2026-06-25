using System.Collections.Generic;
using System.Threading.Tasks;
using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Repositories.Interfaces
{
    public interface IGoalRepository : IGenericRepository<LearningGoal>
    {
        Task<IEnumerable<LearningGoal>> GetActiveGoalsAsync();
    }
}
