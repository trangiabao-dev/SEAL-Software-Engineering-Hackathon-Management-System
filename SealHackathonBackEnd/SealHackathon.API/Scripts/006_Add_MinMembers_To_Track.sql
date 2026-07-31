-- ==========================================
-- SCRIPT 006: Add MinMembers to Track Table
-- Allows dynamic minimum team size validation
-- ==========================================

IF COL_LENGTH('dbo.Track', 'MinMembers') IS NULL
BEGIN
    ALTER TABLE dbo.Track ADD MinMembers INT NULL;
    PRINT 'Added MinMembers column to Track table.';
    
    -- Cập nhật dữ liệu cũ mặc định là 3 (để không bị lỗi Null)
    UPDATE dbo.Track SET MinMembers = 3 WHERE MinMembers IS NULL;
    PRINT 'Updated existing Track rows to have MinMembers = 3.';
END
ELSE
BEGIN
    PRINT 'MinMembers column already exists in Track table.';
END
