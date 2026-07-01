-- Improve query performance for Team, Submission, Scoring, Ranking, AuditLog.
-- DbUp runs this script once.

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Event_Status_IsDeleted' AND object_id = OBJECT_ID('dbo.Event'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Event_Status_IsDeleted
    ON dbo.[Event](Status, IsDeleted);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Track_EventId_IsDeleted' AND object_id = OBJECT_ID('dbo.Track'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Track_EventId_IsDeleted
    ON dbo.Track(EventId, IsDeleted);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Round_TrackId_Status' AND object_id = OBJECT_ID('dbo.Round'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Round_TrackId_Status
    ON dbo.Round(TrackId, Status);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Topic_RoundId' AND object_id = OBJECT_ID('dbo.Topic'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Topic_RoundId
    ON dbo.Topic(RoundId);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Team_LeaderId_IsDeleted_TrackId' AND object_id = OBJECT_ID('dbo.Team'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Team_LeaderId_IsDeleted_TrackId
    ON dbo.Team(LeaderId, IsDeleted, TrackId);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Team_TrackId_Status_IsDeleted' AND object_id = OBJECT_ID('dbo.Team'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Team_TrackId_Status_IsDeleted
    ON dbo.Team(TrackId, Status, IsDeleted);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TeamMember_TeamId' AND object_id = OBJECT_ID('dbo.TeamMember'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_TeamMember_TeamId
    ON dbo.TeamMember(TeamId);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TeamMember_Email' AND object_id = OBJECT_ID('dbo.TeamMember'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_TeamMember_Email
    ON dbo.TeamMember(Email);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TeamMember_StudentCode' AND object_id = OBJECT_ID('dbo.TeamMember'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_TeamMember_StudentCode
    ON dbo.TeamMember(StudentCode);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Submission_RoundId_IsDisqualified' AND object_id = OBJECT_ID('dbo.Submission'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Submission_RoundId_IsDisqualified
    ON dbo.Submission(RoundId, IsDisqualified);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Submission_TeamId_IsDisqualified' AND object_id = OBJECT_ID('dbo.Submission'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Submission_TeamId_IsDisqualified
    ON dbo.Submission(TeamId, IsDisqualified);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_JudgeAssign_RoundId' AND object_id = OBJECT_ID('dbo.JudgeAssign'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_JudgeAssign_RoundId
    ON dbo.JudgeAssign(RoundId);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Criterion_RoundId' AND object_id = OBJECT_ID('dbo.Criterion'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Criterion_RoundId
    ON dbo.Criterion(RoundId);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScoreRecord_JudgeId' AND object_id = OBJECT_ID('dbo.ScoreRecord'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ScoreRecord_JudgeId
    ON dbo.ScoreRecord(JudgeId);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Ranking_RoundId_RankPosition' AND object_id = OBJECT_ID('dbo.Ranking'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Ranking_RoundId_RankPosition
    ON dbo.Ranking(RoundId, RankPosition);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AuditLog_PerformedBy_CreatedAt' AND object_id = OBJECT_ID('dbo.AuditLog'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_AuditLog_PerformedBy_CreatedAt
    ON dbo.AuditLog(PerformedBy, CreatedAt);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AuditLog_EntityName_EntityId_CreatedAt' AND object_id = OBJECT_ID('dbo.AuditLog'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_AuditLog_EntityName_EntityId_CreatedAt
    ON dbo.AuditLog(EntityName, EntityId, CreatedAt);
END;