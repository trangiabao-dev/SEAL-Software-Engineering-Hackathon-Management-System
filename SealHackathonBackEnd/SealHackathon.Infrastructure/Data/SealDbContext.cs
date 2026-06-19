using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SealHackathon.Domain.Entities;

namespace SealHackathon.Infrastructure.Data;

public partial class SealDbContext : DbContext
{
    public SealDbContext(DbContextOptions<SealDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; } = null!;

    public virtual DbSet<AuditLog> AuditLogs { get; set; } = null!;

    public virtual DbSet<Criterion> Criteria { get; set; } = null!;

    public virtual DbSet<CriterionTemplate> CriterionTemplates { get; set; } = null!;

    public virtual DbSet<CriterionTemplateItem> CriterionTemplateItems { get; set; } = null!;

    public virtual DbSet<Event> Events { get; set; } = null!;

    public virtual DbSet<EventAccount> EventAccounts { get; set; } = null!;

    public virtual DbSet<JudgeAssign> JudgeAssigns { get; set; } = null!;

    public virtual DbSet<MentorAssign> MentorAssigns { get; set; } = null!;

    public virtual DbSet<Notification> Notifications { get; set; } = null!;

    public virtual DbSet<Prize> Prizes { get; set; } = null!;

    public virtual DbSet<Ranking> Rankings { get; set; } = null!;

    public virtual DbSet<Round> Rounds { get; set; } = null!;

    public virtual DbSet<RoundTeam> RoundTeams { get; set; } = null!;

    public virtual DbSet<SchemaVersion> SchemaVersions { get; set; } = null!;

    public virtual DbSet<ScoreRecord> ScoreRecords { get; set; } = null!;

    public virtual DbSet<Submission> Submissions { get; set; } = null!;

    public virtual DbSet<Team> Teams { get; set; } = null!;

    public virtual DbSet<TeamMember> TeamMembers { get; set; } = null!;

    public virtual DbSet<Topic> Topics { get; set; } = null!;

