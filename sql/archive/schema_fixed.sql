IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [english_proficiency_levels] (
    [id] int NOT NULL IDENTITY,
    [code] varchar(20) NOT NULL,
    [name] nvarchar(100) NOT NULL,
    [order_index] int NOT NULL,
    [description] nvarchar(1000) NULL,
    CONSTRAINT [PK__english___3213E83F203E1EC7] PRIMARY KEY ([id])
);

CREATE TABLE [english_skills] (
    [id] int NOT NULL IDENTITY,
    [skill_code] varchar(50) NOT NULL,
    [skill_name] nvarchar(100) NOT NULL,
    [description] nvarchar(1000) NULL,
    [order_index] int NOT NULL,
    [is_active] bit NOT NULL DEFAULT CAST(1 AS bit),
    CONSTRAINT [PK__english___3213E83F663F25FF] PRIMARY KEY ([id])
);

CREATE TABLE [learning_goals] (
    [id] int NOT NULL IDENTITY,
    [goal_code] varchar(50) NOT NULL,
    [goal_name] nvarchar(255) NOT NULL,
    [description] nvarchar(1000) NULL,
    [is_active] bit NOT NULL DEFAULT CAST(1 AS bit),
    CONSTRAINT [PK__learning__3213E83FD1E17598] PRIMARY KEY ([id])
);

CREATE TABLE [roles] (
    [id] int NOT NULL IDENTITY,
    [role_code] varchar(50) NOT NULL,
    [role_name] nvarchar(100) NOT NULL,
    [description] nvarchar(500) NULL,
    [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK__roles__3213E83FC530F3DF] PRIMARY KEY ([id])
);

