using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SealHackathon.API.Models.Entities;

[Table("Account")]
[Index("Username", Name = "UQ__Account__536C85E4ABEF538E", IsUnique = true)]
[Index("Email", Name = "UQ__Account__A9D10534B18055E9", IsUnique = true)]
public partial class Account
{
    [Key]
    public Guid Id { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string Username { get; set; } = null!;

    [StringLength(500)]
    [Unicode(false)]
    public string PasswordHash { get; set; } = null!;

    [StringLength(255)]
    [Unicode(false)]
    public string Email { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string Role { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string Status { get; set; } = null!;

    [StringLength(500)]
    public string? RejectedReason { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? JudgeType { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("PerformedByNavigation")]
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Criterion> CriterionCreatedByNavigations { get; set; } = new List<Criterion>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<CriterionTemplate> CriterionTemplates { get; set; } = new List<CriterionTemplate>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<Criterion> CriterionUpdatedByNavigations { get; set; } = new List<Criterion>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Event> EventCreatedByNavigations { get; set; } = new List<Event>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<Event> EventUpdatedByNavigations { get; set; } = new List<Event>();

    [InverseProperty("AssignedByNavigation")]
    public virtual ICollection<JudgeAssign> JudgeAssignAssignedByNavigations { get; set; } = new List<JudgeAssign>();

    [InverseProperty("Judge")]
    public virtual ICollection<JudgeAssign> JudgeAssignJudges { get; set; } = new List<JudgeAssign>();

    [InverseProperty("AssignedByNavigation")]
    public virtual ICollection<MentorAssign> MentorAssignAssignedByNavigations { get; set; } = new List<MentorAssign>();

    [InverseProperty("Mentor")]
    public virtual ICollection<MentorAssign> MentorAssignMentors { get; set; } = new List<MentorAssign>();

    [InverseProperty("Account")]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Round> RoundCreatedByNavigations { get; set; } = new List<Round>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<Round> RoundUpdatedByNavigations { get; set; } = new List<Round>();

    [InverseProperty("Judge")]
    public virtual ICollection<ScoreRecord> ScoreRecords { get; set; } = new List<ScoreRecord>();

    [InverseProperty("DisqualifiedByNavigation")]
    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Team> TeamCreatedByNavigations { get; set; } = new List<Team>();

    [InverseProperty("Leader")]
    public virtual ICollection<Team> TeamLeaders { get; set; } = new List<Team>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<TeamMember> TeamMemberCreatedByNavigations { get; set; } = new List<TeamMember>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<TeamMember> TeamMemberUpdatedByNavigations { get; set; } = new List<TeamMember>();

    [InverseProperty("Mentor")]
    public virtual ICollection<Team> TeamMentors { get; set; } = new List<Team>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<Team> TeamUpdatedByNavigations { get; set; } = new List<Team>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Topic> TopicCreatedByNavigations { get; set; } = new List<Topic>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<Topic> TopicUpdatedByNavigations { get; set; } = new List<Topic>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Track> TrackCreatedByNavigations { get; set; } = new List<Track>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<Track> TrackUpdatedByNavigations { get; set; } = new List<Track>();
}
