-- Luu danh sach team duoc tham gia tung Round va Topic duoc gan trong Round do.
-- Giu Team.TopicId de tuong thich voi API cu, nhung RoundTeam moi la noi luu lich su theo tung Round.

IF OBJECT_ID('dbo.RoundTeam', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RoundTeam
    (
        Id UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT PK_RoundTeam PRIMARY KEY
            CONSTRAINT DF_RoundTeam_Id DEFAULT NEWID(),

        RoundId INT NOT NULL,
        TeamId UNIQUEIDENTIFIER NOT NULL,
        TopicId INT NULL,

        AssignedAt DATETIME2(7) NOT NULL
            CONSTRAINT DF_RoundTeam_AssignedAt DEFAULT SYSUTCDATETIME(),
        AssignedBy UNIQUEIDENTIFIER NULL,

        CreatedAt DATETIME2(7) NOT NULL
            CONSTRAINT DF_RoundTeam_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2(7) NOT NULL
            CONSTRAINT DF_RoundTeam_UpdatedAt DEFAULT SYSUTCDATETIME(),

        CONSTRAINT FK_RoundTeam_Round
            FOREIGN KEY (RoundId) REFERENCES dbo.Round(Id),
        CONSTRAINT FK_RoundTeam_Team
            FOREIGN KEY (TeamId) REFERENCES dbo.Team(Id),
        CONSTRAINT FK_RoundTeam_Topic
            FOREIGN KEY (TopicId) REFERENCES dbo.Topic(Id),
        CONSTRAINT FK_RoundTeam_AssignedBy
            FOREIGN KEY (AssignedBy) REFERENCES dbo.Account(Id)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_RoundTeam_Round_Team' AND object_id = OBJECT_ID('dbo.RoundTeam'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_RoundTeam_Round_Team
    ON dbo.RoundTeam(RoundId, TeamId);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RoundTeam_TeamId' AND object_id = OBJECT_ID('dbo.RoundTeam'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_RoundTeam_TeamId
    ON dbo.RoundTeam(TeamId);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RoundTeam_TopicId' AND object_id = OBJECT_ID('dbo.RoundTeam'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_RoundTeam_TopicId
    ON dbo.RoundTeam(TopicId);
END;
