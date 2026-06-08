IF COL_LENGTH('dbo.TeamMember', 'University') IS NULL
BEGIN
    ALTER TABLE dbo.TeamMember
    ADD University NVARCHAR(255) NULL;
END;

EXEC(N'
UPDATE tm
SET tm.University = t.University
FROM dbo.TeamMember tm
INNER JOIN dbo.[Team] t ON tm.TeamId = t.Id
WHERE tm.University IS NULL OR LTRIM(RTRIM(tm.University)) = '''';
');

IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.TeamMember')
      AND name = N'University'
      AND is_nullable = 1
)
BEGIN
    EXEC(N'ALTER TABLE dbo.TeamMember ALTER COLUMN University NVARCHAR(255) NOT NULL;');
END;