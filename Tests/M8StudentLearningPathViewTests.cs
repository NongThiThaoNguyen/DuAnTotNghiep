using Xunit;

namespace DuAnTotNghiep.Tests;

public class M8StudentLearningPathViewTests
{
    [Fact]
    public void GenerateView_ShouldUseM8ModelLayoutAndRequiredClasses()
    {
        var viewPath = ProjectFile("Areas", "Student", "Views", "LearningPath", "Generate.cshtml");

        Assert.True(File.Exists(viewPath), $"Missing view: {viewPath}");
        var content = File.ReadAllText(viewPath);

        Assert.Contains("@model DuAnTotNghiep.Models.ViewModels.LearningPath.M8.LearningPathGenerateViewModel", content);
        Assert.Contains("Layout = \"~/Views/Shared/_StudentLayout.cshtml\"", content);
        Assert.Contains("m8-generate-form", content);
        Assert.Contains("m8-missing-step", content);
    }

    [Fact]
    public void SummaryView_ShouldUseM8ModelLayoutAndRequiredActions()
    {
        var viewPath = ProjectFile("Areas", "Student", "Views", "LearningPath", "Summary.cshtml");

        Assert.True(File.Exists(viewPath), $"Missing view: {viewPath}");
        var content = File.ReadAllText(viewPath);

        Assert.Contains("@model DuAnTotNghiep.Models.ViewModels.LearningPath.M8.LearningPathSummaryViewModel", content);
        Assert.Contains("Layout = \"~/Views/Shared/_StudentLayout.cshtml\"", content);
        Assert.Contains("m8-path-summary", content);
        Assert.Contains("asp-action=\"Detail\"", content);
        Assert.Contains("asp-action=\"Regenerate\"", content);
    }

    [Fact]
    public void DetailView_ShouldUseM8ModelLayoutAndNodeStatusClasses()
    {
        var viewPath = ProjectFile("Areas", "Student", "Views", "LearningPath", "Detail.cshtml");

        Assert.True(File.Exists(viewPath), $"Missing view: {viewPath}");
        var content = File.ReadAllText(viewPath);

        Assert.Contains("@model DuAnTotNghiep.Models.ViewModels.LearningPath.M8.LearningPathDetailViewModel", content);
        Assert.Contains("Layout = \"~/Views/Shared/_StudentLayout.cshtml\"", content);
        Assert.Contains("m8-node-card", content);
        Assert.Contains("m8-node-status--locked", content);
        Assert.Contains("m8-node-status--available", content);
        Assert.Contains("m8-node-status--completed", content);
        Assert.Contains("asp-action=\"OpenNode\"", content);
    }

    [Fact]
    public void LearningPathM8Css_ShouldContainRequiredBEMClasses()
    {
        var cssPath = ProjectFile("wwwroot", "css", "learning-path-m8.css");

        Assert.True(File.Exists(cssPath), $"Missing css: {cssPath}");
        var content = File.ReadAllText(cssPath);

        Assert.Contains(".m8-path-summary", content);
        Assert.Contains(".m8-node-card", content);
        Assert.Contains(".m8-node-status--locked", content);
        Assert.Contains(".m8-node-status--available", content);
        Assert.Contains(".m8-node-status--completed", content);
        Assert.Contains(".m8-generate-form", content);
        Assert.Contains(".m8-missing-step", content);
        Assert.DoesNotContain(".lp-", content);
    }

    private static string ProjectFile(params string[] segments)
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        return Path.Combine(new[] { root }.Concat(segments).ToArray());
    }
}
