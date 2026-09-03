using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DuAnTotNghiep.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClassroomEnrollmentTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('test_attempts') AND name = 'fullscreen_exit_count')
    ALTER TABLE [test_attempts] ADD [fullscreen_exit_count] int NOT NULL DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('test_attempts') AND name = 'tab_switch_count')
    ALTER TABLE [test_attempts] ADD [tab_switch_count] int NOT NULL DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('test_attempts') AND name = 'violation_log')
    ALTER TABLE [test_attempts] ADD [violation_log] nvarchar(max) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('schedules') AND name = 'meet_notification_sent')
    ALTER TABLE [schedules] ADD [meet_notification_sent] bit NOT NULL DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('schedules') AND name = 'meet_url')
    ALTER TABLE [schedules] ADD [meet_url] nvarchar(500) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('quizzes') AND name = 'is_exam_mode')
    ALTER TABLE [quizzes] ADD [is_exam_mode] bit NOT NULL DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('quizzes') AND name = 'max_violations')
    ALTER TABLE [quizzes] ADD [max_violations] int NOT NULL DEFAULT 3;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('quiz_attempts') AND name = 'fullscreen_exit_count')
    ALTER TABLE [quiz_attempts] ADD [fullscreen_exit_count] int NOT NULL DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('quiz_attempts') AND name = 'tab_switch_count')
    ALTER TABLE [quiz_attempts] ADD [tab_switch_count] int NOT NULL DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('quiz_attempts') AND name = 'violation_log')
    ALTER TABLE [quiz_attempts] ADD [violation_log] nvarchar(max) NULL;
");

            migrationBuilder.CreateTable(
                name: "classrooms",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    class_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    level_id = table.Column<int>(type: "int", nullable: false),
                    teacher_id = table.Column<int>(type: "int", nullable: false),
                    max_students = table.Column<int>(type: "int", nullable: false, defaultValue: 30),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "ACTIVE"),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    start_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    end_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_classrooms", x => x.id);
                    table.ForeignKey(
                        name: "FK_classrooms_levels",
                        column: x => x.level_id,
                        principalTable: "english_proficiency_levels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_classrooms_teachers",
                        column: x => x.teacher_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "class_schedules",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    classroom_id = table.Column<int>(type: "int", nullable: false),
                    day_of_week = table.Column<int>(type: "int", nullable: false),
                    start_time = table.Column<TimeSpan>(type: "time", nullable: false),
                    end_time = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_class_schedules", x => x.id);
                    table.ForeignKey(
                        name: "FK_class_schedules_classrooms",
                        column: x => x.classroom_id,
                        principalTable: "classrooms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "enrollments",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    classroom_id = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "ACTIVE"),
                    enrolled_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    dropped_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enrollments", x => x.id);
                    table.ForeignKey(
                        name: "FK_enrollments_classrooms",
                        column: x => x.classroom_id,
                        principalTable: "classrooms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_enrollments_students",
                        column: x => x.student_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_class_schedules_classroom_id",
                table: "class_schedules",
                column: "classroom_id");

            migrationBuilder.CreateIndex(
                name: "IX_classrooms_level_id",
                table: "classrooms",
                column: "level_id");

            migrationBuilder.CreateIndex(
                name: "IX_classrooms_teacher_id",
                table: "classrooms",
                column: "teacher_id");

            migrationBuilder.CreateIndex(
                name: "IX_enrollments_classroom_id",
                table: "enrollments",
                column: "classroom_id");

            migrationBuilder.CreateIndex(
                name: "UQ_enrollments_student_classroom",
                table: "enrollments",
                columns: new[] { "student_id", "classroom_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "class_schedules");

            migrationBuilder.DropTable(
                name: "enrollments");

            migrationBuilder.DropTable(
                name: "classrooms");

            migrationBuilder.DropColumn(
                name: "fullscreen_exit_count",
                table: "test_attempts");

            migrationBuilder.DropColumn(
                name: "tab_switch_count",
                table: "test_attempts");

            migrationBuilder.DropColumn(
                name: "violation_log",
                table: "test_attempts");

            migrationBuilder.DropColumn(
                name: "meet_notification_sent",
                table: "schedules");

            migrationBuilder.DropColumn(
                name: "meet_url",
                table: "schedules");

            migrationBuilder.DropColumn(
                name: "is_exam_mode",
                table: "quizzes");

            migrationBuilder.DropColumn(
                name: "max_violations",
                table: "quizzes");

            migrationBuilder.DropColumn(
                name: "fullscreen_exit_count",
                table: "quiz_attempts");

            migrationBuilder.DropColumn(
                name: "tab_switch_count",
                table: "quiz_attempts");

            migrationBuilder.DropColumn(
                name: "violation_log",
                table: "quiz_attempts");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "learning_goals",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(sysutcdatetime())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "(sysutcdatetime())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "learning_goals",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(sysutcdatetime())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "(sysutcdatetime())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "english_skills",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(sysutcdatetime())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "(sysutcdatetime())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "english_skills",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(sysutcdatetime())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "(sysutcdatetime())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "english_proficiency_levels",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(sysutcdatetime())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "(sysutcdatetime())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "english_proficiency_levels",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(sysutcdatetime())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "(sysutcdatetime())");
        }
    }
}
