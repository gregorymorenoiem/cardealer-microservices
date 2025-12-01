using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoleService.Infrastructure.Migrations
{
    public partial class AddIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_error_logs_status_code_occurred_at",
                table: "error_logs",
                columns: new[] { "status_code", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "IX_error_logs_user_id_occurred_at",
                table: "error_logs",
                columns: new[] { "user_id", "occurred_at" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_error_logs_status_code_occurred_at",
                table: "error_logs");

            migrationBuilder.DropIndex(
                name: "IX_error_logs_user_id_occurred_at",
                table: "error_logs");
        }
    }
}
