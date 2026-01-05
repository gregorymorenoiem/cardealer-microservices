using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VehiclesSaleService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DealerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IconUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_categories_categories_ParentId",
                        column: x => x.ParentId,
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_makes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: false),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsPopular = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_makes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "vehicles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DealerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SellerType = table.Column<int>(type: "integer", nullable: false),
                    SellerPhone = table.Column<string>(type: "text", nullable: true),
                    SellerEmail = table.Column<string>(type: "text", nullable: true),
                    SellerWhatsApp = table.Column<string>(type: "text", nullable: true),
                    SellerVerified = table.Column<bool>(type: "boolean", nullable: false),
                    SellerRating = table.Column<decimal>(type: "numeric", nullable: true),
                    SellerReviewCount = table.Column<int>(type: "integer", nullable: true),
                    SellerCity = table.Column<string>(type: "text", nullable: true),
                    SellerState = table.Column<string>(type: "text", nullable: true),
                    SellerLogoUrl = table.Column<string>(type: "text", nullable: true),
                    VIN = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: true),
                    StockNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MakeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Make = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ModelId = table.Column<Guid>(type: "uuid", nullable: true),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Trim = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Generation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    VehicleType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    BodyStyle = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Doors = table.Column<int>(type: "integer", nullable: false),
                    Seats = table.Column<int>(type: "integer", nullable: false),
                    FuelType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EngineSize = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Horsepower = table.Column<int>(type: "integer", nullable: true),
                    Torque = table.Column<int>(type: "integer", nullable: true),
                    Transmission = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DriveType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Cylinders = table.Column<int>(type: "integer", nullable: true),
                    Mileage = table.Column<int>(type: "integer", nullable: false),
                    MileageUnit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Condition = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PreviousOwners = table.Column<int>(type: "integer", nullable: true),
                    AccidentHistory = table.Column<bool>(type: "boolean", nullable: false),
                    HasCleanTitle = table.Column<bool>(type: "boolean", nullable: false),
                    ExteriorColor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    InteriorColor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    InteriorMaterial = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MpgCity = table.Column<int>(type: "integer", nullable: true),
                    MpgHighway = table.Column<int>(type: "integer", nullable: true),
                    MpgCombined = table.Column<int>(type: "integer", nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ZipCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, defaultValue: "USA"),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    IsCertified = table.Column<bool>(type: "boolean", nullable: false),
                    CertificationProgram = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CarfaxReportUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LastServiceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ServiceHistoryNotes = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    WarrantyInfo = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FeaturesJson = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "[]"),
                    PackagesJson = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "[]"),
                    ViewCount = table.Column<int>(type: "integer", nullable: false),
                    FavoriteCount = table.Column<int>(type: "integer", nullable: false),
                    InquiryCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SoldAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vehicles_categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_models",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MakeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: false),
                    VehicleType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DefaultBodyStyle = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    StartYear = table.Column<int>(type: "integer", nullable: true),
                    EndYear = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsPopular = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_models", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vehicle_models_vehicle_makes_MakeId",
                        column: x => x.MakeId,
                        principalTable: "vehicle_makes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_images",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DealerId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Caption = table.Column<string>(type: "text", nullable: true),
                    ImageType = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    MimeType = table.Column<string>(type: "text", nullable: true),
                    Width = table.Column<int>(type: "integer", nullable: true),
                    Height = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vehicle_images_vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_trims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    BaseMSRP = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    EngineSize = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Horsepower = table.Column<int>(type: "integer", nullable: true),
                    Torque = table.Column<int>(type: "integer", nullable: true),
                    FuelType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Transmission = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    DriveType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    MpgCity = table.Column<int>(type: "integer", nullable: true),
                    MpgHighway = table.Column<int>(type: "integer", nullable: true),
                    MpgCombined = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_trims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vehicle_trims_vehicle_models_ModelId",
                        column: x => x.ModelId,
                        principalTable: "vehicle_models",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "categories",
                columns: new[] { "Id", "CreatedAt", "DealerId", "Description", "IconUrl", "IsActive", "IsSystem", "Level", "Name", "ParentId", "Slug", "SortOrder", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), "Sedans, coupes, hatchbacks and sports cars", null, true, false, 0, "Cars", null, "cars", 1, new DateTime(2026, 1, 5, 2, 58, 59, 24, DateTimeKind.Utc).AddTicks(3850) },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), "Pickup trucks and commercial trucks", null, true, false, 0, "Trucks", null, "trucks", 2, new DateTime(2026, 1, 5, 2, 58, 59, 24, DateTimeKind.Utc).AddTicks(3870) },
                    { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), "Sport utility vehicles and crossovers", null, true, false, 0, "SUVs & Crossovers", null, "suvs-crossovers", 3, new DateTime(2026, 1, 5, 2, 58, 59, 24, DateTimeKind.Utc).AddTicks(3870) },
                    { new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), "Sport bikes, cruisers and ATVs", null, true, false, 0, "Motorcycles", null, "motorcycles", 4, new DateTime(2026, 1, 5, 2, 58, 59, 24, DateTimeKind.Utc).AddTicks(3880) },
                    { new Guid("55555555-5555-5555-5555-555555555555"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), "Boats, jet skis and watercraft", null, true, false, 0, "Boats & Watercraft", null, "boats-watercraft", 5, new DateTime(2026, 1, 5, 2, 58, 59, 24, DateTimeKind.Utc).AddTicks(3880) },
                    { new Guid("66666666-6666-6666-6666-666666666666"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), "Recreational vehicles and campers", null, true, false, 0, "RVs & Campers", null, "rvs-campers", 6, new DateTime(2026, 1, 5, 2, 58, 59, 24, DateTimeKind.Utc).AddTicks(3880) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_categories_IsActive",
                table: "categories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_categories_Level",
                table: "categories",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_categories_ParentId",
                table: "categories",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_categories_Slug",
                table: "categories",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Category_DealerId",
                table: "categories",
                column: "DealerId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_images_VehicleId",
                table: "vehicle_images",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_images_VehicleId_IsPrimary",
                table: "vehicle_images",
                columns: new[] { "VehicleId", "IsPrimary" });

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_images_VehicleId_SortOrder",
                table: "vehicle_images",
                columns: new[] { "VehicleId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleImage_DealerId",
                table: "vehicle_images",
                column: "DealerId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_makes_IsActive",
                table: "vehicle_makes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_makes_Name",
                table: "vehicle_makes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_makes_SortOrder",
                table: "vehicle_makes",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_models_IsActive",
                table: "vehicle_models",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_models_MakeId",
                table: "vehicle_models",
                column: "MakeId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_models_MakeId_Name",
                table: "vehicle_models",
                columns: new[] { "MakeId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_models_Name",
                table: "vehicle_models",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_trims_IsActive",
                table: "vehicle_trims",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_trims_ModelId",
                table: "vehicle_trims",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_trims_ModelId_Year_Name",
                table: "vehicle_trims",
                columns: new[] { "ModelId", "Year", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_trims_Year",
                table: "vehicle_trims",
                column: "Year");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_DealerId",
                table: "vehicles",
                column: "DealerId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_BodyStyle",
                table: "vehicles",
                column: "BodyStyle");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_CategoryId",
                table: "vehicles",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_City",
                table: "vehicles",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_Condition",
                table: "vehicles",
                column: "Condition");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_CreatedAt",
                table: "vehicles",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_FuelType",
                table: "vehicles",
                column: "FuelType");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_IsDeleted",
                table: "vehicles",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_IsFeatured",
                table: "vehicles",
                column: "IsFeatured");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_Make",
                table: "vehicles",
                column: "Make");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_Make_Model_Year",
                table: "vehicles",
                columns: new[] { "Make", "Model", "Year" });

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_MakeId",
                table: "vehicles",
                column: "MakeId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_Mileage",
                table: "vehicles",
                column: "Mileage");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_Model",
                table: "vehicles",
                column: "Model");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_ModelId",
                table: "vehicles",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_Price",
                table: "vehicles",
                column: "Price");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_SellerId",
                table: "vehicles",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_State",
                table: "vehicles",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_State_City",
                table: "vehicles",
                columns: new[] { "State", "City" });

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_Status",
                table: "vehicles",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_Status_IsDeleted",
                table: "vehicles",
                columns: new[] { "Status", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_Transmission",
                table: "vehicles",
                column: "Transmission");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_VehicleType",
                table: "vehicles",
                column: "VehicleType");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_VIN",
                table: "vehicles",
                column: "VIN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_Year",
                table: "vehicles",
                column: "Year");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_ZipCode",
                table: "vehicles",
                column: "ZipCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vehicle_images");

            migrationBuilder.DropTable(
                name: "vehicle_trims");

            migrationBuilder.DropTable(
                name: "vehicles");

            migrationBuilder.DropTable(
                name: "vehicle_models");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "vehicle_makes");
        }
    }
}
