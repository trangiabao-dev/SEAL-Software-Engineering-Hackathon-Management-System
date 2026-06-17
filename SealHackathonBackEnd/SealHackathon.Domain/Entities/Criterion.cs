using System;
using System.Collections.Generic;

namespace SealHackathon.Domain.Entities;

public partial class Criterion
{
    public int Id { get; set; }

    public int RoundId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public double   MaxScore { get; set; }

    public double Weight { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public virtual Account? CreatedByNavigation { get; set; }

    public virtual Round Round { get; set; } = null!;

    public virtual ICollection<ScoreRecord> ScoreRecords { get; set; } = new List<ScoreRecord>();

    public virtual Account? UpdatedByNavigation { get; set; }
}
