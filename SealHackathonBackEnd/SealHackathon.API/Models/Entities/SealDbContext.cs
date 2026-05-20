using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SealHackathon.API.Models.Entities;

public partial class SealDbContext : DbContext
{
    public SealDbContext()
    {
    }

    public SealDbContext(DbContextOptions<SealDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Criterion> Criteria { get; set; }

    public virtual DbSet<CriterionTemplate> CriterionTemplates { get; set; }

    public virtual DbSet<CriterionTemplateItem> CriterionTemplateItems { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<JudgeAssign> JudgeAssigns { get; set; }

    public virtual DbSet<MentorAssign> MentorAssigns { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Prize> Prizes { get; set; }

    public virtual DbSet<Ranking> Rankings { get; set; }

    public virtual DbSet<Round> Rounds { get; set; }

    public virtual DbSet<SchemaVersion> SchemaVersions { get; set; }

    public virtual DbSet<ScoreRecord> ScoreRecords { get; set; }

    public virtual DbSet<Submission> Submissions { get; set; }

    public virtual DbSet<Team> Teams { get; set; }

    public virtual DbSet<TeamMember> TeamMembers { get; set; }

    public virtual DbSet<Topic> Topics { get; set; }

    public virtual DbSet<Track> Tracks { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LAPTOP-V5C19DP0;Database=SEAL_Hackathon_DB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Account__3214EC0789F8B553");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.Status).HasDefaultValue("Pending");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AuditLog__3214EC0710F740E5");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.PerformedByNavigation).WithMany(p => p.AuditLogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AuditLog__Perfor__30C33EC3");
        });

        modelBuilder.Entity<Criterion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Criterio__3214EC077D276082");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.CriterionCreatedByNavigations).HasConstraintName("FK__Criterion__Creat__160F4887");

            entity.HasOne(d => d.Round).WithMany(p => p.Criteria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Criterion__Round__1332DBDC");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.CriterionUpdatedByNavigations).HasConstraintName("FK__Criterion__Updat__17036CC0");
        });

