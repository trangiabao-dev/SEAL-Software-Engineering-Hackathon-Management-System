using System;
using System.Collections.Generic;

namespace SealHackathon.Domain.Entities;

public partial class Prize
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int RankPosition { get; set; }

    public decimal? Amount { get; set; }

    public virtual Event Event { get; set; } = null!;
}
