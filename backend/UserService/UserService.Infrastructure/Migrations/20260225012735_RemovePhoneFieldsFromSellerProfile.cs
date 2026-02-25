using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovePhoneFieldsFromSellerProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlternatePhone",
                table: "SellerProfiles");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "SellerProfiles");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "SellerProfiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AlternatePhone",
                table: "SellerProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "SellerProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "SellerProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
