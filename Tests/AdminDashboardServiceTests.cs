using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Tests;

public class AdminDashboardServiceTests
{
    private static DbContextOptions<ApplicationDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task GetSystemOverviewAsync_ReturnsRoleCountsLearningStatsAndRecentUsers()
    {
        var options = CreateOptions();
        var now = new DateTime(2026, 7, 3, 12, 0, 0, DateTimeKind.Utc);

        using (var context = new ApplicationDbContext(options))
        {
            SeedDashboardData(context, now);
            await context.SaveChangesAsync();
        }

        using (var context = new ApplicationDbContext(options))
        {
            var service = CreateAdminDashboardService(context);

            var result = await InvokeAsync(service, "GetSystemOverviewAsync");
            var recentUsers = GetEnumerableProperty(result, "RecentUsers").ToList();

            Assert.Equal(4, GetIntProperty(result, "TotalUsers"));
            Assert.Equal(2, GetIntProperty(result, "TotalStudents"));
            Assert.Equal(1, GetIntProperty(result, "TotalTeachers"));
            Assert.Equal(1, GetIntProperty(result, "TotalAdmins"));
            Assert.Equal(1, GetIntProperty(result, "ActiveLearningStudents"));
            Assert.Equal(1, GetIntProperty(result, "CompletedLearningPaths"));
            Assert.Equal(1, GetIntProperty(result, "PlacementAttemptsLast30Days"));
            Assert.Equal(1, GetIntProperty(result, "UnreadNotifications"));
            Assert.Equal(new[] { "Student New", "Student Old" }, recentUsers.Select(u => GetStringProperty(u, "FullName")).ToArray());
        }
    }

    [Fact]
    public async Task GetLearningPathCompletionStatsAsync_ReturnsCompletionRateChartItems()
    {
        var options = CreateOptions();
        var now = new DateTime(2026, 7, 3, 12, 0, 0, DateTimeKind.Utc);

        using (var context = new ApplicationDbContext(options))
        {
            SeedDashboardData(context, now);
            await context.SaveChangesAsync();
        }

        using (var context = new ApplicationDbContext(options))
        {
            var service = CreateAdminDashboardService(context);

            var result = await InvokeAsync(service, "GetLearningPathCompletionStatsAsync");
            var items = GetEnumerableProperty(result, "Items").ToList();

            Assert.Equal(3, GetIntProperty(result, "TotalPaths"));
            Assert.Equal(1, GetIntProperty(result, "CompletedPaths"));
            Assert.Equal(33.33m, GetDecimalProperty(result, "CompletionRate"));
            Assert.Contains(items, item => GetStringProperty(item, "Label") == "Hoàn thành" && GetDoubleProperty(item, "Value") == 1);
            Assert.Contains(items, item => GetStringProperty(item, "Label") == "Đang học" && GetDoubleProperty(item, "Value") == 2);
        }
    }

