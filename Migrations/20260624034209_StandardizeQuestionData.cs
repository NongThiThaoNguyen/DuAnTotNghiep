using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DuAnTotNghiep.Migrations
{
    /// <inheritdoc />
    public partial class StandardizeQuestionData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_QuestionBank_DifficultyLevel",
                table: "question_bank",
                sql: "[difficulty_level] IN ('BASIC', 'MEDIUM', 'ADVANCED')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_QuestionBank_QuestionType",
                table: "question_bank",
                sql: "[question_type] IN ('MCQ', 'TRUE_FALSE', 'SHORT_ANSWER', 'LISTENING')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PracticeTask_DifficultyLevel",
                table: "practice_tasks",
                sql: "[difficulty_level] IN ('BASIC', 'MEDIUM', 'ADVANCED')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PracticeTask_TaskType",
                table: "practice_tasks",
                sql: "[task_type] IN ('MCQ', 'TRUE_FALSE', 'SHORT_ANSWER', 'LISTENING')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_QuestionBank_DifficultyLevel",
                table: "question_bank");

            migrationBuilder.DropCheckConstraint(
                name: "CK_QuestionBank_QuestionType",
                table: "question_bank");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PracticeTask_DifficultyLevel",
                table: "practice_tasks");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PracticeTask_TaskType",
                table: "practice_tasks");
        }
    }
}
