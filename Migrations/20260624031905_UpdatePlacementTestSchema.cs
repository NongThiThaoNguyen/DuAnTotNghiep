using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DuAnTotNghiep.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePlacementTestSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ptq_question",
                table: "placement_test_questions");

            migrationBuilder.DropForeignKey(
                name: "FK_ptq_section",
                table: "placement_test_questions");

            migrationBuilder.DropForeignKey(
                name: "FK_ta_attempt",
                table: "test_answers");

            migrationBuilder.DropForeignKey(
                name: "FK_ta_question",
                table: "test_answers");

            migrationBuilder.DropForeignKey(
                name: "FK_attempt_student",
                table: "test_attempts");

            migrationBuilder.DropForeignKey(
                name: "FK_attempt_test",
                table: "test_attempts");

            migrationBuilder.DropIndex(
                name: "IX_test_attempt_student",
                table: "test_attempts");

            // migrationBuilder.DropIndex(
            //     name: "IX_test_answers_attempt_id",
            //     table: "test_answers");

            // migrationBuilder.DropIndex(
            //     name: "IX_placement_test_sections_placement_test_id",
            //     table: "placement_test_sections");

            migrationBuilder.CreateIndex(
                name: "IX_test_attempt_student",
                table: "test_attempts",
                columns: new[] { "student_id", "placement_test_id", "status" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_TestAttempt_Status",
                table: "test_attempts",
                sql: "[status] IN ('IN_PROGRESS', 'SUBMITTED', 'GRADED', 'EXPIRED')");

            migrationBuilder.CreateIndex(
                name: "UQ_test_answer_attempt_question",
                table: "test_answers",
                columns: new[] { "attempt_id", "question_id" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_PlacementTest_Status",
                table: "placement_tests",
                sql: "[status] IN ('DRAFT', 'PUBLISHED', 'ARCHIVED')");

            migrationBuilder.CreateIndex(
                name: "IX_placement_test_sections_test_skill",
                table: "placement_test_sections",
                columns: new[] { "placement_test_id", "skill_id" });

            migrationBuilder.AddForeignKey(
                name: "FK_ptq_question",
                table: "placement_test_questions",
                column: "question_id",
                principalTable: "question_bank",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ptq_section",
                table: "placement_test_questions",
                column: "section_id",
                principalTable: "placement_test_sections",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ta_attempt",
                table: "test_answers",
                column: "attempt_id",
                principalTable: "test_attempts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ta_question",
                table: "test_answers",
                column: "question_id",
                principalTable: "question_bank",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_attempt_student",
                table: "test_attempts",
                column: "student_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_attempt_test",
                table: "test_attempts",
                column: "placement_test_id",
                principalTable: "placement_tests",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ptq_question",
                table: "placement_test_questions");

            migrationBuilder.DropForeignKey(
                name: "FK_ptq_section",
                table: "placement_test_questions");

            migrationBuilder.DropForeignKey(
                name: "FK_ta_attempt",
                table: "test_answers");

            migrationBuilder.DropForeignKey(
                name: "FK_ta_question",
                table: "test_answers");

            migrationBuilder.DropForeignKey(
                name: "FK_attempt_student",
                table: "test_attempts");

            migrationBuilder.DropForeignKey(
                name: "FK_attempt_test",
                table: "test_attempts");

            migrationBuilder.DropIndex(
                name: "IX_test_attempt_student",
                table: "test_attempts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TestAttempt_Status",
                table: "test_attempts");

            migrationBuilder.DropIndex(
                name: "UQ_test_answer_attempt_question",
                table: "test_answers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PlacementTest_Status",
                table: "placement_tests");

            migrationBuilder.DropIndex(
                name: "IX_placement_test_sections_test_skill",
                table: "placement_test_sections");

            migrationBuilder.CreateIndex(
                name: "IX_test_attempt_student",
                table: "test_attempts",
                columns: new[] { "student_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_test_answers_attempt_id",
                table: "test_answers",
                column: "attempt_id");

            migrationBuilder.CreateIndex(
                name: "IX_placement_test_sections_placement_test_id",
                table: "placement_test_sections",
                column: "placement_test_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ptq_question",
                table: "placement_test_questions",
                column: "question_id",
                principalTable: "question_bank",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_ptq_section",
                table: "placement_test_questions",
                column: "section_id",
                principalTable: "placement_test_sections",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_ta_attempt",
                table: "test_answers",
                column: "attempt_id",
                principalTable: "test_attempts",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_ta_question",
                table: "test_answers",
                column: "question_id",
                principalTable: "question_bank",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_attempt_student",
                table: "test_attempts",
                column: "student_id",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_attempt_test",
                table: "test_attempts",
                column: "placement_test_id",
                principalTable: "placement_tests",
                principalColumn: "id");
        }
    }
}
