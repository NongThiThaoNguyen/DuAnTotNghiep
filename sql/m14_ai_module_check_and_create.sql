/*
Safe migration script for Module M14 (AI-generated quiz/tasks)
 - Checks for existence of required tables and creates them if missing
 - Adds CHECK constraints for content_type and review_status
 - Creates indexes on review_status, content_type, related_topic_id
 - Adds trigger to prevent hard deletes of AI content that has been reviewed
 - Designed to be safe: uses IF NOT EXISTS and does not DROP existing objects
*/

SET NOCOUNT ON;

-- 1) Create ai_prompt_templates
IF NOT EXISTS (SELECT 1 FROM sys.tables t WHERE t.name = 'ai_prompt_templates' AND SCHEMA_NAME(t.schema_id) = 'dbo')
BEGIN
    CREATE TABLE dbo.ai_prompt_templates (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        name NVARCHAR(255) NOT NULL,
        prompt_text NVARCHAR(MAX) NOT NULL,
        content_type NVARCHAR(50) NOT NULL,
        metadata NVARCHAR(MAX) NULL,
        created_by NVARCHAR(100) NULL,
        created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
    ALTER TABLE dbo.ai_prompt_templates
        ADD CONSTRAINT CK_ai_prompt_templates_content_type CHECK (content_type IN ('QUESTION','QUIZ','PRACTICE_TASK','ESSAY_PROMPT','SPEAKING_PROMPT'));
END

-- 2) Create ai_usage_logs
IF NOT EXISTS (SELECT 1 FROM sys.tables t WHERE t.name = 'ai_usage_logs' AND SCHEMA_NAME(t.schema_id) = 'dbo')
BEGIN
    CREATE TABLE dbo.ai_usage_logs (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        prompt_template_id INT NULL,
        user_id NVARCHAR(100) NULL,
        usage_payload NVARCHAR(MAX) NULL,
        created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_ai_usage_logs_prompt_template FOREIGN KEY (prompt_template_id) REFERENCES dbo.ai_prompt_templates(id)
    );
END

-- 3) Create ai_generated_contents
IF NOT EXISTS (SELECT 1 FROM sys.tables t WHERE t.name = 'ai_generated_contents' AND SCHEMA_NAME(t.schema_id) = 'dbo')
BEGIN
    CREATE TABLE dbo.ai_generated_contents (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        prompt_template_id INT NULL,
        usage_log_id INT NULL,
        content_type NVARCHAR(50) NOT NULL,
        title NVARCHAR(1024) NULL,
        content NVARCHAR(MAX) NOT NULL,
        related_topic_id INT NULL,
        metadata NVARCHAR(MAX) NULL,
        review_status NVARCHAR(50) NOT NULL DEFAULT 'PENDING',
        reviewed_by NVARCHAR(100) NULL,
        reviewed_at DATETIME2 NULL,
        is_deleted BIT NOT NULL DEFAULT 0,
        created_by NVARCHAR(100) NULL,
        created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_ai_generated_contents_prompt_template FOREIGN KEY (prompt_template_id) REFERENCES dbo.ai_prompt_templates(id),
        CONSTRAINT FK_ai_generated_contents_usage_log FOREIGN KEY (usage_log_id) REFERENCES dbo.ai_usage_logs(id)
    );
    ALTER TABLE dbo.ai_generated_contents
        ADD CONSTRAINT CK_ai_generated_contents_content_type CHECK (content_type IN ('QUESTION','QUIZ','PRACTICE_TASK','ESSAY_PROMPT','SPEAKING_PROMPT'));
    ALTER TABLE dbo.ai_generated_contents
        ADD CONSTRAINT CK_ai_generated_contents_review_status CHECK (review_status IN ('PENDING','APPROVED','REJECTED','NEEDS_REVISION'));
END

-- 4) Create content_compliance_reviews
IF NOT EXISTS (SELECT 1 FROM sys.tables t WHERE t.name = 'content_compliance_reviews' AND SCHEMA_NAME(t.schema_id) = 'dbo')
BEGIN
    CREATE TABLE dbo.content_compliance_reviews (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        generated_content_id INT NOT NULL,
        reviewer_id NVARCHAR(100) NULL,
        review_status NVARCHAR(50) NOT NULL,
        comments NVARCHAR(MAX) NULL,
        created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_content_compliance_reviews_generated_content FOREIGN KEY (generated_content_id) REFERENCES dbo.ai_generated_contents(id)
    );
    ALTER TABLE dbo.content_compliance_reviews
        ADD CONSTRAINT CK_content_compliance_reviews_review_status CHECK (review_status IN ('PENDING','APPROVED','REJECTED','NEEDS_REVISION'));
END

-- 5) Create question_bank
IF NOT EXISTS (SELECT 1 FROM sys.tables t WHERE t.name = 'question_bank' AND SCHEMA_NAME(t.schema_id) = 'dbo')
BEGIN
    CREATE TABLE dbo.question_bank (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ai_generated_content_id INT NULL,
        question_text NVARCHAR(MAX) NOT NULL,
        difficulty NVARCHAR(50) NULL,
        created_by NVARCHAR(100) NULL,
        created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_question_bank_ai_generated_content FOREIGN KEY (ai_generated_content_id) REFERENCES dbo.ai_generated_contents(id)
    );
END

