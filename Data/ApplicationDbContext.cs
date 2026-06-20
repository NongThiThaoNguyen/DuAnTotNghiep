using System;
using System.Collections.Generic;
using DuAnTotNghiep.Models;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AiFeedback> AiFeedbacks { get; set; }

    public virtual DbSet<AiGeneratedContent> AiGeneratedContents { get; set; }

    public virtual DbSet<AiPromptTemplate> AiPromptTemplates { get; set; }

    public virtual DbSet<AiReplanningEvent> AiReplanningEvents { get; set; }

    public virtual DbSet<AiTutorConversation> AiTutorConversations { get; set; }

    public virtual DbSet<AiTutorMessage> AiTutorMessages { get; set; }

    public virtual DbSet<AiUsageLog> AiUsageLogs { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<CompetencyAnalysis> CompetencyAnalyses { get; set; }

    public virtual DbSet<CompetencySkillScore> CompetencySkillScores { get; set; }

    public virtual DbSet<ContentComplianceReview> ContentComplianceReviews { get; set; }

    public virtual DbSet<EnglishProficiencyLevel> EnglishProficiencyLevels { get; set; }

    public virtual DbSet<EnglishSkill> EnglishSkills { get; set; }

    public virtual DbSet<LearningGoal> LearningGoals { get; set; }

    public virtual DbSet<LearningObjective> LearningObjectives { get; set; }

    public virtual DbSet<LearningPathNode> LearningPathNodes { get; set; }

    public virtual DbSet<LearningPathTemplate> LearningPathTemplates { get; set; }

    public virtual DbSet<LearningPathTemplateNode> LearningPathTemplateNodes { get; set; }

    public virtual DbSet<LearningTopic> LearningTopics { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<NotificationRead> NotificationReads { get; set; }

    public virtual DbSet<OriginalLesson> OriginalLessons { get; set; }

    public virtual DbSet<PlacementTest> PlacementTests { get; set; }

    public virtual DbSet<PlacementTestQuestion> PlacementTestQuestions { get; set; }

    public virtual DbSet<PlacementTestSection> PlacementTestSections { get; set; }

    public virtual DbSet<PracticeSubmission> PracticeSubmissions { get; set; }

    public virtual DbSet<PracticeTask> PracticeTasks { get; set; }

    public virtual DbSet<QuestionBank> QuestionBanks { get; set; }

    public virtual DbSet<QuestionOption> QuestionOptions { get; set; }

    public virtual DbSet<Quiz> Quizzes { get; set; }

    public virtual DbSet<QuizAnswer> QuizAnswers { get; set; }

    public virtual DbSet<QuizAttempt> QuizAttempts { get; set; }

    public virtual DbSet<QuizQuestion> QuizQuestions { get; set; }

    public virtual DbSet<ReferenceSource> ReferenceSources { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<ReportSnapshot> ReportSnapshots { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SearchLog> SearchLogs { get; set; }

    public virtual DbSet<StudentLearningPath> StudentLearningPaths { get; set; }

    public virtual DbSet<StudentLearningProfile> StudentLearningProfiles { get; set; }

    public virtual DbSet<StudentProgressSnapshot> StudentProgressSnapshots { get; set; }

    public virtual DbSet<StudentSkillPreference> StudentSkillPreferences { get; set; }

    public virtual DbSet<StudyActivityLog> StudyActivityLogs { get; set; }

    public virtual DbSet<SystemSetting> SystemSettings { get; set; }

    public virtual DbSet<TestAnswer> TestAnswers { get; set; }

    public virtual DbSet<TestAttempt> TestAttempts { get; set; }

    public virtual DbSet<TopicReference> TopicReferences { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserProfile> UserProfiles { get; set; }

    public virtual DbSet<UserSession> UserSessions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Will be configured via Dependency Injection in Program.cs
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AiFeedback>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ai_feedb__3213E83F659EE2FA");

            entity.ToTable("ai_feedbacks");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.FeedbackText).HasColumnName("feedback_text");
            entity.Property(e => e.FeedbackType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("feedback_type");
            entity.Property(e => e.MistakeAnalysis).HasColumnName("mistake_analysis");
            entity.Property(e => e.PracticeSubmissionId).HasColumnName("practice_submission_id");
            entity.Property(e => e.QuizAttemptId).HasColumnName("quiz_attempt_id");
            entity.Property(e => e.RecommendedAction).HasColumnName("recommended_action");
            entity.Property(e => e.StudentId).HasColumnName("student_id");

            entity.HasOne(d => d.PracticeSubmission).WithMany(p => p.AiFeedbacks)
                .HasForeignKey(d => d.PracticeSubmissionId)
                .HasConstraintName("FK_feedback_practice");

            entity.HasOne(d => d.QuizAttempt).WithMany(p => p.AiFeedbacks)
                .HasForeignKey(d => d.QuizAttemptId)
                .HasConstraintName("FK_feedback_quiz_attempt");

            entity.HasOne(d => d.Student).WithMany(p => p.AiFeedbacks)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_feedback_student");
        });

        modelBuilder.Entity<AiGeneratedContent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ai_gener__3213E83F42F7F589");

            entity.ToTable("ai_generated_contents");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AiModel)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("ai_model");
            entity.Property(e => e.ContentType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("content_type");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.GeneratedContent).HasColumnName("generated_content");
            entity.Property(e => e.PromptText).HasColumnName("prompt_text");
            entity.Property(e => e.RelatedTopicId).HasColumnName("related_topic_id");
            entity.Property(e => e.RequestedBy).HasColumnName("requested_by");
            entity.Property(e => e.ReviewNote).HasColumnName("review_note");
            entity.Property(e => e.ReviewStatus)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("PENDING")
                .HasColumnName("review_status");
            entity.Property(e => e.ReviewedAt).HasColumnName("reviewed_at");
            entity.Property(e => e.ReviewedBy).HasColumnName("reviewed_by");

            entity.HasOne(d => d.RelatedTopic).WithMany(p => p.AiGeneratedContents)
                .HasForeignKey(d => d.RelatedTopicId)
                .HasConstraintName("FK_aig_topic");

            entity.HasOne(d => d.RequestedByNavigation).WithMany(p => p.AiGeneratedContentRequestedByNavigations)
                .HasForeignKey(d => d.RequestedBy)
                .HasConstraintName("FK_aig_requested_by");

            entity.HasOne(d => d.ReviewedByNavigation).WithMany(p => p.AiGeneratedContentReviewedByNavigations)
                .HasForeignKey(d => d.ReviewedBy)
                .HasConstraintName("FK_aig_reviewed_by");
        });

        modelBuilder.Entity<AiPromptTemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ai_promp__3213E83F34DDDEE9");

            entity.ToTable("ai_prompt_templates");

            entity.HasIndex(e => e.PromptCode, "UQ__ai_promp__3BD932B213642BD3").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.ModuleCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("module_code");
            entity.Property(e => e.OutputSchema).HasColumnName("output_schema");
            entity.Property(e => e.PromptCode)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("prompt_code");
            entity.Property(e => e.PromptName)
                .HasMaxLength(255)
                .HasColumnName("prompt_name");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("ACTIVE")
                .HasColumnName("status");
            entity.Property(e => e.SystemPrompt).HasColumnName("system_prompt");
            entity.Property(e => e.VersionNo)
                .HasDefaultValue(1)
                .HasColumnName("version_no");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.AiPromptTemplates)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_prompt_created_by");
        });

        modelBuilder.Entity<AiReplanningEvent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ai_repla__3213E83F1F00B02E");

            entity.ToTable("ai_replanning_events");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.LearningPathId).HasColumnName("learning_path_id");
            entity.Property(e => e.NewPlanSummary).HasColumnName("new_plan_summary");
            entity.Property(e => e.OldPlanSummary).HasColumnName("old_plan_summary");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("APPLIED")
                .HasColumnName("status");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.TriggerType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("trigger_type");

            entity.HasOne(d => d.LearningPath).WithMany(p => p.AiReplanningEvents)
                .HasForeignKey(d => d.LearningPathId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_replan_path");

            entity.HasOne(d => d.Student).WithMany(p => p.AiReplanningEvents)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_replan_student");
        });

        modelBuilder.Entity<AiTutorConversation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ai_tutor__3213E83F49FD9E0A");

            entity.ToTable("ai_tutor_conversations");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.LearningPathNodeId).HasColumnName("learning_path_node_id");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("ACTIVE")
                .HasColumnName("status");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.TopicId).HasColumnName("topic_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.LearningPathNode).WithMany(p => p.AiTutorConversations)
                .HasForeignKey(d => d.LearningPathNodeId)
                .HasConstraintName("FK_conv_node");

            entity.HasOne(d => d.Student).WithMany(p => p.AiTutorConversations)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_conv_student");

            entity.HasOne(d => d.Topic).WithMany(p => p.AiTutorConversations)
                .HasForeignKey(d => d.TopicId)
                .HasConstraintName("FK_conv_topic");
        });

        modelBuilder.Entity<AiTutorMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ai_tutor__3213E83FA2FBD9A7");

            entity.ToTable("ai_tutor_messages");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AiModel)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("ai_model");
            entity.Property(e => e.ConversationId).HasColumnName("conversation_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.MessageText).HasColumnName("message_text");
            entity.Property(e => e.SafetyFlag)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("safety_flag");
            entity.Property(e => e.SenderType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("sender_type");
            entity.Property(e => e.TokenUsage).HasColumnName("token_usage");

            entity.HasOne(d => d.Conversation).WithMany(p => p.AiTutorMessages)
                .HasForeignKey(d => d.ConversationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_msg_conversation");
        });

        modelBuilder.Entity<AiUsageLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ai_usage__3213E83F2E522233");

            entity.ToTable("ai_usage_logs");

            entity.HasIndex(e => new { e.ModuleCode, e.CreatedAt }, "IX_ai_usage_module_date");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AiModel)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("ai_model");
            entity.Property(e => e.CostEstimate)
                .HasColumnType("decimal(18, 6)")
                .HasColumnName("cost_estimate");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message");
            entity.Property(e => e.InputTokens).HasColumnName("input_tokens");
            entity.Property(e => e.ModuleCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("module_code");
            entity.Property(e => e.OutputTokens).HasColumnName("output_tokens");
            entity.Property(e => e.PromptTemplateId).HasColumnName("prompt_template_id");
            entity.Property(e => e.RequestStatus)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("SUCCESS")
                .HasColumnName("request_status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.PromptTemplate).WithMany(p => p.AiUsageLogs)
                .HasForeignKey(d => d.PromptTemplateId)
                .HasConstraintName("FK_ai_usage_prompt");

            entity.HasOne(d => d.User).WithMany(p => p.AiUsageLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_ai_usage_user");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__audit_lo__3213E83F13E45708");

            entity.ToTable("audit_logs");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Action)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("action");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.EntityName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("entity_name");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("ip_address");
            entity.Property(e => e.NewValue).HasColumnName("new_value");
            entity.Property(e => e.OldValue).HasColumnName("old_value");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_audit_logs_users");
        });

        modelBuilder.Entity<CompetencyAnalysis>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__competen__3213E83FBF5D890C");

            entity.ToTable("competency_analyses");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AiModel)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("ai_model");
            entity.Property(e => e.ConfidenceScore)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("confidence_score");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CurrentLevelId).HasColumnName("current_level_id");
            entity.Property(e => e.GapAnalysis).HasColumnName("gap_analysis");
            entity.Property(e => e.RecommendedLevelId).HasColumnName("recommended_level_id");
            entity.Property(e => e.Strengths).HasColumnName("strengths");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.Summary).HasColumnName("summary");
            entity.Property(e => e.TestAttemptId).HasColumnName("test_attempt_id");
            entity.Property(e => e.Weaknesses).HasColumnName("weaknesses");

            entity.HasOne(d => d.CurrentLevel).WithMany(p => p.CompetencyAnalysisCurrentLevels)
                .HasForeignKey(d => d.CurrentLevelId)
                .HasConstraintName("FK_ca_current_level");

            entity.HasOne(d => d.RecommendedLevel).WithMany(p => p.CompetencyAnalysisRecommendedLevels)
                .HasForeignKey(d => d.RecommendedLevelId)
                .HasConstraintName("FK_ca_recommended_level");

            entity.HasOne(d => d.Student).WithMany(p => p.CompetencyAnalyses)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ca_student");

            entity.HasOne(d => d.TestAttempt).WithMany(p => p.CompetencyAnalyses)
                .HasForeignKey(d => d.TestAttemptId)
                .HasConstraintName("FK_ca_attempt");
        });

        modelBuilder.Entity<CompetencySkillScore>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__competen__3213E83FC90ADCC0");

            entity.ToTable("competency_skill_scores");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CompetencyAnalysisId).HasColumnName("competency_analysis_id");
            entity.Property(e => e.LevelId).HasColumnName("level_id");
            entity.Property(e => e.PriorityLevel)
                .HasDefaultValue(1)
                .HasColumnName("priority_level");
            entity.Property(e => e.Score)
                .HasColumnType("decimal(6, 2)")
                .HasColumnName("score");
            entity.Property(e => e.SkillId).HasColumnName("skill_id");
            entity.Property(e => e.WeaknessNote).HasColumnName("weakness_note");

            entity.HasOne(d => d.CompetencyAnalysis).WithMany(p => p.CompetencySkillScores)
                .HasForeignKey(d => d.CompetencyAnalysisId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_css_analysis");

            entity.HasOne(d => d.Level).WithMany(p => p.CompetencySkillScores)
                .HasForeignKey(d => d.LevelId)
                .HasConstraintName("FK_css_level");

            entity.HasOne(d => d.Skill).WithMany(p => p.CompetencySkillScores)
                .HasForeignKey(d => d.SkillId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_css_skill");
        });

        modelBuilder.Entity<ContentComplianceReview>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__content___3213E83FABD8D38F");

            entity.ToTable("content_compliance_reviews");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ContentId).HasColumnName("content_id");
            entity.Property(e => e.ContentType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("content_type");
            entity.Property(e => e.CopyrightCheck).HasColumnName("copyright_check");
            entity.Property(e => e.PlagiarismRisk)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("plagiarism_risk");
            entity.Property(e => e.ReviewNote).HasColumnName("review_note");
            entity.Property(e => e.ReviewStatus)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("review_status");
            entity.Property(e => e.ReviewedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("reviewed_at");
            entity.Property(e => e.ReviewerId).HasColumnName("reviewer_id");

            entity.HasOne(d => d.Reviewer).WithMany(p => p.ContentComplianceReviews)
                .HasForeignKey(d => d.ReviewerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_content_reviews_reviewer");
        });

        modelBuilder.Entity<EnglishProficiencyLevel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__english___3213E83F203E1EC7");

            entity.ToTable("english_proficiency_levels");

            entity.HasIndex(e => e.Code, "UQ__english___357D4CF9AA0D901F").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("code");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.OrderIndex).HasColumnName("order_index");
        });

        modelBuilder.Entity<EnglishSkill>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__english___3213E83F663F25FF");

            entity.ToTable("english_skills");

            entity.HasIndex(e => e.SkillCode, "UQ__english___03ED21D844E200DF").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.OrderIndex).HasColumnName("order_index");
            entity.Property(e => e.SkillCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("skill_code");
            entity.Property(e => e.SkillName)
                .HasMaxLength(100)
                .HasColumnName("skill_name");
        });

        modelBuilder.Entity<LearningGoal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__learning__3213E83FD1E17598");

            entity.ToTable("learning_goals");

            entity.HasIndex(e => e.GoalCode, "UQ__learning__A2EA35BF031D9129").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.GoalCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("goal_code");
            entity.Property(e => e.GoalName)
                .HasMaxLength(255)
                .HasColumnName("goal_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
        });

        modelBuilder.Entity<LearningObjective>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__learning__3213E83F6239F152");

            entity.ToTable("learning_objectives");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CognitiveLevel)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("UNDERSTAND")
                .HasColumnName("cognitive_level");
            entity.Property(e => e.ObjectiveText).HasColumnName("objective_text");
            entity.Property(e => e.OrderIndex)
                .HasDefaultValue(1)
                .HasColumnName("order_index");
            entity.Property(e => e.TopicId).HasColumnName("topic_id");

            entity.HasOne(d => d.Topic).WithMany(p => p.LearningObjectives)
                .HasForeignKey(d => d.TopicId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_objectives_topics");
        });

        modelBuilder.Entity<LearningPathNode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__learning__3213E83FDB07DA35");

            entity.ToTable("learning_path_nodes");

            entity.HasIndex(e => new { e.LearningPathId, e.Status, e.OrderIndex }, "IX_learning_nodes_path_status");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AiReason).HasColumnName("ai_reason");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.EstimatedMinutes).HasColumnName("estimated_minutes");
            entity.Property(e => e.LearningPathId).HasColumnName("learning_path_id");
            entity.Property(e => e.LessonId).HasColumnName("lesson_id");
            entity.Property(e => e.NodeDescription).HasColumnName("node_description");
            entity.Property(e => e.NodeTitle)
                .HasMaxLength(255)
                .HasColumnName("node_title");
            entity.Property(e => e.NodeType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("node_type");
            entity.Property(e => e.OrderIndex).HasColumnName("order_index");
            entity.Property(e => e.PathPhase)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("path_phase");
            entity.Property(e => e.PracticeTaskId).HasColumnName("practice_task_id");
            entity.Property(e => e.QuizId).HasColumnName("quiz_id");
            entity.Property(e => e.ScheduledDate).HasColumnName("scheduled_date");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("LOCKED")
                .HasColumnName("status");
            entity.Property(e => e.TopicId).HasColumnName("topic_id");

            entity.HasOne(d => d.LearningPath).WithMany(p => p.LearningPathNodes)
                .HasForeignKey(d => d.LearningPathId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_lpn_path");

            entity.HasOne(d => d.Lesson).WithMany(p => p.LearningPathNodes)
                .HasForeignKey(d => d.LessonId)
                .HasConstraintName("FK_lpn_lesson");

            entity.HasOne(d => d.PracticeTask).WithMany(p => p.LearningPathNodes)
                .HasForeignKey(d => d.PracticeTaskId)
                .HasConstraintName("FK_lpn_task");

            entity.HasOne(d => d.Quiz).WithMany(p => p.LearningPathNodes)
                .HasForeignKey(d => d.QuizId)
                .HasConstraintName("FK_lpn_quiz");

            entity.HasOne(d => d.Topic).WithMany(p => p.LearningPathNodes)
                .HasForeignKey(d => d.TopicId)
                .HasConstraintName("FK_lpn_topic");
        });

        modelBuilder.Entity<LearningPathTemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__learning__3213E83F1B0089FE");

            entity.ToTable("learning_path_templates");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DurationWeeks).HasColumnName("duration_weeks");
            entity.Property(e => e.GoalId).HasColumnName("goal_id");
            entity.Property(e => e.StartLevelId).HasColumnName("start_level_id");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("DRAFT")
                .HasColumnName("status");
            entity.Property(e => e.TargetLevelId).HasColumnName("target_level_id");
            entity.Property(e => e.TemplateName)
                .HasMaxLength(255)
                .HasColumnName("template_name");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.LearningPathTemplates)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_lpt_created_by");

            entity.HasOne(d => d.Goal).WithMany(p => p.LearningPathTemplates)
                .HasForeignKey(d => d.GoalId)
                .HasConstraintName("FK_lpt_goal");

            entity.HasOne(d => d.StartLevel).WithMany(p => p.LearningPathTemplateStartLevels)
                .HasForeignKey(d => d.StartLevelId)
                .HasConstraintName("FK_lpt_start_level");

            entity.HasOne(d => d.TargetLevel).WithMany(p => p.LearningPathTemplateTargetLevels)
                .HasForeignKey(d => d.TargetLevelId)
                .HasConstraintName("FK_lpt_target_level");
        });

        modelBuilder.Entity<LearningPathTemplateNode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__learning__3213E83F13D5DDD4");

            entity.ToTable("learning_path_template_nodes");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EstimatedMinutes).HasColumnName("estimated_minutes");
            entity.Property(e => e.NodeTitle)
                .HasMaxLength(255)
                .HasColumnName("node_title");
            entity.Property(e => e.NodeType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("node_type");
            entity.Property(e => e.OrderIndex).HasColumnName("order_index");
            entity.Property(e => e.SkillId).HasColumnName("skill_id");
            entity.Property(e => e.TemplateId).HasColumnName("template_id");
            entity.Property(e => e.TopicId).HasColumnName("topic_id");
            entity.Property(e => e.UnlockCondition).HasColumnName("unlock_condition");

            entity.HasOne(d => d.Skill).WithMany(p => p.LearningPathTemplateNodes)
                .HasForeignKey(d => d.SkillId)
                .HasConstraintName("FK_lptn_skill");

            entity.HasOne(d => d.Template).WithMany(p => p.LearningPathTemplateNodes)
                .HasForeignKey(d => d.TemplateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_lptn_template");

            entity.HasOne(d => d.Topic).WithMany(p => p.LearningPathTemplateNodes)
                .HasForeignKey(d => d.TopicId)
                .HasConstraintName("FK_lptn_topic");
        });

        modelBuilder.Entity<LearningTopic>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__learning__3213E83F8C330B01");

            entity.ToTable("learning_topics");

            entity.HasIndex(e => new { e.SkillId, e.LevelId }, "IX_topics_skill_level");

            entity.HasIndex(e => e.TopicCode, "UQ__learning__DDA414C51EC96AFC").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DifficultyLevel)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("BASIC")
                .HasColumnName("difficulty_level");
            entity.Property(e => e.EstimatedMinutes).HasColumnName("estimated_minutes");
            entity.Property(e => e.LevelId).HasColumnName("level_id");
            entity.Property(e => e.OrderIndex)
                .HasDefaultValue(1)
                .HasColumnName("order_index");
            entity.Property(e => e.ParentTopicId).HasColumnName("parent_topic_id");
            entity.Property(e => e.SkillId).HasColumnName("skill_id");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("ACTIVE")
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.TopicCode)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("topic_code");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.LearningTopics)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_topics_users");

            entity.HasOne(d => d.Level).WithMany(p => p.LearningTopics)
                .HasForeignKey(d => d.LevelId)
                .HasConstraintName("FK_topics_level");

            entity.HasOne(d => d.ParentTopic).WithMany(p => p.InverseParentTopic)
                .HasForeignKey(d => d.ParentTopicId)
                .HasConstraintName("FK_topics_parent");

            entity.HasOne(d => d.Skill).WithMany(p => p.LearningTopics)
                .HasForeignKey(d => d.SkillId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_topics_skills");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__notifica__3213E83FE246EE88");

            entity.ToTable("notifications");

            entity.HasIndex(e => new { e.TargetUserId, e.CreatedAt }, "IX_notifications_target");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.NotificationType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("notification_type");
            entity.Property(e => e.TargetUserId).HasColumnName("target_user_id");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.NotificationCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_notifications_created_by");

            entity.HasOne(d => d.TargetUser).WithMany(p => p.NotificationTargetUsers)
                .HasForeignKey(d => d.TargetUserId)
                .HasConstraintName("FK_notifications_target");
        });

        modelBuilder.Entity<NotificationRead>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__notifica__3213E83FE8DFDE34");

            entity.ToTable("notification_reads");

            entity.HasIndex(e => new { e.NotificationId, e.UserId }, "UQ_notification_read").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.ReadAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("read_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Notification).WithMany(p => p.NotificationReads)
                .HasForeignKey(d => d.NotificationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_nr_notification");

            entity.HasOne(d => d.User).WithMany(p => p.NotificationReads)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_nr_user");
        });

        modelBuilder.Entity<OriginalLesson>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__original__3213E83FFCDBE3D5");

            entity.ToTable("original_lessons");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ApprovedAt).HasColumnName("approved_at");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.ContentType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("TEXT")
                .HasColumnName("content_type");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.EstimatedMinutes).HasColumnName("estimated_minutes");
            entity.Property(e => e.IsAiGenerated).HasColumnName("is_ai_generated");
            entity.Property(e => e.ReviewStatus)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("DRAFT")
                .HasColumnName("review_status");
            entity.Property(e => e.SourceType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("SELF_CREATED")
                .HasColumnName("source_type");
            entity.Property(e => e.Summary).HasColumnName("summary");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.TopicId).HasColumnName("topic_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.OriginalLessonApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("FK_lessons_approved_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.OriginalLessonCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_lessons_created_by");

            entity.HasOne(d => d.Topic).WithMany(p => p.OriginalLessons)
                .HasForeignKey(d => d.TopicId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_lessons_topics");
        });

        modelBuilder.Entity<PlacementTest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__placemen__3213E83F955AE2ED");

            entity.ToTable("placement_tests");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("DRAFT")
                .HasColumnName("status");
            entity.Property(e => e.TargetLevelId).HasColumnName("target_level_id");
            entity.Property(e => e.TimeLimitMinutes).HasColumnName("time_limit_minutes");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.TotalScore)
                .HasDefaultValue(100m)
                .HasColumnType("decimal(6, 2)")
                .HasColumnName("total_score");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.PlacementTests)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_pt_created_by");

            entity.HasOne(d => d.TargetLevel).WithMany(p => p.PlacementTests)
                .HasForeignKey(d => d.TargetLevelId)
                .HasConstraintName("FK_pt_level");
        });

        modelBuilder.Entity<PlacementTestQuestion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__placemen__3213E83FBEEA4D47");

            entity.ToTable("placement_test_questions");

            entity.HasIndex(e => new { e.SectionId, e.QuestionId }, "UQ_ptq").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OrderIndex)
                .HasDefaultValue(1)
                .HasColumnName("order_index");
            entity.Property(e => e.Points)
                .HasDefaultValue(1m)
                .HasColumnType("decimal(6, 2)")
                .HasColumnName("points");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");
            entity.Property(e => e.SectionId).HasColumnName("section_id");

            entity.HasOne(d => d.Question).WithMany(p => p.PlacementTestQuestions)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ptq_question");

            entity.HasOne(d => d.Section).WithMany(p => p.PlacementTestQuestions)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ptq_section");
        });

        modelBuilder.Entity<PlacementTestSection>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__placemen__3213E83FC01FC668");

            entity.ToTable("placement_test_sections");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Instruction).HasColumnName("instruction");
            entity.Property(e => e.MaxScore)
                .HasColumnType("decimal(6, 2)")
                .HasColumnName("max_score");
            entity.Property(e => e.OrderIndex)
                .HasDefaultValue(1)
                .HasColumnName("order_index");
            entity.Property(e => e.PlacementTestId).HasColumnName("placement_test_id");
            entity.Property(e => e.SectionName)
                .HasMaxLength(255)
                .HasColumnName("section_name");
            entity.Property(e => e.SkillId).HasColumnName("skill_id");

            entity.HasOne(d => d.PlacementTest).WithMany(p => p.PlacementTestSections)
                .HasForeignKey(d => d.PlacementTestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_pts_test");

            entity.HasOne(d => d.Skill).WithMany(p => p.PlacementTestSections)
                .HasForeignKey(d => d.SkillId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_pts_skill");
        });

        modelBuilder.Entity<PracticeSubmission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__practice__3213E83FFB71DA05");

            entity.ToTable("practice_submissions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AiFeedback).HasColumnName("ai_feedback");
            entity.Property(e => e.AudioUrl)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("audio_url");
            entity.Property(e => e.FileUrl)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("file_url");
            entity.Property(e => e.PracticeTaskId).HasColumnName("practice_task_id");
            entity.Property(e => e.Score)
                .HasColumnType("decimal(6, 2)")
                .HasColumnName("score");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("SUBMITTED")
                .HasColumnName("status");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.SubmissionText).HasColumnName("submission_text");
            entity.Property(e => e.SubmittedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("submitted_at");
            entity.Property(e => e.TeacherFeedback).HasColumnName("teacher_feedback");

            entity.HasOne(d => d.PracticeTask).WithMany(p => p.PracticeSubmissions)
                .HasForeignKey(d => d.PracticeTaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ps_task");

            entity.HasOne(d => d.Student).WithMany(p => p.PracticeSubmissions)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ps_student");
        });

        modelBuilder.Entity<PracticeTask>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__practice__3213E83F9C46F05B");

            entity.ToTable("practice_tasks");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DifficultyLevel)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("BASIC")
                .HasColumnName("difficulty_level");
            entity.Property(e => e.Instruction).HasColumnName("instruction");
            entity.Property(e => e.SkillId).HasColumnName("skill_id");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("PUBLISHED")
                .HasColumnName("status");
            entity.Property(e => e.TaskType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("task_type");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.TopicId).HasColumnName("topic_id");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.PracticeTasks)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_practice_created_by");

            entity.HasOne(d => d.Skill).WithMany(p => p.PracticeTasks)
                .HasForeignKey(d => d.SkillId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_practice_skill");

            entity.HasOne(d => d.Topic).WithMany(p => p.PracticeTasks)
                .HasForeignKey(d => d.TopicId)
                .HasConstraintName("FK_practice_topic");
        });

        modelBuilder.Entity<QuestionBank>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__question__3213E83F73ECF016");

            entity.ToTable("question_bank");

            entity.HasIndex(e => new { e.SkillId, e.TopicId, e.DifficultyLevel }, "IX_questions_skill_topic");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ApprovedAt).HasColumnName("approved_at");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.AudioUrl)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("audio_url");
            entity.Property(e => e.CorrectAnswer).HasColumnName("correct_answer");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DifficultyLevel)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("BASIC")
                .HasColumnName("difficulty_level");
            entity.Property(e => e.Explanation).HasColumnName("explanation");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("image_url");
            entity.Property(e => e.LevelId).HasColumnName("level_id");
            entity.Property(e => e.QuestionText).HasColumnName("question_text");
            entity.Property(e => e.QuestionType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("question_type");
            entity.Property(e => e.ReviewStatus)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("DRAFT")
                .HasColumnName("review_status");
            entity.Property(e => e.SkillId).HasColumnName("skill_id");
            entity.Property(e => e.SourceType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("SELF_CREATED")
                .HasColumnName("source_type");
            entity.Property(e => e.TopicId).HasColumnName("topic_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.QuestionBankApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("FK_qb_approved_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.QuestionBankCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_qb_created_by");

            entity.HasOne(d => d.Level).WithMany(p => p.QuestionBanks)
                .HasForeignKey(d => d.LevelId)
                .HasConstraintName("FK_qb_level");

            entity.HasOne(d => d.Skill).WithMany(p => p.QuestionBanks)
                .HasForeignKey(d => d.SkillId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_qb_skill");

            entity.HasOne(d => d.Topic).WithMany(p => p.QuestionBanks)
                .HasForeignKey(d => d.TopicId)
                .HasConstraintName("FK_qb_topic");
        });

        modelBuilder.Entity<QuestionOption>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__question__3213E83FC1CBAC30");

            entity.ToTable("question_options");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IsCorrect).HasColumnName("is_correct");
            entity.Property(e => e.OptionText).HasColumnName("option_text");
            entity.Property(e => e.OrderIndex)
                .HasDefaultValue(1)
                .HasColumnName("order_index");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");

            entity.HasOne(d => d.Question).WithMany(p => p.QuestionOptions)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_options_question");
        });

        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__quizzes__3213E83F0BD02077");

            entity.ToTable("quizzes");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.PassingScore)
                .HasColumnType("decimal(6, 2)")
                .HasColumnName("passing_score");
            entity.Property(e => e.QuizType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("PRACTICE")
                .HasColumnName("quiz_type");
            entity.Property(e => e.SkillId).HasColumnName("skill_id");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("DRAFT")
                .HasColumnName("status");
            entity.Property(e => e.TimeLimitMinutes).HasColumnName("time_limit_minutes");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.TopicId).HasColumnName("topic_id");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Quizzes)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_quiz_created_by");

            entity.HasOne(d => d.Skill).WithMany(p => p.Quizzes)
                .HasForeignKey(d => d.SkillId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_quiz_skill");

            entity.HasOne(d => d.Topic).WithMany(p => p.Quizzes)
                .HasForeignKey(d => d.TopicId)
                .HasConstraintName("FK_quiz_topic");
        });

        modelBuilder.Entity<QuizAnswer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__quiz_ans__3213E83F04A1567E");

            entity.ToTable("quiz_answers");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AiExplanation).HasColumnName("ai_explanation");
            entity.Property(e => e.AnswerText).HasColumnName("answer_text");
            entity.Property(e => e.AnsweredAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("answered_at");
            entity.Property(e => e.IsCorrect).HasColumnName("is_correct");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");
            entity.Property(e => e.QuizAttemptId).HasColumnName("quiz_attempt_id");
            entity.Property(e => e.Score)
                .HasColumnType("decimal(6, 2)")
                .HasColumnName("score");
            entity.Property(e => e.SelectedOptionId).HasColumnName("selected_option_id");

            entity.HasOne(d => d.Question).WithMany(p => p.QuizAnswers)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_qans_question");

            entity.HasOne(d => d.QuizAttempt).WithMany(p => p.QuizAnswers)
                .HasForeignKey(d => d.QuizAttemptId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_qans_attempt");

            entity.HasOne(d => d.SelectedOption).WithMany(p => p.QuizAnswers)
                .HasForeignKey(d => d.SelectedOptionId)
                .HasConstraintName("FK_qans_option");
        });

        modelBuilder.Entity<QuizAttempt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__quiz_att__3213E83F46B04B87");

            entity.ToTable("quiz_attempts");

            entity.HasIndex(e => new { e.StudentId, e.SubmittedAt }, "IX_quiz_attempt_student");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.QuizId).HasColumnName("quiz_id");
            entity.Property(e => e.Score)
                .HasColumnType("decimal(6, 2)")
                .HasColumnName("score");
            entity.Property(e => e.StartedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("started_at");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("IN_PROGRESS")
                .HasColumnName("status");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.SubmittedAt).HasColumnName("submitted_at");

            entity.HasOne(d => d.Quiz).WithMany(p => p.QuizAttempts)
                .HasForeignKey(d => d.QuizId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_qa_quiz");

            entity.HasOne(d => d.Student).WithMany(p => p.QuizAttempts)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_qa_student");
        });

        modelBuilder.Entity<QuizQuestion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__quiz_que__3213E83FAD1F10DF");

            entity.ToTable("quiz_questions");

            entity.HasIndex(e => new { e.QuizId, e.QuestionId }, "UQ_quiz_question").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OrderIndex)
                .HasDefaultValue(1)
                .HasColumnName("order_index");
            entity.Property(e => e.Points)
                .HasDefaultValue(1m)
                .HasColumnType("decimal(6, 2)")
                .HasColumnName("points");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");
            entity.Property(e => e.QuizId).HasColumnName("quiz_id");

            entity.HasOne(d => d.Question).WithMany(p => p.QuizQuestions)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_qq_question");

            entity.HasOne(d => d.Quiz).WithMany(p => p.QuizQuestions)
                .HasForeignKey(d => d.QuizId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_qq_quiz");
        });

        modelBuilder.Entity<ReferenceSource>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__referenc__3213E83F85C4EC5F");

            entity.ToTable("reference_sources");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ApprovedAt).HasColumnName("approved_at");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.LicenseNote).HasColumnName("license_note");
            entity.Property(e => e.SourceName)
                .HasMaxLength(255)
                .HasColumnName("source_name");
            entity.Property(e => e.SourceType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("source_type");
            entity.Property(e => e.SourceUrl)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("source_url");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("PENDING")
                .HasColumnName("status");
            entity.Property(e => e.UsagePolicy).HasColumnName("usage_policy");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.ReferenceSourceApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("FK_ref_approved_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ReferenceSourceCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_ref_created_by");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__refresh___3213E83FF99585F1");

            entity.ToTable("refresh_tokens");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.RevokedAt).HasColumnName("revoked_at");
            entity.Property(e => e.TokenHash)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("token_hash");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_refresh_tokens_users");
        });

        modelBuilder.Entity<ReportSnapshot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__report_s__3213E83F404FE2DE");

            entity.ToTable("report_snapshots");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.GeneratedBy).HasColumnName("generated_by");
            entity.Property(e => e.ReportData).HasColumnName("report_data");
            entity.Property(e => e.ReportDate)
                .HasDefaultValueSql("(CONVERT([date],sysutcdatetime()))")
                .HasColumnName("report_date");
            entity.Property(e => e.ReportType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("report_type");

            entity.HasOne(d => d.GeneratedByNavigation).WithMany(p => p.ReportSnapshots)
                .HasForeignKey(d => d.GeneratedBy)
                .HasConstraintName("FK_report_generated_by");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__roles__3213E83FC530F3DF");

            entity.ToTable("roles");

            entity.HasIndex(e => e.RoleCode, "UQ__roles__BAE630753730BBBE").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.RoleCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("role_code");
            entity.Property(e => e.RoleName)
                .HasMaxLength(100)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<SearchLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__search_l__3213E83FBB7ECF99");

            entity.ToTable("search_logs");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Keyword)
                .HasMaxLength(255)
                .HasColumnName("keyword");
            entity.Property(e => e.ResultCount).HasColumnName("result_count");
            entity.Property(e => e.SearchScope)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("ALL")
                .HasColumnName("search_scope");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.SearchLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_search_user");
        });

        modelBuilder.Entity<StudentLearningPath>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__student___3213E83F4EC62BD7");

            entity.ToTable("student_learning_paths");

            entity.HasIndex(e => new { e.StudentId, e.Status }, "IX_learning_paths_student");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AiPlanSummary).HasColumnName("ai_plan_summary");
            entity.Property(e => e.CompetencyAnalysisId).HasColumnName("competency_analysis_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.GeneratedByAi)
                .HasDefaultValue(true)
                .HasColumnName("generated_by_ai");
            entity.Property(e => e.GoalId).HasColumnName("goal_id");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("ACTIVE")
                .HasColumnName("status");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.TargetEndDate).HasColumnName("target_end_date");
            entity.Property(e => e.TemplateId).HasColumnName("template_id");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.CompetencyAnalysis).WithMany(p => p.StudentLearningPaths)
                .HasForeignKey(d => d.CompetencyAnalysisId)
                .HasConstraintName("FK_slpath_analysis");

            entity.HasOne(d => d.Goal).WithMany(p => p.StudentLearningPaths)
                .HasForeignKey(d => d.GoalId)
                .HasConstraintName("FK_slpath_goal");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentLearningPaths)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_slpath_student");

            entity.HasOne(d => d.Template).WithMany(p => p.StudentLearningPaths)
                .HasForeignKey(d => d.TemplateId)
                .HasConstraintName("FK_slpath_template");
        });

        modelBuilder.Entity<StudentLearningProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__student___3213E83FB021A51B");

            entity.ToTable("student_learning_profiles");

            entity.HasIndex(e => e.UserId, "UQ__student___B9BE370EE2CB829C").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CurrentLevelId).HasColumnName("current_level_id");
            entity.Property(e => e.DailyStudyMinutes).HasColumnName("daily_study_minutes");
            entity.Property(e => e.LearningNote).HasColumnName("learning_note");
            entity.Property(e => e.MainGoalId).HasColumnName("main_goal_id");
            entity.Property(e => e.OnboardingStatus)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("NOT_STARTED")
                .HasColumnName("onboarding_status");
            entity.Property(e => e.PreferredStudyTime)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("preferred_study_time");
            entity.Property(e => e.TargetLevelId).HasColumnName("target_level_id");
            entity.Property(e => e.TargetScore)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("target_score");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.WeeklyStudyDays).HasColumnName("weekly_study_days");

            entity.HasOne(d => d.CurrentLevel).WithMany(p => p.StudentLearningProfileCurrentLevels)
                .HasForeignKey(d => d.CurrentLevelId)
                .HasConstraintName("FK_slp_current_level");

            entity.HasOne(d => d.MainGoal).WithMany(p => p.StudentLearningProfiles)
                .HasForeignKey(d => d.MainGoalId)
                .HasConstraintName("FK_slp_goal");

            entity.HasOne(d => d.TargetLevel).WithMany(p => p.StudentLearningProfileTargetLevels)
                .HasForeignKey(d => d.TargetLevelId)
                .HasConstraintName("FK_slp_target_level");

            entity.HasOne(d => d.User).WithOne(p => p.StudentLearningProfile)
                .HasForeignKey<StudentLearningProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_slp_users");
        });

        modelBuilder.Entity<StudentProgressSnapshot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__student___3213E83F6D6FC72B");

            entity.ToTable("student_progress_snapshots");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AverageScore)
                .HasColumnType("decimal(6, 2)")
                .HasColumnName("average_score");
            entity.Property(e => e.CompletedNodes).HasColumnName("completed_nodes");
            entity.Property(e => e.ProgressPercent)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("progress_percent");
            entity.Property(e => e.SkillId).HasColumnName("skill_id");
            entity.Property(e => e.SnapshotDate)
                .HasDefaultValueSql("(CONVERT([date],sysutcdatetime()))")
                .HasColumnName("snapshot_date");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.TopicId).HasColumnName("topic_id");
            entity.Property(e => e.TotalStudyMinutes).HasColumnName("total_study_minutes");
            entity.Property(e => e.WeakPoints).HasColumnName("weak_points");

            entity.HasOne(d => d.Skill).WithMany(p => p.StudentProgressSnapshots)
                .HasForeignKey(d => d.SkillId)
                .HasConstraintName("FK_sps_skill");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentProgressSnapshots)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_sps_student");

            entity.HasOne(d => d.Topic).WithMany(p => p.StudentProgressSnapshots)
                .HasForeignKey(d => d.TopicId)
                .HasConstraintName("FK_sps_topic");
        });

        modelBuilder.Entity<StudentSkillPreference>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__student___3213E83F3D22755E");

            entity.ToTable("student_skill_preferences");

            entity.HasIndex(e => new { e.StudentProfileId, e.SkillCode }, "UQ_ssp_profile_skill").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.PriorityLevel)
                .HasDefaultValue(1)
                .HasColumnName("priority_level");
            entity.Property(e => e.SkillCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("skill_code");
            entity.Property(e => e.StudentProfileId).HasColumnName("student_profile_id");

            entity.HasOne(d => d.StudentProfile).WithMany(p => p.StudentSkillPreferences)
                .HasForeignKey(d => d.StudentProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ssp_profile");
        });

        modelBuilder.Entity<StudyActivityLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__study_ac__3213E83F34CF263F");

            entity.ToTable("study_activity_logs");

            entity.HasIndex(e => new { e.StudentId, e.CreatedAt }, "IX_activity_student_date");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActivityType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("activity_type");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes");
            entity.Property(e => e.LearningPathNodeId).HasColumnName("learning_path_node_id");
            entity.Property(e => e.Metadata).HasColumnName("metadata");
            entity.Property(e => e.Score)
                .HasColumnType("decimal(6, 2)")
                .HasColumnName("score");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.TopicId).HasColumnName("topic_id");

            entity.HasOne(d => d.LearningPathNode).WithMany(p => p.StudyActivityLogs)
                .HasForeignKey(d => d.LearningPathNodeId)
                .HasConstraintName("FK_sal_node");

            entity.HasOne(d => d.Student).WithMany(p => p.StudyActivityLogs)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_sal_student");

            entity.HasOne(d => d.Topic).WithMany(p => p.StudyActivityLogs)
                .HasForeignKey(d => d.TopicId)
                .HasConstraintName("FK_sal_topic");
        });

        modelBuilder.Entity<SystemSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__system_s__3213E83FCC2454AB");

            entity.ToTable("system_settings");

            entity.HasIndex(e => e.SettingKey, "UQ__system_s__0DFAC42717432674").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.SettingKey)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("setting_key");
            entity.Property(e => e.SettingValue).HasColumnName("setting_value");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.SystemSettings)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK_settings_updated_by");
        });

        modelBuilder.Entity<TestAnswer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__test_ans__3213E83F49C84548");

            entity.ToTable("test_answers");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AnswerText).HasColumnName("answer_text");
            entity.Property(e => e.AnsweredAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("answered_at");
            entity.Property(e => e.AttemptId).HasColumnName("attempt_id");
            entity.Property(e => e.IsCorrect).HasColumnName("is_correct");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");
            entity.Property(e => e.Score)
                .HasColumnType("decimal(6, 2)")
                .HasColumnName("score");
            entity.Property(e => e.SelectedOptionId).HasColumnName("selected_option_id");

            entity.HasOne(d => d.Attempt).WithMany(p => p.TestAnswers)
                .HasForeignKey(d => d.AttemptId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ta_attempt");

            entity.HasOne(d => d.Question).WithMany(p => p.TestAnswers)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ta_question");

            entity.HasOne(d => d.SelectedOption).WithMany(p => p.TestAnswers)
                .HasForeignKey(d => d.SelectedOptionId)
                .HasConstraintName("FK_ta_option");
        });

        modelBuilder.Entity<TestAttempt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__test_att__3213E83FE21DABC0");

            entity.ToTable("test_attempts");

            entity.HasIndex(e => new { e.StudentId, e.Status }, "IX_test_attempt_student");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EstimatedLevelId).HasColumnName("estimated_level_id");
            entity.Property(e => e.PlacementTestId).HasColumnName("placement_test_id");
            entity.Property(e => e.StartedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("started_at");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("IN_PROGRESS")
                .HasColumnName("status");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.SubmittedAt).HasColumnName("submitted_at");
            entity.Property(e => e.TotalScore)
                .HasColumnType("decimal(6, 2)")
                .HasColumnName("total_score");

            entity.HasOne(d => d.EstimatedLevel).WithMany(p => p.TestAttempts)
                .HasForeignKey(d => d.EstimatedLevelId)
                .HasConstraintName("FK_attempt_level");

            entity.HasOne(d => d.PlacementTest).WithMany(p => p.TestAttempts)
                .HasForeignKey(d => d.PlacementTestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_attempt_test");

            entity.HasOne(d => d.Student).WithMany(p => p.TestAttempts)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_attempt_student");
        });

        modelBuilder.Entity<TopicReference>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__topic_re__3213E83FAB67747E");

            entity.ToTable("topic_references");

            entity.HasIndex(e => new { e.TopicId, e.ReferenceSourceId }, "UQ_topic_reference").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Note)
                .HasMaxLength(1000)
                .HasColumnName("note");
            entity.Property(e => e.ReferenceSourceId).HasColumnName("reference_source_id");
            entity.Property(e => e.TopicId).HasColumnName("topic_id");

            entity.HasOne(d => d.ReferenceSource).WithMany(p => p.TopicReferences)
                .HasForeignKey(d => d.ReferenceSourceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_topic_ref_source");

            entity.HasOne(d => d.Topic).WithMany(p => p.TopicReferences)
                .HasForeignKey(d => d.TopicId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_topic_ref_topic");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__users__3213E83F43CE90C9");

            entity.ToTable("users");

            entity.HasIndex(e => new { e.RoleId, e.Status }, "IX_users_role_status");

            entity.HasIndex(e => e.Email, "UQ__users__AB6E6164B7147E19").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("avatar_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.LastLoginAt).HasColumnName("last_login_at");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("ACTIVE")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_users_roles");
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__user_pro__3213E83FD33481F9");

            entity.ToTable("user_profiles");

            entity.HasIndex(e => e.UserId, "UQ__user_pro__B9BE370E7D6D2F0C").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Bio).HasColumnName("bio");
            entity.Property(e => e.Country)
                .HasMaxLength(100)
                .HasDefaultValue("Viá»‡t Nam")
                .HasColumnName("country");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.Gender)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("gender");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithOne(p => p.UserProfile)
                .HasForeignKey<UserProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_user_profiles_users");
        });

        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__user_ses__3213E83FA162BF6F");

            entity.ToTable("user_sessions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DeviceType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("device_type");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("ip_address");
            entity.Property(e => e.LastActivityAt).HasColumnName("last_activity_at");
            entity.Property(e => e.SessionToken)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("session_token");
            entity.Property(e => e.UserAgent)
                .HasMaxLength(500)
                .HasColumnName("user_agent");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserSessions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_user_sessions_users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