    private static void SeedDashboardData(ApplicationDbContext context, DateTime now)
    {
        var adminRole = new Role { Id = 1, RoleCode = "ADMIN", RoleName = "Admin", CreatedAt = now };
        var teacherRole = new Role { Id = 2, RoleCode = "TEACHER", RoleName = "Teacher", CreatedAt = now };
        var studentRole = new Role { Id = 3, RoleCode = "STUDENT", RoleName = "Student", CreatedAt = now };

        var admin = CreateUser(1, "Admin One", "admin@test.com", adminRole, now.AddDays(-3));
        var teacher = CreateUser(2, "Teacher One", "teacher@test.com", teacherRole, now.AddDays(-2));
        var oldStudent = CreateUser(3, "Student Old", "old@student.test", studentRole, now.AddMonths(-2));
        var newStudent = CreateUser(4, "Student New", "new@student.test", studentRole, now.AddDays(-1));

        var placement = new PlacementTest
        {
            Id = 1,
            Title = "Placement A",
            Description = "Placement test",
            TotalScore = 100,
            Status = "ACTIVE",
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Roles.AddRange(adminRole, teacherRole, studentRole);
        context.Users.AddRange(admin, teacher, oldStudent, newStudent);
        context.PlacementTests.Add(placement);
        context.TestAttempts.AddRange(
            new TestAttempt
            {
                Id = 1,
                PlacementTestId = placement.Id,
                PlacementTest = placement,
                StudentId = newStudent.Id,
                Student = newStudent,
                StartedAt = now.AddDays(-10),
                SubmittedAt = now.AddDays(-10).AddMinutes(20),
                TotalScore = 80,
                Status = "SUBMITTED"
            },
            new TestAttempt
            {
                Id = 2,
                PlacementTestId = placement.Id,
                PlacementTest = placement,
                StudentId = oldStudent.Id,
                Student = oldStudent,
                StartedAt = now.AddDays(-45),
                SubmittedAt = now.AddDays(-45).AddMinutes(20),
                TotalScore = 60,
                Status = "SUBMITTED"
            });

        context.StudentLearningPaths.AddRange(
            new StudentLearningPath { Id = 1, StudentId = newStudent.Id, Student = newStudent, Title = "Active Path", Status = "ACTIVE", CreatedAt = now.AddDays(-8), UpdatedAt = now.AddDays(-1) },
            new StudentLearningPath { Id = 2, StudentId = oldStudent.Id, Student = oldStudent, Title = "Done Path", Status = "COMPLETED", CreatedAt = now.AddDays(-20), UpdatedAt = now.AddDays(-2) },
            new StudentLearningPath { Id = 3, StudentId = oldStudent.Id, Student = oldStudent, Title = "Paused Path", Status = "PAUSED", CreatedAt = now.AddDays(-30), UpdatedAt = now.AddDays(-3) });

        context.Notifications.AddRange(
            new Notification { Id = 1, Title = "Unread", Content = "Unread notice", NotificationType = "SYSTEM", TargetUserId = newStudent.Id, CreatedBy = admin.Id, CreatedAt = now },
            new Notification { Id = 2, Title = "Read", Content = "Read notice", NotificationType = "SYSTEM", TargetUserId = oldStudent.Id, CreatedBy = admin.Id, CreatedAt = now });

        context.NotificationReads.Add(new NotificationRead
        {
            Id = 1,
            NotificationId = 2,
            UserId = oldStudent.Id,
            ReadAt = now
        });
    }

    private static User CreateUser(int id, string fullName, string email, Role role, DateTime createdAt)
    {
        return new User
        {
            Id = id,
            Email = email,
            FullName = fullName,
            PasswordHash = "hash",
            RoleId = role.Id,
            Role = role,
            Status = "ACTIVE",
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };
    }

    private static object CreateAdminDashboardService(ApplicationDbContext context)
    {
        var serviceType = Type.GetType("DuAnTotNghiep.Services.AdminDashboardService, DuAnTotNghiep");
        Assert.NotNull(serviceType);
        return Activator.CreateInstance(serviceType!, context)!;
    }

    private static async Task<object> InvokeAsync(object target, string methodName, params object[] args)
    {
        var method = target.GetType().GetMethod(methodName);
        Assert.NotNull(method);

        var task = (Task)method!.Invoke(target, args)!;
        await task.ConfigureAwait(false);
        var resultProperty = task.GetType().GetProperty("Result");
        Assert.NotNull(resultProperty);
        return resultProperty!.GetValue(task)!;
    }

    private static IEnumerable<object> GetEnumerableProperty(object target, string propertyName)
    {
        return ((System.Collections.IEnumerable)GetProperty(target, propertyName)!).Cast<object>();
    }

    private static int GetIntProperty(object target, string propertyName)
    {
        return Convert.ToInt32(GetProperty(target, propertyName));
    }

    private static decimal GetDecimalProperty(object target, string propertyName)
    {
        return Convert.ToDecimal(GetProperty(target, propertyName));
    }

    private static double GetDoubleProperty(object target, string propertyName)
    {
        return Convert.ToDouble(GetProperty(target, propertyName));
    }

    private static string GetStringProperty(object target, string propertyName)
    {
        return Convert.ToString(GetProperty(target, propertyName)) ?? string.Empty;
    }

    private static object? GetProperty(object target, string propertyName)
    {
        var property = target.GetType().GetProperty(propertyName);
        Assert.NotNull(property);
        return property!.GetValue(target);
    }
}
