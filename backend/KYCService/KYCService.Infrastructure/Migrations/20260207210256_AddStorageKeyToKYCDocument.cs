using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KYCService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStorageKeyToKYCDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "storage_key",
                table: "kyc_documents",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "storage_key",
                table: "kyc_documents");
        }
    }
}
