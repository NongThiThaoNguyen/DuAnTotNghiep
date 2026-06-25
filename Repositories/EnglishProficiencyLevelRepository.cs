using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Repositories
{
    public class EnglishProficiencyLevelRepository : GenericRepository<EnglishProficiencyLevel>, IEnglishProficiencyLevelRepository
    {
        public EnglishProficiencyLevelRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<EnglishProficiencyLevel>> GetActiveLevelsAsync()
        {
            return await _dbSet
                .Where(l => l.IsActive)
                .OrderBy(l => l.OrderIndex)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> ExistsCodeAsync(string code)
        {
            return await _dbSet.AnyAsync(l => l.Code.ToLower() == code.ToLower());
        }
    }
}
