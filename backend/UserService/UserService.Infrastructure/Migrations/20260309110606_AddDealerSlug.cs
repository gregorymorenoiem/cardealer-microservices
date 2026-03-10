using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDealerSlug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Dealers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WhatsAppMarketing",
                table: "CommunicationPreferences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WhatsAppPriceAlerts",
                table: "CommunicationPreferences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WhatsAppTransactional",
                table: "CommunicationPreferences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ConsentRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Granted = table.Column<bool>(type: "boolean", nullable: false),
                    Source = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsentRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsentRecords_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsentRecords_Timestamp",
                table: "ConsentRecords",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentRecords_UserId",
                table: "ConsentRecords",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentRecords_UserId_ConsentType",
                table: "ConsentRecords",
                columns: new[] { "UserId", "ConsentType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsentRecords");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "WhatsAppMarketing",
                table: "CommunicationPreferences");

            migrationBuilder.DropColumn(
                name: "WhatsAppPriceAlerts",
                table: "CommunicationPreferences");

            migrationBuilder.DropColumn(
                name: "WhatsAppTransactional",
                table: "CommunicationPreferences");
        }
    }
}
