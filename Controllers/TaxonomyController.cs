using System.Threading.Tasks;
using DuAnTotNghiep.Services;
using Microsoft.AspNetCore.Mvc;

namespace DuAnTotNghiep.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaxonomyController : ControllerBase
    {
        private readonly ITaxonomyService _taxonomyService;

        public TaxonomyController(ITaxonomyService taxonomyService)
        {
            _taxonomyService = taxonomyService;
        }

        [HttpGet("GetActiveSkills")]
        public async Task<IActionResult> GetActiveSkills()
        {
            var skills = await _taxonomyService.GetActiveSkillsAsync();
            return Ok(skills);
        }

        [HttpGet("GetActiveTopicsBySkill")]
        public async Task<IActionResult> GetActiveTopicsBySkill(int skillId)
        {
            var topics = await _taxonomyService.GetActiveTopicsBySkillAsync(skillId);
            return Ok(topics);
        }

        [HttpGet("GetProficiencyLevels")]
        public async Task<IActionResult> GetProficiencyLevels()
        {
            var levels = await _taxonomyService.GetProficiencyLevelsAsync();
            return Ok(levels);
        }
    }
}
