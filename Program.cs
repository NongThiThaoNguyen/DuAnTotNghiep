using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models.Repositories.Interfaces;
using DuAnTotNghiep.Models.Repositories;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.Services;
using DuAnTotNghiep.Services.AI;
using DuAnTotNghiep.Services.Validators;
using DuAnTotNghiep.Data.Seeders;
using DuAnTotNghiep.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);
Console.WriteLine("CONNECTION STRING IS: " + builder.Configuration.GetConnectionString("DefaultConnection"));

// Load .env file
DotNetEnv.Env.Load();
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<DuAnTotNghiep.Filters.RequireOnboardingFilter>();
    options.Filters.Add<DuAnTotNghiep.Filters.RequirePlacementTestFilter>();
});
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpContextAccessor();
builder.Services.Configure<AiProviderSettings>(builder.Configuration.GetSection("AI"));
builder.Services.AddHttpClient("AiProvider", client =>
{
    var endpoint = builder.Configuration["AI:Endpoint"];
    if (!string.IsNullOrWhiteSpace(endpoint))
    {
        client.BaseAddress = new Uri(endpoint);
    }
});

// Đăng ký Generic Repository
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Đăng ký Specific Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<ILoginLogRepository, LoginLogRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
builder.Services.AddScoped<IUserSessionRepository, UserSessionRepository>();
builder.Services.AddScoped<ILearningProfileRepository, LearningProfileRepository>();
builder.Services.AddScoped<ILearningTopicRepository, LearningTopicRepository>();
builder.Services.AddScoped<IReferenceSourceRepository, ReferenceSourceRepository>();
builder.Services.AddScoped<IEnglishSkillRepository, EnglishSkillRepository>();
builder.Services.AddScoped<IEnglishProficiencyLevelRepository, EnglishProficiencyLevelRepository>();
builder.Services.AddScoped<ILearningObjectiveRepository, LearningObjectiveRepository>();
builder.Services.AddScoped<IProgressRepository, ProgressRepository>();
builder.Services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
builder.Services.AddScoped<ILearningPathRepository, LearningPathRepository>();
builder.Services.AddScoped<ICompetencyAnalysisRepository, CompetencyAnalysisRepository>();
builder.Services.AddScoped<DuAnTotNghiep.Models.Repositories.Interfaces.ITopicPrerequisiteRepository, DuAnTotNghiep.Models.Repositories.TopicPrerequisiteRepository>();

// Đăng ký Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<IUserSessionService, UserSessionService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<DuAnTotNghiep.Services.Interfaces.IUserProfileService, DuAnTotNghiep.Services.UserProfileService>();
builder.Services.AddScoped<ILearningProfileService, LearningProfileService>();
builder.Services.AddScoped<IPlacementTestService, PlacementTestService>();
builder.Services.AddScoped<IMasterDataService, MasterDataService>();
builder.Services.AddScoped<ITestScoringService, TestScoringService>();
builder.Services.AddScoped<IPlacementTestManagementService, PlacementTestManagementService>();
builder.Services.AddScoped<IPlacementAttemptService, PlacementAttemptService>();
builder.Services.AddScoped<IPlacementTestSectionService, PlacementTestSectionService>();
builder.Services.AddScoped<IPlacementTestQuestionService, PlacementTestQuestionService>();
builder.Services.AddScoped<IPlacementTestValidationService, PlacementTestValidationService>();
builder.Services.AddScoped<IPlacementRequirementService, PlacementRequirementService>();
builder.Services.AddScoped<ILearningTopicService, LearningTopicService>();
builder.Services.AddScoped<IReferenceSourceService, ReferenceSourceService>();
builder.Services.AddScoped<IValidateLicenseService, ValidateLicenseService>();
builder.Services.AddScoped<IReferenceSourcePolicyService, ReferenceSourcePolicyService>();
builder.Services.AddScoped<ITopicImportService, TopicImportService>();
builder.Services.AddScoped<ILearningObjectiveService, LearningObjectiveService>();
builder.Services.AddScoped<IM4ValidationService, M4ValidationService>();
builder.Services.AddScoped<IEnglishSkillService, EnglishSkillService>();
builder.Services.AddScoped<IEnglishProficiencyLevelService, EnglishProficiencyLevelService>();
builder.Services.AddScoped<IStudentProgressService, StudentProgressService>();
builder.Services.AddScoped<IProgressTrackingService, ProgressTrackingService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IPathViewService, PathViewService>();
builder.Services.AddScoped<ILearningPathEngineService, LearningPathEngineService>();
builder.Services.AddScoped<ILearningPathAiService, LearningPathAiService>();
builder.Services.AddScoped<ILearningPathComplianceService, LearningPathComplianceService>();
builder.Services.AddScoped<DuAnTotNghiep.Services.IPromptTemplateService, PromptTemplateService>();
builder.Services.AddScoped<DuAnTotNghiep.Services.Interfaces.IPromptTemplateService, PromptTemplateService>();
builder.Services.AddScoped<ITaxonomyService, TaxonomyService>();
builder.Services.AddScoped<IAIContentReviewService, AIContentReviewService>();
builder.Services.AddScoped<IPublishingService, PublishingService>();
builder.Services.AddScoped<IReplanningRuleService, ReplanningRuleService>();
builder.Services.AddScoped<IResponseValidator, ResponseValidator>();
builder.Services.AddScoped<IAssessmentAIService, AssessmentAIService>();
builder.Services.AddScoped<ICompetencyPersistenceService, CompetencyPersistenceService>();
builder.Services.AddScoped<ITestResultAggregatorService, TestResultAggregatorService>();

