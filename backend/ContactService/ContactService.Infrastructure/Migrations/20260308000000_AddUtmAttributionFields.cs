using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContactService.Infrastructure.Migrations;

/// <summary>
/// SEM FIX: Add UTM attribution fields to ContactRequests table.
/// Tracks which ad campaign (Google Ads, Facebook, etc.) generated each lead.
/// </summary>
public partial class AddUtmAttributionFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "UtmSource",
            table: "ContactRequests",
            type: "character varying(200)",
            maxLength: 200,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "UtmMedium",
            table: "ContactRequests",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "UtmCampaign",
            table: "ContactRequests",
            type: "character varying(200)",
            maxLength: 200,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "UtmTerm",
            table: "ContactRequests",
            type: "character varying(500)",
            maxLength: 500,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "UtmContent",
            table: "ContactRequests",
            type: "character varying(500)",
            maxLength: 500,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Gclid",
            table: "ContactRequests",
            type: "character varying(200)",
            maxLength: 200,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Fbclid",
            table: "ContactRequests",
            type: "character varying(200)",
            maxLength: 200,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "LandingPage",
            table: "ContactRequests",
            type: "character varying(500)",
            maxLength: 500,
            nullable: true);

        // Index on UtmSource for campaign-level reporting
        migrationBuilder.CreateIndex(
            name: "IX_ContactRequests_UtmSource",
            table: "ContactRequests",
            column: "UtmSource");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_ContactRequests_UtmSource",
            table: "ContactRequests");

        migrationBuilder.DropColumn(name: "UtmSource", table: "ContactRequests");
        migrationBuilder.DropColumn(name: "UtmMedium", table: "ContactRequests");
        migrationBuilder.DropColumn(name: "UtmCampaign", table: "ContactRequests");
        migrationBuilder.DropColumn(name: "UtmTerm", table: "ContactRequests");
        migrationBuilder.DropColumn(name: "UtmContent", table: "ContactRequests");
        migrationBuilder.DropColumn(name: "Gclid", table: "ContactRequests");
        migrationBuilder.DropColumn(name: "Fbclid", table: "ContactRequests");
        migrationBuilder.DropColumn(name: "LandingPage", table: "ContactRequests");
    }
}
