using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentService.Infrastructure.Migrations
{
    /// <summary>
    /// Migración para agregar tablas de tasas de cambio y conversiones
    /// Requerido por DGII para transacciones en moneda extranjera
    /// Fuente oficial: Banco Central de la República Dominicana
    /// </summary>
    public partial class AddExchangeRateTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ==================== EXCHANGE RATES TABLE ====================
            migrationBuilder.CreateTable(
                name: "ExchangeRates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RateDate = table.Column<DateOnly>(type: "date", nullable: false),
                    SourceCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    TargetCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "DOP"),
                    BuyRate = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    SellRate = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    BcrdReferenceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FetchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Metadata = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeRates", x => x.Id);
                });

            // Índice único: solo una tasa activa por moneda/fecha
            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_SourceCurrency_RateDate_IsActive",
                table: "ExchangeRates",
                columns: new[] { "SourceCurrency", "RateDate", "IsActive" },
                unique: true,
                filter: "\"IsActive\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_RateDate",
                table: "ExchangeRates",
                column: "RateDate");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_SourceCurrency",
                table: "ExchangeRates",
                column: "SourceCurrency");

            // ==================== CURRENCY CONVERSIONS TABLE ====================
            migrationBuilder.CreateTable(
                name: "CurrencyConversions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExchangeRateId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    OriginalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ConvertedAmountDop = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AppliedRate = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    RateDate = table.Column<DateOnly>(type: "date", nullable: false),
                    RateSource = table.Column<int>(type: "integer", nullable: false),
                    ConversionType = table.Column<int>(type: "integer", nullable: false),
                    ItbisDop = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalWithItbisDop = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Ncf = table.Column<string>(type: "character varying(19)", maxLength: 19, nullable: true),
                    NcfIssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyConversions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurrencyConversions_ExchangeRates_ExchangeRateId",
                        column: x => x.ExchangeRateId,
                        principalTable: "ExchangeRates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyConversions_PaymentTransactionId",
                table: "CurrencyConversions",
                column: "PaymentTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyConversions_ExchangeRateId",
                table: "CurrencyConversions",
                column: "ExchangeRateId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyConversions_CreatedAt",
                table: "CurrencyConversions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyConversions_Ncf",
                table: "CurrencyConversions",
                column: "Ncf");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CurrencyConversions");

            migrationBuilder.DropTable(
                name: "ExchangeRates");
        }
    }
}
