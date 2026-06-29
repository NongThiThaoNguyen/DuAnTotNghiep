using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DuAnTotNghiep.Migrations
{
    /// <inheritdoc />
    public partial class AddM4DatabaseStandardization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.RenameIndex(
            //     name: "IX_learning_topics_parent_topic_id",
            //     table: "learning_topics",
            //     newName: "IX_topics_parent_topic_id");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "english_proficiency_levels",
                newName: "level_name");

            migrationBuilder.RenameColumn(
                name: "code",
                table: "english_proficiency_levels",
                newName: "level_code");

            migrationBuilder.AlterColumn<string>(
                name: "difficulty_level",
                table: "learning_topics",
                type: "varchar(30)",
                unicode: false,
                maxLength: 30,
                nullable: false,
                defaultValue: "BEGINNER",
                oldClrType: typeof(string),
                oldType: "varchar(30)",
                oldUnicode: false,
                oldMaxLength: 30,
                oldDefaultValue: "BASIC");

            migrationBuilder.AddColumn<int>(
                name: "updated_by",
                table: "learning_topics",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "learning_objectives",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(sysutcdatetime())");

            migrationBuilder.AddColumn<int>(
                name: "created_by",
                table: "learning_objectives",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "learning_objectives",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(sysutcdatetime())");

            migrationBuilder.AddColumn<int>(
                name: "updated_by",
                table: "learning_objectives",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "english_skills",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(sysutcdatetime())");

            migrationBuilder.AddColumn<int>(
                name: "created_by",
                table: "english_skills",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "english_skills",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(sysutcdatetime())");

            migrationBuilder.AddColumn<int>(
                name: "updated_by",
                table: "english_skills",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "english_proficiency_levels",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(sysutcdatetime())");

            migrationBuilder.AddColumn<int>(
                name: "created_by",
                table: "english_proficiency_levels",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "english_proficiency_levels",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "english_proficiency_levels",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(sysutcdatetime())");

            migrationBuilder.AddColumn<int>(
                name: "updated_by",
                table: "english_proficiency_levels",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "topic_prerequisites",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    topic_id = table.Column<int>(type: "int", nullable: false),
                    prerequisite_topic_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_topic_prerequisites", x => x.id);
                    table.ForeignKey(
                        name: "FK_topic_prerequisites_prerequisite",
                        column: x => x.prerequisite_topic_id,
                        principalTable: "learning_topics",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_topic_prerequisites_topic",
                        column: x => x.topic_id,
                        principalTable: "learning_topics",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_learning_topics_updated_by",
                table: "learning_topics",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_topics_order_index",
                table: "learning_topics",
                column: "order_index");

            migrationBuilder.CreateIndex(
                name: "IX_topics_status",
                table: "learning_topics",
                column: "status");

            migrationBuilder.Sql("IF OBJECT_ID('CK_topics_difficulty', 'C') IS NOT NULL ALTER TABLE learning_topics DROP CONSTRAINT CK_topics_difficulty;");
            migrationBuilder.Sql("UPDATE learning_topics SET difficulty_level = 'BEGINNER' WHERE difficulty_level NOT IN ('BEGINNER', 'ELEMENTARY', 'INTERMEDIATE', 'UPPER_INTERMEDIATE', 'ADVANCED');");

            migrationBuilder.AddCheckConstraint(
                name: "CK_topic_difficulty_level",
                table: "learning_topics",
                sql: "difficulty_level IN ('BEGINNER', 'ELEMENTARY', 'INTERMEDIATE', 'UPPER_INTERMEDIATE', 'ADVANCED')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_topic_status",
                table: "learning_topics",
                sql: "status IN ('ACTIVE', 'INACTIVE', 'ARCHIVED')");

            migrationBuilder.CreateIndex(
                name: "IX_learning_objectives_created_by",
                table: "learning_objectives",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_learning_objectives_updated_by",
                table: "learning_objectives",
                column: "updated_by");

            migrationBuilder.AddCheckConstraint(
                name: "CK_objective_cognitive_level",
                table: "learning_objectives",
                sql: "cognitive_level IN ('REMEMBER', 'UNDERSTAND', 'APPLY', 'ANALYZE', 'CREATE')");

            migrationBuilder.CreateIndex(
                name: "IX_english_skills_created_by",
                table: "english_skills",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_english_skills_updated_by",
                table: "english_skills",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_english_proficiency_levels_created_by",
                table: "english_proficiency_levels",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_english_proficiency_levels_updated_by",
                table: "english_proficiency_levels",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_topic_prerequisites_prerequisite_topic_id",
                table: "topic_prerequisites",
                column: "prerequisite_topic_id");

            migrationBuilder.CreateIndex(
                name: "UQ_topic_prerequisite",
                table: "topic_prerequisites",
                columns: new[] { "topic_id", "prerequisite_topic_id" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_levels_created_by",
                table: "english_proficiency_levels",
                column: "created_by",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_levels_updated_by",
                table: "english_proficiency_levels",
                column: "updated_by",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_skills_created_by",
                table: "english_skills",
                column: "created_by",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_skills_updated_by",
                table: "english_skills",
                column: "updated_by",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_objectives_created_by",
                table: "learning_objectives",
                column: "created_by",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_objectives_updated_by",
                table: "learning_objectives",
                column: "updated_by",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_topics_updated_by",
                table: "learning_topics",
                column: "updated_by",
                principalTable: "users",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_levels_created_by",
                table: "english_proficiency_levels");

            migrationBuilder.DropForeignKey(
                name: "FK_levels_updated_by",
                table: "english_proficiency_levels");

            migrationBuilder.DropForeignKey(
                name: "FK_skills_created_by",
                table: "english_skills");

            migrationBuilder.DropForeignKey(
                name: "FK_skills_updated_by",
                table: "english_skills");

            migrationBuilder.DropForeignKey(
                name: "FK_objectives_created_by",
                table: "learning_objectives");

            migrationBuilder.DropForeignKey(
                name: "FK_objectives_updated_by",
                table: "learning_objectives");

            migrationBuilder.DropForeignKey(
                name: "FK_topics_updated_by",
                table: "learning_topics");

            migrationBuilder.DropTable(
                name: "topic_prerequisites");

            migrationBuilder.DropIndex(
                name: "IX_learning_topics_updated_by",
                table: "learning_topics");

            migrationBuilder.DropIndex(
                name: "IX_topics_order_index",
                table: "learning_topics");

            migrationBuilder.DropIndex(
                name: "IX_topics_status",
                table: "learning_topics");

            migrationBuilder.DropCheckConstraint(
                name: "CK_topic_difficulty_level",
                table: "learning_topics");

            migrationBuilder.DropCheckConstraint(
                name: "CK_topic_status",
                table: "learning_topics");

            migrationBuilder.DropIndex(
                name: "IX_learning_objectives_created_by",
                table: "learning_objectives");

            migrationBuilder.DropIndex(
                name: "IX_learning_objectives_updated_by",
                table: "learning_objectives");

            migrationBuilder.DropCheckConstraint(
                name: "CK_objective_cognitive_level",
                table: "learning_objectives");

            migrationBuilder.DropIndex(
                name: "IX_english_skills_created_by",
                table: "english_skills");

            migrationBuilder.DropIndex(
                name: "IX_english_skills_updated_by",
                table: "english_skills");

            migrationBuilder.DropIndex(
                name: "IX_english_proficiency_levels_created_by",
                table: "english_proficiency_levels");

            migrationBuilder.DropIndex(
                name: "IX_english_proficiency_levels_updated_by",
                table: "english_proficiency_levels");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "learning_topics");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "learning_objectives");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "learning_objectives");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "learning_objectives");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "learning_objectives");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "english_skills");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "english_skills");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "english_skills");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "english_skills");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "english_proficiency_levels");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "english_proficiency_levels");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "english_proficiency_levels");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "english_proficiency_levels");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "english_proficiency_levels");

            migrationBuilder.RenameIndex(
                name: "IX_topics_parent_topic_id",
                table: "learning_topics",
                newName: "IX_learning_topics_parent_topic_id");

            migrationBuilder.RenameColumn(
                name: "level_name",
                table: "english_proficiency_levels",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "level_code",
                table: "english_proficiency_levels",
                newName: "code");

            migrationBuilder.AlterColumn<string>(
                name: "difficulty_level",
                table: "learning_topics",
                type: "varchar(30)",
                unicode: false,
                maxLength: 30,
                nullable: false,
                defaultValue: "BASIC",
                oldClrType: typeof(string),
                oldType: "varchar(30)",
                oldUnicode: false,
                oldMaxLength: 30,
                oldDefaultValue: "BEGINNER");
        }
    }
}
