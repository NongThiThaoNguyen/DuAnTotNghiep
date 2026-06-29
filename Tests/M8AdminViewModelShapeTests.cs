using DuAnTotNghiep.ViewModels.Admin.LearningPaths;
using DuAnTotNghiep.ViewModels.Admin.PathTemplates;
using Xunit;

namespace DuAnTotNghiep.Tests;

public class M8AdminViewModelShapeTests
{
    [Fact]
    public void LearningPathAdminViewModels_ShouldExposeHistoryDetailAndLogs()
    {
        AssertHasProperty<PathHistoryViewModel>("Paths");
        AssertHasProperty<PathHistoryViewModel>("Page");
        AssertHasProperty<PathHistoryViewModel>("TotalPages");
        AssertHasProperty<PathDetailAdminViewModel>("Nodes");
        AssertHasProperty<PathDetailAdminViewModel>("AiModel");
        AssertHasProperty<PathDetailAdminViewModel>("PromptVersion");
        AssertHasProperty<PathDetailAdminViewModel>("ErrorMessage");
        AssertHasProperty<GenerationLogViewModel>("Logs");
    }

    [Fact]
    public void PathTemplateAdminViewModels_ShouldExposeTemplateForms()
    {
        AssertHasProperty<PathTemplateViewModel>("TemplateName");
        AssertHasProperty<PathTemplateViewModel>("Nodes");
        AssertHasProperty<CreatePathTemplateViewModel>("Nodes");
        AssertHasProperty<EditPathTemplateViewModel>("Id");
        AssertHasProperty<EditPathTemplateViewModel>("Nodes");
    }

    private static void AssertHasProperty<T>(string propertyName)
    {
        Assert.NotNull(typeof(T).GetProperty(propertyName));
    }
}
