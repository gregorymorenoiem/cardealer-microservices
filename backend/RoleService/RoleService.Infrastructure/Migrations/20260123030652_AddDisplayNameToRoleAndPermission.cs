using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoleService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDisplayNameToRoleAndPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Roles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Permissions",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Permissions");
        }
    }
}