CREATE TABLE [users] (
    [id] int NOT NULL IDENTITY,
    [email] varchar(255) NOT NULL,
    [password_hash] varchar(500) NOT NULL,
    [full_name] nvarchar(255) NOT NULL,
    [role_id] int NOT NULL,
    [status] varchar(30) NOT NULL DEFAULT 'ACTIVE',
    [avatar_url] varchar(500) NULL,
    [phone] varchar(20) NULL,
    [last_login_at] datetime2 NULL,
    [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    [updated_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK__users__3213E83F43CE90C9] PRIMARY KEY ([id]),
    CONSTRAINT [FK_users_roles] FOREIGN KEY ([role_id]) REFERENCES [roles] ([id])
);

CREATE TABLE [ai_prompt_templates] (
    [id] int NOT NULL IDENTITY,
    [prompt_code] varchar(100) NOT NULL,
    [prompt_name] nvarchar(255) NOT NULL,
    [module_code] varchar(50) NOT NULL,
    [system_prompt] nvarchar(max) NOT NULL,
    [output_schema] nvarchar(max) NULL,
    [status] varchar(50) NOT NULL DEFAULT 'ACTIVE',
    [version_no] int NOT NULL DEFAULT 1,
    [created_by] int NULL,
    [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK__ai_promp__3213E83F34DDDEE9] PRIMARY KEY ([id]),
    CONSTRAINT [FK_prompt_created_by] FOREIGN KEY ([created_by]) REFERENCES [users] ([id])
);

CREATE TABLE [audit_logs] (
    [id] bigint NOT NULL IDENTITY,
    [user_id] int NULL,
    [action] varchar(100) NOT NULL,
    [entity_name] varchar(100) NULL,
    [entity_id] int NULL,
    [old_value] nvarchar(max) NULL,
    [new_value] nvarchar(max) NULL,
    [ip_address] varchar(45) NULL,
    [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK__audit_lo__3213E83F13E45708] PRIMARY KEY ([id]),
    CONSTRAINT [FK_audit_logs_users] FOREIGN KEY ([user_id]) REFERENCES [users] ([id])
);

CREATE TABLE [content_compliance_reviews] (
    [id] int NOT NULL IDENTITY,
    [content_type] varchar(50) NOT NULL,
    [content_id] int NOT NULL,
    [reviewer_id] int NOT NULL,
    [copyright_check] bit NOT NULL,
    [plagiarism_risk] varchar(30) NULL,
    [review_status] varchar(50) NOT NULL,
    [review_note] nvarchar(max) NULL,
    [reviewed_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK__content___3213E83FABD8D38F] PRIMARY KEY ([id]),
    CONSTRAINT [FK_content_reviews_reviewer] FOREIGN KEY ([reviewer_id]) REFERENCES [users] ([id])
);

CREATE TABLE [learning_path_templates] (
    [id] int NOT NULL IDENTITY,
    [template_name] nvarchar(255) NOT NULL,
    [goal_id] int NULL,
    [start_level_id] int NULL,
    [target_level_id] int NULL,
    [duration_weeks] int NULL,
    [description] nvarchar(max) NULL,
    [status] varchar(50) NOT NULL DEFAULT 'DRAFT',
    [created_by] int NULL,
    [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK__learning__3213E83F1B0089FE] PRIMARY KEY ([id]),
    CONSTRAINT [FK_lpt_created_by] FOREIGN KEY ([created_by]) REFERENCES [users] ([id]),
    CONSTRAINT [FK_lpt_goal] FOREIGN KEY ([goal_id]) REFERENCES [learning_goals] ([id]),
    CONSTRAINT [FK_lpt_start_level] FOREIGN KEY ([start_level_id]) REFERENCES [english_proficiency_levels] ([id]),
    CONSTRAINT [FK_lpt_target_level] FOREIGN KEY ([target_level_id]) REFERENCES [english_proficiency_levels] ([id])
);

CREATE TABLE [learning_topics] (
    [id] int NOT NULL IDENTITY,
    [skill_id] int NOT NULL,
    [parent_topic_id] int NULL,
    [level_id] int NULL,
    [topic_code] varchar(100) NULL,
    [title] nvarchar(255) NOT NULL,
    [description] nvarchar(max) NULL,
    [difficulty_level] varchar(30) NOT NULL DEFAULT 'BASIC',
    [estimated_minutes] int NULL,
    [order_index] int NOT NULL DEFAULT 1,
    [status] varchar(30) NOT NULL DEFAULT 'ACTIVE',
    [created_by] int NULL,
    [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    [updated_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK__learning__3213E83F8C330B01] PRIMARY KEY ([id]),
    CONSTRAINT [FK_topics_level] FOREIGN KEY ([level_id]) REFERENCES [english_proficiency_levels] ([id]),
    CONSTRAINT [FK_topics_parent] FOREIGN KEY ([parent_topic_id]) REFERENCES [learning_topics] ([id]),
    CONSTRAINT [FK_topics_skills] FOREIGN KEY ([skill_id]) REFERENCES [english_skills] ([id]),
    CONSTRAINT [FK_topics_users] FOREIGN KEY ([created_by]) REFERENCES [users] ([id])
);

CREATE TABLE [notifications] (
    [id] int NOT NULL IDENTITY,
    [title] nvarchar(255) NOT NULL,
    [content] nvarchar(max) NOT NULL,
    [notification_type] varchar(50) NOT NULL,
    [target_user_id] int NULL,
    [created_by] int NULL,
    [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK__notifica__3213E83FE246EE88] PRIMARY KEY ([id]),
    CONSTRAINT [FK_notifications_created_by] FOREIGN KEY ([created_by]) REFERENCES [users] ([id]),
    CONSTRAINT [FK_notifications_target] FOREIGN KEY ([target_user_id]) REFERENCES [users] ([id])
);

CREATE TABLE [placement_tests] (
    [id] int NOT NULL IDENTITY,
    [title] nvarchar(255) NOT NULL,
    [description] nvarchar(max) NULL,
    [target_level_id] int NULL,
    [time_limit_minutes] int NULL,
    [total_score] decimal(6,2) NOT NULL DEFAULT 100.0,
    [status] varchar(50) NOT NULL DEFAULT 'DRAFT',
    [created_by] int NULL,
    [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    [updated_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK__placemen__3213E83F955AE2ED] PRIMARY KEY ([id]),
    CONSTRAINT [FK_pt_created_by] FOREIGN KEY ([created_by]) REFERENCES [users] ([id]),
    CONSTRAINT [FK_pt_level] FOREIGN KEY ([target_level_id]) REFERENCES [english_proficiency_levels] ([id])
);

CREATE TABLE [reference_sources] (
    [id] int NOT NULL IDENTITY,
    [source_name] nvarchar(255) NOT NULL,
    [source_url] varchar(1000) NULL,
    [source_type] varchar(50) NOT NULL,
    [license_note] nvarchar(max) NULL,
    [usage_policy] nvarchar(max) NULL,
    [status] varchar(50) NOT NULL DEFAULT 'PENDING',
    [created_by] int NULL,
    [approved_by] int NULL,
    [approved_at] datetime2 NULL,
    [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK__referenc__3213E83F85C4EC5F] PRIMARY KEY ([id]),
    CONSTRAINT [FK_ref_approved_by] FOREIGN KEY ([approved_by]) REFERENCES [users] ([id]),
    CONSTRAINT [FK_ref_created_by] FOREIGN KEY ([created_by]) REFERENCES [users] ([id])
);

CREATE TABLE [refresh_tokens] (
    [id] int NOT NULL IDENTITY,
    [user_id] int NOT NULL,
    [token_hash] varchar(500) NOT NULL,
    [expires_at] datetime2 NOT NULL,
    [revoked_at] datetime2 NULL,
    [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK__refresh___3213E83FF99585F1] PRIMARY KEY ([id]),
    CONSTRAINT [FK_refresh_tokens_users] FOREIGN KEY ([user_id]) REFERENCES [users] ([id])
);

CREATE TABLE [report_snapshots] (
    [id] int NOT NULL IDENTITY,
    [report_type] varchar(50) NOT NULL,
    [report_date] date NOT NULL DEFAULT ((CONVERT([date],sysutcdatetime()))),
    [generated_by] int NULL,
    [report_data] nvarchar(max) NOT NULL,
    [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK__report_s__3213E83F404FE2DE] PRIMARY KEY ([id]),
    CONSTRAINT [FK_report_generated_by] FOREIGN KEY ([generated_by]) REFERENCES [users] ([id])
);

CREATE TABLE [search_logs] (
    [id] bigint NOT NULL IDENTITY,
    [user_id] int NULL,
    [keyword] nvarchar(255) NOT NULL,
    [search_scope] varchar(50) NOT NULL DEFAULT 'ALL',
    [result_count] int NOT NULL,
    [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK__search_l__3213E83FBB7ECF99] PRIMARY KEY ([id]),
    CONSTRAINT [FK_search_user] FOREIGN KEY ([user_id]) REFERENCES [users] ([id])
);

CREATE TABLE [student_learning_profiles] (
    [id] int NOT NULL IDENTITY,
    [user_id] int NOT NULL,
    [current_level_id] int NULL,
    [target_level_id] int NULL,
    [main_goal_id] int NULL,
    [target_score] decimal(5,2) NULL,
    [daily_study_minutes] int NULL,
    [weekly_study_days] int NULL,
    [preferred_study_time] varchar(50) NULL,
    [learning_note] nvarchar(max) NULL,
    [onboarding_status] varchar(30) NOT NULL DEFAULT 'NOT_STARTED',
    [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    [updated_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK__student___3213E83FB021A51B] PRIMARY KEY ([id]),
    CONSTRAINT [FK_slp_current_level] FOREIGN KEY ([current_level_id]) REFERENCES [english_proficiency_levels] ([id]),
    CONSTRAINT [FK_slp_goal] FOREIGN KEY ([main_goal_id]) REFERENCES [learning_goals] ([id]),
    CONSTRAINT [FK_slp_target_level] FOREIGN KEY ([target_level_id]) REFERENCES [english_proficiency_levels] ([id]),
    CONSTRAINT [FK_slp_users] FOREIGN KEY ([user_id]) REFERENCES [users] ([id])
);

CREATE TABLE [system_settings] (
    [id] int NOT NULL IDENTITY,
    [setting_key] varchar(100) NOT NULL,
    [setting_value] nvarchar(max) NULL,
    [description] nvarchar(500) NULL,
    [updated_by] int NULL,
    [updated_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK__system_s__3213E83FCC2454AB] PRIMARY KEY ([id]),
    CONSTRAINT [FK_settings_updated_by] FOREIGN KEY ([updated_by]) REFERENCES [users] ([id])
);

CREATE TABLE [user_profiles] (
    [id] int NOT NULL IDENTITY,
    [user_id] int NOT NULL,
    [date_of_birth] date NULL,
    [gender] varchar(20) NULL,
    [country] nvarchar(100) NULL DEFAULT N'Viá»‡t Nam',
    [bio] nvarchar(max) NULL,
    [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    [updated_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK__user_pro__3213E83FD33481F9] PRIMARY KEY ([id]),
    CONSTRAINT [FK_user_profiles_users] FOREIGN KEY ([user_id]) REFERENCES [users] ([id])
);

CREATE TABLE [user_sessions] (
    [id] int NOT NULL IDENTITY,
    [user_id] int NOT NULL,
    [session_token] varchar(500) NOT NULL,
    [ip_address] varchar(45) NULL,
    [user_agent] nvarchar(500) NULL,
    [device_type] varchar(50) NULL,
    [last_activity_at] datetime2 NULL,
    [expires_at] datetime2 NOT NULL,
    [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK__user_ses__3213E83FA162BF6F] PRIMARY KEY ([id]),
    CONSTRAINT [FK_user_sessions_users] FOREIGN KEY ([user_id]) REFERENCES [users] ([id])
);

CREATE TABLE [ai_usage_logs] (
    [id] bigint NOT NULL IDENTITY,
    [user_id] int NULL,
    [module_code] varchar(50) NOT NULL,
    [prompt_template_id] int NULL,
    [ai_model] varchar(100) NULL,
    [input_tokens] int NULL,
    [output_tokens] int NULL,
    [cost_estimate] decimal(18,6) NULL,
    [request_status] varchar(50) NOT NULL DEFAULT 'SUCCESS',
    [error_message] nvarchar(max) NULL,
    [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK__ai_usage__3213E83F2E522233] PRIMARY KEY ([id]),
    CONSTRAINT [FK_ai_usage_prompt] FOREIGN KEY ([prompt_template_id]) REFERENCES [ai_prompt_templates] ([id]),
    CONSTRAINT [FK_ai_usage_user] FOREIGN KEY ([user_id]) REFERENCES [users] ([id])
);

CREATE TABLE [ai_generated_contents] (
    [id] int NOT NULL IDENTITY,
    [requested_by] int NULL,
    [content_type] varchar(50) NOT NULL,
    [related_topic_id] int NULL,
    [prompt_text] nvarchar(max) NULL,
    [generated_content] nvarchar(max) NOT NULL,
    [ai_model] varchar(100) NULL,
    [review_status] varchar(50) NOT NULL DEFAULT 'PENDING',
    [reviewed_by] int NULL,
    [review_note] nvarchar(max) NULL,
    [reviewed_at] datetime2 NULL,
    [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK__ai_gener__3213E83F42F7F589] PRIMARY KEY ([id]),
    CONSTRAINT [FK_aig_requested_by] FOREIGN KEY ([requested_by]) REFERENCES [users] ([id]),
    CONSTRAINT [FK_aig_reviewed_by] FOREIGN KEY ([reviewed_by]) REFERENCES [users] ([id]),
    CONSTRAINT [FK_aig_topic] FOREIGN KEY ([related_topic_id]) REFERENCES [learning_topics] ([id])
);

CREATE TABLE [learning_objectives] (
    [id] int NOT NULL IDENTITY,
    [topic_id] int NOT NULL,
    [objective_text] nvarchar(max) NOT NULL,
    [cognitive_level] varchar(50) NOT NULL DEFAULT 'UNDERSTAND',
    [order_index] int NOT NULL DEFAULT 1,
    CONSTRAINT [PK__learning__3213E83F6239F152] PRIMARY KEY ([id]),
    CONSTRAINT [FK_objectives_topics] FOREIGN KEY ([topic_id]) REFERENCES [learning_topics] ([id])
);

CREATE TABLE [learning_path_template_nodes] (
    [id] int NOT NULL IDENTITY,
    [template_id] int NOT NULL,
    [topic_id] int NULL,
    [skill_id] int NULL,
    [node_title] nvarchar(255) NOT NULL,
    [node_type] varchar(50) NOT NULL,
    [estimated_minutes] int NULL,
    [order_index] int NOT NULL,
    [unlock_condition] nvarchar(max) NULL,
    CONSTRAINT [PK__learning__3213E83F13D5DDD4] PRIMARY KEY ([id]),
    CONSTRAINT [FK_lptn_skill] FOREIGN KEY ([skill_id]) REFERENCES [english_skills] ([id]),
    CONSTRAINT [FK_lptn_template] FOREIGN KEY ([template_id]) REFERENCES [learning_path_templates] ([id]),
    CONSTRAINT [FK_lptn_topic] FOREIGN KEY ([topic_id]) REFERENCES [learning_topics] ([id])
);

CREATE TABLE [original_lessons] (
    [id] int NOT NULL IDENTITY,
    [topic_id] int NOT NULL,
    [title] nvarchar(255) NOT NULL,
    [summary] nvarchar(max) NULL,
    [content] nvarchar(max) NULL,
    [content_type] varchar(50) NOT NULL DEFAULT 'TEXT',
    [estimated_minutes] int NULL,
    [source_type] varchar(50) NOT NULL DEFAULT 'SELF_CREATED',
    [review_status] varchar(50) NOT NULL DEFAULT 'DRAFT',
    [is_ai_generated] bit NOT NULL,
    [created_by] int NULL,
    [approved_by] int NULL,
    [approved_at] datetime2 NULL,
    [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    [updated_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK__original__3213E83FFCDBE3D5] PRIMARY KEY ([id]),
    CONSTRAINT [FK_lessons_approved_by] FOREIGN KEY ([approved_by]) REFERENCES [users] ([id]),
    CONSTRAINT [FK_lessons_created_by] FOREIGN KEY ([created_by]) REFERENCES [users] ([id]),
    CONSTRAINT [FK_lessons_topics] FOREIGN KEY ([topic_id]) REFERENCES [learning_topics] ([id])
);

CREATE TABLE [practice_tasks] (
    [id] int NOT NULL IDENTITY,
    [topic_id] int NULL,
    [skill_id] int NOT NULL,
    [title] nvarchar(255) NOT NULL,
    [instruction] nvarchar(max) NOT NULL,
    [task_type] varchar(50) NOT NULL,
    [difficulty_level] varchar(30) NOT NULL DEFAULT 'BASIC',
    [status] varchar(50) NOT NULL DEFAULT 'PUBLISHED',
    [created_by] int NULL,
    [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK__practice__3213E83F9C46F05B] PRIMARY KEY ([id]),
    CONSTRAINT [FK_practice_created_by] FOREIGN KEY ([created_by]) REFERENCES [users] ([id]),
    CONSTRAINT [FK_practice_skill] FOREIGN KEY ([skill_id]) REFERENCES [english_skills] ([id]),
    CONSTRAINT [FK_practice_topic] FOREIGN KEY ([topic_id]) REFERENCES [learning_topics] ([id])
);

CREATE TABLE [question_bank] (
    [id] int NOT NULL IDENTITY,
    [topic_id] int NULL,
    [skill_id] int NOT NULL,
    [level_id] int NULL,
    [question_type] varchar(50) NOT NULL,
    [question_text] nvarchar(max) NOT NULL,
    [audio_url] varchar(1000) NULL,
    [image_url] varchar(1000) NULL,
    [correct_answer] nvarchar(max) NULL,
    [explanation] nvarchar(max) NULL,
    [difficulty_level] varchar(30) NOT NULL DEFAULT 'BASIC',
    [source_type] varchar(50) NOT NULL DEFAULT 'SELF_CREATED',
    [review_status] varchar(50) NOT NULL DEFAULT 'DRAFT',
    [created_by] int NULL,
    [approved_by] int NULL,
    [approved_at] datetime2 NULL,
    [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    [updated_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK__question__3213E83F73ECF016] PRIMARY KEY ([id]),
    CONSTRAINT [FK_qb_approved_by] FOREIGN KEY ([approved_by]) REFERENCES [users] ([id]),
    CONSTRAINT [FK_qb_created_by] FOREIGN KEY ([created_by]) REFERENCES [users] ([id]),
    CONSTRAINT [FK_qb_level] FOREIGN KEY ([level_id]) REFERENCES [english_proficiency_levels] ([id]),
    CONSTRAINT [FK_qb_skill] FOREIGN KEY ([skill_id]) REFERENCES [english_skills] ([id]),
    CONSTRAINT [FK_qb_topic] FOREIGN KEY ([topic_id]) REFERENCES [learning_topics] ([id])
);

CREATE TABLE [quizzes] (
    [id] int NOT NULL IDENTITY,
    [topic_id] int NULL,
    [skill_id] int NOT NULL,
    [title] nvarchar(255) NOT NULL,
    [description] nvarchar(max) NULL,
    [quiz_type] varchar(50) NOT NULL DEFAULT 'PRACTICE',
    [time_limit_minutes] int NULL,
    [passing_score] decimal(6,2) NULL,
    [status] varchar(50) NOT NULL DEFAULT 'DRAFT',
    [created_by] int NULL,
    [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK__quizzes__3213E83F0BD02077] PRIMARY KEY ([id]),
    CONSTRAINT [FK_quiz_created_by] FOREIGN KEY ([created_by]) REFERENCES [users] ([id]),
    CONSTRAINT [FK_quiz_skill] FOREIGN KEY ([skill_id]) REFERENCES [english_skills] ([id]),
    CONSTRAINT [FK_quiz_topic] FOREIGN KEY ([topic_id]) REFERENCES [learning_topics] ([id])
);

CREATE TABLE [student_progress_snapshots] (
    [id] int NOT NULL IDENTITY,
    [student_id] int NOT NULL,
    [skill_id] int NULL,
    [topic_id] int NULL,
    [progress_percent] decimal(5,2) NOT NULL,
    [average_score] decimal(6,2) NULL,
    [total_study_minutes] int NOT NULL,
    [completed_nodes] int NOT NULL,
    [weak_points] nvarchar(max) NULL,
    [snapshot_date] date NOT NULL DEFAULT ((CONVERT([date],sysutcdatetime()))),
    CONSTRAINT [PK__student___3213E83F6D6FC72B] PRIMARY KEY ([id]),
    CONSTRAINT [FK_sps_skill] FOREIGN KEY ([skill_id]) REFERENCES [english_skills] ([id]),
    CONSTRAINT [FK_sps_student] FOREIGN KEY ([student_id]) REFERENCES [users] ([id]),
    CONSTRAINT [FK_sps_topic] FOREIGN KEY ([topic_id]) REFERENCES [learning_topics] ([id])
);

CREATE TABLE [notification_reads] (
    [id] int NOT NULL IDENTITY,
    [notification_id] int NOT NULL,
    [user_id] int NOT NULL,
    [read_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK__notifica__3213E83FE8DFDE34] PRIMARY KEY ([id]),
    CONSTRAINT [FK_nr_notification] FOREIGN KEY ([notification_id]) REFERENCES [notifications] ([id]),
    CONSTRAINT [FK_nr_user] FOREIGN KEY ([user_id]) REFERENCES [users] ([id])
);

CREATE TABLE [placement_test_sections] (
    [id] int NOT NULL IDENTITY,
    [placement_test_id] int NOT NULL,
    [skill_id] int NOT NULL,
    [section_name] nvarchar(255) NOT NULL,
    [instruction] nvarchar(max) NULL,
    [order_index] int NOT NULL DEFAULT 1,
    [max_score] decimal(6,2) NOT NULL,
    CONSTRAINT [PK__placemen__3213E83FC01FC668] PRIMARY KEY ([id]),
    CONSTRAINT [FK_pts_skill] FOREIGN KEY ([skill_id]) REFERENCES [english_skills] ([id]),
    CONSTRAINT [FK_pts_test] FOREIGN KEY ([placement_test_id]) REFERENCES [placement_tests] ([id])
);

CREATE TABLE [test_attempts] (
    [id] int NOT NULL IDENTITY,
    [placement_test_id] int NOT NULL,
    [student_id] int NOT NULL,
    [started_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    [submitted_at] datetime2 NULL,
    [total_score] decimal(6,2) NULL,
    [estimated_level_id] int NULL,
    [status] varchar(50) NOT NULL DEFAULT 'IN_PROGRESS',
    CONSTRAINT [PK__test_att__3213E83FE21DABC0] PRIMARY KEY ([id]),
    CONSTRAINT [FK_attempt_level] FOREIGN KEY ([estimated_level_id]) REFERENCES [english_proficiency_levels] ([id]),
    CONSTRAINT [FK_attempt_student] FOREIGN KEY ([student_id]) REFERENCES [users] ([id]),
    CONSTRAINT [FK_attempt_test] FOREIGN KEY ([placement_test_id]) REFERENCES [placement_tests] ([id])
);

CREATE TABLE [topic_references] (
    [id] int NOT NULL IDENTITY,
    [topic_id] int NOT NULL,
    [reference_source_id] int NOT NULL,
    [note] nvarchar(1000) NULL,
    [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK__topic_re__3213E83FAB67747E] PRIMARY KEY ([id]),
    CONSTRAINT [FK_topic_ref_source] FOREIGN KEY ([reference_source_id]) REFERENCES [reference_sources] ([id]),
    CONSTRAINT [FK_topic_ref_topic] FOREIGN KEY ([topic_id]) REFERENCES [learning_topics] ([id])
);

CREATE TABLE [student_skill_preferences] (
    [id] int NOT NULL IDENTITY,
    [student_profile_id] int NOT NULL,
    [skill_code] varchar(50) NOT NULL,
    [priority_level] int NOT NULL DEFAULT 1,
    [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK__student___3213E83F3D22755E] PRIMARY KEY ([id]),
    CONSTRAINT [FK_ssp_profile] FOREIGN KEY ([student_profile_id]) REFERENCES [student_learning_profiles] ([id])
);

CREATE TABLE [practice_submissions] (
    [id] int NOT NULL IDENTITY,
    [practice_task_id] int NOT NULL,
    [student_id] int NOT NULL,
    [submission_text] nvarchar(max) NULL,
    [file_url] varchar(1000) NULL,
    [audio_url] varchar(1000) NULL,
    [submitted_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    [score] decimal(6,2) NULL,
    [ai_feedback] nvarchar(max) NULL,
    [teacher_feedback] nvarchar(max) NULL,
    [status] varchar(50) NOT NULL DEFAULT 'SUBMITTED',
    CONSTRAINT [PK__practice__3213E83FFB71DA05] PRIMARY KEY ([id]),
    CONSTRAINT [FK_ps_student] FOREIGN KEY ([student_id]) REFERENCES [users] ([id]),
    CONSTRAINT [FK_ps_task] FOREIGN KEY ([practice_task_id]) REFERENCES [practice_tasks] ([id])
);

CREATE TABLE [question_options] (
    [id] int NOT NULL IDENTITY,
    [question_id] int NOT NULL,
    [option_text] nvarchar(max) NOT NULL,
    [is_correct] bit NOT NULL,
    [order_index] int NOT NULL DEFAULT 1,
    CONSTRAINT [PK__question__3213E83FC1CBAC30] PRIMARY KEY ([id]),
    CONSTRAINT [FK_options_question] FOREIGN KEY ([question_id]) REFERENCES [question_bank] ([id])
);

CREATE TABLE [quiz_attempts] (
    [id] int NOT NULL IDENTITY,
    [quiz_id] int NOT NULL,
    [student_id] int NOT NULL,
    [started_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    [submitted_at] datetime2 NULL,
    [score] decimal(6,2) NULL,
    [status] varchar(50) NOT NULL DEFAULT 'IN_PROGRESS',
    CONSTRAINT [PK__quiz_att__3213E83F46B04B87] PRIMARY KEY ([id]),
    CONSTRAINT [FK_qa_quiz] FOREIGN KEY ([quiz_id]) REFERENCES [quizzes] ([id]),
    CONSTRAINT [FK_qa_student] FOREIGN KEY ([student_id]) REFERENCES [users] ([id])
);

CREATE TABLE [quiz_questions] (
    [id] int NOT NULL IDENTITY,
    [quiz_id] int NOT NULL,
    [question_id] int NOT NULL,
    [points] decimal(6,2) NOT NULL DEFAULT 1.0,
    [order_index] int NOT NULL DEFAULT 1,
    CONSTRAINT [PK__quiz_que__3213E83FAD1F10DF] PRIMARY KEY ([id]),
    CONSTRAINT [FK_qq_question] FOREIGN KEY ([question_id]) REFERENCES [question_bank] ([id]),
    CONSTRAINT [FK_qq_quiz] FOREIGN KEY ([quiz_id]) REFERENCES [quizzes] ([id])
);

CREATE TABLE [placement_test_questions] (
    [id] int NOT NULL IDENTITY,
    [section_id] int NOT NULL,
    [question_id] int NOT NULL,
    [points] decimal(6,2) NOT NULL DEFAULT 1.0,
    [order_index] int NOT NULL DEFAULT 1,
    CONSTRAINT [PK__placemen__3213E83FBEEA4D47] PRIMARY KEY ([id]),
    CONSTRAINT [FK_ptq_question] FOREIGN KEY ([question_id]) REFERENCES [question_bank] ([id]),
    CONSTRAINT [FK_ptq_section] FOREIGN KEY ([section_id]) REFERENCES [placement_test_sections] ([id])
);

CREATE TABLE [competency_analyses] (
    [id] int NOT NULL IDENTITY,
    [student_id] int NOT NULL,
    [test_attempt_id] int NULL,
    [summary] nvarchar(max) NOT NULL,
    [current_level_id] int NULL,
    [recommended_level_id] int NULL,
    [strengths] nvarchar(max) NULL,
    [weaknesses] nvarchar(max) NULL,
    [gap_analysis] nvarchar(max) NULL,
    [ai_model] varchar(100) NULL,
    [confidence_score] decimal(5,2) NULL,
    [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK__competen__3213E83FBF5D890C] PRIMARY KEY ([id]),
    CONSTRAINT [FK_ca_attempt] FOREIGN KEY ([test_attempt_id]) REFERENCES [test_attempts] ([id]),
    CONSTRAINT [FK_ca_current_level] FOREIGN KEY ([current_level_id]) REFERENCES [english_proficiency_levels] ([id]),
    CONSTRAINT [FK_ca_recommended_level] FOREIGN KEY ([recommended_level_id]) REFERENCES [english_proficiency_levels] ([id]),
    CONSTRAINT [FK_ca_student] FOREIGN KEY ([student_id]) REFERENCES [users] ([id])
);

CREATE TABLE [test_answers] (
    [id] int NOT NULL IDENTITY,
    [attempt_id] int NOT NULL,
    [question_id] int NOT NULL,
    [selected_option_id] int NULL,
    [answer_text] nvarchar(max) NULL,
    [is_correct] bit NULL,
    [score] decimal(6,2) NULL,
    [answered_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK__test_ans__3213E83F49C84548] PRIMARY KEY ([id]),
    CONSTRAINT [FK_ta_attempt] FOREIGN KEY ([attempt_id]) REFERENCES [test_attempts] ([id]),
    CONSTRAINT [FK_ta_option] FOREIGN KEY ([selected_option_id]) REFERENCES [question_options] ([id]),
    CONSTRAINT [FK_ta_question] FOREIGN KEY ([question_id]) REFERENCES [question_bank] ([id])
);

CREATE TABLE [ai_feedbacks] (
    [id] int NOT NULL IDENTITY,
    [student_id] int NOT NULL,
    [quiz_attempt_id] int NULL,
    [practice_submission_id] int NULL,
    [feedback_type] varchar(50) NOT NULL,
    [feedback_text] nvarchar(max) NOT NULL,
    [mistake_analysis] nvarchar(max) NULL,
    [recommended_action] nvarchar(max) NULL,
    [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK__ai_feedb__3213E83F659EE2FA] PRIMARY KEY ([id]),
    CONSTRAINT [FK_feedback_practice] FOREIGN KEY ([practice_submission_id]) REFERENCES [practice_submissions] ([id]),
    CONSTRAINT [FK_feedback_quiz_attempt] FOREIGN KEY ([quiz_attempt_id]) REFERENCES [quiz_attempts] ([id]),
    CONSTRAINT [FK_feedback_student] FOREIGN KEY ([student_id]) REFERENCES [users] ([id])
);

CREATE TABLE [quiz_answers] (
    [id] int NOT NULL IDENTITY,
    [quiz_attempt_id] int NOT NULL,
    [question_id] int NOT NULL,
    [selected_option_id] int NULL,
    [answer_text] nvarchar(max) NULL,
    [is_correct] bit NULL,
    [score] decimal(6,2) NULL,
    [ai_explanation] nvarchar(max) NULL,
    [answered_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK__quiz_ans__3213E83F04A1567E] PRIMARY KEY ([id]),
    CONSTRAINT [FK_qans_attempt] FOREIGN KEY ([quiz_attempt_id]) REFERENCES [quiz_attempts] ([id]),
    CONSTRAINT [FK_qans_option] FOREIGN KEY ([selected_option_id]) REFERENCES [question_options] ([id]),
    CONSTRAINT [FK_qans_question] FOREIGN KEY ([question_id]) REFERENCES [question_bank] ([id])
);

CREATE TABLE [competency_skill_scores] (
    [id] int NOT NULL IDENTITY,
    [competency_analysis_id] int NOT NULL,
    [skill_id] int NOT NULL,
    [score] decimal(6,2) NOT NULL,
    [level_id] int NULL,
    [weakness_note] nvarchar(max) NULL,
    [priority_level] int NOT NULL DEFAULT 1,
    CONSTRAINT [PK__competen__3213E83FC90ADCC0] PRIMARY KEY ([id]),
    CONSTRAINT [FK_css_analysis] FOREIGN KEY ([competency_analysis_id]) REFERENCES [competency_analyses] ([id]),
    CONSTRAINT [FK_css_level] FOREIGN KEY ([level_id]) REFERENCES [english_proficiency_levels] ([id]),
    CONSTRAINT [FK_css_skill] FOREIGN KEY ([skill_id]) REFERENCES [english_skills] ([id])
);

CREATE TABLE [student_learning_paths] (
    [id] int NOT NULL IDENTITY,
    [student_id] int NOT NULL,
    [template_id] int NULL,
    [competency_analysis_id] int NULL,
    [title] nvarchar(255) NOT NULL,
    [description] nvarchar(max) NULL,
    [goal_id] int NULL,
    [start_date] date NULL,
    [target_end_date] date NULL,
    [ai_plan_summary] nvarchar(max) NULL,
    [status] varchar(50) NOT NULL DEFAULT 'ACTIVE',
    [generated_by_ai] bit NOT NULL DEFAULT CAST(1 AS bit),
    [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    [updated_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK__student___3213E83F4EC62BD7] PRIMARY KEY ([id]),
    CONSTRAINT [FK_slpath_analysis] FOREIGN KEY ([competency_analysis_id]) REFERENCES [competency_analyses] ([id]),
    CONSTRAINT [FK_slpath_goal] FOREIGN KEY ([goal_id]) REFERENCES [learning_goals] ([id]),
    CONSTRAINT [FK_slpath_student] FOREIGN KEY ([student_id]) REFERENCES [users] ([id]),
    CONSTRAINT [FK_slpath_template] FOREIGN KEY ([template_id]) REFERENCES [learning_path_templates] ([id])
);

CREATE TABLE [ai_replanning_events] (
    [id] int NOT NULL IDENTITY,
    [student_id] int NOT NULL,
    [learning_path_id] int NOT NULL,
    [trigger_type] varchar(50) NOT NULL,
    [old_plan_summary] nvarchar(max) NULL,
    [new_plan_summary] nvarchar(max) NULL,
    [reason] nvarchar(max) NOT NULL,
    [status] varchar(50) NOT NULL DEFAULT 'APPLIED',
    [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK__ai_repla__3213E83F1F00B02E] PRIMARY KEY ([id]),
    CONSTRAINT [FK_replan_path] FOREIGN KEY ([learning_path_id]) REFERENCES [student_learning_paths] ([id]),
    CONSTRAINT [FK_replan_student] FOREIGN KEY ([student_id]) REFERENCES [users] ([id])
);

CREATE TABLE [learning_path_nodes] (
    [id] int NOT NULL IDENTITY,
    [learning_path_id] int NOT NULL,
    [topic_id] int NULL,
    [lesson_id] int NULL,
    [quiz_id] int NULL,
    [practice_task_id] int NULL,
    [node_title] nvarchar(255) NOT NULL,
    [node_description] nvarchar(max) NULL,
    [node_type] varchar(50) NOT NULL,
    [path_phase] varchar(50) NULL,
    [scheduled_date] date NULL,
    [estimated_minutes] int NULL,
    [order_index] int NOT NULL,
    [status] varchar(50) NOT NULL DEFAULT 'LOCKED',
    [ai_reason] nvarchar(max) NULL,
    [completed_at] datetime2 NULL,
    CONSTRAINT [PK__learning__3213E83FDB07DA35] PRIMARY KEY ([id]),
    CONSTRAINT [FK_lpn_lesson] FOREIGN KEY ([lesson_id]) REFERENCES [original_lessons] ([id]),
    CONSTRAINT [FK_lpn_path] FOREIGN KEY ([learning_path_id]) REFERENCES [student_learning_paths] ([id]),
    CONSTRAINT [FK_lpn_quiz] FOREIGN KEY ([quiz_id]) REFERENCES [quizzes] ([id]),
    CONSTRAINT [FK_lpn_task] FOREIGN KEY ([practice_task_id]) REFERENCES [practice_tasks] ([id]),
    CONSTRAINT [FK_lpn_topic] FOREIGN KEY ([topic_id]) REFERENCES [learning_topics] ([id])
);

CREATE TABLE [ai_tutor_conversations] (
    [id] int NOT NULL IDENTITY,
    [student_id] int NOT NULL,
    [topic_id] int NULL,
    [learning_path_node_id] int NULL,
    [title] nvarchar(255) NULL,
    [status] varchar(50) NOT NULL DEFAULT 'ACTIVE',
    [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    [updated_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK__ai_tutor__3213E83F49FD9E0A] PRIMARY KEY ([id]),
    CONSTRAINT [FK_conv_node] FOREIGN KEY ([learning_path_node_id]) REFERENCES [learning_path_nodes] ([id]),
    CONSTRAINT [FK_conv_student] FOREIGN KEY ([student_id]) REFERENCES [users] ([id]),
    CONSTRAINT [FK_conv_topic] FOREIGN KEY ([topic_id]) REFERENCES [learning_topics] ([id])
);

CREATE TABLE [study_activity_logs] (
    [id] bigint NOT NULL IDENTITY,
    [student_id] int NOT NULL,
    [activity_type] varchar(50) NOT NULL,
    [topic_id] int NULL,
    [learning_path_node_id] int NULL,
    [duration_minutes] int NULL,
    [score] decimal(6,2) NULL,
    [metadata] nvarchar(max) NULL,
    [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK__study_ac__3213E83F34CF263F] PRIMARY KEY ([id]),
    CONSTRAINT [FK_sal_node] FOREIGN KEY ([learning_path_node_id]) REFERENCES [learning_path_nodes] ([id]),
    CONSTRAINT [FK_sal_student] FOREIGN KEY ([student_id]) REFERENCES [users] ([id]),
    CONSTRAINT [FK_sal_topic] FOREIGN KEY ([topic_id]) REFERENCES [learning_topics] ([id])
);

CREATE TABLE [ai_tutor_messages] (
    [id] bigint NOT NULL IDENTITY,
    [conversation_id] int NOT NULL,
    [sender_type] varchar(20) NOT NULL,
    [message_text] nvarchar(max) NOT NULL,
    [ai_model] varchar(100) NULL,
    [token_usage] int NULL,
    [safety_flag] varchar(50) NULL,
    [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK__ai_tutor__3213E83FA2FBD9A7] PRIMARY KEY ([id]),
    CONSTRAINT [FK_msg_conversation] FOREIGN KEY ([conversation_id]) REFERENCES [ai_tutor_conversations] ([id])
);

CREATE INDEX [IX_ai_feedbacks_practice_submission_id] ON [ai_feedbacks] ([practice_submission_id]);

CREATE INDEX [IX_ai_feedbacks_quiz_attempt_id] ON [ai_feedbacks] ([quiz_attempt_id]);

CREATE INDEX [IX_ai_feedbacks_student_id] ON [ai_feedbacks] ([student_id]);

CREATE INDEX [IX_ai_generated_contents_related_topic_id] ON [ai_generated_contents] ([related_topic_id]);

CREATE INDEX [IX_ai_generated_contents_requested_by] ON [ai_generated_contents] ([requested_by]);

CREATE INDEX [IX_ai_generated_contents_reviewed_by] ON [ai_generated_contents] ([reviewed_by]);

CREATE INDEX [IX_ai_prompt_templates_created_by] ON [ai_prompt_templates] ([created_by]);

CREATE UNIQUE INDEX [UQ__ai_promp__3BD932B213642BD3] ON [ai_prompt_templates] ([prompt_code]);

CREATE INDEX [IX_ai_replanning_events_learning_path_id] ON [ai_replanning_events] ([learning_path_id]);

CREATE INDEX [IX_ai_replanning_events_student_id] ON [ai_replanning_events] ([student_id]);

CREATE INDEX [IX_ai_tutor_conversations_learning_path_node_id] ON [ai_tutor_conversations] ([learning_path_node_id]);

CREATE INDEX [IX_ai_tutor_conversations_student_id] ON [ai_tutor_conversations] ([student_id]);

CREATE INDEX [IX_ai_tutor_conversations_topic_id] ON [ai_tutor_conversations] ([topic_id]);

CREATE INDEX [IX_ai_tutor_messages_conversation_id] ON [ai_tutor_messages] ([conversation_id]);

CREATE INDEX [IX_ai_usage_logs_prompt_template_id] ON [ai_usage_logs] ([prompt_template_id]);

CREATE INDEX [IX_ai_usage_logs_user_id] ON [ai_usage_logs] ([user_id]);

CREATE INDEX [IX_ai_usage_module_date] ON [ai_usage_logs] ([module_code], [created_at]);

CREATE INDEX [IX_audit_logs_user_id] ON [audit_logs] ([user_id]);

CREATE INDEX [IX_competency_analyses_current_level_id] ON [competency_analyses] ([current_level_id]);

CREATE INDEX [IX_competency_analyses_recommended_level_id] ON [competency_analyses] ([recommended_level_id]);

CREATE INDEX [IX_competency_analyses_student_id] ON [competency_analyses] ([student_id]);

CREATE INDEX [IX_competency_analyses_test_attempt_id] ON [competency_analyses] ([test_attempt_id]);

CREATE INDEX [IX_competency_skill_scores_competency_analysis_id] ON [competency_skill_scores] ([competency_analysis_id]);

CREATE INDEX [IX_competency_skill_scores_level_id] ON [competency_skill_scores] ([level_id]);

CREATE INDEX [IX_competency_skill_scores_skill_id] ON [competency_skill_scores] ([skill_id]);

CREATE INDEX [IX_content_compliance_reviews_reviewer_id] ON [content_compliance_reviews] ([reviewer_id]);

CREATE UNIQUE INDEX [UQ__english___357D4CF9AA0D901F] ON [english_proficiency_levels] ([code]);

CREATE UNIQUE INDEX [UQ__english___03ED21D844E200DF] ON [english_skills] ([skill_code]);

CREATE UNIQUE INDEX [UQ__learning__A2EA35BF031D9129] ON [learning_goals] ([goal_code]);

CREATE INDEX [IX_learning_objectives_topic_id] ON [learning_objectives] ([topic_id]);

CREATE INDEX [IX_learning_nodes_path_status] ON [learning_path_nodes] ([learning_path_id], [status], [order_index]);

CREATE INDEX [IX_learning_path_nodes_lesson_id] ON [learning_path_nodes] ([lesson_id]);

CREATE INDEX [IX_learning_path_nodes_practice_task_id] ON [learning_path_nodes] ([practice_task_id]);

CREATE INDEX [IX_learning_path_nodes_quiz_id] ON [learning_path_nodes] ([quiz_id]);

CREATE INDEX [IX_learning_path_nodes_topic_id] ON [learning_path_nodes] ([topic_id]);

CREATE INDEX [IX_learning_path_template_nodes_skill_id] ON [learning_path_template_nodes] ([skill_id]);

CREATE INDEX [IX_learning_path_template_nodes_template_id] ON [learning_path_template_nodes] ([template_id]);

CREATE INDEX [IX_learning_path_template_nodes_topic_id] ON [learning_path_template_nodes] ([topic_id]);

CREATE INDEX [IX_learning_path_templates_created_by] ON [learning_path_templates] ([created_by]);

CREATE INDEX [IX_learning_path_templates_goal_id] ON [learning_path_templates] ([goal_id]);

CREATE INDEX [IX_learning_path_templates_start_level_id] ON [learning_path_templates] ([start_level_id]);

CREATE INDEX [IX_learning_path_templates_target_level_id] ON [learning_path_templates] ([target_level_id]);

CREATE INDEX [IX_learning_topics_created_by] ON [learning_topics] ([created_by]);

CREATE INDEX [IX_learning_topics_level_id] ON [learning_topics] ([level_id]);

CREATE INDEX [IX_learning_topics_parent_topic_id] ON [learning_topics] ([parent_topic_id]);

CREATE INDEX [IX_topics_skill_level] ON [learning_topics] ([skill_id], [level_id]);

CREATE UNIQUE INDEX [UQ__learning__DDA414C51EC96AFC] ON [learning_topics] ([topic_code]) WHERE [topic_code] IS NOT NULL;

CREATE INDEX [IX_notification_reads_user_id] ON [notification_reads] ([user_id]);

CREATE UNIQUE INDEX [UQ_notification_read] ON [notification_reads] ([notification_id], [user_id]);

CREATE INDEX [IX_notifications_created_by] ON [notifications] ([created_by]);

CREATE INDEX [IX_notifications_target] ON [notifications] ([target_user_id], [created_at]);

CREATE INDEX [IX_original_lessons_approved_by] ON [original_lessons] ([approved_by]);

CREATE INDEX [IX_original_lessons_created_by] ON [original_lessons] ([created_by]);

CREATE INDEX [IX_original_lessons_topic_id] ON [original_lessons] ([topic_id]);

CREATE INDEX [IX_placement_test_questions_question_id] ON [placement_test_questions] ([question_id]);

CREATE UNIQUE INDEX [UQ_ptq] ON [placement_test_questions] ([section_id], [question_id]);

CREATE INDEX [IX_placement_test_sections_placement_test_id] ON [placement_test_sections] ([placement_test_id]);

CREATE INDEX [IX_placement_test_sections_skill_id] ON [placement_test_sections] ([skill_id]);

CREATE INDEX [IX_placement_tests_created_by] ON [placement_tests] ([created_by]);

CREATE INDEX [IX_placement_tests_target_level_id] ON [placement_tests] ([target_level_id]);

CREATE INDEX [IX_practice_submissions_practice_task_id] ON [practice_submissions] ([practice_task_id]);

CREATE INDEX [IX_practice_submissions_student_id] ON [practice_submissions] ([student_id]);

CREATE INDEX [IX_practice_tasks_created_by] ON [practice_tasks] ([created_by]);

CREATE INDEX [IX_practice_tasks_skill_id] ON [practice_tasks] ([skill_id]);

CREATE INDEX [IX_practice_tasks_topic_id] ON [practice_tasks] ([topic_id]);

CREATE INDEX [IX_question_bank_approved_by] ON [question_bank] ([approved_by]);

CREATE INDEX [IX_question_bank_created_by] ON [question_bank] ([created_by]);

CREATE INDEX [IX_question_bank_level_id] ON [question_bank] ([level_id]);

CREATE INDEX [IX_question_bank_topic_id] ON [question_bank] ([topic_id]);

CREATE INDEX [IX_questions_skill_topic] ON [question_bank] ([skill_id], [topic_id], [difficulty_level]);

CREATE INDEX [IX_question_options_question_id] ON [question_options] ([question_id]);

CREATE INDEX [IX_quiz_answers_question_id] ON [quiz_answers] ([question_id]);

CREATE INDEX [IX_quiz_answers_quiz_attempt_id] ON [quiz_answers] ([quiz_attempt_id]);

CREATE INDEX [IX_quiz_answers_selected_option_id] ON [quiz_answers] ([selected_option_id]);

CREATE INDEX [IX_quiz_attempt_student] ON [quiz_attempts] ([student_id], [submitted_at]);

CREATE INDEX [IX_quiz_attempts_quiz_id] ON [quiz_attempts] ([quiz_id]);

CREATE INDEX [IX_quiz_questions_question_id] ON [quiz_questions] ([question_id]);

CREATE UNIQUE INDEX [UQ_quiz_question] ON [quiz_questions] ([quiz_id], [question_id]);

CREATE INDEX [IX_quizzes_created_by] ON [quizzes] ([created_by]);

CREATE INDEX [IX_quizzes_skill_id] ON [quizzes] ([skill_id]);

CREATE INDEX [IX_quizzes_topic_id] ON [quizzes] ([topic_id]);

CREATE INDEX [IX_reference_sources_approved_by] ON [reference_sources] ([approved_by]);

CREATE INDEX [IX_reference_sources_created_by] ON [reference_sources] ([created_by]);

CREATE INDEX [IX_refresh_tokens_user_id] ON [refresh_tokens] ([user_id]);

CREATE INDEX [IX_report_snapshots_generated_by] ON [report_snapshots] ([generated_by]);

CREATE UNIQUE INDEX [UQ__roles__BAE630753730BBBE] ON [roles] ([role_code]);

CREATE INDEX [IX_search_logs_user_id] ON [search_logs] ([user_id]);

CREATE INDEX [IX_learning_paths_student] ON [student_learning_paths] ([student_id], [status]);

CREATE INDEX [IX_student_learning_paths_competency_analysis_id] ON [student_learning_paths] ([competency_analysis_id]);

CREATE INDEX [IX_student_learning_paths_goal_id] ON [student_learning_paths] ([goal_id]);

CREATE INDEX [IX_student_learning_paths_template_id] ON [student_learning_paths] ([template_id]);

CREATE INDEX [IX_student_learning_profiles_current_level_id] ON [student_learning_profiles] ([current_level_id]);

CREATE INDEX [IX_student_learning_profiles_main_goal_id] ON [student_learning_profiles] ([main_goal_id]);

CREATE INDEX [IX_student_learning_profiles_target_level_id] ON [student_learning_profiles] ([target_level_id]);

CREATE UNIQUE INDEX [UQ__student___B9BE370EE2CB829C] ON [student_learning_profiles] ([user_id]);

CREATE INDEX [IX_student_progress_snapshots_skill_id] ON [student_progress_snapshots] ([skill_id]);

CREATE INDEX [IX_student_progress_snapshots_student_id] ON [student_progress_snapshots] ([student_id]);

CREATE INDEX [IX_student_progress_snapshots_topic_id] ON [student_progress_snapshots] ([topic_id]);

CREATE UNIQUE INDEX [UQ_ssp_profile_skill] ON [student_skill_preferences] ([student_profile_id], [skill_code]);

CREATE INDEX [IX_activity_student_date] ON [study_activity_logs] ([student_id], [created_at]);

CREATE INDEX [IX_study_activity_logs_learning_path_node_id] ON [study_activity_logs] ([learning_path_node_id]);

CREATE INDEX [IX_study_activity_logs_topic_id] ON [study_activity_logs] ([topic_id]);

CREATE INDEX [IX_system_settings_updated_by] ON [system_settings] ([updated_by]);

CREATE UNIQUE INDEX [UQ__system_s__0DFAC42717432674] ON [system_settings] ([setting_key]);

CREATE INDEX [IX_test_answers_attempt_id] ON [test_answers] ([attempt_id]);

CREATE INDEX [IX_test_answers_question_id] ON [test_answers] ([question_id]);

CREATE INDEX [IX_test_answers_selected_option_id] ON [test_answers] ([selected_option_id]);

CREATE INDEX [IX_test_attempt_student] ON [test_attempts] ([student_id], [status]);

CREATE INDEX [IX_test_attempts_estimated_level_id] ON [test_attempts] ([estimated_level_id]);

CREATE INDEX [IX_test_attempts_placement_test_id] ON [test_attempts] ([placement_test_id]);

CREATE INDEX [IX_topic_references_reference_source_id] ON [topic_references] ([reference_source_id]);

CREATE UNIQUE INDEX [UQ_topic_reference] ON [topic_references] ([topic_id], [reference_source_id]);

CREATE UNIQUE INDEX [UQ__user_pro__B9BE370E7D6D2F0C] ON [user_profiles] ([user_id]);

CREATE INDEX [IX_user_sessions_user_id] ON [user_sessions] ([user_id]);

CREATE INDEX [IX_users_role_status] ON [users] ([role_id], [status]);

CREATE UNIQUE INDEX [UQ__users__AB6E6164B7147E19] ON [users] ([email]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260620033728_InitialCreate', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [users] ADD [failed_login_count] int NOT NULL DEFAULT 0;

ALTER TABLE [users] ADD [lockout_until] datetime2 NULL;

ALTER TABLE [user_sessions] ADD [revoked_at] datetime2 NULL;

CREATE TABLE [login_logs] (
    [id] int NOT NULL IDENTITY,
    [user_id] int NULL,
    [ip_address] nvarchar(50) NULL,
    [user_agent] nvarchar(255) NULL,
    [is_success] bit NOT NULL,
    [failure_reason] nvarchar(255) NULL,
    [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK_login_logs] PRIMARY KEY ([id]),
    CONSTRAINT [FK_login_logs_users] FOREIGN KEY ([user_id]) REFERENCES [users] ([id]) ON DELETE CASCADE
);

CREATE TABLE [password_reset_tokens] (
    [id] int NOT NULL IDENTITY,
    [user_id] int NOT NULL,
    [token_hash] nvarchar(255) NOT NULL,
    [expires_at] datetime2 NOT NULL,
    [used_at] datetime2 NULL,
    [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK_password_reset_tokens] PRIMARY KEY ([id]),
    CONSTRAINT [FK_password_reset_tokens_users] FOREIGN KEY ([user_id]) REFERENCES [users] ([id]) ON DELETE CASCADE
);

ALTER TABLE [users] ADD CONSTRAINT [CK_User_Status] CHECK ([status] IN ('ACTIVE', 'LOCKED', 'PENDING', 'DELETED'));

CREATE INDEX [IX_login_logs_user_id] ON [login_logs] ([user_id]);

CREATE INDEX [IX_password_reset_tokens_user_id] ON [password_reset_tokens] ([user_id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260620081504_UpdateM1AccountFeatures', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [user_profiles] DROP CONSTRAINT [FK_user_profiles_users];

ALTER TABLE [user_profiles] ADD [avatar_url] varchar(500) NULL;

ALTER TABLE [user_profiles] ADD [language] nvarchar(50) NULL DEFAULT N'vi-VN';

ALTER TABLE [user_profiles] ADD [notification_preferences] nvarchar(max) NULL;

ALTER TABLE [user_profiles] ADD [phone] varchar(20) NULL;

ALTER TABLE [user_profiles] ADD [timezone] nvarchar(50) NULL DEFAULT N'Asia/Ho_Chi_Minh';


                UPDATE up
                SET up.avatar_url = u.avatar_url,
                    up.phone = u.phone
                FROM user_profiles up
                INNER JOIN users u ON up.user_id = u.id;
            

DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[users]') AND [c].[name] = N'avatar_url');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [users] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [users] DROP COLUMN [avatar_url];

DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[users]') AND [c].[name] = N'phone');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [users] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [users] DROP COLUMN [phone];

ALTER TABLE [user_profiles] ADD CONSTRAINT [FK_user_profiles_users] FOREIGN KEY ([user_id]) REFERENCES [users] ([id]) ON DELETE CASCADE;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260622014205_UpdateUserProfileSchema', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [users] ADD [avatar_url] varchar(500) NULL;

ALTER TABLE [users] ADD [phone] varchar(20) NULL;

CREATE TABLE [user_avatar_histories] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [old_avatar_url] nvarchar(500) NULL,
    [new_avatar_url] nvarchar(500) NULL,
    [changed_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK_user_avatar_histories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_user_avatar_histories_users_UserId] FOREIGN KEY ([UserId]) REFERENCES [users] ([id]) ON DELETE CASCADE
);

CREATE TABLE [user_settings] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [language] nvarchar(50) NULL DEFAULT N'vi-VN',
    [timezone] nvarchar(50) NULL DEFAULT N'Asia/Ho_Chi_Minh',
    [email_notifications] bit NOT NULL DEFAULT CAST(1 AS bit),
    [study_reminder_enabled] bit NOT NULL DEFAULT CAST(1 AS bit),
    [theme] nvarchar(20) NULL DEFAULT N'light',
    CONSTRAINT [PK_user_settings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_user_settings_users_UserId] FOREIGN KEY ([UserId]) REFERENCES [users] ([id]) ON DELETE CASCADE
);


                -- Copy phone and avatar_url back to users
                UPDATE u
                SET u.avatar_url = up.avatar_url,
                    u.phone = up.phone
                FROM users u
                INNER JOIN user_profiles up ON u.id = up.user_id;

                -- Copy settings to user_settings
                INSERT INTO user_settings (UserId, language, timezone)
                SELECT user_id, language, timezone
                FROM user_profiles;
            

DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user_profiles]') AND [c].[name] = N'avatar_url');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [user_profiles] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [user_profiles] DROP COLUMN [avatar_url];

DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user_profiles]') AND [c].[name] = N'language');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [user_profiles] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [user_profiles] DROP COLUMN [language];

DECLARE @var4 nvarchar(max);
SELECT @var4 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user_profiles]') AND [c].[name] = N'notification_preferences');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [user_profiles] DROP CONSTRAINT ' + @var4 + ';');
ALTER TABLE [user_profiles] DROP COLUMN [notification_preferences];

DECLARE @var5 nvarchar(max);
SELECT @var5 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user_profiles]') AND [c].[name] = N'phone');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [user_profiles] DROP CONSTRAINT ' + @var5 + ';');
ALTER TABLE [user_profiles] DROP COLUMN [phone];

DECLARE @var6 nvarchar(max);
SELECT @var6 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user_profiles]') AND [c].[name] = N'timezone');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [user_profiles] DROP CONSTRAINT ' + @var6 + ';');
ALTER TABLE [user_profiles] DROP COLUMN [timezone];

UPDATE [users] SET [avatar_url] = NULL, [phone] = NULL
WHERE [id] = 1;
SELECT @@ROWCOUNT;


CREATE INDEX [IX_user_avatar_histories_UserId] ON [user_avatar_histories] ([UserId]);

CREATE UNIQUE INDEX [IX_user_settings_UserId] ON [user_settings] ([UserId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260622021009_ApplyM2Architecture', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [placement_test_questions] DROP CONSTRAINT [FK_ptq_question];

ALTER TABLE [placement_test_questions] DROP CONSTRAINT [FK_ptq_section];

ALTER TABLE [test_answers] DROP CONSTRAINT [FK_ta_attempt];

ALTER TABLE [test_answers] DROP CONSTRAINT [FK_ta_question];

ALTER TABLE [test_attempts] DROP CONSTRAINT [FK_attempt_student];

ALTER TABLE [test_attempts] DROP CONSTRAINT [FK_attempt_test];

DROP INDEX [IX_test_attempt_student] ON [test_attempts];

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

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260624031905_UpdatePlacementTestSchema', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [question_bank] ADD CONSTRAINT [CK_QuestionBank_DifficultyLevel] CHECK ([difficulty_level] IN ('BASIC', 'MEDIUM', 'ADVANCED'));

ALTER TABLE [question_bank] ADD CONSTRAINT [CK_QuestionBank_QuestionType] CHECK ([question_type] IN ('MCQ', 'TRUE_FALSE', 'SHORT_ANSWER', 'LISTENING'));

ALTER TABLE [practice_tasks] ADD CONSTRAINT [CK_PracticeTask_DifficultyLevel] CHECK ([difficulty_level] IN ('BASIC', 'MEDIUM', 'ADVANCED'));

ALTER TABLE [practice_tasks] ADD CONSTRAINT [CK_PracticeTask_TaskType] CHECK ([task_type] IN ('MCQ', 'TRUE_FALSE', 'SHORT_ANSWER', 'LISTENING'));

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260624034209_StandardizeQuestionData', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
EXEC sp_rename N'[english_proficiency_levels].[name]', N'level_name', 'COLUMN';

EXEC sp_rename N'[english_proficiency_levels].[code]', N'level_code', 'COLUMN';

DECLARE @var7 nvarchar(max);
SELECT @var7 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[learning_topics]') AND [c].[name] = N'difficulty_level');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [learning_topics] DROP CONSTRAINT ' + @var7 + ';');
ALTER TABLE [learning_topics] ADD DEFAULT 'BEGINNER' FOR [difficulty_level];

ALTER TABLE [learning_topics] ADD [updated_by] int NULL;

ALTER TABLE [learning_objectives] ADD [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime()));

ALTER TABLE [learning_objectives] ADD [created_by] int NULL;

ALTER TABLE [learning_objectives] ADD [updated_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime()));

ALTER TABLE [learning_objectives] ADD [updated_by] int NULL;

ALTER TABLE [english_skills] ADD [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime()));

ALTER TABLE [english_skills] ADD [created_by] int NULL;

ALTER TABLE [english_skills] ADD [updated_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime()));

ALTER TABLE [english_skills] ADD [updated_by] int NULL;

ALTER TABLE [english_proficiency_levels] ADD [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime()));

ALTER TABLE [english_proficiency_levels] ADD [created_by] int NULL;

ALTER TABLE [english_proficiency_levels] ADD [is_active] bit NOT NULL DEFAULT CAST(1 AS bit);

ALTER TABLE [english_proficiency_levels] ADD [updated_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime()));

