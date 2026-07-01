-- 003_Change_Account_Default_SystemRole_To_Leader.sql
-- Đổi default SystemRole từ Participant sang Leader

DECLARE @ConstraintName NVARCHAR(200);

SELECT @ConstraintName = dc.name
FROM sys.default_constraints dc
JOIN sys.columns c ON dc.parent_object_id = c.object_id
                  AND dc.parent_column_id = c.column_id
JOIN sys.tables t ON c.object_id = t.object_id
WHERE t.name = 'Account'
  AND c.name = 'SystemRole';

IF @ConstraintName IS NOT NULL
BEGIN
    EXEC('ALTER TABLE Account DROP CONSTRAINT ' + @ConstraintName);
END

ALTER TABLE Account
ADD CONSTRAINT DF_Account_SystemRole
DEFAULT 'Leader' FOR SystemRole;

UPDATE Account
SET SystemRole = 'Leader'
WHERE SystemRole = 'Participant';