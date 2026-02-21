using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AdvertisingService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ad_campaigns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PlacementType = table.Column<int>(type: "integer", nullable: false),
                    PricingModel = table.Column<int>(type: "integer", nullable: false),
                    PricePerView = table.Column<decimal>(type: "numeric(10,4)", nullable: true),
                    FixedPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    TotalBudget = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SpentBudget = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    TotalViewsPurchased = table.Column<int>(type: "integer", nullable: true),
                    ViewsConsumed = table.Column<int>(type: "integer", nullable: false),
                    Clicks = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BillingReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    QualityScore = table.Column<decimal>(type: "numeric(3,2)", nullable: false, defaultValue: 0.50m),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ad_campaigns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "brand_configs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BrandKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LogoInitials = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    VehicleCount = table.Column<int>(type: "integer", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false),
                    Route = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_brand_configs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "category_image_configs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryKey = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IconUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Gradient = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "from-blue-600 to-blue-800"),
                    VehicleCount = table.Column<int>(type: "integer", nullable: false),
                    IsTrending = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false),
                    Route = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category_image_configs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "rotation_configs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Section = table.Column<int>(type: "integer", nullable: false),
                    AlgorithmType = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    RefreshIntervalMinutes = table.Column<int>(type: "integer", nullable: false),
                    MaxVehiclesShown = table.Column<int>(type: "integer", nullable: false),
                    WeightRemainingBudget = table.Column<decimal>(type: "numeric(3,2)", nullable: false, defaultValue: 0.30m),
                    WeightCtr = table.Column<decimal>(type: "numeric(3,2)", nullable: false, defaultValue: 0.25m),
                    WeightQualityScore = table.Column<decimal>(type: "numeric(3,2)", nullable: false, defaultValue: 0.25m),
                    WeightRecency = table.Column<decimal>(type: "numeric(3,2)", nullable: false, defaultValue: 0.20m),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rotation_configs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ad_clicks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImpressionId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ad_clicks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ad_clicks_ad_campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "ad_campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ad_impressions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IpHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Section = table.Column<int>(type: "integer", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ad_impressions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ad_impressions_ad_campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "ad_campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "brand_configs",
                columns: new[] { "Id", "BrandKey", "DisplayName", "DisplayOrder", "IsVisible", "LogoInitials", "LogoUrl", "Route", "UpdatedAt", "UpdatedBy", "VehicleCount" },
                values: new object[,]
                {
                    { new Guid("22222222-0000-0000-0000-000000000001"), "toyota", "Toyota", 1, true, "TO", null, "/buscar?marca=toyota", new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("22222222-0000-0000-0000-000000000002"), "honda", "Honda", 2, true, "HO", null, "/buscar?marca=honda", new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("22222222-0000-0000-0000-000000000003"), "hyundai", "Hyundai", 3, true, "HY", null, "/buscar?marca=hyundai", new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("22222222-0000-0000-0000-000000000004"), "kia", "Kia", 4, true, "KI", null, "/buscar?marca=kia", new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("22222222-0000-0000-0000-000000000005"), "nissan", "Nissan", 5, true, "NI", null, "/buscar?marca=nissan", new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("22222222-0000-0000-0000-000000000006"), "mazda", "Mazda", 6, true, "MA", null, "/buscar?marca=mazda", new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("22222222-0000-0000-0000-000000000007"), "ford", "Ford", 7, true, "FO", null, "/buscar?marca=ford", new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("22222222-0000-0000-0000-000000000008"), "chevrolet", "Chevrolet", 8, true, "CH", null, "/buscar?marca=chevrolet", new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("22222222-0000-0000-0000-000000000009"), "bmw", "BMW", 9, true, "BM", null, "/buscar?marca=bmw", new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("22222222-0000-0000-0000-00000000000a"), "mercedes-benz", "Mercedes-Benz", 10, true, "ME", null, "/buscar?marca=mercedes-benz", new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("22222222-0000-0000-0000-00000000000b"), "audi", "Audi", 11, true, "AU", null, "/buscar?marca=audi", new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("22222222-0000-0000-0000-00000000000c"), "volkswagen", "Volkswagen", 12, true, "VO", null, "/buscar?marca=volkswagen", new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 }
                });

            migrationBuilder.InsertData(
                table: "category_image_configs",
                columns: new[] { "Id", "CategoryKey", "Description", "DisplayName", "DisplayOrder", "Gradient", "IconUrl", "ImageUrl", "IsTrending", "IsVisible", "Route", "UpdatedAt", "UpdatedBy", "VehicleCount" },
                values: new object[,]
                {
                    { new Guid("33333333-0000-0000-0000-000000000001"), "suv", "Versatilidad y espacio para toda la familia", "SUV", 1, "from-blue-600 to-blue-800", null, "", true, true, "/buscar?tipo=suv", new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("33333333-0000-0000-0000-000000000002"), "sedan", "Elegancia y eficiencia para el día a día", "Sedán", 2, "from-primary to-primary/90", null, "", false, true, "/buscar?tipo=sedan", new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("33333333-0000-0000-0000-000000000003"), "camioneta", "Potencia y capacidad de carga", "Camioneta", 3, "from-amber-600 to-amber-800", null, "", false, true, "/buscar?tipo=camioneta", new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("33333333-0000-0000-0000-000000000004"), "deportivo", "Rendimiento y adrenalina pura", "Deportivo", 4, "from-red-600 to-red-800", null, "", false, true, "/buscar?tipo=deportivo", new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("33333333-0000-0000-0000-000000000005"), "electrico", "El futuro de la movilidad sostenible", "Eléctrico", 5, "from-green-600 to-green-800", null, "", true, true, "/buscar?tipo=electrico", new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("33333333-0000-0000-0000-000000000006"), "hibrido", "Lo mejor de dos mundos", "Híbrido", 6, "from-teal-600 to-teal-800", null, "", false, true, "/buscar?tipo=hibrido", new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 }
                });

            migrationBuilder.InsertData(
                table: "rotation_configs",
                columns: new[] { "Id", "CreatedAt", "IsActive", "MaxVehiclesShown", "RefreshIntervalMinutes", "Section", "UpdatedAt", "UpdatedBy", "WeightCtr", "WeightQualityScore", "WeightRecency", "WeightRemainingBudget" },
                values: new object[] { new Guid("11111111-0000-0000-0000-000000000001"), new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), true, 8, 30, 0, new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, 0.25m, 0.25m, 0.20m, 0.30m });

            migrationBuilder.InsertData(
                table: "rotation_configs",
                columns: new[] { "Id", "AlgorithmType", "CreatedAt", "IsActive", "MaxVehiclesShown", "RefreshIntervalMinutes", "Section", "UpdatedAt", "UpdatedBy", "WeightCtr", "WeightQualityScore", "WeightRecency", "WeightRemainingBudget" },
                values: new object[] { new Guid("11111111-0000-0000-0000-000000000002"), 3, new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), true, 4, 60, 1, new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, 0.20m, 0.20m, 0.20m, 0.40m });

            migrationBuilder.CreateIndex(
                name: "idx_ad_campaigns_owner",
                table: "ad_campaigns",
                columns: new[] { "OwnerId", "OwnerType" });

            migrationBuilder.CreateIndex(
                name: "idx_ad_campaigns_placement_status",
                table: "ad_campaigns",
                columns: new[] { "PlacementType", "Status" });

            migrationBuilder.CreateIndex(
                name: "idx_ad_campaigns_vehicle",
                table: "ad_campaigns",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "idx_ad_clicks_campaign",
                table: "ad_clicks",
                columns: new[] { "CampaignId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "idx_ad_impressions_campaign",
                table: "ad_impressions",
                columns: new[] { "CampaignId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_brand_configs_BrandKey",
                table: "brand_configs",
                column: "BrandKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_category_image_configs_CategoryKey",
                table: "category_image_configs",
                column: "CategoryKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rotation_configs_Section",
                table: "rotation_configs",
                column: "Section",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ad_clicks");

            migrationBuilder.DropTable(
                name: "ad_impressions");

            migrationBuilder.DropTable(
                name: "brand_configs");

            migrationBuilder.DropTable(
                name: "category_image_configs");

            migrationBuilder.DropTable(
                name: "rotation_configs");

            migrationBuilder.DropTable(
                name: "ad_campaigns");
        }
    }
}
