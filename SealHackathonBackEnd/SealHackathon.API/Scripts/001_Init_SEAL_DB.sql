-- ==========================================
-- SCRIPT 001: COMPLETE SQUASHED MIGRATION (INIT SEAL DB)
-- Contains all 24 tables, constraints, indexes, and initial Seed data up to present state.
-- All table creations are idempotent (safe against existing tables).
-- ==========================================

-- 1. Account
IF OBJECT_ID('dbo.Account', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Account (
        Id                UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Username          NVARCHAR(255)    NOT NULL UNIQUE,
        PasswordHash      VARCHAR(500)     NOT NULL,
        Email             VARCHAR(255)     NOT NULL UNIQUE,
        SystemRole        VARCHAR(50)      NOT NULL DEFAULT 'Leader',
        IsDeleted         BIT              NOT NULL DEFAULT 0,
        EmailConfirmToken VARCHAR(500)     NULL,
        TokenExpiresAt    DATETIME2        NULL,
        CreatedAt         DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt         DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME()
    );
END;

-- 2. Event
IF OBJECT_ID('dbo.Event', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.[Event] (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        Name        NVARCHAR(255)    NOT NULL,
        Description NVARCHAR(MAX)    NULL,
        BannerUrl   VARCHAR(1000)    NULL,
        Location    NVARCHAR(500)    NULL,
        IsOnline    BIT              NULL,
        StartDate   DATETIME2        NOT NULL,
        EndDate     DATETIME2        NOT NULL,
        Status      VARCHAR(50)      NOT NULL DEFAULT 'Draft',
        IsDeleted   BIT              NOT NULL DEFAULT 0,
        CreatedAt   DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt   DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
        CreatedBy   UNIQUEIDENTIFIER NULL REFERENCES dbo.Account(Id),
        UpdatedBy   UNIQUEIDENTIFIER NULL REFERENCES dbo.Account(Id)
    );
END;

-- 3. EventAccount
IF OBJECT_ID('dbo.EventAccount', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EventAccount (
        Id             INT IDENTITY(1,1) PRIMARY KEY,
        EventId        INT               NOT NULL REFERENCES dbo.[Event](Id),
        AccountId      UNIQUEIDENTIFIER  NOT NULL REFERENCES dbo.Account(Id),
        EventRole      NVARCHAR(50)      NOT NULL,
        JudgeType      NVARCHAR(100)     NULL,
        [Status]       NVARCHAR(50)      NOT NULL DEFAULT 'Pending',
        RejectedReason NVARCHAR(500)     NULL,
        AssignedBy     UNIQUEIDENTIFIER  NULL REFERENCES dbo.Account(Id),
        AssignedAt     DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_EventAccount UNIQUE (EventId, AccountId, EventRole)
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EventAccount_AccountId' AND object_id = OBJECT_ID('dbo.EventAccount'))
    CREATE NONCLUSTERED INDEX IX_EventAccount_AccountId ON dbo.EventAccount(AccountId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EventAccount_EventId' AND object_id = OBJECT_ID('dbo.EventAccount'))
    CREATE NONCLUSTERED INDEX IX_EventAccount_EventId ON dbo.EventAccount(EventId);

-- 4. Track
IF OBJECT_ID('dbo.Track', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Track (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        EventId     INT               NOT NULL REFERENCES dbo.[Event](Id),
        Name        NVARCHAR(255)     NOT NULL,
        Description NVARCHAR(MAX)     NULL,
        MaxTeams    INT               NULL,
        MaxMembers  INT               NULL,
        IsFinal     BIT               NOT NULL DEFAULT 0,
        IsDeleted   BIT               NOT NULL DEFAULT 0,
        CreatedAt   DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt   DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
        CreatedBy   UNIQUEIDENTIFIER  NULL REFERENCES dbo.Account(Id),
        UpdatedBy   UNIQUEIDENTIFIER  NULL REFERENCES dbo.Account(Id)
    );
END;

-- 5. MentorAssign
IF OBJECT_ID('dbo.MentorAssign', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.MentorAssign (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        MentorId    UNIQUEIDENTIFIER  NOT NULL REFERENCES dbo.Account(Id),
        TrackId     INT               NOT NULL REFERENCES dbo.Track(Id),
        AssignedAt  DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
        AssignedBy  UNIQUEIDENTIFIER  NOT NULL REFERENCES dbo.Account(Id),
        CONSTRAINT UQ_Mentor_Track UNIQUE (MentorId, TrackId)
    );
END;

-- 6. Prize
IF OBJECT_ID('dbo.Prize', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Prize (
        Id           INT IDENTITY(1,1) PRIMARY KEY,
        EventId      INT               NOT NULL REFERENCES dbo.[Event](Id),
        Name         NVARCHAR(255)     NOT NULL,
        Description  NVARCHAR(MAX)     NULL,
        RankPosition INT               NOT NULL,
        Amount       DECIMAL(18,2)     NULL
    );
END;

-- 7. Round
IF OBJECT_ID('dbo.Round', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Round (
        Id             INT IDENTITY(1,1) PRIMARY KEY,
        TrackId        INT               NOT NULL REFERENCES dbo.Track(Id),
        Name           NVARCHAR(255)     NOT NULL,
        OrderIndex     INT               NOT NULL,
        StartTime      DATETIME2         NOT NULL,
        EndTime        DATETIME2         NOT NULL,
        AdvancingSlots INT               NULL,
        Status         VARCHAR(50)       NOT NULL DEFAULT 'Upcoming',
        CreatedAt      DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt      DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
        CreatedBy      UNIQUEIDENTIFIER  NULL REFERENCES dbo.Account(Id),
        UpdatedBy      UNIQUEIDENTIFIER  NULL REFERENCES dbo.Account(Id)
    );
END;

-- 8. CriterionTemplate
IF OBJECT_ID('dbo.CriterionTemplate', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CriterionTemplate (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        Name        NVARCHAR(255)     NOT NULL,
        Description NVARCHAR(MAX)     NULL,
        CreatedAt   DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
        CreatedBy   UNIQUEIDENTIFIER  NULL REFERENCES dbo.Account(Id)
    );
END;

-- 9. CriterionTemplateItem
IF OBJECT_ID('dbo.CriterionTemplateItem', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CriterionTemplateItem (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        TemplateId  INT               NOT NULL REFERENCES dbo.CriterionTemplate(Id),
        Name        NVARCHAR(255)     NOT NULL,
        Description NVARCHAR(MAX)     NULL,
        MaxScore    FLOAT             NOT NULL,
        Weight      FLOAT             NOT NULL
    );
END;

-- 10. Topic
IF OBJECT_ID('dbo.Topic', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Topic (
        Id            INT IDENTITY(1,1) PRIMARY KEY,
        RoundId       INT               NOT NULL REFERENCES dbo.Round(Id),
        EventId       INT               NULL REFERENCES dbo.[Event](Id),
        Title         NVARCHAR(255)     NOT NULL,
        Description   NVARCHAR(MAX)     NULL,
        Requirements  NVARCHAR(MAX)     NULL,
        AttachmentUrl VARCHAR(1000)     NULL,
        CreatedAt     DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt     DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
        CreatedBy     UNIQUEIDENTIFIER  NULL REFERENCES dbo.Account(Id),
        UpdatedBy     UNIQUEIDENTIFIER  NULL REFERENCES dbo.Account(Id)
    );
END;

-- 11. Team
IF OBJECT_ID('dbo.Team', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Team (
        Id               UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TrackId          INT               NOT NULL REFERENCES dbo.Track(Id),
        LeaderId         UNIQUEIDENTIFIER  NOT NULL REFERENCES dbo.Account(Id),
        MentorId         UNIQUEIDENTIFIER  NULL REFERENCES dbo.Account(Id),
        TopicId          INT               NULL REFERENCES dbo.Topic(Id),
        TeamName         NVARCHAR(255)     NOT NULL,
        University       NVARCHAR(255)     NOT NULL,
        GithubRepoLink   VARCHAR(500)      NULL,
        Status           VARCHAR(50)       NOT NULL DEFAULT 'Pending',
        RejectedReason   NVARCHAR(500)     NULL,
        DisqualifyReason NVARCHAR(500)     NULL,
        IsDeleted        BIT               NOT NULL DEFAULT 0,
        CreatedAt        DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt        DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
        CreatedBy        UNIQUEIDENTIFIER  NULL REFERENCES dbo.Account(Id),
        UpdatedBy        UNIQUEIDENTIFIER  NULL REFERENCES dbo.Account(Id)
    );
END;

-- 12. TeamMember
IF OBJECT_ID('dbo.TeamMember', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TeamMember (
        Id           INT IDENTITY(1,1) PRIMARY KEY,
        TeamId       UNIQUEIDENTIFIER  NOT NULL REFERENCES dbo.Team(Id),
        FullName     NVARCHAR(255)     NOT NULL,
        StudentCode  VARCHAR(50)       NOT NULL,
        Email        VARCHAR(255)      NOT NULL,
        University   NVARCHAR(255)     NULL,
        Phone        VARCHAR(20)       NOT NULL,
        IsLeader     BIT               NOT NULL DEFAULT 0,
        IsFPTStudent BIT               NOT NULL DEFAULT 1,
        CreatedAt    DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt    DATETIME2         NULL,
        CreatedBy    UNIQUEIDENTIFIER  NULL REFERENCES dbo.Account(Id),
        UpdatedBy    UNIQUEIDENTIFIER  NULL REFERENCES dbo.Account(Id)
    );
END;

-- 13. Submission
IF OBJECT_ID('dbo.Submission', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Submission (
        Id               UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TeamId           UNIQUEIDENTIFIER  NOT NULL REFERENCES dbo.Team(Id),
        RoundId          INT               NOT NULL REFERENCES dbo.Round(Id),
        DemoUrl          VARCHAR(1000)     NULL,
        PresentationUrl  VARCHAR(1000)     NULL,
        CanEdit          BIT               NOT NULL DEFAULT 1,
        IsDisqualified   BIT               NOT NULL DEFAULT 0,
        DisqualifyReason NVARCHAR(500)     NULL,
        DisqualifiedAt   DATETIME2         NULL,
        DisqualifiedBy   UNIQUEIDENTIFIER  NULL REFERENCES dbo.Account(Id),
        CreatedAt        DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt        DATETIME2         NULL,
        CONSTRAINT UQ_Submission_Team_Round UNIQUE (TeamId, RoundId)
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Submission_RoundId' AND object_id = OBJECT_ID('dbo.Submission'))
    CREATE NONCLUSTERED INDEX IX_Submission_RoundId ON dbo.Submission(RoundId);

-- 14. JudgeAssign
IF OBJECT_ID('dbo.JudgeAssign', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.JudgeAssign (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        JudgeId     UNIQUEIDENTIFIER  NOT NULL REFERENCES dbo.Account(Id),
        RoundId     INT               NOT NULL REFERENCES dbo.Round(Id),
        AssignedAt  DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
        AssignedBy  UNIQUEIDENTIFIER  NOT NULL REFERENCES dbo.Account(Id),
        CONSTRAINT UQ_Judge_Round UNIQUE (JudgeId, RoundId)
    );
END;

-- 15. Criterion
IF OBJECT_ID('dbo.Criterion', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Criterion (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        RoundId     INT               NOT NULL REFERENCES dbo.Round(Id),
        Name        NVARCHAR(255)     NOT NULL,
        Description NVARCHAR(MAX)     NULL,
        MaxScore    FLOAT             NOT NULL,
        Weight      FLOAT             NOT NULL,
        CreatedAt   DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt   DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
        CreatedBy   UNIQUEIDENTIFIER  NULL REFERENCES dbo.Account(Id),
        UpdatedBy   UNIQUEIDENTIFIER  NULL REFERENCES dbo.Account(Id)
    );
END;

-- 16. ScoreRecord
IF OBJECT_ID('dbo.ScoreRecord', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ScoreRecord (
        Id            UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        SubmissionId  UNIQUEIDENTIFIER  NOT NULL REFERENCES dbo.Submission(Id),
        JudgeId       UNIQUEIDENTIFIER  NOT NULL REFERENCES dbo.Account(Id),
        CriterionId   INT               NOT NULL REFERENCES dbo.Criterion(Id),
        Score         FLOAT             NOT NULL,
        Comment       NVARCHAR(MAX)     NULL,
        ScoredAt      DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt     DATETIME2         NULL,
        CONSTRAINT UQ_ScoreRecord UNIQUE (SubmissionId, JudgeId, CriterionId)
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScoreRecord_SubmissionId' AND object_id = OBJECT_ID('dbo.ScoreRecord'))
    CREATE NONCLUSTERED INDEX IX_ScoreRecord_SubmissionId ON dbo.ScoreRecord(SubmissionId);

-- 17. Ranking
IF OBJECT_ID('dbo.Ranking', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Ranking (
        Id           UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TeamId       UNIQUEIDENTIFIER  NOT NULL REFERENCES dbo.Team(Id),
        RoundId      INT               NOT NULL REFERENCES dbo.Round(Id),
        TotalScore   FLOAT             NOT NULL,
        RankPosition INT               NOT NULL,
        IsAdvancing  BIT               NOT NULL DEFAULT 0,
        CalculatedAt DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_Ranking_Team_Round UNIQUE (TeamId, RoundId)
    );
END;

-- 18. Notification
IF OBJECT_ID('dbo.Notification', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Notification (
        Id        UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        AccountId UNIQUEIDENTIFIER  NOT NULL REFERENCES dbo.Account(Id),
        Title     NVARCHAR(255)     NOT NULL,
        Message   NVARCHAR(MAX)     NOT NULL,
        Type      VARCHAR(50)       NOT NULL,
        IsRead    BIT               NOT NULL DEFAULT 0,
        CreatedAt DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME()
    );
END;

-- 19. AuditLog
IF OBJECT_ID('dbo.AuditLog', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLog (
        Id          UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        PerformedBy UNIQUEIDENTIFIER  NOT NULL REFERENCES dbo.Account(Id),
        Action      VARCHAR(100)      NOT NULL,
        EntityName  VARCHAR(100)      NOT NULL,
        EntityId    VARCHAR(100)      NOT NULL,
        OldValues   NVARCHAR(MAX)     NULL,
        NewValues   NVARCHAR(MAX)     NULL,
        CreatedAt   DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME()
    );
END;

-- 20. RoundTeam
IF OBJECT_ID('dbo.RoundTeam', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RoundTeam (
        Id         UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_RoundTeam PRIMARY KEY CONSTRAINT DF_RoundTeam_Id DEFAULT NEWID(),
        RoundId    INT NOT NULL REFERENCES dbo.Round(Id),
        TeamId     UNIQUEIDENTIFIER NOT NULL REFERENCES dbo.Team(Id),
        TopicId    INT NULL REFERENCES dbo.Topic(Id),
        AssignedAt DATETIME2(7) NOT NULL CONSTRAINT DF_RoundTeam_AssignedAt DEFAULT SYSUTCDATETIME(),
        AssignedBy UNIQUEIDENTIFIER NULL REFERENCES dbo.Account(Id),
        CreatedAt  DATETIME2(7) NOT NULL CONSTRAINT DF_RoundTeam_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt  DATETIME2(7) NOT NULL CONSTRAINT DF_RoundTeam_UpdatedAt DEFAULT SYSUTCDATETIME()
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_RoundTeam_Round_Team' AND object_id = OBJECT_ID('dbo.RoundTeam'))
    CREATE UNIQUE NONCLUSTERED INDEX UQ_RoundTeam_Round_Team ON dbo.RoundTeam(RoundId, TeamId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RoundTeam_TeamId' AND object_id = OBJECT_ID('dbo.RoundTeam'))
    CREATE NONCLUSTERED INDEX IX_RoundTeam_TeamId ON dbo.RoundTeam(TeamId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RoundTeam_TopicId' AND object_id = OBJECT_ID('dbo.RoundTeam'))
    CREATE NONCLUSTERED INDEX IX_RoundTeam_TopicId ON dbo.RoundTeam(TopicId);

-- 21. TieBreakSession
IF OBJECT_ID('dbo.TieBreakSession', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TieBreakSession (
        Id           UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_TieBreakSession PRIMARY KEY CONSTRAINT DF_TieBreakSession_Id DEFAULT NEWID(),
        RoundId      INT NOT NULL REFERENCES dbo.[Round](Id),
        RankPosition INT NOT NULL,
        Status       VARCHAR(50) NOT NULL CONSTRAINT DF_TieBreakSession_Status DEFAULT 'PendingScoring',
        CreatedAt    DATETIME2(7) NOT NULL CONSTRAINT DF_TieBreakSession_CreatedAt DEFAULT SYSUTCDATETIME(),
        CompletedAt  DATETIME2(7) NULL
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TieBreakSession_Round_Rank_Status' AND object_id = OBJECT_ID('dbo.TieBreakSession'))
    CREATE NONCLUSTERED INDEX IX_TieBreakSession_Round_Rank_Status ON dbo.TieBreakSession(RoundId, RankPosition, Status);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_TieBreakSession_Open_Round_Rank' AND object_id = OBJECT_ID('dbo.TieBreakSession'))
    CREATE UNIQUE NONCLUSTERED INDEX UQ_TieBreakSession_Open_Round_Rank ON dbo.TieBreakSession(RoundId, RankPosition) WHERE Status = 'PendingScoring';

-- 22. TieBreakSubmission
IF OBJECT_ID('dbo.TieBreakSubmission', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TieBreakSubmission (
        Id                UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_TieBreakSubmission PRIMARY KEY CONSTRAINT DF_TieBreakSubmission_Id DEFAULT NEWID(),
        TieBreakSessionId UNIQUEIDENTIFIER NOT NULL REFERENCES dbo.TieBreakSession(Id),
        SubmissionId      UNIQUEIDENTIFIER NOT NULL REFERENCES dbo.Submission(Id),
        CreatedAt         DATETIME2(7) NOT NULL CONSTRAINT DF_TieBreakSubmission_CreatedAt DEFAULT SYSUTCDATETIME()
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_TieBreakSubmission_Session_Submission' AND object_id = OBJECT_ID('dbo.TieBreakSubmission'))
    CREATE UNIQUE NONCLUSTERED INDEX UQ_TieBreakSubmission_Session_Submission ON dbo.TieBreakSubmission(TieBreakSessionId, SubmissionId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TieBreakSubmission_SubmissionId' AND object_id = OBJECT_ID('dbo.TieBreakSubmission'))
    CREATE NONCLUSTERED INDEX IX_TieBreakSubmission_SubmissionId ON dbo.TieBreakSubmission(SubmissionId);

-- 23. TieBreakScoreRecord
IF OBJECT_ID('dbo.TieBreakScoreRecord', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TieBreakScoreRecord (
        Id                   UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_TieBreakScoreRecord PRIMARY KEY CONSTRAINT DF_TieBreakScoreRecord_Id DEFAULT NEWID(),
        TieBreakSubmissionId UNIQUEIDENTIFIER NOT NULL REFERENCES dbo.TieBreakSubmission(Id),
        JudgeId              UNIQUEIDENTIFIER NOT NULL REFERENCES dbo.Account(Id),
        CriterionId          INT NOT NULL REFERENCES dbo.Criterion(Id),
        Score                FLOAT NOT NULL,
        Comment              NVARCHAR(MAX) NULL,
        ScoredAt             DATETIME2(7) NOT NULL CONSTRAINT DF_TieBreakScoreRecord_ScoredAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt            DATETIME2(7) NULL
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_TieBreakScoreRecord' AND object_id = OBJECT_ID('dbo.TieBreakScoreRecord'))
    CREATE UNIQUE NONCLUSTERED INDEX UQ_TieBreakScoreRecord ON dbo.TieBreakScoreRecord(TieBreakSubmissionId, JudgeId, CriterionId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TieBreakScoreRecord_JudgeId' AND object_id = OBJECT_ID('dbo.TieBreakScoreRecord'))
    CREATE NONCLUSTERED INDEX IX_TieBreakScoreRecord_JudgeId ON dbo.TieBreakScoreRecord(JudgeId);

-- ==========================================
-- PERFORMANCE INDEXES
-- ==========================================
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Event_Status_IsDeleted' AND object_id = OBJECT_ID('dbo.Event'))
    CREATE NONCLUSTERED INDEX IX_Event_Status_IsDeleted ON dbo.[Event](Status, IsDeleted);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Track_EventId_IsDeleted' AND object_id = OBJECT_ID('dbo.Track'))
    CREATE NONCLUSTERED INDEX IX_Track_EventId_IsDeleted ON dbo.Track(EventId, IsDeleted);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Round_TrackId_Status' AND object_id = OBJECT_ID('dbo.Round'))
    CREATE NONCLUSTERED INDEX IX_Round_TrackId_Status ON dbo.Round(TrackId, Status);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Topic_RoundId' AND object_id = OBJECT_ID('dbo.Topic'))
    CREATE NONCLUSTERED INDEX IX_Topic_RoundId ON dbo.Topic(RoundId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Team_LeaderId_IsDeleted_TrackId' AND object_id = OBJECT_ID('dbo.Team'))
    CREATE NONCLUSTERED INDEX IX_Team_LeaderId_IsDeleted_TrackId ON dbo.Team(LeaderId, IsDeleted, TrackId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Team_TrackId_Status_IsDeleted' AND object_id = OBJECT_ID('dbo.Team'))
    CREATE NONCLUSTERED INDEX IX_Team_TrackId_Status_IsDeleted ON dbo.Team(TrackId, Status, IsDeleted);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TeamMember_TeamId' AND object_id = OBJECT_ID('dbo.TeamMember'))
    CREATE NONCLUSTERED INDEX IX_TeamMember_TeamId ON dbo.TeamMember(TeamId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TeamMember_Email' AND object_id = OBJECT_ID('dbo.TeamMember'))
    CREATE NONCLUSTERED INDEX IX_TeamMember_Email ON dbo.TeamMember(Email);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TeamMember_StudentCode' AND object_id = OBJECT_ID('dbo.TeamMember'))
    CREATE NONCLUSTERED INDEX IX_TeamMember_StudentCode ON dbo.TeamMember(StudentCode);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Submission_RoundId_IsDisqualified' AND object_id = OBJECT_ID('dbo.Submission'))
    CREATE NONCLUSTERED INDEX IX_Submission_RoundId_IsDisqualified ON dbo.Submission(RoundId, IsDisqualified);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Submission_TeamId_IsDisqualified' AND object_id = OBJECT_ID('dbo.Submission'))
    CREATE NONCLUSTERED INDEX IX_Submission_TeamId_IsDisqualified ON dbo.Submission(TeamId, IsDisqualified);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_JudgeAssign_RoundId' AND object_id = OBJECT_ID('dbo.JudgeAssign'))
    CREATE NONCLUSTERED INDEX IX_JudgeAssign_RoundId ON dbo.JudgeAssign(RoundId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Criterion_RoundId' AND object_id = OBJECT_ID('dbo.Criterion'))
    CREATE NONCLUSTERED INDEX IX_Criterion_RoundId ON dbo.Criterion(RoundId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScoreRecord_JudgeId' AND object_id = OBJECT_ID('dbo.ScoreRecord'))
    CREATE NONCLUSTERED INDEX IX_ScoreRecord_JudgeId ON dbo.ScoreRecord(JudgeId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Ranking_RoundId_RankPosition' AND object_id = OBJECT_ID('dbo.Ranking'))
    CREATE NONCLUSTERED INDEX IX_Ranking_RoundId_RankPosition ON dbo.Ranking(RoundId, RankPosition);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AuditLog_PerformedBy_CreatedAt' AND object_id = OBJECT_ID('dbo.AuditLog'))
    CREATE NONCLUSTERED INDEX IX_AuditLog_PerformedBy_CreatedAt ON dbo.AuditLog(PerformedBy, CreatedAt);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AuditLog_EntityName_EntityId_CreatedAt' AND object_id = OBJECT_ID('dbo.AuditLog'))
    CREATE NONCLUSTERED INDEX IX_AuditLog_EntityName_EntityId_CreatedAt ON dbo.AuditLog(EntityName, EntityId, CreatedAt);
