using System;
using System.Collections.Generic;

namespace SealHackathon.Domain.Entities;

public partial class Track
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int? MaxTeams { get; set; }

    public int? MaxMembers { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public virtual Account? CreatedByNavigation { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual ICollection<MentorAssign> MentorAssigns { get; set; } = new List<MentorAssign>();

    public virtual ICollection<Prize> Prizes { get; set; } = new List<Prize>();

    public virtual ICollection<Round> Rounds { get; set; } = new List<Round>();

    public virtual ICollection<Team> Teams { get; set; } = new List<Team>();

    public virtual Account? UpdatedByNavigation { get; set; }
}
