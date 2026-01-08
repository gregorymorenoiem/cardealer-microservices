using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlertService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "price_alerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Condition = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsTriggered = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    TriggeredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_price_alerts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "saved_searches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SearchCriteria = table.Column<string>(type: "jsonb", nullable: false),
                    SendEmailNotifications = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Frequency = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    LastNotificationSent = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_saved_searches", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_price_alerts_active",
                table: "price_alerts",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "idx_price_alerts_user",
                table: "price_alerts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "idx_price_alerts_user_vehicle",
                table: "price_alerts",
                columns: new[] { "UserId", "VehicleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_price_alerts_vehicle",
                table: "price_alerts",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "idx_saved_searches_active",
                table: "saved_searches",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "idx_saved_searches_last_notification",
                table: "saved_searches",
                column: "LastNotificationSent");

            migrationBuilder.CreateIndex(
                name: "idx_saved_searches_user",
                table: "saved_searches",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "price_alerts");

            migrationBuilder.DropTable(
                name: "saved_searches");
        }
    }
}