ALTER TABLE [english_proficiency_levels] ADD [updated_by] int NULL;

CREATE TABLE [topic_prerequisites] (
    [id] int NOT NULL IDENTITY,
    [topic_id] int NOT NULL,
    [prerequisite_topic_id] int NOT NULL,
    [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK_topic_prerequisites] PRIMARY KEY ([id]),
    CONSTRAINT [FK_topic_prerequisites_prerequisite] FOREIGN KEY ([prerequisite_topic_id]) REFERENCES [learning_topics] ([id]),
    CONSTRAINT [FK_topic_prerequisites_topic] FOREIGN KEY ([topic_id]) REFERENCES [learning_topics] ([id])
);

CREATE INDEX [IX_learning_topics_updated_by] ON [learning_topics] ([updated_by]);

CREATE INDEX [IX_topics_order_index] ON [learning_topics] ([order_index]);

CREATE INDEX [IX_topics_status] ON [learning_topics] ([status]);

IF OBJECT_ID('CK_topics_difficulty', 'C') IS NOT NULL ALTER TABLE learning_topics DROP CONSTRAINT CK_topics_difficulty;

UPDATE learning_topics SET difficulty_level = 'BEGINNER' WHERE difficulty_level NOT IN ('BEGINNER', 'ELEMENTARY', 'INTERMEDIATE', 'UPPER_INTERMEDIATE', 'ADVANCED');

