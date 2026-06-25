using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DuAnTotNghiep.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingColumnsForMasterData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "learning_goals",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(sysutcdatetime())");

            migrationBuilder.AddColumn<int>(
                name: "created_by",
                table: "learning_goals",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "order_index",
                table: "learning_goals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "learning_goals",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(sysutcdatetime())");

            migrationBuilder.AddColumn<int>(
                name: "updated_by",
                table: "learning_goals",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StudentAvailableStudySlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentProfileId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentAvailableStudySlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentAvailableStudySlots_student_learning_profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalTable: "student_learning_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_learning_goals_created_by",
                table: "learning_goals",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_learning_goals_updated_by",
                table: "learning_goals",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAvailableStudySlots_StudentProfileId",
                table: "StudentAvailableStudySlots",
                column: "StudentProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_goals_created_by",
                table: "learning_goals",
                column: "created_by",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_goals_updated_by",
                table: "learning_goals",
                column: "updated_by",
                principalTable: "users",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_goals_created_by",
                table: "learning_goals");

            migrationBuilder.DropForeignKey(
                name: "FK_goals_updated_by",
                table: "learning_goals");

            migrationBuilder.DropTable(
                name: "StudentAvailableStudySlots");

            migrationBuilder.DropIndex(
                name: "IX_learning_goals_created_by",
                table: "learning_goals");

            migrationBuilder.DropIndex(
                name: "IX_learning_goals_updated_by",
                table: "learning_goals");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "learning_goals");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "learning_goals");

            migrationBuilder.DropColumn(
                name: "order_index",
                table: "learning_goals");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "learning_goals");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "learning_goals");
        }
    }
}
