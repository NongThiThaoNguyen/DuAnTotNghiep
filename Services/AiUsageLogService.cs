using System.Threading;
using System.Threading.Tasks;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.DTOs;
using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Services
{
    public class AiUsageLogService
    {
        private readonly ApplicationDbContext _db;

        public AiUsageLogService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task LogRequestAsync(AiUsageLogDto dto, CancellationToken cancellationToken = default)
        {
            var entity = new AiUsageLog
            {
                UserId = dto.UserId,
                ModuleCode = dto.ModuleCode,
                PromptTemplateId = dto.PromptTemplateId,
                AiModel = dto.AiModel,
                InputTokens = dto.InputTokens,
                OutputTokens = dto.OutputTokens,
                CostEstimate = dto.CostEstimate,
                RequestStatus = dto.RequestStatus,
                ErrorMessage = dto.ErrorMessage,
                DurationMs = dto.DurationMs,
                CreatedAt = System.DateTime.UtcNow
            };

            _db.AiUsageLogs.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
