using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuditService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditEventTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "audit");

            migrationBuilder.CreateTable(
                name: "audit_events",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: false),
                    EventTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConsumedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Metadata = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Resource = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    UserIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AdditionalDataJson = table.Column<string>(type: "jsonb", nullable: false),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ServiceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_ConsumedAt",
                schema: "audit",
                table: "audit_events",
                column: "ConsumedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_CorrelationId",
                schema: "audit",
                table: "audit_events",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_EventId",
                schema: "audit",
                table: "audit_events",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_EventTimestamp",
                schema: "audit",
                table: "audit_events",
                column: "EventTimestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_EventType",
                schema: "audit",
                table: "audit_events",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_Source",
                schema: "audit",
                table: "audit_events",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_UserId",
                schema: "audit",
                table: "audit_events",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_Action",
                schema: "audit",
                table: "audit_logs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_Action_CreatedAt",
                schema: "audit",
                table: "audit_logs",
                columns: new[] { "Action", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_CreatedAt",
                schema: "audit",
                table: "audit_logs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_Resource",
                schema: "audit",
                table: "audit_logs",
                column: "Resource");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_ServiceName",
                schema: "audit",
                table: "audit_logs",
                column: "ServiceName");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_ServiceName_CreatedAt",
                schema: "audit",
                table: "audit_logs",
                columns: new[] { "ServiceName", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_Severity",
                schema: "audit",
                table: "audit_logs",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_Success",
                schema: "audit",
                table: "audit_logs",
                column: "Success");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_UserId",
                schema: "audit",
                table: "audit_logs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_UserId_CreatedAt",
                schema: "audit",
                table: "audit_logs",
                columns: new[] { "UserId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_events",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "audit_logs",
                schema: "audit");
        }
    }
}
