<<<<<<< HEAD
<<<<<<< HEAD
﻿using System;
=======
using System;
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
=======
using System;
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class CompetencySkillScore
{
    public int Id { get; set; }

    public int CompetencyAnalysisId { get; set; }

    public int SkillId { get; set; }

    public decimal Score { get; set; }

    public int? LevelId { get; set; }

    public string? WeaknessNote { get; set; }

    public int PriorityLevel { get; set; }

<<<<<<< HEAD
<<<<<<< HEAD
=======
    public int? TopicId { get; set; }

>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
=======
    public int? TopicId { get; set; }

>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
    public virtual CompetencyAnalysis CompetencyAnalysis { get; set; } = null!;

    public virtual EnglishProficiencyLevel? Level { get; set; }

    public virtual EnglishSkill Skill { get; set; } = null!;
<<<<<<< HEAD
<<<<<<< HEAD
=======

    public virtual LearningTopic? Topic { get; set; }
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
=======

    public virtual LearningTopic? Topic { get; set; }
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
}
