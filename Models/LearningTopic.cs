<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
=======
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52

namespace DuAnTotNghiep.Models;

public partial class LearningTopic
{
    public int Id { get; set; }

<<<<<<< HEAD
    public int SkillId { get; set; }

    public int? ParentTopicId { get; set; }

    public int? LevelId { get; set; }

    public string? TopicCode { get; set; }

=======
    [ForeignKey("Skill")]
    public int SkillId { get; set; }

    [ForeignKey("ParentTopic")]
    public int? ParentTopicId { get; set; }

    [ForeignKey("Level")]
    public int? LevelId { get; set; }

    [StringLength(50)]
    public string? TopicCode { get; set; }

    [Required]
    [StringLength(255)]
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

<<<<<<< HEAD
=======
    [Required]
    [StringLength(50)]
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
    public string DifficultyLevel { get; set; } = null!;

    public int? EstimatedMinutes { get; set; }

    public int OrderIndex { get; set; }

<<<<<<< HEAD
=======
    [Required]
    [StringLength(50)]
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
    public string Status { get; set; } = null!;

    public int? CreatedBy { get; set; }

<<<<<<< HEAD
=======
    public int? UpdatedBy { get; set; }

>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<AiGeneratedContent> AiGeneratedContents { get; set; } = new List<AiGeneratedContent>();

    public virtual ICollection<AiTutorConversation> AiTutorConversations { get; set; } = new List<AiTutorConversation>();

    public virtual User? CreatedByNavigation { get; set; }

<<<<<<< HEAD
=======
    public virtual User? UpdatedByNavigation { get; set; }

>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
    public virtual ICollection<LearningTopic> InverseParentTopic { get; set; } = new List<LearningTopic>();

    public virtual ICollection<LearningObjective> LearningObjectives { get; set; } = new List<LearningObjective>();

    public virtual ICollection<LearningPathNode> LearningPathNodes { get; set; } = new List<LearningPathNode>();

    public virtual ICollection<LearningPathTemplateNode> LearningPathTemplateNodes { get; set; } = new List<LearningPathTemplateNode>();

    public virtual EnglishProficiencyLevel? Level { get; set; }

    public virtual ICollection<OriginalLesson> OriginalLessons { get; set; } = new List<OriginalLesson>();

    public virtual LearningTopic? ParentTopic { get; set; }

    public virtual ICollection<PracticeTask> PracticeTasks { get; set; } = new List<PracticeTask>();

    public virtual ICollection<QuestionBank> QuestionBanks { get; set; } = new List<QuestionBank>();

    public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();

    public virtual EnglishSkill Skill { get; set; } = null!;

    public virtual ICollection<StudentProgressSnapshot> StudentProgressSnapshots { get; set; } = new List<StudentProgressSnapshot>();

    public virtual ICollection<StudyActivityLog> StudyActivityLogs { get; set; } = new List<StudyActivityLog>();

    public virtual ICollection<TopicReference> TopicReferences { get; set; } = new List<TopicReference>();
<<<<<<< HEAD
=======

    public virtual ICollection<TopicPrerequisite> TopicPrerequisites { get; set; } = new List<TopicPrerequisite>();

    public virtual ICollection<TopicPrerequisite> PrerequisiteForTopics { get; set; } = new List<TopicPrerequisite>();
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
}
