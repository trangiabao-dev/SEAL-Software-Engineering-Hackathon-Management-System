using System;
using System.Collections.Generic;

namespace SealHackathon.Domain.Entities;

public partial class TeamMember
{
    public int Id { get; set; }

    public Guid TeamId { get; set; }

    public string FullName { get; set; } = null!;

    public string StudentCode { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public bool IsLeader { get; set; }

    public bool IsFptstudent { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public virtual Account? CreatedByNavigation { get; set; }

    public virtual Team Team { get; set; } = null!;

    public virtual Account? UpdatedByNavigation { get; set; }
}
