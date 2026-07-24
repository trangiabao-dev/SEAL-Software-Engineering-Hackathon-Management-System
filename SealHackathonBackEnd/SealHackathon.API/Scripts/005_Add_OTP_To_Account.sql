-- ==========================================
-- SCRIPT 005: ADD OTP FIELDS TO ACCOUNT
-- Adds ResetPasswordToken and ResetPasswordTokenExpiresAt columns
-- ==========================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE Name = N'ResetPasswordToken' 
    AND Object_ID = Object_ID(N'dbo.Account')
)
BEGIN
    ALTER TABLE dbo.Account ADD ResetPasswordToken VARCHAR(500) NULL;
    ALTER TABLE dbo.Account ADD ResetPasswordTokenExpiresAt DATETIME2 NULL;
END;
