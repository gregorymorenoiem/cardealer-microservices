using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthService.Infrastructure.Migrations
{
    /// <summary>
    /// Adds performance optimization indexes to Users, LoginAttempts, and RefreshTokens tables.
    /// These indexes improve query performance for common authentication operations.
    /// </summary>
    public partial class AddDatabaseIndexOptimization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Users table indexes
            migrationBuilder.CreateIndex(
                name: "IX_Users_Email_IsEmailVerified",
                table: "Users",
                columns: new[] { "Email", "IsEmailVerified" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedAt",
                table: "Users",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Users_LastLogin",
                table: "Users",
                column: "LastLogin");

            // RefreshTokens table indexes
            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId_IsRevoked_ExpiresAt",
                table: "RefreshTokens",
                columns: new[] { "UserId", "IsRevoked", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ExpiresAt",
                table: "RefreshTokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_CreatedAt",
                table: "RefreshTokens",
                column: "CreatedAt");

            // Note: Add indexes for LoginAttempts if this table exists
            // Uncomment if LoginAttempts table is present in the schema
            /*
            migrationBuilder.CreateIndex(
                name: "IX_LoginAttempts_Email_Timestamp",
                table: "LoginAttempts",
                columns: new[] { "Email", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_LoginAttempts_IPAddress_Timestamp",
                table: "LoginAttempts",
                columns: new[] { "IPAddress", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_LoginAttempts_Timestamp",
                table: "LoginAttempts",
                column: "Timestamp");
            */
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop Users table indexes
            migrationBuilder.DropIndex(
                name: "IX_Users_Email_IsEmailVerified",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_CreatedAt",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_LastLogin",
                table: "Users");

            // Drop RefreshTokens table indexes
            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_UserId_IsRevoked_ExpiresAt",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_ExpiresAt",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_CreatedAt",
                table: "RefreshTokens");

            // Drop LoginAttempts table indexes (if uncommented in Up)
            /*
            migrationBuilder.DropIndex(
                name: "IX_LoginAttempts_Email_Timestamp",
                table: "LoginAttempts");

            migrationBuilder.DropIndex(
                name: "IX_LoginAttempts_IPAddress_Timestamp",
                table: "LoginAttempts");

            migrationBuilder.DropIndex(
                name: "IX_LoginAttempts_Timestamp",
                table: "LoginAttempts");
            */
        }
    }
}
