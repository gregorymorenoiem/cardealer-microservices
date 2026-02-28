using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContactService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContactRequestFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ContactRequests: Add Name, Email, Phone fields for generic contact form support
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ContactRequests",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "ContactRequests",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "ContactRequests",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            // ContactRequests: Add ProductId for non-vehicle contact requests
            migrationBuilder.AddColumn<Guid>(
                name: "ProductId",
                table: "ContactRequests",
                type: "uuid",
                nullable: true);

            // ContactRequests: Make VehicleId nullable (was NOT NULL in InitialCreate)
            migrationBuilder.AlterColumn<Guid>(
                name: "VehicleId",
                table: "ContactRequests",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "ContactRequests");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "ContactRequests");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "ContactRequests");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "ContactRequests");

            migrationBuilder.AlterColumn<Guid>(
                name: "VehicleId",
                table: "ContactRequests",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
