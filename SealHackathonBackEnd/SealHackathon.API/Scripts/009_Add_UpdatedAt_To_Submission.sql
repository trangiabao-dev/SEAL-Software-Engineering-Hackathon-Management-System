IF COL_LENGTH('dbo.Submission', 'UpdatedAt') IS NULL
BEGIN
    ALTER TABLE dbo.Submission
    ADD UpdatedAt DATETIME2 NULL;
END;