/* ============================================================
   AI STUDY ENGLISH - SQL SERVER DATABASE SCRIPT
   Version: 1.0
   Purpose: Schema for AI English Learning Platform
   Direction: Duolingo / ELSA / IELTS Prep mini + AI Tutor + Learning Path

   Main modules covered:
   M01 Account & Authorization
   M02 Student Learning Profile / Onboarding
   M03 English Skills / Levels / Topics
   M04 Legal Reference Materials
   M05 Placement Test
   M06 AI Competency Analysis
   M07 AI Learning Path
   M08 Duolingo-style Path Nodes
   M09 Quiz & Practice
   M10 AI Generated Quiz / Essay / Exercises
   M11 AI Tutor
   M12 Progress Tracking
   M13 AI Replanning
   M14 Notifications
   M15 Admin Management
   M16 Teacher / Content Moderation
   M17 Reports & Statistics
   M18 Copyright / Content Compliance
   M19 Internal Search
   M20 AI Prompt / Configuration

   IMPORTANT COPYRIGHT PRINCIPLE:
   - The system must NOT store copyrighted textbook/PDF content copied from third parties.
   - Only store metadata, source links, license notes, self-created content,
     teacher-created content, or AI-generated content after moderation.
   ============================================================ */

IF DB_ID(N'AIStudyEnglish') IS NOT NULL
BEGIN
    ALTER DATABASE AIStudyEnglish SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE AIStudyEnglish;
END
GO

CREATE DATABASE AIStudyEnglish;
GO

USE AIStudyEnglish;
GO

SET NOCOUNT ON;
GO

/* ============================================================
   M01 - ACCOUNT & AUTHORIZATION
   ============================================================ */

