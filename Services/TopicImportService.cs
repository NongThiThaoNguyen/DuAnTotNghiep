using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using DuAnTotNghiep.Models.DTOs.TopicImport;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.Repositories.Interfaces;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DuAnTotNghiep.Data;
using Microsoft.AspNetCore.Http;

namespace DuAnTotNghiep.Services
{
    public class TopicImportService : ITopicImportService
    {
        private readonly ILearningTopicRepository _topicRepository;
        private readonly IEnglishSkillRepository _skillRepository;
        private readonly IEnglishProficiencyLevelRepository _levelRepository;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<TopicImportService> _logger;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TopicImportService(
            ILearningTopicRepository topicRepository,
            IEnglishSkillRepository skillRepository,
            IEnglishProficiencyLevelRepository levelRepository,
            ApplicationDbContext dbContext,
            ILogger<TopicImportService> logger,
            IAuditService auditService,
            IHttpContextAccessor httpContextAccessor)
        {
            _topicRepository = topicRepository;
            _skillRepository = skillRepository;
            _levelRepository = levelRepository;
            _dbContext = dbContext;
            _logger = logger;
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
        }

        // ---------------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------------
        public async Task<TopicImportPreviewDto> PreviewImportAsync(byte[] fileBytes, string fileName)
        {
            var rows = await ParseFileAsync(fileBytes, fileName);
            var validationResult = await ValidateRowsAsync(rows);
            return new TopicImportPreviewDto
            {
                TotalRows = rows.Count,
                ValidRows = validationResult.ValidRows.Count,
                ErrorRows = validationResult.Errors.Count,
                Errors = validationResult.Errors,
                ValidRowsData = validationResult.ValidRows
            };
        }

        public async Task ValidateImportFileAsync(byte[] fileBytes, string fileName)
        {
            var rows = await ParseFileAsync(fileBytes, fileName);
            var result = await ValidateRowsAsync(rows);
            if (result.Errors.Any())
            {
                var messages = string.Join("; ", result.Errors.Select(e => $"Row {e.RowNumber} - {e.ColumnName}: {e.Message}"));
                throw new InvalidOperationException($"Import validation failed: {messages}");
            }
        }

        public async Task ImportTopicsAsync(byte[] fileBytes, string fileName)
        {
            // Full validation first (preview step) to guarantee atomic import
            var rows = await ParseFileAsync(fileBytes, fileName);
            var validation = await ValidateRowsAsync(rows);
            if (validation.Errors.Any())
                throw new InvalidOperationException("Import contains validation errors; aborting.");

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // Separate create / update based on existence
                var existingTopics = await _topicRepository.GetAllAsync();
                var existingByCode = existingTopics.ToDictionary(t => t.TopicCode.ToLower(), t => t);

                var toCreate = new List<LearningTopic>();
                var toUpdate = new List<LearningTopic>();

                foreach (var row in validation.ValidRows)
                {
                    var skill = await _skillRepository.GetByCodeAsync(row.SkillCode.Trim());
                    var level = await _levelRepository.GetByCodeAsync(row.LevelCode.Trim());
                    var parent = string.IsNullOrWhiteSpace(row.ParentCode)
                        ? null
                        : existingByCode.TryGetValue(row.ParentCode.Trim().ToLower(), out var p) ? p : null;

                    var topicCodeKey = row.TopicCode.Trim().ToLower();
                    if (existingByCode.TryGetValue(topicCodeKey, out var existing))
                    {
                        // Update (only allowed fields, never overwrite Active without confirmation)
                        if (existing.Status == "Active")
                        {
                            // Skip update – will be reported as skipped
                            continue;
                        }
                        existing.Title = row.Title.Trim();
                        existing.Description = row.Description?.Trim();
                        existing.SkillId = skill.Id;
                        existing.LevelId = level?.Id;
                        existing.ParentTopicId = parent?.Id;
                        existing.DifficultyLevel = row.Difficulty.Trim().ToUpper();
                        existing.EstimatedMinutes = row.EstimatedMinutes;
                        existing.OrderIndex = row.OrderIndex;
                        existing.Status = row.Status.Trim();
                        existing.UpdatedAt = DateTime.UtcNow;
                        toUpdate.Add(existing);
                    }
                    else
                    {
                        var newTopic = new LearningTopic
                        {
                            TopicCode = row.TopicCode.Trim(),
                            Title = row.Title.Trim(),
                            Description = row.Description?.Trim(),
                            SkillId = skill.Id,
                            LevelId = level?.Id,
                            ParentTopicId = parent?.Id,
                            DifficultyLevel = row.Difficulty.Trim().ToUpper(),
                            EstimatedMinutes = row.EstimatedMinutes,
                            OrderIndex = row.OrderIndex,
                            Status = row.Status.Trim(),
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        toCreate.Add(newTopic);
                    }
                }

                // Bulk insert / update
                if (toCreate.Any())
                    await _topicRepository.AddRangeAsync(toCreate);
                if (toUpdate.Any())
                    await _topicRepository.UpdateRangeAsync(toUpdate);

                await _topicRepository.SaveChangesAsync();
                await transaction.CommitAsync();

                // Audit log – optional if IAuditService exists
                await _auditService?.LogAsync(GetCurrentUserId(), "Import Topics", "LearningTopic", null, null, $"Created: {toCreate.Count}, Updated: {toUpdate.Count}, Skipped: {validation.ValidRows.Count - toCreate.Count - toUpdate.Count}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error during Topic import, transaction rolled back.");
                throw;
            }
        }

