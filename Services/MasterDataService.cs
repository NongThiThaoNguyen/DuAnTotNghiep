using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DuAnTotNghiep.Models.Enums;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.Repositories.Interfaces;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DuAnTotNghiep.Services
{
    public class MasterDataService : IMasterDataService
    {
        private readonly IGenericRepository<EnglishSkill> _skillRepository;
        private readonly IGenericRepository<EnglishProficiencyLevel> _levelRepository;

        public MasterDataService(
            IGenericRepository<EnglishSkill> skillRepository,
            IGenericRepository<EnglishProficiencyLevel> levelRepository)
        {
            _skillRepository = skillRepository;
            _levelRepository = levelRepository;
        }

        public async Task<IEnumerable<EnglishSkill>> GetActiveSkillsAsync()
        {
            var allSkills = await _skillRepository.GetAllAsync();
            return allSkills.Where(s => s.IsActive).OrderBy(s => s.OrderIndex);
        }

        public async Task<IEnumerable<EnglishProficiencyLevel>> GetActiveLevelsAsync()
        {
            var allLevels = await _levelRepository.GetAllAsync();
            return allLevels.OrderBy(l => l.OrderIndex);
        }

        public IEnumerable<SelectListItem> GetDifficulties()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = QuestionDifficulty.Basic, Text = "Cơ bản" },
                new SelectListItem { Value = QuestionDifficulty.Medium, Text = "Trung bình" },
                new SelectListItem { Value = QuestionDifficulty.Advanced, Text = "Nâng cao" }
            };
        }

        public IEnumerable<SelectListItem> GetQuestionTypes()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = QuestionTypeConstants.MCQ, Text = "Trắc nghiệm nhiều lựa chọn" },
                new SelectListItem { Value = QuestionTypeConstants.TrueFalse, Text = "Đúng/Sai" },
                new SelectListItem { Value = QuestionTypeConstants.ShortAnswer, Text = "Câu trả lời ngắn" },
                new SelectListItem { Value = QuestionTypeConstants.Listening, Text = "Nghe hiểu" }
            };
        }

        public async Task<SelectList> GetSkillsSelectListAsync(int? selectedValue = null)
        {
            var skills = await GetActiveSkillsAsync();
            return new SelectList(skills, "Id", "SkillName", selectedValue);
        }

        public async Task<SelectList> GetLevelsSelectListAsync(int? selectedValue = null)
        {
            var levels = await GetActiveLevelsAsync();
            return new SelectList(levels, "Id", "Name", selectedValue);
        }
    }
}
