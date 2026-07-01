-- Cho phep cung mot Account co nhieu EventRole khac nhau trong cung mot Event.
-- Vi du: vua Mentor mot Track, vua Judge mot Round khac trong cung Event.

IF EXISTS (
    SELECT 1
    FROM sys.key_constraints
    WHERE name = 'UQ_EventAccount'
      AND parent_object_id = OBJECT_ID('EventAccount')
)
BEGIN
    ALTER TABLE EventAccount DROP CONSTRAINT UQ_EventAccount;
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.key_constraints
    WHERE name = 'UQ_EventAccount'
      AND parent_object_id = OBJECT_ID('EventAccount')
)
BEGIN
    ALTER TABLE EventAccount
    ADD CONSTRAINT UQ_EventAccount UNIQUE (EventId, AccountId, EventRole);
END;
