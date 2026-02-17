using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComparisonService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "comparisons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    VehicleIds = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    ShareToken = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comparisons", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_comparisons_CreatedAt",
                table: "comparisons",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_comparisons_ShareToken",
                table: "comparisons",
                column: "ShareToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_comparisons_UserId",
                table: "comparisons",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "comparisons");
        }
    }
}
