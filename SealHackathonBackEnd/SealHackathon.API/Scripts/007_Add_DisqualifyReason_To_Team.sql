IF COL_LENGTH('dbo.Team', 'DisqualifyReason') IS NULL
BEGIN
    ALTER TABLE dbo.Team
    ADD DisqualifyReason NVARCHAR(500) NULL;
END;