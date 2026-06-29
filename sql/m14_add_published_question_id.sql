-- M14: Add published_question_id column for tracking published Question Bank entries
-- Safe migration: checks if column exists before adding

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'ai_generated_contents'
    AND COLUMN_NAME = 'published_question_id'
)
BEGIN
    ALTER TABLE ai_generated_contents
    ADD published_question_id INT NULL;

    -- Add foreign key constraint to question_bank
    ALTER TABLE ai_generated_contents
    ADD CONSTRAINT FK_AiGeneratedContent_PublishedQuestion
    FOREIGN KEY (published_question_id) REFERENCES question_bank(id);

    PRINT 'Added published_question_id column and FK constraint to ai_generated_contents';
END
ELSE
BEGIN
    PRINT 'Column published_question_id already exists in ai_generated_contents - skipping';
END
