using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificationService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTenantSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationLogs_Notifications_NotificationId",
                table: "NotificationLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_NotificationQueues_Notifications_NotificationId",
                table: "NotificationQueues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Notifications",
                table: "Notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NotificationTemplates",
                table: "NotificationTemplates");

            migrationBuilder.RenameTable(
                name: "Notifications",
                newName: "notifications");

            migrationBuilder.RenameTable(
                name: "NotificationTemplates",
                newName: "notification_templates");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "notifications",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Subject",
                table: "notifications",
                newName: "subject");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "notifications",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Recipient",
                table: "notifications",
                newName: "recipient");

            migrationBuilder.RenameColumn(
                name: "Provider",
                table: "notifications",
                newName: "provider");

            migrationBuilder.RenameColumn(
                name: "Priority",
                table: "notifications",
                newName: "priority");

            migrationBuilder.RenameColumn(
                name: "Metadata",
                table: "notifications",
                newName: "metadata");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "notifications",
                newName: "content");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "notifications",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TemplateName",
                table: "notifications",
                newName: "template_name");

            migrationBuilder.RenameColumn(
                name: "SentAt",
                table: "notifications",
                newName: "sent_at");

            migrationBuilder.RenameColumn(
                name: "RetryCount",
                table: "notifications",
                newName: "retry_count");

            migrationBuilder.RenameColumn(
                name: "ErrorMessage",
                table: "notifications",
                newName: "error_message");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "notifications",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Variables",
                table: "notification_templates",
                newName: "variables");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "notification_templates",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Subject",
                table: "notification_templates",
                newName: "subject");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "notification_templates",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Body",
                table: "notification_templates",
                newName: "body");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "notification_templates",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "notification_templates",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "notification_templates",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "notification_templates",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_NotificationTemplates_Name",
                table: "notification_templates",
                newName: "IX_notification_templates_name");

            migrationBuilder.AlterColumn<string>(
                name: "type",
                table: "notifications",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "notifications",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "provider",
                table: "notifications",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "priority",
                table: "notifications",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "metadata",
                table: "notifications",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DealerId",
                table: "notifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextRetryAt",
                table: "NotificationQueues",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "variables",
                table: "notification_templates",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "type",
                table: "notification_templates",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "DealerId",
                table: "notification_templates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "category",
                table: "notification_templates",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "notification_templates",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "System");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "notification_templates",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "preview_data",
                table: "notification_templates",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "previous_version_id",
                table: "notification_templates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tags",
                table: "notification_templates",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by",
                table: "notification_templates",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "validation_rules",
                table: "notification_templates",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "version",
                table: "notification_templates",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddPrimaryKey(
                name: "PK_notifications",
                table: "notifications",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_notification_templates",
                table: "notification_templates",
                column: "id");

            migrationBuilder.CreateTable(
                name: "scheduled_notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    notification_id = table.Column<Guid>(type: "uuid", nullable: false),
                    scheduled_for = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    time_zone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, defaultValue: "UTC"),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_recurring = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    recurrence_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    cron_expression = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    next_execution = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_execution = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    execution_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    max_executions = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "System"),
                    cancelled_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cancellation_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    failure_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    last_error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scheduled_notifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_scheduled_notifications_notifications_notification_id",
                        column: x => x.notification_id,
                        principalTable: "notifications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notification_DealerId",
                table: "notifications",
                column: "DealerId");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_created_at",
                table: "notifications",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_provider",
                table: "notifications",
                column: "provider");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_recipient",
                table: "notifications",
                column: "recipient");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_recipient_created_at",
                table: "notifications",
                columns: new[] { "recipient", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_status",
                table: "notifications",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_type",
                table: "notifications",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_type_status",
                table: "notifications",
                columns: new[] { "type", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_notification_templates_category",
                table: "notification_templates",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "IX_notification_templates_is_active",
                table: "notification_templates",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_notification_templates_previous_version_id",
                table: "notification_templates",
                column: "previous_version_id");

            migrationBuilder.CreateIndex(
                name: "IX_notification_templates_type",
                table: "notification_templates",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_notification_templates_version",
                table: "notification_templates",
                column: "version");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplate_DealerId",
                table: "notification_templates",
                column: "DealerId");

            migrationBuilder.CreateIndex(
                name: "IX_scheduled_notifications_is_recurring",
                table: "scheduled_notifications",
                column: "is_recurring");

            migrationBuilder.CreateIndex(
                name: "IX_scheduled_notifications_next_execution",
                table: "scheduled_notifications",
                column: "next_execution");

            migrationBuilder.CreateIndex(
                name: "IX_scheduled_notifications_notification_id",
                table: "scheduled_notifications",
                column: "notification_id");

            migrationBuilder.CreateIndex(
                name: "IX_scheduled_notifications_scheduled_for",
                table: "scheduled_notifications",
                column: "scheduled_for");

            migrationBuilder.CreateIndex(
                name: "IX_scheduled_notifications_status",
                table: "scheduled_notifications",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_scheduled_notifications_status_next_execution",
                table: "scheduled_notifications",
                columns: new[] { "status", "next_execution" });

            migrationBuilder.CreateIndex(
                name: "IX_scheduled_notifications_status_scheduled_for",
                table: "scheduled_notifications",
                columns: new[] { "status", "scheduled_for" });

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationLogs_notifications_NotificationId",
                table: "NotificationLogs",
                column: "NotificationId",
                principalTable: "notifications",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationQueues_notifications_NotificationId",
                table: "NotificationQueues",
                column: "NotificationId",
                principalTable: "notifications",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationLogs_notifications_NotificationId",
                table: "NotificationLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_NotificationQueues_notifications_NotificationId",
                table: "NotificationQueues");

            migrationBuilder.DropTable(
                name: "scheduled_notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_notifications",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notification_DealerId",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "IX_notifications_created_at",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "IX_notifications_provider",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "IX_notifications_recipient",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "IX_notifications_recipient_created_at",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "IX_notifications_status",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "IX_notifications_type",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "IX_notifications_type_status",
                table: "notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_notification_templates",
                table: "notification_templates");

            migrationBuilder.DropIndex(
                name: "IX_notification_templates_category",
                table: "notification_templates");

            migrationBuilder.DropIndex(
                name: "IX_notification_templates_is_active",
                table: "notification_templates");

            migrationBuilder.DropIndex(
                name: "IX_notification_templates_previous_version_id",
                table: "notification_templates");

            migrationBuilder.DropIndex(
                name: "IX_notification_templates_type",
                table: "notification_templates");

            migrationBuilder.DropIndex(
                name: "IX_notification_templates_version",
                table: "notification_templates");

            migrationBuilder.DropIndex(
                name: "IX_NotificationTemplate_DealerId",
                table: "notification_templates");

            migrationBuilder.DropColumn(
                name: "DealerId",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "NextRetryAt",
                table: "NotificationQueues");

            migrationBuilder.DropColumn(
                name: "DealerId",
                table: "notification_templates");

            migrationBuilder.DropColumn(
                name: "category",
                table: "notification_templates");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "notification_templates");

            migrationBuilder.DropColumn(
                name: "description",
                table: "notification_templates");

            migrationBuilder.DropColumn(
                name: "preview_data",
                table: "notification_templates");

            migrationBuilder.DropColumn(
                name: "previous_version_id",
                table: "notification_templates");

            migrationBuilder.DropColumn(
                name: "tags",
                table: "notification_templates");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "notification_templates");

            migrationBuilder.DropColumn(
                name: "validation_rules",
                table: "notification_templates");

            migrationBuilder.DropColumn(
                name: "version",
                table: "notification_templates");

            migrationBuilder.RenameTable(
                name: "notifications",
                newName: "Notifications");

            migrationBuilder.RenameTable(
                name: "notification_templates",
                newName: "NotificationTemplates");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "Notifications",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "subject",
                table: "Notifications",
                newName: "Subject");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Notifications",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "recipient",
                table: "Notifications",
                newName: "Recipient");

            migrationBuilder.RenameColumn(
                name: "provider",
                table: "Notifications",
                newName: "Provider");

            migrationBuilder.RenameColumn(
                name: "priority",
                table: "Notifications",
                newName: "Priority");

            migrationBuilder.RenameColumn(
                name: "metadata",
                table: "Notifications",
                newName: "Metadata");

            migrationBuilder.RenameColumn(
                name: "content",
                table: "Notifications",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Notifications",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "template_name",
                table: "Notifications",
                newName: "TemplateName");

            migrationBuilder.RenameColumn(
                name: "sent_at",
                table: "Notifications",
                newName: "SentAt");

            migrationBuilder.RenameColumn(
                name: "retry_count",
                table: "Notifications",
                newName: "RetryCount");

            migrationBuilder.RenameColumn(
                name: "error_message",
                table: "Notifications",
                newName: "ErrorMessage");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Notifications",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "variables",
                table: "NotificationTemplates",
                newName: "Variables");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "NotificationTemplates",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "subject",
                table: "NotificationTemplates",
                newName: "Subject");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "NotificationTemplates",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "body",
                table: "NotificationTemplates",
                newName: "Body");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "NotificationTemplates",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "NotificationTemplates",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "NotificationTemplates",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "NotificationTemplates",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_notification_templates_name",
                table: "NotificationTemplates",
                newName: "IX_NotificationTemplates_Name");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Notifications",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Notifications",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Provider",
                table: "Notifications",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Priority",
                table: "Notifications",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Metadata",
                table: "Notifications",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Variables",
                table: "NotificationTemplates",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "NotificationTemplates",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notifications",
                table: "Notifications",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NotificationTemplates",
                table: "NotificationTemplates",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationLogs_Notifications_NotificationId",
                table: "NotificationLogs",
                column: "NotificationId",
                principalTable: "Notifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationQueues_Notifications_NotificationId",
                table: "NotificationQueues",
                column: "NotificationId",
                principalTable: "Notifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
