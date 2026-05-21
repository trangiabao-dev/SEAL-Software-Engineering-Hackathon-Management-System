using System;
using System.Collections.Generic;

namespace SealHackathon.Domain.Entities;

public partial class EventAccount
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public Guid AccountId { get; set; }

    public string EventRole { get; set; } = null!;

    public string? JudgeType { get; set; }

    public string Status { get; set; } = null!;

    public string? RejectedReason { get; set; }

    public Guid? AssignedBy { get; set; }

    public DateTime AssignedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Account? AssignedByNavigation { get; set; }

    public virtual Event Event { get; set; } = null!;
}
