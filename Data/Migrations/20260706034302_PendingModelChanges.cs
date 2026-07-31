using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DuAnTotNghiep.Data.Migrations
{
    /// <inheritdoc />
    public partial class PendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_sal_node",
                table: "study_activity_logs");

            migrationBuilder.AlterColumn<decimal>(
                name: "LowScoreThreshold",
                table: "ReplanningRules",
                type: "decimal(5,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "FastProgressScoreThreshold",
                table: "ReplanningRules",
                type: "decimal(5,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "reference_sources",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldUnicode: false,
                oldMaxLength: 50,
                oldDefaultValue: "PENDING");

            // migrationBuilder.AddColumn<string>(
            //     name: "video_url",
            //     table: "original_lessons",
            //     type: "nvarchar(2000)",
            //     maxLength: 2000,
            //     nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_study_activity_logs_learning_path_nodes_learning_path_node_id",
                table: "study_activity_logs",
                column: "learning_path_node_id",
                principalTable: "learning_path_nodes",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_study_activity_logs_learning_path_nodes_learning_path_node_id",
                table: "study_activity_logs");

            migrationBuilder.DropColumn(
                name: "video_url",
                table: "original_lessons");

            migrationBuilder.AlterColumn<decimal>(
                name: "LowScoreThreshold",
                table: "ReplanningRules",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "FastProgressScoreThreshold",
                table: "ReplanningRules",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "reference_sources",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: false,
                defaultValue: "PENDING",
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldUnicode: false,
                oldMaxLength: 50);

            migrationBuilder.AddForeignKey(
                name: "FK_sal_node",
                table: "study_activity_logs",
                column: "learning_path_node_id",
                principalTable: "learning_path_nodes",
                principalColumn: "id");
        }
    }
}