// AI services
builder.Services.AddHttpClient<IAIProvider, OpenAIProvider>();
builder.Services.AddScoped<AIQuizGenerationService>();
builder.Services.AddScoped<AiOutputSchemaValidator>();
builder.Services.AddScoped<AiUsageLogService>();

// Đăng ký M7 AI Analysis Services
builder.Services.AddScoped<IPlacementTestAnalysisPayloadBuilder, PlacementTestAnalysisPayloadBuilder>();
builder.Services.AddScoped<ICompetencyAnalysisService, CompetencyAnalysisService>();
builder.Services.AddSingleton<DuAnTotNghiep.Services.Background.IAiAnalysisQueue, DuAnTotNghiep.Services.Background.AiAnalysisQueue>();
builder.Services.AddHostedService<DuAnTotNghiep.Services.Background.AiAnalysisBackgroundService>();
builder.Services.AddHostedService<DuAnTotNghiep.Services.ProgressSnapshotBackgroundService>();
builder.Services.AddScoped<ICompetencyScoreCalculatorService, CompetencyScoreCalculatorService>();
builder.Services.AddScoped<ICompetencyFeedbackService, CompetencyFeedbackService>();
builder.Services.AddScoped<ICompetencyAnalysisOrchestrator, CompetencyAnalysisOrchestrator>();
builder.Services.AddSingleton<IAiLoggingService, AiLoggingService>();

// Đăng ký Seeder
builder.Services.AddScoped<DatabaseSeeder>();
builder.Services.AddScoped<AILearnSeeder>();
builder.Services.AddScoped<DuAnTotNghiep.Services.Interfaces.IM4SchemaService, DuAnTotNghiep.Services.M4SchemaService>();

// Đăng ký Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromDays(1);
        options.SlidingExpiration = true;
        options.Events = new CookieAuthenticationEvents
        {
            OnValidatePrincipal = async context =>
            {
                var userPrincipal = context.Principal;
                var sessionToken = userPrincipal?.FindFirst("SessionToken")?.Value;

                if (string.IsNullOrEmpty(sessionToken))
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    return;
                }

                var dbContext = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
                var session = await dbContext.UserSessions.FirstOrDefaultAsync(s => s.SessionToken == sessionToken);

                if (session == null || session.RevokedAt != null || session.ExpiresAt < DateTime.UtcNow)
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }
            }
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Chạy Database Seeder
using (var scope = app.Services.CreateScope())
{
    try
    {
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();

        var aiSeeder = scope.ServiceProvider.GetRequiredService<AILearnSeeder>();
        await aiSeeder.SeedAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during database seeding: {ex.Message}");
    }
}

app.Run();
