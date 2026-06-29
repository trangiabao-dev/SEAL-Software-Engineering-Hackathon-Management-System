-- Thêm cột EventId vào bảng Topic
ALTER TABLE Topic
ADD EventId INT NULL;
GO

-- Sửa cột RoundId cho phép NULL
ALTER TABLE Topic
ALTER COLUMN RoundId INT NULL;
GO

-- Thêm khóa ngoại cho EventId
ALTER TABLE Topic
ADD CONSTRAINT FK_Topic_EventId FOREIGN KEY (EventId) REFERENCES [Event](Id);
GO