CREATE TABLE roles (
    id INT IDENTITY(1,1) PRIMARY KEY,
    role_code VARCHAR(50) NOT NULL UNIQUE,       -- ADMIN / TEACHER / STUDENT
    role_name NVARCHAR(100) NOT NULL,
    description NVARCHAR(500) NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

CREATE TABLE users (
    id INT IDENTITY(1,1) PRIMARY KEY,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(500) NOT NULL,
    full_name NVARCHAR(255) NOT NULL,
    role_id INT NOT NULL,
    status VARCHAR(30) NOT NULL DEFAULT 'ACTIVE',
    avatar_url VARCHAR(500) NULL,
    phone VARCHAR(20) NULL,
    last_login_at DATETIME2 NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    updated_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_users_roles FOREIGN KEY (role_id) REFERENCES roles(id),
    CONSTRAINT CK_users_status CHECK (status IN ('ACTIVE', 'LOCKED', 'PENDING', 'DELETED'))
);
GO

CREATE TABLE user_profiles (
    id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL UNIQUE,
    date_of_birth DATE NULL,
    gender VARCHAR(20) NULL,
    country NVARCHAR(100) NULL DEFAULT N'Việt Nam',
    bio NVARCHAR(MAX) NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    updated_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_user_profiles_users FOREIGN KEY (user_id) REFERENCES users(id),
    CONSTRAINT CK_user_profiles_gender CHECK (gender IS NULL OR gender IN ('MALE', 'FEMALE', 'OTHER'))
);
GO

CREATE TABLE refresh_tokens (
    id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    token_hash VARCHAR(500) NOT NULL,
    expires_at DATETIME2 NOT NULL,
    revoked_at DATETIME2 NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_refresh_tokens_users FOREIGN KEY (user_id) REFERENCES users(id)
);
GO

CREATE TABLE user_sessions (
    id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    session_token VARCHAR(500) NOT NULL,
    ip_address VARCHAR(45) NULL,
    user_agent NVARCHAR(500) NULL,
    device_type VARCHAR(50) NULL,
    last_activity_at DATETIME2 NULL,
    expires_at DATETIME2 NOT NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_user_sessions_users FOREIGN KEY (user_id) REFERENCES users(id)
);
GO

CREATE TABLE audit_logs (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NULL,
    action VARCHAR(100) NOT NULL,
    entity_name VARCHAR(100) NULL,
    entity_id INT NULL,
    old_value NVARCHAR(MAX) NULL,
    new_value NVARCHAR(MAX) NULL,
    ip_address VARCHAR(45) NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_audit_logs_users FOREIGN KEY (user_id) REFERENCES users(id)
);
GO

/* ============================================================
   M02 - STUDENT LEARNING PROFILE / ONBOARDING
   ============================================================ */

CREATE TABLE english_proficiency_levels (
    id INT IDENTITY(1,1) PRIMARY KEY,
    code VARCHAR(20) NOT NULL UNIQUE,            -- A1 / A2 / B1 / B2 / C1 / C2
    name NVARCHAR(100) NOT NULL,
    order_index INT NOT NULL,
    description NVARCHAR(1000) NULL
);
GO

CREATE TABLE learning_goals (
    id INT IDENTITY(1,1) PRIMARY KEY,
    goal_code VARCHAR(50) NOT NULL UNIQUE,       -- COMMUNICATION / IELTS / SCHOOL / VOCABULARY...
    goal_name NVARCHAR(255) NOT NULL,
    description NVARCHAR(1000) NULL,
    is_active BIT NOT NULL DEFAULT 1
);
GO

CREATE TABLE student_learning_profiles (
    id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL UNIQUE,
    current_level_id INT NULL,
    target_level_id INT NULL,
    main_goal_id INT NULL,
    target_score DECIMAL(5,2) NULL,              -- IELTS target score if applicable
    daily_study_minutes INT NULL,
    weekly_study_days INT NULL,
    preferred_study_time VARCHAR(50) NULL,       -- MORNING / AFTERNOON / EVENING / NIGHT
    learning_note NVARCHAR(MAX) NULL,
    onboarding_status VARCHAR(30) NOT NULL DEFAULT 'NOT_STARTED',
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    updated_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_slp_users FOREIGN KEY (user_id) REFERENCES users(id),
    CONSTRAINT FK_slp_current_level FOREIGN KEY (current_level_id) REFERENCES english_proficiency_levels(id),
    CONSTRAINT FK_slp_target_level FOREIGN KEY (target_level_id) REFERENCES english_proficiency_levels(id),
    CONSTRAINT FK_slp_goal FOREIGN KEY (main_goal_id) REFERENCES learning_goals(id),
    CONSTRAINT CK_slp_status CHECK (onboarding_status IN ('NOT_STARTED', 'IN_PROGRESS', 'COMPLETED'))
);
GO

CREATE TABLE student_skill_preferences (
    id INT IDENTITY(1,1) PRIMARY KEY,
    student_profile_id INT NOT NULL,
    skill_code VARCHAR(50) NOT NULL,             -- VOCABULARY / GRAMMAR / READING / LISTENING / SPEAKING / WRITING
    priority_level INT NOT NULL DEFAULT 1,       -- 1 high, 2 medium, 3 low
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_ssp_profile FOREIGN KEY (student_profile_id) REFERENCES student_learning_profiles(id),
    CONSTRAINT CK_ssp_priority CHECK (priority_level BETWEEN 1 AND 5),
    CONSTRAINT UQ_ssp_profile_skill UNIQUE (student_profile_id, skill_code)
);
GO

/* ============================================================
   M03 - ENGLISH SKILLS / TOPICS / LEARNING OBJECTIVES
   ============================================================ */

CREATE TABLE english_skills (
    id INT IDENTITY(1,1) PRIMARY KEY,
    skill_code VARCHAR(50) NOT NULL UNIQUE,      -- VOCABULARY, GRAMMAR, READING, LISTENING, SPEAKING, WRITING, PRONUNCIATION
    skill_name NVARCHAR(100) NOT NULL,
    description NVARCHAR(1000) NULL,
    order_index INT NOT NULL,
    is_active BIT NOT NULL DEFAULT 1
);
GO

CREATE TABLE learning_topics (
    id INT IDENTITY(1,1) PRIMARY KEY,
    skill_id INT NOT NULL,
    parent_topic_id INT NULL,
    level_id INT NULL,
    topic_code VARCHAR(100) NULL UNIQUE,
    title NVARCHAR(255) NOT NULL,
    description NVARCHAR(MAX) NULL,
    difficulty_level VARCHAR(30) NOT NULL DEFAULT 'BASIC',
    estimated_minutes INT NULL,
    order_index INT NOT NULL DEFAULT 1,
    status VARCHAR(30) NOT NULL DEFAULT 'ACTIVE',
    created_by INT NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    updated_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_topics_skills FOREIGN KEY (skill_id) REFERENCES english_skills(id),
    CONSTRAINT FK_topics_parent FOREIGN KEY (parent_topic_id) REFERENCES learning_topics(id),
    CONSTRAINT FK_topics_level FOREIGN KEY (level_id) REFERENCES english_proficiency_levels(id),
    CONSTRAINT FK_topics_users FOREIGN KEY (created_by) REFERENCES users(id),
    CONSTRAINT CK_topics_difficulty CHECK (difficulty_level IN ('BASIC', 'MEDIUM', 'ADVANCED')),
    CONSTRAINT CK_topics_status CHECK (status IN ('ACTIVE', 'INACTIVE', 'ARCHIVED'))
);
GO

CREATE TABLE learning_objectives (
    id INT IDENTITY(1,1) PRIMARY KEY,
    topic_id INT NOT NULL,
    objective_text NVARCHAR(MAX) NOT NULL,
    cognitive_level VARCHAR(50) NOT NULL DEFAULT 'UNDERSTAND', -- REMEMBER / UNDERSTAND / APPLY / ANALYZE
    order_index INT NOT NULL DEFAULT 1,

    CONSTRAINT FK_objectives_topics FOREIGN KEY (topic_id) REFERENCES learning_topics(id),
    CONSTRAINT CK_objectives_cognitive CHECK (cognitive_level IN ('REMEMBER', 'UNDERSTAND', 'APPLY', 'ANALYZE', 'CREATE'))
);
GO

CREATE TABLE original_lessons (
    id INT IDENTITY(1,1) PRIMARY KEY,
    topic_id INT NOT NULL,
    title NVARCHAR(255) NOT NULL,
    summary NVARCHAR(MAX) NULL,
    content NVARCHAR(MAX) NULL,                  -- only self-created / teacher-created / approved AI-generated content
    content_type VARCHAR(50) NOT NULL DEFAULT 'TEXT',
    estimated_minutes INT NULL,
    source_type VARCHAR(50) NOT NULL DEFAULT 'SELF_CREATED',
    review_status VARCHAR(50) NOT NULL DEFAULT 'DRAFT',
    is_ai_generated BIT NOT NULL DEFAULT 0,
    created_by INT NULL,
    approved_by INT NULL,
    approved_at DATETIME2 NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    updated_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_lessons_topics FOREIGN KEY (topic_id) REFERENCES learning_topics(id),
    CONSTRAINT FK_lessons_created_by FOREIGN KEY (created_by) REFERENCES users(id),
    CONSTRAINT FK_lessons_approved_by FOREIGN KEY (approved_by) REFERENCES users(id),
    CONSTRAINT CK_lessons_content_type CHECK (content_type IN ('TEXT', 'AUDIO_LINK', 'VIDEO_LINK', 'REFERENCE_LINK')),
    CONSTRAINT CK_lessons_source_type CHECK (source_type IN ('SELF_CREATED', 'TEACHER_CREATED', 'AI_GENERATED', 'OPEN_LICENSE', 'REFERENCE_ONLY')),
    CONSTRAINT CK_lessons_review_status CHECK (review_status IN ('DRAFT', 'PENDING', 'APPROVED', 'REJECTED', 'PUBLISHED', 'ARCHIVED'))
);
GO

/* ============================================================
   M04 / M18 - LEGAL REFERENCE MATERIALS & COPYRIGHT COMPLIANCE
   ============================================================ */

CREATE TABLE reference_sources (
    id INT IDENTITY(1,1) PRIMARY KEY,
    source_name NVARCHAR(255) NOT NULL,          -- Cambridge, British Council, VOA...
    source_url VARCHAR(1000) NULL,
    source_type VARCHAR(50) NOT NULL,            -- OFFICIAL / OPEN_LICENSE / REFERENCE_ONLY / SELF_CREATED
    license_note NVARCHAR(MAX) NULL,
    usage_policy NVARCHAR(MAX) NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'PENDING',
    created_by INT NULL,
    approved_by INT NULL,
    approved_at DATETIME2 NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_ref_created_by FOREIGN KEY (created_by) REFERENCES users(id),
    CONSTRAINT FK_ref_approved_by FOREIGN KEY (approved_by) REFERENCES users(id),
    CONSTRAINT CK_ref_source_type CHECK (source_type IN ('OFFICIAL', 'OPEN_LICENSE', 'REFERENCE_ONLY', 'SELF_CREATED', 'TEACHER_CREATED')),
    CONSTRAINT CK_ref_status CHECK (status IN ('PENDING', 'APPROVED', 'REJECTED', 'ARCHIVED'))
);
GO

CREATE TABLE topic_references (
    id INT IDENTITY(1,1) PRIMARY KEY,
    topic_id INT NOT NULL,
    reference_source_id INT NOT NULL,
    note NVARCHAR(1000) NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_topic_ref_topic FOREIGN KEY (topic_id) REFERENCES learning_topics(id),
    CONSTRAINT FK_topic_ref_source FOREIGN KEY (reference_source_id) REFERENCES reference_sources(id),
    CONSTRAINT UQ_topic_reference UNIQUE (topic_id, reference_source_id)
);
GO

CREATE TABLE content_compliance_reviews (
    id INT IDENTITY(1,1) PRIMARY KEY,
    content_type VARCHAR(50) NOT NULL,           -- LESSON / QUESTION / REFERENCE / AI_CONTENT
    content_id INT NOT NULL,
    reviewer_id INT NOT NULL,
    copyright_check BIT NOT NULL DEFAULT 0,
    plagiarism_risk VARCHAR(30) NULL,
    review_status VARCHAR(50) NOT NULL,
    review_note NVARCHAR(MAX) NULL,
    reviewed_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_content_reviews_reviewer FOREIGN KEY (reviewer_id) REFERENCES users(id),
    CONSTRAINT CK_content_reviews_status CHECK (review_status IN ('APPROVED', 'REJECTED', 'NEEDS_REVISION')),
    CONSTRAINT CK_content_reviews_risk CHECK (plagiarism_risk IS NULL OR plagiarism_risk IN ('LOW', 'MEDIUM', 'HIGH'))
);
GO

/* ============================================================
   M05 - PLACEMENT TEST / ENTRANCE ASSESSMENT
   ============================================================ */

CREATE TABLE placement_tests (
    id INT IDENTITY(1,1) PRIMARY KEY,
    title NVARCHAR(255) NOT NULL,
    description NVARCHAR(MAX) NULL,
    target_level_id INT NULL,
    time_limit_minutes INT NULL,
    total_score DECIMAL(6,2) NOT NULL DEFAULT 100,
    status VARCHAR(50) NOT NULL DEFAULT 'DRAFT',
    created_by INT NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    updated_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_pt_level FOREIGN KEY (target_level_id) REFERENCES english_proficiency_levels(id),
    CONSTRAINT FK_pt_created_by FOREIGN KEY (created_by) REFERENCES users(id),
    CONSTRAINT CK_pt_status CHECK (status IN ('DRAFT', 'PUBLISHED', 'ARCHIVED'))
);
GO

CREATE TABLE placement_test_sections (
    id INT IDENTITY(1,1) PRIMARY KEY,
    placement_test_id INT NOT NULL,
    skill_id INT NOT NULL,
    section_name NVARCHAR(255) NOT NULL,
    instruction NVARCHAR(MAX) NULL,
    order_index INT NOT NULL DEFAULT 1,
    max_score DECIMAL(6,2) NOT NULL DEFAULT 0,

    CONSTRAINT FK_pts_test FOREIGN KEY (placement_test_id) REFERENCES placement_tests(id),
    CONSTRAINT FK_pts_skill FOREIGN KEY (skill_id) REFERENCES english_skills(id)
);
GO

/* ============================================================
   M09 / M10 - QUESTION BANK, QUIZ, PRACTICE
   ============================================================ */

CREATE TABLE question_bank (
    id INT IDENTITY(1,1) PRIMARY KEY,
    topic_id INT NULL,
    skill_id INT NOT NULL,
    level_id INT NULL,
    question_type VARCHAR(50) NOT NULL,          -- MCQ / TRUE_FALSE / SHORT_ANSWER / ESSAY / LISTENING / SPEAKING
    question_text NVARCHAR(MAX) NOT NULL,
    audio_url VARCHAR(1000) NULL,
    image_url VARCHAR(1000) NULL,
    correct_answer NVARCHAR(MAX) NULL,
    explanation NVARCHAR(MAX) NULL,
    difficulty_level VARCHAR(30) NOT NULL DEFAULT 'BASIC',
    source_type VARCHAR(50) NOT NULL DEFAULT 'SELF_CREATED',
    review_status VARCHAR(50) NOT NULL DEFAULT 'DRAFT',
    created_by INT NULL,
    approved_by INT NULL,
    approved_at DATETIME2 NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    updated_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_qb_topic FOREIGN KEY (topic_id) REFERENCES learning_topics(id),
    CONSTRAINT FK_qb_skill FOREIGN KEY (skill_id) REFERENCES english_skills(id),
    CONSTRAINT FK_qb_level FOREIGN KEY (level_id) REFERENCES english_proficiency_levels(id),
    CONSTRAINT FK_qb_created_by FOREIGN KEY (created_by) REFERENCES users(id),
    CONSTRAINT FK_qb_approved_by FOREIGN KEY (approved_by) REFERENCES users(id),
    CONSTRAINT CK_qb_type CHECK (question_type IN ('MCQ', 'TRUE_FALSE', 'SHORT_ANSWER', 'ESSAY', 'LISTENING', 'SPEAKING')),
    CONSTRAINT CK_qb_difficulty CHECK (difficulty_level IN ('BASIC', 'MEDIUM', 'ADVANCED')),
    CONSTRAINT CK_qb_source_type CHECK (source_type IN ('SELF_CREATED', 'TEACHER_CREATED', 'AI_GENERATED', 'OPEN_LICENSE', 'REFERENCE_ONLY')),
    CONSTRAINT CK_qb_review_status CHECK (review_status IN ('DRAFT', 'PENDING', 'APPROVED', 'REJECTED', 'PUBLISHED', 'ARCHIVED'))
);
GO

CREATE TABLE question_options (
    id INT IDENTITY(1,1) PRIMARY KEY,
    question_id INT NOT NULL,
    option_text NVARCHAR(MAX) NOT NULL,
    is_correct BIT NOT NULL DEFAULT 0,
    order_index INT NOT NULL DEFAULT 1,

    CONSTRAINT FK_options_question FOREIGN KEY (question_id) REFERENCES question_bank(id)
);
GO

CREATE TABLE placement_test_questions (
    id INT IDENTITY(1,1) PRIMARY KEY,
    section_id INT NOT NULL,
    question_id INT NOT NULL,
    points DECIMAL(6,2) NOT NULL DEFAULT 1,
    order_index INT NOT NULL DEFAULT 1,

    CONSTRAINT FK_ptq_section FOREIGN KEY (section_id) REFERENCES placement_test_sections(id),
    CONSTRAINT FK_ptq_question FOREIGN KEY (question_id) REFERENCES question_bank(id),
    CONSTRAINT UQ_ptq UNIQUE (section_id, question_id)
);
GO

CREATE TABLE test_attempts (
    id INT IDENTITY(1,1) PRIMARY KEY,
    placement_test_id INT NOT NULL,
    student_id INT NOT NULL,
    started_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    submitted_at DATETIME2 NULL,
    total_score DECIMAL(6,2) NULL,
    estimated_level_id INT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'IN_PROGRESS',

    CONSTRAINT FK_attempt_test FOREIGN KEY (placement_test_id) REFERENCES placement_tests(id),
    CONSTRAINT FK_attempt_student FOREIGN KEY (student_id) REFERENCES users(id),
    CONSTRAINT FK_attempt_level FOREIGN KEY (estimated_level_id) REFERENCES english_proficiency_levels(id),
    CONSTRAINT CK_attempt_status CHECK (status IN ('IN_PROGRESS', 'SUBMITTED', 'GRADED', 'CANCELLED'))
);
GO

CREATE TABLE test_answers (
    id INT IDENTITY(1,1) PRIMARY KEY,
    attempt_id INT NOT NULL,
    question_id INT NOT NULL,
    selected_option_id INT NULL,
    answer_text NVARCHAR(MAX) NULL,
    is_correct BIT NULL,
    score DECIMAL(6,2) NULL,
    answered_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_ta_attempt FOREIGN KEY (attempt_id) REFERENCES test_attempts(id),
    CONSTRAINT FK_ta_question FOREIGN KEY (question_id) REFERENCES question_bank(id),
    CONSTRAINT FK_ta_option FOREIGN KEY (selected_option_id) REFERENCES question_options(id)
);
GO

CREATE TABLE quizzes (
    id INT IDENTITY(1,1) PRIMARY KEY,
    topic_id INT NULL,
    skill_id INT NOT NULL,
    title NVARCHAR(255) NOT NULL,
    description NVARCHAR(MAX) NULL,
    quiz_type VARCHAR(50) NOT NULL DEFAULT 'PRACTICE',
    time_limit_minutes INT NULL,
    passing_score DECIMAL(6,2) NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'DRAFT',
    created_by INT NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_quiz_topic FOREIGN KEY (topic_id) REFERENCES learning_topics(id),
    CONSTRAINT FK_quiz_skill FOREIGN KEY (skill_id) REFERENCES english_skills(id),
    CONSTRAINT FK_quiz_created_by FOREIGN KEY (created_by) REFERENCES users(id),
    CONSTRAINT CK_quiz_type CHECK (quiz_type IN ('PRACTICE', 'REVIEW', 'PLACEMENT', 'AI_PERSONALIZED')),
    CONSTRAINT CK_quiz_status CHECK (status IN ('DRAFT', 'PUBLISHED', 'ARCHIVED'))
);
GO

CREATE TABLE quiz_questions (
    id INT IDENTITY(1,1) PRIMARY KEY,
    quiz_id INT NOT NULL,
    question_id INT NOT NULL,
    points DECIMAL(6,2) NOT NULL DEFAULT 1,
    order_index INT NOT NULL DEFAULT 1,

    CONSTRAINT FK_qq_quiz FOREIGN KEY (quiz_id) REFERENCES quizzes(id),
    CONSTRAINT FK_qq_question FOREIGN KEY (question_id) REFERENCES question_bank(id),
    CONSTRAINT UQ_quiz_question UNIQUE (quiz_id, question_id)
);
GO

CREATE TABLE quiz_attempts (
    id INT IDENTITY(1,1) PRIMARY KEY,
    quiz_id INT NOT NULL,
    student_id INT NOT NULL,
    started_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    submitted_at DATETIME2 NULL,
    score DECIMAL(6,2) NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'IN_PROGRESS',

    CONSTRAINT FK_qa_quiz FOREIGN KEY (quiz_id) REFERENCES quizzes(id),
    CONSTRAINT FK_qa_student FOREIGN KEY (student_id) REFERENCES users(id),
    CONSTRAINT CK_qa_status CHECK (status IN ('IN_PROGRESS', 'SUBMITTED', 'GRADED', 'CANCELLED'))
);
GO

CREATE TABLE quiz_answers (
    id INT IDENTITY(1,1) PRIMARY KEY,
    quiz_attempt_id INT NOT NULL,
    question_id INT NOT NULL,
    selected_option_id INT NULL,
    answer_text NVARCHAR(MAX) NULL,
    is_correct BIT NULL,
    score DECIMAL(6,2) NULL,
    ai_explanation NVARCHAR(MAX) NULL,
    answered_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_qans_attempt FOREIGN KEY (quiz_attempt_id) REFERENCES quiz_attempts(id),
    CONSTRAINT FK_qans_question FOREIGN KEY (question_id) REFERENCES question_bank(id),
    CONSTRAINT FK_qans_option FOREIGN KEY (selected_option_id) REFERENCES question_options(id)
);
GO

CREATE TABLE practice_tasks (
    id INT IDENTITY(1,1) PRIMARY KEY,
    topic_id INT NULL,
    skill_id INT NOT NULL,
    title NVARCHAR(255) NOT NULL,
    instruction NVARCHAR(MAX) NOT NULL,
    task_type VARCHAR(50) NOT NULL,              -- ESSAY / SPEAKING / READING / LISTENING / VOCABULARY / GRAMMAR
    difficulty_level VARCHAR(30) NOT NULL DEFAULT 'BASIC',
    status VARCHAR(50) NOT NULL DEFAULT 'PUBLISHED',
    created_by INT NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_practice_topic FOREIGN KEY (topic_id) REFERENCES learning_topics(id),
    CONSTRAINT FK_practice_skill FOREIGN KEY (skill_id) REFERENCES english_skills(id),
    CONSTRAINT FK_practice_created_by FOREIGN KEY (created_by) REFERENCES users(id),
    CONSTRAINT CK_practice_type CHECK (task_type IN ('ESSAY', 'SPEAKING', 'READING', 'LISTENING', 'VOCABULARY', 'GRAMMAR')),
    CONSTRAINT CK_practice_difficulty CHECK (difficulty_level IN ('BASIC', 'MEDIUM', 'ADVANCED')),
    CONSTRAINT CK_practice_status CHECK (status IN ('DRAFT', 'PENDING', 'PUBLISHED', 'ARCHIVED'))
);
GO

CREATE TABLE practice_submissions (
    id INT IDENTITY(1,1) PRIMARY KEY,
    practice_task_id INT NOT NULL,
    student_id INT NOT NULL,
    submission_text NVARCHAR(MAX) NULL,
    file_url VARCHAR(1000) NULL,
    audio_url VARCHAR(1000) NULL,
    submitted_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    score DECIMAL(6,2) NULL,
    ai_feedback NVARCHAR(MAX) NULL,
    teacher_feedback NVARCHAR(MAX) NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'SUBMITTED',

    CONSTRAINT FK_ps_task FOREIGN KEY (practice_task_id) REFERENCES practice_tasks(id),
    CONSTRAINT FK_ps_student FOREIGN KEY (student_id) REFERENCES users(id),
    CONSTRAINT CK_ps_status CHECK (status IN ('SUBMITTED', 'AI_REVIEWED', 'TEACHER_REVIEWED', 'RETURNED'))
);
GO

/* ============================================================
   M06 - AI COMPETENCY ANALYSIS
   ============================================================ */

CREATE TABLE competency_analyses (
    id INT IDENTITY(1,1) PRIMARY KEY,
    student_id INT NOT NULL,
    test_attempt_id INT NULL,
    summary NVARCHAR(MAX) NOT NULL,
    current_level_id INT NULL,
    recommended_level_id INT NULL,
    strengths NVARCHAR(MAX) NULL,
    weaknesses NVARCHAR(MAX) NULL,
    gap_analysis NVARCHAR(MAX) NULL,
    ai_model VARCHAR(100) NULL,
    confidence_score DECIMAL(5,2) NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_ca_student FOREIGN KEY (student_id) REFERENCES users(id),
    CONSTRAINT FK_ca_attempt FOREIGN KEY (test_attempt_id) REFERENCES test_attempts(id),
    CONSTRAINT FK_ca_current_level FOREIGN KEY (current_level_id) REFERENCES english_proficiency_levels(id),
    CONSTRAINT FK_ca_recommended_level FOREIGN KEY (recommended_level_id) REFERENCES english_proficiency_levels(id)
);
GO

CREATE TABLE competency_skill_scores (
    id INT IDENTITY(1,1) PRIMARY KEY,
    competency_analysis_id INT NOT NULL,
    skill_id INT NOT NULL,
    score DECIMAL(6,2) NOT NULL,
    level_id INT NULL,
    weakness_note NVARCHAR(MAX) NULL,
    priority_level INT NOT NULL DEFAULT 1,

    CONSTRAINT FK_css_analysis FOREIGN KEY (competency_analysis_id) REFERENCES competency_analyses(id),
    CONSTRAINT FK_css_skill FOREIGN KEY (skill_id) REFERENCES english_skills(id),
    CONSTRAINT FK_css_level FOREIGN KEY (level_id) REFERENCES english_proficiency_levels(id),
    CONSTRAINT CK_css_priority CHECK (priority_level BETWEEN 1 AND 5)
);
GO

/* ============================================================
   M07 / M08 - AI LEARNING PATH & DUOLINGO-STYLE PATH NODES
   ============================================================ */

CREATE TABLE learning_path_templates (
    id INT IDENTITY(1,1) PRIMARY KEY,
    template_name NVARCHAR(255) NOT NULL,
    goal_id INT NULL,
    start_level_id INT NULL,
    target_level_id INT NULL,
    duration_weeks INT NULL,
    description NVARCHAR(MAX) NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'DRAFT',
    created_by INT NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_lpt_goal FOREIGN KEY (goal_id) REFERENCES learning_goals(id),
    CONSTRAINT FK_lpt_start_level FOREIGN KEY (start_level_id) REFERENCES english_proficiency_levels(id),
    CONSTRAINT FK_lpt_target_level FOREIGN KEY (target_level_id) REFERENCES english_proficiency_levels(id),
    CONSTRAINT FK_lpt_created_by FOREIGN KEY (created_by) REFERENCES users(id),
    CONSTRAINT CK_lpt_status CHECK (status IN ('DRAFT', 'PUBLISHED', 'ARCHIVED'))
);
GO

CREATE TABLE learning_path_template_nodes (
    id INT IDENTITY(1,1) PRIMARY KEY,
    template_id INT NOT NULL,
    topic_id INT NULL,
    skill_id INT NULL,
    node_title NVARCHAR(255) NOT NULL,
    node_type VARCHAR(50) NOT NULL,              -- LEARN / QUIZ / PRACTICE / REVIEW / TEST / AI_TUTOR
    estimated_minutes INT NULL,
    order_index INT NOT NULL,
    unlock_condition NVARCHAR(MAX) NULL,

    CONSTRAINT FK_lptn_template FOREIGN KEY (template_id) REFERENCES learning_path_templates(id),
    CONSTRAINT FK_lptn_topic FOREIGN KEY (topic_id) REFERENCES learning_topics(id),
    CONSTRAINT FK_lptn_skill FOREIGN KEY (skill_id) REFERENCES english_skills(id),
    CONSTRAINT CK_lptn_type CHECK (node_type IN ('LEARN', 'QUIZ', 'PRACTICE', 'REVIEW', 'TEST', 'AI_TUTOR'))
);
GO

CREATE TABLE student_learning_paths (
    id INT IDENTITY(1,1) PRIMARY KEY,
    student_id INT NOT NULL,
    template_id INT NULL,
    competency_analysis_id INT NULL,
    title NVARCHAR(255) NOT NULL,
    description NVARCHAR(MAX) NULL,
    goal_id INT NULL,
    start_date DATE NULL,
    target_end_date DATE NULL,
    ai_plan_summary NVARCHAR(MAX) NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'ACTIVE',
    generated_by_ai BIT NOT NULL DEFAULT 1,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    updated_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_slpath_student FOREIGN KEY (student_id) REFERENCES users(id),
    CONSTRAINT FK_slpath_template FOREIGN KEY (template_id) REFERENCES learning_path_templates(id),
    CONSTRAINT FK_slpath_analysis FOREIGN KEY (competency_analysis_id) REFERENCES competency_analyses(id),
    CONSTRAINT FK_slpath_goal FOREIGN KEY (goal_id) REFERENCES learning_goals(id),
    CONSTRAINT CK_slpath_status CHECK (status IN ('ACTIVE', 'COMPLETED', 'PAUSED', 'ARCHIVED'))
);
GO

CREATE TABLE learning_path_nodes (
    id INT IDENTITY(1,1) PRIMARY KEY,
    learning_path_id INT NOT NULL,
    topic_id INT NULL,
    lesson_id INT NULL,
    quiz_id INT NULL,
    practice_task_id INT NULL,
    node_title NVARCHAR(255) NOT NULL,
    node_description NVARCHAR(MAX) NULL,
    node_type VARCHAR(50) NOT NULL,
    path_phase VARCHAR(50) NULL,                 -- MONTH / WEEK / DAY / STAGE
    scheduled_date DATE NULL,
    estimated_minutes INT NULL,
    order_index INT NOT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'LOCKED',
    ai_reason NVARCHAR(MAX) NULL,
    completed_at DATETIME2 NULL,

    CONSTRAINT FK_lpn_path FOREIGN KEY (learning_path_id) REFERENCES student_learning_paths(id),
    CONSTRAINT FK_lpn_topic FOREIGN KEY (topic_id) REFERENCES learning_topics(id),
    CONSTRAINT FK_lpn_lesson FOREIGN KEY (lesson_id) REFERENCES original_lessons(id),
    CONSTRAINT FK_lpn_quiz FOREIGN KEY (quiz_id) REFERENCES quizzes(id),
    CONSTRAINT FK_lpn_task FOREIGN KEY (practice_task_id) REFERENCES practice_tasks(id),
    CONSTRAINT CK_lpn_type CHECK (node_type IN ('LEARN', 'QUIZ', 'PRACTICE', 'REVIEW', 'TEST', 'AI_TUTOR')),
    CONSTRAINT CK_lpn_status CHECK (status IN ('LOCKED', 'AVAILABLE', 'IN_PROGRESS', 'COMPLETED', 'NEED_REVIEW', 'SKIPPED'))
);
GO

/* ============================================================
   M11 - AI TUTOR
   ============================================================ */

CREATE TABLE ai_tutor_conversations (
    id INT IDENTITY(1,1) PRIMARY KEY,
    student_id INT NOT NULL,
    topic_id INT NULL,
    learning_path_node_id INT NULL,
    title NVARCHAR(255) NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'ACTIVE',
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    updated_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_conv_student FOREIGN KEY (student_id) REFERENCES users(id),
    CONSTRAINT FK_conv_topic FOREIGN KEY (topic_id) REFERENCES learning_topics(id),
    CONSTRAINT FK_conv_node FOREIGN KEY (learning_path_node_id) REFERENCES learning_path_nodes(id),
    CONSTRAINT CK_conv_status CHECK (status IN ('ACTIVE', 'CLOSED', 'ARCHIVED'))
);
GO

CREATE TABLE ai_tutor_messages (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    conversation_id INT NOT NULL,
    sender_type VARCHAR(20) NOT NULL,            -- STUDENT / AI / TEACHER / SYSTEM
    message_text NVARCHAR(MAX) NOT NULL,
    ai_model VARCHAR(100) NULL,
    token_usage INT NULL,
    safety_flag VARCHAR(50) NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_msg_conversation FOREIGN KEY (conversation_id) REFERENCES ai_tutor_conversations(id),
    CONSTRAINT CK_msg_sender CHECK (sender_type IN ('STUDENT', 'AI', 'TEACHER', 'SYSTEM')),
    CONSTRAINT CK_msg_safety CHECK (safety_flag IS NULL OR safety_flag IN ('NORMAL', 'NEEDS_REVIEW', 'BLOCKED'))
);
GO

/* ============================================================
   M10 / M16 - AI GENERATED CONTENT & MODERATION
   ============================================================ */

CREATE TABLE ai_generated_contents (
    id INT IDENTITY(1,1) PRIMARY KEY,
    requested_by INT NULL,
    content_type VARCHAR(50) NOT NULL,           -- QUESTION / QUIZ / LESSON / FEEDBACK / PATH / ESSAY_PROMPT
    related_topic_id INT NULL,
    prompt_text NVARCHAR(MAX) NULL,
    generated_content NVARCHAR(MAX) NOT NULL,
    ai_model VARCHAR(100) NULL,
    review_status VARCHAR(50) NOT NULL DEFAULT 'PENDING',
    reviewed_by INT NULL,
    review_note NVARCHAR(MAX) NULL,
    reviewed_at DATETIME2 NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_aig_requested_by FOREIGN KEY (requested_by) REFERENCES users(id),
    CONSTRAINT FK_aig_topic FOREIGN KEY (related_topic_id) REFERENCES learning_topics(id),
    CONSTRAINT FK_aig_reviewed_by FOREIGN KEY (reviewed_by) REFERENCES users(id),
    CONSTRAINT CK_aig_type CHECK (content_type IN ('QUESTION', 'QUIZ', 'LESSON', 'FEEDBACK', 'PATH', 'ESSAY_PROMPT', 'SPEAKING_PROMPT')),
    CONSTRAINT CK_aig_review CHECK (review_status IN ('PENDING', 'APPROVED', 'REJECTED', 'NEEDS_REVISION'))
);
GO

/* ============================================================
   M12 / M13 - PROGRESS TRACKING & AI REPLANNING
   ============================================================ */

CREATE TABLE study_activity_logs (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    student_id INT NOT NULL,
    activity_type VARCHAR(50) NOT NULL,          -- LOGIN / LEARN / QUIZ / PRACTICE / CHAT / REVIEW
    topic_id INT NULL,
    learning_path_node_id INT NULL,
    duration_minutes INT NULL,
    score DECIMAL(6,2) NULL,
    metadata NVARCHAR(MAX) NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_sal_student FOREIGN KEY (student_id) REFERENCES users(id),
    CONSTRAINT FK_sal_topic FOREIGN KEY (topic_id) REFERENCES learning_topics(id),
    CONSTRAINT FK_sal_node FOREIGN KEY (learning_path_node_id) REFERENCES learning_path_nodes(id),
    CONSTRAINT CK_sal_activity CHECK (activity_type IN ('LOGIN', 'LEARN', 'QUIZ', 'PRACTICE', 'CHAT', 'REVIEW', 'TEST'))
);
GO

CREATE TABLE student_progress_snapshots (
    id INT IDENTITY(1,1) PRIMARY KEY,
    student_id INT NOT NULL,
    skill_id INT NULL,
    topic_id INT NULL,
    progress_percent DECIMAL(5,2) NOT NULL DEFAULT 0,
    average_score DECIMAL(6,2) NULL,
    total_study_minutes INT NOT NULL DEFAULT 0,
    completed_nodes INT NOT NULL DEFAULT 0,
    weak_points NVARCHAR(MAX) NULL,
    snapshot_date DATE NOT NULL DEFAULT CONVERT(DATE, SYSUTCDATETIME()),

    CONSTRAINT FK_sps_student FOREIGN KEY (student_id) REFERENCES users(id),
    CONSTRAINT FK_sps_skill FOREIGN KEY (skill_id) REFERENCES english_skills(id),
    CONSTRAINT FK_sps_topic FOREIGN KEY (topic_id) REFERENCES learning_topics(id),
    CONSTRAINT CK_sps_progress CHECK (progress_percent BETWEEN 0 AND 100)
);
GO

CREATE TABLE ai_feedbacks (
    id INT IDENTITY(1,1) PRIMARY KEY,
    student_id INT NOT NULL,
    quiz_attempt_id INT NULL,
    practice_submission_id INT NULL,
    feedback_type VARCHAR(50) NOT NULL,          -- QUIZ_RESULT / ESSAY / SPEAKING / GENERAL
    feedback_text NVARCHAR(MAX) NOT NULL,
    mistake_analysis NVARCHAR(MAX) NULL,
    recommended_action NVARCHAR(MAX) NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_feedback_student FOREIGN KEY (student_id) REFERENCES users(id),
    CONSTRAINT FK_feedback_quiz_attempt FOREIGN KEY (quiz_attempt_id) REFERENCES quiz_attempts(id),
    CONSTRAINT FK_feedback_practice FOREIGN KEY (practice_submission_id) REFERENCES practice_submissions(id),
    CONSTRAINT CK_feedback_type CHECK (feedback_type IN ('QUIZ_RESULT', 'ESSAY', 'SPEAKING', 'GENERAL', 'PLACEMENT_TEST'))
);
GO

CREATE TABLE ai_replanning_events (
    id INT IDENTITY(1,1) PRIMARY KEY,
    student_id INT NOT NULL,
    learning_path_id INT NOT NULL,
    trigger_type VARCHAR(50) NOT NULL,           -- LOW_SCORE / INACTIVE / FAST_PROGRESS / MANUAL_REQUEST
    old_plan_summary NVARCHAR(MAX) NULL,
    new_plan_summary NVARCHAR(MAX) NULL,
    reason NVARCHAR(MAX) NOT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'APPLIED',
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_replan_student FOREIGN KEY (student_id) REFERENCES users(id),
    CONSTRAINT FK_replan_path FOREIGN KEY (learning_path_id) REFERENCES student_learning_paths(id),
    CONSTRAINT CK_replan_trigger CHECK (trigger_type IN ('LOW_SCORE', 'INACTIVE', 'FAST_PROGRESS', 'MANUAL_REQUEST', 'AI_RECOMMENDATION')),
    CONSTRAINT CK_replan_status CHECK (status IN ('SUGGESTED', 'APPLIED', 'REJECTED'))
);
GO

/* ============================================================
   M14 - NOTIFICATIONS
   ============================================================ */

CREATE TABLE notifications (
    id INT IDENTITY(1,1) PRIMARY KEY,
    title NVARCHAR(255) NOT NULL,
    content NVARCHAR(MAX) NOT NULL,
    notification_type VARCHAR(50) NOT NULL,      -- STUDY_REMINDER / PATH_UPDATED / QUIZ_PENDING...
    target_user_id INT NULL,
    created_by INT NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_notifications_target FOREIGN KEY (target_user_id) REFERENCES users(id),
    CONSTRAINT FK_notifications_created_by FOREIGN KEY (created_by) REFERENCES users(id),
    CONSTRAINT CK_notifications_type CHECK (notification_type IN ('STUDY_REMINDER', 'PATH_UPDATED', 'QUIZ_PENDING', 'CONTENT_APPROVED', 'SYSTEM', 'AI_FEEDBACK'))
);
GO

CREATE TABLE notification_reads (
    id INT IDENTITY(1,1) PRIMARY KEY,
    notification_id INT NOT NULL,
    user_id INT NOT NULL,
    read_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_nr_notification FOREIGN KEY (notification_id) REFERENCES notifications(id),
    CONSTRAINT FK_nr_user FOREIGN KEY (user_id) REFERENCES users(id),
    CONSTRAINT UQ_notification_read UNIQUE (notification_id, user_id)
);
GO

/* ============================================================
   M17 - REPORTS & STATISTICS
   ============================================================ */

CREATE TABLE report_snapshots (
    id INT IDENTITY(1,1) PRIMARY KEY,
    report_type VARCHAR(50) NOT NULL,            -- STUDENT_PROGRESS / QUIZ_STATS / AI_USAGE / CONTENT_STATUS
    report_date DATE NOT NULL DEFAULT CONVERT(DATE, SYSUTCDATETIME()),
    generated_by INT NULL,
    report_data NVARCHAR(MAX) NOT NULL,          -- JSON
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_report_generated_by FOREIGN KEY (generated_by) REFERENCES users(id),
    CONSTRAINT CK_report_type CHECK (report_type IN ('STUDENT_PROGRESS', 'QUIZ_STATS', 'AI_USAGE', 'CONTENT_STATUS', 'SYSTEM_OVERVIEW'))
);
GO

/* ============================================================
   M19 - INTERNAL SEARCH
   ============================================================ */

CREATE TABLE search_logs (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NULL,
    keyword NVARCHAR(255) NOT NULL,
    search_scope VARCHAR(50) NOT NULL DEFAULT 'ALL', -- TOPIC / LESSON / QUESTION / RESOURCE / PATH / ALL
    result_count INT NOT NULL DEFAULT 0,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_search_user FOREIGN KEY (user_id) REFERENCES users(id),
    CONSTRAINT CK_search_scope CHECK (search_scope IN ('TOPIC', 'LESSON', 'QUESTION', 'RESOURCE', 'PATH', 'ALL'))
);
GO

/* ============================================================
   M20 - AI PROMPT / CONFIGURATION
   ============================================================ */

CREATE TABLE ai_prompt_templates (
    id INT IDENTITY(1,1) PRIMARY KEY,
    prompt_code VARCHAR(100) NOT NULL UNIQUE,
    prompt_name NVARCHAR(255) NOT NULL,
    module_code VARCHAR(50) NOT NULL,            -- ASSESSMENT / LEARNING_PATH / QUIZ_GENERATION / FEEDBACK / TUTOR
    system_prompt NVARCHAR(MAX) NOT NULL,
    output_schema NVARCHAR(MAX) NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'ACTIVE',
    version_no INT NOT NULL DEFAULT 1,
    created_by INT NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_prompt_created_by FOREIGN KEY (created_by) REFERENCES users(id),
    CONSTRAINT CK_prompt_module CHECK (module_code IN ('ASSESSMENT', 'LEARNING_PATH', 'QUIZ_GENERATION', 'FEEDBACK', 'TUTOR', 'REPLANNING')),
    CONSTRAINT CK_prompt_status CHECK (status IN ('ACTIVE', 'INACTIVE', 'ARCHIVED'))
);
GO

CREATE TABLE ai_usage_logs (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NULL,
    module_code VARCHAR(50) NOT NULL,
    prompt_template_id INT NULL,
    ai_model VARCHAR(100) NULL,
    input_tokens INT NULL,
    output_tokens INT NULL,
    cost_estimate DECIMAL(18,6) NULL,
    request_status VARCHAR(50) NOT NULL DEFAULT 'SUCCESS',
    error_message NVARCHAR(MAX) NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_ai_usage_user FOREIGN KEY (user_id) REFERENCES users(id),
    CONSTRAINT FK_ai_usage_prompt FOREIGN KEY (prompt_template_id) REFERENCES ai_prompt_templates(id),
    CONSTRAINT CK_ai_usage_status CHECK (request_status IN ('SUCCESS', 'FAILED', 'BLOCKED'))
);
GO

CREATE TABLE system_settings (
    id INT IDENTITY(1,1) PRIMARY KEY,
    setting_key VARCHAR(100) NOT NULL UNIQUE,
    setting_value NVARCHAR(MAX) NULL,
    description NVARCHAR(500) NULL,
    updated_by INT NULL,
    updated_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_settings_updated_by FOREIGN KEY (updated_by) REFERENCES users(id)
);
GO

/* ============================================================
   INDEXES
   ============================================================ */

CREATE INDEX IX_users_role_status ON users(role_id, status);
CREATE INDEX IX_topics_skill_level ON learning_topics(skill_id, level_id);
CREATE INDEX IX_questions_skill_topic ON question_bank(skill_id, topic_id, difficulty_level);
CREATE INDEX IX_test_attempt_student ON test_attempts(student_id, status);
CREATE INDEX IX_quiz_attempt_student ON quiz_attempts(student_id, submitted_at);
CREATE INDEX IX_learning_paths_student ON student_learning_paths(student_id, status);
CREATE INDEX IX_learning_nodes_path_status ON learning_path_nodes(learning_path_id, status, order_index);
CREATE INDEX IX_activity_student_date ON study_activity_logs(student_id, created_at);
CREATE INDEX IX_notifications_target ON notifications(target_user_id, created_at);
CREATE INDEX IX_ai_usage_module_date ON ai_usage_logs(module_code, created_at);
GO

/* ============================================================
   SEED DATA
   ============================================================ */

INSERT INTO roles (role_code, role_name, description) VALUES
('ADMIN', N'Quản trị viên', N'Quản lý toàn bộ hệ thống'),
('TEACHER', N'Giáo viên / Kiểm duyệt viên', N'Tạo và kiểm duyệt nội dung học tập'),
('STUDENT', N'Học sinh', N'Sử dụng hệ thống học Tiếng Anh cá nhân hóa');
GO

INSERT INTO english_proficiency_levels (code, name, order_index, description) VALUES
('A1', N'Beginner', 1, N'Mới bắt đầu, nắm các mẫu câu và từ vựng cơ bản'),
('A2', N'Elementary', 2, N'Giao tiếp đơn giản trong tình huống quen thuộc'),
('B1', N'Intermediate', 3, N'Có thể hiểu và sử dụng tiếng Anh trong nhiều tình huống phổ biến'),
('B2', N'Upper Intermediate', 4, N'Có khả năng đọc hiểu và diễn đạt ý kiến tương đối độc lập'),
('C1', N'Advanced', 5, N'Sử dụng tiếng Anh linh hoạt trong học thuật và công việc'),
('C2', N'Proficient', 6, N'Sử dụng tiếng Anh gần như thành thạo');
GO

INSERT INTO learning_goals (goal_code, goal_name, description) VALUES
('COMMUNICATION', N'Cải thiện giao tiếp', N'Tập trung từ vựng, nghe, nói và phản xạ giao tiếp'),
('IELTS', N'Luyện thi IELTS', N'Tập trung Reading, Listening, Writing, Speaking theo mục tiêu điểm'),
('SCHOOL_ENGLISH', N'Học tốt Tiếng Anh trên lớp', N'Củng cố ngữ pháp, từ vựng, đọc hiểu và bài kiểm tra ở trường'),
('VOCABULARY', N'Mở rộng từ vựng', N'Tập trung ghi nhớ và sử dụng từ vựng theo chủ đề'),
('GRAMMAR', N'Củng cố ngữ pháp', N'Tập trung cấu trúc câu và bài tập ngữ pháp');
GO

INSERT INTO english_skills (skill_code, skill_name, description, order_index) VALUES
('VOCABULARY', N'Từ vựng', N'Học từ vựng theo chủ đề, nghĩa, ví dụ và cách dùng', 1),
('GRAMMAR', N'Ngữ pháp', N'Học cấu trúc câu, thì, mệnh đề, dạng bài ngữ pháp', 2),
('READING', N'Đọc hiểu', N'Luyện đọc hiểu đoạn văn, bài đọc học thuật và kỹ năng scan/skimming', 3),
('LISTENING', N'Nghe hiểu', N'Luyện nghe hội thoại, độc thoại, ghi chú và nhận diện keyword', 4),
('SPEAKING', N'Nói', N'Luyện phản xạ nói, phát âm, câu trả lời ngắn/dài', 5),
('WRITING', N'Viết', N'Luyện viết câu, đoạn văn, essay và phản hồi AI', 6),
('PRONUNCIATION', N'Phát âm', N'Luyện âm, trọng âm, nối âm, ngữ điệu theo hướng ELSA mini', 7);
GO

DECLARE @AdminRole INT = (SELECT id FROM roles WHERE role_code = 'ADMIN');
DECLARE @TeacherRole INT = (SELECT id FROM roles WHERE role_code = 'TEACHER');
DECLARE @StudentRole INT = (SELECT id FROM roles WHERE role_code = 'STUDENT');

INSERT INTO users (email, password_hash, full_name, role_id, status) VALUES
('admin@aistudyenglish.local', '$2a$12$sample_admin_hash_replace_in_real_app', N'Quản trị hệ thống', @AdminRole, 'ACTIVE'),
('teacher@aistudyenglish.local', '$2a$12$sample_teacher_hash_replace_in_real_app', N'Giáo viên kiểm duyệt', @TeacherRole, 'ACTIVE'),
('student@aistudyenglish.local', '$2a$12$sample_student_hash_replace_in_real_app', N'Nguyễn Văn A', @StudentRole, 'ACTIVE');
GO

DECLARE @GrammarSkill INT = (SELECT id FROM english_skills WHERE skill_code = 'GRAMMAR');
DECLARE @VocabularySkill INT = (SELECT id FROM english_skills WHERE skill_code = 'VOCABULARY');
DECLARE @ReadingSkill INT = (SELECT id FROM english_skills WHERE skill_code = 'READING');
DECLARE @ListeningSkill INT = (SELECT id FROM english_skills WHERE skill_code = 'LISTENING');
DECLARE @A1 INT = (SELECT id FROM english_proficiency_levels WHERE code = 'A1');
DECLARE @A2 INT = (SELECT id FROM english_proficiency_levels WHERE code = 'A2');
DECLARE @B1 INT = (SELECT id FROM english_proficiency_levels WHERE code = 'B1');
DECLARE @Teacher INT = (SELECT id FROM users WHERE email = 'teacher@aistudyenglish.local');

INSERT INTO learning_topics (skill_id, level_id, topic_code, title, description, difficulty_level, estimated_minutes, order_index, created_by) VALUES
(@VocabularySkill, @A1, 'VOCAB_A1_DAILY_ROUTINE', N'Từ vựng chủ đề Daily Routine', N'Học từ vựng về hoạt động hằng ngày', 'BASIC', 45, 1, @Teacher),
(@GrammarSkill, @A1, 'GRAMMAR_A1_PRESENT_SIMPLE', N'Thì hiện tại đơn', N'Cấu trúc, cách dùng và dấu hiệu nhận biết thì hiện tại đơn', 'BASIC', 60, 2, @Teacher),
(@ReadingSkill, @A2, 'READING_A2_SHORT_PASSAGE', N'Đọc hiểu đoạn văn ngắn', N'Luyện kỹ năng đọc hiểu đoạn văn ngắn và tìm thông tin chính', 'MEDIUM', 60, 3, @Teacher),
(@ListeningSkill, @A2, 'LISTENING_A2_KEYWORD', N'Nghe bắt từ khóa', N'Luyện nghe các đoạn hội thoại ngắn và xác định keyword', 'MEDIUM', 60, 4, @Teacher);
GO

DECLARE @PresentSimple INT = (SELECT id FROM learning_topics WHERE topic_code = 'GRAMMAR_A1_PRESENT_SIMPLE');

INSERT INTO learning_objectives (topic_id, objective_text, cognitive_level, order_index) VALUES
(@PresentSimple, N'Nhận biết cấu trúc khẳng định, phủ định và nghi vấn của thì hiện tại đơn.', 'REMEMBER', 1),
(@PresentSimple, N'Hiểu cách dùng thì hiện tại đơn trong thói quen và sự thật hiển nhiên.', 'UNDERSTAND', 2),
(@PresentSimple, N'Vận dụng thì hiện tại đơn để hoàn thành câu và viết câu đơn giản.', 'APPLY', 3);
GO

INSERT INTO reference_sources (source_name, source_url, source_type, license_note, usage_policy, status, created_by, approved_by, approved_at)
VALUES
(N'British Council LearnEnglish', 'https://learnenglish.britishcouncil.org/', 'REFERENCE_ONLY',
 N'Chỉ lưu link tham khảo, không sao chép nội dung.', N'Dùng làm nguồn tham khảo hợp pháp cho học viên.', 'APPROVED',
 (SELECT id FROM users WHERE email='admin@aistudyenglish.local'), (SELECT id FROM users WHERE email='admin@aistudyenglish.local'), SYSUTCDATETIME()),
(N'VOA Learning English', 'https://learningenglish.voanews.com/', 'REFERENCE_ONLY',
 N'Chỉ lưu link tham khảo, không sao chép nội dung.', N'Dùng làm nguồn luyện nghe/đọc tham khảo.', 'APPROVED',
 (SELECT id FROM users WHERE email='admin@aistudyenglish.local'), (SELECT id FROM users WHERE email='admin@aistudyenglish.local'), SYSUTCDATETIME());
GO

INSERT INTO ai_prompt_templates (prompt_code, prompt_name, module_code, system_prompt, output_schema, status, created_by)
VALUES
('ASSESSMENT_ENGLISH_V1', N'Prompt phân tích năng lực Tiếng Anh', 'ASSESSMENT',
 N'Bạn là chuyên gia đánh giá năng lực Tiếng Anh. Hãy phân tích kết quả test theo kỹ năng, điểm mạnh, điểm yếu, lỗ hổng và khuyến nghị học tập.',
 N'{ "summary": "...", "strengths": [], "weaknesses": [], "recommended_level": "...", "priority_topics": [] }',
 'ACTIVE', (SELECT id FROM users WHERE email='admin@aistudyenglish.local')),
('LEARNING_PATH_ENGLISH_V1', N'Prompt tạo lộ trình học Tiếng Anh', 'LEARNING_PATH',
 N'Bạn là AI Study Coach. Hãy tạo learning path theo tháng, tuần, ngày dựa trên mục tiêu, trình độ, thời gian rảnh và điểm yếu của học viên.',
 N'{ "monthly_plan": [], "weekly_plan": [], "daily_tasks": [] }',
 'ACTIVE', (SELECT id FROM users WHERE email='admin@aistudyenglish.local')),
('AI_TUTOR_ENGLISH_V1', N'Prompt AI Tutor Tiếng Anh', 'TUTOR',
 N'Bạn là AI Tutor tiếng Anh. Hãy giải thích dễ hiểu, hướng dẫn từng bước, không chỉ đưa đáp án trực tiếp nếu học viên cần học phương pháp.',
 N'{ "answer": "...", "explanation": "...", "next_step": "..." }',
 'ACTIVE', (SELECT id FROM users WHERE email='admin@aistudyenglish.local'));
GO

PRINT N'AIStudyEnglish database schema created successfully.';
GO
