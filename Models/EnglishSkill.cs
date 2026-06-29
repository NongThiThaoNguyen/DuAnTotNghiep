<<<<<<< HEAD
<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
=======
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
=======
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52

namespace DuAnTotNghiep.Models;

public partial class EnglishSkill
{
    public int Id { get; set; }

<<<<<<< HEAD
<<<<<<< HEAD
    public string SkillCode { get; set; } = null!;

=======
=======
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
    [Required]
    [StringLength(50)]
    public string SkillCode { get; set; } = null!;

    [Required]
    [StringLength(255)]
<<<<<<< HEAD
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
=======
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
    public string SkillName { get; set; } = null!;

    public string? Description { get; set; }

    public int OrderIndex { get; set; }

    public bool IsActive { get; set; }

<<<<<<< HEAD
<<<<<<< HEAD
=======
=======
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual User? UpdatedByNavigation { get; set; }

<<<<<<< HEAD
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
=======
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
    public virtual ICollection<CompetencySkillScore> CompetencySkillScores { get; set; } = new List<CompetencySkillScore>();

    public virtual ICollection<LearningPathTemplateNode> LearningPathTemplateNodes { get; set; } = new List<LearningPathTemplateNode>();

    public virtual ICollection<LearningTopic> LearningTopics { get; set; } = new List<LearningTopic>();

    public virtual ICollection<PlacementTestSection> PlacementTestSections { get; set; } = new List<PlacementTestSection>();

    public virtual ICollection<PracticeTask> PracticeTasks { get; set; } = new List<PracticeTask>();

    public virtual ICollection<QuestionBank> QuestionBanks { get; set; } = new List<QuestionBank>();

    public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();

    public virtual ICollection<StudentProgressSnapshot> StudentProgressSnapshots { get; set; } = new List<StudentProgressSnapshot>();
}
