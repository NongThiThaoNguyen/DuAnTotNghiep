using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DuAnTotNghiep.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderIndexToGoal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('learning_goals') AND name = 'order_index')
                BEGIN
                    ALTER TABLE [learning_goals] ADD [order_index] int NOT NULL DEFAULT 0;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('learning_goals') AND name = 'order_index')
                BEGIN
                    ALTER TABLE [learning_goals] DROP COLUMN [order_index];
                END
            ");
        }
    }
}
