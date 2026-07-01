-- SCRIPT 013: Cập nhật cấu trúc bảng Track
-- Thêm trường giới hạn số lượng thành viên (MaxMembers)

IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[Track]') AND name = 'MaxMembers'
)
BEGIN
    ALTER TABLE Track
    ADD MaxMembers INT NULL;
END
GO
