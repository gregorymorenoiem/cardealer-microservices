using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPreferencesAndCommunicationPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreferredCurrency",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredLocale",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CommunicationPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmailActivityNotifications = table.Column<bool>(type: "boolean", nullable: false),
                    EmailListingUpdates = table.Column<bool>(type: "boolean", nullable: false),
                    EmailNewsletter = table.Column<bool>(type: "boolean", nullable: false),
                    EmailPromotions = table.Column<bool>(type: "boolean", nullable: false),
                    EmailPriceAlerts = table.Column<bool>(type: "boolean", nullable: false),
                    SmsVerificationCodes = table.Column<bool>(type: "boolean", nullable: false),
                    SmsPriceAlerts = table.Column<bool>(type: "boolean", nullable: false),
                    SmsPromotions = table.Column<bool>(type: "boolean", nullable: false),
                    PushNewMessages = table.Column<bool>(type: "boolean", nullable: false),
                    PushPriceChanges = table.Column<bool>(type: "boolean", nullable: false),
                    PushRecommendations = table.Column<bool>(type: "boolean", nullable: false),
                    AllowProfiling = table.Column<bool>(type: "boolean", nullable: false),
                    AllowThirdPartySharing = table.Column<bool>(type: "boolean", nullable: false),
                    AllowAnalytics = table.Column<bool>(type: "boolean", nullable: false),
                    AllowRetargeting = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunicationPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommunicationPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationPreferences_UserId",
                table: "CommunicationPreferences",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommunicationPreferences");

            migrationBuilder.DropColumn(
                name: "PreferredCurrency",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PreferredLocale",
                table: "Users");
        }
    }
}
