using System;
using System.Collections.Generic;

namespace SealHackathon.Domain.Entities;

public partial class Round
{
    public int Id { get; set; }

    public int TrackId { get; set; }

    public string Name { get; set; } = null!;

    public int OrderIndex { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public int? AdvancingSlots { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public virtual Account? CreatedByNavigation { get; set; }

    public virtual ICollection<Criterion> Criteria { get; set; } = new List<Criterion>();

    public virtual ICollection<JudgeAssign> JudgeAssigns { get; set; } = new List<JudgeAssign>();

    public virtual ICollection<Ranking> Rankings { get; set; } = new List<Ranking>();

    public virtual ICollection<RoundTeam> RoundTeams { get; set; } = new List<RoundTeam>();

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();

    public virtual ICollection<Topic> Topics { get; set; } = new List<Topic>();

    public virtual Track Track { get; set; } = null!;

    public virtual Account? UpdatedByNavigation { get; set; }
}
