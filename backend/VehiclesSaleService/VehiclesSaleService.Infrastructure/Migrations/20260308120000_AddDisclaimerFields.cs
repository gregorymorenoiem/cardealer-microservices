using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehiclesSaleService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDisclaimerFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DisclaimerAcceptedAt",
                table: "vehicles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisclaimerAcceptedFromIp",
                table: "vehicles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisclaimerTosVersion",
                table: "vehicles",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisclaimerAcceptedAt",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "DisclaimerAcceptedFromIp",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "DisclaimerTosVersion",
                table: "vehicles");
        }
    }
}