ALTER TABLE [learning_topics] ADD CONSTRAINT [CK_topic_difficulty_level] CHECK (difficulty_level IN ('BEGINNER', 'ELEMENTARY', 'INTERMEDIATE', 'UPPER_INTERMEDIATE', 'ADVANCED'));

ALTER TABLE [learning_topics] ADD CONSTRAINT [CK_topic_status] CHECK (status IN ('ACTIVE', 'INACTIVE', 'ARCHIVED'));

CREATE INDEX [IX_learning_objectives_created_by] ON [learning_objectives] ([created_by]);

CREATE INDEX [IX_learning_objectives_updated_by] ON [learning_objectives] ([updated_by]);

ALTER TABLE [learning_objectives] ADD CONSTRAINT [CK_objective_cognitive_level] CHECK (cognitive_level IN ('REMEMBER', 'UNDERSTAND', 'APPLY', 'ANALYZE', 'CREATE'));

CREATE INDEX [IX_english_skills_created_by] ON [english_skills] ([created_by]);

CREATE INDEX [IX_english_skills_updated_by] ON [english_skills] ([updated_by]);

CREATE INDEX [IX_english_proficiency_levels_created_by] ON [english_proficiency_levels] ([created_by]);

CREATE INDEX [IX_english_proficiency_levels_updated_by] ON [english_proficiency_levels] ([updated_by]);

