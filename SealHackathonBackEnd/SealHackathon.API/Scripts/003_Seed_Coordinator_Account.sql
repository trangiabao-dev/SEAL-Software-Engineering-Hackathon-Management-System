-- ==========================================
-- SCRIPT 003: SEED COORDINATOR ACCOUNT
-- Ensures the coordinator account exists so we don't have to manually create it every time the DB is reset
-- Password is '123456'
-- ==========================================

IF NOT EXISTS (SELECT 1 FROM dbo.Account WHERE Username = 'coordinator')
BEGIN
    INSERT INTO dbo.Account (Id, Username, PasswordHash, Email, SystemRole, IsDeleted, CreatedAt, UpdatedAt)
    VALUES (
        '17977DF8-6F49-40E6-92C3-154B16BE2D66',
        'coordinator',
        '$2a$11$DrWbtkP4OrtWYckqNUsDoOFRh8DkrG13JloawKINv9qvvGdL2XKl6',
        'coordinator@seal.edu.vn',
        'Coordinator',
        0,
        SYSUTCDATETIME(),
        SYSUTCDATETIME()
    );
END
