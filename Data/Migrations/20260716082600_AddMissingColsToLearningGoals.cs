using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DuAnTotNghiep.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingColsToLearningGoals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('learning_goals') AND name = 'created_at')
                    ALTER TABLE [learning_goals] ADD [created_at] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME();

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('learning_goals') AND name = 'updated_at')
                    ALTER TABLE [learning_goals] ADD [updated_at] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME();

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('learning_goals') AND name = 'created_by')
                    ALTER TABLE [learning_goals] ADD [created_by] INT NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('learning_goals') AND name = 'updated_by')
                    ALTER TABLE [learning_goals] ADD [updated_by] INT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('learning_goals') AND name = 'created_at')
                    ALTER TABLE [learning_goals] DROP COLUMN [created_at];

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('learning_goals') AND name = 'updated_at')
                    ALTER TABLE [learning_goals] DROP COLUMN [updated_at];

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('learning_goals') AND name = 'created_by')
                    ALTER TABLE [learning_goals] DROP COLUMN [created_by];

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('learning_goals') AND name = 'updated_by')
                    ALTER TABLE [learning_goals] DROP COLUMN [updated_by];
            ");
        }
    }
}
