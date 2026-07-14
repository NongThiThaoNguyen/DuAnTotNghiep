using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Data.Seeders;

public class AdminDataSeeder
{
    private readonly ApplicationDbContext _context;

    public AdminDataSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        await SeedSystemSettingsAsync();
        await SeedAiPromptTemplatesAsync();
        await SeedNotificationsAsync();
        await SeedAuditLogsAsync();
        await SeedAiUsageLogsAsync();
        await SeedLearningPathTemplatesAsync();
    }

    private async Task SeedSystemSettingsAsync()
    {
        var settings = new List<SystemSetting>
        {
            new SystemSetting { SettingKey = "MaxLoginAttempts", SettingValue = "5", Description = "Maximum failed login attempts before lockout", UpdatedAt = DateTime.UtcNow },
            new SystemSetting { SettingKey = "SessionTimeout", SettingValue = "60", Description = "Session timeout in minutes", UpdatedAt = DateTime.UtcNow },
            new SystemSetting { SettingKey = "DefaultAiModel", SettingValue = "gemini-1.5-pro", Description = "Default AI model for text generation", UpdatedAt = DateTime.UtcNow },
            new SystemSetting { SettingKey = "MaintenanceMode", SettingValue = "false", Description = "Enable maintenance mode", UpdatedAt = DateTime.UtcNow },
            new SystemSetting { SettingKey = "MaxDailyAiTokens", SettingValue = "100000", Description = "Maximum AI tokens allowed per user per day", UpdatedAt = DateTime.UtcNow }
        };

        foreach (var setting in settings)
        {
            if (!await _context.SystemSettings.AnyAsync(s => s.SettingKey == setting.SettingKey))
            {
                _context.SystemSettings.Add(setting);
            }
        }
        await _context.SaveChangesAsync();
    }

    private async Task SeedAiPromptTemplatesAsync()
    {
        var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleCode == "ADMIN");
        var adminUser = adminRole != null ? await _context.Users.FirstOrDefaultAsync(u => u.RoleId == adminRole.Id) : null;
        var adminId = adminUser?.Id;

        var templates = new List<AiPromptTemplate>
        {
            new AiPromptTemplate { PromptCode = "QUIZ_GENERATION", PromptName = "Tạo Quiz từ văn bản", SystemPrompt = "Tạo 5 câu hỏi trắc nghiệm từ văn bản sau: {{text}}", Status = "ACTIVE", CreatedBy = adminId, CreatedAt = DateTime.UtcNow },
            new AiPromptTemplate { PromptCode = "WRITING_EVALUATION", PromptName = "Chấm điểm bài viết", SystemPrompt = "Đánh giá bài viết sau theo tiêu chí IELTS (Lexical Resource, Grammatical Range). Bài viết: {{essay}}", Status = "ACTIVE", CreatedBy = adminId, CreatedAt = DateTime.UtcNow },
            new AiPromptTemplate { PromptCode = "SUMMARIZE_TEXT", PromptName = "Tóm tắt nội dung", SystemPrompt = "Tóm tắt đoạn văn sau trong 3 câu: {{content}}", Status = "ACTIVE", CreatedBy = adminId, CreatedAt = DateTime.UtcNow }
        };

        foreach (var template in templates)
        {
            if (!await _context.AiPromptTemplates.AnyAsync(t => t.PromptCode == template.PromptCode))
            {
                _context.AiPromptTemplates.Add(template);
            }
        }
        await _context.SaveChangesAsync();
    }

    private async Task SeedNotificationsAsync()
    {
        if (!await _context.Notifications.AnyAsync())
        {
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleCode == "ADMIN");
            var adminUser = adminRole != null ? await _context.Users.FirstOrDefaultAsync(u => u.RoleId == adminRole.Id) : null;
            var adminId = adminUser?.Id;

            var notifications = new List<Notification>
            {
                new Notification { Title = "Bảo trì hệ thống", Content = "Hệ thống sẽ bảo trì từ 2h-4h sáng ngày mai.", NotificationType = "SYSTEM", CreatedAt = DateTime.UtcNow },
                new Notification { Title = "Cập nhật khóa học mới", Content = "Khóa học IELTS Speaking Advanced đã được phát hành.", NotificationType = "ANNOUNCEMENT", CreatedBy = adminId, CreatedAt = DateTime.UtcNow }
            };

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedAuditLogsAsync()
    {
        if (!await _context.AuditLogs.AnyAsync())
        {
            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "admin@aistudyenglish.com");
            var adminId = adminUser?.Id ?? 1;

            var logs = new List<AuditLog>
            {
                new AuditLog { UserId = adminId, Action = "LOGIN", EntityName = "User", EntityId = adminId, NewValue = "Admin logged in successfully", IpAddress = "127.0.0.1", CreatedAt = DateTime.UtcNow.AddDays(-1) },
                new AuditLog { UserId = adminId, Action = "UPDATE_SETTINGS", EntityName = "SystemSetting", EntityId = 1, NewValue = "Updated MaxLoginAttempts", IpAddress = "127.0.0.1", CreatedAt = DateTime.UtcNow.AddHours(-10) },
                new AuditLog { UserId = adminId, Action = "CREATE_USER", EntityName = "User", EntityId = 100, NewValue = "Created new student account", IpAddress = "127.0.0.1", CreatedAt = DateTime.UtcNow.AddHours(-5) }
            };

            _context.AuditLogs.AddRange(logs);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedAiUsageLogsAsync()
    {
        if (!await _context.AiUsageLogs.AnyAsync())
        {
            var student = await _context.Users.FirstOrDefaultAsync(u => u.Email == "student1@aistudyenglish.com");
            var teacher = await _context.Users.FirstOrDefaultAsync(u => u.Email == "teacher@aistudyenglish.com");

            var logs = new List<AiUsageLog>
            {
                new AiUsageLog { UserId = student?.Id ?? 1, ModuleCode = "CHAT_TUTOR", AiModel = "gemini-1.5-pro", InputTokens = 50, OutputTokens = 120, PromptInput = "...", ResponseOutput = "...", RequestStatus = "SUCCESS", CreatedAt = DateTime.UtcNow.AddDays(-2) },
                new AiUsageLog { UserId = teacher?.Id ?? 2, ModuleCode = "GENERATE_QUIZ", AiModel = "gemini-1.5-pro", InputTokens = 200, OutputTokens = 500, PromptInput = "...", ResponseOutput = "...", RequestStatus = "SUCCESS", CreatedAt = DateTime.UtcNow.AddDays(-1) },
                new AiUsageLog { UserId = student?.Id ?? 1, ModuleCode = "ASSESS_WRITING", AiModel = "gemini-1.5-pro", InputTokens = 300, OutputTokens = 250, PromptInput = "...", ResponseOutput = "...", RequestStatus = "SUCCESS", CreatedAt = DateTime.UtcNow.AddHours(-5) }
            };

            _context.AiUsageLogs.AddRange(logs);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedLearningPathTemplatesAsync()
    {
        if (!await _context.LearningPathTemplates.AnyAsync())
        {
            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "admin@aistudyenglish.com");
            var adminId = adminUser?.Id ?? 1;

            var templates = new List<LearningPathTemplate>
            {
                new LearningPathTemplate { TemplateName = "Nền tảng IELTS 5.0", Description = "Lộ trình 3 tháng cho người mới bắt đầu luyện IELTS", DurationWeeks = 12, Status = "ACTIVE", CreatedBy = adminId, CreatedAt = DateTime.UtcNow },
                new LearningPathTemplate { TemplateName = "Giao tiếp cơ bản", Description = "Lộ trình giao tiếp thực dụng 30 ngày", DurationWeeks = 4, Status = "ACTIVE", CreatedBy = adminId, CreatedAt = DateTime.UtcNow }
            };

            _context.LearningPathTemplates.AddRange(templates);
            await _context.SaveChangesAsync();

            var nodes = new List<LearningPathTemplateNode>
            {
                new LearningPathTemplateNode { TemplateId = templates[0].Id, NodeTitle = "Tuần 1-4: Từ vựng & Ngữ pháp lõi", NodeType = "TOPIC", OrderIndex = 1 },
                new LearningPathTemplateNode { TemplateId = templates[0].Id, NodeTitle = "Tuần 5-8: Kỹ năng Đọc Nghe", NodeType = "TOPIC", OrderIndex = 2 },
                new LearningPathTemplateNode { TemplateId = templates[0].Id, NodeTitle = "Tuần 9-12: Kỹ năng Nói Viết", NodeType = "TOPIC", OrderIndex = 3 }
            };
            
            _context.LearningPathTemplateNodes.AddRange(nodes);
            await _context.SaveChangesAsync();
        }
    }
}

