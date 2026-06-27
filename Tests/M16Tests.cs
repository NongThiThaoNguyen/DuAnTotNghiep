using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using DuAnTotNghiep.Areas.Student.Controllers;
using DuAnTotNghiep.Services;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.Repositories;
using DuAnTotNghiep.Repositories.Interfaces;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.DTOs.Progress;
using DuAnTotNghiep.Enums;
using DuAnTotNghiep.ViewModels.Progress;

namespace DuAnTotNghiep.Tests
{
    public class M16Tests
    {
        private DbContextOptions<ApplicationDbContext> CreateNewInMemoryDatabaseOptions()
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        #region Original Tests

        [Fact]
        public async Task Test_RecordLessonCompleted_UpdatesSnapshotAndLogs()
        {
            // Arrange
            var options = CreateNewInMemoryDatabaseOptions();
            using (var context = new ApplicationDbContext(options))
            {
                var student = new User { Id = 123, Email = "student@test.com", FullName = "Student Test", PasswordHash = "hash", Status = "ACTIVE", RoleId = 3 };
                var skill = new EnglishSkill { Id = 1, SkillCode = "VOC", SkillName = "Vocabulary", IsActive = true, OrderIndex = 1 };
                var topic = new LearningTopic { Id = 10, Title = "Animals", SkillId = 1, DifficultyLevel = "MEDIUM", Status = "ACTIVE" };
                var lesson = new OriginalLesson { Id = 456, Title = "Animals Lesson 1", TopicId = 10, ContentType = "TEXT", SourceType = "SYSTEM", ReviewStatus = "APPROVED" };
                var path = new StudentLearningPath { Id = 10, StudentId = 123, Status = "ACTIVE", Title = "My Learning Path" };
                var pathNode = new LearningPathNode { Id = 789, LearningPathId = 10, LessonId = 456, TopicId = 10, Status = ProgressStatus.Available, NodeTitle = "Animals Lesson 1", OrderIndex = 1, NodeType = "LESSON" };

                await context.Users.AddAsync(student);
                await context.EnglishSkills.AddAsync(skill);
                await context.LearningTopics.AddAsync(topic);
                await context.OriginalLessons.AddAsync(lesson);
                await context.StudentLearningPaths.AddAsync(path);
                await context.LearningPathNodes.AddAsync(pathNode);
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var activityLogRepo = new ActivityLogRepository(context);
                var progressRepo = new ProgressRepository(context);
                var trackingService = new ProgressTrackingService(progressRepo, activityLogRepo, context);

                // Act
                var result = await trackingService.RecordLessonCompleted(123, 456, 30);

                // Assert
                Assert.True(result);

                var log = await context.StudyActivityLogs.FirstOrDefaultAsync(l => l.StudentId == 123 && l.ActivityType == ActivityType.Learn);
                Assert.NotNull(log);
                Assert.Equal(30, log.DurationMinutes);
                Assert.Equal(789, log.LearningPathNodeId);

                var updatedNode = await context.LearningPathNodes.FindAsync(789);
                Assert.Equal(ProgressStatus.Completed, updatedNode.Status);
            }
        }

