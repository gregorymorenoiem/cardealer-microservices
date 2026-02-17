using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PropertiesRentService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPropertiesRentServiceMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IconUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    CustomFieldsSchemaJson = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "[]"),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                name: "products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CustomFieldsJson = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_products_categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "product_custom_fields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DataType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "string"),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsSearchable = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_custom_fields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_product_custom_fields_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_images",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_product_images_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "categories",
                columns: new[] { "Id", "CreatedAt", "CustomFieldsSchemaJson", "Description", "IconUrl", "IsActive", "Level", "Name", "ParentId", "Slug", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2025, 12, 5, 21, 13, 19, 880, DateTimeKind.Utc).AddTicks(7057), "[\r\n                    {\"key\":\"make\",\"label\":\"Marca\",\"type\":\"string\",\"required\":true},\r\n                    {\"key\":\"model\",\"label\":\"Modelo\",\"type\":\"string\",\"required\":true},\r\n                    {\"key\":\"year\",\"label\":\"Año\",\"type\":\"number\",\"required\":true},\r\n                    {\"key\":\"mileage\",\"label\":\"Kilometraje\",\"type\":\"number\",\"unit\":\"km\"},\r\n                    {\"key\":\"transmission\",\"label\":\"Transmisión\",\"type\":\"string\"},\r\n                    {\"key\":\"fuelType\",\"label\":\"Combustible\",\"type\":\"string\"},\r\n                    {\"key\":\"color\",\"label\":\"Color\",\"type\":\"string\"}\r\n                ]", "Autos, motos y vehículos comerciales", null, true, 0, "Vehículos", null, "vehiculos", 1 },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2025, 12, 5, 21, 13, 19, 880, DateTimeKind.Utc).AddTicks(7062), "[\r\n                    {\"key\":\"bedrooms\",\"label\":\"Habitaciones\",\"type\":\"number\",\"required\":true},\r\n                    {\"key\":\"bathrooms\",\"label\":\"Baños\",\"type\":\"number\",\"required\":true},\r\n                    {\"key\":\"sqft\",\"label\":\"Área\",\"type\":\"number\",\"unit\":\"m²\"},\r\n                    {\"key\":\"parking\",\"label\":\"Estacionamiento\",\"type\":\"boolean\"},\r\n                    {\"key\":\"furnished\",\"label\":\"Amueblado\",\"type\":\"boolean\"}\r\n                ]", "Casas, departamentos y terrenos", null, true, 0, "Inmuebles", null, "inmuebles", 2 },
                    { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2025, 12, 5, 21, 13, 19, 880, DateTimeKind.Utc).AddTicks(7065), "[\r\n                    {\"key\":\"brand\",\"label\":\"Marca\",\"type\":\"string\",\"required\":true},\r\n                    {\"key\":\"condition\",\"label\":\"Condición\",\"type\":\"string\"},\r\n                    {\"key\":\"warranty\",\"label\":\"Garantía\",\"type\":\"boolean\"}\r\n                ]", "Computadoras, celulares y gadgets", null, true, 0, "Electrónicos", null, "electronicos", 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_categories_IsActive",
                table: "categories",
                column: "IsActive");

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
                name: "IX_product_custom_fields_IsSearchable",
                table: "product_custom_fields",
                column: "IsSearchable");

            migrationBuilder.CreateIndex(
                name: "IX_product_custom_fields_Key",
                table: "product_custom_fields",
                column: "Key");

            migrationBuilder.CreateIndex(
                name: "IX_product_custom_fields_ProductId",
                table: "product_custom_fields",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_product_custom_fields_ProductId_Key",
                table: "product_custom_fields",
                columns: new[] { "ProductId", "Key" });

            migrationBuilder.CreateIndex(
                name: "IX_product_images_ProductId",
                table: "product_images",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_product_images_ProductId_SortOrder",
                table: "product_images",
                columns: new[] { "ProductId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_products_CategoryId",
                table: "products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_products_CreatedAt",
                table: "products",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_products_Name",
                table: "products",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_products_Price",
                table: "products",
                column: "Price");

            migrationBuilder.CreateIndex(
                name: "IX_products_SellerId",
                table: "products",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_products_Status",
                table: "products",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_custom_fields");

            migrationBuilder.DropTable(
                name: "product_images");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "categories");
        }
    }
}
