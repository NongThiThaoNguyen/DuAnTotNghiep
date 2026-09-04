using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DuAnTotNghiep.Data.Migrations;

public partial class AddClassroomIdToSchedules : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('schedules') AND name = 'classroom_id')
    ALTER TABLE [schedules] ADD [classroom_id] int NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_schedules_classroom_id' AND object_id = OBJECT_ID('schedules'))
    CREATE INDEX [IX_schedules_classroom_id] ON [schedules] ([classroom_id]);

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_schedules_classrooms')
    ALTER TABLE [schedules] ADD CONSTRAINT [FK_schedules_classrooms] FOREIGN KEY ([classroom_id]) REFERENCES [classrooms] ([id]) ON DELETE SET NULL;
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_schedules_classrooms')
    ALTER TABLE [schedules] DROP CONSTRAINT [FK_schedules_classrooms];
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_schedules_classroom_id' AND object_id = OBJECT_ID('schedules'))
    DROP INDEX [IX_schedules_classroom_id] ON [schedules];
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('schedules') AND name = 'classroom_id')
    ALTER TABLE [schedules] DROP COLUMN [classroom_id];
");
    }
}