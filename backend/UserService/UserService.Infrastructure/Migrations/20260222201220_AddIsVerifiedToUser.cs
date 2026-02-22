using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsVerifiedToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use IF NOT EXISTS because these columns already exist in production
            // (they were added manually during e2e testing on 2026-02-22)
            migrationBuilder.Sql("""
                ALTER TABLE "Users"
                    ADD COLUMN IF NOT EXISTS "IsVerified" boolean NOT NULL DEFAULT false,
                    ADD COLUMN IF NOT EXISTS "VerifiedAt" timestamp with time zone;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "VerifiedAt",
                table: "Users");
        }
    }
}
