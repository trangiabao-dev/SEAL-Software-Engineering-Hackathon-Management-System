using System;

namespace SealHackathon.Domain.Entities;

/// <summary>
/// Lưu điểm Judge chấm lại cho một bài trong phiên tie-break.
/// </summary>
public partial class TieBreakScoreRecord
{
    public Guid Id { get; set; }

    public Guid TieBreakSubmissionId { get; set; }

    public Guid JudgeId { get; set; }

    public int CriterionId { get; set; }

    public double Score { get; set; }

    public string? Comment { get; set; }

    public DateTime ScoredAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Criterion Criterion { get; set; } = null!;

    public virtual Account Judge { get; set; } = null!;

    public virtual TieBreakSubmission TieBreakSubmission { get; set; } = null!;
}
