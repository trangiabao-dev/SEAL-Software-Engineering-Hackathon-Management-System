-- 019_Add_TieBreak_Tables.sql
-- Tao cac bang rieng de luu diem cham lai khi xu ly dong hang.

IF OBJECT_ID(N'dbo.TieBreakSession', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TieBreakSession
    (
        Id UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT PK_TieBreakSession PRIMARY KEY
            CONSTRAINT DF_TieBreakSession_Id DEFAULT NEWID(),

        RoundId INT NOT NULL,
        RankPosition INT NOT NULL,
        Status VARCHAR(50) NOT NULL
            CONSTRAINT DF_TieBreakSession_Status DEFAULT 'PendingScoring',

        CreatedAt DATETIME2(7) NOT NULL
            CONSTRAINT DF_TieBreakSession_CreatedAt DEFAULT SYSUTCDATETIME(),
        CompletedAt DATETIME2(7) NULL,

        CONSTRAINT FK_TieBreakSession_Round
            FOREIGN KEY (RoundId) REFERENCES dbo.[Round](Id)
    );
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_TieBreakSession_Round_Rank_Status'
      AND object_id = OBJECT_ID(N'dbo.TieBreakSession')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_TieBreakSession_Round_Rank_Status
    ON dbo.TieBreakSession(RoundId, RankPosition, Status);
END;

-- Moi Round va RankPosition chi duoc co mot phien tie-break dang cho cham.
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UQ_TieBreakSession_Open_Round_Rank'
      AND object_id = OBJECT_ID(N'dbo.TieBreakSession')
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_TieBreakSession_Open_Round_Rank
    ON dbo.TieBreakSession(RoundId, RankPosition)
    WHERE Status = 'PendingScoring';
END;

IF OBJECT_ID(N'dbo.TieBreakSubmission', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TieBreakSubmission
    (
        Id UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT PK_TieBreakSubmission PRIMARY KEY
            CONSTRAINT DF_TieBreakSubmission_Id DEFAULT NEWID(),

        TieBreakSessionId UNIQUEIDENTIFIER NOT NULL,
        SubmissionId UNIQUEIDENTIFIER NOT NULL,

        CreatedAt DATETIME2(7) NOT NULL
            CONSTRAINT DF_TieBreakSubmission_CreatedAt DEFAULT SYSUTCDATETIME(),

        CONSTRAINT FK_TieBreakSubmission_TieBreakSession
            FOREIGN KEY (TieBreakSessionId) REFERENCES dbo.TieBreakSession(Id),
        CONSTRAINT FK_TieBreakSubmission_Submission
            FOREIGN KEY (SubmissionId) REFERENCES dbo.Submission(Id)
    );
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UQ_TieBreakSubmission_Session_Submission'
      AND object_id = OBJECT_ID(N'dbo.TieBreakSubmission')
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_TieBreakSubmission_Session_Submission
    ON dbo.TieBreakSubmission(TieBreakSessionId, SubmissionId);
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_TieBreakSubmission_SubmissionId'
      AND object_id = OBJECT_ID(N'dbo.TieBreakSubmission')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_TieBreakSubmission_SubmissionId
    ON dbo.TieBreakSubmission(SubmissionId);
END;

IF OBJECT_ID(N'dbo.TieBreakScoreRecord', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TieBreakScoreRecord
    (
        Id UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT PK_TieBreakScoreRecord PRIMARY KEY
            CONSTRAINT DF_TieBreakScoreRecord_Id DEFAULT NEWID(),

        TieBreakSubmissionId UNIQUEIDENTIFIER NOT NULL,
        JudgeId UNIQUEIDENTIFIER NOT NULL,
        CriterionId INT NOT NULL,
        Score FLOAT NOT NULL,
        Comment NVARCHAR(MAX) NULL,

        ScoredAt DATETIME2(7) NOT NULL
            CONSTRAINT DF_TieBreakScoreRecord_ScoredAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2(7) NULL,

        CONSTRAINT FK_TieBreakScoreRecord_TieBreakSubmission
            FOREIGN KEY (TieBreakSubmissionId) REFERENCES dbo.TieBreakSubmission(Id),
        CONSTRAINT FK_TieBreakScoreRecord_Judge
            FOREIGN KEY (JudgeId) REFERENCES dbo.Account(Id),
        CONSTRAINT FK_TieBreakScoreRecord_Criterion
            FOREIGN KEY (CriterionId) REFERENCES dbo.Criterion(Id)
    );
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UQ_TieBreakScoreRecord'
      AND object_id = OBJECT_ID(N'dbo.TieBreakScoreRecord')
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_TieBreakScoreRecord
    ON dbo.TieBreakScoreRecord(TieBreakSubmissionId, JudgeId, CriterionId);
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_TieBreakScoreRecord_JudgeId'
      AND object_id = OBJECT_ID(N'dbo.TieBreakScoreRecord')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_TieBreakScoreRecord_JudgeId
    ON dbo.TieBreakScoreRecord(JudgeId);
END;