        public async Task SeedDefaultTopicsAsync()
        {
            var demoRows = GetDemoRows();
            // Reuse validation logic to avoid duplicates
            var validation = await ValidateRowsAsync(demoRows);
            // Only create rows that are not already present
            var existing = await _topicRepository.GetAllAsync();
            var existingCodes = new HashSet<string>(existing.Select(t => t.TopicCode.ToLower()));
            var rowsToCreate = validation.ValidRows.Where(r => !existingCodes.Contains(r.TopicCode.Trim().ToLower())).ToList();
            if (!rowsToCreate.Any())
                return; // nothing to seed

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var toCreate = new List<LearningTopic>();
                foreach (var row in rowsToCreate)
                {
                    var skill = await _skillRepository.GetByCodeAsync(row.SkillCode.Trim());
                    var level = await _levelRepository.GetByCodeAsync(row.LevelCode.Trim());
                    var newTopic = new LearningTopic
                    {
                        TopicCode = row.TopicCode.Trim(),
                        Title = row.Title.Trim(),
                        Description = row.Description?.Trim(),
                        SkillId = skill.Id,
                        LevelId = level?.Id,
                        ParentTopicId = null,
                        DifficultyLevel = row.Difficulty.Trim().ToUpper(),
                        EstimatedMinutes = row.EstimatedMinutes,
                        OrderIndex = row.OrderIndex,
                        Status = row.Status.Trim(),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    toCreate.Add(newTopic);
                }

                await _topicRepository.AddRangeAsync(toCreate);
                await _topicRepository.SaveChangesAsync();
                await transaction.CommitAsync();

                await _auditService?.LogAsync(GetCurrentUserId(), "Seed Demo Topics", "LearningTopic", null, null, $"Created: {toCreate.Count}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error seeding demo topics.");
                throw;
            }
        }

        // ---------------------------------------------------------------------
        // Private helpers
        // ---------------------------------------------------------------------
        private async Task<List<TopicImportRowDto>> ParseFileAsync(byte[] fileBytes, string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            if (ext == ".csv")
                return await ParseCsvAsync(fileBytes);
            if (ext == ".xlsx" || ext == ".xls")
                return await ParseExcelAsync(fileBytes);
            if (ext == ".json")
                return await ParseJsonAsync(fileBytes);
            throw new InvalidOperationException($"Unsupported file type: {ext}");
        }

        private async Task<List<TopicImportRowDto>> ParseCsvAsync(byte[] bytes)
        {
            var content = System.Text.Encoding.UTF8.GetString(bytes);
            var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (!lines.Any())
                throw new InvalidOperationException("CSV file is empty.");
            var header = lines[0].Split(',');
            var rows = new List<TopicImportRowDto>();
            for (int i = 1; i < lines.Length; i++)
            {
                var columns = lines[i].Split(',');
                var dict = header.Zip(columns, (h, c) => new { h, c })
                                 .ToDictionary(x => x.h.Trim().ToLower(), x => x.c.Trim());
                rows.Add(MapDictionaryToRow(dict));
            }
            return rows;
        }

        private async Task<List<TopicImportRowDto>> ParseExcelAsync(byte[] bytes)
        {
            using var ms = new MemoryStream(bytes);
            using var workbook = new XLWorkbook(ms);
            var worksheet = workbook.Worksheets.First();
            var rows = new List<TopicImportRowDto>();
            var headerRow = worksheet.Row(1);
            var headers = headerRow.Cells().Select(c => c.GetString().Trim().ToLower()).ToArray();
            foreach (var dataRow in worksheet.RowsUsed().Skip(1))
            {
                var dict = new Dictionary<string, string>();
                int idx = 0;
                foreach (var cell in dataRow.Cells(1, headers.Length))
                {
                    dict[headers[idx]] = cell.GetString().Trim();
                    idx++;
                }
                rows.Add(MapDictionaryToRow(dict));
            }
            return rows;
        }

