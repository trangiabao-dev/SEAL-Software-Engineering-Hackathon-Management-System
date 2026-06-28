using System;
using System.Collections.Generic;

namespace SealHackathon.Domain.Entities;

/// <summary>
/// Đại diện cho một phiên chấm lại để xử lý nhóm đội đồng hạng trong một Round.
/// </summary>
public partial class TieBreakSession
{
    public Guid Id { get; set; }

    public int RoundId { get; set; }

    public int RankPosition { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public virtual Round Round { get; set; } = null!;

    public virtual ICollection<TieBreakSubmission> TieBreakSubmissions { get; set; } =
        new List<TieBreakSubmission>();
}
