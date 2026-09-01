using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models.DTOs.PlacementTestQuestion;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services
{
    public class PlacementTestQuestionService : IPlacementTestQuestionService
    {
        private readonly ApplicationDbContext _dbContext;

        public PlacementTestQuestionService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AttachQuestionToSectionAsync(AttachQuestionDto dto)
        {
            var section = await _dbContext.PlacementTestSections
                .Include(s => s.PlacementTest)
                .FirstOrDefaultAsync(s => s.Id == dto.SectionId);

            if (section == null) throw new InvalidOperationException("Không tìm thấy Section.");
            if (section.PlacementTest.Status == "ARCHIVED") throw new InvalidOperationException("Không thể sửa đổi Section của bài thi đã lưu trữ.");

            var question = await _dbContext.QuestionBanks.FindAsync(dto.QuestionId);
            if (question == null) throw new InvalidOperationException("Câu hỏi không tồn tại.");
            if (question.ReviewStatus != "APPROVED") throw new InvalidOperationException("Chỉ được sử dụng câu hỏi đã APPROVED.");

            if (question.SkillId != section.SkillId)
                throw new InvalidOperationException("Câu hỏi phải có cùng Kỹ năng (Skill) với Section.");

            var isDuplicate = await _dbContext.PlacementTestQuestions
                .AnyAsync(q => q.SectionId == dto.SectionId && q.QuestionId == dto.QuestionId);
            if (isDuplicate) throw new InvalidOperationException("Câu hỏi đã tồn tại trong Section này.");

            var newOrderIndex = dto.OrderIndex;
            if (newOrderIndex <= 0)
            {
                var maxOrder = await _dbContext.PlacementTestQuestions
                    .Where(q => q.SectionId == dto.SectionId)
                    .MaxAsync(q => (int?)q.OrderIndex) ?? 0;
                newOrderIndex = maxOrder + 1;
            }

            var duplicateOrder = await _dbContext.PlacementTestQuestions
                .AnyAsync(q => q.SectionId == dto.SectionId && q.OrderIndex == newOrderIndex);
            if (duplicateOrder) throw new InvalidOperationException("Thứ tự (OrderIndex) bị trùng.");

            var testQuestion = new PlacementTestQuestion
            {
                SectionId = dto.SectionId,
                QuestionId = dto.QuestionId,
                Points = dto.Points,
                OrderIndex = newOrderIndex
            };

            _dbContext.PlacementTestQuestions.Add(testQuestion);
            await _dbContext.SaveChangesAsync();

            await RecalculateSectionScoreAsync(dto.SectionId);
        }

        public async Task RemoveQuestionFromSectionAsync(int sectionId, int questionId)
        {
            var section = await _dbContext.PlacementTestSections
                .Include(s => s.PlacementTest)
                .FirstOrDefaultAsync(s => s.Id == sectionId);

            if (section == null) throw new InvalidOperationException("Không tìm thấy Section.");
            if (section.PlacementTest.Status == "ARCHIVED") throw new InvalidOperationException("Không thể sửa đổi Section của bài thi đã lưu trữ.");

            var hasAttempt = await _dbContext.TestAttempts.AnyAsync(a => a.PlacementTestId == section.PlacementTestId);
            if (hasAttempt) throw new InvalidOperationException("Bài thi đã có dữ liệu làm bài. Việc gỡ câu hỏi bị khóa để bảo vệ lịch sử.");

            var testQuestion = await _dbContext.PlacementTestQuestions
                .FirstOrDefaultAsync(q => q.SectionId == sectionId && q.QuestionId == questionId);

            if (testQuestion == null) throw new InvalidOperationException("Không tìm thấy câu hỏi trong Section này.");

            _dbContext.PlacementTestQuestions.Remove(testQuestion);
            await _dbContext.SaveChangesAsync();

            await RecalculateSectionScoreAsync(sectionId);
        }

        public async Task ReorderQuestionsAsync(int sectionId, List<QuestionOrderDto> items)
        {
            var section = await _dbContext.PlacementTestSections
                .Include(s => s.PlacementTest)
                .FirstOrDefaultAsync(s => s.Id == sectionId);

            if (section == null) throw new InvalidOperationException("Không tìm thấy Section.");
            if (section.PlacementTest.Status == "ARCHIVED") throw new InvalidOperationException("Không thể sửa đổi Section của bài thi đã lưu trữ.");

            var existingQuestions = await _dbContext.PlacementTestQuestions
                .Where(q => q.SectionId == sectionId)
                .ToListAsync();

            foreach (var item in items)
            {
                var target = existingQuestions.FirstOrDefault(q => q.QuestionId == item.QuestionId);
                if (target != null)
                {
                    target.OrderIndex = item.OrderIndex;
                }
            }

            var duplicateOrder = existingQuestions.GroupBy(x => x.OrderIndex).Any(g => g.Count() > 1);
            if (duplicateOrder) throw new InvalidOperationException("Thứ tự (OrderIndex) bị trùng lặp.");

            _dbContext.PlacementTestQuestions.UpdateRange(existingQuestions);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<QuestionBankItemDto>> SearchAvailableQuestionsAsync(QuestionFilterDto filter)
        {
            var section = await _dbContext.PlacementTestSections.FindAsync(filter.SectionId);
            if (section == null) throw new InvalidOperationException("Không tìm thấy Section.");

            var attachedQuestionIds = await _dbContext.PlacementTestQuestions
                .Where(q => q.SectionId == filter.SectionId)
                .Select(q => q.QuestionId)
                .ToListAsync();

            var query = _dbContext.QuestionBanks
                .Include(q => q.Skill)
                .Include(q => q.Topic)
                .Where(q => q.ReviewStatus == "APPROVED" && q.SkillId == section.SkillId && !attachedQuestionIds.Contains(q.Id));

            if (!string.IsNullOrEmpty(filter.Keyword))
            {
                query = query.Where(q => q.QuestionText.Contains(filter.Keyword));
            }

            if (!string.IsNullOrEmpty(filter.QuestionType))
            {
                query = query.Where(q => q.QuestionType == filter.QuestionType);
            }

            if (!string.IsNullOrEmpty(filter.DifficultyLevel))
            {
                query = query.Where(q => q.DifficultyLevel == filter.DifficultyLevel);
            }

            return await query.Select(q => new QuestionBankItemDto
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                QuestionType = q.QuestionType,
                DifficultyLevel = q.DifficultyLevel,
                SkillName = q.Skill.SkillName,
                TopicName = q.Topic != null ? q.Topic.Title : null
            }).ToListAsync();
        }

        public async Task<List<PlacementTestQuestionDto>> GetSectionQuestionsAsync(int sectionId)
        {
            return await _dbContext.PlacementTestQuestions
                .Where(q => q.SectionId == sectionId)
                .Include(q => q.Question)
                .ThenInclude(qb => qb.Skill)
                .OrderBy(q => q.OrderIndex)
                .Select(q => new PlacementTestQuestionDto
                {
                    Id = q.Id,
                    SectionId = q.SectionId,
                    QuestionId = q.QuestionId,
                    Points = q.Points,
                    OrderIndex = q.OrderIndex,
                    QuestionText = q.Question.QuestionText,
                    QuestionType = q.Question.QuestionType,
                    DifficultyLevel = q.Question.DifficultyLevel,
                    SkillName = q.Question.Skill.SkillName
                })
                .ToListAsync();
        }

        public async Task<byte[]> GenerateQuestionExcelTemplateAsync()
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Mau_Import_Cau_Hoi");

            var headers = new[]
            {
                "Nội dung câu hỏi (*)",
                "Loại câu hỏi (MULTIPLE_CHOICE / FILL_BLANK)",
                "Độ khó (BASIC / MEDIUM / ADVANCED)",
                "Điểm (*)",
                "Đáp án đúng (*)",
                "Lựa chọn A",
                "Lựa chọn B",
                "Lựa chọn C",
                "Lựa chọn D",
                "Giải thích"
            };

            for (int col = 0; col < headers.Length; col++)
            {
                var cell = worksheet.Cell(1, col + 1);
                cell.Value = headers[col];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#1E293B");
                cell.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            }

            // Sample Row 1: Multiple Choice
            worksheet.Cell(2, 1).Value = "What _____ your name?";
            worksheet.Cell(2, 2).Value = "MULTIPLE_CHOICE";
            worksheet.Cell(2, 3).Value = "BASIC";
            worksheet.Cell(2, 4).Value = 1.0;
            worksheet.Cell(2, 5).Value = "A";
            worksheet.Cell(2, 6).Value = "is";
            worksheet.Cell(2, 7).Value = "are";
            worksheet.Cell(2, 8).Value = "am";
            worksheet.Cell(2, 9).Value = "be";
            worksheet.Cell(2, 10).Value = "Dùng 'is' cho chủ ngữ số ít 'your name'.";

            // Sample Row 2: Fill Blank
            worksheet.Cell(3, 1).Value = "She _____ (go) to school yesterday.";
            worksheet.Cell(3, 2).Value = "FILL_BLANK";
            worksheet.Cell(3, 3).Value = "MEDIUM";
            worksheet.Cell(3, 4).Value = 1.5;
            worksheet.Cell(3, 5).Value = "went";
            worksheet.Cell(3, 6).Value = "";
            worksheet.Cell(3, 7).Value = "";
            worksheet.Cell(3, 8).Value = "";
            worksheet.Cell(3, 9).Value = "";
            worksheet.Cell(3, 10).Value = "Thì quá khứ đơn của 'go' là 'went'.";

            worksheet.Columns().AdjustToContents();

            using var stream = new System.IO.MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<QuestionExcelPreviewResultDto> PreviewQuestionsExcelAsync(int sectionId, System.IO.Stream excelStream)
        {
            var section = await _dbContext.PlacementTestSections
                .Include(s => s.PlacementTest)
                .FirstOrDefaultAsync(s => s.Id == sectionId);

            if (section == null) throw new InvalidOperationException("Không tìm thấy Section.");

            using var workbook = new ClosedXML.Excel.XLWorkbook(excelStream);
            var worksheet = workbook.Worksheets.FirstOrDefault();
            if (worksheet == null) throw new InvalidOperationException("File Excel rỗng hoặc không đúng định dạng.");

            var result = new QuestionExcelPreviewResultDto
            {
                SectionId = sectionId,
                SectionName = section.SectionName
            };

            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;

            for (int rowNum = 2; rowNum <= lastRow; rowNum++)
            {
                var row = worksheet.Row(rowNum);

                var qText = row.Cell(1).GetString().Trim();
                var qType = row.Cell(2).GetString().Trim().ToUpper();
                var diff = row.Cell(3).GetString().Trim().ToUpper();
                var pointsStr = row.Cell(4).GetString().Trim();
                var cAnswer = row.Cell(5).GetString().Trim();
                var optA = row.Cell(6).GetString().Trim();
                var optB = row.Cell(7).GetString().Trim();
                var optC = row.Cell(8).GetString().Trim();
                var optD = row.Cell(9).GetString().Trim();
                var expl = row.Cell(10).GetString().Trim();

                if (string.IsNullOrWhiteSpace(qText) && string.IsNullOrWhiteSpace(qType) && string.IsNullOrWhiteSpace(cAnswer))
                {
                    continue;
                }

                var item = new QuestionExcelRowDto
                {
                    RowNumber = rowNum,
                    QuestionText = qText,
                    QuestionType = string.IsNullOrWhiteSpace(qType) ? "MULTIPLE_CHOICE" : qType,
                    DifficultyLevel = string.IsNullOrWhiteSpace(diff) ? "MEDIUM" : diff,
                    CorrectAnswer = cAnswer,
                    OptionA = optA,
                    OptionB = optB,
                    OptionC = optC,
                    OptionD = optD,
                    Explanation = expl
                };

                if (string.IsNullOrWhiteSpace(qText))
                {
                    item.Errors.Add($"Dòng {rowNum}: Nội dung câu hỏi không được để trống.");
                }

                if (item.DifficultyLevel == "EASY") item.DifficultyLevel = "BASIC";
                if (item.DifficultyLevel == "HARD") item.DifficultyLevel = "ADVANCED";
                if (item.DifficultyLevel != "BASIC" && item.DifficultyLevel != "MEDIUM" && item.DifficultyLevel != "ADVANCED")
                {
                    item.Errors.Add($"Dòng {rowNum}: Độ khó không hợp lệ ({item.DifficultyLevel}). Phải là BASIC, MEDIUM hoặc ADVANCED.");
                }

                if (decimal.TryParse(pointsStr, out var pts) && pts > 0)
                {
                    item.Points = pts;
                }
                else if (row.Cell(4).DataType == ClosedXML.Excel.XLDataType.Number)
                {
                    item.Points = (decimal)row.Cell(4).GetDouble();
                }
                else if (string.IsNullOrWhiteSpace(pointsStr))
                {
                    item.Points = 1.0m;
                }
                else
                {
                    item.Errors.Add($"Dòng {rowNum}: Điểm không hợp lệ ({pointsStr}). Phải là số lớn hơn 0.");
                }

                if (item.QuestionType != "MULTIPLE_CHOICE" && item.QuestionType != "FILL_BLANK")
                {
                    item.Errors.Add($"Dòng {rowNum}: Loại câu hỏi không hợp lệ ({item.QuestionType}). Phải là MULTIPLE_CHOICE hoặc FILL_BLANK.");
                }

                if (string.IsNullOrWhiteSpace(cAnswer))
                {
                    item.Errors.Add($"Dòng {rowNum}: Đáp án đúng không được để trống.");
                }

                if (item.QuestionType == "MULTIPLE_CHOICE")
                {
                    if (string.IsNullOrWhiteSpace(optA) || string.IsNullOrWhiteSpace(optB))
                    {
                        item.Errors.Add($"Dòng {rowNum}: Câu hỏi trắc nghiệm phải có tối thiểu 2 lựa chọn (A và B).");
                    }
                }

                result.Rows.Add(item);
            }

            result.TotalRows = result.Rows.Count;
            result.ValidCount = result.Rows.Count(r => r.IsValid);
            result.ErrorCount = result.Rows.Count(r => !r.IsValid);

            return result;
        }

        public async Task<int> ImportQuestionsFromExcelAsync(int sectionId, List<QuestionExcelRowDto> validRows)
        {
            var section = await _dbContext.PlacementTestSections
                .Include(s => s.PlacementTest)
                .FirstOrDefaultAsync(s => s.Id == sectionId);

            if (section == null) throw new InvalidOperationException("Không tìm thấy Section.");
            if (section.PlacementTest.Status == "ARCHIVED") throw new InvalidOperationException("Không thể sửa đổi Section của bài thi đã lưu trữ.");

            if (validRows == null || !validRows.Any()) throw new InvalidOperationException("Không có dữ liệu câu hỏi hợp lệ để import.");

            var maxOrder = await _dbContext.PlacementTestQuestions
                .Where(q => q.SectionId == sectionId)
                .MaxAsync(q => (int?)q.OrderIndex) ?? 0;

            int importedCount = 0;

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                foreach (var row in validRows)
                {
                    maxOrder++;

                    var qb = new QuestionBank
                    {
                        QuestionText = row.QuestionText,
                        QuestionType = row.QuestionType,
                        DifficultyLevel = row.DifficultyLevel,
                        CorrectAnswer = row.CorrectAnswer,
                        Explanation = row.Explanation,
                        SkillId = section.SkillId,
                        ReviewStatus = "APPROVED",
                        SourceType = "EXCEL_IMPORT",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _dbContext.QuestionBanks.Add(qb);
                    await _dbContext.SaveChangesAsync();

                    if (row.QuestionType == "MULTIPLE_CHOICE")
                    {
                        var opts = new List<(string Text, string Code)>
                        {
                            (row.OptionA ?? "", "A"),
                            (row.OptionB ?? "", "B"),
                            (row.OptionC ?? "", "C"),
                            (row.OptionD ?? "", "D")
                        };

                        int optOrder = 1;
                        foreach (var opt in opts)
                        {
                            if (string.IsNullOrWhiteSpace(opt.Text)) continue;

                            var isCorrect = string.Equals(row.CorrectAnswer, opt.Code, StringComparison.OrdinalIgnoreCase) ||
                                            string.Equals(row.CorrectAnswer, opt.Text, StringComparison.OrdinalIgnoreCase);

                            var qOpt = new QuestionOption
                            {
                                QuestionId = qb.Id,
                                OptionText = opt.Text.Trim(),
                                IsCorrect = isCorrect,
                                OrderIndex = optOrder++
                            };
                            _dbContext.QuestionOptions.Add(qOpt);
                        }
                        await _dbContext.SaveChangesAsync();
                    }

                    var ptq = new PlacementTestQuestion
                    {
                        SectionId = sectionId,
                        QuestionId = qb.Id,
                        Points = row.Points > 0 ? row.Points : 1.0m,
                        OrderIndex = maxOrder
                    };

                    _dbContext.PlacementTestQuestions.Add(ptq);
                    await _dbContext.SaveChangesAsync();

                    importedCount++;
                }

                await transaction.CommitAsync();

                await RecalculateSectionScoreAsync(sectionId);

                return importedCount;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task CreateAndAttachQuestionAsync(CreateAndAttachQuestionDto dto)
        {
            var section = await _dbContext.PlacementTestSections
                .Include(s => s.PlacementTest)
                .FirstOrDefaultAsync(s => s.Id == dto.SectionId);

            if (section == null) throw new InvalidOperationException("Không tìm thấy Section.");
            if (section.PlacementTest.Status == "ARCHIVED") throw new InvalidOperationException("Không thể sửa đổi Section của bài thi đã lưu trữ.");

            if (string.IsNullOrWhiteSpace(dto.QuestionText))
                throw new InvalidOperationException("Nội dung câu hỏi không được để trống.");

            var maxOrder = await _dbContext.PlacementTestQuestions
                .Where(q => q.SectionId == dto.SectionId)
                .MaxAsync(q => (int?)q.OrderIndex) ?? 0;

            var qb = new QuestionBank
            {
                QuestionText = dto.QuestionText.Trim(),
                QuestionType = string.IsNullOrWhiteSpace(dto.QuestionType) ? "MULTIPLE_CHOICE" : dto.QuestionType.Trim(),
                DifficultyLevel = string.IsNullOrWhiteSpace(dto.DifficultyLevel) ? "MEDIUM" : dto.DifficultyLevel.Trim(),
                CorrectAnswer = dto.CorrectAnswer?.Trim() ?? string.Empty,
                Explanation = dto.Explanation?.Trim(),
                SkillId = section.SkillId,
                ReviewStatus = "APPROVED",
                SourceType = "MANUAL_ADMIN",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.QuestionBanks.Add(qb);
            await _dbContext.SaveChangesAsync();

            if (qb.QuestionType == "MULTIPLE_CHOICE")
            {
                var opts = new List<(string Text, string Code)>
                {
                    (dto.OptionA ?? "", "A"),
                    (dto.OptionB ?? "", "B"),
                    (dto.OptionC ?? "", "C"),
                    (dto.OptionD ?? "", "D")
                };

                int optOrder = 1;
                foreach (var opt in opts)
                {
                    if (string.IsNullOrWhiteSpace(opt.Text)) continue;

                    var isCorrect = string.Equals(dto.CorrectAnswer, opt.Code, StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(dto.CorrectAnswer, opt.Text, StringComparison.OrdinalIgnoreCase);

                    var qOpt = new QuestionOption
                    {
                        QuestionId = qb.Id,
                        OptionText = opt.Text.Trim(),
                        IsCorrect = isCorrect,
                        OrderIndex = optOrder++
                    };
                    _dbContext.QuestionOptions.Add(qOpt);
                }
                await _dbContext.SaveChangesAsync();
            }

            var ptq = new PlacementTestQuestion
            {
                SectionId = dto.SectionId,
                QuestionId = qb.Id,
                Points = dto.Points > 0 ? dto.Points : 1.0m,
                OrderIndex = maxOrder + 1
            };

            _dbContext.PlacementTestQuestions.Add(ptq);
            await _dbContext.SaveChangesAsync();

            await RecalculateSectionScoreAsync(dto.SectionId);
        }

        private async Task RecalculateSectionScoreAsync(int sectionId)
        {
            var totalPoints = await _dbContext.PlacementTestQuestions
                .Where(q => q.SectionId == sectionId)
                .SumAsync(q => q.Points);

            var section = await _dbContext.PlacementTestSections
                .Include(s => s.PlacementTest)
                .FirstOrDefaultAsync(s => s.Id == sectionId);

            if (section != null)
            {
                section.MaxScore = totalPoints;
                _dbContext.PlacementTestSections.Update(section);

                var testTotalScore = await _dbContext.PlacementTestSections
                    .Where(s => s.PlacementTestId == section.PlacementTestId)
                    .SumAsync(s => s.MaxScore);
                    
                section.PlacementTest.TotalScore = testTotalScore;
                _dbContext.PlacementTests.Update(section.PlacementTest);

                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
