using System;

namespace SealHackathon.Domain.Entities;

public partial class RoundTeam
{
    public Guid Id { get; set; }

    public int RoundId { get; set; }

    public Guid TeamId { get; set; }

    public int? TopicId { get; set; }

    public DateTime AssignedAt { get; set; }

    public Guid? AssignedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Account? AssignedByNavigation { get; set; }

    public virtual Round Round { get; set; } = null!;

    public virtual Team Team { get; set; } = null!;

    public virtual Topic? Topic { get; set; }
}
