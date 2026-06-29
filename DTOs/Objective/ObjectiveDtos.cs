using DuAnTotNghiep.Enums;

namespace DuAnTotNghiep.DTOs.Objective
{
    public class CreateObjectiveDto
    {
        public int TopicId { get; set; }
        public string ObjectiveText { get; set; } = string.Empty;
        public CognitiveLevel CognitiveLevel { get; set; }
        public int OrderIndex { get; set; }
    }

    public class UpdateObjectiveDto
    {
        public int Id { get; set; }
        public string ObjectiveText { get; set; } = string.Empty;
        public CognitiveLevel CognitiveLevel { get; set; }
        public int OrderIndex { get; set; }
    }

    public class ObjectiveDto
    {
        public int Id { get; set; }
        public string ObjectiveText { get; set; } = string.Empty;
        public string CognitiveLevel { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
    }

    public class ObjectiveDetailDto
    {
        public int Id { get; set; }
        public int TopicId { get; set; }
        public string TopicName { get; set; } = string.Empty;
        public string ObjectiveText { get; set; } = string.Empty;
        public string CognitiveLevel { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
    }
}
