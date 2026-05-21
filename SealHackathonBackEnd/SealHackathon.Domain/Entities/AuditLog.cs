using System;
using System.Collections.Generic;

namespace SealHackathon.Domain.Entities;

public partial class AuditLog
{
    public Guid Id { get; set; }

    public Guid PerformedBy { get; set; }

    public string Action { get; set; } = null!;

    public string EntityName { get; set; } = null!;

    public string EntityId { get; set; } = null!;

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Account PerformedByNavigation { get; set; } = null!;
}
