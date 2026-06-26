using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DuAnTotNghiep.Migrations
{
    /// <inheritdoc />
    public partial class UpdateM5SourceTypeCheckConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ref_source_type",
                table: "reference_sources");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ref_usage_policy",
                table: "reference_sources");

            migrationBuilder.AlterColumn<string>(
                name: "usage_policy",
                table: "reference_sources",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_ref_source_type",
                table: "reference_sources",
                sql: "[source_type] IN ('OFFICIAL', 'OPEN_LICENSE', 'SELF_CREATED', 'TEACHER_CREATED', 'REFERENCE_ONLY')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ref_usage_policy",
                table: "reference_sources",
                sql: "[usage_policy] IS NULL OR [usage_policy] IN ('REFERENCE_ONLY', 'OPEN_USE', 'INTERNAL_ONLY')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ref_source_type",
                table: "reference_sources");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ref_usage_policy",
                table: "reference_sources");

            migrationBuilder.AlterColumn<string>(
                name: "usage_policy",
                table: "reference_sources",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldUnicode: false,
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_ref_source_type",
                table: "reference_sources",
                sql: "[source_type] IN ('OFFICIAL', 'REFERENCE_ONLY', 'OPEN_LICENSE', 'INTERNAL')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ref_usage_policy",
                table: "reference_sources",
                sql: "[usage_policy] IS NULL OR [usage_policy] IN ('REFERENCE_ONLY', 'OPEN_USE', 'INTERNAL_ONLY')");
        }
    }
}
