using System;
using System.Collections.Generic;

namespace SealHackathon.Domain.Entities;

public partial class Topic
{
    public int Id { get; set; }

    public int? RoundId { get; set; }

    public int? EventId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? Requirements { get; set; }

    public string? AttachmentUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public virtual Account? CreatedByNavigation { get; set; }

    public virtual Event? Event { get; set; }

    public virtual Round? Round { get; set; }

    public virtual ICollection<RoundTeam> RoundTeams { get; set; } = new List<RoundTeam>();

    public virtual ICollection<Team> Teams { get; set; } = new List<Team>();

    public virtual Account? UpdatedByNavigation { get; set; }
}
