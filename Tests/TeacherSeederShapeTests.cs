namespace DuAnTotNghiep.Tests;

public class TeacherSeederShapeTests
{
    [Fact]
    public void AILearnSeeder_AssignsDemoCoursesLessonsAndQuizzesToTeacher()
    {
        var seederPath = ProjectFile("Data", "Seeders", "AILearnSeeder.cs");
        var content = File.ReadAllText(seederPath);

        Assert.Contains("teacher@aistudyenglish.com", content);
        Assert.Contains("CreatedBy = teacherUser?.Id", content);
    }

    private static string ProjectFile(params string[] segments)
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        return Path.Combine(new[] { root }.Concat(segments).ToArray());
    }
}