CREATE INDEX [IX_topic_prerequisites_prerequisite_topic_id] ON [topic_prerequisites] ([prerequisite_topic_id]);

CREATE UNIQUE INDEX [UQ_topic_prerequisite] ON [topic_prerequisites] ([topic_id], [prerequisite_topic_id]);

ALTER TABLE [english_proficiency_levels] ADD CONSTRAINT [FK_levels_created_by] FOREIGN KEY ([created_by]) REFERENCES [users] ([id]);

ALTER TABLE [english_proficiency_levels] ADD CONSTRAINT [FK_levels_updated_by] FOREIGN KEY ([updated_by]) REFERENCES [users] ([id]);

ALTER TABLE [english_skills] ADD CONSTRAINT [FK_skills_created_by] FOREIGN KEY ([created_by]) REFERENCES [users] ([id]);

ALTER TABLE [english_skills] ADD CONSTRAINT [FK_skills_updated_by] FOREIGN KEY ([updated_by]) REFERENCES [users] ([id]);

ALTER TABLE [learning_objectives] ADD CONSTRAINT [FK_objectives_created_by] FOREIGN KEY ([created_by]) REFERENCES [users] ([id]);

ALTER TABLE [learning_objectives] ADD CONSTRAINT [FK_objectives_updated_by] FOREIGN KEY ([updated_by]) REFERENCES [users] ([id]);

