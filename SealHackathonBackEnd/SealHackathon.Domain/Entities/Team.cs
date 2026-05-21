using System;
using System.Collections.Generic;

namespace SealHackathon.Domain.Entities;

public partial class Team
{
    public Guid Id { get; set; }

    public int TrackId { get; set; }

    public Guid LeaderId { get; set; }

    public Guid? MentorId { get; set; }

    public int? TopicId { get; set; }

    public string TeamName { get; set; } = null!;

    public string University { get; set; } = null!;

    public string? GithubRepoLink { get; set; }

    public string Status { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public virtual Account? CreatedByNavigation { get; set; }

    public virtual Account Leader { get; set; } = null!;

    public virtual Account? Mentor { get; set; }

    public virtual ICollection<Ranking> Rankings { get; set; } = new List<Ranking>();

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();

    public virtual ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();

    public virtual Topic? Topic { get; set; }

    public virtual Track Track { get; set; } = null!;

    public virtual Account? UpdatedByNavigation { get; set; }
}
