using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SealHackathon.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMinMembersToTrack : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Account",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    Username = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: false),
                    Email = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    SystemRole = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "Leader"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    EmailConfirmToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TokenExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResetPasswordToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResetPasswordTokenExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Account__3214EC07700DABC1", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SchemaVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScriptName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Applied = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchemaVersions_Id", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    PerformedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    EntityName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__AuditLog__3214EC07BB7F158C", x => x.Id);
                    table.ForeignKey(
                        name: "FK__AuditLog__Perfor__2645B050",
                        column: x => x.PerformedBy,
                        principalTable: "Account",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CriterionTemplate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Criterio__3214EC07E6E3C59A", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Criterion__Creat__5812160E",
                        column: x => x.CreatedBy,
                        principalTable: "Account",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Event",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BannerUrl = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsOnline = table.Column<bool>(type: "bit", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "Draft"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Event__3214EC079EBC7F8B", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Event__CreatedBy__32E0915F",
                        column: x => x.CreatedBy,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Event__UpdatedBy__33D4B598",
                        column: x => x.UpdatedBy,
                        principalTable: "Account",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Notifica__3214EC075349CFA0", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Notificat__Accou__208CD6FA",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CriterionTemplateItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaxScore = table.Column<double>(type: "float", nullable: false),
                    Weight = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Criterio__3214EC070FBBE9BE", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Criterion__Templ__5AEE82B9",
                        column: x => x.TemplateId,
                        principalTable: "CriterionTemplate",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EventAccount",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    JudgeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    RejectedReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AssignedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__EventAcc__3214EC070D900EC6", x => x.Id);
                    table.ForeignKey(
                        name: "FK__EventAcco__Accou__38996AB5",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__EventAcco__Assig__3A81B327",
                        column: x => x.AssignedBy,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__EventAcco__Event__37A5467C",
                        column: x => x.EventId,
                        principalTable: "Event",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Prize",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RankPosition = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Prize__3214EC076684359B", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prize_Event",
                        column: x => x.EventId,
                        principalTable: "Event",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Track",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaxTeams = table.Column<int>(type: "int", nullable: true),
                    MaxMembers = table.Column<int>(type: "int", nullable: true),
                    MinMembers = table.Column<int>(type: "int", nullable: true),
                    IsFinal = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Track__3214EC07F9DA9866", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Track__CreatedBy__4222D4EF",
                        column: x => x.CreatedBy,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Track__EventId__3E52440B",
                        column: x => x.EventId,
                        principalTable: "Event",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Track__UpdatedBy__4316F928",
                        column: x => x.UpdatedBy,
                        principalTable: "Account",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MentorAssign",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MentorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrackId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    AssignedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__MentorAs__3214EC07E5501BF2", x => x.Id);
                    table.ForeignKey(
                        name: "FK__MentorAss__Assig__49C3F6B7",
                        column: x => x.AssignedBy,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__MentorAss__Mento__46E78A0C",
                        column: x => x.MentorId,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__MentorAss__Track__47DBAE45",
                        column: x => x.TrackId,
                        principalTable: "Track",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Round",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrackId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdvancingSlots = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "Upcoming"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Round__3214EC079D798DF5", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Round__CreatedBy__534D60F1",
                        column: x => x.CreatedBy,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Round__TrackId__4F7CD00D",
                        column: x => x.TrackId,
                        principalTable: "Track",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Round__UpdatedBy__5441852A",
                        column: x => x.UpdatedBy,
                        principalTable: "Account",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Criterion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoundId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaxScore = table.Column<double>(type: "float", nullable: false),
                    Weight = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Criterio__3214EC077B396BFB", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Criterion__Creat__0B91BA14",
                        column: x => x.CreatedBy,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Criterion__Round__08B54D69",
                        column: x => x.RoundId,
                        principalTable: "Round",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Criterion__Updat__0C85DE4D",
                        column: x => x.UpdatedBy,
                        principalTable: "Account",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "JudgeAssign",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JudgeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    AssignedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__JudgeAss__3214EC07FEBFA91D", x => x.Id);
                    table.ForeignKey(
                        name: "FK__JudgeAssi__Assig__05D8E0BE",
                        column: x => x.AssignedBy,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__JudgeAssi__Judge__02FC7413",
                        column: x => x.JudgeId,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__JudgeAssi__Round__03F0984C",
                        column: x => x.RoundId,
                        principalTable: "Round",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TieBreakSession",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    RoundId = table.Column<int>(type: "int", nullable: false),
                    RankPosition = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "PendingScoring"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TieBreakSession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TieBreakSession_Round",
                        column: x => x.RoundId,
                        principalTable: "Round",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Topic",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoundId = table.Column<int>(type: "int", nullable: true),
                    EventId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Requirements = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttachmentUrl = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Topic__3214EC075C11D878", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Topic_EventId",
                        column: x => x.EventId,
                        principalTable: "Event",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Topic__CreatedBy__60A75C0F",
                        column: x => x.CreatedBy,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Topic__RoundId__5DCAEF64",
                        column: x => x.RoundId,
                        principalTable: "Round",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Topic__UpdatedBy__619B8048",
                        column: x => x.UpdatedBy,
                        principalTable: "Account",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Team",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    TrackId = table.Column<int>(type: "int", nullable: false),
                    LeaderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MentorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TopicId = table.Column<int>(type: "int", nullable: true),
                    TeamName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    University = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    GithubRepoLink = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "Pending"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DisqualifyReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RejectedReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Team__3214EC07A4B7A492", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Team__CreatedBy__6D0D32F4",
                        column: x => x.CreatedBy,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Team__LeaderId__66603565",
                        column: x => x.LeaderId,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Team__MentorId__6754599E",
                        column: x => x.MentorId,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Team__TopicId__68487DD7",
                        column: x => x.TopicId,
                        principalTable: "Topic",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Team__TrackId__656C112C",
                        column: x => x.TrackId,
                        principalTable: "Track",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Team__UpdatedBy__6E01572D",
                        column: x => x.UpdatedBy,
                        principalTable: "Account",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Ranking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: false),
                    TotalScore = table.Column<double>(type: "float", nullable: false),
                    RankPosition = table.Column<int>(type: "int", nullable: false),
                    IsAdvancing = table.Column<bool>(type: "bit", nullable: false),
                    CalculatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Ranking__3214EC075BB438CE", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Ranking__RoundId__1AD3FDA4",
                        column: x => x.RoundId,
                        principalTable: "Round",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Ranking__TeamId__19DFD96B",
                        column: x => x.TeamId,
                        principalTable: "Team",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RoundTeam",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    RoundId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TopicId = table.Column<int>(type: "int", nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    AssignedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoundTeam", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoundTeam_AssignedBy",
                        column: x => x.AssignedBy,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RoundTeam_Round",
                        column: x => x.RoundId,
                        principalTable: "Round",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RoundTeam_Team",
                        column: x => x.TeamId,
                        principalTable: "Team",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RoundTeam_Topic",
                        column: x => x.TopicId,
                        principalTable: "Topic",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Submission",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: false),
                    PresentationUrl = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: false),
                    CanEdit = table.Column<bool>(type: "bit", nullable: false),
                    IsDisqualified = table.Column<bool>(type: "bit", nullable: false),
                    DisqualifyReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisqualifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DisqualifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Submissi__3214EC0768EED89B", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Submissio__Disqu__7E37BEF6",
                        column: x => x.DisqualifiedBy,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Submissio__Round__7C4F7684",
                        column: x => x.RoundId,
                        principalTable: "Round",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Submissio__TeamI__7B5B524B",
                        column: x => x.TeamId,
                        principalTable: "Team",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TeamMember",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StudentCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    University = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Phone = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    IsLeader = table.Column<bool>(type: "bit", nullable: false),
                    IsFPTStudent = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TeamMemb__3214EC07B84759AD", x => x.Id);
                    table.ForeignKey(
                        name: "FK__TeamMembe__Creat__75A278F5",
                        column: x => x.CreatedBy,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__TeamMembe__TeamI__70DDC3D8",
                        column: x => x.TeamId,
                        principalTable: "Team",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__TeamMembe__Updat__76969D2E",
                        column: x => x.UpdatedBy,
                        principalTable: "Account",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ScoreRecord",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    SubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JudgeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CriterionId = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<double>(type: "float", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScoredAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ScoreRec__3214EC07B3B11CEB", x => x.Id);
                    table.ForeignKey(
                        name: "FK__ScoreReco__Crite__1332DBDC",
                        column: x => x.CriterionId,
                        principalTable: "Criterion",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__ScoreReco__Judge__123EB7A3",
                        column: x => x.JudgeId,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__ScoreReco__Submi__114A936A",
                        column: x => x.SubmissionId,
                        principalTable: "Submission",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TieBreakSubmission",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    TieBreakSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TieBreakSubmission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TieBreakSubmission_Submission",
                        column: x => x.SubmissionId,
                        principalTable: "Submission",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TieBreakSubmission_TieBreakSession",
                        column: x => x.TieBreakSessionId,
                        principalTable: "TieBreakSession",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TieBreakScoreRecord",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    TieBreakSubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JudgeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CriterionId = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<double>(type: "float", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScoredAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TieBreakScoreRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TieBreakScoreRecord_Criterion",
                        column: x => x.CriterionId,
                        principalTable: "Criterion",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TieBreakScoreRecord_Judge",
                        column: x => x.JudgeId,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TieBreakScoreRecord_TieBreakSubmission",
                        column: x => x.TieBreakSubmissionId,
                        principalTable: "TieBreakSubmission",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "UQ__Account__A9D105348F2FC7A6",
                table: "Account",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Account_Username",
                table: "Account",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_PerformedBy",
                table: "AuditLog",
                column: "PerformedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Criterion_CreatedBy",
                table: "Criterion",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Criterion_RoundId",
                table: "Criterion",
                column: "RoundId");

            migrationBuilder.CreateIndex(
                name: "IX_Criterion_UpdatedBy",
                table: "Criterion",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CriterionTemplate_CreatedBy",
                table: "CriterionTemplate",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CriterionTemplateItem_TemplateId",
                table: "CriterionTemplateItem",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Event_CreatedBy",
                table: "Event",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Event_UpdatedBy",
                table: "Event",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_EventAccount_AccountId",
                table: "EventAccount",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_EventAccount_AssignedBy",
                table: "EventAccount",
                column: "AssignedBy");

            migrationBuilder.CreateIndex(
                name: "IX_EventAccount_EventId",
                table: "EventAccount",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "UQ_EventAccount",
                table: "EventAccount",
                columns: new[] { "EventId", "AccountId", "EventRole" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JudgeAssign_AssignedBy",
                table: "JudgeAssign",
                column: "AssignedBy");

            migrationBuilder.CreateIndex(
                name: "IX_JudgeAssign_RoundId",
                table: "JudgeAssign",
                column: "RoundId");

            migrationBuilder.CreateIndex(
                name: "UQ_Judge_Round",
                table: "JudgeAssign",
                columns: new[] { "JudgeId", "RoundId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MentorAssign_AssignedBy",
                table: "MentorAssign",
                column: "AssignedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MentorAssign_TrackId",
                table: "MentorAssign",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "UQ_Mentor_Track",
                table: "MentorAssign",
                columns: new[] { "MentorId", "TrackId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notification_AccountId",
                table: "Notification",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "UQ_Prize_Event_RankPosition",
                table: "Prize",
                columns: new[] { "EventId", "RankPosition" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ranking_RoundId",
                table: "Ranking",
                column: "RoundId");

            migrationBuilder.CreateIndex(
                name: "UQ_Ranking_Team_Round",
                table: "Ranking",
                columns: new[] { "TeamId", "RoundId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Round_CreatedBy",
                table: "Round",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Round_TrackId",
                table: "Round",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_Round_UpdatedBy",
                table: "Round",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RoundTeam_AssignedBy",
                table: "RoundTeam",
                column: "AssignedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RoundTeam_TeamId",
                table: "RoundTeam",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_RoundTeam_TopicId",
                table: "RoundTeam",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "UQ_RoundTeam_Round_Team",
                table: "RoundTeam",
                columns: new[] { "RoundId", "TeamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScoreRecord_CriterionId",
                table: "ScoreRecord",
                column: "CriterionId");

            migrationBuilder.CreateIndex(
                name: "IX_ScoreRecord_JudgeId",
                table: "ScoreRecord",
                column: "JudgeId");

            migrationBuilder.CreateIndex(
                name: "IX_ScoreRecord_SubmissionId",
                table: "ScoreRecord",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "UQ_ScoreRecord",
                table: "ScoreRecord",
                columns: new[] { "SubmissionId", "JudgeId", "CriterionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Submission_DisqualifiedBy",
                table: "Submission",
                column: "DisqualifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Submission_RoundId",
                table: "Submission",
                column: "RoundId");

            migrationBuilder.CreateIndex(
                name: "UQ_Submission_Team_Round",
                table: "Submission",
                columns: new[] { "TeamId", "RoundId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Team_CreatedBy",
                table: "Team",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Team_LeaderId",
                table: "Team",
                column: "LeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_Team_MentorId",
                table: "Team",
                column: "MentorId");

            migrationBuilder.CreateIndex(
                name: "IX_Team_TopicId",
                table: "Team",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_Team_TrackId",
                table: "Team",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_Team_UpdatedBy",
                table: "Team",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMember_CreatedBy",
                table: "TeamMember",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMember_TeamId",
                table: "TeamMember",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMember_UpdatedBy",
                table: "TeamMember",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TieBreakScoreRecord_CriterionId",
                table: "TieBreakScoreRecord",
                column: "CriterionId");

            migrationBuilder.CreateIndex(
                name: "IX_TieBreakScoreRecord_JudgeId",
                table: "TieBreakScoreRecord",
                column: "JudgeId");

            migrationBuilder.CreateIndex(
                name: "UQ_TieBreakScoreRecord",
                table: "TieBreakScoreRecord",
                columns: new[] { "TieBreakSubmissionId", "JudgeId", "CriterionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TieBreakSession_Round_Rank_Status",
                table: "TieBreakSession",
                columns: new[] { "RoundId", "RankPosition", "Status" });

            migrationBuilder.CreateIndex(
                name: "UQ_TieBreakSession_Open_Round_Rank",
                table: "TieBreakSession",
                columns: new[] { "RoundId", "RankPosition" },
                unique: true,
                filter: "[Status] = 'PendingScoring'");

            migrationBuilder.CreateIndex(
                name: "IX_TieBreakSubmission_SubmissionId",
                table: "TieBreakSubmission",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "UQ_TieBreakSubmission_Session_Submission",
                table: "TieBreakSubmission",
                columns: new[] { "TieBreakSessionId", "SubmissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Topic_CreatedBy",
                table: "Topic",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Topic_EventId",
                table: "Topic",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Topic_RoundId",
                table: "Topic",
                column: "RoundId");

            migrationBuilder.CreateIndex(
                name: "IX_Topic_UpdatedBy",
                table: "Topic",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Track_CreatedBy",
                table: "Track",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Track_UpdatedBy",
                table: "Track",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "UQ_Track_Event_Final",
                table: "Track",
                column: "EventId",
                unique: true,
                filter: "[IsFinal] = 1 AND [IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLog");

            migrationBuilder.DropTable(
                name: "CriterionTemplateItem");

            migrationBuilder.DropTable(
                name: "EventAccount");

            migrationBuilder.DropTable(
                name: "JudgeAssign");

            migrationBuilder.DropTable(
                name: "MentorAssign");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "Prize");

            migrationBuilder.DropTable(
                name: "Ranking");

            migrationBuilder.DropTable(
                name: "RoundTeam");

            migrationBuilder.DropTable(
                name: "SchemaVersions");

            migrationBuilder.DropTable(
                name: "ScoreRecord");

            migrationBuilder.DropTable(
                name: "TeamMember");

            migrationBuilder.DropTable(
                name: "TieBreakScoreRecord");

            migrationBuilder.DropTable(
                name: "CriterionTemplate");

            migrationBuilder.DropTable(
                name: "Criterion");

            migrationBuilder.DropTable(
                name: "TieBreakSubmission");

            migrationBuilder.DropTable(
                name: "Submission");

            migrationBuilder.DropTable(
                name: "TieBreakSession");

            migrationBuilder.DropTable(
                name: "Team");

            migrationBuilder.DropTable(
                name: "Topic");

            migrationBuilder.DropTable(
                name: "Round");

            migrationBuilder.DropTable(
                name: "Track");

            migrationBuilder.DropTable(
                name: "Event");

            migrationBuilder.DropTable(
                name: "Account");
        }
    }
}