    public virtual DbSet<Track> Tracks { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Account__3214EC07700DABC1");

            entity.ToTable("Account");

            entity.HasIndex(e => e.Username, "UQ_Account_Username").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Account__A9D105348F2FC7A6").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.SystemRole)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Leader");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Username)
                .HasMaxLength(255);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AuditLog__3214EC07BB7F158C");

            entity.ToTable("AuditLog");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Action)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.EntityId)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.EntityName)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.PerformedByNavigation).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.PerformedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AuditLog__Perfor__2645B050");
        });

        modelBuilder.Entity<Criterion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Criterio__3214EC077B396BFB");

            entity.ToTable("Criterion");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.CriterionCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Criterion__Creat__0B91BA14");

            entity.HasOne(d => d.Round).WithMany(p => p.Criteria)
                .HasForeignKey(d => d.RoundId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Criterion__Round__08B54D69");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.CriterionUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK__Criterion__Updat__0C85DE4D");
        });

        modelBuilder.Entity<CriterionTemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Criterio__3214EC07E6E3C59A");

            entity.ToTable("CriterionTemplate");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Name).HasMaxLength(255);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.CriterionTemplates)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Criterion__Creat__5812160E");
        });

        modelBuilder.Entity<CriterionTemplateItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Criterio__3214EC070FBBE9BE");

            entity.ToTable("CriterionTemplateItem");

            entity.Property(e => e.Name).HasMaxLength(255);

            entity.HasOne(d => d.Template).WithMany(p => p.CriterionTemplateItems)
                .HasForeignKey(d => d.TemplateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Criterion__Templ__5AEE82B9");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Event__3214EC079EBC7F8B");

            entity.ToTable("Event");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Draft");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.EventCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Event__CreatedBy__32E0915F");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.EventUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK__Event__UpdatedBy__33D4B598");
        });

        modelBuilder.Entity<EventAccount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__EventAcc__3214EC070D900EC6");

            entity.ToTable("EventAccount");

            entity.HasIndex(e => e.AccountId, "IX_EventAccount_AccountId");

            entity.HasIndex(e => e.EventId, "IX_EventAccount_EventId");

            entity.HasIndex(e => new { e.EventId, e.AccountId, e.EventRole }, "UQ_EventAccount").IsUnique();

            entity.Property(e => e.AssignedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.EventRole)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.JudgeType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RejectedReason).HasMaxLength(500);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.Account).WithMany(p => p.EventAccountAccounts)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventAcco__Accou__38996AB5");

            entity.HasOne(d => d.AssignedByNavigation).WithMany(p => p.EventAccountAssignedByNavigations)
                .HasForeignKey(d => d.AssignedBy)
                .HasConstraintName("FK__EventAcco__Assig__3A81B327");

            entity.HasOne(d => d.Event).WithMany(p => p.EventAccounts)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventAcco__Event__37A5467C");
        });

        modelBuilder.Entity<JudgeAssign>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__JudgeAss__3214EC07FEBFA91D");

            entity.ToTable("JudgeAssign");

            entity.HasIndex(e => new { e.JudgeId, e.RoundId }, "UQ_Judge_Round").IsUnique();

            entity.Property(e => e.AssignedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.AssignedByNavigation).WithMany(p => p.JudgeAssignAssignedByNavigations)
                .HasForeignKey(d => d.AssignedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__JudgeAssi__Assig__05D8E0BE");

            entity.HasOne(d => d.Judge).WithMany(p => p.JudgeAssignJudges)
                .HasForeignKey(d => d.JudgeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__JudgeAssi__Judge__02FC7413");

            entity.HasOne(d => d.Round).WithMany(p => p.JudgeAssigns)
                .HasForeignKey(d => d.RoundId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__JudgeAssi__Round__03F0984C");
        });

        modelBuilder.Entity<MentorAssign>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MentorAs__3214EC07E5501BF2");

            entity.ToTable("MentorAssign");

            entity.HasIndex(e => new { e.MentorId, e.TrackId }, "UQ_Mentor_Track").IsUnique();

            entity.Property(e => e.AssignedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.AssignedByNavigation).WithMany(p => p.MentorAssignAssignedByNavigations)
                .HasForeignKey(d => d.AssignedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MentorAss__Assig__49C3F6B7");

            entity.HasOne(d => d.Mentor).WithMany(p => p.MentorAssignMentors)
                .HasForeignKey(d => d.MentorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MentorAss__Mento__46E78A0C");

            entity.HasOne(d => d.Track).WithMany(p => p.MentorAssigns)
                .HasForeignKey(d => d.TrackId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MentorAss__Track__47DBAE45");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Notifica__3214EC075349CFA0");

            entity.ToTable("Notification");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Account).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__Accou__208CD6FA");
        });

        modelBuilder.Entity<Prize>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Prize__3214EC076684359B");

            entity.ToTable("Prize");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Name).HasMaxLength(255);

            entity.HasOne(d => d.Track).WithMany(p => p.Prizes)
                .HasForeignKey(d => d.TrackId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Prize__TrackId__4CA06362");
        });

        modelBuilder.Entity<Ranking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Ranking__3214EC075BB438CE");

            entity.ToTable("Ranking");

            entity.HasIndex(e => new { e.TeamId, e.RoundId }, "UQ_Ranking_Team_Round").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CalculatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Round).WithMany(p => p.Rankings)
                .HasForeignKey(d => d.RoundId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ranking__RoundId__1AD3FDA4");

            entity.HasOne(d => d.Team).WithMany(p => p.Rankings)
                .HasForeignKey(d => d.TeamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ranking__TeamId__19DFD96B");
        });

        modelBuilder.Entity<Round>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Round__3214EC079D798DF5");

            entity.ToTable("Round");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Upcoming");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.RoundCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Round__CreatedBy__534D60F1");

            entity.HasOne(d => d.Track).WithMany(p => p.Rounds)
                .HasForeignKey(d => d.TrackId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Round__TrackId__4F7CD00D");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.RoundUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK__Round__UpdatedBy__5441852A");
        });

        modelBuilder.Entity<RoundTeam>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_RoundTeam");

            entity.ToTable("RoundTeam");

            entity.HasIndex(e => new { e.RoundId, e.TeamId }, "UQ_RoundTeam_Round_Team").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AssignedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.AssignedByNavigation).WithMany(p => p.RoundTeamAssignedByNavigations)
                .HasForeignKey(d => d.AssignedBy)
                .HasConstraintName("FK_RoundTeam_AssignedBy");

            entity.HasOne(d => d.Round).WithMany(p => p.RoundTeams)
                .HasForeignKey(d => d.RoundId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RoundTeam_Round");

            entity.HasOne(d => d.Team).WithMany(p => p.RoundTeams)
                .HasForeignKey(d => d.TeamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RoundTeam_Team");

            entity.HasOne(d => d.Topic).WithMany(p => p.RoundTeams)
                .HasForeignKey(d => d.TopicId)
                .HasConstraintName("FK_RoundTeam_Topic");
        });

        modelBuilder.Entity<SchemaVersion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_SchemaVersions_Id");

            entity.Property(e => e.Applied).HasColumnType("datetime");
            entity.Property(e => e.ScriptName).HasMaxLength(255);
        });

        modelBuilder.Entity<ScoreRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ScoreRec__3214EC07B3B11CEB");

            entity.ToTable("ScoreRecord");

            entity.HasIndex(e => e.SubmissionId, "IX_ScoreRecord_SubmissionId");

            entity.HasIndex(e => new { e.SubmissionId, e.JudgeId, e.CriterionId }, "UQ_ScoreRecord").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ScoredAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Criterion).WithMany(p => p.ScoreRecords)
                .HasForeignKey(d => d.CriterionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ScoreReco__Crite__1332DBDC");

            entity.HasOne(d => d.Judge).WithMany(p => p.ScoreRecords)
                .HasForeignKey(d => d.JudgeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ScoreReco__Judge__123EB7A3");

            entity.HasOne(d => d.Submission).WithMany(p => p.ScoreRecords)
                .HasForeignKey(d => d.SubmissionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ScoreReco__Submi__114A936A");
        });

        modelBuilder.Entity<Submission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Submissi__3214EC0768EED89B");

            entity.ToTable("Submission");

            entity.HasIndex(e => e.RoundId, "IX_Submission_RoundId");

            entity.HasIndex(e => new { e.TeamId, e.RoundId }, "UQ_Submission_Team_Round").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.UpdatedAt);
            entity.Property(e => e.PresentationUrl)
                .HasMaxLength(1000)
                .IsUnicode(false);

            entity.HasOne(d => d.DisqualifiedByNavigation).WithMany(p => p.Submissions)
                .HasForeignKey(d => d.DisqualifiedBy)
                .HasConstraintName("FK__Submissio__Disqu__7E37BEF6");

            entity.HasOne(d => d.Round).WithMany(p => p.Submissions)
                .HasForeignKey(d => d.RoundId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Submissio__Round__7C4F7684");

            entity.HasOne(d => d.Team).WithMany(p => p.Submissions)
                .HasForeignKey(d => d.TeamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Submissio__TeamI__7B5B524B");
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Team__3214EC07A4B7A492");

            entity.ToTable("Team");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.GithubRepoLink)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Pending");

            entity.Property(e => e.DisqualifyReason).HasMaxLength(500);

            entity.Property(e => e.TeamName).HasMaxLength(255);
            entity.Property(e => e.University).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TeamCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Team__CreatedBy__6D0D32F4");

            entity.HasOne(d => d.Leader).WithMany(p => p.TeamLeaders)
                .HasForeignKey(d => d.LeaderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Team__LeaderId__66603565");

            entity.HasOne(d => d.Mentor).WithMany(p => p.TeamMentors)
                .HasForeignKey(d => d.MentorId)
                .HasConstraintName("FK__Team__MentorId__6754599E");

            entity.HasOne(d => d.Topic).WithMany(p => p.Teams)
                .HasForeignKey(d => d.TopicId)
                .HasConstraintName("FK__Team__TopicId__68487DD7");

            entity.HasOne(d => d.Track).WithMany(p => p.Teams)
                .HasForeignKey(d => d.TrackId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Team__TrackId__656C112C");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.TeamUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK__Team__UpdatedBy__6E01572D");
        });

        modelBuilder.Entity<TeamMember>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TeamMemb__3214EC07B84759AD");

            entity.ToTable("TeamMember");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.University).HasMaxLength(255);
            entity.Property(e => e.IsFptstudent)
                .HasDefaultValue(true)
                .HasColumnName("IsFPTStudent");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.StudentCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TeamMemberCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__TeamMembe__Creat__75A278F5");

            entity.HasOne(d => d.Team).WithMany(p => p.TeamMembers)
                .HasForeignKey(d => d.TeamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TeamMembe__TeamI__70DDC3D8");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.TeamMemberUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK__TeamMembe__Updat__76969D2E");
        });

        modelBuilder.Entity<Topic>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Topic__3214EC075C11D878");

            entity.ToTable("Topic");

            entity.Property(e => e.AttachmentUrl)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TopicCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Topic__CreatedBy__60A75C0F");

            entity.HasOne(d => d.Round).WithMany(p => p.Topics)
                .HasForeignKey(d => d.RoundId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Topic__RoundId__5DCAEF64");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.TopicUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK__Topic__UpdatedBy__619B8048");
        });

        modelBuilder.Entity<Track>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Track__3214EC07F9DA9866");

            entity.ToTable("Track");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TrackCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Track__CreatedBy__4222D4EF");

            entity.HasOne(d => d.Event).WithMany(p => p.Tracks)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Track__EventId__3E52440B");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.TrackUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK__Track__UpdatedBy__4316F928");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
