<<<<<<< HEAD
<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
=======
=======
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
<<<<<<< HEAD
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
=======
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52

namespace DuAnTotNghiep.Models;

public partial class LearningObjective
{
    public int Id { get; set; }

<<<<<<< HEAD
<<<<<<< HEAD
    public int TopicId { get; set; }

    public string ObjectiveText { get; set; } = null!;

=======
=======
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
    [ForeignKey("Topic")]
    public int TopicId { get; set; }

    [Required]
    public string ObjectiveText { get; set; } = null!;

    [Required]
    [StringLength(50)]
<<<<<<< HEAD
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
=======
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
    public string CognitiveLevel { get; set; } = null!;

    public int OrderIndex { get; set; }

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
    public virtual LearningTopic Topic { get; set; } = null!;
}
