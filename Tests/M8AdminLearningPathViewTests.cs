using Xunit;

namespace DuAnTotNghiep.Tests;

public class M8AdminLearningPathViewTests
{
    [Theory]
    [InlineData("PathHistory.cshtml", "@model DuAnTotNghiep.ViewModels.Admin.LearningPaths.PathHistoryViewModel", "Path History")]
    [InlineData("PathDetail.cshtml", "@model DuAnTotNghiep.ViewModels.Admin.LearningPaths.PathDetailAdminViewModel", "Path Detail")]
    [InlineData("GenerationLogs.cshtml", "@model DuAnTotNghiep.ViewModels.Admin.LearningPaths.GenerationLogViewModel", "Generation Logs")]
    public void LearningPathAdminViews_ShouldUseAdminLayoutAndExpectedModels(
        string fileName,
        string modelDirective,
        string title)
    {
        var viewPath = ProjectFile("Areas", "Admin", "Views", "LearningPaths", fileName);

        Assert.True(File.Exists(viewPath), $"Missing view: {viewPath}");
        var content = File.ReadAllText(viewPath);

        Assert.Contains(modelDirective, content);
        Assert.Contains("Layout = \"~/Views/Shared/_AdminLayout.cshtml\"", content);
        Assert.Contains(title, content);
    }

    [Fact]
    public void AdminLayout_ShouldContainM8LearningPathMenuItems()
    {
        var layoutPath = ProjectFile("Views", "Shared", "_AdminLayout.cshtml");

        var content = File.ReadAllText(layoutPath);

        Assert.Contains("Path History", content);
        Assert.Contains("Path Templates", content);
        Assert.Contains("Generation Logs", content);
        Assert.Contains("asp-controller=\"LearningPaths\"", content);
        Assert.Contains("asp-controller=\"PathTemplates\"", content);
    }

    private static string ProjectFile(params string[] segments)
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        return Path.Combine(new[] { root }.Concat(segments).ToArray());
    }
}
