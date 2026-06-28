using System;
using System.Collections.Generic;

namespace SealHackathon.Domain.Entities;

/// <summary>
/// Lưu bài nộp tham gia một phiên tie-break.
/// </summary>
public partial class TieBreakSubmission
{
    public Guid Id { get; set; }

    public Guid TieBreakSessionId { get; set; }

    public Guid SubmissionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Submission Submission { get; set; } = null!;

    public virtual TieBreakSession TieBreakSession { get; set; } = null!;

    public virtual ICollection<TieBreakScoreRecord> TieBreakScoreRecords { get; set; } =
        new List<TieBreakScoreRecord>();
}
