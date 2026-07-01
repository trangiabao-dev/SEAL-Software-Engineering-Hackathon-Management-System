-- Chức năng chấm điểm hiệu chuẩn đã bị loại bỏ.
-- Các điểm hiệu chuẩn cũ không phải điểm chính thức nên phải xóa
-- trước khi xóa cột, tránh chúng bị tính nhầm vào Ranking.

IF COL_LENGTH('dbo.ScoreRecord', 'IsCalibration') IS NOT NULL
BEGIN
    -- Xóa các điểm hiệu chuẩn cũ.
    DELETE FROM dbo.ScoreRecord
    WHERE IsCalibration = 1;

    -- Tìm tên default constraint vì SQL Server có thể tự sinh tên khác nhau
    -- trên mỗi database.
    DECLARE @DefaultConstraintName SYSNAME;

    SELECT @DefaultConstraintName = defaultConstraint.name
    FROM sys.default_constraints AS defaultConstraint
    INNER JOIN sys.columns AS columnInfo
        ON columnInfo.default_object_id = defaultConstraint.object_id
    WHERE defaultConstraint.parent_object_id =
          OBJECT_ID('dbo.ScoreRecord')
      AND columnInfo.name = 'IsCalibration';

    -- Phải xóa default constraint trước khi xóa cột.
    IF @DefaultConstraintName IS NOT NULL
    BEGIN
        DECLARE @DropConstraintSql NVARCHAR(MAX);

        SET @DropConstraintSql =
            N'ALTER TABLE dbo.ScoreRecord DROP CONSTRAINT '
            + QUOTENAME(@DefaultConstraintName);

        EXEC sp_executesql @DropConstraintSql;
    END;

    ALTER TABLE dbo.ScoreRecord
    DROP COLUMN IsCalibration;
END;