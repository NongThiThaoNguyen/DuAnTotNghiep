using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DuAnTotNghiep.Repositories
{
    public class CompetencyAnalysisRepository : ICompetencyAnalysisRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<CompetencyAnalysisRepository> _log;

        public CompetencyAnalysisRepository(ApplicationDbContext db, ILogger<CompetencyAnalysisRepository> log)
        {
            _db = db;
            _log = log;
        }

        public async Task<int> AddAnalysisWithScoresAsync(CompetencyAnalysis analysis, List<CompetencySkillScore> scores)
        {
            // Bắt đầu transaction để bọc cả 2 thao tác ghi (bảng cha + bảng con)
            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                // Bước 1: Lưu bảng cha (competency_analyses)
                // Sau SaveChangesAsync() EF sẽ populate analysis.Id từ SQL Server identity
                _db.CompetencyAnalyses.Add(analysis);
                await _db.SaveChangesAsync();

                // Bước 2: Gán FK bảng con từ Id vừa sinh
                foreach (var s in scores)
                    s.CompetencyAnalysisId = analysis.Id;

                // Bước 3: Lưu bảng con (competency_skill_scores)
                _db.CompetencySkillScores.AddRange(scores);
                await _db.SaveChangesAsync();

                // Commit nếu cả 2 bước đều thành công
                await tx.CommitAsync();

                _log.LogInformation("Persisted CompetencyAnalysis {Id} with {Count} skill scores.", analysis.Id, scores.Count);
                return analysis.Id;
            }
            catch (Exception ex)
            {
                // Rollback để tránh dữ liệu rác mồ côi
                await tx.RollbackAsync();
                _log.LogError(ex, "Transaction rolled back. Failed to persist CompetencyAnalysis.");
                throw;
            }
        }

        public async Task<bool> ValidateTopicExistsAsync(int topicId)
        {
            return await _db.LearningTopics.AnyAsync(t => t.Id == topicId);
        }

        public async Task<bool> ValidateSkillExistsAsync(int skillId)
        {
            return await _db.EnglishSkills.AnyAsync(s => s.Id == skillId);
        }
    }
}
