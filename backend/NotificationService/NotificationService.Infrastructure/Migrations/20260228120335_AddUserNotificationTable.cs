using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificationService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserNotificationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // All other tables (notifications, notification_templates, NotificationLogs,
            // NotificationQueues, scheduled_notifications) already existed from prior migrations:
            //   - InitialCreate      (20251023183351): creates Notifications, NotificationTemplates, NotificationLogs, NotificationQueues
            //   - AddMultiTenantSupport (20251206020716): renames to snake_case, creates scheduled_notifications
            //   - AddUpdatedAtToNotifications (20260220): adds UpdatedAt column to notifications
            //
            // This migration ONLY creates the user_notifications table that was missing.
            migrationBuilder.CreateTable(
                name: "user_notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dealer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    icon = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    link = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_read = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    metadata = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_notifications", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_notifications_created_at",
                table: "user_notifications",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_user_notifications_user_id",
                table: "user_notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_notifications_user_id_is_read",
                table: "user_notifications",
                columns: new[] { "user_id", "is_read" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_notifications");
        }
    }
}
