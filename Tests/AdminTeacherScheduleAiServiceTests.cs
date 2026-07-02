using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Reflection;

namespace DuAnTotNghiep.Tests;

public class AdminTeacherScheduleAiServiceTests
{
    [Fact]
    public async Task GenerateSuggestionsAsync_CallsGeminiAndParsesSuggestions()
    {
        var options = CreateOptions();
        var targetDate = new DateTime(2026, 7, 3);

        using (var context = new ApplicationDbContext(options))
        {
            SeedScheduleData(context, targetDate);
            await context.SaveChangesAsync();
        }

        var handler = new CapturingHandler("""
            {
              "candidates": [
                {
                  "content": {
                    "parts": [
                      {
                        "text": "{\"suggestions\":[{\"title\":\"AI Speaking Practice\",\"teacherId\":10,\"topicId\":200,\"startTime\":\"2026-07-03T09:00:00\",\"endTime\":\"2026-07-03T10:00:00\",\"classroom\":\"A201\",\"reason\":\"Tránh trùng lịch buổi sáng và giữ khung giờ ưu tiên.\"}]}"
                      }
                    ]
                  }
                }
              ],
              "usageMetadata": { "promptTokenCount": 12, "candidatesTokenCount": 18 }
            }
            """);
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://generativelanguage.googleapis.com/")
        };
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GoogleAI:ApiKey"] = "redacted-test-key",
                ["GoogleAI:Model"] = "gemini-test"
            })
            .Build();

        using (var context = new ApplicationDbContext(options))
        {
            var service = CreateAiService(context, httpClient, config);
            var request = CreateRequest(targetDate);

            var result = await InvokeAsync(service, "GenerateSuggestionsAsync", request, CancellationToken.None);
            var suggestions = GetEnumerableProperty(result, "Suggestions").ToList();
            var suggestion = Assert.Single(suggestions);

            Assert.Equal("AI Speaking Practice", GetStringProperty(suggestion, "Title"));
            Assert.Equal(10, GetIntProperty(suggestion, "TeacherId"));
            Assert.Equal(200, GetNullableIntProperty(suggestion, "TopicId"));
            Assert.Equal(targetDate.AddHours(9), GetDateTimeProperty(suggestion, "StartTime"));
            Assert.Equal(targetDate.AddHours(10), GetDateTimeProperty(suggestion, "EndTime"));
            Assert.Equal("A201", GetStringProperty(suggestion, "Classroom"));
            Assert.Contains("Tránh trùng", GetStringProperty(suggestion, "Reason"));
        }

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Contains("models/gemini-test:generateContent", handler.LastRequest?.RequestUri?.ToString());
        Assert.NotNull(handler.LastRequest);
        Assert.True(handler.LastRequest!.Headers.TryGetValues("x-goog-api-key", out var keyValues));
        Assert.Equal("redacted-test-key", Assert.Single(keyValues!));
        Assert.Contains("Existing schedules", handler.LastRequestBody);
        Assert.Contains("Existing class", handler.LastRequestBody);
        Assert.Contains("JSON", handler.LastRequestBody);
    }

    [Fact]
    public async Task GenerateSuggestionsAsync_WhenApiKeyMissing_ThrowsHelpfulError()
    {
        var options = CreateOptions();
        using var context = new ApplicationDbContext(options);
        var handler = new CapturingHandler("{}");
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://generativelanguage.googleapis.com/")
        };
        var config = new ConfigurationBuilder().Build();
        var service = CreateAiService(context, httpClient, config);
        var request = CreateRequest(new DateTime(2026, 7, 3));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => InvokeAsync(service, "GenerateSuggestionsAsync", request, CancellationToken.None));

        Assert.Contains("Gemini API key", exception.Message);
    }

    private static DbContextOptions<ApplicationDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    private static void SeedScheduleData(ApplicationDbContext context, DateTime targetDate)
    {
        var teacherRole = new Role { Id = 2, RoleCode = "TEACHER", RoleName = "Teacher" };
        var teacher = new User
        {
            Id = 10,
            Email = "teacher@test.com",
            FullName = "Nguyen An",
            PasswordHash = "hash",
            RoleId = teacherRole.Id,
            Role = teacherRole,
            Status = "ACTIVE"
        };
        var skill = new EnglishSkill
        {
            Id = 1,
            SkillCode = "ENG",
            SkillName = "English",
            IsActive = true
        };
        var topic = new LearningTopic
        {
            Id = 200,
            SkillId = skill.Id,
            Skill = skill,
            Title = "Speaking",
            DifficultyLevel = "BEGINNER",
            Status = "ACTIVE",
            CreatedBy = teacher.Id
        };

        context.Roles.Add(teacherRole);
        context.Users.Add(teacher);
        context.EnglishSkills.Add(skill);
        context.LearningTopics.Add(topic);
        context.Schedules.Add(new Schedule
        {
            Id = 100,
            TeacherId = teacher.Id,
            Teacher = teacher,
            TopicId = topic.Id,
            Topic = topic,
            Title = "Existing class",
            StartTime = targetDate.AddHours(8),
            EndTime = targetDate.AddHours(9),
            Classroom = "A101"
        });
    }

    private static object CreateAiService(ApplicationDbContext context, HttpClient httpClient, IConfiguration config)
    {
        var serviceType = Type.GetType("DuAnTotNghiep.Services.AdminTeacherScheduleAiService, DuAnTotNghiep");
        Assert.NotNull(serviceType);
        return Activator.CreateInstance(serviceType!, context, httpClient, config)!;
    }

    private static object CreateRequest(DateTime targetDate)
    {
        var requestType = Type.GetType("DuAnTotNghiep.Areas.Admin.ViewModels.AdminTeacherScheduleAiRequestViewModel, DuAnTotNghiep");
        Assert.NotNull(requestType);
        var request = Activator.CreateInstance(requestType!)!;

        SetProperty(request, "TeacherId", 10);
        SetProperty(request, "TopicId", 200);
        SetProperty(request, "FromDate", targetDate);
        SetProperty(request, "ToDate", targetDate);
        SetProperty(request, "SessionsCount", 1);
        SetProperty(request, "DurationMinutes", 60);
        SetProperty(request, "PreferredStartHour", 8);
        SetProperty(request, "PreferredEndHour", 11);
        SetProperty(request, "Classroom", "A201");
        SetProperty(request, "Notes", "Ưu tiên buổi sáng");

        return request;
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

    private static int? GetNullableIntProperty(object target, string propertyName)
    {
        return (int?)target.GetType().GetProperty(propertyName)!.GetValue(target);
    }

    private static string GetStringProperty(object target, string propertyName)
    {
        return (string)target.GetType().GetProperty(propertyName)!.GetValue(target)!;
    }

    private static DateTime GetDateTimeProperty(object target, string propertyName)
    {
        return (DateTime)target.GetType().GetProperty(propertyName)!.GetValue(target)!;
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        target.GetType().GetProperty(propertyName)!.SetValue(target, value);
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        private readonly string _responseBody;

        public CapturingHandler(string responseBody)
        {
            _responseBody = responseBody;
        }

        public HttpRequestMessage? LastRequest { get; private set; }
        public string LastRequestBody { get; private set; } = string.Empty;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            LastRequestBody = request.Content == null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_responseBody)
            };
        }
    }
}
