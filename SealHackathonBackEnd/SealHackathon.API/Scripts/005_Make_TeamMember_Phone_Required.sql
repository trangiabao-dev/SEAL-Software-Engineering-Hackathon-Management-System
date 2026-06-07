IF COL_LENGTH('TeamMember', 'Phone') IS NOT NULL
BEGIN
    UPDATE TeamMember
    SET Phone = '0000000000'
    WHERE Phone IS NULL OR LTRIM(RTRIM(Phone)) = '';

    ALTER TABLE TeamMember
    ALTER COLUMN Phone VARCHAR(20) NOT NULL;
END
