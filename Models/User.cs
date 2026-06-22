using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class User
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public string? Phone { get; set; }

    public int RoleId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? LastLoginAt { get; set; }

    public int FailedLoginCount { get; set; }

    public DateTime? LockoutUntil { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<AiFeedback> AiFeedbacks { get; set; } = new List<AiFeedback>();

    public virtual ICollection<AiGeneratedContent> AiGeneratedContentRequestedByNavigations { get; set; } = new List<AiGeneratedContent>();

    public virtual ICollection<AiGeneratedContent> AiGeneratedContentReviewedByNavigations { get; set; } = new List<AiGeneratedContent>();

    public virtual ICollection<AiPromptTemplate> AiPromptTemplates { get; set; } = new List<AiPromptTemplate>();

    public virtual ICollection<AiReplanningEvent> AiReplanningEvents { get; set; } = new List<AiReplanningEvent>();

    public virtual ICollection<AiTutorConversation> AiTutorConversations { get; set; } = new List<AiTutorConversation>();

    public virtual ICollection<AiUsageLog> AiUsageLogs { get; set; } = new List<AiUsageLog>();

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<CompetencyAnalysis> CompetencyAnalyses { get; set; } = new List<CompetencyAnalysis>();

    public virtual ICollection<ContentComplianceReview> ContentComplianceReviews { get; set; } = new List<ContentComplianceReview>();

    public virtual ICollection<LearningPathTemplate> LearningPathTemplates { get; set; } = new List<LearningPathTemplate>();

    public virtual ICollection<LearningTopic> LearningTopics { get; set; } = new List<LearningTopic>();

    public virtual ICollection<Notification> NotificationCreatedByNavigations { get; set; } = new List<Notification>();

    public virtual ICollection<NotificationRead> NotificationReads { get; set; } = new List<NotificationRead>();

    public virtual ICollection<Notification> NotificationTargetUsers { get; set; } = new List<Notification>();

    public virtual ICollection<OriginalLesson> OriginalLessonApprovedByNavigations { get; set; } = new List<OriginalLesson>();

    public virtual ICollection<OriginalLesson> OriginalLessonCreatedByNavigations { get; set; } = new List<OriginalLesson>();

    public virtual ICollection<PlacementTest> PlacementTests { get; set; } = new List<PlacementTest>();

    public virtual ICollection<PracticeSubmission> PracticeSubmissions { get; set; } = new List<PracticeSubmission>();

    public virtual ICollection<PracticeTask> PracticeTasks { get; set; } = new List<PracticeTask>();

    public virtual ICollection<QuestionBank> QuestionBankApprovedByNavigations { get; set; } = new List<QuestionBank>();

    public virtual ICollection<QuestionBank> QuestionBankCreatedByNavigations { get; set; } = new List<QuestionBank>();

    public virtual ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();

    public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();

    public virtual ICollection<ReferenceSource> ReferenceSourceApprovedByNavigations { get; set; } = new List<ReferenceSource>();

    public virtual ICollection<ReferenceSource> ReferenceSourceCreatedByNavigations { get; set; } = new List<ReferenceSource>();

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public virtual ICollection<ReportSnapshot> ReportSnapshots { get; set; } = new List<ReportSnapshot>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<SearchLog> SearchLogs { get; set; } = new List<SearchLog>();

    public virtual ICollection<StudentLearningPath> StudentLearningPaths { get; set; } = new List<StudentLearningPath>();

    public virtual StudentLearningProfile? StudentLearningProfile { get; set; }

    public virtual ICollection<StudentProgressSnapshot> StudentProgressSnapshots { get; set; } = new List<StudentProgressSnapshot>();

    public virtual ICollection<StudyActivityLog> StudyActivityLogs { get; set; } = new List<StudyActivityLog>();

    public virtual ICollection<SystemSetting> SystemSettings { get; set; } = new List<SystemSetting>();

    public virtual ICollection<TestAttempt> TestAttempts { get; set; } = new List<TestAttempt>();

    public virtual UserProfile? UserProfile { get; set; }

    public virtual UserSetting? UserSetting { get; set; }

    public virtual ICollection<UserAvatarHistory> UserAvatarHistories { get; set; } = new List<UserAvatarHistory>();

    public virtual ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();

    public virtual ICollection<LoginLog> LoginLogs { get; set; } = new List<LoginLog>();

    public virtual ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();
}
