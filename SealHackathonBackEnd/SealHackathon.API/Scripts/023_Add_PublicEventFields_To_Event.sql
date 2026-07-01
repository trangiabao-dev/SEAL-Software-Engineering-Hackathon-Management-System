-- Add new fields for Public Event and FE-BE contract requirements
ALTER TABLE [Event]
ADD [BannerUrl] VARCHAR(1000) NULL,
    [Location] NVARCHAR(500) NULL,
    [IsOnline] BIT NULL;
GO
