using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContactService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ContactMessages: Add IsFromBuyer column
            migrationBuilder.AddColumn<bool>(
                name: "IsFromBuyer",
                table: "ContactMessages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // ContactMessages: Add DealerId column (multi-tenant)
            migrationBuilder.AddColumn<Guid>(
                name: "DealerId",
                table: "ContactMessages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // ContactMessages: Add SentAt column
            migrationBuilder.AddColumn<DateTime>(
                name: "SentAt",
                table: "ContactMessages",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            // Copy existing CreatedAt data to SentAt
            migrationBuilder.Sql(
                "UPDATE \"ContactMessages\" SET \"SentAt\" = \"CreatedAt\" WHERE \"CreatedAt\" IS NOT NULL;");

            // ContactRequests: Add DealerId column (multi-tenant)
            migrationBuilder.AddColumn<Guid>(
                name: "DealerId",
                table: "ContactRequests",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // ContactRequests: Add RespondedAt column
            migrationBuilder.AddColumn<DateTime>(
                name: "RespondedAt",
                table: "ContactRequests",
                type: "timestamp with time zone",
                nullable: true);

            // Create indexes for new columns
            migrationBuilder.CreateIndex(
                name: "IX_ContactMessages_SentAt",
                table: "ContactMessages",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_ContactMessages_DealerId",
                table: "ContactMessages",
                column: "DealerId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactRequests_DealerId",
                table: "ContactRequests",
                column: "DealerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ContactRequests_DealerId",
                table: "ContactRequests");

            migrationBuilder.DropIndex(
                name: "IX_ContactMessages_DealerId",
                table: "ContactMessages");

            migrationBuilder.DropIndex(
                name: "IX_ContactMessages_SentAt",
                table: "ContactMessages");

            migrationBuilder.DropColumn(
                name: "RespondedAt",
                table: "ContactRequests");

            migrationBuilder.DropColumn(
                name: "DealerId",
                table: "ContactRequests");

            migrationBuilder.DropColumn(
                name: "SentAt",
                table: "ContactMessages");

            migrationBuilder.DropColumn(
                name: "DealerId",
                table: "ContactMessages");

            migrationBuilder.DropColumn(
                name: "IsFromBuyer",
                table: "ContactMessages");
        }
    }
}
