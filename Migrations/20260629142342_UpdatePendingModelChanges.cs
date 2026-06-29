using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DuAnTotNghiep.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ref_usage_policy",
                table: "reference_sources");

            migrationBuilder.AddColumn<DateTime>(
                name: "archived_at",
                table: "student_learning_paths",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "path_version",
                table: "student_learning_paths",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "replaced_by_path_id",
                table: "student_learning_paths",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "author",
                table: "reference_sources",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "compliance_evidence_url",
                table: "reference_sources",
                type: "varchar(1000)",
                unicode: false,
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "reference_sources",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "organization",
                table: "reference_sources",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "rejected_at",
                table: "reference_sources",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "rejected_by",
                table: "reference_sources",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "rejection_reason",
                table: "reference_sources",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "required_node_id",
                table: "learning_path_nodes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TopicId",
                table: "competency_skill_scores",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLatest",
                table: "competency_analyses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "KnowledgeGapsJson",
                table: "competency_analyses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetadataJson",
                table: "competency_analyses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrioritizedTopicsJson",
                table: "competency_analyses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "competency_analyses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DurationMs",
                table: "ai_usage_logs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LatencyMs",
                table: "ai_usage_logs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PromptInput",
                table: "ai_usage_logs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponseOutput",
                table: "ai_usage_logs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserPromptTemplate",
                table: "ai_prompt_templates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BatchId",
                table: "ai_generated_contents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PublishedQuestionId",
                table: "ai_generated_contents",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReplanningRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LowScoreThreshold = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MissedDaysThreshold = table.Column<int>(type: "int", nullable: false),
                    FastProgressScoreThreshold = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AutoApplyEnabled = table.Column<bool>(type: "bit", nullable: false),
                    SuggestionExpiryDays = table.Column<int>(type: "int", nullable: false),
                    DebounceHours = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReplanningRules", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_student_learning_paths_replaced_by_path_id",
                table: "student_learning_paths",
                column: "replaced_by_path_id");

            migrationBuilder.CreateIndex(
                name: "IX_reference_sources_rejected_by",
                table: "reference_sources",
                column: "rejected_by");

            migrationBuilder.CreateIndex(
                name: "IX_reference_sources_source_name",
                table: "reference_sources",
                column: "source_name");

            migrationBuilder.CreateIndex(
                name: "IX_reference_sources_source_url",
                table: "reference_sources",
                column: "source_url");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ref_usage_policy",
                table: "reference_sources",
                sql: "[usage_policy] IS NULL OR [usage_policy] IN ('REFERENCE_ONLY', 'OPEN_LICENSE', 'RESTRICTED')");

            migrationBuilder.CreateIndex(
                name: "IX_learning_path_nodes_required_node_id",
                table: "learning_path_nodes",
                column: "required_node_id");

            migrationBuilder.CreateIndex(
                name: "IX_competency_skill_scores_TopicId",
                table: "competency_skill_scores",
                column: "TopicId");

            migrationBuilder.AddForeignKey(
                name: "FK_competency_skill_scores_learning_topics_TopicId",
                table: "competency_skill_scores",
                column: "TopicId",
                principalTable: "learning_topics",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_learning_path_nodes_required_node",
                table: "learning_path_nodes",
                column: "required_node_id",
                principalTable: "learning_path_nodes",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_reference_sources_users_rejected_by",
                table: "reference_sources",
                column: "rejected_by",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_student_learning_paths_replaced_by_path",
                table: "student_learning_paths",
                column: "replaced_by_path_id",
                principalTable: "student_learning_paths",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_competency_skill_scores_learning_topics_TopicId",
                table: "competency_skill_scores");

            migrationBuilder.DropForeignKey(
                name: "FK_learning_path_nodes_required_node",
                table: "learning_path_nodes");

            migrationBuilder.DropForeignKey(
                name: "FK_reference_sources_users_rejected_by",
                table: "reference_sources");

            migrationBuilder.DropForeignKey(
                name: "FK_student_learning_paths_replaced_by_path",
                table: "student_learning_paths");

            migrationBuilder.DropTable(
                name: "ReplanningRules");

            migrationBuilder.DropIndex(
                name: "IX_student_learning_paths_replaced_by_path_id",
                table: "student_learning_paths");

            migrationBuilder.DropIndex(
                name: "IX_reference_sources_rejected_by",
                table: "reference_sources");

            migrationBuilder.DropIndex(
                name: "IX_reference_sources_source_name",
                table: "reference_sources");

            migrationBuilder.DropIndex(
                name: "IX_reference_sources_source_url",
                table: "reference_sources");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ref_usage_policy",
                table: "reference_sources");

            migrationBuilder.DropIndex(
                name: "IX_learning_path_nodes_required_node_id",
                table: "learning_path_nodes");

            migrationBuilder.DropIndex(
                name: "IX_competency_skill_scores_TopicId",
                table: "competency_skill_scores");

            migrationBuilder.DropColumn(
                name: "archived_at",
                table: "student_learning_paths");

            migrationBuilder.DropColumn(
                name: "path_version",
                table: "student_learning_paths");

            migrationBuilder.DropColumn(
                name: "replaced_by_path_id",
                table: "student_learning_paths");

            migrationBuilder.DropColumn(
                name: "author",
                table: "reference_sources");

            migrationBuilder.DropColumn(
                name: "compliance_evidence_url",
                table: "reference_sources");

            migrationBuilder.DropColumn(
                name: "description",
                table: "reference_sources");

            migrationBuilder.DropColumn(
                name: "organization",
                table: "reference_sources");

            migrationBuilder.DropColumn(
                name: "rejected_at",
                table: "reference_sources");

            migrationBuilder.DropColumn(
                name: "rejected_by",
                table: "reference_sources");

            migrationBuilder.DropColumn(
                name: "rejection_reason",
                table: "reference_sources");

            migrationBuilder.DropColumn(
                name: "required_node_id",
                table: "learning_path_nodes");

            migrationBuilder.DropColumn(
                name: "TopicId",
                table: "competency_skill_scores");

            migrationBuilder.DropColumn(
                name: "IsLatest",
                table: "competency_analyses");

            migrationBuilder.DropColumn(
                name: "KnowledgeGapsJson",
                table: "competency_analyses");

            migrationBuilder.DropColumn(
                name: "MetadataJson",
                table: "competency_analyses");

            migrationBuilder.DropColumn(
                name: "PrioritizedTopicsJson",
                table: "competency_analyses");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "competency_analyses");

            migrationBuilder.DropColumn(
                name: "DurationMs",
                table: "ai_usage_logs");

            migrationBuilder.DropColumn(
                name: "LatencyMs",
                table: "ai_usage_logs");

            migrationBuilder.DropColumn(
                name: "PromptInput",
                table: "ai_usage_logs");

            migrationBuilder.DropColumn(
                name: "ResponseOutput",
                table: "ai_usage_logs");

            migrationBuilder.DropColumn(
                name: "UserPromptTemplate",
                table: "ai_prompt_templates");

            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "ai_generated_contents");

            migrationBuilder.DropColumn(
                name: "PublishedQuestionId",
                table: "ai_generated_contents");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ref_usage_policy",
                table: "reference_sources",
                sql: "[usage_policy] IS NULL OR [usage_policy] IN ('REFERENCE_ONLY', 'OPEN_USE', 'INTERNAL_ONLY')");
        }
    }
}
