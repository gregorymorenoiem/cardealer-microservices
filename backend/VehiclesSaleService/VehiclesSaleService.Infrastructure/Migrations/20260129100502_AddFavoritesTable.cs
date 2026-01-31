using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehiclesSaleService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFavoritesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "favorites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DealerId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    NotifyPriceChange = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_favorites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_favorites_vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 29, 10, 5, 2, 370, DateTimeKind.Utc).AddTicks(2020));

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 29, 10, 5, 2, 370, DateTimeKind.Utc).AddTicks(2040));

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 29, 10, 5, 2, 370, DateTimeKind.Utc).AddTicks(2060));

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 29, 10, 5, 2, 370, DateTimeKind.Utc).AddTicks(2060));

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 29, 10, 5, 2, 370, DateTimeKind.Utc).AddTicks(2060));

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 29, 10, 5, 2, 370, DateTimeKind.Utc).AddTicks(2060));

            migrationBuilder.UpdateData(
                table: "homepage_section_configs",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "MaxItems", "UpdatedAt" },
                values: new object[] { 10, new DateTime(2026, 1, 29, 10, 5, 2, 370, DateTimeKind.Utc).AddTicks(2730) });

            migrationBuilder.UpdateData(
                table: "homepage_section_configs",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 29, 10, 5, 2, 370, DateTimeKind.Utc).AddTicks(2740));

            migrationBuilder.UpdateData(
                table: "homepage_section_configs",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 29, 10, 5, 2, 370, DateTimeKind.Utc).AddTicks(2740));

            migrationBuilder.UpdateData(
                table: "homepage_section_configs",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 29, 10, 5, 2, 370, DateTimeKind.Utc).AddTicks(2740));

            migrationBuilder.UpdateData(
                table: "homepage_section_configs",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 29, 10, 5, 2, 370, DateTimeKind.Utc).AddTicks(2740));

            migrationBuilder.UpdateData(
                table: "homepage_section_configs",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "MaxItems", "UpdatedAt" },
                values: new object[] { 10, new DateTime(2026, 1, 29, 10, 5, 2, 370, DateTimeKind.Utc).AddTicks(2740) });

            migrationBuilder.UpdateData(
                table: "homepage_section_configs",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 29, 10, 5, 2, 370, DateTimeKind.Utc).AddTicks(2740));

            migrationBuilder.CreateIndex(
                name: "IX_Favorite_DealerId",
                table: "favorites",
                column: "DealerId");

            migrationBuilder.CreateIndex(
                name: "IX_favorites_UserId",
                table: "favorites",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_favorites_UserId_VehicleId",
                table: "favorites",
                columns: new[] { "UserId", "VehicleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_favorites_VehicleId",
                table: "favorites",
                column: "VehicleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "favorites");

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 5, 11, 49, 18, 446, DateTimeKind.Utc).AddTicks(8600));

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 5, 11, 49, 18, 446, DateTimeKind.Utc).AddTicks(8600));

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 5, 11, 49, 18, 446, DateTimeKind.Utc).AddTicks(8600));

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 5, 11, 49, 18, 446, DateTimeKind.Utc).AddTicks(8600));

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 5, 11, 49, 18, 446, DateTimeKind.Utc).AddTicks(8610));

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 5, 11, 49, 18, 446, DateTimeKind.Utc).AddTicks(8610));

            migrationBuilder.UpdateData(
                table: "homepage_section_configs",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "MaxItems", "UpdatedAt" },
                values: new object[] { 5, new DateTime(2026, 1, 5, 11, 49, 18, 446, DateTimeKind.Utc).AddTicks(9470) });

            migrationBuilder.UpdateData(
                table: "homepage_section_configs",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 5, 11, 49, 18, 446, DateTimeKind.Utc).AddTicks(9470));

            migrationBuilder.UpdateData(
                table: "homepage_section_configs",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 5, 11, 49, 18, 446, DateTimeKind.Utc).AddTicks(9470));

            migrationBuilder.UpdateData(
                table: "homepage_section_configs",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 5, 11, 49, 18, 446, DateTimeKind.Utc).AddTicks(9480));

            migrationBuilder.UpdateData(
                table: "homepage_section_configs",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 5, 11, 49, 18, 446, DateTimeKind.Utc).AddTicks(9480));

            migrationBuilder.UpdateData(
                table: "homepage_section_configs",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "MaxItems", "UpdatedAt" },
                values: new object[] { 8, new DateTime(2026, 1, 5, 11, 49, 18, 446, DateTimeKind.Utc).AddTicks(9480) });

            migrationBuilder.UpdateData(
                table: "homepage_section_configs",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 5, 11, 49, 18, 446, DateTimeKind.Utc).AddTicks(9480));
        }
    }
}
