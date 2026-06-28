using System;
using System.Collections.Generic;

namespace SealHackathon.Domain.Entities;

public partial class Submission
{
    public Guid Id { get; set; }

    public Guid TeamId { get; set; }

    public int RoundId { get; set; }

    public string PresentationUrl { get; set; } = null!;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool CanEdit { get; set; }

    public bool IsDisqualified { get; set; }

    public string? DisqualifyReason { get; set; }

    public DateTime? DisqualifiedAt { get; set; }

    public Guid? DisqualifiedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Account? DisqualifiedByNavigation { get; set; }

    public virtual Round Round { get; set; } = null!;

    public virtual ICollection<ScoreRecord> ScoreRecords { get; set; } = new List<ScoreRecord>();

    public virtual Team Team { get; set; } = null!;
}