ALTER TABLE [learning_topics] ADD CONSTRAINT [FK_topics_updated_by] FOREIGN KEY ([updated_by]) REFERENCES [users] ([id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260625023529_AddM4DatabaseStandardization', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [learning_goals] ADD [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime()));

ALTER TABLE [learning_goals] ADD [created_by] int NULL;

ALTER TABLE [learning_goals] ADD [order_index] int NOT NULL DEFAULT 0;

ALTER TABLE [learning_goals] ADD [updated_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime()));

ALTER TABLE [learning_goals] ADD [updated_by] int NULL;

CREATE TABLE [StudentAvailableStudySlots] (
    [Id] int NOT NULL IDENTITY,
    [StudentProfileId] int NOT NULL,
    [DayOfWeek] int NOT NULL,
    [StartTime] time NOT NULL,
    [EndTime] time NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_StudentAvailableStudySlots] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_StudentAvailableStudySlots_student_learning_profiles_StudentProfileId] FOREIGN KEY ([StudentProfileId]) REFERENCES [student_learning_profiles] ([id]) ON DELETE CASCADE
);

CREATE INDEX [IX_learning_goals_created_by] ON [learning_goals] ([created_by]);

CREATE INDEX [IX_learning_goals_updated_by] ON [learning_goals] ([updated_by]);

