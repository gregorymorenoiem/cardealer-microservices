using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KYCService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSectorToKYCProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "sector",
                table: "kyc_profiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sector",
                table: "kyc_profiles");
        }
    }
}
