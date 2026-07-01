using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.ViewModels.Teacher;
using DuAnTotNghiep.Services;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Tests;

public class TeacherQuizAndGradingServiceTests
{
    private static DbContextOptions<ApplicationDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task TeacherQuizService_CreatesQuizQuestionsAndReportsAttempts()
    {
        var options = CreateOptions();
        await SeedQuizDataAsync(options);

        using var context = new ApplicationDbContext(options);
        var service = new TeacherQuizService(context);

        var quizId = await service.CreateQuizAsync(new CreateQuizViewModel
        {
            TopicId = 100,
            Title = "New Quiz",
            Description = "Created by teacher",
            TimeLimitMinutes = 20,
            PassScore = 60,
            Questions =
            {
                new QuizQuestionFormViewModel
                {
                    QuestionText = "Choose A",
                    QuestionType = "MCQ",
                    Options = { "A", "B", "C", "D" },
                    CorrectAnswer = "A",
                    Points = 2
                }
            }
        }, 10);

        var created = await context.Quizzes.Include(q => q.QuizQuestions).SingleAsync(q => q.Id == quizId);
        Assert.Equal("PUBLISHED", created.Status);
        Assert.Equal(10, created.CreatedBy);
        Assert.Single(created.QuizQuestions);

        var list = await service.GetQuizzesByTopicAsync(100);
        Assert.Contains(list, q => q.Id == quizId && q.QuestionCount == 1);

        var detail = await service.GetQuizDetailAsync(300);
        Assert.NotNull(detail);
        Assert.Equal("Seed Quiz", detail!.Title);

        var attempts = await service.GetQuizAttemptsAsync(300);
        var attempt = Assert.Single(attempts);
        Assert.Equal("Student One", attempt.StudentName);
        Assert.Equal(88, attempt.Score);

        await service.UpdateQuizAsync(new EditQuizViewModel
        {
            Id = quizId,
            TopicId = 100,
            Title = "Updated Quiz",
            Description = "Updated",
            TimeLimitMinutes = 30,
            PassScore = 70,
            Status = "DRAFT"
        });
        Assert.Equal("Updated Quiz", (await context.Quizzes.FindAsync(quizId))!.Title);

        await service.DeleteQuizAsync(quizId);
        Assert.Equal("ARCHIVED", (await context.Quizzes.FindAsync(quizId))!.Status);
    }

    [Fact]
    public async Task TeacherGradingService_GradesPendingSubmissionsAndBuildsOverview()
    {
        var options = CreateOptions();
        await SeedQuizDataAsync(options);

        using var context = new ApplicationDbContext(options);
        var service = new TeacherGradingService(context);

        var pending = await service.GetPendingSubmissionsAsync(10);
        Assert.Single(pending);
        Assert.Equal("Essay Task", pending[0].TaskTitle);

        var detail = await service.GetSubmissionDetailAsync(500);
        Assert.NotNull(detail);
        Assert.Equal("Student answer", detail!.StudentAnswer);

        await service.GradeSubmissionAsync(500, 9.5m, "Good work", 10);
        var submission = await context.PracticeSubmissions.FindAsync(500);
        Assert.Equal("GRADED", submission!.Status);
        Assert.Equal(9.5m, submission.Score);
        Assert.Equal("Good work", submission.TeacherFeedback);

        var overview = await service.GetGradesOverviewAsync(100);
        var row = Assert.Single(overview);
        Assert.Equal("Student One", row.StudentName);
        Assert.Equal(88, row.QuizScore);
        Assert.Equal(9.5m, row.PracticeScore);
    }

    private static async Task SeedQuizDataAsync(DbContextOptions<ApplicationDbContext> options)
    {
        using var context = new ApplicationDbContext(options);
        var teacherRole = new Role { Id = 2, RoleCode = "TEACHER", RoleName = "Teacher" };
        var studentRole = new Role { Id = 3, RoleCode = "STUDENT", RoleName = "Student" };
        var teacher = new User { Id = 10, Email = "teacher@test.com", FullName = "Teacher", PasswordHash = "hash", RoleId = 2, Role = teacherRole, Status = "ACTIVE" };
        var student = new User { Id = 20, Email = "student@test.com", FullName = "Student One", PasswordHash = "hash", RoleId = 3, Role = studentRole, Status = "ACTIVE" };
        var skill = new EnglishSkill { Id = 1, SkillCode = "WRITING", SkillName = "Writing", IsActive = true };
        var topic = new LearningTopic { Id = 100, SkillId = 1, Skill = skill, Title = "Writing Basics", DifficultyLevel = "BASIC", Status = "ACTIVE", CreatedBy = 10 };
        var quiz = new Quiz { Id = 300, TopicId = 100, Topic = topic, SkillId = 1, Skill = skill, Title = "Seed Quiz", QuizType = "PRACTICE", Status = "PUBLISHED", CreatedBy = 10, CreatedAt = DateTime.UtcNow };
        var task = new PracticeTask { Id = 400, TopicId = 100, Topic = topic, SkillId = 1, Skill = skill, Title = "Essay Task", Instruction = "Write an essay", TaskType = "WRITING", DifficultyLevel = "BASIC", Status = "ACTIVE", CreatedBy = 10 };

        context.Roles.AddRange(teacherRole, studentRole);
        context.Users.AddRange(teacher, student);
        context.EnglishSkills.Add(skill);
        context.LearningTopics.Add(topic);
        context.Quizzes.Add(quiz);
        context.QuizAttempts.Add(new QuizAttempt { Id = 301, QuizId = 300, Quiz = quiz, StudentId = 20, Student = student, StartedAt = DateTime.UtcNow.AddMinutes(-30), SubmittedAt = DateTime.UtcNow, Score = 88, Status = "SUBMITTED" });
        context.PracticeTasks.Add(task);
        context.PracticeSubmissions.Add(new PracticeSubmission { Id = 500, PracticeTaskId = 400, PracticeTask = task, StudentId = 20, Student = student, SubmissionText = "Student answer", SubmittedAt = DateTime.UtcNow, Status = "SUBMITTED" });
        await context.SaveChangesAsync();
    }
}