CREATE INDEX [IX_StudentAvailableStudySlots_StudentProfileId] ON [StudentAvailableStudySlots] ([StudentProfileId]);

ALTER TABLE [learning_goals] ADD CONSTRAINT [FK_goals_created_by] FOREIGN KEY ([created_by]) REFERENCES [users] ([id]);

ALTER TABLE [learning_goals] ADD CONSTRAINT [FK_goals_updated_by] FOREIGN KEY ([updated_by]) REFERENCES [users] ([id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260625033529_AddMissingColumnsForMasterData', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [reference_sources] ADD [is_active] bit NOT NULL DEFAULT CAST(1 AS bit);

ALTER TABLE [reference_sources] ADD [updated_at] datetime2 NULL;

ALTER TABLE [content_compliance_reviews] ADD [reference_source_id] int NULL;

CREATE INDEX [IX_topic_references_topic_id] ON [topic_references] ([topic_id]);

CREATE INDEX [IX_reference_sources_source_type] ON [reference_sources] ([source_type]);

CREATE INDEX [IX_reference_sources_status] ON [reference_sources] ([status]);

ALTER TABLE [reference_sources] ADD CONSTRAINT [CK_ref_source_type] CHECK ([source_type] IN ('OFFICIAL', 'REFERENCE_ONLY', 'OPEN_LICENSE', 'INTERNAL'));

ALTER TABLE [reference_sources] ADD CONSTRAINT [CK_ref_status] CHECK ([status] IN ('DRAFT', 'PENDING', 'APPROVED', 'REJECTED', 'ARCHIVED'));

ALTER TABLE [reference_sources] ADD CONSTRAINT [CK_ref_usage_policy] CHECK ([usage_policy] IS NULL OR [usage_policy] IN ('REFERENCE_ONLY', 'OPEN_USE', 'INTERNAL_ONLY'));

CREATE INDEX [IX_content_compliance_reviews_reference_source_id] ON [content_compliance_reviews] ([reference_source_id]);

CREATE INDEX [IX_content_compliance_reviews_review_status] ON [content_compliance_reviews] ([review_status]);

ALTER TABLE [content_compliance_reviews] ADD CONSTRAINT [CK_content_compliance_reviews_risk] CHECK ([plagiarism_risk] IS NULL OR [plagiarism_risk] IN ('LOW', 'MEDIUM', 'HIGH'));

ALTER TABLE [content_compliance_reviews] ADD CONSTRAINT [CK_content_compliance_reviews_status] CHECK ([review_status] IN ('APPROVED', 'REJECTED', 'NEEDS_REVISION'));

ALTER TABLE [content_compliance_reviews] ADD CONSTRAINT [FK_content_reviews_ref_source] FOREIGN KEY ([reference_source_id]) REFERENCES [reference_sources] ([id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260626090552_AddM5LegalReferenceStandardization', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [reference_sources] DROP CONSTRAINT [CK_ref_source_type];

ALTER TABLE [reference_sources] DROP CONSTRAINT [CK_ref_usage_policy];

DECLARE @var8 nvarchar(max);
SELECT @var8 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[reference_sources]') AND [c].[name] = N'usage_policy');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [reference_sources] DROP CONSTRAINT ' + @var8 + ';');
ALTER TABLE [reference_sources] ALTER COLUMN [usage_policy] varchar(50) NULL;

ALTER TABLE [reference_sources] ADD CONSTRAINT [CK_ref_source_type] CHECK ([source_type] IN ('OFFICIAL', 'OPEN_LICENSE', 'SELF_CREATED', 'TEACHER_CREATED', 'REFERENCE_ONLY'));

ALTER TABLE [reference_sources] ADD CONSTRAINT [CK_ref_usage_policy] CHECK ([usage_policy] IS NULL OR [usage_policy] IN ('REFERENCE_ONLY', 'OPEN_USE', 'INTERNAL_ONLY'));

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260626092802_UpdateM5SourceTypeCheckConstraint', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE INDEX [IX_sps_student_date] ON [student_progress_snapshots] ([student_id], [snapshot_date]);

CREATE INDEX [IX_sps_student_skill] ON [student_progress_snapshots] ([student_id], [skill_id]);

CREATE INDEX [IX_sps_student_topic] ON [student_progress_snapshots] ([student_id], [topic_id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260627062935_AddM16ProgressSnapshotIndexes', N'10.0.9');

COMMIT;
GO

