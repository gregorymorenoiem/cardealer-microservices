using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSavedPaymentMethods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AzulSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AzulSubscriptionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "DOP"),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Frequency = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PaymentMethod = table.Column<int>(type: "integer", nullable: false),
                    CardToken = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CardLastFour = table.Column<string>(type: "text", nullable: true),
                    CustomerEmail = table.Column<string>(type: "text", nullable: true),
                    CustomerPhone = table.Column<string>(type: "text", nullable: true),
                    ChargeCount = table.Column<int>(type: "integer", nullable: false),
                    TotalAmountCharged = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    NextChargeDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastChargeDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancellationReason = table.Column<string>(type: "text", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PlanName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    InvoiceReference = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AzulSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AzulTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AzulTransactionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "DOP"),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PaymentMethod = table.Column<int>(type: "integer", nullable: false),
                    CardLastFour = table.Column<string>(type: "text", nullable: true),
                    TransactionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ResponseCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ResponseMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CardToken = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AuthorizationCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsRecurring = table.Column<bool>(type: "boolean", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    InvoiceReference = table.Column<string>(type: "text", nullable: true),
                    CustomerEmail = table.Column<string>(type: "text", nullable: true),
                    CustomerPhone = table.Column<string>(type: "text", nullable: true),
                    CustomerIpAddress = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailureReason = table.Column<string>(type: "text", nullable: true),
                    RawAzulResponse = table.Column<string>(type: "text", nullable: true),
                    Gateway = table.Column<string>(type: "text", nullable: true),
                    Commission = table.Column<decimal>(type: "numeric", nullable: true),
                    CommissionPercentage = table.Column<decimal>(type: "numeric", nullable: true),
                    NetAmount = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AzulTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AzulWebhookEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    AzulEventId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    Signature = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsValidated = table.Column<bool>(type: "boolean", nullable: false),
                    IsProcessed = table.Column<bool>(type: "boolean", nullable: false),
                    ProcessingResult = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ProcessingError = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SenderIpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    ProcessingAttempts = table.Column<int>(type: "integer", nullable: false),
                    LastProcessingAttempt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AzulWebhookEvents", x => x.Id);
                });

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
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Metadata = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeRates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SavedPaymentMethods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentGateway = table.Column<int>(type: "integer", nullable: false),
                    Token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    NickName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CardBrand = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CardLast4 = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    ExpirationMonth = table.Column<int>(type: "integer", nullable: false),
                    ExpirationYear = table.Column<int>(type: "integer", nullable: false),
                    CardHolderName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BankCountry = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    BankName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AccountLast4 = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    AccountType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AccountBankName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    LastUsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsageCount = table.Column<int>(type: "integer", nullable: false),
                    BillingAddressJson = table.Column<string>(type: "jsonb", nullable: true),
                    ExternalReference = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedPaymentMethods", x => x.Id);
                });

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
                name: "IX_AzulSubscriptions_AzulSubscriptionId",
                table: "AzulSubscriptions",
                column: "AzulSubscriptionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AzulSubscriptions_NextChargeDate",
                table: "AzulSubscriptions",
                column: "NextChargeDate");

            migrationBuilder.CreateIndex(
                name: "IX_AzulSubscriptions_Status",
                table: "AzulSubscriptions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AzulSubscriptions_UserId",
                table: "AzulSubscriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AzulTransactions_AzulTransactionId",
                table: "AzulTransactions",
                column: "AzulTransactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AzulTransactions_CreatedAt",
                table: "AzulTransactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AzulTransactions_Status",
                table: "AzulTransactions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AzulTransactions_UserId",
                table: "AzulTransactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AzulWebhookEvents_AzulEventId",
                table: "AzulWebhookEvents",
                column: "AzulEventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AzulWebhookEvents_EventType",
                table: "AzulWebhookEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_AzulWebhookEvents_IsProcessed",
                table: "AzulWebhookEvents",
                column: "IsProcessed");

            migrationBuilder.CreateIndex(
                name: "IX_AzulWebhookEvents_ReceivedAt",
                table: "AzulWebhookEvents",
                column: "ReceivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyConversions_CreatedAt",
                table: "CurrencyConversions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyConversions_ExchangeRateId",
                table: "CurrencyConversions",
                column: "ExchangeRateId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyConversions_Ncf",
                table: "CurrencyConversions",
                column: "Ncf");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyConversions_PaymentTransactionId",
                table: "CurrencyConversions",
                column: "PaymentTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_RateDate",
                table: "ExchangeRates",
                column: "RateDate");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_SourceCurrency",
                table: "ExchangeRates",
                column: "SourceCurrency");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_SourceCurrency_RateDate_IsActive",
                table: "ExchangeRates",
                columns: new[] { "SourceCurrency", "RateDate", "IsActive" },
                unique: true,
                filter: "\"IsActive\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_SavedPaymentMethods_IsActive",
                table: "SavedPaymentMethods",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SavedPaymentMethods_Token_PaymentGateway",
                table: "SavedPaymentMethods",
                columns: new[] { "Token", "PaymentGateway" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SavedPaymentMethods_UserId",
                table: "SavedPaymentMethods",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedPaymentMethods_UserId_IsDefault",
                table: "SavedPaymentMethods",
                columns: new[] { "UserId", "IsDefault" },
                filter: "\"IsDefault\" = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AzulSubscriptions");

            migrationBuilder.DropTable(
                name: "AzulTransactions");

            migrationBuilder.DropTable(
                name: "AzulWebhookEvents");

            migrationBuilder.DropTable(
                name: "CurrencyConversions");

            migrationBuilder.DropTable(
                name: "SavedPaymentMethods");

            migrationBuilder.DropTable(
                name: "ExchangeRates");
        }
    }
}
