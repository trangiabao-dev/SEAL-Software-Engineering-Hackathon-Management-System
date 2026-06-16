-- ==========================================
-- SCRIPT 001: KHỞI TẠO HỆ THỐNG SEAL
-- Schema version: 2.0 (có EventAccount)
-- ==========================================

-- 1. Account
CREATE TABLE Account (
    Id           UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Username     NVARCHAR(255)    NOT NULL UNIQUE,
    PasswordHash VARCHAR(500)     NOT NULL,
    Email        VARCHAR(255)     NOT NULL UNIQUE,
    SystemRole   VARCHAR(50)      NOT NULL DEFAULT 'Leader',
    IsDeleted    BIT              NOT NULL DEFAULT 0,
    CreatedAt    DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt    DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME()
);

-- 2. Event
CREATE TABLE [Event] (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Name        NVARCHAR(255)    NOT NULL,
    Description NVARCHAR(MAX)    NULL,
    StartDate   DATETIME2        NOT NULL,
    EndDate     DATETIME2        NOT NULL,
    Status      VARCHAR(50)      NOT NULL DEFAULT 'Draft',
    IsDeleted   BIT              NOT NULL DEFAULT 0,
    CreatedAt   DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt   DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy   UNIQUEIDENTIFIER NULL REFERENCES Account(Id),
    UpdatedBy   UNIQUEIDENTIFIER NULL REFERENCES Account(Id)
);

-- 3. EventAccount
CREATE TABLE EventAccount (
    Id             INT IDENTITY(1,1) PRIMARY KEY,
    EventId        INT               NOT NULL REFERENCES [Event](Id),
    AccountId      UNIQUEIDENTIFIER  NOT NULL REFERENCES Account(Id),
    EventRole      VARCHAR(50)       NOT NULL,
    JudgeType      VARCHAR(50)       NULL,
    [Status]       VARCHAR(50)       NOT NULL DEFAULT 'Pending',
    RejectedReason NVARCHAR(500)     NULL,
    AssignedBy     UNIQUEIDENTIFIER  NULL REFERENCES Account(Id),
    AssignedAt     DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_EventAccount UNIQUE (EventId, AccountId, EventRole)
);
CREATE NONCLUSTERED INDEX IX_EventAccount_AccountId ON EventAccount(AccountId);
CREATE NONCLUSTERED INDEX IX_EventAccount_EventId   ON EventAccount(EventId);

-- 4. Track
CREATE TABLE Track (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    EventId     INT               NOT NULL REFERENCES [Event](Id),
    Name        NVARCHAR(255)     NOT NULL,
    Description NVARCHAR(MAX)     NULL,
    MaxTeams    INT               NULL,
    IsDeleted   BIT               NOT NULL DEFAULT 0,
    CreatedAt   DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt   DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy   UNIQUEIDENTIFIER  NULL REFERENCES Account(Id),
    UpdatedBy   UNIQUEIDENTIFIER  NULL REFERENCES Account(Id)
);

-- 5. MentorAssign
CREATE TABLE MentorAssign (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    MentorId    UNIQUEIDENTIFIER  NOT NULL REFERENCES Account(Id),
    TrackId     INT               NOT NULL REFERENCES Track(Id),
    AssignedAt  DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
    AssignedBy  UNIQUEIDENTIFIER  NOT NULL REFERENCES Account(Id),
    CONSTRAINT UQ_Mentor_Track UNIQUE (MentorId, TrackId)
);

-- 6. Prize
CREATE TABLE Prize (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    TrackId      INT               NOT NULL REFERENCES Track(Id),
    Name         NVARCHAR(255)     NOT NULL,
    Description  NVARCHAR(MAX)     NULL,
    RankPosition INT               NOT NULL,
    Amount       DECIMAL(18,2)     NULL
);

