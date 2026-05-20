using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SealHackathon.API.Models.Entities;

[Table("Team")]
public partial class Team
{
    [Key]
    public Guid Id { get; set; }

    public int TrackId { get; set; }

    public Guid LeaderId { get; set; }

    public Guid? MentorId { get; set; }

    public int? TopicId { get; set; }

    [StringLength(255)]
    public string TeamName { get; set; } = null!;

    [StringLength(255)]
    public string University { get; set; } = null!;

    [StringLength(500)]
    [Unicode(false)]
    public string? GithubRepoLink { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Status { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("TeamCreatedByNavigations")]
    public virtual Account? CreatedByNavigation { get; set; }

    [ForeignKey("LeaderId")]
    [InverseProperty("TeamLeaders")]
    public virtual Account Leader { get; set; } = null!;

    [ForeignKey("MentorId")]
    [InverseProperty("TeamMentors")]
    public virtual Account? Mentor { get; set; }

    [InverseProperty("Team")]
    public virtual ICollection<Ranking> Rankings { get; set; } = new List<Ranking>();

    [InverseProperty("Team")]
    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();

    [InverseProperty("Team")]
    public virtual ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();

    [ForeignKey("TopicId")]
    [InverseProperty("Teams")]
    public virtual Topic? Topic { get; set; }

    [ForeignKey("TrackId")]
    [InverseProperty("Teams")]
    public virtual Track Track { get; set; } = null!;

    [ForeignKey("UpdatedBy")]
    [InverseProperty("TeamUpdatedByNavigations")]
    public virtual Account? UpdatedByNavigation { get; set; }
}
