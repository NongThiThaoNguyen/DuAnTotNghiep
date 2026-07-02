namespace DuAnTotNghiep.Tests;

public class AdminTeacherScheduleViewTests
{
    [Fact]
    public void AdminTeacherScheduleFiles_ShouldBeWiredIntoAdminArea()
    {
        var controllerPath = ProjectFile("Areas", "Admin", "Controllers", "TeacherSchedulesController.cs");
        var viewModelPath = ProjectFile("Areas", "Admin", "ViewModels", "AdminTeacherScheduleViewModels.cs");
        var servicePath = ProjectFile("Services", "AdminTeacherScheduleService.cs");
        var interfacePath = ProjectFile("Services", "Interfaces", "IAdminTeacherScheduleService.cs");
        var aiServicePath = ProjectFile("Services", "AdminTeacherScheduleAiService.cs");
        var aiInterfacePath = ProjectFile("Services", "Interfaces", "IAdminTeacherScheduleAiService.cs");
        var indexViewPath = ProjectFile("Areas", "Admin", "Views", "TeacherSchedules", "Index.cshtml");
        var createViewPath = ProjectFile("Areas", "Admin", "Views", "TeacherSchedules", "Create.cshtml");
        var editViewPath = ProjectFile("Areas", "Admin", "Views", "TeacherSchedules", "Edit.cshtml");

        Assert.True(File.Exists(controllerPath), $"Missing controller: {controllerPath}");
        Assert.True(File.Exists(viewModelPath), $"Missing view model: {viewModelPath}");
        Assert.True(File.Exists(servicePath), $"Missing service: {servicePath}");
        Assert.True(File.Exists(interfacePath), $"Missing service interface: {interfacePath}");
        Assert.True(File.Exists(aiServicePath), $"Missing AI service: {aiServicePath}");
        Assert.True(File.Exists(aiInterfacePath), $"Missing AI service interface: {aiInterfacePath}");
        Assert.True(File.Exists(indexViewPath), $"Missing index view: {indexViewPath}");
        Assert.True(File.Exists(createViewPath), $"Missing create view: {createViewPath}");
        Assert.True(File.Exists(editViewPath), $"Missing edit view: {editViewPath}");

        var controller = File.ReadAllText(controllerPath);
        Assert.Contains("[Area(\"Admin\")]", controller);
        Assert.Contains("[Authorize(Roles = \"ADMIN\")]", controller);
        Assert.Contains("IAdminTeacherScheduleService", controller);
        Assert.Contains("IAdminTeacherScheduleAiService", controller);
        Assert.Contains("GenerateAiSuggestions", controller);
        Assert.Contains("ApplyAiSuggestion", controller);

        var program = File.ReadAllText(ProjectFile("Program.cs"));
        Assert.Contains("AddScoped<IAdminTeacherScheduleService, AdminTeacherScheduleService>", program);
        Assert.Contains("AddHttpClient<IAdminTeacherScheduleAiService, AdminTeacherScheduleAiService>", program);
    }

    [Fact]
    public void AdminTeacherScheduleIndex_ShouldExposeFiltersSortingAndActions()
    {
        var content = File.ReadAllText(ProjectFile("Areas", "Admin", "Views", "TeacherSchedules", "Index.cshtml"));

        Assert.Contains("@model DuAnTotNghiep.Areas.Admin.ViewModels.AdminTeacherScheduleListViewModel", content);
        Assert.Contains("Layout = \"~/Views/Shared/_AdminLayout.cshtml\"", content);
        Assert.Contains("asp-for=\"Filter.TeacherId\"", content);
        Assert.Contains("asp-for=\"Filter.FromDate\"", content);
        Assert.Contains("asp-for=\"Filter.ToDate\"", content);
        Assert.Contains("asp-route-SortBy=\"startTime\"", content);
        Assert.Contains("asp-route-SortBy=\"teacher\"", content);
        Assert.Contains("asp-action=\"Create\"", content);
        Assert.Contains("asp-action=\"Edit\"", content);
        Assert.Contains("asp-action=\"Delete\"", content);
        Assert.Contains("Gợi ý lịch bằng AI", content);
        Assert.Contains("asp-action=\"GenerateAiSuggestions\"", content);
        Assert.Contains("asp-action=\"ApplyAiSuggestion\"", content);
        Assert.Contains("asp-for=\"AiRequest.TeacherId\"", content);
        Assert.Contains("asp-for=\"AiRequest.DurationMinutes\"", content);
        Assert.Contains("Model.AiSuggestions", content);
    }

    [Fact]
    public void AdminLayout_ShouldContainTeacherScheduleMenuItem()
    {
        var layout = File.ReadAllText(ProjectFile("Views", "Shared", "_AdminLayout.cshtml"));

        Assert.Contains("\"TeacherSchedules\" => \"Lịch giáo viên\"", layout);
        Assert.Contains("asp-controller=\"TeacherSchedules\"", layout);
        Assert.Contains("Lịch giáo viên", layout);
    }

    private static string ProjectFile(params string[] segments)
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        return Path.Combine(new[] { root }.Concat(segments).ToArray());
    }
}
