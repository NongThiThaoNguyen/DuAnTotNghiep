using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace DuAnTotNghiep.Tests;

public class AdminTeacherScheduleServiceTests
{
    private static DbContextOptions<ApplicationDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task GetSchedulesAsync_FiltersByTeacherAndDateThenSortsDescending()
    {
        var options = CreateOptions();
        var targetDate = new DateTime(2026, 7, 2);

        using (var context = new ApplicationDbContext(options))
        {
            SeedScheduleData(context, targetDate);
            await context.SaveChangesAsync();
        }

        using (var context = new ApplicationDbContext(options))
        {
            var service = CreateAdminTeacherScheduleService(context);
            var filter = CreateFilter(teacherId: 10, fromDate: targetDate, toDate: targetDate, sortBy: "startTime", sortDirection: "desc");

            var result = await InvokeAsync(service, "GetSchedulesAsync", filter);
            var items = GetEnumerableProperty(result, "Items").ToList();

            Assert.Equal(2, GetIntProperty(result, "TotalItems"));
            Assert.Equal(new[] { 101, 100 }, items.Select(item => GetIntProperty(item, "Id")));
            Assert.All(items, item => Assert.Equal(10, GetIntProperty(item, "TeacherId")));
        }
    }

    [Fact]
    public async Task GetSchedulesAsync_FiltersKeywordAcrossTeacherTopicAndClassroom()
    {
        var options = CreateOptions();
        var targetDate = new DateTime(2026, 7, 2);

        using (var context = new ApplicationDbContext(options))
        {
            SeedScheduleData(context, targetDate);
            await context.SaveChangesAsync();
        }

        using (var context = new ApplicationDbContext(options))
        {
            var service = CreateAdminTeacherScheduleService(context);
            var filter = CreateFilter(keyword: "listening", sortBy: "teacher", sortDirection: "asc");

            var result = await InvokeAsync(service, "GetSchedulesAsync", filter);
            var item = Assert.Single(GetEnumerableProperty(result, "Items"));

            Assert.Equal(102, GetIntProperty(item, "Id"));
            Assert.Equal("Tran Binh", GetStringProperty(item, "TeacherName"));
            Assert.Equal("Listening", GetStringProperty(item, "TopicName"));
        }
    }

