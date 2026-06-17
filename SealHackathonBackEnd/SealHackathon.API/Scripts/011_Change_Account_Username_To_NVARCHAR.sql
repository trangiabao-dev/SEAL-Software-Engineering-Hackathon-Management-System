-- Account.Username co the hien thi ten nguoi dung tren FE.
-- Dung NVARCHAR de luu duoc ten co dau tieng Viet, tranh bi loi ky tu khi tao Leader, Mentor, Judge.

DECLARE @UsernameConstraintName SYSNAME;
DECLARE @Sql NVARCHAR(MAX);

SELECT @UsernameConstraintName = kc.name
FROM sys.key_constraints kc
JOIN sys.index_columns ic
    ON kc.parent_object_id = ic.object_id
   AND kc.unique_index_id = ic.index_id
JOIN sys.columns c
    ON ic.object_id = c.object_id
   AND ic.column_id = c.column_id
WHERE kc.parent_object_id = OBJECT_ID(N'dbo.Account')
  AND kc.[type] = 'UQ'
  AND c.name = N'Username';

IF @UsernameConstraintName IS NOT NULL
BEGIN
    -- Khai báo biến để chứa chuỗi lệnh động
    DECLARE @DropConstraintSql NVARCHAR(MAX);
    
    -- Gán chuỗi lệnh vào biến trước
    SET @DropConstraintSql = N'ALTER TABLE dbo.Account DROP CONSTRAINT ' + QUOTENAME(@UsernameConstraintName);
    
    -- Thực thi biến
    EXEC(@DropConstraintSql);

    SET @Sql = N'ALTER TABLE dbo.Account DROP CONSTRAINT ' + QUOTENAME(@UsernameConstraintName);
    EXEC(@Sql);

END;

IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Account')
      AND name = N'Username'
      AND system_type_id <> TYPE_ID(N'nvarchar')
)
BEGIN
    ALTER TABLE dbo.Account
    ALTER COLUMN Username NVARCHAR(255) NOT NULL;
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.key_constraints kc
    JOIN sys.index_columns ic
        ON kc.parent_object_id = ic.object_id
       AND kc.unique_index_id = ic.index_id
    JOIN sys.columns c
        ON ic.object_id = c.object_id
       AND ic.column_id = c.column_id
    WHERE kc.parent_object_id = OBJECT_ID(N'dbo.Account')
      AND kc.[type] = 'UQ'
      AND c.name = N'Username'
)
BEGIN
    ALTER TABLE dbo.Account
    ADD CONSTRAINT UQ_Account_Username UNIQUE (Username);
END;
