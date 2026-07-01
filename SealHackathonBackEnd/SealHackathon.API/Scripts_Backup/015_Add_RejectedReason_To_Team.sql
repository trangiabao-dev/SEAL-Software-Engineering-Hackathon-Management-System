-- Lưu lý do Coordinator từ chối Team.
-- Team bị Rejected không được gửi lại đăng ký trong cùng Event.
IF COL_LENGTH('dbo.Team', 'RejectedReason') IS NULL
BEGIN
    ALTER TABLE dbo.Team
    ADD RejectedReason NVARCHAR(500) NULL;
END;
