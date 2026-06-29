using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DuAnTotNghiep.Tests;

public class M8SchemaMappingTests
{
    [Fact]
    public void StudentLearningPath_ModelMapsM8ArchiveMetadataColumns()
    {
        using var context = CreateContext();

        AssertMappedColumn(context, typeof(StudentLearningPath), "PathVersion", "path_version", "student_learning_paths");
        AssertMappedColumn(context, typeof(StudentLearningPath), "ArchivedAt", "archived_at", "student_learning_paths");
        AssertMappedColumn(context, typeof(StudentLearningPath), "ReplacedByPathId", "replaced_by_path_id", "student_learning_paths");
    }

    [Fact]
    public void LearningPathNode_ModelMapsRequiredNodeColumn()
    {
        using var context = CreateContext();

        AssertMappedColumn(context, typeof(LearningPathNode), "RequiredNodeId", "required_node_id", "learning_path_nodes");
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static void AssertMappedColumn(
        ApplicationDbContext context,
        Type entityType,
        string propertyName,
        string expectedColumnName,
        string tableName)
    {
        var entity = context.Model.FindEntityType(entityType);
        Assert.NotNull(entity);

        var property = entity.FindProperty(propertyName);
        Assert.NotNull(property);

        var table = StoreObjectIdentifier.Table(tableName, null);
        Assert.Equal(expectedColumnName, property!.GetColumnName(table));
    }
}