        private async Task<List<TopicImportRowDto>> ParseJsonAsync(byte[] bytes)
        {
            var json = System.Text.Encoding.UTF8.GetString(bytes);
            var list = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(json);
            if (list == null)
                throw new InvalidOperationException("JSON content could not be parsed.");
            return list.Select(MapDictionaryToRow).ToList();
        }

        private TopicImportRowDto MapDictionaryToRow(Dictionary<string, string> dict)
        {
            string Get(string key) => dict.TryGetValue(key.ToLowerInvariant(), out var v) ? v : string.Empty;

            return new TopicImportRowDto
            {
                SkillCode = Get("skill_code"),
                LevelCode = Get("level_code"),
                ParentCode = string.IsNullOrWhiteSpace(Get("parent_code")) ? null : Get("parent_code"),
                TopicCode = Get("topic_code"),
                Title = Get("title"),
                Description = Get("description"),
                Difficulty = Get("difficulty"),
                EstimatedMinutes = int.TryParse(Get("estimated_minutes"), out var e) ? e : 0,
                OrderIndex = int.TryParse(Get("order_index"), out var o) ? o : 0,
                Status = string.IsNullOrWhiteSpace(Get("status")) ? "Active" : Get("status"),
                Objectives = Get("objectives")
            };
        }

        private async Task<(List<TopicImportErrorDto> Errors, List<TopicImportRowDto> ValidRows)> ValidateRowsAsync(List<TopicImportRowDto> rows)
        {
            var errors = new List<TopicImportErrorDto>();
            var validRows = new List<TopicImportRowDto>();

            // Load reference data once
            var skillDict = (await _skillRepository.GetAllAsync()).ToDictionary(s => s.SkillCode.ToLower(), s => s);
            var levelDict = (await _levelRepository.GetAllAsync()).ToDictionary(l => l.Code.ToLower(), l => l);
            var existingTopicCodes = (await _topicRepository.GetAllAsync())
                                         .Select(t => t.TopicCode.ToLower())
                                         .ToHashSet();

            var fileTopicCodes = new HashSet<string>();
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                int rowNum = i + 2; // header + 1‑based
                // SkillCode
                if (string.IsNullOrWhiteSpace(row.SkillCode) || !skillDict.ContainsKey(row.SkillCode.Trim().ToLower()))
                    errors.Add(new TopicImportErrorDto { RowNumber = rowNum, ColumnName = "SkillCode", Message = "Skill không tồn tại" });
                // LevelCode
                if (!string.IsNullOrWhiteSpace(row.LevelCode) && !levelDict.ContainsKey(row.LevelCode.Trim().ToLower()))
                    errors.Add(new TopicImportErrorDto { RowNumber = rowNum, ColumnName = "LevelCode", Message = "Level không tồn tại" });
                // TopicCode uniqueness in file
                var codeKey = row.TopicCode.Trim().ToLower();
                if (string.IsNullOrWhiteSpace(codeKey))
                    errors.Add(new TopicImportErrorDto { RowNumber = rowNum, ColumnName = "TopicCode", Message = "TopicCode rỗng" });
                else if (!fileTopicCodes.Add(codeKey))
                    errors.Add(new TopicImportErrorDto { RowNumber = rowNum, ColumnName = "TopicCode", Message = "TopicCode trùng trong file" });
                // TopicCode existence in DB
                if (existingTopicCodes.Contains(codeKey))
                    errors.Add(new TopicImportErrorDto { RowNumber = rowNum, ColumnName = "TopicCode", Message = "TopicCode đã tồn tại trong DB" });
                // Difficulty
                var diff = row.Difficulty?.Trim().ToUpper();
                var allowedDiff = new[] { "BEGINNER", "ELEMENTARY", "INTERMEDIATE", "UPPER_INTERMEDIATE", "ADVANCED" };
                if (!allowedDiff.Contains(diff))
                    errors.Add(new TopicImportErrorDto { RowNumber = rowNum, ColumnName = "Difficulty", Message = $"Difficulty phải là {string.Join(", ", allowedDiff)}" });
                // OrderIndex
                if (row.OrderIndex < 0)
                    errors.Add(new TopicImportErrorDto { RowNumber = rowNum, ColumnName = "OrderIndex", Message = "OrderIndex phải >= 0" });
                // ParentCode existence (if provided)
                if (!string.IsNullOrWhiteSpace(row.ParentCode))
                {
                    var parentKey = row.ParentCode.Trim().ToLower();
                    if (!existingTopicCodes.Contains(parentKey) && !fileTopicCodes.Contains(parentKey))
                        errors.Add(new TopicImportErrorDto { RowNumber = rowNum, ColumnName = "ParentCode", Message = "ParentCode không tồn tại" });
                }
                // Objectives JSON validation (optional)
                if (!string.IsNullOrWhiteSpace(row.Objectives))
                {
                    try
                    {
                        JsonConvert.DeserializeObject<object>(row.Objectives);
                    }
                    catch
                    {
                        errors.Add(new TopicImportErrorDto { RowNumber = rowNum, ColumnName = "Objectives", Message = "Objectives không phải JSON hợp lệ" });
                    }
                }

                if (!errors.Any(e => e.RowNumber == rowNum))
                    validRows.Add(row);
            }

