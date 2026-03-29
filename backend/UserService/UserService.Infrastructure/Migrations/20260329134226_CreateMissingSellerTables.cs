using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateMissingSellerTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DgiiTaxpayerStatus",
                table: "Dealers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FraudCheckAt",
                table: "Dealers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasRecentFraudReports",
                table: "Dealers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRncVerified",
                table: "Dealers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWhatsAppVerified",
                table: "Dealers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ModeratedListingsCount",
                table: "Dealers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModeratedListingsUpdatedAt",
                table: "Dealers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RncVerifiedAt",
                table: "Dealers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WhatsAppVerifiedAt",
                table: "Dealers",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DgiiTaxpayerStatus",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "FraudCheckAt",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "HasRecentFraudReports",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "IsRncVerified",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "IsWhatsAppVerified",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "ModeratedListingsCount",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "ModeratedListingsUpdatedAt",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "RncVerifiedAt",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "WhatsAppVerifiedAt",
                table: "Dealers");
        }
    }
}
