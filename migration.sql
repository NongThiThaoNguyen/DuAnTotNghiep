BEGIN TRANSACTION;
ALTER TABLE [placement_test_questions] DROP CONSTRAINT [FK_ptq_question];

ALTER TABLE [placement_test_questions] DROP CONSTRAINT [FK_ptq_section];

ALTER TABLE [test_answers] DROP CONSTRAINT [FK_ta_attempt];

ALTER TABLE [test_answers] DROP CONSTRAINT [FK_ta_question];

ALTER TABLE [test_attempts] DROP CONSTRAINT [FK_attempt_student];

ALTER TABLE [test_attempts] DROP CONSTRAINT [FK_attempt_test];

DROP INDEX [IX_test_attempt_student] ON [test_attempts];

DROP INDEX [IX_test_answers_attempt_id] ON [test_answers];

DROP INDEX [IX_placement_test_sections_placement_test_id] ON [placement_test_sections];

CREATE INDEX [IX_test_attempt_student] ON [test_attempts] ([student_id], [placement_test_id], [status]);

ALTER TABLE [test_attempts] ADD CONSTRAINT [CK_TestAttempt_Status] CHECK ([status] IN ('IN_PROGRESS', 'SUBMITTED', 'GRADED', 'EXPIRED'));

CREATE UNIQUE INDEX [UQ_test_answer_attempt_question] ON [test_answers] ([attempt_id], [question_id]);

ALTER TABLE [placement_tests] ADD CONSTRAINT [CK_PlacementTest_Status] CHECK ([status] IN ('DRAFT', 'PUBLISHED', 'ARCHIVED'));

CREATE INDEX [IX_placement_test_sections_test_skill] ON [placement_test_sections] ([placement_test_id], [skill_id]);

ALTER TABLE [placement_test_questions] ADD CONSTRAINT [FK_ptq_question] FOREIGN KEY ([question_id]) REFERENCES [question_bank] ([id]) ON DELETE NO ACTION;

ALTER TABLE [placement_test_questions] ADD CONSTRAINT [FK_ptq_section] FOREIGN KEY ([section_id]) REFERENCES [placement_test_sections] ([id]) ON DELETE NO ACTION;

ALTER TABLE [test_answers] ADD CONSTRAINT [FK_ta_attempt] FOREIGN KEY ([attempt_id]) REFERENCES [test_attempts] ([id]) ON DELETE NO ACTION;

ALTER TABLE [test_answers] ADD CONSTRAINT [FK_ta_question] FOREIGN KEY ([question_id]) REFERENCES [question_bank] ([id]) ON DELETE NO ACTION;

ALTER TABLE [test_attempts] ADD CONSTRAINT [FK_attempt_student] FOREIGN KEY ([student_id]) REFERENCES [users] ([id]) ON DELETE NO ACTION;

ALTER TABLE [test_attempts] ADD CONSTRAINT [FK_attempt_test] FOREIGN KEY ([placement_test_id]) REFERENCES [placement_tests] ([id]) ON DELETE NO ACTION;

COMMIT;
GO