            return (errors, validRows);
        }

        private List<TopicImportRowDto> GetDemoRows()
        {
            return new List<TopicImportRowDto>
            {
                new TopicImportRowDto { SkillCode = "VOCAB", LevelCode = "A1", ParentCode = null, TopicCode = "VOCAB_DAILY", Title = "Daily Life", Difficulty = "BEGINNER", EstimatedMinutes = 20, OrderIndex = 1, Status = "ACTIVE" },
                new TopicImportRowDto { SkillCode = "VOCAB", LevelCode = "A1", ParentCode = null, TopicCode = "VOCAB_FAMILY", Title = "Family", Difficulty = "BEGINNER", EstimatedMinutes = 20, OrderIndex = 2, Status = "ACTIVE" },
                new TopicImportRowDto { SkillCode = "VOCAB", LevelCode = "A1", ParentCode = null, TopicCode = "VOCAB_SCHOOL", Title = "School", Difficulty = "BEGINNER", EstimatedMinutes = 20, OrderIndex = 3, Status = "ACTIVE" },
                new TopicImportRowDto { SkillCode = "GRAMMAR", LevelCode = "A1", ParentCode = null, TopicCode = "GRAM_A1_PS", Title = "Present Simple", Difficulty = "BEGINNER", EstimatedMinutes = 30, OrderIndex = 1, Status = "ACTIVE" },
                new TopicImportRowDto { SkillCode = "GRAMMAR", LevelCode = "A1", ParentCode = null, TopicCode = "GRAM_A1_PC", Title = "Present Continuous", Difficulty = "BEGINNER", EstimatedMinutes = 30, OrderIndex = 2, Status = "ACTIVE" },
                new TopicImportRowDto { SkillCode = "GRAMMAR", LevelCode = "A1", ParentCode = null, TopicCode = "GRAM_A1_PSIMP", Title = "Past Simple", Difficulty = "BEGINNER", EstimatedMinutes = 30, OrderIndex = 3, Status = "ACTIVE" },
                new TopicImportRowDto { SkillCode = "READING", LevelCode = "A1", ParentCode = null, TopicCode = "READ_A1_SP", Title = "Short Passage", Difficulty = "BEGINNER", EstimatedMinutes = 25, OrderIndex = 1, Status = "ACTIVE" },
                new TopicImportRowDto { SkillCode = "READING", LevelCode = "A1", ParentCode = null, TopicCode = "READ_A1_MI", Title = "Main Idea", Difficulty = "BEGINNER", EstimatedMinutes = 25, OrderIndex = 2, Status = "ACTIVE" },
                new TopicImportRowDto { SkillCode = "LISTENING", LevelCode = "A1", ParentCode = null, TopicCode = "LISTEN_A1_BC", Title = "Basic Conversation", Difficulty = "BEGINNER", EstimatedMinutes = 15, OrderIndex = 1, Status = "ACTIVE" },
                new TopicImportRowDto { SkillCode = "LISTENING", LevelCode = "A1", ParentCode = null, TopicCode = "LISTEN_A1_DC", Title = "Daily Communication", Difficulty = "BEGINNER", EstimatedMinutes = 15, OrderIndex = 2, Status = "ACTIVE" }
            };
        }

        private int? GetCurrentUserId()
        {
            var userIdStr = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdStr, out int userId) ? userId : null;
        }
    }
}
