using System;
using System.Collections.Generic;

namespace SealHackathon.Domain.Entities;

public partial class JudgeAssign
{
    public int Id { get; set; }

    public Guid JudgeId { get; set; }

    public int RoundId { get; set; }

    public DateTime AssignedAt { get; set; }

    public Guid AssignedBy { get; set; }

    public virtual Account AssignedByNavigation { get; set; } = null!;

    public virtual Account Judge { get; set; } = null!;

    public virtual Round Round { get; set; } = null!;
}
