using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Tests;

public class TeacherDashboardServiceTests
{
    private static DbContextOptions<ApplicationDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task GetDashboardAsync_ReturnsTeacherDashboardSummary()
    {
        var options = CreateOptions();
        var today = DateTime.Today;

        using (var context = new ApplicationDbContext(options))
        {
            var teacherRole = new Role { Id = 2, RoleCode = "TEACHER", RoleName = "Teacher" };
            var studentRole = new Role { Id = 3, RoleCode = "STUDENT", RoleName = "Student" };
            var teacher = new User
            {
                Id = 10,
                Email = "teacher@test.com",
                FullName = "Ms Teacher",
                PasswordHash = "hash",
                RoleId = teacherRole.Id,
                Role = teacherRole,
                Status = "ACTIVE"
            };
            var student = new User
            {
                Id = 20,
                Email = "student@test.com",
                FullName = "Student One",
                PasswordHash = "hash",
                RoleId = studentRole.Id,
                Role = studentRole,
                Status = "ACTIVE"
            };
            var inactiveStudent = new User
            {
                Id = 21,
                Email = "inactive@test.com",
                FullName = "Inactive Student",
                PasswordHash = "hash",
                RoleId = studentRole.Id,
                Role = studentRole,
                Status = "LOCKED"
            };
            var skill = new EnglishSkill
            {
                Id = 1,
                SkillCode = "SPEAKING",
                SkillName = "Speaking",
                IsActive = true
            };
            var topic = new LearningTopic
            {
                Id = 100,
                SkillId = skill.Id,
                Skill = skill,
                Title = "Conversation Basics",
                DifficultyLevel = "BEGINNER",
                Status = "ACTIVE",
                CreatedBy = teacher.Id
            };
            var archivedTopic = new LearningTopic
            {
                Id = 101,
                SkillId = skill.Id,
                Skill = skill,
                Title = "Old Course",
                DifficultyLevel = "BEGINNER",
                Status = "INACTIVE",
                CreatedBy = teacher.Id
            };
            var task = new PracticeTask
            {
                Id = 200,
                TopicId = topic.Id,
                Topic = topic,
                SkillId = skill.Id,
                Skill = skill,
                Title = "Introduce yourself",
                Instruction = "Record an answer.",
                TaskType = "SPEAKING",
                DifficultyLevel = "BEGINNER",
                Status = "ACTIVE",
                CreatedBy = teacher.Id
            };
            var quiz = new Quiz
            {
                Id = 300,
                TopicId = topic.Id,
                Topic = topic,
                SkillId = skill.Id,
                Skill = skill,
                Title = "Warmup Quiz",
                QuizType = "PRACTICE",
                Status = "ACTIVE",
                CreatedBy = teacher.Id
            };

            context.Roles.AddRange(teacherRole, studentRole);
            context.Users.AddRange(teacher, student, inactiveStudent);
            context.EnglishSkills.Add(skill);
            context.LearningTopics.AddRange(topic, archivedTopic);
            context.PracticeTasks.Add(task);
            context.PracticeSubmissions.AddRange(
                new PracticeSubmission
                {
                    Id = 400,
                    PracticeTaskId = task.Id,
                    PracticeTask = task,
                    StudentId = student.Id,
                    Student = student,
                    SubmittedAt = today.AddHours(9),
                    Status = "SUBMITTED"
                },
                new PracticeSubmission
                {
                    Id = 401,
                    PracticeTaskId = task.Id,
                    PracticeTask = task,
                    StudentId = student.Id,
                    Student = student,
                    SubmittedAt = today.AddDays(-1),
                    Status = "GRADED"
                });
            context.Schedules.AddRange(
                new Schedule
                {
                    Id = 500,
                    TeacherId = teacher.Id,
                    Teacher = teacher,
                    TopicId = topic.Id,
                    Topic = topic,
                    Title = "Morning class",
                    StartTime = today.AddHours(8),
                    EndTime = today.AddHours(9),
                    Classroom = "A101"
                },
                new Schedule
                {
                    Id = 501,
                    TeacherId = teacher.Id,
                    Teacher = teacher,
                    TopicId = topic.Id,
                    Topic = topic,
                    Title = "Yesterday class",
                    StartTime = today.AddDays(-1).AddHours(8),
                    EndTime = today.AddDays(-1).AddHours(9)
                });
            context.Quizzes.Add(quiz);
            context.QuizAttempts.Add(new QuizAttempt
            {
                Id = 600,
                QuizId = quiz.Id,
                Quiz = quiz,
                StudentId = student.Id,
                Student = student,
                StartedAt = today.AddHours(10),
                SubmittedAt = today.AddHours(10).AddMinutes(15),
                Score = 85,
                Status = "SUBMITTED"
            });
            await context.SaveChangesAsync();
        }

        using (var context = new ApplicationDbContext(options))
        {
            var service = new TeacherDashboardService(context);

            var model = await service.GetDashboardAsync(10);

            Assert.Equal("Ms Teacher", model.TeacherName);
            Assert.Equal(1, model.CoursesCount);
            Assert.Equal(1, model.StudentsCount);
            Assert.Equal(1, model.PendingSubmissionsCount);
            Assert.Equal(1, model.TodaySchedulesCount);
            Assert.Equal(2, model.RecentSubmissions.Count);
            Assert.Equal("Student One", model.RecentSubmissions[0].StudentName);
            Assert.Equal("SUBMITTED", model.RecentSubmissions[0].Status);
            Assert.Equal("Morning class", Assert.Single(model.TodaySchedules).Title);
            Assert.Equal(85, Assert.Single(model.RecentQuizAttempts).Score);
            Assert.Equal(new[] { "Speaking" }, model.ChartLabels);
            Assert.Equal(new[] { 1 }, model.ChartValues);
        }
    }
}
