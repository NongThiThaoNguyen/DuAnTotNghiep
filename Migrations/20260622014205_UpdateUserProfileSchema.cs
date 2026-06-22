using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DuAnTotNghiep.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserProfileSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_profiles_users",
                table: "user_profiles");

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

            // Copy data from users to user_profiles
            migrationBuilder.Sql(@"
                UPDATE up
                SET up.avatar_url = u.avatar_url,
                    up.phone = u.phone
                FROM user_profiles up
                INNER JOIN users u ON up.user_id = u.id;
            ");

            migrationBuilder.DropColumn(
                name: "avatar_url",
                table: "users");

            migrationBuilder.DropColumn(
                name: "phone",
                table: "users");

            migrationBuilder.AddForeignKey(
                name: "FK_user_profiles_users",
                table: "user_profiles",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_profiles_users",
                table: "user_profiles");

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

            // Copy data back from user_profiles to users
            migrationBuilder.Sql(@"
                UPDATE u
                SET u.avatar_url = up.avatar_url,
                    u.phone = up.phone
                FROM users u
                INNER JOIN user_profiles up ON u.id = up.user_id;
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

            migrationBuilder.AddForeignKey(
                name: "FK_user_profiles_users",
                table: "user_profiles",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id");
        }
    }
}
