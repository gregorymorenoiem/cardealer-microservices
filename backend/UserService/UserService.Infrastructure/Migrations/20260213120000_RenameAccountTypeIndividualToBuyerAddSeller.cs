using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameAccountTypeIndividualToBuyerAddSeller : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Migrate existing data: "Individual" → "Buyer"
            migrationBuilder.Sql(
                """
                UPDATE "Users" SET "AccountType" = 'Buyer' WHERE "AccountType" = 'Individual';
                """);

            // 2. Update the default value from "Individual" to "Buyer"
            migrationBuilder.AlterColumn<string>(
                name: "AccountType",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Buyer",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Individual");

            // 3. Also rename SellerProfile SellerType "Individual" (stored as string) if any exist
            // SellerType is stored as int, so no data migration needed for that column
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert default value
            migrationBuilder.AlterColumn<string>(
                name: "AccountType",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Individual",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Buyer");

            // Revert data: "Buyer" → "Individual"
            migrationBuilder.Sql(
                """
                UPDATE "Users" SET "AccountType" = 'Individual' WHERE "AccountType" = 'Buyer';
                """);
        }
    }
}