-- 7. Round
CREATE TABLE Round (
    Id             INT IDENTITY(1,1) PRIMARY KEY,
    TrackId        INT               NOT NULL REFERENCES Track(Id),
    Name           NVARCHAR(255)     NOT NULL,
    OrderIndex     INT               NOT NULL,
    StartTime      DATETIME2         NOT NULL,
    EndTime        DATETIME2         NOT NULL,
    AdvancingSlots INT               NULL,
    Status         VARCHAR(50)       NOT NULL DEFAULT 'Upcoming',
    CreatedAt      DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt      DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy      UNIQUEIDENTIFIER  NULL REFERENCES Account(Id),
    UpdatedBy      UNIQUEIDENTIFIER  NULL REFERENCES Account(Id)
);

-- 8. CriterionTemplate
CREATE TABLE CriterionTemplate (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Name        NVARCHAR(255)     NOT NULL,
    Description NVARCHAR(MAX)     NULL,
    CreatedAt   DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy   UNIQUEIDENTIFIER  NULL REFERENCES Account(Id)
);

-- 9. CriterionTemplateItem
CREATE TABLE CriterionTemplateItem (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    TemplateId  INT               NOT NULL REFERENCES CriterionTemplate(Id),
    Name        NVARCHAR(255)     NOT NULL,
    Description NVARCHAR(MAX)     NULL,
    MaxScore    FLOAT             NOT NULL,
    Weight      FLOAT             NOT NULL
);

-- 10. Topic
CREATE TABLE Topic (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    RoundId       INT               NOT NULL REFERENCES Round(Id),
    Title         NVARCHAR(255)     NOT NULL,
    Description   NVARCHAR(MAX)     NULL,
    Requirements  NVARCHAR(MAX)     NULL,
    AttachmentUrl VARCHAR(1000)     NULL,
    CreatedAt     DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt     DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy     UNIQUEIDENTIFIER  NULL REFERENCES Account(Id),
    UpdatedBy     UNIQUEIDENTIFIER  NULL REFERENCES Account(Id)
);

-- 11. Team
CREATE TABLE Team (
    Id              UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TrackId         INT               NOT NULL REFERENCES Track(Id),
    LeaderId        UNIQUEIDENTIFIER  NOT NULL REFERENCES Account(Id),
    MentorId        UNIQUEIDENTIFIER  NULL REFERENCES Account(Id),
    TopicId         INT               NULL REFERENCES Topic(Id),
    TeamName        NVARCHAR(255)     NOT NULL,
    University      NVARCHAR(255)     NOT NULL,
    GithubRepoLink  VARCHAR(500)      NULL,
    Status          VARCHAR(50)       NOT NULL DEFAULT 'Pending',
    IsDeleted       BIT               NOT NULL DEFAULT 0,
    CreatedAt       DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy       UNIQUEIDENTIFIER  NULL REFERENCES Account(Id),
    UpdatedBy       UNIQUEIDENTIFIER  NULL REFERENCES Account(Id)
);

-- 12. TeamMember
CREATE TABLE TeamMember (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    TeamId      UNIQUEIDENTIFIER  NOT NULL REFERENCES Team(Id),
    FullName    NVARCHAR(255)     NOT NULL,
    StudentCode VARCHAR(50)       NOT NULL,
    Email       VARCHAR(255)      NOT NULL,
    University  NVARCHAR(255)     NOT NULL,
    Phone       VARCHAR(20)       NOT NULL,
    IsLeader    BIT               NOT NULL DEFAULT 0,
    IsFPTStudent BIT              NOT NULL DEFAULT 1,
    CreatedAt   DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt   DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy   UNIQUEIDENTIFIER  NULL REFERENCES Account(Id),
    UpdatedBy   UNIQUEIDENTIFIER  NULL REFERENCES Account(Id)
);

-- 13. Submission
CREATE TABLE Submission (
    Id               UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TeamId           UNIQUEIDENTIFIER  NOT NULL REFERENCES Team(Id),
    RoundId          INT               NOT NULL REFERENCES Round(Id),
    DemoUrl          VARCHAR(1000)     NULL,
    ReportUrl        VARCHAR(1000)     NULL,
    AiEvaluation     NVARCHAR(MAX)     NULL,
    IsDisqualified   BIT               NOT NULL DEFAULT 0,
    DisqualifyReason NVARCHAR(500)     NULL,
    DisqualifiedAt   DATETIME2         NULL,
    DisqualifiedBy   UNIQUEIDENTIFIER  NULL REFERENCES Account(Id),
    CreatedAt        DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Submission_Team_Round UNIQUE (TeamId, RoundId)
);
CREATE NONCLUSTERED INDEX IX_Submission_RoundId ON Submission(RoundId);

