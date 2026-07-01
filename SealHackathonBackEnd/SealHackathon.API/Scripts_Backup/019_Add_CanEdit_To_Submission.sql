IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[Submission]') AND name = 'CanEdit'
)
BEGIN
    ALTER TABLE [Submission] ADD [CanEdit] bit NOT NULL DEFAULT 0;
END
GO
