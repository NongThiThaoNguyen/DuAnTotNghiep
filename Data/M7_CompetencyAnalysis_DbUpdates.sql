-- =========================================================================
-- SQL Migration Script: Module M7 - Competency Analysis Database Enhancements
-- Safe execution with existence checks
-- =========================================================================

USE [AIStudyEnglish];
GO

-- 1. Update [competency_analyses]
PRINT 'Updating [competency_analyses] table...';

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[competency_analyses]') AND name = N'prioritized_topics_json')
BEGIN
    ALTER TABLE [dbo].[competency_analyses] ADD [prioritized_topics_json] NVARCHAR(MAX) NULL;
    PRINT 'Added column [prioritized_topics_json] to [competency_analyses].';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[competency_analyses]') AND name = N'knowledge_gaps_json')
BEGIN
    ALTER TABLE [dbo].[competency_analyses] ADD [knowledge_gaps_json] NVARCHAR(MAX) NULL;
    PRINT 'Added column [knowledge_gaps_json] to [competency_analyses].';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[competency_analyses]') AND name = N'metadata_json')
BEGIN
    ALTER TABLE [dbo].[competency_analyses] ADD [metadata_json] NVARCHAR(MAX) NULL;
    PRINT 'Added column [metadata_json] to [competency_analyses].';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[competency_analyses]') AND name = N'is_latest')
BEGIN
    ALTER TABLE [dbo].[competency_analyses] ADD [is_latest] BIT NOT NULL DEFAULT 1;
    PRINT 'Added column [is_latest] to [competency_analyses].';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[competency_analyses]') AND name = N'status')
BEGIN
    ALTER TABLE [dbo].[competency_analyses] ADD [status] NVARCHAR(50) NOT NULL DEFAULT 'COMPLETED';
    PRINT 'Added column [status] to [competency_analyses].';
END

-- Create indexes for [competency_analyses]
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[competency_analyses]') AND name = N'IX_competency_analyses_student_id')
BEGIN
    CREATE INDEX [IX_competency_analyses_student_id] ON [dbo].[competency_analyses]([student_id]);
    PRINT 'Created index [IX_competency_analyses_student_id].';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[competency_analyses]') AND name = N'IX_competency_analyses_test_attempt_id')
BEGIN
    CREATE INDEX [IX_competency_analyses_test_attempt_id] ON [dbo].[competency_analyses]([test_attempt_id]);
    PRINT 'Created index [IX_competency_analyses_test_attempt_id].';
END
GO


-- 2. Update [competency_skill_scores]
PRINT 'Updating [competency_skill_scores] table...';

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[competency_skill_scores]') AND name = N'topic_id')
BEGIN
    ALTER TABLE [dbo].[competency_skill_scores] ADD [topic_id] INT NULL;
    PRINT 'Added column [topic_id] to [competency_skill_scores].';
END

-- Add Foreign Key constraint for [topic_id]
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_css_topic]') AND parent_object_id = OBJECT_ID(N'[dbo].[competency_skill_scores]'))
BEGIN
    ALTER TABLE [dbo].[competency_skill_scores] 
    ADD CONSTRAINT [FK_css_topic] FOREIGN KEY ([topic_id]) REFERENCES [dbo].[learning_topics]([id]) ON DELETE NO ACTION;
    PRINT 'Added foreign key constraint [FK_css_topic] to [competency_skill_scores].';
END

-- Create indexes for [competency_skill_scores]
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[competency_skill_scores]') AND name = N'IX_competency_skill_scores_competency_analysis_id')
BEGIN
    CREATE INDEX [IX_competency_skill_scores_competency_analysis_id] ON [dbo].[competency_skill_scores]([competency_analysis_id]);
    PRINT 'Created index [IX_competency_skill_scores_competency_analysis_id].';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[competency_skill_scores]') AND name = N'IX_competency_skill_scores_skill_id')
BEGIN
    CREATE INDEX [IX_competency_skill_scores_skill_id] ON [dbo].[competency_skill_scores]([skill_id]);
    PRINT 'Created index [IX_competency_skill_scores_skill_id].';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[competency_skill_scores]') AND name = N'IX_competency_skill_scores_topic_id')
BEGIN
    CREATE INDEX [IX_competency_skill_scores_topic_id] ON [dbo].[competency_skill_scores]([topic_id]) WHERE [topic_id] IS NOT NULL;
    PRINT 'Created index [IX_competency_skill_scores_topic_id].';
END
GO


-- 3. Update [ai_prompt_templates]
PRINT 'Updating [ai_prompt_templates] table...';

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ai_prompt_templates]') AND name = N'user_prompt_template')
BEGIN
    ALTER TABLE [dbo].[ai_prompt_templates] ADD [user_prompt_template] NVARCHAR(MAX) NULL;
    PRINT 'Added column [user_prompt_template] to [ai_prompt_templates].';
END
GO


-- 4. Update [ai_usage_logs]
PRINT 'Updating [ai_usage_logs] table...';

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ai_usage_logs]') AND name = N'prompt_input')
BEGIN
    ALTER TABLE [dbo].[ai_usage_logs] ADD [prompt_input] NVARCHAR(MAX) NULL;
    PRINT 'Added column [prompt_input] to [ai_usage_logs].';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ai_usage_logs]') AND name = N'response_output')
BEGIN
    ALTER TABLE [dbo].[ai_usage_logs] ADD [response_output] NVARCHAR(MAX) NULL;
    PRINT 'Added column [response_output] to [ai_usage_logs].';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ai_usage_logs]') AND name = N'latency_ms')
BEGIN
    ALTER TABLE [dbo].[ai_usage_logs] ADD [latency_ms] INT NULL;
    PRINT 'Added column [latency_ms] to [ai_usage_logs].';
END

-- Create indexes for [ai_usage_logs]
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ai_usage_logs]') AND name = N'IX_ai_usage_logs_prompt_template_id')
BEGIN
    CREATE INDEX [IX_ai_usage_logs_prompt_template_id] ON [dbo].[ai_usage_logs]([prompt_template_id]);
    PRINT 'Created index [IX_ai_usage_logs_prompt_template_id].';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ai_usage_logs]') AND name = N'IX_ai_usage_logs_user_id')
BEGIN
    CREATE INDEX [IX_ai_usage_logs_user_id] ON [dbo].[ai_usage_logs]([user_id]);
    PRINT 'Created index [IX_ai_usage_logs_user_id].';
END
GO

PRINT 'Database update completed successfully!';
GO