-- 6) Create question_options
IF NOT EXISTS (SELECT 1 FROM sys.tables t WHERE t.name = 'question_options' AND SCHEMA_NAME(t.schema_id) = 'dbo')
BEGIN
    CREATE TABLE dbo.question_options (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        question_id INT NOT NULL,
        option_text NVARCHAR(MAX) NOT NULL,
        is_correct BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_question_options_question FOREIGN KEY (question_id) REFERENCES dbo.question_bank(id)
    );
END

-- 7) Create quizzes
IF NOT EXISTS (SELECT 1 FROM sys.tables t WHERE t.name = 'quizzes' AND SCHEMA_NAME(t.schema_id) = 'dbo')
BEGIN
    CREATE TABLE dbo.quizzes (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        title NVARCHAR(1024) NOT NULL,
        ai_generated_content_id INT NULL,
        related_topic_id INT NULL,
        created_by NVARCHAR(100) NULL,
        created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_quizzes_ai_generated_content FOREIGN KEY (ai_generated_content_id) REFERENCES dbo.ai_generated_contents(id)
    );
END

-- 8) Create quiz_questions
IF NOT EXISTS (SELECT 1 FROM sys.tables t WHERE t.name = 'quiz_questions' AND SCHEMA_NAME(t.schema_id) = 'dbo')
BEGIN
    CREATE TABLE dbo.quiz_questions (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        quiz_id INT NOT NULL,
        question_id INT NOT NULL,
        order_index INT NOT NULL DEFAULT 0,
        CONSTRAINT FK_quiz_questions_quiz FOREIGN KEY (quiz_id) REFERENCES dbo.quizzes(id),
        CONSTRAINT FK_quiz_questions_question FOREIGN KEY (question_id) REFERENCES dbo.question_bank(id)
    );
END

-- 9) Create practice_tasks
IF NOT EXISTS (SELECT 1 FROM sys.tables t WHERE t.name = 'practice_tasks' AND SCHEMA_NAME(t.schema_id) = 'dbo')
BEGIN
    CREATE TABLE dbo.practice_tasks (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        title NVARCHAR(1024) NOT NULL,
        description NVARCHAR(MAX) NULL,
        ai_generated_content_id INT NULL,
        related_topic_id INT NULL,
        created_by NVARCHAR(100) NULL,
        created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_practice_tasks_ai_generated_content FOREIGN KEY (ai_generated_content_id) REFERENCES dbo.ai_generated_contents(id)
    );
END

-- 10) Indexes (create if not exists)
IF NOT EXISTS (SELECT 1 FROM sys.indexes i JOIN sys.objects o ON i.object_id = o.object_id WHERE i.name = 'IX_ai_generated_contents_review_status' AND o.name = 'ai_generated_contents')
    CREATE INDEX IX_ai_generated_contents_review_status ON dbo.ai_generated_contents(review_status);

IF NOT EXISTS (SELECT 1 FROM sys.indexes i JOIN sys.objects o ON i.object_id = o.object_id WHERE i.name = 'IX_ai_generated_contents_content_type' AND o.name = 'ai_generated_contents')
    CREATE INDEX IX_ai_generated_contents_content_type ON dbo.ai_generated_contents(content_type);

IF NOT EXISTS (SELECT 1 FROM sys.indexes i JOIN sys.objects o ON i.object_id = o.object_id WHERE i.name = 'IX_ai_generated_contents_related_topic_id' AND o.name = 'ai_generated_contents')
    CREATE INDEX IX_ai_generated_contents_related_topic_id ON dbo.ai_generated_contents(related_topic_id);

IF NOT EXISTS (SELECT 1 FROM sys.indexes i JOIN sys.objects o ON i.object_id = o.object_id WHERE i.name = 'IX_content_compliance_reviews_review_status' AND o.name = 'content_compliance_reviews')
    CREATE INDEX IX_content_compliance_reviews_review_status ON dbo.content_compliance_reviews(review_status);

IF NOT EXISTS (SELECT 1 FROM sys.indexes i JOIN sys.objects o ON i.object_id = o.object_id WHERE i.name = 'IX_quizzes_related_topic_id' AND o.name = 'quizzes')
    CREATE INDEX IX_quizzes_related_topic_id ON dbo.quizzes(related_topic_id);

IF NOT EXISTS (SELECT 1 FROM sys.indexes i JOIN sys.objects o ON i.object_id = o.object_id WHERE i.name = 'IX_practice_tasks_related_topic_id' AND o.name = 'practice_tasks')
    CREATE INDEX IX_practice_tasks_related_topic_id ON dbo.practice_tasks(related_topic_id);

-- 11) Trigger to prevent hard deletes of reviewed AI content
IF OBJECT_ID('dbo.trg_prevent_delete_reviewed_ai_generated_contents', 'TR') IS NULL
BEGIN
    EXEC('CREATE TRIGGER dbo.trg_prevent_delete_reviewed_ai_generated_contents
    ON dbo.ai_generated_contents
    INSTEAD OF DELETE
    AS
    BEGIN
        SET NOCOUNT ON;
        IF EXISTS (SELECT 1 FROM deleted WHERE ISNULL(review_status, ''PENDING'') <> ''PENDING'')
        BEGIN
            RAISERROR(''Cannot hard delete AI generated content that has been reviewed.'',16,1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        -- Allow deletion for PENDING rows only
        DELETE FROM dbo.ai_generated_contents WHERE id IN (SELECT id FROM deleted);
    END');
END

-- Note: This migration intentionally avoids destructive changes. If you need to retrofit
-- CHECK constraints or columns on existing tables, perform careful ALTER TABLE ... ADD
-- operations in a controlled migration ensuring no data is lost.

PRINT 'M14 AI module check/create script completed.';
