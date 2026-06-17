-- Submission chi can luu link bai thuyet trinh/slide.
-- Link ma nguon duoc quan ly o Team.GithubRepoLink.

IF COL_LENGTH('dbo.Submission', 'PresentationUrl') IS NULL
BEGIN
    ALTER TABLE dbo.Submission
    ADD PresentationUrl VARCHAR(1000) NULL;
END;

IF COL_LENGTH('dbo.Submission', 'ReportUrl') IS NOT NULL
BEGIN
    EXEC(N'
        UPDATE dbo.Submission
        SET PresentationUrl = ReportUrl
        WHERE PresentationUrl IS NULL
          AND ReportUrl IS NOT NULL;
    ');
END;

IF COL_LENGTH('dbo.Submission', 'DemoUrl') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Submission
    DROP COLUMN DemoUrl;
END;

IF COL_LENGTH('dbo.Submission', 'ReportUrl') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Submission
    DROP COLUMN ReportUrl;
END;

IF COL_LENGTH('dbo.Submission', 'PresentationUrl') IS NOT NULL
BEGIN
    EXEC(N'
        UPDATE dbo.Submission
        SET PresentationUrl = ''''
        WHERE PresentationUrl IS NULL;
    ');
END;

IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Submission')
      AND name = 'PresentationUrl'
      AND is_nullable = 1
)
BEGIN
    EXEC(N'
        ALTER TABLE dbo.Submission
        ALTER COLUMN PresentationUrl VARCHAR(1000) NOT NULL;
    ');
END;
