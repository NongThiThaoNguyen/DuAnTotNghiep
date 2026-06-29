using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Tests;

public class M8EnumTests
{
    [Fact]
    public void LearningPathStatus_DefinesExpectedStringConstants()
    {
        var assembly = typeof(StudentLearningPath).Assembly;
        var type = assembly.GetType("DuAnTotNghiep.Enums.LearningPathStatus");
        Assert.NotNull(type);

        AssertConstant(type!, "Active", "ACTIVE");
        AssertConstant(type!, "Archived", "ARCHIVED");
        AssertConstant(type!, "Paused", "PAUSED");
        AssertConstant(type!, "Failed", "FAILED");
        AssertConstant(type!, "Generating", "GENERATING");
        Assert.True((bool)type!.GetMethod("IsValid")!.Invoke(null, new object[] { "active" })!);
        Assert.False((bool)type.GetMethod("IsValid")!.Invoke(null, new object[] { "CURRENT" })!);
    }

    private static void AssertConstant(Type type, string fieldName, string expectedValue)
    {
        var field = type.GetField(fieldName);
        Assert.NotNull(field);
        Assert.Equal(expectedValue, field!.GetValue(null));
    }
}
