using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DuAnTotNghiep.Migrations
{
    /// <inheritdoc />
    public partial class AddM5LegalReferenceStandardization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "reference_sources",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "reference_sources",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "reference_source_id",
                table: "content_compliance_reviews",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_topic_references_topic_id",
                table: "topic_references",
                column: "topic_id");

            migrationBuilder.CreateIndex(
                name: "IX_reference_sources_source_type",
                table: "reference_sources",
                column: "source_type");

            migrationBuilder.CreateIndex(
                name: "IX_reference_sources_status",
                table: "reference_sources",
                column: "status");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ref_source_type",
                table: "reference_sources",
                sql: "[source_type] IN ('OFFICIAL', 'REFERENCE_ONLY', 'OPEN_LICENSE', 'INTERNAL')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ref_status",
                table: "reference_sources",
                sql: "[status] IN ('DRAFT', 'PENDING', 'APPROVED', 'REJECTED', 'ARCHIVED')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ref_usage_policy",
                table: "reference_sources",
                sql: "[usage_policy] IS NULL OR [usage_policy] IN ('REFERENCE_ONLY', 'OPEN_USE', 'INTERNAL_ONLY')");

            migrationBuilder.CreateIndex(
                name: "IX_content_compliance_reviews_reference_source_id",
                table: "content_compliance_reviews",
                column: "reference_source_id");

            migrationBuilder.CreateIndex(
                name: "IX_content_compliance_reviews_review_status",
                table: "content_compliance_reviews",
                column: "review_status");

            migrationBuilder.AddCheckConstraint(
                name: "CK_content_compliance_reviews_risk",
                table: "content_compliance_reviews",
                sql: "[plagiarism_risk] IS NULL OR [plagiarism_risk] IN ('LOW', 'MEDIUM', 'HIGH')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_content_compliance_reviews_status",
                table: "content_compliance_reviews",
                sql: "[review_status] IN ('APPROVED', 'REJECTED', 'NEEDS_REVISION')");

            migrationBuilder.AddForeignKey(
                name: "FK_content_reviews_ref_source",
                table: "content_compliance_reviews",
                column: "reference_source_id",
                principalTable: "reference_sources",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_content_reviews_ref_source",
                table: "content_compliance_reviews");

            migrationBuilder.DropIndex(
                name: "IX_topic_references_topic_id",
                table: "topic_references");

            migrationBuilder.DropIndex(
                name: "IX_reference_sources_source_type",
                table: "reference_sources");

            migrationBuilder.DropIndex(
                name: "IX_reference_sources_status",
                table: "reference_sources");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ref_source_type",
                table: "reference_sources");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ref_status",
                table: "reference_sources");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ref_usage_policy",
                table: "reference_sources");

            migrationBuilder.DropIndex(
                name: "IX_content_compliance_reviews_reference_source_id",
                table: "content_compliance_reviews");

            migrationBuilder.DropIndex(
                name: "IX_content_compliance_reviews_review_status",
                table: "content_compliance_reviews");

            migrationBuilder.DropCheckConstraint(
                name: "CK_content_compliance_reviews_risk",
                table: "content_compliance_reviews");

            migrationBuilder.DropCheckConstraint(
                name: "CK_content_compliance_reviews_status",
                table: "content_compliance_reviews");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "reference_sources");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "reference_sources");

            migrationBuilder.DropColumn(
                name: "reference_source_id",
                table: "content_compliance_reviews");
        }
    }
}