        modelBuilder.Entity<CriterionTemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Criterio__3214EC0727CEC786");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.CriterionTemplates).HasConstraintName("FK__Criterion__Creat__628FA481");
        });

        modelBuilder.Entity<CriterionTemplateItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Criterio__3214EC074F0FFE38");

            entity.HasOne(d => d.Template).WithMany(p => p.CriterionTemplateItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Criterion__Templ__656C112C");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Event__3214EC0768927340");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.Status).HasDefaultValue("Draft");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.EventCreatedByNavigations).HasConstraintName("FK__Event__CreatedBy__44FF419A");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.EventUpdatedByNavigations).HasConstraintName("FK__Event__UpdatedBy__45F365D3");
        });

        modelBuilder.Entity<JudgeAssign>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__JudgeAss__3214EC072A0665BE");

            entity.Property(e => e.AssignedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.AssignedByNavigation).WithMany(p => p.JudgeAssignAssignedByNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__JudgeAssi__Assig__10566F31");

            entity.HasOne(d => d.Judge).WithMany(p => p.JudgeAssignJudges)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__JudgeAssi__Judge__0D7A0286");

            entity.HasOne(d => d.Round).WithMany(p => p.JudgeAssigns)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__JudgeAssi__Round__0E6E26BF");
        });

        modelBuilder.Entity<MentorAssign>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MentorAs__3214EC07580965AF");

            entity.Property(e => e.AssignedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.AssignedByNavigation).WithMany(p => p.MentorAssignAssignedByNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MentorAss__Assig__5441852A");

            entity.HasOne(d => d.Mentor).WithMany(p => p.MentorAssignMentors)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MentorAss__Mento__5165187F");

            entity.HasOne(d => d.Track).WithMany(p => p.MentorAssigns)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MentorAss__Track__52593CB8");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Notifica__3214EC07E9338572");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsRead).HasDefaultValue(false);

            entity.HasOne(d => d.Account).WithMany(p => p.Notifications)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__Accou__2B0A656D");
        });

        modelBuilder.Entity<Prize>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Prize__3214EC0743954417");

            entity.HasOne(d => d.Track).WithMany(p => p.Prizes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Prize__TrackId__571DF1D5");
        });

        modelBuilder.Entity<Ranking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Ranking__3214EC07F7E41058");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CalculatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsAdvancing).HasDefaultValue(false);

            entity.HasOne(d => d.Round).WithMany(p => p.Rankings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ranking__RoundId__25518C17");

            entity.HasOne(d => d.Team).WithMany(p => p.Rankings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ranking__TeamId__245D67DE");
        });

        modelBuilder.Entity<Round>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Round__3214EC07EC66AEAC");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Status).HasDefaultValue("Upcoming");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.RoundCreatedByNavigations).HasConstraintName("FK__Round__CreatedBy__5DCAEF64");

            entity.HasOne(d => d.Track).WithMany(p => p.Rounds)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Round__TrackId__59FA5E80");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.RoundUpdatedByNavigations).HasConstraintName("FK__Round__UpdatedBy__5EBF139D");
        });

        modelBuilder.Entity<SchemaVersion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_SchemaVersions_Id");
        });

        modelBuilder.Entity<ScoreRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ScoreRec__3214EC076782634F");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.IsCalibration).HasDefaultValue(false);
            entity.Property(e => e.ScoredAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Criterion).WithMany(p => p.ScoreRecords)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ScoreReco__Crite__1DB06A4F");

            entity.HasOne(d => d.Judge).WithMany(p => p.ScoreRecords)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ScoreReco__Judge__1CBC4616");

            entity.HasOne(d => d.Submission).WithMany(p => p.ScoreRecords)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ScoreReco__Submi__1BC821DD");
        });

        modelBuilder.Entity<Submission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Submissi__3214EC073870AD76");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsDisqualified).HasDefaultValue(false);

            entity.HasOne(d => d.DisqualifiedByNavigation).WithMany(p => p.Submissions).HasConstraintName("FK__Submissio__Disqu__08B54D69");

            entity.HasOne(d => d.Round).WithMany(p => p.Submissions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Submissio__Round__06CD04F7");

            entity.HasOne(d => d.Team).WithMany(p => p.Submissions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Submissio__TeamI__05D8E0BE");
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Team__3214EC07A03C9514");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.Status).HasDefaultValue("Draft");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TeamCreatedByNavigations).HasConstraintName("FK__Team__CreatedBy__778AC167");

            entity.HasOne(d => d.Leader).WithMany(p => p.TeamLeaders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Team__LeaderId__70DDC3D8");

            entity.HasOne(d => d.Mentor).WithMany(p => p.TeamMentors).HasConstraintName("FK__Team__MentorId__71D1E811");

            entity.HasOne(d => d.Topic).WithMany(p => p.Teams).HasConstraintName("FK__Team__TopicId__72C60C4A");

            entity.HasOne(d => d.Track).WithMany(p => p.Teams)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Team__TrackId__6FE99F9F");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.TeamUpdatedByNavigations).HasConstraintName("FK__Team__UpdatedBy__787EE5A0");
        });

        modelBuilder.Entity<TeamMember>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TeamMemb__3214EC07F9DCEF80");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsFptstudent).HasDefaultValue(true);
            entity.Property(e => e.IsLeader).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TeamMemberCreatedByNavigations).HasConstraintName("FK__TeamMembe__Creat__00200768");

            entity.HasOne(d => d.Team).WithMany(p => p.TeamMembers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TeamMembe__TeamI__7B5B524B");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.TeamMemberUpdatedByNavigations).HasConstraintName("FK__TeamMembe__Updat__01142BA1");
        });

        modelBuilder.Entity<Topic>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Topic__3214EC070A750F1D");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TopicCreatedByNavigations).HasConstraintName("FK__Topic__CreatedBy__6B24EA82");

            entity.HasOne(d => d.Round).WithMany(p => p.Topics)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Topic__RoundId__68487DD7");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.TopicUpdatedByNavigations).HasConstraintName("FK__Topic__UpdatedBy__6C190EBB");
        });

        modelBuilder.Entity<Track>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Track__3214EC07F5A72686");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TrackCreatedByNavigations).HasConstraintName("FK__Track__CreatedBy__4CA06362");

            entity.HasOne(d => d.Event).WithMany(p => p.Tracks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Track__EventId__48CFD27E");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.TrackUpdatedByNavigations).HasConstraintName("FK__Track__UpdatedBy__4D94879B");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