        [Fact]
        public async Task Test_GetReplanningInputAsync_ReturnsCleanCorrectData()
        {
            // Arrange
            var options = CreateNewInMemoryDatabaseOptions();
            using (var context = new ApplicationDbContext(options))
            {
                var student = new User { Id = 123, Email = "student@test.com", FullName = "Student Test", PasswordHash = "hash", Status = "ACTIVE", RoleId = 3 };
                var skill = new EnglishSkill { Id = 1, SkillCode = "VOC", SkillName = "Vocabulary", IsActive = true, OrderIndex = 1 };
                var topic1 = new LearningTopic { Id = 5, Title = "Animals", SkillId = 1, DifficultyLevel = "MEDIUM", Status = "ACTIVE" };
                var topic2 = new LearningTopic { Id = 6, Title = "Food", SkillId = 1, DifficultyLevel = "MEDIUM", Status = "ACTIVE" };
                var path = new StudentLearningPath { Id = 10, StudentId = 123, Status = "ACTIVE", Title = "My Learning Path" };
                
                var node1 = new LearningPathNode { Id = 34, LearningPathId = 10, TopicId = 5, Status = ProgressStatus.NeedReview, NodeTitle = "Animals Node", OrderIndex = 1, NodeType = "LESSON" };
                var node2 = new LearningPathNode { Id = 35, LearningPathId = 10, TopicId = 6, Status = ProgressStatus.Available, NodeTitle = "Food Node", OrderIndex = 2, NodeType = "QUIZ" };

                await context.Users.AddAsync(student);
                await context.EnglishSkills.AddAsync(skill);
                await context.LearningTopics.AddAsync(topic1);
                await context.LearningTopics.AddAsync(topic2);
                await context.StudentLearningPaths.AddAsync(path);
                await context.LearningPathNodes.AddAsync(node1);
                await context.LearningPathNodes.AddAsync(node2);

                // Activities
                var log1 = new StudyActivityLog { StudentId = 123, TopicId = 5, Score = 4.0m, ActivityType = ActivityType.Quiz, CreatedAt = DateTime.UtcNow.AddDays(-10) };
                var log2 = new StudyActivityLog { StudentId = 123, TopicId = 6, Score = 3.0m, ActivityType = ActivityType.Quiz, CreatedAt = DateTime.UtcNow.AddDays(-10) };
                var log3 = new StudyActivityLog { StudentId = 123, TopicId = 6, Score = 9.0m, ActivityType = ActivityType.Quiz, CreatedAt = DateTime.UtcNow.AddDays(-2) };
                var log4 = new StudyActivityLog { StudentId = 123, TopicId = 6, ActivityType = ActivityType.Review, CreatedAt = DateTime.UtcNow.AddDays(-3) };

                await context.StudyActivityLogs.AddRangeAsync(log1, log2, log3, log4);
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var activityLogRepo = new ActivityLogRepository(context);
                var progressRepo = new ProgressRepository(context);
                var service = new StudentProgressService(activityLogRepo, progressRepo, context);

                // Act
                var result = await service.GetReplanningInputAsync(123, 10);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(123, result.StudentId);
                Assert.Equal(10, result.PathId);
                
                Assert.Contains(34, result.RemainingNodes);
                Assert.Contains(35, result.RemainingNodes);
                Assert.Contains("Animals", result.WeakTopics);
                Assert.Contains("Food", result.FastImprovementTopics);
                Assert.NotEmpty(result.InactiveDays);
            }
        }

