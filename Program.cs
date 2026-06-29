using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Repositories.Interfaces;
using DuAnTotNghiep.Repositories;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.Services;
using DuAnTotNghiep.Data.Seeders;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<DuAnTotNghiep.Filters.RequireOnboardingFilter>();
    options.Filters.Add<DuAnTotNghiep.Filters.RequirePlacementTestFilter>();
});
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpContextAccessor();

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

// Đăng ký M7 AI Analysis Services
builder.Services.AddScoped<IPlacementTestAnalysisPayloadBuilder, PlacementTestAnalysisPayloadBuilder>();
builder.Services.AddScoped<ICompetencyAnalysisService, CompetencyAnalysisService>();
builder.Services.AddSingleton<DuAnTotNghiep.Services.Background.IAiAnalysisQueue, DuAnTotNghiep.Services.Background.AiAnalysisQueue>();
builder.Services.AddHostedService<DuAnTotNghiep.Services.Background.AiAnalysisBackgroundService>();
builder.Services.AddHostedService<DuAnTotNghiep.Services.ProgressSnapshotBackgroundService>();

builder.Services.AddScoped<ITestResultAggregatorService, TestResultAggregatorService>();
// M7 - AI Assessment Pipeline
builder.Services.AddScoped<ICompetencyScoreCalculatorService, CompetencyScoreCalculatorService>();
builder.Services.AddScoped<ICompetencyFeedbackService, CompetencyFeedbackService>();
builder.Services.AddScoped<ICompetencyAnalysisOrchestrator, CompetencyAnalysisOrchestrator>();
builder.Services.AddSingleton<IAiLoggingService, AiLoggingService>(); // Singleton since it uses IServiceScopeFactory internally

// Đăng ký Seeder
builder.Services.AddScoped<DatabaseSeeder>();
builder.Services.AddScoped<DuAnTotNghiep.Repositories.Interfaces.ITopicPrerequisiteRepository, DuAnTotNghiep.Repositories.TopicPrerequisiteRepository>();
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
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();

}

app.Run();
