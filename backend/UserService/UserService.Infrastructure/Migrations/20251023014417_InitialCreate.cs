using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "error_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    exception_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    stack_trace = table.Column<string>(type: "text", nullable: true),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    endpoint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    http_method = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    status_code = table.Column<int>(type: "integer", nullable: true),
                    user_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    metadata = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_error_logs", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_error_logs_occurred_at",
                table: "error_logs",
                column: "occurred_at");

            migrationBuilder.CreateIndex(
                name: "IX_error_logs_service_name",
                table: "error_logs",
                column: "service_name");

            migrationBuilder.CreateIndex(
                name: "IX_error_logs_service_name_occurred_at",
                table: "error_logs",
                columns: new[] { "service_name", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "IX_error_logs_status_code",
                table: "error_logs",
                column: "status_code");

            migrationBuilder.CreateIndex(
                name: "IX_error_logs_user_id",
                table: "error_logs",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "error_logs");
        }
    }
}
