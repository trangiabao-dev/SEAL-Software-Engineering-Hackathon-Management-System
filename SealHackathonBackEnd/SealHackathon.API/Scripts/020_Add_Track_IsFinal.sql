-- 020_Add_Track_IsFinal.sql
-- Them cot IsFinal de danh dau Track Final cua Event.

IF COL_LENGTH(N'dbo.Track', N'IsFinal') IS NULL
BEGIN
    ALTER TABLE dbo.Track
    ADD IsFinal BIT NOT NULL
        CONSTRAINT DF_Track_IsFinal DEFAULT(0);
END;

DECLARE @hasDuplicatedFinalTrack BIT = 0;

EXEC sp_executesql
    N'
        SELECT @hasDuplicatedFinalTrack =
            CASE
                WHEN EXISTS (
                    SELECT EventId
                    FROM dbo.Track
                    WHERE IsFinal = 1 AND IsDeleted = 0
                    GROUP BY EventId
                    HAVING COUNT(*) > 1
                )
                THEN 1
                ELSE 0
            END;',
    N'@hasDuplicatedFinalTrack BIT OUTPUT',
    @hasDuplicatedFinalTrack OUTPUT;

IF @hasDuplicatedFinalTrack = 1
BEGIN
    RAISERROR('Each Event can have only one active final Track. Clean Track data before adding unique index.', 16, 1);
    RETURN;
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UQ_Track_Event_Final'
      AND object_id = OBJECT_ID(N'dbo.Track')
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_Track_Event_Final
    ON dbo.Track(EventId)
    WHERE IsFinal = 1 AND IsDeleted = 0;
END;
