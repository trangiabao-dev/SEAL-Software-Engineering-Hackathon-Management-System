-- Chức năng đánh giá bài nộp bằng AI không còn nằm trong phạm vi dự án.
-- Xóa cột không được sử dụng để schema khớp với Entity và API response hiện tại.
IF COL_LENGTH('dbo.Submission', 'AiEvaluation') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Submission
    DROP COLUMN AiEvaluation;
END;
