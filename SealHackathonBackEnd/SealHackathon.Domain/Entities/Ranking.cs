using System;
using System.Collections.Generic;

namespace SealHackathon.Domain.Entities;

public partial class Ranking
{
    public Guid Id { get; set; }

    public Guid TeamId { get; set; }

    public int RoundId { get; set; }

    public double TotalScore { get; set; }

    public int RankPosition { get; set; }

    public bool IsAdvancing { get; set; }

    public DateTime CalculatedAt { get; set; }

    public virtual Round Round { get; set; } = null!;

    public virtual Team Team { get; set; } = null!;
}
