using System;
using System.Collections.Generic;

namespace SealHackathon.Domain.Entities;

public partial class Account
{
    public Guid Id { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string SystemRole { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
    // Thêm 2 thuộc tính này vào file Account.cs
    public string? EmailConfirmToken { get; set; }
    public DateTime? TokenExpiresAt { get; set; }

    public string? ResetPasswordToken { get; set; }
    public DateTime? ResetPasswordTokenExpiresAt { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<Criterion> CriterionCreatedByNavigations { get; set; } = new List<Criterion>();

    public virtual ICollection<CriterionTemplate> CriterionTemplates { get; set; } = new List<CriterionTemplate>();

    public virtual ICollection<Criterion> CriterionUpdatedByNavigations { get; set; } = new List<Criterion>();

    public virtual ICollection<EventAccount> EventAccountAccounts { get; set; } = new List<EventAccount>();

    public virtual ICollection<EventAccount> EventAccountAssignedByNavigations { get; set; } = new List<EventAccount>();

    public virtual ICollection<Event> EventCreatedByNavigations { get; set; } = new List<Event>();

    public virtual ICollection<Event> EventUpdatedByNavigations { get; set; } = new List<Event>();

    public virtual ICollection<JudgeAssign> JudgeAssignAssignedByNavigations { get; set; } = new List<JudgeAssign>();

    public virtual ICollection<JudgeAssign> JudgeAssignJudges { get; set; } = new List<JudgeAssign>();

    public virtual ICollection<MentorAssign> MentorAssignAssignedByNavigations { get; set; } = new List<MentorAssign>();

    public virtual ICollection<MentorAssign> MentorAssignMentors { get; set; } = new List<MentorAssign>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Round> RoundCreatedByNavigations { get; set; } = new List<Round>();

    public virtual ICollection<Round> RoundUpdatedByNavigations { get; set; } = new List<Round>();

    public virtual ICollection<RoundTeam> RoundTeamAssignedByNavigations { get; set; } = new List<RoundTeam>();

    public virtual ICollection<ScoreRecord> ScoreRecords { get; set; } = new List<ScoreRecord>();

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();

    public virtual ICollection<Team> TeamCreatedByNavigations { get; set; } = new List<Team>();

    public virtual ICollection<Team> TeamLeaders { get; set; } = new List<Team>();

    public virtual ICollection<TeamMember> TeamMemberCreatedByNavigations { get; set; } = new List<TeamMember>();

    public virtual ICollection<TeamMember> TeamMemberUpdatedByNavigations { get; set; } = new List<TeamMember>();

    public virtual ICollection<Team> TeamMentors { get; set; } = new List<Team>();

    public virtual ICollection<Team> TeamUpdatedByNavigations { get; set; } = new List<Team>();

    public virtual ICollection<Topic> TopicCreatedByNavigations { get; set; } = new List<Topic>();

    public virtual ICollection<Topic> TopicUpdatedByNavigations { get; set; } = new List<Topic>();

    public virtual ICollection<Track> TrackCreatedByNavigations { get; set; } = new List<Track>();

    public virtual ICollection<Track> TrackUpdatedByNavigations { get; set; } = new List<Track>();
}
