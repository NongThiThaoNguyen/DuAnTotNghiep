BEGIN TRANSACTION;
ALTER TABLE [question_bank] ADD CONSTRAINT [CK_QuestionBank_DifficultyLevel] CHECK ([difficulty_level] IN ('BASIC', 'MEDIUM', 'ADVANCED'));

ALTER TABLE [question_bank] ADD CONSTRAINT [CK_QuestionBank_QuestionType] CHECK ([question_type] IN ('MCQ', 'TRUE_FALSE', 'SHORT_ANSWER', 'LISTENING'));

ALTER TABLE [practice_tasks] ADD CONSTRAINT [CK_PracticeTask_DifficultyLevel] CHECK ([difficulty_level] IN ('BASIC', 'MEDIUM', 'ADVANCED'));

ALTER TABLE [practice_tasks] ADD CONSTRAINT [CK_PracticeTask_TaskType] CHECK ([task_type] IN ('MCQ', 'TRUE_FALSE', 'SHORT_ANSWER', 'LISTENING'));

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260624034209_StandardizeQuestionData', N'10.0.9');

COMMIT;
GO

