using Xunit;

namespace DuAnTotNghiep.Tests;

public class M8PathTemplateViewTests
{
    [Theory]
    [InlineData("Index.cshtml", "@model IEnumerable<DuAnTotNghiep.Models.LearningPathTemplate>", "Path Templates")]
    [InlineData("Create.cshtml", "@model DuAnTotNghiep.Models.LearningPathTemplate", "Create Path Template")]
    [InlineData("Edit.cshtml", "@model DuAnTotNghiep.Models.LearningPathTemplate", "Edit Path Template")]
    [InlineData("Details.cshtml", "@model DuAnTotNghiep.Models.LearningPathTemplate", "Path Template Detail")]
    public void PathTemplateViews_ShouldUseAdminLayoutAndExpectedModels(
        string fileName,
        string modelDirective,
        string title)
    {
        var viewPath = ProjectFile("Areas", "Admin", "Views", "PathTemplates", fileName);

        Assert.True(File.Exists(viewPath), $"Missing view: {viewPath}");
        var content = File.ReadAllText(viewPath);

        Assert.Contains(modelDirective, content);
        Assert.Contains("Layout = \"~/Views/Shared/_AdminLayout.cshtml\"", content);
        Assert.Contains(title, content);
    }

    private static string ProjectFile(params string[] segments)
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        return Path.Combine(new[] { root }.Concat(segments).ToArray());
    }
}
