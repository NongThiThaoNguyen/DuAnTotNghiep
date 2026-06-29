namespace DuAnTotNghiep.Models.DTOs.Topic
{
    public class TopicDetailDto
    {
        public int Id { get; set; }
        public string TopicCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string SkillName { get; set; } = string.Empty;
        public string LevelName { get; set; } = string.Empty;
        public string? ParentTopicName { get; set; }
        public string DifficultyLevel { get; set; } = string.Empty; // added
        public int ObjectiveCount { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        // Additional identifiers
        public int? SkillId { get; set; }
        public int? LevelId { get; set; }
        public int? ParentTopicId { get; set; }
        public int OrderIndex { get; set; }
        public string? Difficulty { get; set; } // backward compatibility

        // Navigation collections
        public IEnumerable<DuAnTotNghiep.Models.DTOs.Objective.ObjectiveDto>? Objectives { get; set; }
        public IEnumerable<DuAnTotNghiep.Models.DTOs.Topic.TopicOptionDto>? Prerequisites { get; set; }

        public int LessonCount { get; set; }
        public int QuizCount { get; set; }
        public int PathCount { get; set; }
        public int QuestionCount { get; set; }
        public int PlacementTestCount { get; set; }
        public bool CanArchive { get; set; }
        public bool CanDeactivate { get; set; }
        public string? Reason { get; set; }

        public IEnumerable<LessonDto> Lessons { get; set; } = new List<LessonDto>();
        public IEnumerable<QuizDto> Quizzes { get; set; } = new List<QuizDto>();
        public IEnumerable<LearningPathDto> LearningPaths { get; set; } = new List<LearningPathDto>();
        public IEnumerable<TopicReferenceDto> References { get; set; } = new List<TopicReferenceDto>();
    }

    public class TopicReferenceDto
    {
        public int Id { get; set; }
        public int ReferenceSourceId { get; set; }
        public string SourceName { get; set; } = string.Empty;
        public string? SourceUrl { get; set; }
        public string SourceType { get; set; } = string.Empty;
        public string? LicenseNote { get; set; }
        public string? Note { get; set; }
        public bool IsValid { get; set; } = true;
        public string ValidationLevel { get; set; } = "OK";
        public string ValidationMessage { get; set; } = string.Empty;
        public string SourceStatus { get; set; } = string.Empty;
    }

    public class LessonDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public int? EstimatedMinutes { get; set; }
        public string ReviewStatus { get; set; } = string.Empty;
        public bool IsAiGenerated { get; set; }
    }

    public class QuizDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string QuizType { get; set; } = string.Empty;
        public int? TimeLimitMinutes { get; set; }
        public decimal? PassingScore { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class LearningPathDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
    }

    public class AiTopicPayloadDto
    {
        public int TopicId { get; set; }
        public string TopicCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public string SkillName { get; set; } = string.Empty;
        public string LevelName { get; set; } = string.Empty;
        public List<string> Objectives { get; set; } = new List<string>();
        public List<string> Prerequisites { get; set; } = new List<string>();
        public List<string> Lessons { get; set; } = new List<string>();
        public List<string> Quizzes { get; set; } = new List<string>();
    }
}
