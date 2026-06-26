-- 018_Change_Prize_TrackId_To_EventId.sql
-- Chuyen cau hinh Prize tu cap Track sang cap Event.

IF OBJECT_ID(N'dbo.Prize', N'U') IS NULL
BEGIN
    RAISERROR('Table dbo.Prize does not exist.', 16, 1);
    RETURN;
END;

-- EventId moi se luu Event so huu bo giai thuong.
IF COL_LENGTH(N'dbo.Prize', N'EventId') IS NULL
BEGIN
    ALTER TABLE dbo.Prize
    ADD EventId INT NULL;
END;

-- Lay EventId thong qua TrackId hien tai de giu lai du lieu Prize cu.
IF COL_LENGTH(N'dbo.Prize', N'TrackId') IS NOT NULL
BEGIN
    EXEC sp_executesql N'
        UPDATE prize
        SET EventId = track.EventId
        FROM dbo.Prize AS prize
        INNER JOIN dbo.Track AS track ON track.Id = prize.TrackId
        WHERE prize.EventId IS NULL;';
END;

-- Neu con dong Prize khong map duoc EventId thi dung de tranh mat du lieu sai.
DECLARE @hasPrizeWithoutEventId BIT = 0;

EXEC sp_executesql
    N'SELECT @hasPrizeWithoutEventId = CASE WHEN EXISTS (SELECT 1 FROM dbo.Prize WHERE EventId IS NULL) THEN 1 ELSE 0 END;',
    N'@hasPrizeWithoutEventId BIT OUTPUT',
    @hasPrizeWithoutEventId OUTPUT;

IF @hasPrizeWithoutEventId = 1
BEGIN
    RAISERROR('Cannot migrate Prize.TrackId to Prize.EventId because some Prize rows cannot map to Track.EventId.', 16, 1);
    RETURN;
END;

-- Sau khi da copy du lieu, EventId bat buoc phai co gia tri.
IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Prize')
      AND name = N'EventId'
      AND is_nullable = 1
)
BEGIN
    EXEC sp_executesql N'
        ALTER TABLE dbo.Prize
        ALTER COLUMN EventId INT NOT NULL;';
END;

-- Drop foreign key cu dang tro tu Prize.TrackId sang Track.Id neu con ton tai.
DECLARE @oldPrizeTrackFkName SYSNAME;

SELECT @oldPrizeTrackFkName = fk.name
FROM sys.foreign_keys AS fk
INNER JOIN sys.foreign_key_columns AS fkc ON fkc.constraint_object_id = fk.object_id
INNER JOIN sys.columns AS parentColumn
    ON parentColumn.object_id = fkc.parent_object_id
   AND parentColumn.column_id = fkc.parent_column_id
WHERE fk.parent_object_id = OBJECT_ID(N'dbo.Prize')
  AND fk.referenced_object_id = OBJECT_ID(N'dbo.Track')
  AND parentColumn.name = N'TrackId';

IF @oldPrizeTrackFkName IS NOT NULL
BEGIN
    DECLARE @dropOldPrizeTrackFkSql NVARCHAR(MAX) =
        N'ALTER TABLE dbo.Prize DROP CONSTRAINT ' + QUOTENAME(@oldPrizeTrackFkName) + N';';

    EXEC sp_executesql @dropOldPrizeTrackFkSql;
END;

-- Drop TrackId sau khi da bo FK cu va chuyen xong du lieu sang EventId.
IF COL_LENGTH(N'dbo.Prize', N'TrackId') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Prize
    DROP COLUMN TrackId;
END;

-- Moi Event chi duoc cau hinh mot Prize cho moi hang 1, 2, 3.
DECLARE @hasDuplicatePrizeRank BIT = 0;

EXEC sp_executesql
    N'
        SELECT @hasDuplicatePrizeRank =
            CASE
                WHEN EXISTS (
                    SELECT EventId, RankPosition
                    FROM dbo.Prize
                    GROUP BY EventId, RankPosition
                    HAVING COUNT(*) > 1
                )
                THEN 1
                ELSE 0
            END;',
    N'@hasDuplicatePrizeRank BIT OUTPUT',
    @hasDuplicatePrizeRank OUTPUT;

IF @hasDuplicatePrizeRank = 1
BEGIN
    RAISERROR('Duplicate Prize RankPosition exists in the same Event. Clean data before adding unique index.', 16, 1);
    RETURN;
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UQ_Prize_Event_RankPosition'
      AND object_id = OBJECT_ID(N'dbo.Prize')
)
BEGIN
    EXEC sp_executesql N'
        CREATE UNIQUE INDEX UQ_Prize_Event_RankPosition
        ON dbo.Prize(EventId, RankPosition);';
END;

-- Tao foreign key moi tu Prize.EventId sang Event.Id.
IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_Prize_Event'
      AND parent_object_id = OBJECT_ID(N'dbo.Prize')
)
BEGIN
    EXEC sp_executesql N'
        ALTER TABLE dbo.Prize
        ADD CONSTRAINT FK_Prize_Event
            FOREIGN KEY (EventId) REFERENCES dbo.[Event](Id);';
END;