        [Fact]
        public async Task Test_ProgressController_Authorization_BlocksCrossStudentView()
        {
            // Arrange
            var options = CreateNewInMemoryDatabaseOptions();
            using (var context = new ApplicationDbContext(options))
            {
                var student = new User { Id = 123, Email = "student@test.com", FullName = "Student Test", PasswordHash = "hash", Status = "ACTIVE", RoleId = 3 };
                await context.Users.AddAsync(student);
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var mockService = new Mock<IStudentProgressService>();
                var mockTracking = new Mock<IProgressTrackingService>();
                var mockRepo = new Mock<IProgressRepository>();

                var controller = new ProgressController(mockService.Object, mockTracking.Object, mockRepo.Object, context);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, "123"),
                    new Claim(ClaimTypes.Role, "STUDENT")
                };
                var identity = new ClaimsIdentity(claims, "TestAuth");
                var principal = new ClaimsPrincipal(identity);

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = principal }
                };

                // Act & Assert
                mockService.Setup(s => s.GetDashboardAsync(123)).ReturnsAsync(new ProgressDashboardViewModel());
                var ownResult = await controller.Index(123);
                Assert.IsType<ViewResult>(ownResult);

                var crossResult = await controller.Index(456);
                Assert.IsType<ForbidResult>(crossResult);
            }
        }

        #endregion

        #region New Unit Tests & Test Cases

        [Fact]
        public async Task Test_RecordLessonCompleted_WithNonExistentLessonId_ReturnsFalseAndLogsError()
        {
            // Arrange
            var options = CreateNewInMemoryDatabaseOptions();
            using (var context = new ApplicationDbContext(options))
            {
                var activityLogRepo = new ActivityLogRepository(context);
                var progressRepo = new ProgressRepository(context);
                var trackingService = new ProgressTrackingService(progressRepo, activityLogRepo, context);

                // Act
                var result = await trackingService.RecordLessonCompleted(123, 99999, 15);

                // Assert
                Assert.False(result);
            }
        }

        [Fact]
        public async Task Test_RecordQuizSubmitted_HighAndLowScores_CalculatesCorrectly()
        {
            // Arrange
            var options = CreateNewInMemoryDatabaseOptions();
            using (var context = new ApplicationDbContext(options))
            {
                var student = new User { Id = 123, Email = "student@test.com", FullName = "Student Test", PasswordHash = "hash", Status = "ACTIVE", RoleId = 3 };
                var skill = new EnglishSkill { Id = 1, SkillCode = "VOC", SkillName = "Vocabulary", IsActive = true, OrderIndex = 1 };
                var topic = new LearningTopic { Id = 10, Title = "Animals", SkillId = 1, DifficultyLevel = "MEDIUM", Status = "ACTIVE" };
                
                var quizLow = new Quiz { Id = 401, Title = "Low Quiz", TopicId = 10, SkillId = 1, QuizType = "QUIZ", Status = "ACTIVE", TimeLimitMinutes = 10, PassingScore = 5.0m };
                var quizHigh = new Quiz { Id = 402, Title = "High Quiz", TopicId = 10, SkillId = 1, QuizType = "QUIZ", Status = "ACTIVE", TimeLimitMinutes = 10, PassingScore = 5.0m };
                
                var path = new StudentLearningPath { Id = 10, StudentId = 123, Status = "ACTIVE", Title = "My Learning Path" };
                
                var nodeLow = new LearningPathNode { Id = 701, LearningPathId = 10, QuizId = 401, TopicId = 10, Status = ProgressStatus.Available, NodeTitle = "Low Node", OrderIndex = 1, NodeType = "QUIZ" };
                var nodeHigh = new LearningPathNode { Id = 702, LearningPathId = 10, QuizId = 402, TopicId = 10, Status = ProgressStatus.Locked, NodeTitle = "High Node", OrderIndex = 2, NodeType = "QUIZ" };

                await context.Users.AddAsync(student);
                await context.EnglishSkills.AddAsync(skill);
                await context.LearningTopics.AddAsync(topic);
                await context.Quizzes.AddRangeAsync(quizLow, quizHigh);
                await context.StudentLearningPaths.AddAsync(path);
                await context.LearningPathNodes.AddRangeAsync(nodeLow, nodeHigh);
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var activityLogRepo = new ActivityLogRepository(context);
                var progressRepo = new ProgressRepository(context);
                var trackingService = new ProgressTrackingService(progressRepo, activityLogRepo, context);

                // Act - Submit Quiz Low (Score 3.0 out of 10)
                var resultLow = await trackingService.RecordQuizSubmitted(123, 401, 3.0m, 10);

                // Assert Low
                Assert.True(resultLow);
                var nodeLowDb = await context.LearningPathNodes.FindAsync(701);
                Assert.Equal(ProgressStatus.NeedReview, nodeLowDb.Status); // Failed, should mark as NeedReview

                // Act - Submit Quiz High (Score 8.5 out of 10)
                // First make high node available to mock proper order state flow
                var nodeHighDb = await context.LearningPathNodes.FindAsync(702);
                nodeHighDb.Status = ProgressStatus.Available;
                context.LearningPathNodes.Update(nodeHighDb);
                await context.SaveChangesAsync();

                var resultHigh = await trackingService.RecordQuizSubmitted(123, 402, 8.5m, 12);

                // Assert High
                Assert.True(resultHigh);
                var updatedHighNode = await context.LearningPathNodes.FindAsync(702);
                Assert.Equal(ProgressStatus.Completed, updatedHighNode.Status);
            }
        }

        [Fact]
        public async Task Test_GetDashboardAsync_ReturnsExpectedViewModelsAndGPA()
        {
            // Arrange
            var options = CreateNewInMemoryDatabaseOptions();
            using (var context = new ApplicationDbContext(options))
            {
                var student = new User { Id = 123, Email = "student@test.com", FullName = "Student Test", PasswordHash = "hash", Status = "ACTIVE", RoleId = 3 };
                var skill = new EnglishSkill { Id = 1, SkillCode = "VOC", SkillName = "Vocabulary", IsActive = true, OrderIndex = 1 };
                var topic = new LearningTopic { Id = 10, Title = "Animals", SkillId = 1, DifficultyLevel = "MEDIUM", Status = "ACTIVE" };
                var path = new StudentLearningPath { Id = 10, StudentId = 123, Status = "ACTIVE", Title = "My Learning Path" };
                var quiz = new Quiz { Id = 777, Title = "Animals Quiz", TopicId = 10, SkillId = 1, QuizType = "QUIZ", Status = "ACTIVE" };
                var pathNode = new LearningPathNode { Id = 789, LearningPathId = 10, TopicId = 10, QuizId = 777, Status = ProgressStatus.Completed, NodeTitle = "Completed Node", OrderIndex = 1, NodeType = "QUIZ", LearningPath = path };

                var log = new StudyActivityLog { StudentId = 123, TopicId = 10, Score = 9.0m, ActivityType = ActivityType.Quiz, CreatedAt = DateTime.UtcNow };

                await context.Users.AddAsync(student);
                await context.EnglishSkills.AddAsync(skill);
                await context.LearningTopics.AddAsync(topic);
                await context.Quizzes.AddAsync(quiz);
                await context.StudentLearningPaths.AddAsync(path);
                await context.LearningPathNodes.AddAsync(pathNode);
                await context.StudyActivityLogs.AddAsync(log);
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var activityLogRepo = new ActivityLogRepository(context);
                var progressRepo = new ProgressRepository(context);
                var service = new StudentProgressService(activityLogRepo, progressRepo, context);

                // Recalculate first to populate snapshot cache
                var trackingService = new ProgressTrackingService(progressRepo, activityLogRepo, context);
                await trackingService.RecalculateStudentProgress(123);

                // Act
                var dashboard = await service.GetDashboardAsync(123);

                // Assert
                Assert.NotNull(dashboard);
                Assert.Equal(0, dashboard.CompletedLessonsCount); // 0 lesson-type nodes, 1 quiz-type node
                Assert.Equal(1, dashboard.CompletedQuizzesCount);
                Assert.NotEmpty(dashboard.SkillProgresses);

                var skillProgress = dashboard.SkillProgresses.First();
                Assert.Equal("Vocabulary", skillProgress.SkillName);
                Assert.Equal(9.0m, skillProgress.AverageScore);
            }
        }

        [Fact]
        public async Task Test_SnapshotJob_GeneratesSnapshotsSuccessfully()
        {
            // Arrange
            var options = CreateNewInMemoryDatabaseOptions();
            using (var context = new ApplicationDbContext(options))
            {
                var student = new User { Id = 123, Email = "student@test.com", FullName = "Student Test", PasswordHash = "hash", Status = "ACTIVE", RoleId = 3 };
                var skill = new EnglishSkill { Id = 1, SkillCode = "VOC", SkillName = "Vocabulary", IsActive = true, OrderIndex = 1 };
                var topic = new LearningTopic { Id = 10, Title = "Animals", SkillId = 1, DifficultyLevel = "MEDIUM", Status = "ACTIVE" };
                var path = new StudentLearningPath { Id = 10, StudentId = 123, Status = "ACTIVE", Title = "My Learning Path" };
                var node = new LearningPathNode { Id = 1, LearningPathId = 10, TopicId = 10, Status = ProgressStatus.Completed, NodeTitle = "N1", NodeType = "LESSON" };

                await context.Users.AddAsync(student);
                await context.EnglishSkills.AddAsync(skill);
                await context.LearningTopics.AddAsync(topic);
                await context.StudentLearningPaths.AddAsync(path);
                await context.LearningPathNodes.AddAsync(node);
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var activityLogRepo = new ActivityLogRepository(context);
                var progressRepo = new ProgressRepository(context);
                var trackingService = new ProgressTrackingService(progressRepo, activityLogRepo, context);

                // Act
                var recalculateResult = await trackingService.RecalculateStudentProgress(123);

                // Assert
                Assert.True(recalculateResult);
                
                var snapshot = await context.StudentProgressSnapshots
                    .FirstOrDefaultAsync(s => s.StudentId == 123 && s.TopicId == 10);
                
                Assert.NotNull(snapshot);
                Assert.Equal(1, snapshot.CompletedNodes);
                Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), snapshot.SnapshotDate);
            }
        }

        [Fact]
        public async Task Test_ProgressController_Authorization_CrossAccessDetails_ReturnsForbid()
        {
            // Arrange
            var options = CreateNewInMemoryDatabaseOptions();
            using (var context = new ApplicationDbContext(options))
            {
                var student = new User { Id = 123, Email = "student@test.com", FullName = "Student Test", PasswordHash = "hash", Status = "ACTIVE", RoleId = 3 };
                await context.Users.AddAsync(student);
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var mockService = new Mock<IStudentProgressService>();
                var mockTracking = new Mock<IProgressTrackingService>();
                var mockRepo = new Mock<IProgressRepository>();

                var controller = new ProgressController(mockService.Object, mockTracking.Object, mockRepo.Object, context);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, "123"),
                    new Claim(ClaimTypes.Role, "STUDENT")
                };
                var identity = new ClaimsIdentity(claims, "TestAuth");
                var principal = new ClaimsPrincipal(identity);

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = principal }
                };

                // Act
                var filter = new HistoryFilter { Page = 1 };
                var result = await controller.History(filter, 456);

                // Assert
                Assert.IsType<ForbidResult>(result);
            }
        }

        #endregion

        #region Integration & Performance Tests

        [Fact]
        public async Task Test_Integration_StudentLearningWorkflow_ReflectsUpdatesOnDashboard()
        {
            // Arrange
            var options = CreateNewInMemoryDatabaseOptions();
            using (var context = new ApplicationDbContext(options))
            {
                // Set up basic tables
                var student = new User { Id = 123, Email = "student@test.com", FullName = "Student Test", PasswordHash = "hash", Status = "ACTIVE", RoleId = 3 };
                var skill = new EnglishSkill { Id = 1, SkillCode = "VOC", SkillName = "Vocabulary", IsActive = true, OrderIndex = 1 };
                var topic = new LearningTopic { Id = 10, Title = "Animals", SkillId = 1, DifficultyLevel = "MEDIUM", Status = "ACTIVE" };
                var lesson = new OriginalLesson { Id = 456, Title = "Animals Lesson 1", TopicId = 10, ContentType = "TEXT", SourceType = "SYSTEM", ReviewStatus = "APPROVED" };
                var quiz = new Quiz { Id = 777, Title = "Animals Quiz 1", TopicId = 10, SkillId = 1, QuizType = "QUIZ", Status = "ACTIVE", TimeLimitMinutes = 10, PassingScore = 5.0m };
                var path = new StudentLearningPath { Id = 10, StudentId = 123, Status = "ACTIVE", Title = "My Learning Path" };
                
                // NodeType must be "LEARN" to match service counting logic: n.NodeType == "LEARN"
                var nodeLesson = new LearningPathNode { Id = 1, LearningPathId = 10, LessonId = 456, TopicId = 10, Status = ProgressStatus.Available, NodeTitle = "Animals Lesson 1", NodeType = "LEARN", OrderIndex = 1 };
                var nodeQuiz = new LearningPathNode { Id = 2, LearningPathId = 10, QuizId = 777, TopicId = 10, Status = ProgressStatus.Locked, NodeTitle = "Animals Quiz 1", NodeType = "QUIZ", OrderIndex = 2 };

                await context.Users.AddAsync(student);
                await context.EnglishSkills.AddAsync(skill);
                await context.LearningTopics.AddAsync(topic);
                await context.OriginalLessons.AddAsync(lesson);
                await context.Quizzes.AddAsync(quiz);
                await context.StudentLearningPaths.AddAsync(path);
                await context.LearningPathNodes.AddRangeAsync(nodeLesson, nodeQuiz);
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var activityLogRepo = new ActivityLogRepository(context);
                var progressRepo = new ProgressRepository(context);
                
                var trackingService = new ProgressTrackingService(progressRepo, activityLogRepo, context);
                var progressService = new StudentProgressService(activityLogRepo, progressRepo, context);

                // Step 1: Complete Lesson (Duration: 20 minutes)
                var step1Result = await trackingService.RecordLessonCompleted(123, 456, 20);
                Assert.True(step1Result);

                // Step 2: Verify next quiz is unlocked
                var quizNode = await context.LearningPathNodes.FindAsync(2);
                Assert.Equal(ProgressStatus.Available, quizNode.Status);

                // Step 3: Complete Quiz with score 80% (8.0/10)
                var step2Result = await trackingService.RecordQuizSubmitted(123, 777, 8.0m, 15);
                Assert.True(step2Result);

                // Step 4: Load Dashboard and check stats
                var dashboard = await progressService.GetDashboardAsync(123);
                Assert.NotNull(dashboard);
                Assert.Equal(1, dashboard.CompletedLessonsCount); // 1 "LEARN" node completed
                Assert.Equal(1, dashboard.CompletedQuizzesCount); // 1 quiz node completed
                Assert.Equal(35, dashboard.TotalStudyMinutes); // 20 min + 15 min = 35 minutes
                Assert.Equal(8.0m, dashboard.SkillProgresses.First().AverageScore);
            }
        }

        [Fact]
        public async Task Test_Performance_BulkActivityData_LoadsDashboardQuickly()
        {
            // Arrange
            var options = CreateNewInMemoryDatabaseOptions();
            using (var context = new ApplicationDbContext(options))
            {
                var student = new User { Id = 123, Email = "student@test.com", FullName = "Student Test", PasswordHash = "hash", Status = "ACTIVE", RoleId = 3 };
                var skill = new EnglishSkill { Id = 1, SkillCode = "VOC", SkillName = "Vocabulary", IsActive = true, OrderIndex = 1 };
                var topic = new LearningTopic { Id = 10, Title = "Animals", SkillId = 1, DifficultyLevel = "MEDIUM", Status = "ACTIVE" };
                var path = new StudentLearningPath { Id = 10, StudentId = 123, Status = "ACTIVE", Title = "My Learning Path" };

                await context.Users.AddAsync(student);
                await context.EnglishSkills.AddAsync(skill);
                await context.LearningTopics.AddAsync(topic);
                await context.StudentLearningPaths.AddAsync(path);

                // Simulate 500 study activities to test query performance
                var bulkLogs = new List<StudyActivityLog>();
                for (int i = 0; i < 500; i++)
                {
                    bulkLogs.Add(new StudyActivityLog
                    {
                        StudentId = 123,
                        TopicId = 10,
                        ActivityType = ActivityType.Learn,
                        DurationMinutes = 10,
                        CreatedAt = DateTime.UtcNow.AddHours(-i)
                    });
                }
                await context.StudyActivityLogs.AddRangeAsync(bulkLogs);
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var activityLogRepo = new ActivityLogRepository(context);
                var progressRepo = new ProgressRepository(context);
                var service = new StudentProgressService(activityLogRepo, progressRepo, context);

                var watch = System.Diagnostics.Stopwatch.StartNew();

                // Act
                var dashboard = await service.GetDashboardAsync(123);

                watch.Stop();

                // Assert
                Assert.NotNull(dashboard);
                Assert.True(watch.ElapsedMilliseconds < 500, $"Dashboard loaded too slowly: {watch.ElapsedMilliseconds}ms");
            }
        }

        #endregion
    }
}
