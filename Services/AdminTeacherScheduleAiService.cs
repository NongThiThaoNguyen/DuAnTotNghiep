using DuAnTotNghiep.Areas.Admin.ViewModels;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace DuAnTotNghiep.Services;

public class AdminTeacherScheduleAiService : IAdminTeacherScheduleAiService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ApplicationDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AdminTeacherScheduleAiService(
        ApplicationDbContext context,
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _context = context;
        _httpClient = httpClient;
        _configuration = configuration;

        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
        }
    }

    public async Task<AdminTeacherScheduleAiResultViewModel> GenerateSuggestionsAsync(
        AdminTeacherScheduleAiRequestViewModel request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var apiKey = ResolveApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Gemini API key chưa được cấu hình. Hãy đặt GEMINI_API_KEY hoặc GOOGLE_API_KEY trong môi trường/.env.");
        }

        var model = ResolveModel();
        var prompt = await BuildPromptAsync(request, cancellationToken);

        var body = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.2,
                responseMimeType = "application/json"
            }
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"v1beta/models/{Uri.EscapeDataString(model)}:generateContent")
        {
            Content = JsonContent.Create(body)
        };
        httpRequest.Headers.Add("x-goog-api-key", apiKey);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
        response.EnsureSuccessStatusCode();

        var aiText = ExtractGeminiText(responseText);
        var payload = ParseSuggestions(aiText);
        var enrichedSuggestions = await EnrichSuggestionsAsync(payload.Suggestions, request, cancellationToken);

        if (enrichedSuggestions.Count == 0)
        {
            throw new InvalidOperationException("AI chưa trả về gợi ý lịch hợp lệ. Vui lòng thử lại với yêu cầu cụ thể hơn.");
        }

        return new AdminTeacherScheduleAiResultViewModel
        {
            Request = request,
            Suggestions = enrichedSuggestions,
            Summary = payload.Summary,
            Model = model
        };
    }

    private static void ValidateRequest(AdminTeacherScheduleAiRequestViewModel request)
    {
        if (request.ToDate.Date < request.FromDate.Date)
        {
            throw new InvalidOperationException("Ngày kết thúc phải sau hoặc bằng ngày bắt đầu.");
        }

        if (request.SessionsCount <= 0)
        {
            throw new InvalidOperationException("Số buổi cần gợi ý phải lớn hơn 0.");
        }

        if (request.DurationMinutes <= 0)
        {
            throw new InvalidOperationException("Thời lượng mỗi buổi phải lớn hơn 0.");
        }

        if (request.PreferredEndHour <= request.PreferredStartHour)
        {
            throw new InvalidOperationException("Khung giờ kết thúc phải sau khung giờ bắt đầu.");
        }
    }

    private string ResolveApiKey()
    {
        return _configuration["GEMINI_API_KEY"]
            ?? _configuration["GOOGLE_API_KEY"]
            ?? _configuration["GoogleAI:ApiKey"]
            ?? _configuration["AI:ApiKey"]
            ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY")
            ?? Environment.GetEnvironmentVariable("GOOGLE_API_KEY")
            ?? string.Empty;
    }

    private string ResolveModel()
    {
        var configuredModel = _configuration["GoogleAI:Model"];
        if (!string.IsNullOrWhiteSpace(configuredModel))
        {
            return configuredModel;
        }

        var aiModel = _configuration["AI:Model"];
        return !string.IsNullOrWhiteSpace(aiModel) && aiModel.StartsWith("gemini", StringComparison.OrdinalIgnoreCase)
            ? aiModel
            : "gemini-2.0-flash";
    }

    private async Task<string> BuildPromptAsync(AdminTeacherScheduleAiRequestViewModel request, CancellationToken cancellationToken)
    {
        var teachersQuery = _context.Users
            .AsNoTracking()
            .Where(u => u.Status == "ACTIVE" && u.Role.RoleCode == "TEACHER");

        if (request.TeacherId.HasValue)
        {
            teachersQuery = teachersQuery.Where(u => u.Id == request.TeacherId.Value);
        }

        var teachers = await teachersQuery
            .OrderBy(u => u.FullName)
            .Select(u => new { u.Id, u.FullName, u.Email })
            .ToListAsync(cancellationToken);

        if (teachers.Count == 0)
        {
            throw new InvalidOperationException("Không tìm thấy giáo viên hợp lệ cho yêu cầu gợi ý lịch.");
        }

        var topicsQuery = _context.LearningTopics
            .AsNoTracking()
            .Where(t => t.Status == "ACTIVE");

        if (request.TopicId.HasValue)
        {
            topicsQuery = topicsQuery.Where(t => t.Id == request.TopicId.Value);
        }

        var topics = await topicsQuery
            .OrderBy(t => t.Title)
            .Select(t => new { t.Id, t.Title })
            .Take(20)
            .ToListAsync(cancellationToken);

        var from = request.FromDate.Date;
        var toExclusive = request.ToDate.Date.AddDays(1);
        var existingSchedules = await _context.Schedules
            .AsNoTracking()
            .Include(s => s.Teacher)
            .Include(s => s.Topic)
            .Where(s => s.StartTime >= from && s.StartTime < toExclusive)
            .Where(s => !request.TeacherId.HasValue || s.TeacherId == request.TeacherId.Value)
            .OrderBy(s => s.StartTime)
            .Select(s => new
            {
                s.Title,
                Teacher = s.Teacher.FullName,
                Topic = s.Topic != null ? s.Topic.Title : "",
                s.StartTime,
                s.EndTime,
                s.Classroom
            })
            .ToListAsync(cancellationToken);

        var builder = new StringBuilder();
        builder.AppendLine("You are an academic scheduling assistant for an English learning admin dashboard.");
        builder.AppendLine("Suggest teacher class schedules. Avoid overlapping existing schedules for the same teacher and prefer reasonable teaching gaps.");
        builder.AppendLine("Return JSON only. Do not wrap it in markdown.");
        builder.AppendLine("JSON schema:");
        builder.AppendLine("{\"summary\":\"short Vietnamese summary\",\"suggestions\":[{\"title\":\"string\",\"teacherId\":10,\"topicId\":200,\"startTime\":\"2026-07-03T09:00:00\",\"endTime\":\"2026-07-03T10:00:00\",\"classroom\":\"A101\",\"description\":\"string\",\"reason\":\"Vietnamese explanation\"}]}");
        builder.AppendLine();
        builder.AppendLine("Request:");
        builder.AppendLine($"- Date range: {request.FromDate:yyyy-MM-dd} to {request.ToDate:yyyy-MM-dd}");
        builder.AppendLine($"- Sessions needed: {request.SessionsCount}");
        builder.AppendLine($"- Duration minutes per session: {request.DurationMinutes}");
        builder.AppendLine($"- Preferred daily time window: {request.PreferredStartHour:00}:00 - {request.PreferredEndHour:00}:00");
        builder.AppendLine($"- Preferred classroom: {request.Classroom ?? "not specified"}");
        builder.AppendLine($"- Notes: {request.Notes ?? "none"}");
        builder.AppendLine();
        builder.AppendLine("Available teachers:");
        foreach (var teacher in teachers)
        {
            builder.AppendLine($"- {teacher.Id}: {teacher.FullName} ({teacher.Email})");
        }
        builder.AppendLine();
        builder.AppendLine("Available topics:");
        if (topics.Count == 0)
        {
            builder.AppendLine("- No topic selected; suggestions may use null topicId.");
        }
        else
        {
            foreach (var topic in topics)
            {
                builder.AppendLine($"- {topic.Id}: {topic.Title}");
            }
        }
        builder.AppendLine();
        builder.AppendLine("Existing schedules:");
        if (existingSchedules.Count == 0)
        {
            builder.AppendLine("- none");
        }
        else
        {
            foreach (var schedule in existingSchedules)
            {
                builder.AppendLine($"- {schedule.StartTime:yyyy-MM-dd HH:mm} to {schedule.EndTime:HH:mm}: {schedule.Title}; teacher={schedule.Teacher}; topic={schedule.Topic}; room={schedule.Classroom}");
            }
        }

        return builder.ToString();
    }

    private static string ExtractGeminiText(string responseText)
    {
        using var doc = JsonDocument.Parse(responseText);
        var root = doc.RootElement;
        var candidates = root.GetProperty("candidates");
        if (candidates.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("AI không trả về phản hồi.");
        }

        var parts = candidates[0].GetProperty("content").GetProperty("parts");
        foreach (var part in parts.EnumerateArray())
        {
            if (part.TryGetProperty("text", out var textElement))
            {
                return textElement.GetString() ?? string.Empty;
            }
        }

        throw new InvalidOperationException("AI không trả về nội dung văn bản.");
    }

    private static AiSuggestionPayload ParseSuggestions(string aiText)
    {
        var json = ExtractJsonPayload(aiText);

        if (json.TrimStart().StartsWith("[", StringComparison.Ordinal))
        {
            var list = JsonSerializer.Deserialize<List<AiSuggestionDto>>(json, JsonOptions) ?? new();
            return new AiSuggestionPayload { Suggestions = list };
        }

        return JsonSerializer.Deserialize<AiSuggestionPayload>(json, JsonOptions)
            ?? new AiSuggestionPayload();
    }

    private static string ExtractJsonPayload(string aiText)
    {
        var text = aiText.Trim();
        if (text.StartsWith("```", StringComparison.Ordinal))
        {
            var firstNewLine = text.IndexOf('\n');
            var lastFence = text.LastIndexOf("```", StringComparison.Ordinal);
            if (firstNewLine >= 0 && lastFence > firstNewLine)
            {
                text = text[(firstNewLine + 1)..lastFence].Trim();
            }
        }

        return text;
    }

    private async Task<List<AdminTeacherScheduleAiSuggestionViewModel>> EnrichSuggestionsAsync(
        List<AiSuggestionDto>? suggestions,
        AdminTeacherScheduleAiRequestViewModel request,
        CancellationToken cancellationToken)
    {
        if (suggestions == null || suggestions.Count == 0)
        {
            return new List<AdminTeacherScheduleAiSuggestionViewModel>();
        }

        var teacherIds = suggestions
            .Select(s => s.TeacherId ?? request.TeacherId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var topicIds = suggestions
            .Select(s => s.TopicId ?? request.TopicId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var teachers = await _context.Users
            .AsNoTracking()
            .Where(u => teacherIds.Contains(u.Id) && u.Status == "ACTIVE" && u.Role.RoleCode == "TEACHER")
            .Select(u => new { u.Id, u.FullName, u.Email })
            .ToDictionaryAsync(u => u.Id, cancellationToken);

        var topics = await _context.LearningTopics
            .AsNoTracking()
            .Where(t => topicIds.Contains(t.Id) && t.Status == "ACTIVE")
            .Select(t => new { t.Id, t.Title })
            .ToDictionaryAsync(t => t.Id, cancellationToken);

        var result = new List<AdminTeacherScheduleAiSuggestionViewModel>();
        foreach (var suggestion in suggestions)
        {
            var teacherId = suggestion.TeacherId ?? request.TeacherId;
            if (!teacherId.HasValue || !teachers.TryGetValue(teacherId.Value, out var teacher))
            {
                continue;
            }

            var topicId = suggestion.TopicId ?? request.TopicId;
            string topicName = "Không đính kèm";
            if (topicId.HasValue)
            {
                if (!topics.TryGetValue(topicId.Value, out var topic))
                {
                    continue;
                }

                topicName = topic.Title;
            }

            if (suggestion.EndTime <= suggestion.StartTime || string.IsNullOrWhiteSpace(suggestion.Title))
            {
                continue;
            }

            result.Add(new AdminTeacherScheduleAiSuggestionViewModel
            {
                Title = suggestion.Title.Trim(),
                TeacherId = teacher.Id,
                TeacherName = teacher.FullName,
                TeacherEmail = teacher.Email,
                TopicId = topicId,
                TopicName = topicName,
                StartTime = suggestion.StartTime,
                EndTime = suggestion.EndTime,
                Classroom = string.IsNullOrWhiteSpace(suggestion.Classroom) ? request.Classroom : suggestion.Classroom,
                Description = suggestion.Description,
                Reason = suggestion.Reason ?? "AI đề xuất dựa trên khung giờ và lịch hiện có."
            });
        }

        return result;
    }

    private sealed class AiSuggestionPayload
    {
        public string? Summary { get; set; }
        public List<AiSuggestionDto>? Suggestions { get; set; } = new();
    }

    private sealed class AiSuggestionDto
    {
        public string? Title { get; set; }
        public int? TeacherId { get; set; }
        public int? TopicId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Classroom { get; set; }
        public string? Description { get; set; }
        public string? Reason { get; set; }
    }
}
