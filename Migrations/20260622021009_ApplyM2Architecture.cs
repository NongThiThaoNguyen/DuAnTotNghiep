using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DuAnTotNghiep.Migrations
{
    /// <inheritdoc />
    public partial class ApplyM2Architecture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "avatar_url",
                table: "users",
                type: "varchar(500)",
                unicode: false,
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "phone",
                table: "users",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "user_avatar_histories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    old_avatar_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    new_avatar_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    changed_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_avatar_histories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_avatar_histories_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    language = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "vi-VN"),
                    timezone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "Asia/Ho_Chi_Minh"),
                    email_notifications = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    study_reminder_enabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    theme = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "light")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_settings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_settings_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(@"
                -- Copy phone and avatar_url back to users
                UPDATE u
                SET u.avatar_url = up.avatar_url,
                    u.phone = up.phone
                FROM users u
                INNER JOIN user_profiles up ON u.id = up.user_id;

                -- Copy settings to user_settings
                INSERT INTO user_settings (UserId, language, timezone)
                SELECT user_id, language, timezone
                FROM user_profiles;
            ");

            migrationBuilder.DropColumn(
                name: "avatar_url",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "language",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "notification_preferences",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "phone",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "timezone",
                table: "user_profiles");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "avatar_url", "phone" },
                values: new object[] { null, null });

            migrationBuilder.CreateIndex(
                name: "IX_user_avatar_histories_UserId",
                table: "user_avatar_histories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_settings_UserId",
                table: "user_settings",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "avatar_url",
                table: "user_profiles",
                type: "varchar(500)",
                unicode: false,
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "language",
                table: "user_profiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                defaultValue: "vi-VN");

            migrationBuilder.AddColumn<string>(
                name: "notification_preferences",
                table: "user_profiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "phone",
                table: "user_profiles",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "timezone",
                table: "user_profiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                defaultValue: "Asia/Ho_Chi_Minh");

            migrationBuilder.Sql(@"
                UPDATE up
                SET up.avatar_url = u.avatar_url,
                    up.phone = u.phone,
                    up.language = us.language,
                    up.timezone = us.timezone
                FROM user_profiles up
                INNER JOIN users u ON up.user_id = u.id
                LEFT JOIN user_settings us ON up.user_id = us.UserId;
            ");

            migrationBuilder.DropTable(
                name: "user_avatar_histories");

            migrationBuilder.DropTable(
                name: "user_settings");

            migrationBuilder.DropColumn(
                name: "avatar_url",
                table: "users");

            migrationBuilder.DropColumn(
                name: "phone",
                table: "users");
        }
    }
}
