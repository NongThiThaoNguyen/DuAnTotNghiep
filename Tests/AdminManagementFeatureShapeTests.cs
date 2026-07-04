using System.Reflection;

namespace DuAnTotNghiep.Tests;

public class AdminManagementFeatureShapeTests
{
    private static readonly string[] ControllerNames =
    [
        "StudentAttendanceController",
        "StudentQuizController",
        "StudentAssignmentController",
        "TeacherManagementController",
        "TeacherResourceAdminController",
        "QuizManagementController",
        "AssignmentManagementController",
        "ReportsController",
        "ChatMonitorController"
    ];

    private static readonly Dictionary<string, string[]> RequiredActions = new()
    {
        ["StudentAttendanceController"] = ["Index", "ByTopic", "Export"],
        ["StudentQuizController"] = ["Index", "Details"],
        ["StudentAssignmentController"] = ["Index", "Details"],
        ["TeacherManagementController"] = ["Index", "Profile", "Performance"],
        ["TeacherResourceAdminController"] = ["Index", "Approve", "Reject", "Delete"],
        ["QuizManagementController"] = ["Index", "Details", "Attempts", "Delete"],
        ["AssignmentManagementController"] = ["Index", "Submissions", "Delete"],
        ["ReportsController"] = ["Index", "StudentProgress", "TeacherActivity", "AttendanceSummary", "QuizPerformance"],
        ["ChatMonitorController"] = ["Index", "TeacherChats", "AiTutorSessions"]
    };

    private static readonly string[] ServiceTypeNames =
    [
        "IAdminTeacherManagementService",
        "AdminTeacherManagementService",
        "IAdminQuizManagementService",
        "AdminQuizManagementService",
        "IAdminReportService",
        "AdminReportService",
        "IAdminChatMonitorService",
        "AdminChatMonitorService"
    ];

    private static readonly string[] RequiredViewFiles =
    [
        "StudentAttendance/Index.cshtml",
        "StudentQuiz/Index.cshtml",
        "StudentQuiz/Details.cshtml",
        "StudentAssignment/Index.cshtml",
        "StudentAssignment/Details.cshtml",
        "TeacherManagement/Index.cshtml",
        "TeacherManagement/Profile.cshtml",
        "TeacherManagement/Performance.cshtml",
        "TeacherResourceAdmin/Index.cshtml",
        "QuizManagement/Index.cshtml",
        "QuizManagement/Details.cshtml",
        "QuizManagement/Attempts.cshtml",
        "AssignmentManagement/Index.cshtml",
        "AssignmentManagement/Submissions.cshtml",
        "Reports/Index.cshtml",
        "Reports/StudentProgress.cshtml",
        "Reports/TeacherActivity.cshtml",
        "Reports/AttendanceSummary.cshtml",
        "Reports/QuizPerformance.cshtml",
        "ChatMonitor/Index.cshtml",
        "ChatMonitor/TeacherChats.cshtml",
        "ChatMonitor/AiTutorSessions.cshtml"
    ];

    [Fact]
    public void AdminManagementControllersExposeRequiredActions()
    {
        var assembly = typeof(Program).Assembly;

        foreach (var controllerName in ControllerNames)
        {
            var controllerType = assembly.GetType($"DuAnTotNghiep.Areas.Admin.Controllers.{controllerName}");
            Assert.NotNull(controllerType);

            foreach (var actionName in RequiredActions[controllerName])
            {
                Assert.Contains(controllerType!.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly), method => method.Name == actionName);
            }
        }
    }

    [Fact]
    public void AdminManagementServicesExist()
    {
        var assembly = typeof(Program).Assembly;

        foreach (var typeName in ServiceTypeNames)
        {
            var fullName = typeName.StartsWith("I", StringComparison.Ordinal)
                ? $"DuAnTotNghiep.Services.Interfaces.{typeName}"
                : $"DuAnTotNghiep.Services.{typeName}";

            Assert.NotNull(assembly.GetType(fullName));
        }
    }

    [Fact]
    public void AdminManagementViewsExist()
    {
        var projectRoot = GetProjectRoot();

        foreach (var viewFile in RequiredViewFiles)
        {
            var fullPath = Path.Combine(projectRoot, "Areas", "Admin", "Views", viewFile.Replace('/', Path.DirectorySeparatorChar));
            Assert.True(File.Exists(fullPath), $"Missing admin view: {viewFile}");
        }
    }

    private static string GetProjectRoot()
    {
        var current = AppContext.BaseDirectory;

        while (!string.IsNullOrEmpty(current))
        {
            if (File.Exists(Path.Combine(current, "DuAnTotNghiep.csproj")))
            {
                return current;
            }

            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }

        throw new DirectoryNotFoundException("Cannot locate project root.");
    }
}
