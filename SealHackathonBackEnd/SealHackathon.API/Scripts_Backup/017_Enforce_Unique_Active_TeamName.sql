-- TeamName phải duy nhất trên toàn hệ thống đối với Team chưa bị xóa mềm.

IF EXISTS
(
    SELECT LTRIM(RTRIM(TeamName))
    FROM dbo.Team
    WHERE IsDeleted = 0
    GROUP BY LTRIM(RTRIM(TeamName))
    HAVING COUNT(*) > 1
)
-- MỚI (chạy được):
BEGIN
    RAISERROR(N'Không thể tạo unique index vì database đang có TeamName bị trùng.', 16, 1);
    RETURN;
END;

UPDATE dbo.Team
SET TeamName = LTRIM(RTRIM(TeamName))
WHERE TeamName <> LTRIM(RTRIM(TeamName));

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UQ_Team_TeamName_Active'
      AND object_id = OBJECT_ID('dbo.Team')
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_Team_TeamName_Active
    ON dbo.Team(TeamName)
    WHERE IsDeleted = 0;
END;