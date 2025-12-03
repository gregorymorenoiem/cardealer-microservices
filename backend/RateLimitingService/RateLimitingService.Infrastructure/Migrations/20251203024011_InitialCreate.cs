using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RateLimitingService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:hstore", ",,");

            migrationBuilder.CreateTable(
                name: "rate_limit_violations",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    identifier = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    identifier_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    endpoint = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    rule_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    rule_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    attempted_requests = table.Column<int>(type: "integer", nullable: false),
                    allowed_limit = table.Column<int>(type: "integer", nullable: false),
                    violated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    Metadata = table.Column<Dictionary<string, string>>(type: "hstore", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rate_limit_violations", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_violations_identifier",
                table: "rate_limit_violations",
                column: "identifier");

            migrationBuilder.CreateIndex(
                name: "idx_violations_identifier_date",
                table: "rate_limit_violations",
                columns: new[] { "identifier", "violated_at" });

            migrationBuilder.CreateIndex(
                name: "idx_violations_identifier_type",
                table: "rate_limit_violations",
                column: "identifier_type");

            migrationBuilder.CreateIndex(
                name: "idx_violations_type_date",
                table: "rate_limit_violations",
                columns: new[] { "identifier_type", "violated_at" });

            migrationBuilder.CreateIndex(
                name: "idx_violations_violated_at",
                table: "rate_limit_violations",
                column: "violated_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rate_limit_violations");
        }
    }
}
