using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KYCService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConsentFieldsToKYCProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BiometricConsentGivenAt",
                table: "kyc_profiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConsentGivenAt",
                table: "kyc_profiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsentVersion",
                table: "kyc_profiles",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BiometricConsentGivenAt",
                table: "kyc_profiles");

            migrationBuilder.DropColumn(
                name: "ConsentGivenAt",
                table: "kyc_profiles");

            migrationBuilder.DropColumn(
                name: "ConsentVersion",
                table: "kyc_profiles");
        }
    }
}
