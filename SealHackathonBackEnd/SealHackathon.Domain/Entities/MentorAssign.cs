using System;
using System.Collections.Generic;

namespace SealHackathon.Domain.Entities;

public partial class MentorAssign
{
    public int Id { get; set; }

    public Guid MentorId { get; set; }

    public int TrackId { get; set; }

    public DateTime AssignedAt { get; set; }

    public Guid AssignedBy { get; set; }

    public virtual Account AssignedByNavigation { get; set; } = null!;

    public virtual Account Mentor { get; set; } = null!;

    public virtual Track Track { get; set; } = null!;
}
