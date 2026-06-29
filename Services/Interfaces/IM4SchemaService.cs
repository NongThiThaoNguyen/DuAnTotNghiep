using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IM4SchemaService
    {
        Task<SchemaHealthStatusDto> GetSchemaHealthStatusAsync();
    }

    public class SchemaHealthStatusDto
    {
        public int TotalSkills { get; set; }
        public int TotalLevels { get; set; }
        public int TotalTopics { get; set; }
        public int TotalObjectives { get; set; }

        public List<string> TopicsMissingSkill { get; set; } = new List<string>();
        public List<string> TopicsMissingLevel { get; set; } = new List<string>();
        public List<string> TopicsMissingObjective { get; set; } = new List<string>();
        public List<string> CircularDependencies { get; set; } = new List<string>();
    }
}
