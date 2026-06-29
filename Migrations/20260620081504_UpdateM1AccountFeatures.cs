using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DuAnTotNghiep.Migrations
{
    /// <inheritdoc />
    public partial class UpdateM1AccountFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "failed_login_count",
                table: "users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "lockout_until",
                table: "users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "revoked_at",
                table: "user_sessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "login_logs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    ip_address = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    user_agent = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    is_success = table.Column<bool>(type: "bit", nullable: false),
                    failure_reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_login_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_login_logs_users",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "password_reset_tokens",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    token_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    used_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_password_reset_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_password_reset_tokens_users",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });


            migrationBuilder.AddCheckConstraint(
                name: "CK_User_Status",
                table: "users",
                sql: "[status] IN ('ACTIVE', 'LOCKED', 'PENDING', 'DELETED')");

            migrationBuilder.CreateIndex(
                name: "IX_login_logs_user_id",
                table: "login_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_password_reset_tokens_user_id",
                table: "password_reset_tokens",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "login_logs");

            migrationBuilder.DropTable(
                name: "password_reset_tokens");

            migrationBuilder.DropCheckConstraint(
                name: "CK_User_Status",
                table: "users");


            migrationBuilder.DropColumn(
                name: "failed_login_count",
                table: "users");

            migrationBuilder.DropColumn(
                name: "lockout_until",
                table: "users");

            migrationBuilder.DropColumn(
                name: "revoked_at",
                table: "user_sessions");
        }
    }
}
