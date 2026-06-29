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

public partial class LearningGoal
{
    public int Id { get; set; }

    public string GoalCode { get; set; } = null!;

    public string GoalName { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

<<<<<<< HEAD
<<<<<<< HEAD
=======
=======
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
    public int OrderIndex { get; set; }

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
    public virtual ICollection<LearningPathTemplate> LearningPathTemplates { get; set; } = new List<LearningPathTemplate>();

    public virtual ICollection<StudentLearningPath> StudentLearningPaths { get; set; } = new List<StudentLearningPath>();

    public virtual ICollection<StudentLearningProfile> StudentLearningProfiles { get; set; } = new List<StudentLearningProfile>();
}
