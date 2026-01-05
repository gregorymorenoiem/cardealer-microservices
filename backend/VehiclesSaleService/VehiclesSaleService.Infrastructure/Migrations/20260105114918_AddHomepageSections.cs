using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VehiclesSaleService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHomepageSections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HomepageSections",
                table: "vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "homepage_section_configs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    MaxItems = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Icon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AccentColor = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "blue"),
                    ViewAllHref = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LayoutType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Subtitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_homepage_section_configs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_homepage_sections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    HomepageSectionConfigId = table.Column<Guid>(type: "uuid", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsPinned = table.Column<bool>(type: "boolean", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_homepage_sections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vehicle_homepage_sections_homepage_section_configs_Homepage~",
                        column: x => x.HomepageSectionConfigId,
                        principalTable: "homepage_section_configs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_vehicle_homepage_sections_vehicles_VehicleId",
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

            migrationBuilder.InsertData(
                table: "homepage_section_configs",
                columns: new[] { "Id", "AccentColor", "CreatedAt", "Description", "DisplayOrder", "Icon", "IsActive", "LayoutType", "MaxItems", "Name", "Slug", "Subtitle", "UpdatedAt", "ViewAllHref" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), "blue", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Carousel hero principal del homepage", 1, "FaCar", true, "Hero", 5, "Carousel Principal", "carousel", "Los mejores vehículos del momento", new DateTime(2026, 1, 5, 11, 49, 18, 446, DateTimeKind.Utc).AddTicks(9470), "/vehicles" },
                    { new Guid("10000000-0000-0000-0000-000000000002"), "blue", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sedanes elegantes y confortables", 2, "FaCar", true, "Carousel", 10, "Sedanes", "sedanes", "Elegancia y confort para tu día a día", new DateTime(2026, 1, 5, 11, 49, 18, 446, DateTimeKind.Utc).AddTicks(9470), "/vehicles?bodyStyle=Sedan" },
                    { new Guid("10000000-0000-0000-0000-000000000003"), "blue", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SUVs y Crossovers versátiles", 3, "FaCar", true, "Carousel", 10, "SUVs", "suvs", "Espacio, potencia y versatilidad", new DateTime(2026, 1, 5, 11, 49, 18, 446, DateTimeKind.Utc).AddTicks(9470), "/vehicles?bodyStyle=SUV" },
                    { new Guid("10000000-0000-0000-0000-000000000004"), "blue", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Pickups y camionetas de trabajo", 4, "FaTruck", true, "Carousel", 10, "Camionetas", "camionetas", "Potencia y capacidad para cualquier trabajo", new DateTime(2026, 1, 5, 11, 49, 18, 446, DateTimeKind.Utc).AddTicks(9480), "/vehicles?bodyStyle=Pickup" },
                    { new Guid("10000000-0000-0000-0000-000000000005"), "red", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Autos deportivos y de alto rendimiento", 5, "FaCar", true, "Carousel", 10, "Deportivos", "deportivos", "Velocidad y adrenalina en cada curva", new DateTime(2026, 1, 5, 11, 49, 18, 446, DateTimeKind.Utc).AddTicks(9480), "/vehicles?bodyStyle=SportsCar" },
                    { new Guid("10000000-0000-0000-0000-000000000006"), "amber", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Vehículos destacados de la semana", 6, "FiStar", true, "Grid", 8, "Destacados", "destacados", "Selección exclusiva de nuestros mejores anuncios", new DateTime(2026, 1, 5, 11, 49, 18, 446, DateTimeKind.Utc).AddTicks(9480), "/vehicles?featured=true" },
                    { new Guid("10000000-0000-0000-0000-000000000007"), "purple", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Vehículos de lujo y premium", 7, "FiStar", true, "Carousel", 10, "Lujo", "lujo", "Exclusividad y prestigio", new DateTime(2026, 1, 5, 11, 49, 18, 446, DateTimeKind.Utc).AddTicks(9480), "/vehicles?minPrice=80000" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_homepage_section_configs_DisplayOrder",
                table: "homepage_section_configs",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_homepage_section_configs_IsActive",
                table: "homepage_section_configs",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_homepage_section_configs_Slug",
                table: "homepage_section_configs",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_homepage_sections_HomepageSectionConfigId",
                table: "vehicle_homepage_sections",
                column: "HomepageSectionConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_homepage_sections_IsPinned",
                table: "vehicle_homepage_sections",
                column: "IsPinned");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_homepage_sections_SortOrder",
                table: "vehicle_homepage_sections",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_homepage_sections_VehicleId_HomepageSectionConfigId",
                table: "vehicle_homepage_sections",
                columns: new[] { "VehicleId", "HomepageSectionConfigId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vehicle_homepage_sections");

            migrationBuilder.DropTable(
                name: "homepage_section_configs");

            migrationBuilder.DropColumn(
                name: "HomepageSections",
                table: "vehicles");

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 5, 2, 58, 59, 24, DateTimeKind.Utc).AddTicks(3850));

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 5, 2, 58, 59, 24, DateTimeKind.Utc).AddTicks(3870));

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 5, 2, 58, 59, 24, DateTimeKind.Utc).AddTicks(3870));

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 5, 2, 58, 59, 24, DateTimeKind.Utc).AddTicks(3880));

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 5, 2, 58, 59, 24, DateTimeKind.Utc).AddTicks(3880));

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 5, 2, 58, 59, 24, DateTimeKind.Utc).AddTicks(3880));
        }
    }
}
