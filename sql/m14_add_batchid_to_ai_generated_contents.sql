/* Add batch_id column to ai_generated_contents for grouping generated items into batches */
SET NOCOUNT ON;

IF COL_LENGTH('dbo.ai_generated_contents', 'batch_id') IS NULL
BEGIN
    ALTER TABLE dbo.ai_generated_contents ADD batch_id NVARCHAR(50) NULL;
    PRINT 'Added column batch_id to ai_generated_contents.';
END
ELSE
BEGIN
    PRINT 'Column batch_id already exists.';
END
