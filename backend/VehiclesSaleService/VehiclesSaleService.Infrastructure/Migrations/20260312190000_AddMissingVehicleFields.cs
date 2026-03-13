using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehiclesSaleService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingVehicleFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Fraud Detection ──
            migrationBuilder.AddColumn<int>(
                name: "FraudScore",
                table: "vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // ── Odometer Verification (VinAudit/CARFAX) ──
            migrationBuilder.AddColumn<bool>(
                name: "OdometerRollbackDetected",
                table: "vehicles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "HistoricalMileage",
                table: "vehicles",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OdometerVerifiedAt",
                table: "vehicles",
                type: "timestamp with time zone",
                nullable: true);

            // ── Image Health (CDN URL Verification) ──
            migrationBuilder.AddColumn<bool>(
                name: "HasBrokenImages",
                table: "vehicles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "BrokenImagesDetectedAt",
                table: "vehicles",
                type: "timestamp with time zone",
                nullable: true);

            // ── Buyer Reports ──
            migrationBuilder.AddColumn<int>(
                name: "ReportCount",
                table: "vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "SuspendedAt",
                table: "vehicles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuspendedReason",
                table: "vehicles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "FraudScore", table: "vehicles");
            migrationBuilder.DropColumn(name: "OdometerRollbackDetected", table: "vehicles");
            migrationBuilder.DropColumn(name: "HistoricalMileage", table: "vehicles");
            migrationBuilder.DropColumn(name: "OdometerVerifiedAt", table: "vehicles");
            migrationBuilder.DropColumn(name: "HasBrokenImages", table: "vehicles");
            migrationBuilder.DropColumn(name: "BrokenImagesDetectedAt", table: "vehicles");
            migrationBuilder.DropColumn(name: "ReportCount", table: "vehicles");
            migrationBuilder.DropColumn(name: "SuspendedAt", table: "vehicles");
            migrationBuilder.DropColumn(name: "SuspendedReason", table: "vehicles");
        }
    }
}
