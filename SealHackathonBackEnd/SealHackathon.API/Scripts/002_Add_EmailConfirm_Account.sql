-- 002_Add_EmailConfirm_Account.sql
-- Thêm 2 field xác nhận email vào bảng Account

ALTER TABLE Account
ADD EmailConfirmToken VARCHAR(500) NULL,
    TokenExpiresAt    DATETIME2    NULL;