    [Fact]
    public async Task CreateScheduleAsync_RejectsStudentAssignee()
    {
        var options = CreateOptions();
        var targetDate = new DateTime(2026, 7, 2);

        using (var context = new ApplicationDbContext(options))
        {
            SeedScheduleData(context, targetDate);
            await context.SaveChangesAsync();
        }

        using (var context = new ApplicationDbContext(options))
        {
            var service = CreateAdminTeacherScheduleService(context);
            var form = CreateForm(
                teacherId: 20,
                topicId: 200,
                title: "Student should not be scheduled",
                startTime: targetDate.AddHours(15),
                endTime: targetDate.AddHours(16));

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => InvokeAsync(service, "CreateScheduleAsync", form));
            Assert.Contains("Giáo viên", exception.Message);
        }
    }

    private static void SeedScheduleData(ApplicationDbContext context, DateTime targetDate)
    {
        var teacherRole = new Role { Id = 2, RoleCode = "TEACHER", RoleName = "Teacher" };
        var studentRole = new Role { Id = 3, RoleCode = "STUDENT", RoleName = "Student" };
        var teacherA = new User
        {
            Id = 10,
            Email = "an.teacher@test.com",
            FullName = "Nguyen An",
            PasswordHash = "hash",
            RoleId = teacherRole.Id,
            Role = teacherRole,
            Status = "ACTIVE"
        };
        var teacherB = new User
        {
            Id = 11,
            Email = "binh.teacher@test.com",
            FullName = "Tran Binh",
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
        var skill = new EnglishSkill
        {
            Id = 1,
            SkillCode = "ENG",
            SkillName = "English",
            IsActive = true
        };
        var speaking = new LearningTopic
        {
            Id = 200,
            SkillId = skill.Id,
            Skill = skill,
            Title = "Speaking",
            DifficultyLevel = "BEGINNER",
            Status = "ACTIVE",
            CreatedBy = teacherA.Id
        };
        var listening = new LearningTopic
        {
            Id = 201,
            SkillId = skill.Id,
            Skill = skill,
            Title = "Listening",
            DifficultyLevel = "BEGINNER",
            Status = "ACTIVE",
            CreatedBy = teacherB.Id
        };

        context.Roles.AddRange(teacherRole, studentRole);
        context.Users.AddRange(teacherA, teacherB, student);
        context.EnglishSkills.Add(skill);
        context.LearningTopics.AddRange(speaking, listening);
        context.Schedules.AddRange(
            new Schedule
            {
                Id = 100,
                TeacherId = teacherA.Id,
                Teacher = teacherA,
                TopicId = speaking.Id,
                Topic = speaking,
                Title = "Morning class",
                StartTime = targetDate.AddHours(8),
                EndTime = targetDate.AddHours(9),
                Classroom = "A101"
            },
            new Schedule
            {
                Id = 101,
                TeacherId = teacherA.Id,
                Teacher = teacherA,
                TopicId = speaking.Id,
                Topic = speaking,
                Title = "Later class",
                StartTime = targetDate.AddHours(10),
                EndTime = targetDate.AddHours(11),
                Classroom = "A102"
            },
            new Schedule
            {
                Id = 102,
                TeacherId = teacherB.Id,
                Teacher = teacherB,
                TopicId = listening.Id,
                Topic = listening,
                Title = "Listening workshop",
                StartTime = targetDate.AddHours(7),
                EndTime = targetDate.AddHours(8),
                Classroom = "B201"
            },
            new Schedule
            {
                Id = 103,
                TeacherId = teacherA.Id,
                Teacher = teacherA,
                TopicId = speaking.Id,
                Topic = speaking,
                Title = "Tomorrow class",
                StartTime = targetDate.AddDays(1).AddHours(8),
                EndTime = targetDate.AddDays(1).AddHours(9),
                Classroom = "A103"
            });
    }

    private static object CreateAdminTeacherScheduleService(ApplicationDbContext context)
    {
        var serviceType = Type.GetType("DuAnTotNghiep.Services.AdminTeacherScheduleService, DuAnTotNghiep");
        Assert.NotNull(serviceType);
        return Activator.CreateInstance(serviceType!, context)!;
    }

    private static object CreateFilter(
        string? keyword = null,
        int? teacherId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string sortBy = "startTime",
        string sortDirection = "asc")
    {
        var filterType = Type.GetType("DuAnTotNghiep.Areas.Admin.ViewModels.AdminTeacherScheduleFilterViewModel, DuAnTotNghiep");
        Assert.NotNull(filterType);
        var filter = Activator.CreateInstance(filterType!)!;

        SetProperty(filter, "Keyword", keyword);
        SetProperty(filter, "TeacherId", teacherId);
        SetProperty(filter, "FromDate", fromDate);
        SetProperty(filter, "ToDate", toDate);
        SetProperty(filter, "SortBy", sortBy);
        SetProperty(filter, "SortDirection", sortDirection);
        SetProperty(filter, "Page", 1);
        SetProperty(filter, "PageSize", 10);

        return filter;
    }

    private static object CreateForm(int teacherId, int? topicId, string title, DateTime startTime, DateTime endTime)
    {
        var formType = Type.GetType("DuAnTotNghiep.Areas.Admin.ViewModels.AdminTeacherScheduleFormViewModel, DuAnTotNghiep");
        Assert.NotNull(formType);
        var form = Activator.CreateInstance(formType!)!;

        SetProperty(form, "TeacherId", teacherId);
        SetProperty(form, "TopicId", topicId);
        SetProperty(form, "Title", title);
        SetProperty(form, "StartTime", startTime);
        SetProperty(form, "EndTime", endTime);
        SetProperty(form, "Classroom", "Online");

        return form;
    }

    private static async Task<object> InvokeAsync(object target, string methodName, params object[] args)
    {
        var method = target.GetType().GetMethod(methodName);
        Assert.NotNull(method);

        var task = (Task)method!.Invoke(target, args)!;
        await task;

        var resultProperty = task.GetType().GetProperty("Result");
        return resultProperty?.GetValue(task)!;
    }

    private static IEnumerable<object> GetEnumerableProperty(object target, string propertyName)
    {
        var value = target.GetType().GetProperty(propertyName)?.GetValue(target);
        Assert.NotNull(value);
        return ((System.Collections.IEnumerable)value!).Cast<object>();
    }

    private static int GetIntProperty(object target, string propertyName)
    {
        return (int)target.GetType().GetProperty(propertyName)!.GetValue(target)!;
    }

    private static string GetStringProperty(object target, string propertyName)
    {
        return (string)target.GetType().GetProperty(propertyName)!.GetValue(target)!;
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        target.GetType().GetProperty(propertyName)!.SetValue(target, value);
    }
}
