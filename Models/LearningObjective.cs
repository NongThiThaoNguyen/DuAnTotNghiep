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

public partial class LearningObjective
{
    public int Id { get; set; }

<<<<<<< HEAD
    public int TopicId { get; set; }

    public string ObjectiveText { get; set; } = null!;

=======
    [ForeignKey("Topic")]
    public int TopicId { get; set; }

    [Required]
    public string ObjectiveText { get; set; } = null!;

    [Required]
    [StringLength(50)]
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
    public string CognitiveLevel { get; set; } = null!;

    public int OrderIndex { get; set; }

<<<<<<< HEAD
=======
    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual User? UpdatedByNavigation { get; set; }

>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
    public virtual LearningTopic Topic { get; set; } = null!;
}
