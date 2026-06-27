using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DuAnTotNghiep.Migrations
{
    /// <inheritdoc />
    public partial class AddM16ProgressSnapshotIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.CreateIndex(
                name: "IX_sps_student_date",
                table: "student_progress_snapshots",
                columns: new[] { "student_id", "snapshot_date" });

            migrationBuilder.CreateIndex(
                name: "IX_sps_student_skill",
                table: "student_progress_snapshots",
                columns: new[] { "student_id", "skill_id" });

            migrationBuilder.CreateIndex(
                name: "IX_sps_student_topic",
                table: "student_progress_snapshots",
                columns: new[] { "student_id", "topic_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_sps_student_date",
                table: "student_progress_snapshots");

            migrationBuilder.DropIndex(
                name: "IX_sps_student_skill",
                table: "student_progress_snapshots");

            migrationBuilder.DropIndex(
                name: "IX_sps_student_topic",
                table: "student_progress_snapshots");


        }
    }
}
