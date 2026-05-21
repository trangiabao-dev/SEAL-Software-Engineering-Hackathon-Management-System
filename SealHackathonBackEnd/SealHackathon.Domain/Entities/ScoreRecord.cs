using System;
using System.Collections.Generic;

namespace SealHackathon.Domain.Entities;

public partial class ScoreRecord
{
    public Guid Id { get; set; }

    public Guid SubmissionId { get; set; }

    public Guid JudgeId { get; set; }

    public int CriterionId { get; set; }

    public double Score { get; set; }

    public string? Comment { get; set; }

    public bool IsCalibration { get; set; }

    public DateTime ScoredAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Criterion Criterion { get; set; } = null!;

    public virtual Account Judge { get; set; } = null!;

    public virtual Submission Submission { get; set; } = null!;
}
