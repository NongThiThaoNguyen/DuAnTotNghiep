using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DuAnTotNghiep.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStudyPlanColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "rescheduled_from",
                table: "learning_path_nodes",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "skipped_reason",
                table: "learning_path_nodes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LPN_PathId_ScheduledDate_Status",
                table: "learning_path_nodes",
                columns: new[] { "learning_path_id", "scheduled_date", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LPN_PathId_ScheduledDate_Status",
                table: "learning_path_nodes");

            migrationBuilder.DropColumn(
                name: "rescheduled_from",
                table: "learning_path_nodes");

            migrationBuilder.DropColumn(
                name: "skipped_reason",
                table: "learning_path_nodes");
        }
    }
}
