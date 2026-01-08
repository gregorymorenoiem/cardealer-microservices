using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSellerProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfilePicture",
                table: "Users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Province",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessName",
                table: "Users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessAddress",
                table: "Users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessPhone",
                table: "Users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessWebsite",
                table: "Users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessDescription",
                table: "Users",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YearsInBusiness",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerifiedDealer",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "AverageRating",
                table: "Users",
                type: "numeric(2,1)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalReviews",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProfileCompletedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            // Create indexes for better query performance
            migrationBuilder.CreateIndex(
                name: "IX_Users_City",
                table: "Users",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Province",
                table: "Users",
                column: "Province");

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsVerifiedDealer",
                table: "Users",
                column: "IsVerifiedDealer");

            migrationBuilder.CreateIndex(
                name: "IX_Users_AverageRating",
                table: "Users",
                column: "AverageRating");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_City",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Province",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_IsVerifiedDealer",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_AverageRating",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProfilePicture",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BusinessName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BusinessAddress",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BusinessPhone",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BusinessWebsite",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BusinessDescription",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "YearsInBusiness",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsVerifiedDealer",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AverageRating",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TotalReviews",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProfileCompletedAt",
                table: "Users");
        }
    }
}