-- 14. JudgeAssign
CREATE TABLE JudgeAssign (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    JudgeId     UNIQUEIDENTIFIER  NOT NULL REFERENCES Account(Id),
    RoundId     INT               NOT NULL REFERENCES Round(Id),
    AssignedAt  DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
    AssignedBy  UNIQUEIDENTIFIER  NOT NULL REFERENCES Account(Id),
    CONSTRAINT UQ_Judge_Round UNIQUE (JudgeId, RoundId)
);

-- 15. Criterion
CREATE TABLE Criterion (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    RoundId     INT               NOT NULL REFERENCES Round(Id),
    Name        NVARCHAR(255)     NOT NULL,
    Description NVARCHAR(MAX)     NULL,
    MaxScore    FLOAT             NOT NULL,
    Weight      FLOAT             NOT NULL,
    CreatedAt   DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt   DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy   UNIQUEIDENTIFIER  NULL REFERENCES Account(Id),
    UpdatedBy   UNIQUEIDENTIFIER  NULL REFERENCES Account(Id)
);

-- 16. ScoreRecord
CREATE TABLE ScoreRecord (
    Id           UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    SubmissionId UNIQUEIDENTIFIER  NOT NULL REFERENCES Submission(Id),
    JudgeId      UNIQUEIDENTIFIER  NOT NULL REFERENCES Account(Id),
    CriterionId  INT               NOT NULL REFERENCES Criterion(Id),
    Score        FLOAT             NOT NULL,
    Comment      NVARCHAR(MAX)     NULL,
    IsCalibration BIT              NOT NULL DEFAULT 0,
    ScoredAt     DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt    DATETIME2         NULL,
    CONSTRAINT UQ_ScoreRecord UNIQUE (SubmissionId, JudgeId, CriterionId)
);
CREATE NONCLUSTERED INDEX IX_ScoreRecord_SubmissionId ON ScoreRecord(SubmissionId);

-- 17. Ranking
CREATE TABLE Ranking (
    Id           UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TeamId       UNIQUEIDENTIFIER  NOT NULL REFERENCES Team(Id),
    RoundId      INT               NOT NULL REFERENCES Round(Id),
    TotalScore   FLOAT             NOT NULL,
    RankPosition INT               NOT NULL,
    IsAdvancing  BIT               NOT NULL DEFAULT 0,
    CalculatedAt DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Ranking_Team_Round UNIQUE (TeamId, RoundId)
);

-- 18. Notification
CREATE TABLE Notification (
    Id        UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    AccountId UNIQUEIDENTIFIER  NOT NULL REFERENCES Account(Id),
    Title     NVARCHAR(255)     NOT NULL,
    Message   NVARCHAR(MAX)     NOT NULL,
    Type      VARCHAR(50)       NOT NULL,
    IsRead    BIT               NOT NULL DEFAULT 0,
    CreatedAt DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME()
);

-- 19. AuditLog
CREATE TABLE AuditLog (
    Id          UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PerformedBy UNIQUEIDENTIFIER  NOT NULL REFERENCES Account(Id),
    Action      VARCHAR(100)      NOT NULL,
    EntityName  VARCHAR(100)      NOT NULL,
    EntityId    VARCHAR(100)      NOT NULL,
    OldValues   NVARCHAR(MAX)     NULL,
    NewValues   NVARCHAR(MAX)     NULL,
    CreatedAt   DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME()
);

-- ==========================================
-- SEED: Coordinator mặc định
-- Password: Coordinator@2026
-- Hash này phải được thay bằng BCrypt hash thật trước khi deploy
-- ==========================================
INSERT INTO Account (Id, Username, PasswordHash, Email, SystemRole)
VALUES (
    NEWID(),
    'coordinator',
    '$2a$11$replacethiswithrealbcrypthashxxxxxxxxxxxxxxxxxxxxxxxx',
    'coordinator@seal.edu.vn',
    'Coordinator'
);
